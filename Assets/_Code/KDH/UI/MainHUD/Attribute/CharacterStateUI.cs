using Code.Core.Events.Bus;
using Code.UnitSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public enum CharacterStateClickMode
    {
        OpenInfoPanel,
        SelectExpeditionParty
    }

    public class CharacterStateUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image characterImage;
        [SerializeField] private Image healthBar;
        [SerializeField] private Button stateButton;

        [Header("Settings")]
        [SerializeField] private float tweenTime = 0.3f;
        [SerializeField] private string mainPanelId = "MainUnitInfoPanel";
        [SerializeField] private CharacterStateClickMode clickMode = CharacterStateClickMode.OpenInfoPanel;
        [SerializeField] private bool sendPartyHoverEvents;

        private UnitState _unit;
        private Tween _healthBarTween;

        private void Awake()
        {
            if (stateButton != null)
                stateButton.onClick.AddListener(HandleStateButtonClick);
        }

        private void OnDestroy()
        {
            if (stateButton != null)
                stateButton.onClick.RemoveListener(HandleStateButtonClick);

            UnsubscribeCurrentUnit();
            _healthBarTween?.Kill();
        }

        public void SetClickMode(CharacterStateClickMode mode, bool enablePartyHoverEvents)
        {
            clickMode = mode;
            sendPartyHoverEvents = enablePartyHoverEvents;
        }

        public void SetUnit(UnitState unit)
        {
            UnsubscribeCurrentUnit();

            _unit = unit;

            if (_unit == null)
            {
                if (characterImage != null)
                    characterImage.sprite = null;

                if (healthBar != null)
                    healthBar.fillAmount = 0f;

                return;
            }

            if (_unit.CurrentHp != null)
                _unit.CurrentHp.OnValueChanged += RefreshHealthBar;

            if (characterImage != null)
                characterImage.sprite = _unit.Data.UnitImage;

            RefreshHealthBar(0f, _unit.CurrentHp != null ? _unit.CurrentHp.Value : _unit.Data.Maxhealth);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (sendPartyHoverEvents && _unit?.Data != null)
                Bus<PartyCharacterHoverEvent>.Raise(new PartyCharacterHoverEvent(_unit.Data));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (sendPartyHoverEvents)
                Bus<PartyCharacterHoverEvent>.Raise(new PartyCharacterHoverEvent(null));
        }

        private void HandleStateButtonClick()
        {
            if (_unit == null)
            {
                Debug.LogWarning("유닛 정보가 존재하지 않습니다.");
                return;
            }

            if (clickMode == CharacterStateClickMode.SelectExpeditionParty)
            {
                Bus<PartyCharacterSelectEvent>.Raise(new PartyCharacterSelectEvent(_unit.Data));
                return;
            }

            Bus<CharacterInfoEvent>.Raise(new CharacterInfoEvent(_unit));
            PanelManager.Open(mainPanelId);
        }

        private void RefreshHealthBar(float prev, float next)
        {
            if (_unit == null || healthBar == null)
                return;

            float fillValue = next / _unit.Data.Maxhealth;

            _healthBarTween?.Kill();
            _healthBarTween = healthBar
                .DOFillAmount(fillValue, tweenTime)
                .SetEase(Ease.OutCubic);
        }

        private void UnsubscribeCurrentUnit()
        {
            if (_unit?.CurrentHp != null)
                _unit.CurrentHp.OnValueChanged -= RefreshHealthBar;
        }
    }
}
