using System.IO;

namespace Replays
{
	public class UIntSerializationStrategy : ISerializationStrategy<uint>
	{
		public void Serialize(BinaryWriter writer, uint value) => writer.Write(value);
		public uint Deserialize(BinaryReader reader) => reader.ReadUInt32();
	}
}