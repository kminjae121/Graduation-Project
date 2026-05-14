 using Code.UnitSystem.Enemies;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "InvokeTurnEnd", story: "[Enemy] invoke turn end", category: "Action", id: "a5df9b381357813153e8667bed677d48")]
public partial class InvokeTurnEndAction : Action
{
    [SerializeReference] public BlackboardVariable<AbstractEnemyUnit> Enemy;

    protected override Status OnStart()
    {
        if (Enemy.Value == null)
            return Status.Failure;
        
        Enemy.Value.OnTurnEnd();
        return Status.Success;
    }
}

