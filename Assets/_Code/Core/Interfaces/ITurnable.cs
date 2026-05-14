using UnityEngine;

namespace Code.Core.Interfaces
{
    public interface ITurnable
    {
        GameObject UnitObj { get; set; }
        string UnitName { get; }
        bool IsPlayerUnit { get; }

        float TurnGauge { get; set; }
        
        bool IsReadyDoAct { get; }
        
        int TurnSpeed { get; }
        
        Sprite UnitImage { get; }

        void OnTurnStart();
        void OnTurnEnd();
    }
}