using System;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Replays
{
    // Binary exporter: writes a compact binary file with recorded streams
    // Format (little-endian):
    // int magic = 0x5245504C ('REPL')
    // int version = 1
    // int entityCount
    // for each entity:
    //   int entityIndex
    //   int posCount
    //   posCount * (float time, float x, float y, float z)
    //   int rotCount
    //   rotCount * (float time, float x, float y, float z, float w)
    //   int speedCount
    //   speedCount * (float time, float value)

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class ReplayExportSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.HasSingleton<ReplayRecordingController>()) return;
            var singletonEntity = SystemAPI.GetSingletonEntity<ReplayRecordingController>();
            var ctrl = SystemAPI.GetComponent<ReplayRecordingController>(singletonEntity);

            // export when exportOnStop==1 and recording just stopped (isRecording==0 and lastRecordTime>0)
            if (ctrl.exportOnStop == 1 && ctrl.isRecording == 0 && ctrl.lastRecordTime > 0f)
            {
                var query = GetEntityQuery(ComponentType.ReadOnly<ReplayRecordTag>(), ComponentType.ReadOnly<TrafficReplayStreamDataComponent>());
                using var entities = query.ToEntityArray(Allocator.TempJob);

                var path = Path.Combine(Application.persistentDataPath, $"replay_{DateTime.Now:yyyyMMdd_HHmmss}.repl");

                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(0x5245504C); // 'REPL'
                    bw.Write(1); // version
                    bw.Write(entities.Length);

                    var em = EntityManager;
                    foreach (var e in entities)
                    {
                        bw.Write(e.Index);

                        var pos = em.GetBuffer<Float3Key>(e);
                        bw.Write(pos.Length);
                        for (int i = 0; i < pos.Length; ++i)
                        {
                            bw.Write(pos[i].Time);
                            bw.Write(pos[i].Value.x);
                            bw.Write(pos[i].Value.y);
                            bw.Write(pos[i].Value.z);
                        }

                        var rot = em.GetBuffer<QuatKey>(e);
                        bw.Write(rot.Length);
                        for (int i = 0; i < rot.Length; ++i)
                        {
                            bw.Write(rot[i].Time);
                            bw.Write(rot[i].Value.value.x);
                            bw.Write(rot[i].Value.value.y);
                            bw.Write(rot[i].Value.value.z);
                            bw.Write(rot[i].Value.value.w);
                        }

                        var sp = em.GetBuffer<FloatKey>(e);
                        bw.Write(sp.Length);
                        for (int i = 0; i < sp.Length; ++i)
                        {
                            bw.Write(sp[i].Time);
                            bw.Write(sp[i].Value);
                        }
                    }
                }

                Debug.Log($"Replay exported to {path}");

                // reset export flag and lastRecordTime to avoid repeated export
                ctrl.exportOnStop = 0;
                ctrl.lastRecordTime = 0f;
                SystemAPI.SetComponent(singletonEntity, ctrl);
            }
        }
    }
}
