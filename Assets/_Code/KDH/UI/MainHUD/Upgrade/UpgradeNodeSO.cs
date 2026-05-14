using UnityEngine;

namespace Code.UnitSystem.Upgrade
{
    [CreateAssetMenu(fileName = "UpgradeNode", menuName = "SO/UpgradeSystem/UpgradeNode")]
    public class UpgradeNodeSO : ScriptableObject
    {
        public string upgradeName;
        public Sprite icon;
        
        [TextArea(3, 10)]
        public string description;
        
        public string statOrSkillInfo; 
        
        public int cost;
        public bool isUnlocked;
    }
}