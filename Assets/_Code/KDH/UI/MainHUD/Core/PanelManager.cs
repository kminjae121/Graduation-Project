using System.Collections.Generic;
using UnityEngine;

namespace Code.UI
{
    [DisallowMultipleComponent]
    public class PanelManager : MonoBehaviour
    {
        private readonly Dictionary<string, Panel> _panels = new();
        private static PanelManager _singleton;

        public static PanelManager Singleton
        {
            get
            {
                if (_singleton != null)
                    return _singleton;

                _singleton = FindFirstObjectByType<PanelManager>(FindObjectsInactive.Include);

                if (_singleton == null)
                {
                    Debug.LogWarning("PanelManager가 씬에 없어 런타임 객체를 생성합니다. 로비 UI 씬에는 PanelManager를 명시적으로 배치하는 것을 권장합니다.");
                    _singleton = new GameObject("PanelManager").AddComponent<PanelManager>();
                }

                return _singleton;
            }
        }

        private void Awake()
        {
            if (_singleton != null && _singleton != this)
            {
                Debug.LogWarning("씬에 PanelManager가 둘 이상 있습니다. 나중에 생성된 PanelManager를 제거합니다.", this);
                Destroy(gameObject);
                return;
            }

            _singleton = this;
        }

        private void OnDestroy()
        {
            if (_singleton != this)
                return;

            _panels.Clear();
            _singleton = null;
        }

        public static void Register(Panel panel)
        {
            if (panel == null || string.IsNullOrWhiteSpace(panel.ID))
                return;

            if (Singleton._panels.TryGetValue(panel.ID, out Panel registeredPanel) && registeredPanel != panel)
                Debug.LogWarning($"중복 패널 ID가 등록되었습니다: {panel.ID}", panel);

            Singleton._panels[panel.ID] = panel;
        }

        public static void Unregister(Panel panel)
        {
            if (_singleton == null || panel == null || string.IsNullOrWhiteSpace(panel.ID))
                return;

            if (_singleton._panels.TryGetValue(panel.ID, out Panel registeredPanel) && registeredPanel == panel)
                _singleton._panels.Remove(panel.ID);
        }

        public static bool TryGet(string id, out Panel panel)
        {
            panel = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            return Singleton._panels.TryGetValue(id, out panel) && panel != null;
        }

        public static Panel GetSingleton(string id)
        {
            if (TryGet(id, out Panel panel))
                return panel;

            Debug.LogWarning($"패널 매니저에 '{id}' ID를 가진 패널이 등록되어 있지 않습니다.");
            return null;
        }

        public static bool TryOpen(string id)
        {
            if (!TryGet(id, out Panel panel))
                return false;

            panel.Open();
            return true;
        }

        public static void Open(string id)
        {
            TryOpen(id);
        }

        public static bool TryClose(string id)
        {
            if (!TryGet(id, out Panel panel))
                return false;

            panel.Close();
            return true;
        }

        public static void Close(string id)
        {
            TryClose(id);
        }

        public static bool IsOpen(string id)
        {
            return TryGet(id, out Panel panel) && panel.IsOpen;
        }

        public static void CloseAll()
        {
            List<Panel> panels = new(Singleton._panels.Values);

            foreach (Panel panel in panels)
            {
                if (panel != null)
                    panel.Close();
            }
        }
    }
}
