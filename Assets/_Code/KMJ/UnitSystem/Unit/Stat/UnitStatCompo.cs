using _Code.UnitSystem;
using UnityEngine;

namespace Code.UnitSystem
{
    public enum StatInfo
    {
        MoveRange, 
        AtkDamage,
        MaxHealth,
        DefensivePower,
        AvoidProbability,
        CriticalProbability,
        CriticalIncreaseValue,
    }
    public class UnitStatCompo : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private UnitSO unitSO;

        private int MoveRange => unitSO.MoveRange;

        private int MaxHealth => unitSO.Maxhealth;
        
        private int AttackDamage => unitSO.AttackDamage;
        
        private int DefensivePower => unitSO.DefensivePower;
        
        private int AvoidProbability => unitSO.AvoidProbability;

        private int CriticalProbability => unitSO.CriticalProbability;

        private int CriticalIncreaseValue => unitSO.CriticalDamageIncrease;

        public void Initialize(Unit owner)
        {
            if(unitSO == null)
                unitSO = owner.unitSO;
        }

        public int GetStat(StatInfo statInfo)
        {
            int value = 0;
            
            switch (statInfo)
            {
                case StatInfo.MoveRange:
                    value = MoveRange;
                    break;
                case StatInfo.MaxHealth:
                    value = MaxHealth;
                    break;
                case StatInfo.AtkDamage:
                    value = AttackDamage;
                    break;
                case StatInfo.DefensivePower:
                    value = DefensivePower;
                    break;
                case StatInfo.AvoidProbability:
                    value = AvoidProbability;
                    break;
                case  StatInfo.CriticalProbability:
                    value = CriticalProbability;
                    break;
                case StatInfo.CriticalIncreaseValue:
                    value = CriticalIncreaseValue;
                    break;
            }
            
            value += InGameStatCompo.Instance.GetStatToInt(statInfo, unitSO.UnitType);
            
            return value;
        }
    }
}