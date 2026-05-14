using DG.Tweening;
using TMPro;

namespace Code.Core.Managers
{
    public static class TMPTween
    {
        public static Tweener DoText(this TextMeshProUGUI thisTmp, string text, float duration)
        {
            int length = 0;
            
            return DOTween.To(
                () => length,
                x =>
                {
                    length = x;
                    thisTmp.text = text.Substring(0, length);
                },
                text.Length,
                duration
            ).SetEase(Ease.Linear);
        }

        public static Tween RemoveText(this TextMeshProUGUI thisTmp, float duration)
        {
            string text = thisTmp.text;
            
            int length = text.Length;

            return DOTween.To(
                () => length,
                x =>
                {
                    length = x;
                    thisTmp.text = text.Remove(length - 1);
                },
                0,
                duration
            ).SetEase(Ease.Linear);
        }
    }
}