using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace Replays
{
    // Safe loader: uses ECB only (no GameObjectConversionUtility).
    // If instantiateFromPrefab==true we Instantiate the GameObject visually and create a matching entity.
    public class ReplayPlaybackLoader : MonoBehaviour
    {
        public string filename;
        public bool instantiateFromPrefab = false;
        public GameObject defaultPrefab;
        public GameObject[] prefabsByRecord;

        EntityManager m_em;

        void Start()
        {
            m_em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (string.IsNullOrEmpty(filename)) return;
            var path = filename;
            if (!Path.IsPathRooted(path)) path = Path.Combine(Application.persistentDataPath, filename);
            if (!File.Exists(path)) { Debug.LogError($"Replay file not found: {path}"); return; }
            LoadBinaryReplay(path);
        }

        void LoadBinaryReplay(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs);

            int magic = br.ReadInt32();
            if (magic != 0x5245504C) { Debug.LogError("Invalid replay file"); return; }
            _ = br.ReadInt32(); // version
            int entityCount = br.ReadInt32();

            // ensure job sync
            m_em.CompleteAllTrackedJobs();

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            // collect GameObject creations for linking after playback
            var createdGameObjects = new List<(int, GameObject)>();

            for (int rec = 0; rec < entityCount; rec++)
            {
                _ = br.ReadInt32(); // exportedIndex, ignored

                // choose prefab gameobject if requested
                GameObject go = null;
                if (instantiateFromPrefab)
                {
                    if (prefabsByRecord != null && prefabsByRecord.Length > rec) go = prefabsByRecord[rec];
                    if (go == null) go = defaultPrefab;
                }

                Entity ent = ecb.CreateEntity();
                // add tag and meta
                ecb.AddComponent(ent, new ReplayRecordTag());
                ecb.AddComponent(ent, new TrafficReplayStreamDataComponent());
                // store replay instance id on entity immediately so we can map after Playback
                ecb.AddComponent(ent, new ReplayInstanceId { id = rec });
                // create LocalTransform so playback can update it
                ecb.AddComponent(ent, new LocalTransform { Position = float3.zero, Rotation = quaternion.identity, Scale = 1f });
                // add buffers
                ecb.AddBuffer<Float3Key>(ent);
                ecb.AddBuffer<QuatKey>(ent);
                ecb.AddBuffer<FloatKey>(ent);

                // if requested, instantiate visual GameObject (not linked to entity automatically)
                if (go != null)
                {
                    var goInst = GameObject.Instantiate(go);
                    // attach helper to link after playback
                    var link = goInst.AddComponent<EntityGameObjectLink>();
                    link.replayInstanceId = rec;
                    // store mapping of (rec id) -> GameObject to wire up after playback
                    createdGameObjects.Add((rec, goInst));
                    // register in global map
                    GameObjectEntityRegistry.Register(rec, goInst);
                }

                // read streams into temp arrays
                int posCount = br.ReadInt32();
                var posKeys = new Float3Key[posCount];
                for (int i = 0; i < posCount; i++) posKeys[i] = new Float3Key { Time = br.ReadSingle(), Value = new float3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()), Interp = (byte)InterpType.Linear };

                int rotCount = br.ReadInt32();
                var rotKeys = new QuatKey[rotCount];
                for (int i = 0; i < rotCount; i++) rotKeys[i] = new QuatKey { Time = br.ReadSingle(), Value = new quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()), Interp = (byte)InterpType.Linear };

                int speedCount = br.ReadInt32();
                var speedKeys = new FloatKey[speedCount];
                for (int i = 0; i < speedCount; i++) speedKeys[i] = new FloatKey { Time = br.ReadSingle(), Value = br.ReadSingle(), Interp = (byte)InterpType.Linear };

                // normalize times so playback starts at 0
                float baseTime = float.MaxValue;
                if (posKeys.Length > 0) baseTime = math.min(baseTime, posKeys[0].Time);
                if (rotKeys.Length > 0) baseTime = math.min(baseTime, rotKeys[0].Time);
                if (speedKeys.Length > 0) baseTime = math.min(baseTime, speedKeys[0].Time);
                if (baseTime == float.MaxValue) baseTime = 0f;

                for (int i = 0; i < posKeys.Length; i++) { posKeys[i].Time -= baseTime; ecb.AppendToBuffer(ent, posKeys[i]); }
                for (int i = 0; i < rotKeys.Length; i++) { rotKeys[i].Time -= baseTime; ecb.AppendToBuffer(ent, rotKeys[i]); }
                for (int i = 0; i < speedKeys.Length; i++) { speedKeys[i].Time -= baseTime; ecb.AppendToBuffer(ent, speedKeys[i]); }
            }

            ecb.Playback(m_em);
            ecb.Dispose();

            // After playback, build mapping id -> entity and link GameObjects
            var qIds = m_em.CreateEntityQuery(ComponentType.ReadOnly<ReplayInstanceId>());
            using var entsArr = qIds.ToEntityArray(Unity.Collections.Allocator.Temp);
            using var idsArr = qIds.ToComponentDataArray<ReplayInstanceId>(Unity.Collections.Allocator.Temp);
            var entityById = new Dictionary<int, Entity>(entsArr.Length);
            for (int i = 0; i < entsArr.Length; ++i)
            {
                entityById[idsArr[i].id] = entsArr[i];
            }

            for (int i = 0; i < createdGameObjects.Count; ++i)
            {
                var recId = createdGameObjects[i].Item1;
                var goObj = createdGameObjects[i].Item2;
                if (entityById.TryGetValue(recId, out var ent))
                {
                    // optional: set link component's linkedEntity immediately
                    var linkComp = goObj.GetComponent<EntityGameObjectLink>();
                    if (linkComp != null)
                    {
                        linkComp.linkedEntity = ent;
                        linkComp.replayInstanceId = recId;
                    }
                }
                else
                {
                    Debug.LogWarning($"ReplayPlaybackLoader: couldn't find entity for recId {recId}");
                }
            }
            createdGameObjects.Clear();

            // debug summary
            var q = m_em.CreateEntityQuery(ComponentType.ReadOnly<ReplayRecordTag>());
            using var arr = q.ToEntityArray(Unity.Collections.Allocator.Temp);
            Debug.Log($"Replay loaded: total entities with ReplayRecordTag = {arr.Length}");
            if (arr.Length > 0)
            {
                var first = arr[0];
                if (m_em.HasComponent<Float3Key>(first)) { var buf = m_em.GetBuffer<Float3Key>(first); Debug.Log($"First pos keys={buf.Length}"); if (buf.Length>0) Debug.Log($"first pos t={buf[0].Time}"); }
            }

            Debug.Log($"Loaded binary replay {path} with {entityCount} records");
        }
    }
}
