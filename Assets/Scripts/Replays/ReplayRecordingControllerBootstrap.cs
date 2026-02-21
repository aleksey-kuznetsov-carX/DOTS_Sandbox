using Unity.Entities;
using UnityEngine;

namespace Replays
{
    // Simple MonoBehaviour to create the singleton ReplayRecordingController and toggle recording using R key.
    public class ReplayRecordingControllerBootstrap : MonoBehaviour
    {
        EntityManager m_em;
        Entity m_singleton;

        void Start()
        {
            m_em = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Try to find existing singleton entity with ReplayRecordingController
            var query = m_em.CreateEntityQuery(ComponentType.ReadOnly<ReplayRecordingController>());
            if (query.IsEmptyIgnoreFilter)
            {
                m_singleton = m_em.CreateEntity(typeof(ReplayRecordingController));
                m_em.SetComponentData(m_singleton, new ReplayRecordingController { isRecording = 0, exportOnStop = 1});
            }
            else
            {
                using (var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp))
                {
                    m_singleton = entities[0];
                }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                var ctrl = m_em.GetComponentData<ReplayRecordingController>(m_singleton);
                ctrl.isRecording = (byte)(ctrl.isRecording == 1 ? 0 : 1);
                m_em.SetComponentData(m_singleton, ctrl);
                Debug.Log($"Replay recording {(ctrl.isRecording == 1 ? "started" : "stopped")}");
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                var q = em.CreateEntityQuery(typeof(ReplayPlaybackController));
                
                Entity playSingleton = q.IsEmptyIgnoreFilter ?
                    em.CreateEntity(typeof(ReplayPlaybackController)) :
                    q.GetSingletonEntity();
                
                em.SetComponentData(playSingleton, new ReplayPlaybackController
                {
                    isPlaying = 1, playbackSpeed = 1f, playbackTime = 0f, loop = 1
                });
                
                Debug.Log($"Playback started");
            }
            
        }

        public void StartRecording()
        {
            m_em.SetComponentData(m_singleton, new ReplayRecordingController { isRecording = 1 });
        }

        public void StopRecording()
        {
            m_em.SetComponentData(m_singleton, new ReplayRecordingController { isRecording = 0 });
        }
    }
}
