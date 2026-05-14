using System.Collections.Generic;
using _Code.Passive;
using _Code.UnitSystem;
using Code.Core;
using Code.Items;
using UnityEngine;

namespace _Code.KMJ.UnitSystem
{
    public class PassiveStorage : MonoSingleton<PassiveStorage>
    {
        private Dictionary<UnitType, List<PassiveSO>> items 
            = new Dictionary<UnitType, List<PassiveSO>>();

        public void SetPassive(UnitType unitType, PassiveSO itemSO)
        {
            if (items.TryGetValue(unitType, out var itemList))
            {
                if (!itemList.Contains(itemSO))
                    itemList.Add(itemSO);
            }
            else
            {
                var newList = new List<PassiveSO>
                {
                    itemSO
                };
                items.Add(unitType, newList);
            }
        }

        public void RemovePassive(UnitType unitType, PassiveSO itemSO)
        {
            if (items.TryGetValue(unitType, out var itemList))
                if (itemList.Contains(itemSO))
                    itemList.Remove(itemSO);
        }

        public List<PassiveSO> GetPassive(UnitType unitType)
            => items.GetValueOrDefault(unitType);
    }
}