using Code.Core.Events.Bus;
using Code.SkillSystem;
using EnemySystem;
using Input;
using Code.UnitSystem.TraitSystem;
using UnityEngine;

namespace Code.UnitSystem.Combat
{
    public class CharacterUnitTargeting : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private InputReader inputSO;
        [SerializeField] private UnitTrait traitCompo;
        [SerializeField] private UnitMoveCompo moveCompo;
        [SerializeField] private SkillManageComponent skillManager;

        private GameObject _targetEnemy;
        private UnitOutLineCompo _targetOutLineCompo;
        private EnemyTargeting _targetingCompo;
        [SerializeField] private CharacterUnit unit;

        public void Initialize(Unit owner)
        {
            unit = owner as CharacterUnit;
        }

        private void Update() 
        {
            HandleTargeting();
        }

        private void HandleTargeting()
        {
            if (!unit.isMyTurn || inputSO == null)
                return;
            
            if (skillManager.GetSkillInfo() != null && skillManager.GetSkillInfo().IsActive)
            {
                SetSkillTargeting();
                return;
            }
            else if (traitCompo.IsTargeting)
            {
                SetTargetingTraitCompo();
                return;
            }
            
            traitCompo.SetEnemy(null);
            
            EnemyInfoTargeting();
        }

        private void SetTargetingTraitCompo()
        {
            GameObject enemy = inputSO.GetEnemy();
            
            if (enemy == null)
            {
                if (_targetEnemy != null)
                {
                    _targetOutLineCompo = _targetEnemy.GetComponent<UnitOutLineCompo>();
                    
                    if(_targetOutLineCompo != null)
                        _targetOutLineCompo.ResetOutLine();

                    traitCompo.SetEnemy(null);
                    _targetEnemy = null;
                }
            }
            else
            {
                _targetEnemy = enemy;
                
                _targetOutLineCompo = _targetEnemy.GetComponent<UnitOutLineCompo>();
                
                if(_targetOutLineCompo != null)
                    _targetOutLineCompo.SetOutLine();
                
                traitCompo.SetEnemy(_targetEnemy);
            }
        }

        private void EnemyInfoTargeting()
        {
            GameObject enemy = inputSO.GetEnemy();

            if (moveCompo.VisualPrefabs.activeInHierarchy
                || enemy == null && _targetEnemy != null)
                ClearTarget();
            else if (enemy != null)
                SetTarget(enemy);
        }

        private void SetSkillTargeting()
        {
            GameObject enemy = inputSO.GetEnemy();

            if (skillManager.GetSkillInfo().SkillSO.IsOwnSkill)
            {
                Unit unit = inputSO.GetUnit();
                
                CharacterUnit thisUnit = unit as CharacterUnit;

                if (thisUnit == this.unit)
                {
                    skillManager.GetSkillInfo().FindEnemyIsThere(unit.gameObject);
                    this.unit.OutLineCompo.SetOutLine();
                }
                else
                {
                    skillManager.GetSkillInfo().SetEnemy(null);
                    this.unit.OutLineCompo.ResetOutLine();
                }

                return;
            }
            if (enemy == null)
            {
                if (_targetEnemy != null)
                {
                    _targetingCompo = _targetEnemy.GetComponent<EnemyTargeting>();
                    _targetOutLineCompo = _targetEnemy.GetComponent<UnitOutLineCompo>();

                    if (_targetingCompo != null)
                        _targetingCompo.OffTargeting();
                    
                    if(_targetOutLineCompo != null)
                        _targetOutLineCompo.ResetOutLine();

                    Bus<EnemyHpInfo>.Raise(new EnemyHpInfo(0, 0, 0, 0, false,
                        GetTargetIcon(_targetEnemy), true));
                    
                    skillManager.GetSkillInfo().SetEnemyTargeting(null);
                    skillManager.GetSkillInfo().SetEnemy(null);
                }
            }
            else
            {
                skillManager.GetSkillInfo().FindEnemyIsThere(enemy);
                
                if (skillManager.GetSkillInfo().GetEnemy() == null) return;
                
                _targetEnemy = enemy;
                
                var skill = skillManager.GetSkillInfo();
                
                if (skill != null)
                {
                    if (skill.RotatorCompo != null)
                        skill.RotatorCompo.SetDir(enemy.transform.position);

                    if (skill.CriticalSpot != null)
                        skill.SetAddDamage(skill.CriticalSpot.CheckEnemyBody(skill.DamageData, enemy, skill.Damage));
                    else
                        skill.SetAddDamage(0);
                }
                
                _targetingCompo = enemy.GetComponent<EnemyTargeting>();
                _targetOutLineCompo = _targetEnemy.GetComponent<UnitOutLineCompo>();
                
                if(_targetOutLineCompo != null)
                    _targetOutLineCompo.SetOutLine();
                if (_targetingCompo != null)
                    _targetingCompo.Targeting();
                
                TryGetTargetHealthInfo(enemy, out float currentHp, out float maxHp, out Sprite icon);

                Bus<EnemyHpInfo>.Raise(new EnemyHpInfo(skillManager.GetSkillInfo().AddDamage, currentHp,
                    maxHp,
                    skillManager.GetSkillInfo().DamageData.damage, true,
                    icon, true));
                
                skillManager.GetSkillInfo().SetEnemyTargeting(_targetingCompo);
                skillManager.GetSkillInfo().SetEnemy(_targetEnemy);
            }
        }
        

        private void SetTarget(GameObject enemy)
        {
            _targetEnemy = enemy;

            if (_targetEnemy == null)
                return;

            if (_targetingCompo == null)
            {
                _targetingCompo = _targetEnemy.GetComponent<EnemyTargeting>();

                if (_targetingCompo != null)
                    _targetingCompo.Targeting();

                TryGetTargetHealthInfo(_targetEnemy, out float currentHp, out float maxHp, out Sprite img);

                Bus<EnemyHpInfo>.Raise(new EnemyHpInfo(0, currentHp, maxHp, 0, true, img, false, 3));
            }
        }

        private void ClearTarget()
        {
            if (_targetEnemy != null)
            {
                if (_targetingCompo == null)
                    _targetingCompo = _targetEnemy.GetComponent<EnemyTargeting>();

                if (_targetingCompo != null)
                    _targetingCompo.OffTargeting();
                
                _targetOutLineCompo = _targetEnemy.GetComponent<UnitOutLineCompo>();

                if (_targetOutLineCompo != null)
                    _targetOutLineCompo.ResetOutLine();
                

                Bus<EnemyHpInfo>.Raise(new EnemyHpInfo(0, 0, 0, 0, false,
                    GetTargetIcon(_targetEnemy), false, 0));
            }

            _targetEnemy = null;
            _targetingCompo = null;
        }

        private static bool TryGetTargetHealthInfo(GameObject target, out float currentHp,
            out float maxHp, out Sprite icon)
        {
            currentHp = 0f;
            maxHp = 0f;
            icon = null;

            if (target == null)
                return false;

            UnitHealth health = target.GetComponent<UnitHealth>();
            icon = GetTargetIcon(target);

            if (health == null)
                return false;

            currentHp = health.CurrentHealth;
            maxHp = health.MaxHealth;
            return true;
        }

        private static Sprite GetTargetIcon(GameObject target)
        {
            if (target == null)
                return null;

            Unit targetUnit = target.GetComponent<Unit>();
            return targetUnit != null && targetUnit.unitSO != null
                ? targetUnit.unitSO.UnitImage
                : null;
        }
    }
}
