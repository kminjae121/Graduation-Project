using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/TurnChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "TurnChannel", message: "Start Self turn", category: "Events", id: "464f29d1a6bfa152fc814b90046c46b9")]
public sealed partial class TurnChannel : EventChannel { }

