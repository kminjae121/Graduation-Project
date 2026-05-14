using Code.Core.Managers;
using Code.Managers;
using Code.SkillSystem;
using UnityEngine;

namespace Code.UI.SkillTreeUI
{
    public class SkillNode : MonoBehaviour, INode
    {
        [SerializeField] private UnitType unitType;

        [SerializeField] private SkillSO skillSO;

        [SerializeField] private int nodePrice;
       
        public void UseNode()
        {
            if (nodePrice > PlayerManager.Instance.Gold)
                return;
            
            PlayerManager.Instance.RemoveGold(nodePrice);
            SkillSendManager.Instance.AddSkillList(skillSO);
        }
    }
}