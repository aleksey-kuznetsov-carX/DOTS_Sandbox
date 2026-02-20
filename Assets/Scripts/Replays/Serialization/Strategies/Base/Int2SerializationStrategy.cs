using System.IO;
using Unity.Mathematics;

namespace Replays
{
	public class Int2SerializationStrategy : ISerializationStrategy<int2>
	{
		public void Serialize(BinaryWriter writer, int2 value)
		{
			writer.Write(value.x);
			writer.Write(value.y);
		}
    
		public int2 Deserialize(BinaryReader reader)
		{
			return new int2(reader.ReadInt32(), reader.ReadInt32());
		}
	}
}