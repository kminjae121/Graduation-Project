using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class RogueShadow : MonoBehaviour
    {
        [SerializeField] private UnitGetEnemyCompo enemyCompo;

        public AbstractEnemyUnit NearestEnemy { get; private set; }

        private Vector3 _worldPosition;
        private Quaternion _worldRotation;
        private Vector3 _worldScale;

        private void Start()
        {
            _worldPosition = transform.position;
            _worldRotation = transform.rotation;
            _worldScale = transform.lossyScale;
        }

        private void LateUpdate()
        {
            transform.position = _worldPosition;
            transform.rotation = _worldRotation;

            Vector3 parentScale = Vector3.one;

            if (transform.parent != null)
            {
                parentScale = transform.parent.lossyScale;
            }

            transform.localScale = new Vector3(
                _worldScale.x / parentScale.x,
                _worldScale.y / parentScale.y,
                _worldScale.z / parentScale.z

            );
        }

        public void SetPos(Vector3 pos)
        {
            _worldPosition = pos;
        }

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