// using Unity.Burst;
// using Unity.Burst.Intrinsics;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Physics;
// using Unity.Transforms;
//
// namespace Replays
// {
// 	public struct DestroyedTag : IComponentData { }
//
//     [UpdateInGroup(typeof(SimulationSystemGroup))]
//     public partial class EntityLifecycleManagerSystem312312312312 : SystemBase
//     {
//        private EndSimulationEntityCommandBufferSystem _ecbSystem;
//
//     protected override void OnCreate()
//     {
//         base.OnCreate();
//         //_ecbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
//     }
//
//     protected override void OnUpdate()
//     {
//         // var ecbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
//         // var ecb = ecbSystem.CreateCommandBuffer();
//         //
//         // var query = GetEntityQuery(
//         //     ComponentType.ReadOnly<TrafficReplayStreamDataComponent>(),
//         //     ComponentType.ReadOnly<DestroyedTag>()
//         // );
//         //
//         // var entities = query.ToEntityArray(Allocator.Temp);
//         //
//         // for (int i = 0; i < entities.Length; i++)
//         // {
//         //     var entity = entities[i];
//         //
//         //     var replayComp = query.GetComponent<TrafficReplayStreamDataComponent>(entity);
//         //
//         //     if (replayComp.PositionData.IsCreated)
//         //     {
//         //         replayComp.PositionData.Dispose();
//         //     }
//         //     if (replayComp.RotationData.IsCreated)
//         //     {
//         //         replayComp.RotationData.Dispose();
//         //     }
//         //
//         //     // ПРАВИЛЬНО для прямого перебора: используем обычный Writer
//         //     ecb.RemoveComponent<TrafficReplayStreamDataComponent>(entity);
//         // }
//         //
//         // entities.Dispose();
//         // ecbSystem.AddJobHandleForProducer(Dependency);
//     }
//
//     private void CleanupDestroyedEntities(EntityCommandBuffer ecb)
//     {
//         // var query = GetEntityQuery(
//         //     ComponentType.ReadOnly<TrafficReplayStreamDataComponent>(),
//         //     ComponentType.ReadOnly<DestroyedTag>()
//         // );
//         //
//         // var replayStreamLookup = GetComponentLookup<TrafficReplayStreamDataComponent>(true);
//         // var destroyedEntities = query.ToEntityArray(Allocator.Temp);
//         //
//         // foreach (var entity in destroyedEntities)
//         // {
//         //     if (replayStreamLookup.HasComponent(entity))
//         //     {
//         //         var replayComp = replayStreamLookup[entity];
//         //
//         //         if (replayComp.PositionData.IsCreated)
//         //         {
//         //             replayComp.PositionData.Dispose();
//         //         }
//         //         if (replayComp.RotationData.IsCreated)
//         //         {
//         //             replayComp.RotationData.Dispose();
//         //         }
//         //     }
//         //
//         //     ecb.RemoveComponent<TrafficReplayStreamDataComponent>(entity);
//         // }
//         //
//         // destroyedEntities.Dispose();
//     }
//     
//     // private struct CleanupJob : IJobParallelFor
//     // {
//     //     [ReadOnly] public NativeArray<Entity> DestroyedEntities;
//     //     [ReadOnly] public ComponentLookup<TrafficReplayStreamDataComponent> ReplayStreamLookup;
//     //     public EntityCommandBuffer.ParallelWriter Ecb;
//     //
//     //     public void Execute(int index)
//     //     {
//     //         var entity = DestroyedEntities[index];
//     //
//     //         if (ReplayStreamLookup.HasComponent(entity))
//     //         {
//     //             var replayComp = ReplayStreamLookup[entity];
//     //
//     //             if (replayComp.PositionData.IsCreated)
//     //             {
//     //                 replayComp.PositionData.Dispose();
//     //             }
//     //             if (replayComp.RotationData.IsCreated)
//     //             {
//     //                 replayComp.RotationData.Dispose();
//     //             }
//     //
//     //             Ecb.RemoveComponent<TrafficReplayStreamDataComponent>(index, entity);
//     //         }
//     //     }
//     // }
//     
//     // private partial struct CleanupJob : IJobEntity
//     // {
//     //     // public EntityCommandBuffer.ParallelWriter Ecb;
//     //     //
//     //     // void Execute(Entity entity, in TrafficReplayStreamDataComponent replayComp, in DestroyedTag destroyedTag)
//     //     // {
//     //     //     // Освобождаем выделенную память
//     //     //     if (replayComp.PositionData.IsCreated)
//     //     //     {
//     //     //         replayComp.PositionData.Dispose();
//     //     //     }
//     //     //     if (replayComp.RotationData.IsCreated)
//     //     //     {
//     //     //         replayComp.RotationData.Dispose();
//     //     //     }
//     //     //
//     //     //     Ecb.RemoveComponent<TrafficReplayStreamDataComponent>(entity);
//     //     // }
//     // }
//
//     }
// }
