using System.Collections.Generic;
using Code.UnitSystem;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.UnitManaging
{
    public class UnitStorage : MonoBehaviour
    { 
        [SerializeField] private UnitStorageSO _storage;
        
        public Dictionary<string, PoolingItemSO> units = new Dictionary<string, PoolingItemSO>();
        
        public List<PoolingItemSO> unitInfos = new List<PoolingItemSO>();

        private void Awake()
        {
            _storage.units.ForEach(unit =>
            {
                unitInfos.Add(unit.poolingItem);
            });    
        }

        /// <summary>
        /// 유닛을 찾는 함수
        /// </summary>
        /// <param name="unitName">찾을 유닛 이름</param>
        /// <returns></returns>
        public PoolingItemSO GetUnitInfo(string unitName)
        {
            return units.GetValueOrDefault(unitName);
        }
    }
}