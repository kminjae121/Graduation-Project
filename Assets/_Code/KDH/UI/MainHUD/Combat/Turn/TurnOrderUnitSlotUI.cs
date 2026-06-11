using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using DG.Tweening;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class TurnOrderUnitSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPoolable
    {
        [SerializeField] private PoolingItemSO poolingType;

        [Header("UI Elements")]
        [SerializeField] private Image unitIcon;
        [SerializeField] private GameObject highlightFrame;

        [Header("Team Indicator")]
        [SerializeField] private Image teamIndicatorImage;
        [SerializeField] private Sprite allyIndicatorSprite;
        [SerializeField] private Sprite enemyIndicatorSprite;

        private ITurnable _targetUnit;
        private RectTransform _rectTransform;
        private Tween _scaleTween;
        private Pool _pool;
        private bool _isCurrentTurnSlot;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            _scaleTween?.Kill();
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }

        public void ResetItem()
        {
            _scaleTween?.Kill();
            _targetUnit = null;
            _isCurrentTurnSlot = false;
            transform.localScale = Vector3.one;

            if (unitIcon != null)
            {
                unitIcon.sprite = null;
                unitIcon.gameObject.SetActive(false);
            }

            if (highlightFrame != null)
            {
                highlightFrame.SetActive(false);
            }

            if (teamIndicatorImage != null)
            {
                teamIndicatorImage.sprite = null;
                teamIndicatorImage.gameObject.SetActive(false);
            }
        }

        public void ReturnToPool()
        {
            if (_pool != null)
            {
                _pool.Push(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Setup(ITurnable unit)
        {
            Setup(unit, false);
        }

        public void SetScalePivot(Vector2 pivot)
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_rectTransform == null)
                return;

            _rectTransform.pivot = pivot;
        }

        public void Setup(ITurnable unit, bool isCurrentTurnSlot)
        {
            _targetUnit = unit;
            _isCurrentTurnSlot = isCurrentTurnSlot;

            if (unitIcon != null)
            {
                unitIcon.sprite = unit != null ? unit.UnitImage : null;
                unitIcon.gameObject.SetActive(unitIcon.sprite != null);
            }

            if (teamIndicatorImage != null)
            {
                teamIndicatorImage.sprite = unit != null && unit.IsPlayerUnit ? allyIndicatorSprite : enemyIndicatorSprite;
                teamIndicatorImage.gameObject.SetActive(unit != null && teamIndicatorImage.sprite != null);
            }

            if (highlightFrame != null)
            {
                highlightFrame.SetActive(_isCurrentTurnSlot);
            }
        }

        public void ApplyDisplayState(float targetScale, bool isCurrentTurnSlot, float duration, Ease ease, System.Action onLayoutChanged = null)
        {
            _isCurrentTurnSlot = isCurrentTurnSlot;

            if (highlightFrame != null)
            {
                highlightFrame.SetActive(_isCurrentTurnSlot);
            }

            Transform scaleTarget = _rectTransform != null ? _rectTransform : transform;
            Vector3 target = Vector3.one * targetScale;

            _scaleTween?.Kill();
            if (duration <= 0f)
            {
                scaleTarget.localScale = target;
                onLayoutChanged?.Invoke();
                return;
            }

            _scaleTween = scaleTarget.DOScale(target, duration)
                .SetEase(ease)
                .OnUpdate(() => onLayoutChanged?.Invoke())
                .OnComplete(() => onLayoutChanged?.Invoke());
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_targetUnit == null) return;

            if (highlightFrame != null)
            {
                highlightFrame.SetActive(true);
            }

            Bus<CombatUnitHoverEvent>.Raise(new CombatUnitHoverEvent(_targetUnit, true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_targetUnit == null) return;

            if (highlightFrame != null)
            {
                highlightFrame.SetActive(_isCurrentTurnSlot);
            }

            Bus<CombatUnitHoverEvent>.Raise(new CombatUnitHoverEvent(_targetUnit, false));
        }
    }
}
