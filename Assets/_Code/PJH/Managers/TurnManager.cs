using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core.Debugs;
using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using Code.UnitSystem;
using GondrLib.Dependencies;
using UnityEngine;

namespace Code.Managers
{
    [Provide]
    public class TurnManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Turn Settings")]
        [SerializeField] private float baseTurnGauge = 100f;
        [SerializeField] private float firstRoundInterval = 150f;
        [SerializeField] private float roundInterval = 100f;

        [Header("Dependencies")]
        [SerializeField] private UnitManager unitManager;

        public int CurrentRound { get; private set; }

        public event Action OnTurnStart;

        private ITurnable _currentTurnUnit;
        private List<ITurnable> _units;
        private RoundTracker _roundTracker;
        private bool _turnFlag;

        private void Awake()
        {
            Bus<UnitTurnEndEvent>.Subscribe(OnUnitTurnEnd);
        }

        private void Update()
        {
            if (!_turnFlag)
                return;
            
            _turnFlag = false;
            StartNextTurn();
        }

        private void OnDestroy()
        {
            Bus<UnitTurnEndEvent>.Unsubscribe(OnUnitTurnEnd);
        }

        public void StartBattle()
        {
            CurrentRound = 1;
            
            _roundTracker = new RoundTracker
            {
                NextRound = 2,
                TurnGauge = firstRoundInterval
            };

            RefreshUnits();

            foreach (var unit in _units)
            {
                if (unit is RoundTracker)
                    continue;
                
                unit.TurnGauge = CalculateBaseTurnGauge(unit);
            }

            StartNextTurn();
        }

        private float CalculateBaseTurnGauge(ITurnable unit)
        {
            if (unit == null)
                return 0;
            return baseTurnGauge / Mathf.Max(1f, unit.TurnSpeed);
        }

        private void OnUnitTurnEnd(UnitTurnEndEvent evt)
        {
            if (_currentTurnUnit == null)
                return;

            if (_currentTurnUnit.UnitObj.TryGetComponent(out CharacterUnit unit))
            {
                unit.OnTurnEnd();
            }

            if (!ReferenceEquals(evt.Unit, _currentTurnUnit))
            {
                UnityLogger.LogWarning($"[{nameof(TurnManager)}] 현재 턴 유닛이 [{_currentTurnUnit.UnitName}]이지만, [{evt.Unit?.UnitName}]의 턴 종료 이벤트가 발행됨.");
                return;
            }

            if (_currentTurnUnit != null)
            {
                _currentTurnUnit.TurnGauge = CalculateBaseTurnGauge(_currentTurnUnit);
                _currentTurnUnit = null;   
            }

            _turnFlag = true;
        }

        private void StartNextTurn()
        {
            if (unitManager == null)
            {
                UnityLogger.LogError($"[{nameof(TurnManager)}] UnitManager가 할당되지 않았습니다.");
                return;
            }

            int safeCount = 0;
            
            while (safeCount < 100)
            {
                ++safeCount;
                RefreshUnits();

                if (_units == null || _units.Count == 0)
                {
                    UnityLogger.LogWarning($"[{nameof(TurnManager)}] 턴을 진행할 수 있는 유닛이 없습니다.");
                    _currentTurnUnit = null;
                    return;
                }

                _currentTurnUnit = GetNextUnit();
                AdvanceTime(_currentTurnUnit);

                if (_currentTurnUnit is RoundTracker rt)
                {
                    CurrentRound = rt.NextRound;
                    rt.NextRound = CurrentRound + 1;
                    rt.TurnGauge = roundInterval;
                    _currentTurnUnit = null;
                    
                    Bus<TurnOrderUpdateEvent>.Raise(new TurnOrderUpdateEvent());
                    continue;
                }

                OnTurnStart?.Invoke();
                _currentTurnUnit.OnTurnStart();

                Bus<TurnOrderUpdateEvent>.Raise(new TurnOrderUpdateEvent());
                return;
            }
            
            UnityLogger.LogError("턴 계산 과정에서 무한 루프 발생");
        }

        private void RefreshUnits()
        {
            _units = unitManager.GetAllUnits().OfType<ITurnable>().ToList();

            if (_roundTracker != null)
                _units.Add(_roundTracker);
        }

        private ITurnable GetNextUnit()
        {
            return _units.OrderBy(u => u.TurnGauge).First();
        }

        private void AdvanceTime(ITurnable actingUnit)
        {
            float delta = actingUnit.TurnGauge;

            foreach (var unit in _units)
                unit.TurnGauge -= delta;

            ClampAllTurnGauge();
        }

        private void ClampAllTurnGauge()
        {
            foreach (var unit in _units)
                unit.TurnGauge = Mathf.Max(0f, unit.TurnGauge);
        }

        public void ModifyTurnGauge(ITurnable unit, float delta)
        {
            unit.TurnGauge += delta;
            unit.TurnGauge = Mathf.Max(0f, unit.TurnGauge);
            Bus<TurnOrderUpdateEvent>.Raise(new TurnOrderUpdateEvent());
        }

        public void ForceImmediateTurn(ITurnable unit)
        {
            unit.TurnGauge = 0f;
            Bus<TurnOrderUpdateEvent>.Raise(new TurnOrderUpdateEvent());
        }

        public List<ITurnable> GetTimelineUnits(int count)
        {
            List<ITurnable> timeline = new List<ITurnable>();
            
            if (_units == null || _units.Count == 0)
                return timeline;

            Dictionary<ITurnable, float> currentGauges = new Dictionary<ITurnable, float>();
            
            foreach (var u in _units)
                currentGauges[u] = u.TurnGauge;

            for (int i = 0; i < count; ++i)
            {
                if (currentGauges.Count == 0)
                    break;

                ITurnable nextUnit = null;
                float minGauge = float.MaxValue;

                foreach (var kvp in currentGauges)
                    if (kvp.Value < minGauge)
                    {
                        minGauge = kvp.Value;
                        nextUnit = kvp.Key;
                    }

                if (nextUnit == null)
                    break;

                timeline.Add(nextUnit);

                var keys = currentGauges.Keys.ToList();
                
                foreach (var k in keys)
                    currentGauges[k] -= minGauge;

                if (nextUnit is RoundTracker)
                    currentGauges[nextUnit] += roundInterval;
                else
                    currentGauges[nextUnit] += CalculateBaseTurnGauge(nextUnit);
            }

            return timeline;
        }
    }
}