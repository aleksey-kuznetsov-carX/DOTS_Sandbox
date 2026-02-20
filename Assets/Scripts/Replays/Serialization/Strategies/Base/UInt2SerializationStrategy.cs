using System.IO;
using Unity.Mathematics;

namespace Replays
{
	public class UInt2SerializationStrategy : ISerializationStrategy<uint2>
	{
		public void Serialize(BinaryWriter writer, uint2 value)
		{
			writer.Write(value.x);
			writer.Write(value.y);
		}
    
		public uint2 Deserialize(BinaryReader reader)
		{
			return new uint2(reader.ReadUInt32(), reader.ReadUInt32());
		}
	}
}