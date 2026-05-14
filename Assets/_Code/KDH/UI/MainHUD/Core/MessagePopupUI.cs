using Code.Core.Events.Bus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class MessagePopupUI : Panel
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI messageText;
        
        [Header("Buttons")]
        [SerializeField] private Button checkButton;

        public override void Awake()
        {
            base.Awake();
            Bus<ShowMessageUIEvent>.Subscribe(HandleShowMessage);
            checkButton.onClick.AddListener(Hide);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            Bus<ShowMessageUIEvent>.Unsubscribe(HandleShowMessage);
            checkButton.onClick.RemoveListener(Hide);
        }
        
        private void HandleShowMessage(ShowMessageUIEvent evt)
        {
            Show(evt.Message);
        }
        
        private void Show(string message)
        {
            messageText.text = message;
            base.Open();
        }
        
        private void Hide()
        {
            base.Close();
        }
    }
}