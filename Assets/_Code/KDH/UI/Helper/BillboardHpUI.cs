using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.UnitSystem.Combat;

namespace Code.UI
{
    public class BillboardHpUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnitHealth targetHealth;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private TextMeshProUGUI hpText;

        private Camera _mainCam;

        private void Start()
        {
            _mainCam = Camera.main;

            if (targetHealth == null)
            {
                targetHealth = GetComponentInParent<UnitHealth>();
            }

            if (targetHealth != null)
            {
                targetHealth.OnHealthChangedEvent += UpdateHpUI;
                UpdateHpUI(targetHealth.CurrentHealth, targetHealth.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChangedEvent -= UpdateHpUI;
            }
        }

        private void LateUpdate()
        {
            if (_mainCam != null)
            {
                transform.rotation = _mainCam.transform.rotation;
            }
        }

        private void UpdateHpUI(float currentHp, float maxHp)
        {
            if (hpFillImage != null && maxHp > 0)
            {
                hpFillImage.fillAmount = currentHp / maxHp;
            }

            if (hpText != null)
            {
                hpText.text = $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
            }
        }
    }
}