using System.Collections;
using Code.UnitSystem;
using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class RoguePerform : MonoBehaviour, IUnitPerform
    {
        [SerializeField] private UnitRotator rotatorCompo;
        [SerializeField] private UnitAnimation animCompo;
        [SerializeField] private UnitEffectCompo effectCompo;
        [SerializeField] private RogueShadowSpawn shadowCompo;
        
        private UnitEffectCompo _effectCompo;
        private Unit _unit;

        private DamageData _damageData;
        
        public void Initialize(Unit unit)
        {
            _unit = unit;
            _effectCompo = _unit.GetUnitCompo<UnitEffectCompo>();
            _damageData = new DamageData();
        }

        public void Perform(GameObject target)
        {
            StartCoroutine(PerformTarget(target));
        }

        private IEnumerator PerformTarget(GameObject target)
        {
            effectCompo.StopTargetEffect("DarkHeal");
            int addDamage = 0;
            Vector3 pos = _unit.transform.position;
            
            foreach (var shadow in shadowCompo.GetShadows())
            {
                _unit.transform.position = shadow.gameObject.transform.position;
                
                AbstractEnemyUnit enemy = shadow.GetNearEnemy();
                
                
                if (enemy != null)
                {
                    rotatorCompo.SetDir(enemy.transform.position);
                    animCompo.RestartFromEntry();
                    animCompo.PlaySelectAnimation("ATTACK");
                    UnitHealth health = enemy.GetUnitCompo<UnitHealth>();
                    if (health != null)
                    {
                        int damage = Mathf.FloorToInt(health.CurrentHealth * 0.1f);

                        _damageData.damage = damage;
                        addDamage += damage;
                        
                        Bus<DamageEvent>.Raise(new DamageEvent(_damageData, enemy.gameObject, 0, _unit, false, false, 0.3f));
                    }    
                }
                shadow.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.7f);
            }

            _unit.transform.position = pos;
            _damageData.damage = addDamage;
            
            Bus<DamageEvent>.Raise(new DamageEvent(_damageData,target.gameObject,0, _unit,false,false,0.3f));
            shadowCompo.ResetAllShadow();
            animCompo.ReturnIdleAnimation();
        }
    }
}