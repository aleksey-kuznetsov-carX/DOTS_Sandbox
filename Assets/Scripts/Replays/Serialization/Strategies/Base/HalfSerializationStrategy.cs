using System.IO;
using Unity.Mathematics;

namespace Replays
{
	public class HalfSerializationStrategy : ISerializationStrategy<half>
	{
		public void Serialize(BinaryWriter writer, half value)
		{
			writer.Write((short)value.value);
		}
    
		public half Deserialize(BinaryReader reader)
		{
			short value = reader.ReadInt16();
			return new half(value);
		}
	}
}