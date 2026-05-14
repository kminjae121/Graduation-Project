using System.Collections.Generic;
using _Code.Passive;
using Code.UnitSystem;
using NUnit.Framework;
using UnityEngine;

namespace Code.Items
{
    public enum ArtifactRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    [System.Serializable]
    public struct ArtifactStat
    {
        public StatInfo StatInfo;

        public float StatValue;
    }

    [CreateAssetMenu(fileName = "ArtifactSO", menuName = "SO/ArtifactSystem/ArtifactSO")]
    public class EquipmentItemSO : ItemSO
    {
        public ArtifactRarity rarity;

        public List<ArtifactStat> Stats = new();

        public PassiveSO PassiveSO;
    }
}