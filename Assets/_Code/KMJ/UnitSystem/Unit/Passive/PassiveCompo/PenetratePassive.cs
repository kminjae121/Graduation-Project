using System;
using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using UnityEngine;

namespace _Code.Passive
{
    public class PenetratePassive : MyTurnPassive
    {
        public override void StartPassive()
        {
            AttackApplyCompo.Instance.AttackStartEvent += PeneratePassive;
        }

        public override void StopPassive()
        {
            AttackApplyCompo.Instance.AttackStartEvent -= PeneratePassive;
        }

        private void PeneratePassive(ref DamageEvent evt, ref bool isCritical, ref bool isPenetrate)
        {
            isPenetrate = true;
        }
    }
}