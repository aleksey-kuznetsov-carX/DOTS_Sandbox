// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Physics;
// using Unity.Transforms;
// using UnityEngine;
//
// namespace Replays
// {
// 	[UpdateInGroup(typeof(SimulationSystemGroup))]
// 	[UpdateBefore(typeof(EntityLifecycleManagerSystem))]
// 	public partial class DynamicReplayReplayerSystem : SystemBase
// 	{
// 		protected override void OnUpdate()
// 		{
// 			float currentTime = (float)SystemAPI.Time.ElapsedTime;
//
// 			// Entities
// 			// 	.WithAll<TrafficReplayStreamDataComponent>()
// 			// 	.WithNone<PhysicsVelocity>() // Только для нефизических объектов
// 			// 	.ForEach((ref LocalToWorld transform, ref TrafficReplayStreamDataComponent trafficReplayComp) =>
// 			// 	{
// 			// 		if (trafficReplayComp.PositionData.Value.count == 0) return;
// 			//
// 			// 		// Читаем интерполированную позицию
// 			// 		float3 position = trafficReplayComp.PositionData.Value.ReadValue(
// 			// 			currentTime, transform.Position);
// 			//
// 			// 		quaternion rotation = trafficReplayComp.RotationData.Value.ReadValue(
// 			// 			currentTime, transform.Rotation);
// 			// 		
// 			// 		// Обновляем позицию
// 			// 		transform = new LocalToWorld { Value = float4x4.TRS(position, rotation, float3.zero) };
// 			// 	})
// 			// 	.ScheduleParallel();
// 		}
// 	}
// }