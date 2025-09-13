using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(menuName = "Config/Pool Config", fileName = "PoolConfig")]
    public sealed class PoolConfig : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            public string Key;
            public int Min = 8;
            public int Max = 64;
        }

        public List<Entry> Entries = new List<Entry>();

        public bool TryGet(string key, out PoolPolicy policy)
        {
            foreach (var e in Entries)
            {
                if (e != null && e.Key == key)
                {
                    policy = new PoolPolicy(e.Min, e.Max);
                    return true;
                }
            }
            policy = default;
            return false;
        }
    }
}
