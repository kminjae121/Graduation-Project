using Code.Combat.StatusEffect;
using Code.UnitSystem;

namespace Code.Core.Events.Bus
{
    public struct ApplyStatusEffectEvent : IEvent
    {
        public Unit Target { get; private set; }
        public EffectType EffectType { get; private set; }
        public StatusEffectApplyData ApplyData { get; private set; }
        
        public ApplyStatusEffectEvent(Unit target, EffectType effectType, StatusEffectApplyData applyData)
        {
            Target = target;
            EffectType = effectType;
            ApplyData = applyData;
        }
    }
}