using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct EnemyHpInfo : IEvent
    {
        public bool isActive;

        public bool isAttack;

        public float atkDamage;
        
        public float hp;

        public float damage;

        public float maxHp;

        public float plusDamage;

        public float lastValue;

        public Sprite sprite;
        
        public EnemyHpInfo(float damageInfo, float hp,float maxHp, float damage , bool isActive , Sprite sprite
            , bool isAttack = true, float atkDamage = 0)
        {
            this.plusDamage = damageInfo;
            this.isActive = isActive;
            this.hp = hp;
            this.damage = damage;
            this.maxHp = maxHp;
            this.sprite = sprite;
            this.isAttack = isAttack;
            this.atkDamage = atkDamage;
            
            if (maxHp <= 0f)
            {
                lastValue = 0f; 
                return;
            }
            
            float clampedCurrent = Mathf.Clamp(hp - damage - plusDamage, 0f, maxHp);
            lastValue = clampedCurrent / maxHp; 
        }
    }
}