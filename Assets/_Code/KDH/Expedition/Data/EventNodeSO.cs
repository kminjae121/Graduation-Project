using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Expedition.Data
{
    [Serializable]
    public class EventChoice
    {
        public string choiceText;
        public string effectId; 
    }

    [CreateAssetMenu(fileName = "NewEventNode", menuName = "SO/Expedition/EventNode")]
    public class EventNodeSO : ExpeditionNodeSO
    {
        [Header("Event Content")]
        public string title;
        [TextArea] public string description;
        public Sprite eventImage;
        
        [Header("Choices")]
        public List<EventChoice> choices;

        private void OnEnable()
        {
            nodeType = ExpeditionNodeType.Event;
        }
    }
}