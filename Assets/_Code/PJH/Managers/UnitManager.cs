using System.Collections.Generic;
using System.Linq;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using GondrLib.Dependencies;
using UnityEngine;

namespace Code.Managers
{
    [Provide]
    public class UnitManager : MonoBehaviour, IDependencyProvider
    {
        private readonly HashSet<Unit> activeUnits = new();

        private void Awake()
        {
            Bus<UnitSpawnEvent>.Subscribe(RegisterUnit);
            Bus<UnitDeadEvent>.Subscribe(RemoveUnit);
        }

        private void OnDestroy()
        {
            Bus<UnitSpawnEvent>.Unsubscribe(RegisterUnit);
            Bus<UnitDeadEvent>.Unsubscribe(RemoveUnit);
        }
        
        #region Public Functions

        public IReadOnlyCollection<Unit> GetAllUnits()
            => activeUnits;

        public IEnumerable<Unit> GetPlayerUnits()
            => activeUnits.Where(unit => unit.IsPlayerUnit);

        public IEnumerable<Unit> GetEnemyUnits()
            => activeUnits.Where(unit => !unit.IsPlayerUnit);
        
        #endregion
        
        private void RegisterUnit(UnitSpawnEvent evt)
        {
            if (evt.Unit != null)
                activeUnits.Add(evt.Unit);
        }

        private void RemoveUnit(UnitDeadEvent evt)
        {
            if (evt.Unit != null)
                activeUnits.Remove(evt.Unit);
        }
    }
}
