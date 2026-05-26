using Code.Core.Events.Bus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Campaign
{
    public sealed class CampaignDateUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI weekText;
        [SerializeField] private TextMeshProUGUI expeditionText;
        [SerializeField] private string weekFormat = "Week {0}";
        [SerializeField] private string expeditionFormat = "Expedition {0}";
        [SerializeField] private bool showExpeditionCount = true;

        private void Awake()
        {
            Bus<CampaignDateChangedEvent>.Subscribe(HandleCampaignDateChanged);
            Refresh();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            Bus<CampaignDateChangedEvent>.Unsubscribe(HandleCampaignDateChanged);
        }

        public void Refresh()
        {
            Refresh(CampaignDateSystem.Current);
        }

        public void ResetDateForDebug()
        {
            CampaignDateSystem.ResetDate();
        }

        private void HandleCampaignDateChanged(CampaignDateChangedEvent evt)
        {
            Refresh(evt.Snapshot);
        }

        private void Refresh(CampaignDateSnapshot snapshot)
        {
            if (weekText != null)
                weekText.text = string.Format(weekFormat, snapshot.Week);

            if (expeditionText != null)
            {
                expeditionText.gameObject.SetActive(showExpeditionCount);
                expeditionText.text = string.Format(expeditionFormat, snapshot.ExpeditionCount);
            }
        }

        public static CampaignDateUI CreateDefault(Canvas canvas)
        {
            if (canvas == null)
                return null;

            RectTransform root = CreateRect("CampaignDateHUD", canvas.transform);
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = new Vector2(48f, -40f);
            root.sizeDelta = new Vector2(280f, 92f);

            Image background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0.03f, 0.035f, 0.045f, 0.82f);

            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.16f);
            outline.effectDistance = new Vector2(2f, -2f);

            TextMeshProUGUI week = CreateText("WeekText", root, new Vector2(18f, -12f), new Vector2(-18f, -50f), 30f, FontStyles.Bold);
            week.alignment = TextAlignmentOptions.Left;

            TextMeshProUGUI expedition = CreateText("ExpeditionText", root, new Vector2(20f, -52f), new Vector2(-18f, -84f), 17f, FontStyles.Normal);
            expedition.alignment = TextAlignmentOptions.Left;

            CampaignDateUI ui = root.gameObject.AddComponent<CampaignDateUI>();
            ui.weekText = week;
            ui.expeditionText = expedition;
            ui.Refresh();
            return ui;
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject go = new(objectName, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static TextMeshProUGUI CreateText(
            string objectName,
            Transform parent,
            Vector2 offsetMin,
            Vector2 offsetMax,
            float fontSize,
            FontStyles style)
        {
            RectTransform rect = CreateRect(objectName, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }
    }
}
