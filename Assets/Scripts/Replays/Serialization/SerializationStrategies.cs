using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Replays
{
	public static class SerializationStrategies
	{
		private static readonly Dictionary<Type, object> m_strategies = new();

		static SerializationStrategies()
		{
			// Базовые типы
			m_strategies[typeof(float)] = new FloatSerializationStrategy();
			m_strategies[typeof(double)] = new DoubleSerializationStrategy();
			m_strategies[typeof(int)] = new IntSerializationStrategy();
			m_strategies[typeof(uint)] = new UIntSerializationStrategy();
			m_strategies[typeof(bool)] = new BoolSerializationStrategy();
			m_strategies[typeof(byte)] = new ByteSerializationStrategy();

			// Векторные типы
			m_strategies[typeof(float2)] = new Float2SerializationStrategy();
			m_strategies[typeof(float3)] = new Float3SerializationStrategy();
			m_strategies[typeof(float4)] = new Float4SerializationStrategy();
			m_strategies[typeof(int2)] = new Int2SerializationStrategy();
			m_strategies[typeof(uint2)] = new UInt2SerializationStrategy();

			// Специальные типы
			m_strategies[typeof(quaternion)] = new QuaternionSerializationStrategy();
			m_strategies[typeof(half)] = new HalfSerializationStrategy();
			m_strategies[typeof(half2)] = new Half2SerializationStrategy();
			
			//ReplayStreamData
			m_strategies[typeof(FloatReplayStreamData)] = new FloatReplayStreamDataSerializationStrategy();
			m_strategies[typeof(Float3ReplayStreamData)] = new Float3ReplayStreamDataSerializationStrategy();
			m_strategies[typeof(QuaternionReplayStreamData)] = new Float3ReplayStreamDataSerializationStrategy();
		}

		public static ISerializationStrategy<T> GetStrategy<T>() where T : unmanaged
		{
			if (m_strategies.TryGetValue(typeof(T), out var strategy))
			{
				return (ISerializationStrategy<T>)strategy;
			}
			throw new NotSupportedException($"No serialization strategy found for type {typeof(T)}");
		}
	}
}