using System.IO;
using Unity.Mathematics;

namespace Replays
{
	// Float

	// Double

	// Int

	// UInt

	// Bool

	// Byte

	// float2

	// float3

	// float4

	// int2

	// uint2

	// quaternion

	// half

	// half2
	public class Half2SerializationStrategy : ISerializationStrategy<half2>
	{
		public void Serialize(BinaryWriter writer, half2 value)
		{
			writer.Write((short)value.x.value);
			writer.Write((short)value.y.value);
		}
    
		public half2 Deserialize(BinaryReader reader)
		{
			short x = reader.ReadInt16();
			short y = reader.ReadInt16();
			return new half2(new half(x), new half(y));
		}
	}
}