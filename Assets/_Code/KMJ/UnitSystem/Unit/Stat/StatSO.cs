using System;
using System.Collections.Generic;
using UnityEngine;

namespace EntityComponent
{
    [CreateAssetMenu(fileName = "Stat", menuName = "SO/UnitSO/UnitStat", order = 0)]
    public class StatSO : ScriptableObject, ICloneable
    {
        [SerializeField] private Sprite icon;
        [SerializeField] private string displayName;
        [SerializeField] private float baseValue, minValue, maxValue;
        
        public delegate void ValueChangeHandler(StatSO stat, float currentValue, float previousValue);
        public event ValueChangeHandler OnValueChanged;

        public string statName;
        public string description;

        public float incrementStep = 1f;

        private Dictionary<object, float> _modifyValueByKey = new Dictionary<object, float>();
        
        [field: SerializeField] public bool IsPercent { get; private set; }

        private float _modifiedValue = 0;
        public Sprite Icon => icon;

        public float MaxValue
        {
            get => maxValue;
            set => maxValue = value;
        }

        public float MinValue
        {
            get => minValue;
            set => minValue = value;
        }
        
        public float Value => Mathf.Clamp(baseValue + _modifiedValue, MinValue, MaxValue);
        public bool IsMax => Mathf.Approximately(Value, maxValue);
        public bool IsMin => Mathf.Approximately(Value, minValue);
        public bool CanIncrementStep() => BaseValue + incrementStep <= MaxValue;
        
        public float BaseValue
        {
            get => baseValue;
            set
            {
                float prevValue = Value;
                baseValue = Mathf.Clamp(value, MinValue, MaxValue);
                TryInvokeValueChangeEvent(Value, prevValue);
            }
        }

        private void TryInvokeValueChangeEvent(float value, float prevValue)
        {
            if (Mathf.Approximately(value, prevValue) == false)
            {
                OnValueChanged?.Invoke(this, value, prevValue);
            }
        }
        
        //힘 스탯이 몇이냐? 10 + 4 + 2 + 5

        public void AddModifier(object key, float value)
        {
            if (_modifyValueByKey.ContainsKey(key)) return; //동일아이템에 대한 중복적용은 허용하지 않는다.

            float prevValue = Value;
            _modifiedValue += value;
            _modifyValueByKey.Add(key, value);
            TryInvokeValueChangeEvent(Value, prevValue);
        }

        public void RemoveModifier(object key)
        {
            if (_modifyValueByKey.TryGetValue(key, out float value))
            {
                float prevValue = Value;
                _modifiedValue -= value;
                _modifyValueByKey.Remove(key);
                
                TryInvokeValueChangeEvent(Value, prevValue);
            }
        }

        public void ClearModifier()
        {
            float prevValue = Value;
            _modifyValueByKey.Clear();
            _modifiedValue = 0;
            TryInvokeValueChangeEvent(Value, prevValue);
        }

        public object Clone()
        {
            return Instantiate(this); //자기자신을 복제해서 준다.
        }
    }
}
