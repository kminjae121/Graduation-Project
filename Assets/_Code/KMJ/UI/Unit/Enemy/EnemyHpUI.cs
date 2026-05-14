using Code.Core.Events.Bus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class EnemyHpUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI damageTxt;
        [SerializeField] private TextMeshProUGUI damageInfoTxt;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider hpSlider2;
        [SerializeField] private GameObject _enemyInfo;
        [SerializeField] private GameObject _enemybasicInfo;
        [SerializeField] private TextMeshProUGUI atkInfo;
        [SerializeField] private TextMeshProUGUI currentHealhtxt;

        [SerializeField] private Image enemyImage;
        [SerializeField] private Image enemyImage2;
        
        private void Awake()
        {
            Bus<EnemyHpInfo>.Subscribe(SetHp);
        }

        private void OnDisable()
        {
            Bus<EnemyHpInfo>.Unsubscribe(SetHp);
        }

        public void SetHp(EnemyHpInfo evt)
        {
            if (evt.isActive == false)
            {
                _enemyInfo.SetActive(false);    
                _enemybasicInfo.SetActive(false);
            }
            else if(evt.isAttack)
            {
                _enemyInfo.SetActive(true);
                _enemybasicInfo.SetActive(false);

                float hp = evt.hp - evt.damage - evt.plusDamage;

                if (hp <= 0)
                {
                    hp = 0;
                }

                enemyImage.sprite = evt.sprite;
                
                damageTxt.text = $"{hp}";
                damageInfoTxt.text = $"{evt.hp} - ({evt.damage} + {evt.plusDamage})";

                hpSlider.value = evt.lastValue;   
            }
            else if(!evt.isAttack)
            {
                _enemybasicInfo.SetActive(true);
                _enemyInfo.SetActive(false);

                enemyImage2.sprite = evt.sprite;

                atkInfo.text = $"{evt.atkDamage}";
                currentHealhtxt.text = $"{evt.hp}";

                hpSlider2.value = evt.lastValue;  
            }
        }
    }
}