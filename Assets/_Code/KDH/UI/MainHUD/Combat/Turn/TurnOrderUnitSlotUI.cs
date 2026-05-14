using Code.Core.Events.Bus;
using Code.Core.Interfaces;
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
        private Pool _pool;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }

        public void ResetItem()
        {
            _targetUnit = null;
            if (highlightFrame != null)
            {
                highlightFrame.SetActive(false);
            }
            if (teamIndicatorImage != null)
            {
                teamIndicatorImage.sprite = null;
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
            _targetUnit = unit;
            
            if (unitIcon != null)
            {
                unitIcon.sprite = unit.UnitImage;
            }
            
            if (teamIndicatorImage != null)
            {
                teamIndicatorImage.sprite = unit.IsPlayerUnit ? allyIndicatorSprite : enemyIndicatorSprite;
                
                if (teamIndicatorImage.sprite != null)
                {
                    teamIndicatorImage.gameObject.SetActive(true);
                }
                else
                {
                    teamIndicatorImage.gameObject.SetActive(false);
                }
            }
            
            if (highlightFrame != null)
            {
                highlightFrame.SetActive(false);
            }
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
                highlightFrame.SetActive(false);
            }
            Bus<CombatUnitHoverEvent>.Raise(new CombatUnitHoverEvent(_targetUnit, false));
        }
    }
}