using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Replays
{
    // Attach a single instance of this MonoBehaviour in the scene.
    // It will link GameObjects with EntityGameObjectLink to ECS entities (by ReplayInstanceId)
    // and copy LocalTransform (preferred) or LocalToWorld (fallback) to the GameObject.transform each frame.
    public class EntityGameObjectSync : MonoBehaviour
    {
        EntityManager em;
        List<EntityGameObjectLink> cachedLinks;
        public bool debugLogs = false;

        void Start()
        {
            em = World.DefaultGameObjectInjectionWorld.EntityManager;
            // cache links once (loader instantiates GameObjects before playback)
            RefreshCachedLinks();
        }

        public void RefreshCachedLinks()
        {
            cachedLinks = new List<EntityGameObjectLink>(FindObjectsOfType<EntityGameObjectLink>());
            if (debugLogs) Debug.Log($"EntityGameObjectSync: cached {cachedLinks.Count} links");
        }

        void LateUpdate()
        {
            if (em == null) return;

            // Make sure all ECS jobs finished so component reads are up-to-date
            em.CompleteAllTrackedJobs();

            // Build mapping id -> entity
            var q = em.CreateEntityQuery(ComponentType.ReadOnly<ReplayInstanceId>());
            using var ents = q.ToEntityArray(Unity.Collections.Allocator.Temp);
            using var ids = q.ToComponentDataArray<ReplayInstanceId>(Unity.Collections.Allocator.Temp);
            var map = new Dictionary<int, Entity>(ents.Length);
            for (int i = 0; i < ents.Length; ++i)
            {
                map[ids[i].id] = ents[i];
            }

            // If cache is empty, refresh once
            if (cachedLinks == null || cachedLinks.Count == 0)
                RefreshCachedLinks();

            // Iterate cached links
            foreach (var link in cachedLinks)
            {
                if (link == null) continue;
                if (!map.TryGetValue(link.replayInstanceId, out var ent))
                {
                    if (debugLogs) Debug.LogWarning($"No entity for replayInstanceId={link.replayInstanceId}");
                    continue;
                }

                link.linkedEntity = ent;

                // Prefer LocalTransform
                if (em.HasComponent<LocalTransform>(ent))
                {
                    var lt = em.GetComponentData<LocalTransform>(ent);
                    SetTransform(link.gameObject.transform, lt.Position, lt.Rotation);
                }
                else if (em.HasComponent<LocalToWorld>(ent))
                {
                    var ltw = em.GetComponentData<LocalToWorld>(ent);
                    var m = ltw.Value;
                    var pos = new Vector3(m.c3.x, m.c3.y, m.c3.z);
                    var rot = Float4x4ToQuaternion(m);
                    link.gameObject.transform.SetPositionAndRotation(pos, rot);
                }
            }
        }

        static void SetTransform(Transform t, float3 pos, quaternion rot)
        {
            t.SetPositionAndRotation(new Vector3(pos.x, pos.y, pos.z), new Quaternion(rot.value.x, rot.value.y, rot.value.z, rot.value.w));
        }

        static Quaternion Float4x4ToQuaternion(float4x4 m)
        {
            var mat = new UnityEngine.Matrix4x4();
            mat.m00 = m.c0.x; mat.m01 = m.c1.x; mat.m02 = m.c2.x; mat.m03 = m.c3.x;
            mat.m10 = m.c0.y; mat.m11 = m.c1.y; mat.m12 = m.c2.y; mat.m13 = m.c3.y;
            mat.m20 = m.c0.z; mat.m21 = m.c1.z; mat.m22 = m.c2.z; mat.m23 = m.c3.z;
            mat.m30 = m.c0.w; mat.m31 = m.c1.w; mat.m32 = m.c2.w; mat.m33 = m.c3.w;
            return mat.rotation;
        }
    }
}
