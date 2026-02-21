using Unity.Entities;
using UnityEngine;

namespace Replays
{
    // Attach to instantiated GameObjects to link them to an entity at runtime
    public class EntityGameObjectLink : MonoBehaviour
    {
        public int replayInstanceId;
        public Entity linkedEntity; // optional, set after playback (requires using Unity.Entities in other code)
    }
}
