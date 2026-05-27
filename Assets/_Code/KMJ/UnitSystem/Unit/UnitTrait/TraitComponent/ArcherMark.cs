using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using UnityEditor;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class ArcherMark : MonoBehaviour
    {
        [SerializeField] private int maxValue = 8;
        
        private int _currentValue = 0;

        public bool SetMarkEnemy(GameObject target)
        {
            EnemyMark enemyMark = target.GetComponentInChildren<EnemyMark>();
            enemyMark.SetMark();

            _currentValue += 1;
            
            Bus<ArcherGimicEvent>.Raise(new ArcherGimicEvent(_currentValue));
            
            if (_currentValue >= maxValue)
            {
                return true;
            }

            return false;
        }

        public void ResetMark()
        {
            _currentValue = 0;
        }
    }
}