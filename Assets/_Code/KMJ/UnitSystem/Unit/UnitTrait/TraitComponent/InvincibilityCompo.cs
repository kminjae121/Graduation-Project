using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.Effects;
using Code.UnitSystem.Combat;
using GondrLib.Dependencies;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class InvincibilityCompo : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private string effectName;
        [Inject] protected TurnManager _turnManager;
        
        private UnitVFXCompo _vfxCompo;
        private UnitHealth _healthCompo;
        private Unit _owner;
        
        private int _curTurnCnt;
        private int _maxTurnCnt;
        private int _frontGuardTurnCnt;
        private int _frontGuardMaxTurnCnt;

        private bool _isFrontGuard;
        private float _frontGuardDamageRate;
        private float _frontGuardDot;
        private Transform _frontGuardTrm;
        private AttackApplyCompo _attackApplyCompo;
        
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

            ClearFrontGuard();
        }

        public void SetUnitInvincibility(int maxTurn)
        {
            _maxTurnCnt = maxTurn;
            _curTurnCnt = 0;
            _healthCompo.IsInvincibility = true;
            
            _vfxCompo?.PlayVFX(effectName, _owner.transform.position, Quaternion.identity);

            if (_turnManager == null)
                return;

            _turnManager.OnTurnStart -= CheckInvincibility;
            _turnManager.OnTurnStart += CheckInvincibility;
        }

        public void SetFrontGuard(int maxTurn, Transform guardTrm, float damageRate = 0f, float frontAngle = 120f)
        {
            _frontGuardMaxTurnCnt = Mathf.Max(1, maxTurn);
            _frontGuardTurnCnt = 0;
            _frontGuardTrm = guardTrm != null ? guardTrm : _owner.transform;
            _frontGuardDamageRate = Mathf.Clamp01(damageRate);
            _frontGuardDot = Mathf.Cos(Mathf.Clamp(frontAngle, 0f, 180f) * 0.5f * Mathf.Deg2Rad);
            _isFrontGuard = true;

            _vfxCompo?.PlayVFX(effectName, _owner.transform.position, Quaternion.identity);

            _attackApplyCompo = AttackApplyCompo.Instance;

            if (_attackApplyCompo != null)
            {
                _attackApplyCompo.AttackStartEvent -= GuardFrontDamage;
                _attackApplyCompo.AttackStartEvent += GuardFrontDamage;
            }

            if (_turnManager == null)
                return;

            _turnManager.OnTurnStart -= CheckFrontGuard;
            _turnManager.OnTurnStart += CheckFrontGuard;
        }

        private void CheckInvincibility()
        {
            if (_maxTurnCnt <= _curTurnCnt)
            {
                _turnManager.OnTurnStart -= CheckInvincibility;
                _healthCompo.IsInvincibility = false;
                _curTurnCnt = 0;
                _vfxCompo?.StopVFX(effectName);
                return;
            }
            
            _healthCompo.IsInvincibility = true;
            _curTurnCnt += 1;
        }

        private void CheckFrontGuard()
        {
            if (_frontGuardMaxTurnCnt <= _frontGuardTurnCnt)
            {
                ClearFrontGuard();
                return;
            }

            _frontGuardTurnCnt += 1;
        }

        private void GuardFrontDamage(ref DamageEvent evt, ref bool isCritical, ref bool isPenetrate)
        {
            if (!_isFrontGuard || evt.target != _owner.gameObject || evt.Owner == null)
                return;

            if (!IsFrontAttack(evt.Owner.transform.position))
                return;

            evt.DamageData.damage = Mathf.RoundToInt(evt.DamageData.damage * _frontGuardDamageRate);
            isCritical = false;
        }

        private bool IsFrontAttack(Vector3 attackerPos)
        {
            Vector3 toAttacker = attackerPos - _frontGuardTrm.position;
            toAttacker.y = 0f;

            if (toAttacker.sqrMagnitude <= 0.001f)
                return true;

            Vector3 forward = _frontGuardTrm.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude <= 0.001f)
                forward = _owner.transform.forward;

            return Vector3.Dot(forward.normalized, toAttacker.normalized) >= _frontGuardDot;
        }

        private void ClearFrontGuard()
        {
            _isFrontGuard = false;
            _frontGuardTurnCnt = 0;
            _frontGuardMaxTurnCnt = 0;
            _frontGuardTrm = null;

            if (_attackApplyCompo != null)
                _attackApplyCompo.AttackStartEvent -= GuardFrontDamage;

            _attackApplyCompo = null;

            if (_turnManager != null)
                _turnManager.OnTurnStart -= CheckFrontGuard;

            _vfxCompo?.StopVFX(effectName);
        }
    }
}
