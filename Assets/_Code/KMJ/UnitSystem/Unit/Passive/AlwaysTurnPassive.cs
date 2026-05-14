using System;
using Code.Managers;
using GondrLib.Dependencies;
using UnityEngine;

namespace _Code.Passive
{
    public class AlwaysTurnPassive : BasePassive
    {
        [Inject] protected TurnManager _turnManager;

        protected virtual void Start()
        {
            Injector.InjectInto(this);
        }

        public override void StartPassive()
        {
            
            
        }

        public override void StopPassive()
        {
            
        }
    }
}