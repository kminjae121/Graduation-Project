using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.UnitSystem
{
    [CreateAssetMenu(fileName = "UnitInfo", menuName = "SO/UnitSO/UnitInfo", order = 0)]
    public class UnitSpawnSO : ScriptableObject
    {
        public string UnitName;
        public GameObject UnitPrefab;
        public PoolingItemSO poolingItem;
        public UnitSO UnitSO;
    }
}