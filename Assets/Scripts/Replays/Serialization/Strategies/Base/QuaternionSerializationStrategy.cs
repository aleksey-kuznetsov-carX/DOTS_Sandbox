using System.IO;
using Unity.Mathematics;

namespace Replays
{
	public class QuaternionSerializationStrategy : ISerializationStrategy<quaternion>
	{
		public void Serialize(BinaryWriter writer, quaternion value)
		{
			writer.Write(value.value.x);
			writer.Write(value.value.y);
			writer.Write(value.value.z);
			writer.Write(value.value.w);
		}
    
		public quaternion Deserialize(BinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();
			float w = reader.ReadSingle();
			return new quaternion(x, y, z, w);
		}
	}
}