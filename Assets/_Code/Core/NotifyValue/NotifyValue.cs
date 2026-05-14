using System;
using UnityEngine;

namespace Code.Core
{
    [Serializable]
    public class NotifyValue<T>
    {
        public delegate void ValueChanged(T prev, T next);

        public event ValueChanged OnValueChanged;

        [SerializeField] private T value;

        public T Value
        {
            get => value;

            set
            {
                T before = this.value;
                this.value = value;
                
                if ((before == null && value != null) || !before.Equals(this.value))
                    OnValueChanged?.Invoke(before, this.value);
            }
        }
        
        public NotifyValue()
        {
            value = default;
        }

        public NotifyValue(T value)
        {
            this.value = value;
        }
    }
}