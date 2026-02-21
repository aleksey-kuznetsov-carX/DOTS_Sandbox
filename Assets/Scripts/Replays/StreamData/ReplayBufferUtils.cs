using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Replays
{
    // Helper utilities to operate on DynamicBuffer<Float3Key> and DynamicBuffer<QuatKey>
    public static class ReplayBufferUtils
    {
        public static void AddKey(ref DynamicBuffer<Float3Key> buf, float time, float3 value, InterpType interp = InterpType.Linear)
        {
            if (buf.Length > 0 && buf[buf.Length - 1].Time >= time) return; // require ascending times
            buf.Add(new Float3Key { Time = time, Value = value, Interp = (byte)interp });
        }

        public static void AddKey(ref DynamicBuffer<QuatKey> buf, float time, quaternion value, InterpType interp = InterpType.Linear)
        {
            if (buf.Length > 0 && buf[buf.Length - 1].Time >= time) return;
            buf.Add(new QuatKey { Time = time, Value = value, Interp = (byte)interp });
        }

        // New: Add for FloatKey
        public static void AddKey(ref DynamicBuffer<FloatKey> buf, float time, float value, InterpType interp = InterpType.Linear)
        {
            if (buf.Length > 0 && buf[buf.Length - 1].Time >= time) return;
            buf.Add(new FloatKey { Time = time, Value = value, Interp = (byte)interp });
        }

        // New: trim buffer to max keys (remove oldest entries)
        public static void TrimToMax<T>(ref DynamicBuffer<T> buf, int maxKeys) where T : unmanaged, IBufferElementData
        {
            if (maxKeys <= 0) return;
            int excess = buf.Length - maxKeys;
            if (excess <= 0) return;
            // remove from start: shift left
            for (int i = excess; i < buf.Length; ++i)
            {
                buf[i - excess] = buf[i];
            }
            buf.RemoveRange(maxKeys, excess);
        }

        // ---------------- Float3 sampling ----------------
        [BurstCompile]
        public static int FindIndexWithCache(in DynamicBuffer<Float3Key> buf, float time, ref int lastIndex)
        {
            int count = buf.Length;
            if (count == 0) return -1;
            if (lastIndex < 0) lastIndex = 0;
            if (lastIndex >= count) lastIndex = count - 1;

            if (time >= buf[lastIndex].Time && (lastIndex == count - 1 || time <= buf[lastIndex + 1].Time))
            {
                while (lastIndex < count - 1 && time > buf[lastIndex + 1].Time) lastIndex++;
                while (lastIndex > 0 && time < buf[lastIndex].Time) lastIndex--;
                return lastIndex;
            }

            int low = 0, high = count - 1;
            while (low <= high)
            {
                int mid = (low + high) >> 1;
                float tmid = buf[mid].Time;
                if (tmid == time) { lastIndex = mid; return mid; }
                if (tmid < time) low = mid + 1; else high = mid - 1;
            }
            lastIndex = math.clamp(high, 0, count - 1);
            return lastIndex;
        }

        [BurstCompile]
        public static int FindIndexWithCache(in DynamicBuffer<QuatKey> buf, float time, ref int lastIndex)
        {
            int count = buf.Length;
            if (count == 0) return -1;
            if (lastIndex < 0) lastIndex = 0;
            if (lastIndex >= count) lastIndex = count - 1;

            if (time >= buf[lastIndex].Time && (lastIndex == count - 1 || time <= buf[lastIndex + 1].Time))
            {
                while (lastIndex < count - 1 && time > buf[lastIndex + 1].Time) lastIndex++;
                while (lastIndex > 0 && time < buf[lastIndex].Time) lastIndex--;
                return lastIndex;
            }

            int low = 0, high = count - 1;
            while (low <= high)
            {
                int mid = (low + high) >> 1;
                float tmid = buf[mid].Time;
                if (tmid == time) { lastIndex = mid; return mid; }
                if (tmid < time) low = mid + 1; else high = mid - 1;
            }
            lastIndex = math.clamp(high, 0, count - 1);
            return lastIndex;
        }

        [BurstCompile]
        public static int FindIndexWithCache(in DynamicBuffer<FloatKey> buf, float time, ref int lastIndex)
        {
            int count = buf.Length;
            if (count == 0) return -1;
            if (lastIndex < 0) lastIndex = 0;
            if (lastIndex >= count) lastIndex = count - 1;

            if (time >= buf[lastIndex].Time && (lastIndex == count - 1 || time <= buf[lastIndex + 1].Time))
            {
                while (lastIndex < count - 1 && time > buf[lastIndex + 1].Time) lastIndex++;
                while (lastIndex > 0 && time < buf[lastIndex].Time) lastIndex--;
                return lastIndex;
            }

            int low = 0, high = count - 1;
            while (low <= high)
            {
                int mid = (low + high) >> 1;
                float tmid = buf[mid].Time;
                if (tmid == time) { lastIndex = mid; return mid; }
                if (tmid < time) low = mid + 1; else high = mid - 1;
            }
            lastIndex = math.clamp(high, 0, count - 1);
            return lastIndex;
        }

        [BurstCompile]
        public static float CalcFactor(float t0, float t1, float t)
        {
            float dt = t1 - t0;
            if (dt <= 1e-8f) return 0f;
            return math.clamp((t - t0) / dt, 0f, 1f);
        }

        [BurstCompile]
        public static float3 SampleFloat3(in DynamicBuffer<Float3Key> buf, float time, ref int lastIndex, float3 defaultValue = default)
        {
            int idx = FindIndexWithCache(buf, time, ref lastIndex);
            if (idx < 0) return defaultValue;
            if (idx >= buf.Length - 1) return buf[buf.Length - 1].Value;

            var k0 = buf[idx];
            var k1 = buf[idx + 1];
            InterpType type = (InterpType)k0.Interp;
            if (type == InterpType.Const) return k0.Value;
            float t = CalcFactor(k0.Time, k1.Time, time);
            if (type == InterpType.Linear) return math.lerp(k0.Value, k1.Value, t);

            // Parab/Cubic: follow same logic as existing implementation using CoefsParab/CoefsCubic
            if (type == InterpType.Parab)
            {
                // ensure there is a previous key
                if (idx >= 1 && buf.Length >= 3)
                {
                    (float aParab, float bParab) = CoefsParab(buf[idx - 1].Time, buf[idx].Time, buf[idx + 1].Time, time);
                    float3 p2 = math.lerp(buf[idx - 1].Value, buf[idx].Value, bParab / (1f - aParab));
                    return math.lerp(p2, buf[idx + 1].Value, aParab);
                }
                return k0.Value;
            }

            if (type == InterpType.Cubic)
            {
                if (idx >= 2 && buf.Length >= 4)
                {
                    (float aCubic, float bCubic, float gCubic) = CoefsCubic(buf[idx - 2].Time, buf[idx - 1].Time, buf[idx].Time, buf[idx + 1].Time, time);
                    float3 p3 = math.lerp(buf[idx - 2].Value, buf[idx - 1].Value, gCubic / (1f - aCubic - bCubic));
                    float3 p2Cubic = math.lerp(p3, buf[idx].Value, bCubic / (1f - aCubic));
                    return math.lerp(p2Cubic, buf[idx + 1].Value, aCubic);
                }
                return k0.Value;
            }

            return k0.Value;
        }

        [BurstCompile]
        private static (float a, float b) CoefsParab(float t0, float t1, float t2, float t)
        {
            float dt0 = t1 - t0;
            float dt1 = t2 - t1;
            float w0 = (t - t1) / dt0;
            float w1 = (t2 - t) / dt1;
            return (w0, w1);
        }

        [BurstCompile]
        private static (float a, float b, float g) CoefsCubic(float t0, float t1, float t2, float t3, float t)
        {
            float dt0 = t1 - t0;
            float dt1 = t2 - t1;
            float dt2 = t3 - t2;
            float w0 = (t - t1) * (t - t2) / (dt0 * (t3 - t1));
            float w1 = (t3 - t) * (t - t2) / (dt1 * (t3 - t0));
            float w2 = (t3 - t) * (t - t1) / (dt2 * (t2 - t0));
            return (w0, w1, w2);
        }

        // ---------------- Quaternion sampling ----------------
        [BurstCompile]
        public static quaternion SampleQuat(in DynamicBuffer<QuatKey> buf, float time, ref int lastIndex, quaternion defaultValue = default)
        {
            int idx = FindIndexWithCache(buf, time, ref lastIndex);
            if (idx < 0) return defaultValue;
            if (idx >= buf.Length - 1) return buf[buf.Length - 1].Value;

            var k0 = buf[idx];
            var k1 = buf[idx + 1];
            InterpType type = (InterpType)k0.Interp;
            if (type == InterpType.Const) return k0.Value;
            float t = CalcFactor(k0.Time, k1.Time, time);
            if (type == InterpType.Linear) {
                quaternion a = k0.Value;
                quaternion b = k1.Value;
                float dot = math.dot(a.value, b.value);
                if (dot < 0f) b.value = -b.value;
                quaternion res = math.slerp(a, b, t);
                return math.normalize(res);
            }

            if (type == InterpType.Parab)
            {
                if (idx >= 1 && buf.Length >= 3)
                {
                    (float aParab, float bParab) = CoefsParab(buf[idx - 1].Time, buf[idx].Time, buf[idx + 1].Time, time);
                    quaternion p2 = math.slerp(buf[idx - 1].Value, buf[idx].Value, bParab / (1f - aParab));
                    quaternion p1 = math.slerp(p2, buf[idx + 1].Value, aParab);
                    return math.normalize(p1);
                }
                return k0.Value;
            }

            if (type == InterpType.Cubic)
            {
                if (idx >= 2 && buf.Length >= 4)
                {
                    (float aCubic, float bCubic, float gCubic) = CoefsCubic(buf[idx - 2].Time, buf[idx - 1].Time, buf[idx].Time, buf[idx + 1].Time, time);
                    quaternion p3 = math.slerp(buf[idx - 2].Value, buf[idx - 1].Value, gCubic / (1f - aCubic - bCubic));
                    quaternion p2Cubic = math.slerp(p3, buf[idx].Value, bCubic / (1f - aCubic));
                    quaternion p1Cubic = math.slerp(p2Cubic, buf[idx + 1].Value, aCubic);
                    return math.normalize(p1Cubic);
                }
                return k0.Value;
            }

            return k0.Value;
        }

        // New: sampling for FloatKey
        [BurstCompile]
        public static float SampleFloat(in DynamicBuffer<FloatKey> buf, float time, ref int lastIndex, float defaultValue = 0f)
        {
            int idx = FindIndexWithCache(buf, time, ref lastIndex);
            if (idx < 0) return defaultValue;
            if (idx >= buf.Length - 1) return buf[buf.Length - 1].Value;

            var k0 = buf[idx];
            var k1 = buf[idx + 1];
            InterpType type = (InterpType)k0.Interp;
            if (type == InterpType.Const) return k0.Value;
            float t = CalcFactor(k0.Time, k1.Time, time);
            if (type == InterpType.Linear) return math.lerp(k0.Value, k1.Value, t);

            // Parab/Cubic fallback: simple linearizing
            if (type == InterpType.Parab || type == InterpType.Cubic) return math.lerp(k0.Value, k1.Value, t);

            return k0.Value;
        }
    }
}

