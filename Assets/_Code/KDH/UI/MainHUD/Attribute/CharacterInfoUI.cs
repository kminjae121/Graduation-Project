using Code.Core.Events.Bus;
using Code.UnitSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class CharacterInfoUI : Panel
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private Image unitImage;
        
        [Header("Buttons")]
        [SerializeField] private Button exitButton;
        
        private UnitState _unit;

        public override void Awake()
        {
            base.Awake();
            exitButton.onClick.AddListener(HandleExitButton);
            Bus<CharacterInfoEvent>.Subscribe(HandleUnitInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            exitButton.onClick.RemoveListener(HandleExitButton);
            Bus<CharacterInfoEvent>.Unsubscribe(HandleUnitInfo);
        }
        
        private void HandleUnitInfo(CharacterInfoEvent evt)
        {
            _unit = evt.Unit;
            
            unitNameText.text = _unit.Data.UnitName;
            unitImage.sprite = _unit.Data.UnitImage;
            
            base.Open();
        }
        
        private void HandleExitButton()
        {
            base.Close();
        }
    }
}