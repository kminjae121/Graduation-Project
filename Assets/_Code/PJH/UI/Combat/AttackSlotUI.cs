using Code.SkillSystem;
using GondrLib.ObjectPool.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class AttackSlotUI : MonoBehaviour, IPoolable, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private TextMeshProUGUI skillDescText;
        [SerializeField] private TextMeshProUGUI skillCostText;
        [SerializeField] private Image skillIcon;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color baseColor;
        [field: SerializeField] public PoolingItemSO PoolingType { get; private set; }
        
        public GameObject GameObject => gameObject;
        
        private AttackUI _owner;
        private SkillSO _skill;
        private SkillComponent _skillCompo;
        private Button _button;
        
        public RectTransform Rect => transform as RectTransform;
        public SkillSO Skill => _skill;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void Initialize(AttackUI owner)
        {
            _owner = owner;
        }

        public void SetSkill(SkillSO skill, SkillComponent skillCompo)
        {
            _skill = skill;
            _skillCompo = skillCompo;
            
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(HandleSkill);

            if (skillNameText != null)
                skillNameText.text = skill.skillName;

            if (skillDescText != null)
                skillDescText.text = skill.SkillDescription;

            if (skillCostText != null)
                skillCostText.text = $"코스트 - {skill.SkillValue}";

            if (skillIcon != null)
                skillIcon.sprite = skill.skillUIImage;
        }
        
        private void HandleSkill()
        {
            _skillCompo.CancelAllSkill();
            _skillCompo.StartSkill(_skill);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            skillCostText.gameObject.SetActive(true);
            skillDescText.gameObject.SetActive(true);
            skillCostText.color = selectedColor;
            skillDescText.color = selectedColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            skillCostText.gameObject.SetActive(false);
            skillDescText.gameObject.SetActive(false);
            skillCostText.color = baseColor;
            skillDescText.color = baseColor;
        }
        
        public void SetUpPool(GondrLib.ObjectPool.Runtime.Pool pool)
        {
        }

        public void ResetItem()
        {
            skillCostText.gameObject.SetActive(false);
            skillDescText.gameObject.SetActive(false);

            skillCostText.color = baseColor;
            skillDescText.color = baseColor;

            _button.onClick.RemoveAllListeners();

            _skill = null;
            _skillCompo = null;
        }
    }
}