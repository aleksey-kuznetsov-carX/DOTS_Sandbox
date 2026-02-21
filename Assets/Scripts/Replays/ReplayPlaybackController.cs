using Unity.Entities;

namespace Replays
{
    public struct ReplayPlaybackController : IComponentData
    {
        public byte isPlaying; // 0/1
        public float playbackTime; // seconds
        public float playbackSpeed; // 1.0 = real time
        public byte loop; // 0/1
    }
}

