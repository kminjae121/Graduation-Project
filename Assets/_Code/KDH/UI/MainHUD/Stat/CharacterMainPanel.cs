using UnityEngine;

namespace Code.UI
{
    public class CharacterMainPanel : Panel
    {
        [Header("Default Settings")]
        [SerializeField] private string defaultOpenPanelId = "StatPanel";

        public override void Open()
        {
            base.Open();
            
            if (string.IsNullOrEmpty(defaultOpenPanelId) == false)
            {
                PanelManager.Open(defaultOpenPanelId);
            }
            else
            {
                Debug.LogWarning("기본으로 열릴 패널 ID가 지정되지 않았습니다.");
            }
        }
    }
}