using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Replays
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class ReplayGameObjectSyncSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var em = EntityManager;

            // Query entities that have ReplayInstanceId and LocalTransform/LocalToWorld
            var q = GetEntityQuery(ComponentType.ReadOnly<ReplayInstanceId>());
            using var ents = q.ToEntityArray(Unity.Collections.Allocator.Temp);
            using var ids = q.ToComponentDataArray<ReplayInstanceId>(Unity.Collections.Allocator.Temp);

            for (int i = 0; i < ents.Length; ++i)
            {
                var id = ids[i].id;
                if (!GameObjectEntityRegistry.TryGet(id, out var go)) continue;
                var ent = ents[i];

                if (em.HasComponent<LocalTransform>(ent))
                {
                    var lt = em.GetComponentData<LocalTransform>(ent);
                    go.transform.SetPositionAndRotation(new Vector3(lt.Position.x, lt.Position.y, lt.Position.z), new Quaternion(lt.Rotation.value.x, lt.Rotation.value.y, lt.Rotation.value.z, lt.Rotation.value.w));
                }
                else if (em.HasComponent<LocalToWorld>(ent))
                {
                    var ltw = em.GetComponentData<LocalToWorld>(ent);
                    var m = ltw.Value;
                    // build Unity Matrix4x4 from float4x4 and extract rotation
                    var mat = new UnityEngine.Matrix4x4();
                    mat.m00 = m.c0.x; mat.m01 = m.c1.x; mat.m02 = m.c2.x; mat.m03 = m.c3.x;
                    mat.m10 = m.c0.y; mat.m11 = m.c1.y; mat.m12 = m.c2.y; mat.m13 = m.c3.y;
                    mat.m20 = m.c0.z; mat.m21 = m.c1.z; mat.m22 = m.c2.z; mat.m23 = m.c3.z;
                    mat.m30 = m.c0.w; mat.m31 = m.c1.w; mat.m32 = m.c2.w; mat.m33 = m.c3.w;
                    var pos = new Vector3(mat.m03, mat.m13, mat.m23);
                    var rot = mat.rotation;
                    go.transform.SetPositionAndRotation(pos, rot);
                }
            }
        }
    }
}
