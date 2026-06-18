using System;
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
        private bool _skipCurrentOwnerTurnEnd;
        private Action<Unit> _frontGuardHitAction;

        public bool IsFrontGuard => _isFrontGuard;
        
        public void Initialize(Unit owner)
        {
            if (_owner != null)
                _owner.OnDeathEvent -= HandleOwnerDeath;

            _owner = owner;
            _healthCompo = owner.GetUnitCompo<UnitHealth>();
            _vfxCompo = owner.GetUnitCompo<UnitVFXCompo>();   

            _owner.OnDeathEvent -= HandleOwnerDeath;
            _owner.OnDeathEvent += HandleOwnerDeath;

            Injector.InjectInto(this);
        }

        private void OnDestroy()
        {
            if (_owner != null)
                _owner.OnDeathEvent -= HandleOwnerDeath;

            if(_turnManager != null)
                _turnManager.OnTurnStart -= CheckInvincibility;

            ClearFrontGuard();
        }

        public void SetUnitInvincibility(int maxTurn)
        {
            _maxTurnCnt = maxTurn;
            _curTurnCnt = 0;
            _healthCompo.IsInvincibility = true;

            PlayEffect();

            if (_turnManager == null)
                return;

            _turnManager.OnTurnStart -= CheckInvincibility;
            _turnManager.OnTurnStart += CheckInvincibility;
        }

        public void SetFrontGuard(int maxTurn, Transform guardTrm, float damageRate = 0f,
            float frontAngle = 120f, Action<Unit> frontGuardHitAction = null)
        {
            ClearFrontGuard();

            _frontGuardMaxTurnCnt = Mathf.Max(1, maxTurn);
            _frontGuardTurnCnt = 0;
            _frontGuardTrm = guardTrm != null ? guardTrm : _owner.transform;
            _frontGuardDamageRate = Mathf.Clamp01(damageRate);
            _frontGuardDot = Mathf.Cos(Mathf.Clamp(frontAngle, 0f, 180f) * 0.5f * Mathf.Deg2Rad);
            _isFrontGuard = true;
            _skipCurrentOwnerTurnEnd = true;
            _frontGuardHitAction = frontGuardHitAction;

            _attackApplyCompo = AttackApplyCompo.Instance;

            if (_attackApplyCompo != null)
            {
                _attackApplyCompo.AttackStartEvent -= GuardFrontDamage;
                _attackApplyCompo.AttackStartEvent += GuardFrontDamage;
            }

            Bus<UnitTurnEndEvent>.Unsubscribe(CheckFrontGuard);
            Bus<UnitTurnEndEvent>.Subscribe(CheckFrontGuard);

            PlayEffect();
        }

        private void CheckInvincibility()
        {
            if (_maxTurnCnt <= _curTurnCnt)
            {
                _turnManager.OnTurnStart -= CheckInvincibility;
                _healthCompo.IsInvincibility = false;
                _curTurnCnt = 0;
                StopEffect();
                return;
            }
            
            _healthCompo.IsInvincibility = true;
            _curTurnCnt += 1;
        }

        private void CheckFrontGuard(UnitTurnEndEvent evt)
        {
            if (!_isFrontGuard || !ReferenceEquals(evt.Unit, _owner))
                return;

            if (_skipCurrentOwnerTurnEnd)
            {
                _skipCurrentOwnerTurnEnd = false;
                return;
            }

            _frontGuardTurnCnt += 1;

            if (_frontGuardTurnCnt >= _frontGuardMaxTurnCnt)
            {
                ClearFrontGuard();
                return;
            }
        }

        private void GuardFrontDamage(ref DamageEvent evt, ref bool isCritical, ref bool isPenetrate)
        {
            if (!_isFrontGuard || _owner == null || evt.target != _owner.gameObject || evt.Owner == null)
                return;

            if (_healthCompo == null || _healthCompo.IsDead)
                return;

            if (!IsFrontAttack(evt.Owner.transform.position))
                return;

            evt.DamageData.damage = Mathf.RoundToInt(evt.DamageData.damage * _frontGuardDamageRate);
            isCritical = false;
            _frontGuardHitAction?.Invoke(evt.Owner);
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
            _skipCurrentOwnerTurnEnd = false;
            _frontGuardHitAction = null;

            if (_attackApplyCompo != null)
                _attackApplyCompo.AttackStartEvent -= GuardFrontDamage;

            _attackApplyCompo = null;

            Bus<UnitTurnEndEvent>.Unsubscribe(CheckFrontGuard);

            StopEffect();
        }

        private void HandleOwnerDeath()
        {
            if (_turnManager != null)
                _turnManager.OnTurnStart -= CheckInvincibility;

            _curTurnCnt = 0;
            _maxTurnCnt = 0;

            if (_healthCompo != null)
                _healthCompo.IsInvincibility = false;

            ClearFrontGuard();
            StopEffect();
        }

        private void PlayEffect()
        {
            if (!string.IsNullOrWhiteSpace(effectName))
                _vfxCompo?.PlayVFX(effectName, _owner.transform.position, Quaternion.identity);
        }

        private void StopEffect()
        {
            if (!string.IsNullOrWhiteSpace(effectName))
                _vfxCompo?.StopVFX(effectName);
        }
    }
}
