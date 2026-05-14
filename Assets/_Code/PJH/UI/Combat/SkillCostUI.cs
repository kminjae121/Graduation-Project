using Code.UnitSystem;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class SkillCostUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image turnCostGaugeImage;
        [SerializeField] private Image showCostImg;
        [SerializeField] private TextMeshProUGUI costText;

        private UnitSkillCost _skillCostCompo;
        
        [Header("Settings")]
        [SerializeField] private float gaugeTweenTime = 0.3f;

        private Tween _gaugeTween;
        private Tween _showCostTween;
        
        public void SetSkillCostCompo(UnitSkillCost costCompo)
        {
            if(_skillCostCompo != null)
                _skillCostCompo.skillCostChanged.RemoveAllListeners();
            
            _skillCostCompo = costCompo;
            _skillCostCompo.skillCostChanged.AddListener(RefreshGauge);
            _skillCostCompo.skillCostChanged?.Invoke(_skillCostCompo.GetUnitSkillCost());
        }

        public void SetShowGauge(int value)
        {
            int max = _skillCostCompo.GetMaxSkillCost();

            float targetFill = (max <= 0) ? 0f : (float)value / max;
            targetFill = Mathf.Clamp01(targetFill);

            _showCostTween?.Kill();
            _showCostTween = showCostImg
                .DOFillAmount(targetFill, gaugeTweenTime)
                .SetEase(Ease.OutCubic);
        }

        public void ReturnShowFilled()
        {
            showCostImg.fillAmount = turnCostGaugeImage.fillAmount;
        }


        private void RefreshGauge(int value)
        {
            int max = _skillCostCompo.GetMaxSkillCost();
            costText.text = $"{value} / {max}";

            float targetFill = (max <= 0) ? 0f : (float)value / max;
            targetFill = Mathf.Clamp01(targetFill); 

            _gaugeTween?.Kill();
            _gaugeTween = turnCostGaugeImage
                .DOFillAmount(targetFill, gaugeTweenTime)
                .SetEase(Ease.OutCubic);
            showCostImg.fillAmount = targetFill;
        }
    }
}