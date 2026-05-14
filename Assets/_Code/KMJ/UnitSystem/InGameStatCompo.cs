using System.Collections.Generic;
using Code.Core;
using Code.UnitSystem;
using UnityEngine;


namespace _Code.UnitSystem
{
    public struct InGameStat
    {
        public UnitType UnitType;
        public StatInfo UpgradeStat;
        public float UpgradeValue;
    }

    public class InGameStatCompo : MonoSingleton<InGameStatCompo>
    {
        private List<InGameStat> _statsList = new List<InGameStat>();

        public void ReStartGame() =>
            _statsList.Clear();

        public void SetStat(StatInfo upgradeStat, float upgradeValue, UnitType unitType)
        {
            for (int i = 0; i < _statsList.Count; i++)
            {
                var item = _statsList[i];
                if (item.UpgradeStat == upgradeStat && item.UnitType == unitType)
                {
                    item.UpgradeValue += upgradeValue;
                    _statsList[i] = item;
                    return;
                }
            }

            _statsList.Add(new InGameStat
            {
                UnitType = unitType,
                UpgradeStat = upgradeStat,
                UpgradeValue = upgradeValue
            });
        }

        public float GetStat(StatInfo statInfo, UnitType unitType)
        {
            foreach (var stat in _statsList)
                if (stat.UnitType == unitType && stat.UpgradeStat == statInfo)
                    return stat.UpgradeValue;

            return 0f;
        }

        public int GetStatToInt(StatInfo statInfo, UnitType unitType)
        {
            foreach (var stat in _statsList)
                if (stat.UnitType == unitType && stat.UpgradeStat == statInfo)
                    return (int)stat.UpgradeValue;

            return 0;
        }
    }
}