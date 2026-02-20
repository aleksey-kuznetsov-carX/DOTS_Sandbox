using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Replays
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateBefore(typeof(EntityLifecycleManagerSystem))]
	public partial class DynamicReplayRecorderSystem : SystemBase
	{
		private EntityCommandBufferSystem _ecbSystem;

		protected override void OnCreate()
		{
			base.OnCreate();
			_ecbSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
			float currentTime = (float)SystemAPI.Time.ElapsedTime;

			Entities
				.WithAll<LocalToWorld, PhysicsVelocity>()
				.ForEach((int entityInQueryIndex, Entity entity, ref TrafficReplayStreamDataComponent replayComp,
					in LocalToWorld localToWorld, in PhysicsVelocity velocity) =>
				{
					if (!replayComp.IsRecording) return;

					// Инициализируем поток данных, если его ещё нет
					if (!replayComp.PositionData.IsCreated)
					{
						replayComp.PositionData = new NativeReference<Float3ReplayStreamData>(Allocator.Persistent);
						replayComp.PositionData.Value = new Float3ReplayStreamData();
						
						replayComp.RotationData = new NativeReference<QuaternionReplayStreamData>(Allocator.Persistent);
						replayComp.RotationData.Value = new QuaternionReplayStreamData();
					}

					// Записываем позицию
					float3 position = localToWorld.Position;
					replayComp.PositionData.Value.AddKeyFrame(
						currentTime, position);
					
					quaternion rotation = localToWorld.Rotation;
					replayComp.RotationData.Value.AddKeyFrame(
						currentTime, rotation);

					ecb.SetComponent(entityInQueryIndex, entity, replayComp);
				})
				.ScheduleParallel();

			_ecbSystem.AddJobHandleForProducer(Dependency);
		}
	}
}