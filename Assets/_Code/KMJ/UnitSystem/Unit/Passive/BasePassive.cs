using Code.UnitSystem;
using UnityEngine;

namespace _Code.Passive
{
    public abstract class BasePassive : MonoBehaviour
    {
        protected Unit _unit;
        
        public abstract void StartPassive();
        public abstract void StopPassive();

        public void SetOwner(Unit owner)
        {
            _unit = owner;
        }
        
        public virtual void HandleTurnStartEvent()
        {   
        }
    }
}