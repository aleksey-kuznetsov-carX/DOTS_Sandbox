using System.IO;

namespace Replays
{
	public class IntSerializationStrategy : ISerializationStrategy<int>
	{
		public void Serialize(BinaryWriter writer, int value) => writer.Write(value);
		public int Deserialize(BinaryReader reader) => reader.ReadInt32();
	}
}