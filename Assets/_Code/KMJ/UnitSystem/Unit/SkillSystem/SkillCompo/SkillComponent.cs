using System;
using System.Collections.Generic;
using System.Linq;
using _Code.KMJ.UnitSystem.involveUnitSO;
using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Code.Core.Managers;
using Code.UnitSystem;
using UnityEngine;

namespace Code.SkillSystem
{
    public abstract class SkillComponent : MonoBehaviour, IUnitComponent
    {
        [SerializeField] protected List<SkillSO> skillList = new List<SkillSO>();
        public Dictionary<SkillSO, BaseSkill> Skills { get; private set; }

        [SerializeField] private UnitSkillStorageSO skillStorage;

        protected UnitStatCompo _statCompo;
        protected Unit _unit;

        protected int basicDamage = 0;
        protected bool isUseSkill = true;

        public void Initialize(Unit owner)
        {
            _unit = owner;
            
            if(skillStorage != null)
            {
                skillList = skillStorage.skills;
            }
            
            if (_unit != null && _statCompo == null)
                _statCompo = _unit.GetUnitCompo<UnitStatCompo>();

            foreach (var skill in SkillSendManager.Instance.GetEquipSkills(_unit.unitSO.UnitType))
            {
                if(!skillList.Contains(skill))
                  skillList.Add(skill);
            }
            
            Skills = new Dictionary<SkillSO, BaseSkill>();
            

            foreach (var skillData in skillList)
            {
                if (skillData == null || string.IsNullOrEmpty(skillData.className))
                    continue;

                Type type = GetTypeByName(skillData.className);

                if (type == null)
                {
                    Debug.LogError($"[SkillComponent] '{_unit.name}'의 스킬 '{skillData.skillName}'에 해당하는 클래스 '{skillData.className}'를 찾을 수 없습니다. (네임스페이스 확인 필요)");
                    continue;
                }

                var component = _unit.GetComponentInChildren(type, true);

                if (component is BaseSkill baseSkill)
                {

                    if (component == null)
                        continue;
                    Skills.TryAdd(skillData, baseSkill);
                }
                else
                    Debug.LogWarning($"[SkillComponent] '{_unit.name}'에 스킬 컴포넌트 '{type.Name}'가 부착되어 있지 않습니다.");
            }

            SkillSetDamage();

            Bus<UsingSkillEvent>.Subscribe(BooleanSkill);
        }

        private void SkillSetDamage()
        {
            if (Skills.Count > 0)
            {
                foreach (var skill in Skills.Values)
                {
                    if (_statCompo != null)
                    {
                        int skillDamageValue = _statCompo.GetStat(StatInfo.AtkDamage);  
                        int finallyDamage = skillDamageValue * skill.BasicSkillDamage/ 100;  
                        basicDamage = finallyDamage;
                    }
                    else
                        basicDamage = skill.BasicSkillDamage;

                    skill.InitializeSkill();
                    skill.SetDamage(basicDamage);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            Bus<UsingSkillEvent>.Unsubscribe(BooleanSkill);
        }
        
        private Type GetTypeByName(string className)
        {
            Type type = Type.GetType(className);

            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetTypes().FirstOrDefault(t =>
                    t.Name == className || t.FullName == className || t.FullName.EndsWith($".{className}"));

                if (type != null)
                    return type;
            }

            return null;
        }
        
        private void BooleanSkill(UsingSkillEvent evt)
        {
            isUseSkill = evt.isUsingSkill;
        }
        public void ResetSkillsCount()
        {
            foreach (var skill in Skills.Values)
            {
                skill.ResetSkillCnt();
            }
        }
        
        public void UpdateSkillUI()
        {
            Bus<SkillUIEvent>.Raise(new SkillUIEvent(skillList, this));
        }

        public void SetAddSkillDamage(float addDamage,SkillType skillType)
        {
            foreach (var skill in Skills.Values)
            {
                if (skill.SkillSO.SkillType == skillType)
                {
                    int damage = basicDamage + (int)addDamage;
                    skill.SetDamage(damage);
                }
            }
        }
        
        public void StartSkill(SkillSO skillSO)
        {
            if (!isUseSkill)
                return;
            
            if (!Skills.ContainsKey(skillSO))
                return;

            BaseSkill skill = Skills.GetValueOrDefault(skillSO);

            if (skill != null)
            {
                StartSkill(skill,skillSO);
            }
        }

        public void CancelAllSkill()
        {
            if (_unit.isMyTurn)
            {
                foreach (var skill in Skills.Values)
                {
                    CancelSkill(skill);
                }   
            }
        }

        protected virtual void StartSkill(BaseSkill skill, SkillSO skillSO)
        {
            
        }

        protected virtual void CancelSkill(BaseSkill skill)
        {
            
        }
    }
}
