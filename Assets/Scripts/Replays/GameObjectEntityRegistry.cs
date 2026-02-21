using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Replays
{
    // Simple runtime registry to map replay instance id -> GameObject
    public static class GameObjectEntityRegistry
    {
        static ConcurrentDictionary<int, GameObject> map = new ConcurrentDictionary<int, GameObject>();

        public static void Register(int id, GameObject go)
        {
            if (go == null) return;
            map[id] = go;
        }

        public static void Unregister(int id)
        {
            map.TryRemove(id, out _);
        }

        public static bool TryGet(int id, out GameObject go)
        {
            return map.TryGetValue(id, out go);
        }

        public static void Clear()
        {
            map.Clear();
        }

        public static KeyValuePair<int, GameObject>[] GetAllEntries()
        {
            return map.ToArray();
        }
    }
}
