using System;
using Code.Core.Debugs;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Code.UnitSystem.Enemies.AI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "ChangeAnimation", story: "[UnitAnimator] change to [animName]", category: "Action", id: "fb10095125fc8a3f972d297b55a57e1b")]
    public partial class ChangeAnimationAction : Action
    {
        [SerializeReference] public BlackboardVariable<UnitAnimation> UnitAnimator;
        [SerializeReference] public BlackboardVariable<string> AnimName;

        protected override Status OnStart()
        {
            UnitAnimator.Value.PlaySelectAnimation(AnimName.Value);
            //UnityLogger.Log($"애니메이션 체인지 : {AnimName.Value}");
            return Status.Success;
        }
    }
}