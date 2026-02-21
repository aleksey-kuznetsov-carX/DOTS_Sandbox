using NUnit.Framework;
using Replays;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tests.Editor
{
    public class ReplayTests
    {
        World m_testWorld;
        EntityManager m_em;

        [SetUp]
        public void SetUp()
        {
            m_testWorld = new World("TestWorld");
            m_em = m_testWorld.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
            m_testWorld.Dispose();
        }

        [Test]
        public void LinearInterpolationFloat3_Works()
        {
            var e = m_em.CreateEntity();
            var buf = m_em.AddBuffer<Float3Key>(e);
            ReplayBufferUtils.AddKey(ref buf, 0f, new float3(0f, 0f, 0f));
            ReplayBufferUtils.AddKey(ref buf, 1f, new float3(10f, 0f, 0f));

            int idx = 0;
            var sampled = ReplayBufferUtils.SampleFloat3(buf, 0.5f, ref idx, new float3(0f));
            Assert.AreEqual(5f, sampled.x, 1e-4f);
        }

        [Test]
        public void RecordingToggle_CreatesBuffers_WhenActive()
        {
            // create singleton controller
            var singleton = m_em.CreateEntity(typeof(ReplayRecordingController));
            m_em.SetComponentData(singleton, new ReplayRecordingController { isRecording = 1, sampleInterval = 0f, lastRecordTime = 0f, maxKeys = 0, exportOnStop = 0 });

            // create an entity to record
            var ent = m_em.CreateEntity();
            m_em.AddComponentData(ent, new LocalToWorld { Value = float4x4.identity });
            m_em.AddComponentData(ent, new global::Replays.ReplayRecordTag());

            // run the recorder system
            var sys = m_testWorld.GetOrCreateSystemManaged<DynamicReplayRecorderSystem>();
            sys.Update(); // should create buffers and write one sample

            Assert.IsTrue(m_em.HasComponent<Float3Key>(ent));
            var buf = m_em.GetBuffer<Float3Key>(ent);
            Assert.IsTrue(buf.Length > 0);
        }
    }
}
