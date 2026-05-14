using System;
using Code.Core;
using Code.Core.Debugs;
using UnityEngine;

namespace Code.UnitSystem
{
    [Serializable]
    public class UnitState
    {
        [field: SerializeField] public UnitSO Data { get; private set; }
        
        public NotifyValue<float> CurrentHp { get; private set; }
        public bool IsDead => CurrentHp.Value <= 0;
        
        public float MaxHealth { get; private set; }

        public UnitState(UnitSO data)
        {
            if (data == null)
            {
                UnityLogger.LogError("유닛 SO가 null 입니다.");
                return;
            }
            
            Data = data;
            MaxHealth = Data.Maxhealth;
            CurrentHp = new NotifyValue<float>(Data.Maxhealth);
        }
        
        public void TakeDamage(float value)
        {
            if (IsDead)
                return;

            CurrentHp.Value = Mathf.Max(0, CurrentHp.Value - value);
        }

        public void Heal(float value)
        {
            if (IsDead)
                return;
            
            CurrentHp.Value = Mathf.Min(Data.Maxhealth, CurrentHp.Value + value);
        }
    }
}