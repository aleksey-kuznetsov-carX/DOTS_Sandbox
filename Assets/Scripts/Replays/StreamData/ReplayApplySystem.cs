using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Replays
{
    // Example SystemBase that samples both Float3 and Quat streams on each entity that has them.
    // public partial class ReplayApplySystem : SystemBase
    // {
    //     [BurstCompile]
    //     partial struct ApplyJob : IJobEntity
    //     {
    //         public float timeNow;
    //
    //         public void Execute(ref TrafficReplayStreamDataComponent meta, in DynamicBuffer<Float3Key> floatBuf, in DynamicBuffer<QuatKey> quatBuf)
    //         {
    //             meta.keyCount = floatBuf.Length;
    //             int idxCache = meta.lastIndex;
    //             _ = ReplayBufferUtils.SampleFloat3(floatBuf, timeNow, ref idxCache, new float3(0f));
    //             int rotCache = meta.lastIndex;
    //             _ = ReplayBufferUtils.SampleQuat(quatBuf, timeNow, ref rotCache, quaternion.identity);
    //             meta.lastIndex = idxCache;
    //         }
    //     }
    //
    //     protected override void OnUpdate()
    //     {
    //         var job = new ApplyJob { timeNow = (float)SystemAPI.Time.ElapsedTime };
    //         Dependency = job.Schedule(Dependency);
    //     }
    // }
}
