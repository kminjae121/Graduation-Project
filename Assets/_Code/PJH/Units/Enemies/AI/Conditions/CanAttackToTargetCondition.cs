using System;
using Unity.Behavior;
using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "CanAttackToTarget", story: "[Enemy] can attack [Target]", category: "Conditions", id: "4a277512aaf7c426779eff612a000870")]
    public partial class CanAttackToTargetCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemyUnit> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        public override bool IsTrue()
        {
            if (Enemy.Value == null || Enemy.Value.EnemyManager == null)
                return false;

            Enemy.Value.EnemyManager.RefreshPlan(Enemy.Value);
            
            if (!Enemy.Value.EnemyManager.TryGetPlan(Enemy.Value, out EnemyPlan plan)
                || !plan.CanAttackImmediately)
                return false;

            Target.Value = plan.Target.gameObject;
            return true;
        }
    }
}
