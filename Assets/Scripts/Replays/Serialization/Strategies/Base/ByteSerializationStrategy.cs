using System.IO;

namespace Replays
{
	public class ByteSerializationStrategy : ISerializationStrategy<byte>
	{
		public void Serialize(BinaryWriter writer, byte value) => writer.Write(value);
		public byte Deserialize(BinaryReader reader) => reader.ReadByte();
	}
}