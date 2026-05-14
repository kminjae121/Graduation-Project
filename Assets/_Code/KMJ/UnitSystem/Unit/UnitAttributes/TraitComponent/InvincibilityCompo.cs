using System;
using _Code.UnitSystem;
using Code.Managers;
using Code.UnitSystem.Combat;
using GondrLib.Dependencies;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public class InvincibilityCompo : MonoBehaviour, IUnitComponent
    {
        [Inject] protected TurnManager _turnManager;
        private UnitEffectCompo _effectCompo;
        
        private UnitHealth _healthCompo;
        
        private int _curTurnCnt = 0;
        private int _maxTurnCnt;
        
        public void Initialize(Unit owner)
        {
            _healthCompo = owner.GetUnitCompo<UnitHealth>();
            _effectCompo = owner.GetUnitCompo<UnitEffectCompo>();   
            Injector.InjectInto(this);
        }

        private void OnDestroy()
        {
            if(_turnManager != null)
                _turnManager.OnTurnStart -= CheckInvincibility;
        }

        public void SetUnitInvincibility(int maxTurn)
        {
            _maxTurnCnt = maxTurn;
            _curTurnCnt = 0;
            
            _effectCompo.PlayTargetEffect("SunShield");
            _turnManager.OnTurnStart -= CheckInvincibility;
            _turnManager.OnTurnStart += CheckInvincibility;
        }

        private void CheckInvincibility()
        {
            if (_maxTurnCnt <= _curTurnCnt)
            {
                _turnManager.OnTurnStart -= CheckInvincibility;
                _healthCompo.IsInvincibility = false;
                _curTurnCnt = 0;
                _effectCompo.StopTargetEffect("SunShield");
                return;
            }
            
            _healthCompo.IsInvincibility = true;
            _curTurnCnt += 1;
        }
    }
}