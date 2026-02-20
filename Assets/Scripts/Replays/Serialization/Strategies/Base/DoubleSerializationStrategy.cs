using System.IO;

namespace Replays
{
	public class DoubleSerializationStrategy : ISerializationStrategy<double>
	{
		public void Serialize(BinaryWriter writer, double value) => writer.Write(value);
		public double Deserialize(BinaryReader reader) => reader.ReadDouble();
	}
}