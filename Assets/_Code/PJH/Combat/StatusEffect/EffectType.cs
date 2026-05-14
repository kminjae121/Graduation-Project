using System;

namespace Code.Combat.StatusEffect
{
    [Flags]
    public enum EffectType
    {
        None = 0,
        Burn = 1 << 1,
        Poison = 1 << 2,
        Stun = 1 << 3
    }
}