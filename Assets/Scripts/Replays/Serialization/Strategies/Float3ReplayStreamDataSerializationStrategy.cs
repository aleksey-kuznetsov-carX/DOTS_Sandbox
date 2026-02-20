using System.IO;
using Unity.Entities;
using Unity.Mathematics;

namespace Replays
{
	public class Float3ReplayStreamDataSerializationStrategy : ISerializationStrategy<Float3ReplayStreamData>
	{
		public void Serialize(BinaryWriter writer, Float3ReplayStreamData value)
		{
			// Сериализуем количество элементов
			writer.Write(value.count);

			// Получаем стратегии для базовых типов
			var floatStrategy = SerializationStrategies.GetStrategy<float>();
			var float3Strategy = SerializationStrategies.GetStrategy<float3>();
			var byteStrategy = SerializationStrategies.GetStrategy<byte>();

			// Сериализуем m_times
			for (int i = 0; i < value.count; i++)
			{
				floatStrategy.Serialize(writer, value.m_times[i]);
			}

			// Сериализуем m_values (тип float3)
			for (int i = 0; i < value.count; i++)
			{
				float3Strategy.Serialize(writer, value.m_values[i]);
			}

			// Сериализуем m_interpTypes
			for (int i = 0; i < value.count; i++)
			{
				byteStrategy.Serialize(writer, value.m_interpTypes[i]);
			}
		}

		public Float3ReplayStreamData Deserialize(BinaryReader reader)
		{
			var result = new Float3ReplayStreamData();

			// Десериализуем количество элементов
			result.count = reader.ReadInt32();

			if (result.count > 0)
			{
				// Инициализируем буферы с предварительной выделением ёмкости
				result.m_times = new DynamicBuffer<float>();
				result.m_times.ResizeUninitialized(result.count);

				result.m_values = new DynamicBuffer<float3>();
				result.m_values.ResizeUninitialized(result.count);

				result.m_interpTypes = new DynamicBuffer<byte>();
				result.m_interpTypes.ResizeUninitialized(result.count);

				// Получаем стратегии для базовых типов
				var floatStrategy = SerializationStrategies.GetStrategy<float>();
				var float3Strategy = SerializationStrategies.GetStrategy<float3>();
				var byteStrategy = SerializationStrategies.GetStrategy<byte>();

				// Десериализуем m_times
				for (int i = 0; i < result.count; i++)
				{
					float time = floatStrategy.Deserialize(reader);
					result.m_times[i] = time;
				}

				// Десериализуем m_values
				for (int i = 0; i < result.count; i++)
				{
					float3 value = float3Strategy.Deserialize(reader);
					result.m_values[i] = value;
				}

				// Десериализуем m_interpTypes
				for (int i = 0; i < result.count; i++)
				{
					byte interpType = byteStrategy.Deserialize(reader);
					result.m_interpTypes[i] = interpType;
				}
			}
			else
			{
				// Если count == 0, инициализируем пустые буферы
				result.m_times = new DynamicBuffer<float>();
				result.m_values = new DynamicBuffer<float3>();
				result.m_interpTypes = new DynamicBuffer<byte>();
			}
        
			return result;
		}
	}
}