using Code.Core.Managers;
using Code.Effects;
using Code.UnitSystem.Combat;
using GondrLib.Dependencies;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class InvincibilityCompo : MonoBehaviour, IUnitComponent
    {
        [Inject] protected TurnManager _turnManager;
        private UnitVFXCompo _vfxCompo;
        
        private UnitHealth _healthCompo;
        private Unit _owner;
        
        private int _curTurnCnt;
        private int _maxTurnCnt;
        
        public void Initialize(Unit owner)
        {
            _owner = owner;
            _healthCompo = owner.GetUnitCompo<UnitHealth>();
            _vfxCompo = owner.GetUnitCompo<UnitVFXCompo>();   
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
            
            _vfxCompo.PlayVFX("SunShield", _owner.transform.position, Quaternion.identity);
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
                _vfxCompo.StopVFX("SunShield");
                return;
            }
            
            _healthCompo.IsInvincibility = true;
            _curTurnCnt += 1;
        }
    }
}