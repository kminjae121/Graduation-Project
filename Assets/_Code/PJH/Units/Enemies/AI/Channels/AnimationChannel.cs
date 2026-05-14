using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/AnimationChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "AnimationChannel", message: "change to [NextAnimation]", category: "Events", id: "de731e9af72efb2cdaaa4f05d6c30be8")]
public sealed partial class AnimationChannel : EventChannel<string> { }

