using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

namespace Replays
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class DynamicReplayRecorderSystem : SystemBase
    {
        void EnsureBuffersAndComponents()
        {
            var q = GetEntityQuery(ComponentType.ReadOnly<ReplayRecordTag>(), ComponentType.ReadOnly<LocalToWorld>());
            using (var entities = q.ToEntityArray(Allocator.Temp))
            {
                if (entities.Length == 0) return;
                var ecb = new EntityCommandBuffer(Allocator.Temp);
                foreach (var e in entities)
                {
                    if (!EntityManager.HasComponent<TrafficReplayStreamDataComponent>(e))
                    {
                        ecb.AddComponent(e, new TrafficReplayStreamDataComponent { posKeyCount = 0, rotKeyCount = 0, speedKeyCount = 0, lastIndexPos = 0, lastIndexRot = 0, lastIndexSpeed = 0, startTime = 0f, endTime = 0f, looping = 0 });
                        ecb.AddBuffer<Float3Key>(e);
                        ecb.AddBuffer<QuatKey>(e);
                        ecb.AddBuffer<FloatKey>(e);
                    }
                }
                ecb.Playback(EntityManager);
                ecb.Dispose();
            }
        }

        [BurstCompile]
        partial struct RecordWithPhysicsJob : IJobEntity
        {
            public float timeNow;
            public int maxKeys;

            public void Execute(ref TrafficReplayStreamDataComponent meta, ref DynamicBuffer<Float3Key> posBuf, ref DynamicBuffer<QuatKey> rotBuf, ref DynamicBuffer<FloatKey> speedBuf, in LocalToWorld ltw, in PhysicsVelocity physVel)
            {
                ReplayBufferUtils.AddKey(ref posBuf, timeNow, ltw.Position);
                ReplayBufferUtils.AddKey(ref rotBuf, timeNow, ltw.Rotation);
                float speed = math.length(physVel.Linear);
                ReplayBufferUtils.AddKey(ref speedBuf, timeNow, speed);

                if (maxKeys > 0)
                {
                    ReplayBufferUtils.TrimToMax(ref posBuf, maxKeys);
                    ReplayBufferUtils.TrimToMax(ref rotBuf, maxKeys);
                    ReplayBufferUtils.TrimToMax(ref speedBuf, maxKeys);
                }

                meta.posKeyCount = posBuf.Length;
                meta.rotKeyCount = rotBuf.Length;
                meta.speedKeyCount = speedBuf.Length;
            }
        }

        [BurstCompile]
        partial struct RecordWithoutPhysicsJob : IJobEntity
        {
            public float timeNow;
            public int maxKeys;

            public void Execute(ref TrafficReplayStreamDataComponent meta, ref DynamicBuffer<Float3Key> posBuf, ref DynamicBuffer<QuatKey> rotBuf, ref DynamicBuffer<FloatKey> speedBuf, in LocalToWorld ltw)
            {
                ReplayBufferUtils.AddKey(ref posBuf, timeNow, ltw.Position);
                ReplayBufferUtils.AddKey(ref rotBuf, timeNow, ltw.Rotation);
                ReplayBufferUtils.AddKey(ref speedBuf, timeNow, 0f);

                if (maxKeys > 0)
                {
                    ReplayBufferUtils.TrimToMax(ref posBuf, maxKeys);
                    ReplayBufferUtils.TrimToMax(ref rotBuf, maxKeys);
                    ReplayBufferUtils.TrimToMax(ref speedBuf, maxKeys);
                }

                meta.posKeyCount = posBuf.Length;
                meta.rotKeyCount = rotBuf.Length;
                meta.speedKeyCount = speedBuf.Length;
            }
        }

        protected override void OnUpdate()
        {
            if (!SystemAPI.HasSingleton<ReplayRecordingController>()) return;

            var singletonEntity = SystemAPI.GetSingletonEntity<ReplayRecordingController>();
            var ctrl = SystemAPI.GetComponent<ReplayRecordingController>(singletonEntity);

            if (ctrl.isRecording == 0) return;

            float sampleInterval = ctrl.sampleInterval;
            int maxKeys = ctrl.maxKeys;

            var timeNow = (float)SystemAPI.Time.ElapsedTime;

            if (sampleInterval > 0f && (timeNow - ctrl.lastRecordTime) < sampleInterval) return;

            // update lastRecordTime
            ctrl.lastRecordTime = timeNow;
            SystemAPI.SetComponent(singletonEntity, ctrl);

            EnsureBuffersAndComponents();

            var withPhys = new RecordWithPhysicsJob { timeNow = timeNow, maxKeys = maxKeys };
            Dependency = withPhys.Schedule(Dependency);

            var withoutPhys = new RecordWithoutPhysicsJob { timeNow = timeNow, maxKeys = maxKeys };
            Dependency = withoutPhys.Schedule(Dependency);
        }
    }
}
