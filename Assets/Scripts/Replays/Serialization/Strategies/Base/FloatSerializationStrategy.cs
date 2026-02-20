using System.IO;

namespace Replays
{
	public class FloatSerializationStrategy : ISerializationStrategy<float>
	{
		public void Serialize(BinaryWriter writer, float value) => writer.Write(value);
		public float Deserialize(BinaryReader reader) => reader.ReadSingle();
	}
}