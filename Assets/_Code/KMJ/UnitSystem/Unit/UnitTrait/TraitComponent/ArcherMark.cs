using UnityEditor;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class ArcherMark : MonoBehaviour
    {
        private int _currentValue = 0;
        [SerializeField] private int _maxValue = 10;

        public bool SetMarkEnemy(GameObject target)
        {
            EnemyMark enemyMark = target.GetComponentInChildren<EnemyMark>();

            enemyMark.SetMark();

            _currentValue += 1;
            
            if (_currentValue >= _maxValue)
            {
                return true;
            }

            return false;
        }
    }
}