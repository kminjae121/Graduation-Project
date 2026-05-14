using Code.Core.Interfaces;
using UnityEngine;

namespace Code.Managers
{
    public class RoundTracker : ITurnable
    {
        public GameObject UnitObj { get; set; } = null;
        public string UnitName => "Round Marker";
        public bool IsPlayerUnit => false;
        
        public float TurnGauge { get; set; }
        
        public bool IsReadyDoAct => false;
        
        public int TurnSpeed => 0;
        
        public Sprite UnitImage => null;

        public int NextRound { get; set; } = 1;

        public void OnTurnStart() 
        {
        }
        
        public void OnTurnEnd() 
        {
        }
    }
}