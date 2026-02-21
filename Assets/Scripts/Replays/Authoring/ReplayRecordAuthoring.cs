using Unity.Entities;
using UnityEngine;
using Replays;

namespace Replays.Authoring
{
    // Attach this MonoBehaviour to GameObjects you want to record.
    // It will be converted to an entity with `ReplayRecordTag` during conversion (Baker API).
    public class ReplayRecordAuthoring : MonoBehaviour
    {
        class Baker : Baker<ReplayRecordAuthoring>
        {
            public override void Bake(ReplayRecordAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(e, new ReplayRecordTag());
            }
        }
    }
}
