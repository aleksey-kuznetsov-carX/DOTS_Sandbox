using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;

namespace Replays
{
    public enum InterpType : byte
    {
        None = 0,
        Const = 1,
        Linear = 2,
        Parab = 3,
        Cubic = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    [InternalBufferCapacity(8)]
    public struct Float3Key : IBufferElementData
    {
        // Time first to make binary searches simpler (time-sorted)
        public float Time;
        public float3 Value;
        public byte Interp; // stores InterpType as byte
    }

    [StructLayout(LayoutKind.Sequential)]
    [InternalBufferCapacity(4)]
    public struct QuatKey : IBufferElementData
    {
        public float Time;
        public quaternion Value;
        public byte Interp;
    }

    // New: simple float key for scalar streams (e.g., speed)
    [StructLayout(LayoutKind.Sequential)]
    [InternalBufferCapacity(8)]
    public struct FloatKey : IBufferElementData
    {
        public float Time;
        public float Value;
        public byte Interp;
    }
}
