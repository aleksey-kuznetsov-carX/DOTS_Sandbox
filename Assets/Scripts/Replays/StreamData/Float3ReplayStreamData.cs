// using Unity.Burst;
// using Unity.Entities;
// using Unity.Mathematics;
//
// namespace Replays
// {
//     public struct Float3ReplayStreamData
//     {
//         public DynamicBuffer<float> m_times;
//         public DynamicBuffer<float3> m_values;
//         public DynamicBuffer<byte> m_interpTypes;
//
//         private int m_lastFoundIndex; // Поле для кэширования последнего найденного индекса
//         
//         public int count;
//         
//         [BurstCompile]
//         public void AddKeyFrame(float time, float3 value, InterpType interpType = InterpType.Linear)
//         {
//             if (m_times.IsEmpty)
//             {
//                 m_times = new DynamicBuffer<float>();
//                 m_values = new DynamicBuffer<float3>();
//                 m_interpTypes = new DynamicBuffer<byte>();
//             }
//
//             if (count > 0 && m_times[count - 1] >= time) return;
//
//             m_times.Add(time);
//             m_values.Add(value);
//             m_interpTypes.Add((byte)interpType);
//             count++;
//             // При добавлении нового кадра сбрасываем кэш — он может стать неактуальным
//             m_lastFoundIndex = -1;
//         }
//
//         [BurstCompile]
//         public InterpType GetInterpType(float time, float3 value, float epsilon)
//         {
//             InterpType type = InterpType.None;
//
//             if (count == 0) return type;
//
//             int ix = FindKeyFrameIndexWithCache(time);
//             if (ix < 0) return type;
//
//             float3 x1Value = m_values[ix];
//             if (math.lengthsq(x1Value - value) < epsilon * epsilon)
//             {
//                 return InterpType.Const;
//             }
//
//             if (count >= 2 && ix >= 1)
//             {
//                 // Линейная интерполяция
//                 float a = CoefsLinear(m_times[ix - 1], m_times[ix], time);
//                 float3 linearResult = math.lerp(m_values[ix - 1], x1Value, a);
//
//                 if (math.lengthsq(linearResult - value) < epsilon * epsilon)
//                 {
//                     return InterpType.Linear;
//                 }
//             }
//
//             if (count >= 3 && ix >= 2)
//             {
//                 // Параболическая интерполяция
//                 (float aParab, float bParab) = CoefsParab(m_times[ix - 2], m_times[ix - 1], m_times[ix], time);
//                 float3 p2 = math.lerp(m_values[ix - 2], m_values[ix - 1], bParab / (1f - aParab));
//                 float3 p1 = math.lerp(p2, x1Value, aParab);
//
//                 if (math.lengthsq(p1 - value) < epsilon * epsilon)
//                 {
//                     return InterpType.Parab;
//                 }
//             }
//
//             if (count >= 4 && ix >= 3)
//             {
//                 // Кубическая интерполяция
//                 (float aCubic, float bCubic, float gCubic) = CoefsCubic(m_times[ix - 3], m_times[ix - 2],
//                     m_times[ix - 1], m_times[ix], time);
//                 float3 p3 = math.lerp(m_values[ix - 3], m_values[ix - 2], gCubic / (1f - aCubic - bCubic));
//                 float3 p2Cubic = math.lerp(p3, m_values[ix - 1], bCubic / (1f - aCubic));
//                 float3 p1Cubic = math.lerp(p2Cubic, x1Value, aCubic);
//
//                 if (math.lengthsq(p1Cubic - value) < epsilon * epsilon)
//                 {
//                     return InterpType.Cubic;
//                 }
//             }
//
//             return type;
//         }
//
//         [BurstCompile]
//         private int FindKeyFrameIndexWithCache(float time)
//         {
//             // Быстрая проверка кэшированного индекса
//             if (m_lastFoundIndex >= 0 && m_lastFoundIndex < count)
//             {
//                 // Если время попадает в интервал текущего кэшированного кадра или следующего
//                 if (time >= m_times[m_lastFoundIndex] &&
//                     (m_lastFoundIndex == count - 1 || time <= m_times[m_lastFoundIndex + 1]))
//                 {
//                     // Проверяем, не нужно ли сдвинуться вперёд
//                     while (m_lastFoundIndex < count - 1 && time > m_times[m_lastFoundIndex + 1])
//                     {
//                         m_lastFoundIndex++;
//                     }
//
//                     // Или назад (если время уменьшилось)
//                     while (m_lastFoundIndex > 0 && time < m_times[m_lastFoundIndex])
//                     {
//                         m_lastFoundIndex--;
//                     }
//
//                     return m_lastFoundIndex;
//                 }
//             }
//
//             // Если кэш не подходит — выполняем бинарный поиск
//             int low = 0, high = count - 1;
//             while (low <= high)
//             {
//                 int mid = (low + high) >> 1;
//                 if (m_times[mid] < time)
//                     low = mid + 1;
//                 else if (m_times[mid] > time)
//                     high = mid - 1;
//                 else
//                 {
//                     m_lastFoundIndex = mid; // Обновляем кэш
//                     return mid;
//                 }
//             }
//
//             m_lastFoundIndex = high; // Обновляем кэш
//             return high;
//         }
//
//         [BurstCompile]
//         private float CoefsLinear(float t0, float t1, float t)
//         {
//             return math.clamp((t - t0) / (t1 - t0), 0f, 1f);
//         }
//
//         [BurstCompile]
//         private (float a, float b) CoefsParab(float t0, float t1, float t2, float t)
//         {
//             float dt0 = t1 - t0;
//             float dt1 = t2 - t1;
//             float w0 = (t - t1) / dt0;
//             float w1 = (t2 - t) / dt1;
//             return (w0, w1);
//         }
//
//         [BurstCompile]
//         private (float a, float b, float g) CoefsCubic(float t0, float t1, float t2, float t3, float t)
//         {
//             float dt0 = t1 - t0;
//             float dt1 = t2 - t1;
//             float dt2 = t3 - t2;
//             float w0 = (t - t1) * (t - t2) / (dt0 * (t3 - t1));
//             float w1 = (t3 - t) * (t - t2) / (dt1 * (t3 - t0));
//             float w2 = (t3 - t) * (t - t1) / (dt2 * (t2 - t0));
//             return (w0, w1, w2);
//         }
//
//         [BurstCompile]
//         public float3 Lerp(float3 from, float3 to, float t)
//         {
//             return math.lerp(from, to, t);
//         }
//
//         [BurstCompile]
//         public float3 ReadValue(float time, float3 defaultValue)
//         {
//             if (count == 0) return defaultValue;
//
//             int index = FindKeyFrameIndexWithCache(time);
//             if (index < 0) return defaultValue;
//
//             return PerformInterpolation(index, time);
//         }
//
//         [BurstCompile]
//         private float3 PerformInterpolation(int index, float time)
//         {
//             if (index >= count - 1)
//                 return m_values[count - 1];
//
//             InterpType type = (InterpType)m_interpTypes[index];
//             float t = CalculateInterpolationFactor(index, time);
//
//             switch (type)
//             {
//                 case InterpType.Const:
//                     return m_values[index];
//                 case InterpType.Linear:
//                     return math.lerp(m_values[index], m_values[index + 1], t);
//                 case InterpType.Parab:
//                     if (index >= 1 && count >= 3)
//                     {
//                         (float aParab, float bParab) = CoefsParab(
//                             m_times[index - 1], m_times[index], m_times[index + 1], time);
//                         float3 p2 = math.lerp(m_values[index - 1], m_values[index], bParab / (1f - aParab));
//                         return math.lerp(p2, m_values[index + 1], aParab);
//                     }
//
//                     break;
//                 case InterpType.Cubic:
//                     if (index >= 2 && count >= 4)
//                     {
//                         (float aCubic, float bCubic, float gCubic) = CoefsCubic(
//                             m_times[index - 2], m_times[index - 1], m_times[index], m_times[index + 1], time);
//                         float3 p3 = math.lerp(m_values[index - 2], m_values[index - 1],
//                             gCubic / (1f - aCubic - bCubic));
//                         float3 p2Cubic = math.lerp(p3, m_values[index], bCubic / (1f - aCubic));
//                         return math.lerp(p2Cubic, m_values[index + 1], aCubic);
//                     }
//
//                     break;
//             }
//
//             // Fallback — возвращаем значение текущего кадра
//             return m_values[index];
//         }
//
//         [BurstCompile]
//         private float CalculateInterpolationFactor(int index, float time)
//         {
//             float time0 = m_times[index];
//             float time1 = m_times[index + 1];
//             return math.clamp((time - time0) / (time1 - time0), 0f, 1f);
//         }
//     }
// }