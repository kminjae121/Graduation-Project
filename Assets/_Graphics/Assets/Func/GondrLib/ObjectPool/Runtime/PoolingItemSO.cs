using UnityEngine;

namespace GondrLib.ObjectPool.Runtime
{
    [CreateAssetMenu(fileName = "PoolingItem", menuName = "SO/Pool/Item", order = 0)]
    public class PoolingItemSO : ScriptableObject
    {
        public string poolingName;
        public GameObject prefab;
        public int initCount;
    }
}