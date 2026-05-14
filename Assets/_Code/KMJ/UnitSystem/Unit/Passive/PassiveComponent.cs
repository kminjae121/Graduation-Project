using System;
using System.Collections.Generic;
using System.Linq;
using _Code.KMJ.UnitSystem;
using Code.Managers;
using Code.SkillSystem;
using Code.UnitSystem;
using GondrLib.Dependencies;
using NUnit.Framework;
using UnityEngine;


namespace _Code.Passive
{
    public class PassiveComponent : MonoBehaviour, IUnitComponent
    {
        private Unit _unit;

        [SerializeField] private List<PassiveSO> _passiveList;
        
        private Dictionary<PassiveSO, BasePassive> _passiveDict = new Dictionary<PassiveSO, BasePassive>();
        
        public void Initialize(Unit owner)
        {
            _unit = owner;

            if(PassiveStorage.Instance.GetPassive(_unit.unitSO.UnitType) != null)
                _passiveList = PassiveStorage.Instance.GetPassive(_unit.unitSO.UnitType);

            FindPassive();
        }

        private void Start()
        {
            StartAllAlwaysPassives();
        }

        private void OnDestroy()
        {
            StopAllAlwaysPassives();
        }

        private void FindPassive()
        {
            foreach (var passiveData in _passiveList)
            {
                if (passiveData == null || string.IsNullOrEmpty(passiveData.ClassName))
                    continue;

                Type type = GetTypeByName(passiveData.ClassName);

                if (type == null)
                {
                    Debug.LogError($"[Passive] '{_unit.name}'의 패시브 '{passiveData.PassiveName}'에 해당하는 클래스 '{passiveData.ClassName}'를 찾을 수 없습니다. (네임스페이스 확인 필요)");
                    continue;
                }

                var component = _unit.GetComponentInChildren(type, true);

                if (component is BasePassive basePassive)
                {
                    if (component == null)
                        continue;
                    _passiveDict.TryAdd(passiveData, basePassive);
                    basePassive.SetOwner(_unit);
                    
                    var f = basePassive as AlwaysTurnPassive;
                }
                else
                    Debug.LogWarning($"[Passive] '{_unit.name}'에 패시프 컴포넌트에 '{type.Name}'가 부착되어 있지 않습니다.");
            }
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

        public void StartAllAlwaysPassives()
        {
            foreach (var passive in _passiveDict)
            {
                if(passive.Value as AlwaysTurnPassive)
                    passive.Value.StartPassive();
            }
        }

        public void StopAllAlwaysPassives()
        {
            foreach (var passive in _passiveDict)
            {
                if(passive.Value as AlwaysTurnPassive)
                    passive.Value.StopPassive();
            }
        }
        
        public void StartAllTurnPassives()
        {
            foreach (var passive in _passiveDict)
            {
                if(passive.Value as MyTurnPassive)
                    passive.Value.StartPassive();
            }
        }
        
        public void StopAllTurnPassives()
        {
            foreach (var passive in _passiveDict)
            {
                if(passive.Value as MyTurnPassive)
                    passive.Value.StopPassive();
            }
        }

        public void StartPassive(PassiveSO passive)
        {
            _passiveDict.GetValueOrDefault(passive)?.StartPassive();
        }

        public void StopPassive(PassiveSO passive)
        {
            _passiveDict.GetValueOrDefault(passive)?.StopPassive();
        }
    }
}