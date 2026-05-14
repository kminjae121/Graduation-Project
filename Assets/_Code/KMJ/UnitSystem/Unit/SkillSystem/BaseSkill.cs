using Code.Core.Events.Bus;
using Code.UnitSystem;
using Code.UnitSystem.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace Code.SkillSystem
{
    public enum SkillType
    {
        BasicSkill,
        ActiveSkill,
    }

    public abstract class BaseSkill : MonoBehaviour
    {
        [Header("Base Settings")] 
        [field: SerializeField] public SkillSO SkillSO { get; private set; }
        [SerializeField] protected AttackDataSO attackData;
        public int BasicSkillDamage => SkillSO.SkillDamage;
        
        public int AddDamage { get; private set; } = 0;
        public float Damage { get; set; }
        protected int SkillRange { get; private set; }

        [Header("Unit Component")] 
        protected SkillComponent _skillCompo;
        [SerializeField] protected UnitAnimationTrigger triggerCompo;
        [SerializeField] private UnitStatCompo statCompo;
        [SerializeField] protected RangeComponent rangeCompo;
        public UnitRotator RotatorCompo { get; set; }
        
        [Header("Skill Event")] 
        public UnityEvent<GameObject> SkillEvent;
        public UnityEvent SkillFeedbackEvent;
        public UnityEvent SkillEndEvent;

        [Header("Camera & Effects")] 
        public DamageData DamageData;

        protected GameObject _targetEnemy = null;
        public bool isCanUseSkill = false;
        public bool IsActive = false;
        public int SkillCount { get; set; } = 0;

        public virtual void InitializeSkill()
        {
            SkillEndEvent.AddListener(CanUseSkillTrue);
            SkillEvent.AddListener(StartSkill);
        }

        public void ConfigureSkillRange(SkillSO skillData)
        {
            SkillRange = skillData == null ? 0 : Mathf.Max(0, Mathf.RoundToInt(skillData.SkillRange));
        }

        protected virtual void OnDestroy()
        {
            SkillEndEvent.RemoveListener(CanUseSkillTrue);
        }

        public void SetDamage(int damage)
        {
            DamageData.damage = damage += AddDamage;
        }

        private void StartSkill(GameObject arg0)
        {
        }

        public void SetAddDamage(int addDamage)
        {
            AddDamage = addDamage;
        }

        protected virtual void StartEvent()
        {
            Bus<SetAtkUIEvent>.Raise(new SetAtkUIEvent(false));
        }

        protected virtual void RemoveEvent()
        {
        }
        
        public void ResetSkillCnt()
        {
            SkillCount = 0;
        }

        protected virtual void CanUseSkillTrue()
        {
        }

        public virtual void ShowSkillRange()
        {
            IsActive = true;
        }

        public virtual void CheckCanAttack()
        {
            Bus<UnitAttackControlEvent>.Raise(new UnitAttackControlEvent(true));
            Bus<UnitMoveControlEvent>.Raise(new UnitMoveControlEvent(true));
            Bus<TurnEndUIEvent>.Raise(new TurnEndUIEvent(true));

            if (SkillSO.IsOwnSkill)
            {
                rangeCompo.FindObjectInRange(0);
                return;
            }
            rangeCompo.FindObjectInRange(SkillSO.SkillRange);
        }

        public virtual void SkillFinished(bool isCancel)
        {
            BooleanSkillUse(false);
            rangeCompo.ResetTile();
            
            Bus<UnitSkilStartEvent>.Raise(new UnitSkilStartEvent(false));
        }

        public virtual void AttackEnemy()
        {
        }

        public virtual void UseSkill()
        {
            if (isCanUseSkill == false)
                return;

            AttackEnemy();
        }

        public void BooleanSkillUse(bool isSkill)
        {
            isCanUseSkill = isSkill;
        }

        public virtual void ForceUseSkill(GameObject target)
        {
            if (target == null) return;

            _targetEnemy = target;
            isCanUseSkill = true;

            if (RotatorCompo != null)
                RotatorCompo.SetDir(target.transform.position);

            StartEvent();
            
            Bus<UnitSkilStartEvent>.Raise(new UnitSkilStartEvent(true));

            SkillEvent?.Invoke(_targetEnemy);
        }

        protected int GetRange() => SkillRange;
    }
}