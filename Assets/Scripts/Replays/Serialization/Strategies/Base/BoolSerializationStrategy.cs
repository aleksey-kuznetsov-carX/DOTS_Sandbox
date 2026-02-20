using System.IO;

namespace Replays
{
	public class BoolSerializationStrategy : ISerializationStrategy<bool>
	{
		public void Serialize(BinaryWriter writer, bool value) => writer.Write(value);
		public bool Deserialize(BinaryReader reader) => reader.ReadBoolean();
	}
}