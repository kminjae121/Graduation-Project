using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using Code.Core.Managers;
using DG.Tweening;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class TurnOrderUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TurnManager turnManager;

        [Header("Settings")]
        [SerializeField] private int showTurnOrderCount = 5;
        [SerializeField] private int currentTurnDisplayIndex = 2;
        [SerializeField] private float edgeSlotScale = 0.75f;
        [SerializeField] private float nearCurrentSlotScale = 1f;
        [SerializeField] private float currentTurnSlotScale = 1.25f;
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private Ease scaleEase = Ease.OutCubic;
        [SerializeField] private bool alignSlotTopEdges = true;
        [SerializeField] private Vector2 slotScalePivot = new Vector2(0.5f, 1f);

        [Header("UI Pool")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private PoolingItemSO unitSlotPoolingSO;

        private PoolManagerMono _poolManager;
        private RectTransform _slotContainerRect;
        private readonly List<TurnOrderUnitSlotUI> _activeUnitSlots = new List<TurnOrderUnitSlotUI>();
        private readonly List<ITurnable> _recentTurnHistory = new List<ITurnable>();

        private bool _isGhostComponent;

        private void Awake()
        {
            if (unitSlotPoolingSO == null)
            {
                _isGhostComponent = true;
                return;
            }

            if (turnManager == null)
                turnManager = UnityEngine.Object.FindFirstObjectByType<TurnManager>();

            _poolManager = UnityEngine.Object.FindFirstObjectByType<PoolManagerMono>();
            ConfigureSlotContainerLayout();
        }

        private void OnEnable()
        {
            if (_isGhostComponent) return;

            Bus<TurnOrderUpdateEvent>.Subscribe(HandleTurnOrderUpdate);
            Bus<UnitTurnEndEvent>.Subscribe(HandleUnitTurnEnd);
        }

        private void OnDisable()
        {
            if (_isGhostComponent) return;

            Bus<TurnOrderUpdateEvent>.Unsubscribe(HandleTurnOrderUpdate);
            Bus<UnitTurnEndEvent>.Unsubscribe(HandleUnitTurnEnd);
        }

        private void HandleTurnOrderUpdate(TurnOrderUpdateEvent evt)
        {
            if (turnManager == null) return;

            List<ITurnable> displayUnits = BuildDisplayUnits();
            int slotCount = Mathf.Max(1, showTurnOrderCount);
            int currentIndex = GetCurrentDisplayIndex();

            ClearAllSlots();

            for (int i = 0; i < slotCount; ++i)
            {
                ITurnable turnable = i < displayUnits.Count ? displayUnits[i] : null;
                TurnOrderUnitSlotUI unitSlot = PopUnitSlot();
                if (unitSlot == null)
                    continue;

                bool isCurrentTurnSlot = i == currentIndex && turnable != null;

                unitSlot.transform.SetParent(slotContainer != null ? slotContainer : transform, false);
                unitSlot.transform.SetSiblingIndex(i);
                unitSlot.SetScalePivot(slotScalePivot);

                RectTransform targetRect = unitSlot.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    targetRect.DOKill(false);
                    Vector2 pos = targetRect.anchoredPosition;
                    pos.y = 0f;
                    targetRect.anchoredPosition = pos;
                }

                unitSlot.Setup(turnable, isCurrentTurnSlot);
                unitSlot.ApplyDisplayState(GetSlotScale(i), isCurrentTurnSlot, scaleDuration, scaleEase, MarkSlotLayoutDirty);
                _activeUnitSlots.Add(unitSlot);
            }

            MarkSlotLayoutDirty();
        }

        private void HandleUnitTurnEnd(UnitTurnEndEvent evt)
        {
            if (!IsValidUnitTurn(evt.Unit))
                return;

            _recentTurnHistory.Remove(evt.Unit);
            _recentTurnHistory.Add(evt.Unit);

            int maxHistoryCount = GetCurrentDisplayIndex();
            while (_recentTurnHistory.Count > maxHistoryCount)
                _recentTurnHistory.RemoveAt(0);
        }

        private List<ITurnable> BuildDisplayUnits()
        {
            int slotCount = Mathf.Max(1, showTurnOrderCount);
            int currentIndex = GetCurrentDisplayIndex();
            List<ITurnable> displayUnits = new List<ITurnable>(slotCount);
            for (int i = 0; i < slotCount; ++i)
                displayUnits.Add(null);

            ITurnable currentUnit = GetCurrentTurnUnit();
            if (currentUnit != null)
                displayUnits[currentIndex] = currentUnit;

            List<ITurnable> recentTurns = GetRecentTurnHistory(currentIndex);
            int historyStartIndex = currentIndex - recentTurns.Count;
            for (int i = 0; i < recentTurns.Count; ++i)
                displayUnits[historyStartIndex + i] = recentTurns[i];

            List<ITurnable> futureUnits = GetFutureUnits(currentUnit);
            int futureIndex = 0;
            for (int i = currentIndex + 1; i < slotCount && futureIndex < futureUnits.Count; ++i)
            {
                displayUnits[i] = futureUnits[futureIndex];
                ++futureIndex;
            }

            return displayUnits;
        }

        private ITurnable GetCurrentTurnUnit()
        {
            ITurnable currentUnit = turnManager.CurrentTurnUnit;
            if (IsValidUnitTurn(currentUnit))
                return currentUnit;

            List<ITurnable> timelineUnits = GetFilteredTimelineUnits(showTurnOrderCount + currentTurnDisplayIndex + 6);
            return timelineUnits.Count > 0 ? timelineUnits[0] : null;
        }

        private List<ITurnable> GetFutureUnits(ITurnable currentUnit)
        {
            List<ITurnable> timelineUnits = GetFilteredTimelineUnits(showTurnOrderCount + currentTurnDisplayIndex + 6);
            List<ITurnable> futureUnits = new List<ITurnable>();
            bool skippedCurrentTurn = currentUnit == null;

            foreach (ITurnable unit in timelineUnits)
            {
                if (!skippedCurrentTurn && ReferenceEquals(unit, currentUnit))
                {
                    skippedCurrentTurn = true;
                    continue;
                }

                futureUnits.Add(unit);
            }

            return futureUnits;
        }

        private List<ITurnable> GetFilteredTimelineUnits(int count)
        {
            List<ITurnable> timelineUnits = turnManager.GetTimelineUnits(count);
            timelineUnits.RemoveAll(unit => !IsValidUnitTurn(unit));
            return timelineUnits;
        }

        private List<ITurnable> GetRecentTurnHistory(int maxCount)
        {
            _recentTurnHistory.RemoveAll(unit => !IsValidUnitTurn(unit));

            int startIndex = Mathf.Max(0, _recentTurnHistory.Count - maxCount);
            return _recentTurnHistory.GetRange(startIndex, _recentTurnHistory.Count - startIndex);
        }

        private bool IsValidUnitTurn(ITurnable unit)
        {
            return unit != null && !(unit is RoundTracker);
        }

        private int GetCurrentDisplayIndex()
        {
            return Mathf.Clamp(currentTurnDisplayIndex, 0, Mathf.Max(0, showTurnOrderCount - 1));
        }

        private float GetSlotScale(int index)
        {
            int distance = Mathf.Abs(index - GetCurrentDisplayIndex());

            if (distance == 0)
                return currentTurnSlotScale;
            if (distance == 1)
                return nearCurrentSlotScale;
            return edgeSlotScale;
        }

        private void ConfigureSlotContainerLayout()
        {
            Transform targetContainer = slotContainer != null ? slotContainer : transform;
            _slotContainerRect = targetContainer as RectTransform;

            HorizontalOrVerticalLayoutGroup layoutGroup = targetContainer.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup == null)
                return;

            layoutGroup.childScaleWidth = true;
            layoutGroup.childScaleHeight = true;
            if (alignSlotTopEdges)
                layoutGroup.childAlignment = TextAnchor.UpperLeft;

            MarkSlotLayoutDirty();
        }

        private void MarkSlotLayoutDirty()
        {
            if (_slotContainerRect == null)
                return;

            LayoutRebuilder.MarkLayoutForRebuild(_slotContainerRect);
        }

        private TurnOrderUnitSlotUI PopUnitSlot()
        {
            if (unitSlotPoolingSO == null)
                return null;

            TurnOrderUnitSlotUI unitSlot = null;
            if (_poolManager != null)
                unitSlot = _poolManager.Pop<TurnOrderUnitSlotUI>(unitSlotPoolingSO);

            if (unitSlot != null || unitSlotPoolingSO.prefab == null)
                return unitSlot;

            GameObject instance = Instantiate(unitSlotPoolingSO.prefab);
            return instance.GetComponent<TurnOrderUnitSlotUI>();
        }

        private void ClearAllSlots()
        {
            foreach (TurnOrderUnitSlotUI slot in _activeUnitSlots)
            {
                if (slot == null)
                    continue;

                RectTransform rectTransform = slot.GetComponent<RectTransform>();
                if (rectTransform != null)
                    rectTransform.DOKill(false);

                slot.ReturnToPool();
            }

            _activeUnitSlots.Clear();
        }
    }
}
