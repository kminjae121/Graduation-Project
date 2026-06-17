using UnityEngine;

namespace Code.UI
{
    public class DamageIncreaseEvnetUI : EventUI
    {
        protected override void Buff(int randValue)
        {
            storageSO.unitStates.ForEach(state =>
            {
                //state.Data
            });
        }

        protected override void DeBuff(int randValue)
        {
            storageSO.unitStates.ForEach(state =>
            {
                //state.Data
            });
        }
    }
}