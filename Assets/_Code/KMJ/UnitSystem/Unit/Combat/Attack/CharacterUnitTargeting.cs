using Code.Core.Events.Bus;
using Code.SkillSystem;
using EnemySystem;
using Input;
using UnityEngine;

namespace Code.UnitSystem.Combat
{
    public class CharacterUnitTargeting : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private InputReader inputSO;
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
            
            EnemyInfoTargeting();
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
                        _targetEnemy.GetComponent<Unit>().unitSO.UnitImage, true));
                    
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
                    skill.RotatorCompo.SetDir(enemy.transform.position);
                    skill.SetAddDamage(skill.CriticalSpot.CheckEnemyBody(skill.DamageData, enemy, skill.Damage));
                }
                
                UnitHealth health = enemy.GetComponent<UnitHealth>();
                _targetingCompo = enemy.GetComponent<EnemyTargeting>();
                _targetOutLineCompo = _targetEnemy.GetComponent<UnitOutLineCompo>();
                
                if(_targetOutLineCompo != null)
                    _targetOutLineCompo.SetOutLine();
                if (_targetingCompo != null)
                    _targetingCompo.Targeting();
                
                Bus<EnemyHpInfo>.Raise(new EnemyHpInfo(skillManager.GetSkillInfo().AddDamage, health.CurrentHealth,
                    health.MaxHealth,
                    skillManager.GetSkillInfo().DamageData.damage, true,
                    enemy.GetComponent<Unit>().unitSO.UnitImage, true));
                
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

                UnitHealth health = _targetEnemy.GetComponent<UnitHealth>();
                Unit unit = _targetEnemy.GetComponent<Unit>();

                Sprite img = (unit != null && unit.unitSO != null) ? unit.unitSO.UnitImage : null;

                float currentHp = health != null ? health.CurrentHealth : 0;
                float maxHp = health != null ? health.MaxHealth : 0;

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
                

                Sprite img = null;

                var unit = _targetEnemy.GetComponent<Unit>();

                if (unit != null && unit.unitSO != null)
                    img = unit.unitSO.UnitImage;

                Bus<EnemyHpInfo>.Raise(new EnemyHpInfo(0, 0, 0, 0, false, img, false, 0));
            }

            _targetEnemy = null;
            _targetingCompo = null;
        }
        
        
    }
}
