// ...existing code...
using Unity.Entities;

namespace Replays
{
    // Singleton to control recording
    public struct ReplayRecordingController : IComponentData
    {
        public byte isRecording; // 0 = false, 1 = true
        public float sampleInterval; // seconds between samples; 0 = every frame
        public float lastRecordTime; // internal: time of last sample
        public int maxKeys; // 0 = unlimited
        public byte exportOnStop; // if 1, export buffers when stopping
    }

    // Tag to mark entities whose transform/physics should be recorded
    public struct ReplayRecordTag : IComponentData { }
}

