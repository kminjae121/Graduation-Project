using System.Collections.Generic;
using _Code.KMJ.UnitSystem;
using _Code.Passive;
using _Code.UnitSystem;
using Code.Core;
using Code.Items;
using UnityEngine;

namespace _Code.Item
{
    public class ItemStorage : MonoSingleton<ItemStorage>
    {
        private Dictionary<UnitType, List<EquipmentItemSO>> items 
            = new Dictionary<UnitType, List<EquipmentItemSO>>();
        public Dictionary<UnitType, List<EquipmentItemSO>> GetAllEquippedItems()
        {
            return items;
        }

        public void SetItem(UnitType unitType, EquipmentItemSO itemSO)
        {
            if (items.TryGetValue(unitType, out var itemList))
            {
                if (!itemList.Contains(itemSO))
                {
                    itemList.Add(itemSO);
                    foreach (var statInfo in itemSO.Stats)
                    {
                        InGameStatCompo.Instance.SetStat(statInfo.StatInfo, statInfo.StatValue, unitType);   
                    }
                    if (itemSO.PassiveSO != null)
                        PassiveStorage.Instance.SetPassive(unitType, itemSO.PassiveSO);
                    
                }
            }
            else
            {
                var newList = new List<EquipmentItemSO>
                {
                    itemSO
                };
                items.Add(unitType, newList);
                foreach (var StatInfo in itemSO.Stats)
                {
                    InGameStatCompo.Instance.SetStat(StatInfo.StatInfo, StatInfo.StatValue, unitType);   
                }
                
                if (itemSO.PassiveSO != null)
                    PassiveStorage.Instance.SetPassive(unitType, itemSO.PassiveSO);
            }
        }

        public void RemoveItem(UnitType unitType, EquipmentItemSO itemSO)
        {
            if (items.TryGetValue(unitType, out var itemList))
            {
                if (itemList.Contains(itemSO))
                {
                    itemList.Remove(itemSO);
                    
                    foreach (var statInfo in itemSO.Stats)
                    {
                        InGameStatCompo.Instance.SetStat(statInfo.StatInfo, -statInfo.StatValue, unitType);   
                    }
                    
                    if (itemSO.PassiveSO != null)
                        PassiveStorage.Instance.RemovePassive(unitType, itemSO.PassiveSO);
                }
            }
        }
    }
}