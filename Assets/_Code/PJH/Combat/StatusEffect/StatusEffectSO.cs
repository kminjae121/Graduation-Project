using UnityEngine;

namespace Code.Combat.StatusEffect
{
    [CreateAssetMenu(fileName = "StatusEffect", menuName = "SO/StatusEffect/Effect", order = 0)]
    public class StatusEffectSO : ScriptableObject
    {
        public string effectName;
        public string description;
        public string className;
        public EffectPolarity polarity;
        public EffectType effectType;
        public EffectTriggerTiming triggerTiming;
        public Sprite effectIcon;
    }
}