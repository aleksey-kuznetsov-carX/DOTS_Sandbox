using Unity.Entities;

namespace Replays
{
    // Lightweight component that stores metadata and cache for a replay stream.
    public struct TrafficReplayStreamDataComponent : IComponentData
    {
        // separate counts per stream
        public int posKeyCount;
        public int rotKeyCount;
        public int speedKeyCount;

        // separate last index caches per stream
        public int lastIndexPos;
        public int lastIndexRot;
        public int lastIndexSpeed;

        public float startTime; // optional
        public float endTime;   // optional
        public byte looping;    // 0 = false, 1 = true
    }
}
