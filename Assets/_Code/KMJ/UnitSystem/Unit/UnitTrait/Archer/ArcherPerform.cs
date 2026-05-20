using System;
using System.Collections;
using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class ArcherPerform : MonoBehaviour, IUnitPerform
    {
        [SerializeField] private ArcherMark markCompo;
        [SerializeField] private UnitGetEnemyCompo getEnemyCompo;
        [SerializeField] private UnitAnimation animationCompo;
        [SerializeField] private UnitAnimationTrigger triggerCompo;

        private UnitStatCompo _unitStatCompo;
        private ShootItemAttackManager _shootItemManager;
        private Unit _unit;

        private DamageData _damageData;

        private int _atkDamage;

        public void Initialize(Unit unit)
        {
            _unit = unit;
            _unitStatCompo = unit.GetUnitCompo<UnitStatCompo>();
            _shootItemManager = unit.GetUnitCompo<ShootItemAttackManager>();

            _atkDamage = _unitStatCompo.GetStat(StatInfo.AtkDamage);
            _damageData.damage = _atkDamage;
        }

        private void AtkAllEnemies()
        {
            foreach (var enemy in getEnemyCompo.Enemies)
            {
                Vector3 pos = animationCompo.transform.position;
                pos.y += 0.3f;
            
                Vector3 slashRot = _unit.transform.rotation.eulerAngles;
            
                _shootItemManager.SetTarget(enemy.gameObject);
                _shootItemManager.SetDamageData(_damageData,0);
                _shootItemManager.CreateShootItem("CriticalArrow",pos, slashRot);   
            }
            Bus<CamShakeEvent>.Raise(new CamShakeEvent(0.15f));
            
            triggerCompo.OnAttackTrigger -= AtkAllEnemies; 
        }

        public void Perform(GameObject target)
        {
            triggerCompo.OnAttackTrigger += AtkAllEnemies;
            getEnemyCompo.FindEnemies();
            
            animationCompo.PlaySelectAnimation("ATTACK");
            markCompo.ResetMark();
        }
    } 
}