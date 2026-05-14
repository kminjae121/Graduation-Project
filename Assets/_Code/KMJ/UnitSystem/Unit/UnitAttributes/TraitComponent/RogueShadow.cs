using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public class RogueShadow : MonoBehaviour
    {
        [SerializeField] private UnitGetEnemyCompo enemyCompo;
        public AbstractEnemyUnit NearestEnemy { get; private set; }
        
        
        public AbstractEnemyUnit GetNearEnemy()
        {
            enemyCompo.FindEnemies();

            AbstractEnemyUnit nearest = null;
            float farDistance = float.PositiveInfinity;

            Vector3 myPos = transform.position;

            foreach (AbstractEnemyUnit enemy in enemyCompo.Enemies)
            {
                if (enemy == null) continue;

                float thisDistance = (enemy.transform.position - myPos).sqrMagnitude;
                
                if (thisDistance < farDistance)
                {
                    farDistance = thisDistance;
                    nearest = enemy;
                }
            }

            NearestEnemy = nearest;
            return nearest;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out CharacterUnit characterUnit))
            {
                characterUnit.MoveCompo.MoveCount = 0;
                characterUnit.SetMoveTile();
            }
        }
    }
}