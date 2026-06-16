using UnityEngine;

namespace Code.UnitSystem.Combat
{
    public interface ITargetHealthInfo
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        Sprite Icon { get; }
    }
}
