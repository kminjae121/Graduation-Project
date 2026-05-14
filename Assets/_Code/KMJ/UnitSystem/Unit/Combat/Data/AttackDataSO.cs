using UnityEngine;

namespace Code.UnitSystem.Combat
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "SO/UnitSO/AttackDataSO", order = 0)]
    public class AttackDataSO : ScriptableObject
    {
        public DamageType damageType = DamageType.MELEE;
        
        public string AttackName;
        public float damageMultiplier = 1f; //증가 뎀
        public float damageIncrease = 0;  //추가 뎀
        public bool isPowerAttack;
        public float impulseForce; //카메라 셰이크 포스
        
        private void OnEnable()
        {
            AttackName = name;
        }
    }
}