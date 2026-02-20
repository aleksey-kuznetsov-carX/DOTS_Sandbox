using System.IO;
using Unity.Mathematics;

namespace Replays
{
	public class Float4SerializationStrategy : ISerializationStrategy<float4>
	{
		public void Serialize(BinaryWriter writer, float4 value)
		{
			writer.Write(value.x);
			writer.Write(value.y);
			writer.Write(value.z);
			writer.Write(value.w);
		}
    
		public float4 Deserialize(BinaryReader reader)
		{
			return new float4(reader.ReadSingle(), reader.ReadSingle(),
				reader.ReadSingle(), reader.ReadSingle());
		}
	}
}