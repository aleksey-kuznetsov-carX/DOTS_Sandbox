using Unity.Collections;
using Unity.Entities;

namespace Replays
{
	public struct TrafficReplayStreamDataComponent : IComponentData
	{
		public NativeReference<Float3ReplayStreamData> PositionData;
		public NativeReference<QuaternionReplayStreamData> RotationData;
		public bool IsRecording;
		public Entity OwnerEntity; // Ссылка на управляющую сущность
	}
}