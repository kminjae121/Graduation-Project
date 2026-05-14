using System;
using Code.Core.Debugs;
using Code.Map;
using Code.UnitSystem.UnitComponent;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Code.UnitSystem.Enemies.AI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "MoveToTarget", story: "[Enemy] move to [Target]", category: "Action", id: "b573e234c5921f41ffd38ca38e3e3074")]
    public partial class MoveToTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemyUnit> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        private PathMover _mover;
        private GridMap _gridMap;
        private bool _isMoving;
        private bool _reservedTile;

        protected override Status OnStart()
        {
            if (Enemy.Value == null)
                return Status.Failure;
            
            _gridMap = GridMap.Instance;
            _mover = Enemy.Value.PathMover;

            if (_gridMap == null)
            {
                UnityLogger.LogError("GridMap is missing.");
                return Status.Failure;
            }

            if (_mover == null)
            {
                UnityLogger.LogError("PathMover is missing.");
                return Status.Failure;
            }

            if (Enemy.Value.EnemyManager == null)
                return Status.Failure;

            Enemy.Value.EnemyManager.RefreshPlan(Enemy.Value);
            
            if (!Enemy.Value.EnemyManager.TryGetPlan(Enemy.Value, out EnemyPlan plan) || plan.Target == null)
                return Status.Failure;

            Target.Value = plan.Target.gameObject;

            if (!plan.HasMoveTile)
                return Status.Success;

            Vector2Int startPos = _gridMap.WorldToGridPos(Enemy.Value.transform.position);
            Vector2Int destination = plan.MoveTile;

            if (destination == startPos)
                return Status.Success;

            if (!Enemy.Value.EnemyManager.TryReserveTile(Enemy.Value, destination))
                return Status.Success;

            _reservedTile = true;
            _isMoving = true;
            _mover.OnMoveEnd += HandleMovementEnd;
            _mover.SetPathAndMove(startPos, destination);

            return Status.Running;
        }
        
        protected override Status OnUpdate()
        {
            return _isMoving ? Status.Running : Status.Success;
        }

        protected override void OnEnd()
        {
            if (_mover != null)
                _mover.OnMoveEnd -= HandleMovementEnd;

            if (_reservedTile && Enemy.Value?.EnemyManager != null)
            {
                Enemy.Value.EnemyManager.ReleaseReservation(Enemy.Value);
                _reservedTile = false;
            }
        }

        private void HandleMovementEnd() => _isMoving = false;
    }
}
