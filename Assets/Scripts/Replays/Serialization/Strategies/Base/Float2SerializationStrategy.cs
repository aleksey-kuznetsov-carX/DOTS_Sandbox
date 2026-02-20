using System.IO;
using Unity.Mathematics;

namespace Replays
{
	public class Float2SerializationStrategy : ISerializationStrategy<float2>
	{
		public void Serialize(BinaryWriter writer, float2 value)
		{
			writer.Write(value.x);
			writer.Write(value.y);
		}
    
		public float2 Deserialize(BinaryReader reader)
		{
			return new float2(reader.ReadSingle(), reader.ReadSingle());
		}
	}
}