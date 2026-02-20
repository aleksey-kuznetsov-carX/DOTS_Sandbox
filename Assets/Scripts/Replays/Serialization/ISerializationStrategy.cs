using System.IO;

namespace Replays
{
	public interface ISerializationStrategy<T> where T : unmanaged
	{
		void Serialize(BinaryWriter writer, T value);
		T Deserialize(BinaryReader reader);
	}
}