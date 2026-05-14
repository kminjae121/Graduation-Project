using System;

namespace Code.UnitSystem.Combat
{
    [Flags]
    public enum DamageType
    {
        None = 0, MELEE = 1, RANGE = 2, MAGIC = 4
    }
    
    public struct DamageData
    {
        public int damage;
        public bool isCritical;
        public DamageType damageType;
        //데미지에 관련된 모든 것을 저장하는 구조체
    }
}