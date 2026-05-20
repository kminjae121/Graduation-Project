using UnityEditor;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class ArcherMark : MonoBehaviour
    {
        [SerializeField] private int maxValue = 3;
        
        private int _currentValue = 0;

        public bool SetMarkEnemy(GameObject target)
        {
            EnemyMark enemyMark = target.GetComponentInChildren<EnemyMark>();
            enemyMark.SetMark();

            _currentValue += 1;
            
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