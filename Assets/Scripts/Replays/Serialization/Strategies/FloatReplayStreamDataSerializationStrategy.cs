using System.IO;
using Unity.Entities;

namespace Replays
{
	public class FloatReplayStreamDataSerializationStrategy : ISerializationStrategy<FloatReplayStreamData>
	{
		public void Serialize(BinaryWriter writer, FloatReplayStreamData value)
		{
			// Сериализуем количество элементов
			writer.Write(value.count);

			// Получаем стратегии для базовых типов
			var floatStrategy = SerializationStrategies.GetStrategy<float>();
			var byteStrategy = SerializationStrategies.GetStrategy<byte>();

			// Сериализуем m_times
			for (int i = 0; i < value.count; i++)
			{
				floatStrategy.Serialize(writer, value.m_times[i]);
			}

			// Сериализуем m_values
			for (int i = 0; i < value.count; i++)
			{
				floatStrategy.Serialize(writer, value.m_values[i]);
			}

			// Сериализуем m_interpTypes
			for (int i = 0; i < value.count; i++)
			{
				byteStrategy.Serialize(writer, value.m_interpTypes[i]);
			}
		}

		public FloatReplayStreamData Deserialize(BinaryReader reader)
		{
			var result = new FloatReplayStreamData();

			// Десериализуем количество элементов
			result.count = reader.ReadInt32();

			if (result.count > 0)
			{
				// Инициализируем буферы
				result.m_times = new DynamicBuffer<float>();
				result.m_times.ResizeUninitialized(result.count);
            
				result.m_values = new DynamicBuffer<float>();
				result.m_values.ResizeUninitialized(result.count);
            
				result.m_interpTypes = new DynamicBuffer<byte>();
				result.m_interpTypes.ResizeUninitialized(result.count);
            
				// Получаем стратегии для базовых типов
				var floatStrategy = SerializationStrategies.GetStrategy<float>();
				var byteStrategy = SerializationStrategies.GetStrategy<byte>();

				// Десериализуем m_times
				for (int i = 0; i < result.count; i++)
				{
					float time = floatStrategy.Deserialize(reader);
					result.m_times.Add(time);
				}

				// Десериализуем m_values
				for (int i = 0; i < result.count; i++)
				{
					float value = floatStrategy.Deserialize(reader);
					result.m_values.Add(value);
				}

				// Десериализуем m_interpTypes
				for (int i = 0; i < result.count; i++)
				{
					byte interpType = byteStrategy.Deserialize(reader);
					result.m_interpTypes.Add(interpType);
				}
			}
			else
			{
				// Если count == 0, инициализируем пустые буферы
				result.m_times = new DynamicBuffer<float>();
				result.m_values = new DynamicBuffer<float>();
				result.m_interpTypes = new DynamicBuffer<byte>();
			}
        
			return result;
		}
	}
}