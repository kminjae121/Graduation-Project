using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public abstract class SpecUI : MonoBehaviour
    {
        public UnitType UnitType;

        public abstract void OperationUI();

        protected static List<Image> ResolveGaugeImages(Transform root, IReadOnlyList<Image> assignedImages)
        {
            List<Image> result = new List<Image>();

            if (assignedImages != null)
            {
                for (int i = 0; i < assignedImages.Count; ++i)
                {
                    Image image = assignedImages[i];
                    if (image != null && !result.Contains(image))
                        result.Add(image);
                }
            }

            if (result.Count == 0 && root != null)
            {
                foreach (Transform child in root)
                {
                    Image image = SelectGaugeImage(child);
                    if (image != null && !result.Contains(image))
                        result.Add(image);
                }
            }

            return result;
        }

        protected static void SetCountGaugeImages(IReadOnlyList<Image> images, int value, int maxValue, float duration, bool immediate)
        {
            if (images == null || images.Count == 0)
                return;

            int slotCount = Mathf.Min(images.Count, Mathf.Max(0, maxValue));
            int filledCount = Mathf.Clamp(value, 0, slotCount);

            for (int i = 0; i < images.Count; ++i)
            {
                Image image = images[i];
                if (image == null)
                    continue;

                PrepareGaugeImage(image);
                float targetFillAmount = i < filledCount ? 1f : 0f;
                image.DOKill(false);

                if (immediate || duration <= 0f)
                {
                    image.fillAmount = targetFillAmount;
                    continue;
                }

                image.DOFillAmount(targetFillAmount, duration);
            }
        }

        protected static void KillGaugeImageTweens(IReadOnlyList<Image> images)
        {
            if (images == null)
                return;

            for (int i = 0; i < images.Count; ++i)
            {
                if (images[i] != null)
                    images[i].DOKill(false);
            }
        }

        private static Image SelectGaugeImage(Transform slot)
        {
            if (slot == null)
                return null;

            Image[] images = slot.GetComponentsInChildren<Image>(true);

            for (int i = 0; i < images.Length; ++i)
            {
                Image image = images[i];
                if (image != null && image.transform != slot && image.name.IndexOf("Spec", StringComparison.OrdinalIgnoreCase) >= 0)
                    return image;
            }

            for (int i = 0; i < images.Length; ++i)
            {
                Image image = images[i];
                if (image != null && image.type == Image.Type.Filled)
                    return image;
            }

            Image directImage = slot.GetComponent<Image>();
            if (directImage != null)
                return directImage;

            return images.Length > 0 ? images[0] : null;
        }

        private static void PrepareGaugeImage(Image image)
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = 0;
        }
    }
}
