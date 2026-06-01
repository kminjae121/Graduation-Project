namespace PixeLadder.EasyTransition
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A helper component that automatically passes a UI Image's RectTransform size to its material.
    /// Useful for standalone UI elements using custom shaders.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [ExecuteAlways]
    public class UIMaterialPropertyUpdater : MonoBehaviour
    {
        private Image image;
        private RectTransform rectTransform;
        private Material materialInstance;
        private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");

        private void OnEnable()
        {
            image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            RefreshMaterial();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (enabled && gameObject.activeInHierarchy)
            {
                UpdateMaterialProperties();
            }
        }

        private void Update()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                // Ensure material stays updated in Editor mode without needing a recompile
                UpdateMaterialProperties();
            }
        }

        /// <summary>
        /// Creates a unique material instance if one doesn't exist and updates properties.
        /// </summary>
        public void RefreshMaterial()
        {
            if (image == null || image.material == null) return;

            // Don't instantiate if we are already using a custom instance, 
            // unless the base material changed.
            if (materialInstance == null || image.material != materialInstance)
            {
                materialInstance = new Material(image.material);
                materialInstance.name = $"{image.material.name} (Instance)";
                image.material = materialInstance;
            }

            UpdateMaterialProperties();
        }

        private void UpdateMaterialProperties()
        {
            if (materialInstance == null || rectTransform == null) return;

            Vector2 currentSize = rectTransform.rect.size;
            materialInstance.SetVector(RectSizeID, new Vector4(currentSize.x, currentSize.y, 0, 0));
        }

        private void OnDisable()
        {
            if (materialInstance != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(materialInstance);
                else
                    Destroy(materialInstance);

                materialInstance = null;
            }
        }
    }
}