using System;
using System.Collections.Generic;

namespace Replays
{
    [Serializable]
    public class ExportWrapper
    {
        public string timestamp;
        public ExportEntityRecord[] records;
    }

    [Serializable]
    public class ExportEntityRecord
    {
        public int entity;
        public List<ExportFloat3Key> positions;
        public List<ExportQuatKey> rotations;
        public List<ExportFloatKey> speeds;
    }

    [Serializable]
    public class ExportFloat3Key { public float t; public float x; public float y; public float z; }
    [Serializable]
    public class ExportQuatKey { public float t; public float x; public float y; public float z; public float w; }
    [Serializable]
    public class ExportFloatKey { public float t; public float v; }
}

