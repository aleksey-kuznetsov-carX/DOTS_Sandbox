using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Replays
{
    // Debug helper: press 'L' to list registry; press 'K' to dump first entity transform
    public class DebugReplayInspector : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                var entries = GameObjectEntityRegistry.GetAllEntries();
                Debug.Log($"Registry entries: {entries.Length}");
                foreach (var e in entries) Debug.Log($"id={e.Key} go={e.Value.name}");
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                var q = em.CreateEntityQuery(ComponentType.ReadOnly<ReplayInstanceId>());
                using var ents = q.ToEntityArray(Unity.Collections.Allocator.Temp);
                using var ids = q.ToComponentDataArray<ReplayInstanceId>(Unity.Collections.Allocator.Temp);
                if (ents.Length > 0)
                {
                    var ent = ents[0];
                    var id = ids[0].id;
                    Debug.Log($"First entity {ent} id={id}");
                    if (em.HasComponent<LocalTransform>(ent)) { var lt = em.GetComponentData<LocalTransform>(ent); Debug.Log($"LT pos={lt.Position} rot={lt.Rotation}"); }
                    if (em.HasComponent<LocalToWorld>(ent)) { var ltw = em.GetComponentData<LocalToWorld>(ent); Debug.Log($"LTW c3={ltw.Value.c3}"); }
                }
            }
        }
    }
}

