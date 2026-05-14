using _Code.UnitSystem;
using Code.Managers;
using Code.UnitSystem;
using UnityEngine;

namespace Code.UI.SkillTreeUI
{
    public class StatNode : MonoBehaviour, INode
    {
        [SerializeField] private UnitType unitType;
        [SerializeField] private StatInfo upgradeStat;
        [SerializeField] private float upgradeValue;
        
        
        [SerializeField] private int nodePrice;
        
        public void UseNode()
        {
            if (nodePrice > PlayerManager.Instance.Gold)
                return;
            
            Debug.Log($"{unitType} 의 {upgradeStat}스탯이 {upgradeValue}만큼 증가됐다.");
            
            PlayerManager.Instance.RemoveGold(nodePrice);
            InGameStatCompo.Instance.SetStat(upgradeStat, upgradeValue, unitType);
        }
    }
}