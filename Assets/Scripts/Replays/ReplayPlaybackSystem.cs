using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Replays
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class ReplayPlaybackSystem : SystemBase
    {
        [BurstCompile]
        partial struct PlaybackApplyLocalToWorldJob : IJobEntity
        {
            public float currentTime;
            [ReadOnly] public ComponentLookup<LocalTransform> localTransformLookup;

            public void Execute(Entity e, ref LocalToWorld ltw, ref TrafficReplayStreamDataComponent meta, ref DynamicBuffer<Float3Key> posBuf, ref DynamicBuffer<QuatKey> rotBuf)
            {
                if (localTransformLookup.HasComponent(e)) return; // prefer LocalTransform pathway
                int posIdx = meta.lastIndexPos;
                int rotIdx = meta.lastIndexRot;
                var pos = ReplayBufferUtils.SampleFloat3(posBuf, currentTime, ref posIdx, new float3(0f));
                var rot = ReplayBufferUtils.SampleQuat(rotBuf, currentTime, ref rotIdx, quaternion.identity);

                meta.lastIndexPos = posIdx;
                meta.lastIndexRot = rotIdx;

                ltw.Value = float4x4.TRS(pos, rot, new float3(1f));
            }
        }

        [BurstCompile]
        partial struct PlaybackApplyTRJob : IJobEntity
        {
            public float currentTime;

            public void Execute(ref LocalTransform localTransform, ref TrafficReplayStreamDataComponent meta, ref DynamicBuffer<Float3Key> posBuf, ref DynamicBuffer<QuatKey> rotBuf)
            {
                int posIdx = meta.lastIndexPos;
                int rotIdx = meta.lastIndexRot;
                var pos = ReplayBufferUtils.SampleFloat3(posBuf, currentTime, ref posIdx, new float3(0f));
                var rot = ReplayBufferUtils.SampleQuat(rotBuf, currentTime, ref rotIdx, quaternion.identity);

                meta.lastIndexPos = posIdx;
                meta.lastIndexRot = rotIdx;

                // set LocalTransform (position, rotation, scale)
                localTransform.Position = pos;
                localTransform.Rotation = rot;
                localTransform.Scale = 1f;
            }
        }

        protected override void OnUpdate()
        {
            if (!SystemAPI.HasSingleton<ReplayPlaybackController>()) return;
            var singletonEntity = SystemAPI.GetSingletonEntity<ReplayPlaybackController>();
            var ctrl = SystemAPI.GetComponent<ReplayPlaybackController>(singletonEntity);

            if (ctrl.isPlaying == 0) return;

            float dt = SystemAPI.Time.DeltaTime * ctrl.playbackSpeed;
            ctrl.playbackTime += dt;
            SystemAPI.SetComponent(singletonEntity, ctrl);

            // Prepare component lookup for LocalTransform to let LocalToWorld job skip entities that have LocalTransform
            var localTransformLookup = GetComponentLookup<LocalTransform>(true);
            var job1 = new PlaybackApplyLocalToWorldJob { currentTime = ctrl.playbackTime, localTransformLookup = localTransformLookup };

            // Schedule TR job first (updates LocalTransform), then LocalToWorld job will skip those entities
            var job2 = new PlaybackApplyTRJob { currentTime = ctrl.playbackTime };
            Dependency = job2.Schedule(Dependency);
            Dependency = job1.Schedule(Dependency);
        }
    }
}
