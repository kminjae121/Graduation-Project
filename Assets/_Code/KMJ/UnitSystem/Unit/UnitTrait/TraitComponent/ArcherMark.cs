using UnityEditor;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class ArcherMark : MonoBehaviour
    {
        [SerializeField] private int maxValue = 10;
        
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
    }
}