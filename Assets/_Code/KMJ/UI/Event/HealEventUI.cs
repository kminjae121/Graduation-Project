using UnityEngine;
using Event = Unity.Services.Analytics.Event;

namespace Code.UI
{
    public class HealEventUI : EventUI
    {
        protected override void Buff(int randValue)
        {
            storageSO.unitStates.ForEach(state =>
            {
                state.Heal(eventTexts[randValue].value);
            });
        }

        protected override void DeBuff(int randValue)
        {
            storageSO.unitStates.ForEach(state =>
            {
                state.TakeDamage(eventTexts[randValue].value);
            });
        }
    }
}