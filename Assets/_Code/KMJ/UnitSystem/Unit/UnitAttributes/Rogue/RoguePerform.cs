using System.Collections;
using _Code.UnitSystem;
using Code.Core.Events.Bus;
using Code.UnitSystem.Combat;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public class RoguePerform : MonoBehaviour, IUnitPerform
    {
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
            int addDamage = 0;
            Vector3 pos = _unit.transform.position;
            
            foreach (var shadow in shadowCompo.GetShadows())
            {
                _unit.transform.position = shadow.gameObject.transform.position;
                
                AbstractEnemyUnit enemy = shadow.GetNearEnemy();
                
                if (enemy != null)
                {
                    UnitHealth health = enemy.GetUnitCompo<UnitHealth>();
                    if (health != null)
                    {
                        int damage = Mathf.FloorToInt(health.CurrentHealth * 0.1f);

                        _damageData.damage = damage;
                        addDamage += damage;
                        
                        Bus<DamageEvent>.Raise(new DamageEvent(_damageData, enemy.gameObject, 0, _unit, false, false, 0.3f));
                    }    
                }
                
                yield return new WaitForSeconds(0.2f);
            }

            _unit.transform.position = pos;
            _damageData.damage = addDamage;
            
            Bus<DamageEvent>.Raise(new DamageEvent(_damageData,target.gameObject,0, _unit,false,false,0.3f));
            shadowCompo.ResetAllShadow();
        }
    }
}