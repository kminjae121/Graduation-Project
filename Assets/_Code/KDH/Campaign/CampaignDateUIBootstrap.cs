using Code.Tower;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.Campaign
{
    public static class CampaignDateUIBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            TryCreateForScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryCreateForScene(scene);
        }

        private static void TryCreateForScene(Scene scene)
        {
            if (!scene.IsValid() || scene.name != TowerRunSession.DefaultLobbySceneName)
                return;

            if (Object.FindFirstObjectByType<CampaignDateUI>(FindObjectsInactive.Include) != null)
                return;

            Canvas canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Exclude);

            if (canvas == null)
                canvas = CreateCanvas();

            CampaignDateUI.CreateDefault(canvas);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new("CampaignDateCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }
    }
}
