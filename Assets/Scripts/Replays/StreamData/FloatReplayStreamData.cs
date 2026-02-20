using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Replays
{
	public struct FloatReplayStreamData
	{
		public DynamicBuffer<float> m_times;
		public DynamicBuffer<float> m_values;
		public DynamicBuffer<byte> m_interpTypes;

		private int m_lastFoundIndex; // Поле для кэширования последнего найденного индекса
		
		public int count;

		[BurstCompile]
		public void AddKeyFrame(float time, float value, InterpType interpType = InterpType.Linear)
		{
			if (m_times.IsEmpty)
			{
				m_times = new DynamicBuffer<float>();
				m_values = new DynamicBuffer<float>();
				m_interpTypes = new DynamicBuffer<byte>();
			}
        
			if (count > 0 && m_times[count - 1] >= time) return;
        
			m_times.Add(time);
			m_values.Add(value);
			m_interpTypes.Add((byte)interpType);
			count++;
			// При добавлении нового кадра сбрасываем кэш — он может стать неактуальным
			m_lastFoundIndex = -1;
		}

		[BurstCompile]
		public float ReadValue(float time, float defaultValue)
		{
			if (count == 0) return defaultValue;

			int index = FindKeyFrameIndexWithCache(time);
			if (index < 0) return defaultValue;

			return PerformInterpolation(index, time);
		}

		[BurstCompile]
		private int FindKeyFrameIndexWithCache(float time)
		{
			// Быстрая проверка кэшированного индекса
			if (m_lastFoundIndex >= 0 && m_lastFoundIndex < count)
			{
				// Если время попадает в интервал текущего кэшированного кадра или следующего
				if (time >= m_times[m_lastFoundIndex] &&
				    (m_lastFoundIndex == count - 1 || time <= m_times[m_lastFoundIndex + 1]))
				{
					// Проверяем, не нужно ли сдвинуться вперёд
					while (m_lastFoundIndex < count - 1 && time > m_times[m_lastFoundIndex + 1])
					{
						m_lastFoundIndex++;
					}
					// Или назад (если время уменьшилось)
					while (m_lastFoundIndex > 0 && time < m_times[m_lastFoundIndex])
					{
						m_lastFoundIndex--;
					}
					return m_lastFoundIndex;
				}
			}

			// Если кэш не подходит — выполняем бинарный поиск
			int low = 0, high = count - 1;
			while (low <= high)
			{
				int mid = (low + high) >> 1;
				if (m_times[mid] < time)
					low = mid + 1;
				else if (m_times[mid] > time)
					high = mid - 1;
				else
				{
					m_lastFoundIndex = mid; // Обновляем кэш
					return mid;
				}
			}
			m_lastFoundIndex = high; // Обновляем кэш
			return high;
		}

		[BurstCompile]
		private float PerformInterpolation(int index, float time)
		{
			if (index >= count - 1)
				return m_values[count - 1];

			InterpType type = (InterpType)m_interpTypes[index];
			float t = CalculateInterpolationFactor(index, time);

			switch (type)
			{
				case InterpType.Const:
					return m_values[index];
				case InterpType.Linear:
					return math.lerp(m_values[index], m_values[index + 1], t);
				default:
					return m_values[index]; // fallback
			}
		}

		[BurstCompile]
		private float CalculateInterpolationFactor(int index, float time)
		{
			float time0 = m_times[index];
			float time1 = m_times[index + 1];
			return math.clamp((time - time0) / (time1 - time0), 0f, 1f);
		}
	}
}