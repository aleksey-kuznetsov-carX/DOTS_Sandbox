using System.IO;
using Unity.Mathematics;

namespace Replays
{
	public class Float3SerializationStrategy : ISerializationStrategy<float3>
	{
		public void Serialize(BinaryWriter writer, float3 value)
		{
			writer.Write(value.x);
			writer.Write(value.y);
			writer.Write(value.z);
		}
    
		public float3 Deserialize(BinaryReader reader)
		{
			return new float3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}
	}
}