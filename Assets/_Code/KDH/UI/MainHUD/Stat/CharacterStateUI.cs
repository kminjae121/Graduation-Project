using Code.Core.Events.Bus;
using Code.UnitSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CharacterStateUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image characterImage;
        [SerializeField] private Image healthBar;
        [SerializeField] private Button stateButton;
        
        [Header("Settings")]
        [SerializeField] private float tweenTime = 0.3f;
        [SerializeField] private string mainPanelId = "MainUnitInfoPanel";

        private UnitState _unit;
        private Tween _healthBarTween;

        private void Awake()
        {
            if (stateButton != null)
            {
                stateButton.onClick.AddListener(HandleStateButtonClick);
            }
        }

        private void OnDestroy()
        {
            if (stateButton != null)
            {
                stateButton.onClick.RemoveListener(HandleStateButtonClick);
            }

            if (_unit != null)
            {
                _unit.CurrentHp.OnValueChanged -= RefreshHealthBar;
            }
        }

        public void SetUnit(UnitState unit)
        {
            _unit = unit;
            _unit.CurrentHp.OnValueChanged += RefreshHealthBar;
            
            characterImage.sprite = _unit.Data.UnitImage;
        }

        private void HandleStateButtonClick()
        {
            if (_unit == null)
            {
                Debug.LogWarning("유닛 정보가 존재하지 않습니다.");
                return;
            }

            Bus<CharacterInfoEvent>.Raise(new CharacterInfoEvent(_unit));
            PanelManager.Open(mainPanelId);
        }

        private void RefreshHealthBar(float prev, float next)
        {
            float fillValue = next / _unit.Data.Maxhealth;

            _healthBarTween?.Kill();
            _healthBarTween = healthBar
                .DOFillAmount(fillValue, tweenTime)
                .SetEase(Ease.OutCubic);
        }
    }
}