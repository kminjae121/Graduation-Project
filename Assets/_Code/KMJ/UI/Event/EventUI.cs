using System.Collections.Generic;
using Code.Core.Managers;
using Code.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Code.Core.Events.Bus;
using Code.UnitManaging;

namespace Code.UI
{
    public class EventUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mainTxt;
        [SerializeField] private TextMeshProUGUI selectBtnTxt;
        [SerializeField] private TextMeshProUGUI skipBtnTxt;
        [SerializeField] private TextMeshProUGUI popUpTxt;
        [SerializeField] private Image eventImg;
        [SerializeField] private GameObject evtObject;

        [SerializeField] private Button skipBtn;
        [SerializeField] private Button selectBtn;
        

        [SerializeField] private float activeTime = 1;

        [SerializeField] private List<EventTextSO> eventTexts;

        [SerializeField] private Image thisObjectImg;

        [SerializeField] private UnitStorageSO storageSO;
        
        private void OnEnable()
        {
            int randValue = Random.Range(0, eventTexts.Count);
            thisObjectImg = GetComponent<Image>();
            
            selectBtnTxt.text = eventTexts[randValue].ApplyTxt;
            skipBtnTxt.text = eventTexts[randValue].CancelTxt;
            
            eventImg.sprite = eventTexts[randValue].EventImg;

            DOTween.Sequence()
                .Append(popUpTxt.transform.DOScale(1, 1f))
                .Append(popUpTxt.DOFade(0, 0.6f))
                .Append(thisObjectImg.DOFade(1, 0.4f))
                .Append(eventImg.DOFade(1, 1f))
                .Append(mainTxt.DoText(eventTexts[randValue].MainTxt, activeTime))
                .Append(selectBtn.transform.DOScale(1, 0.5f))
                .Append(skipBtn.transform.DOScale(1, 0.5f));
            
            int randomValue = Random.Range(0, 3);
            
            skipBtn.onClick.AddListener(() => HandleSkipBtn(randValue));
            selectBtn.onClick.AddListener(() =>HandleSelectBtn(randomValue, randValue));
        }

        private void HandleSkipBtn(int value)
        {
            skipBtn.gameObject.SetActive(false);
            selectBtn.gameObject.SetActive(false);
            DOTween.Sequence()
                .Append(mainTxt.DoText(eventTexts[value].SkipTxt, activeTime))
                .AppendInterval(0.5f)
                .Append(eventImg.DOFade(0, 0.5f))
                .Append(mainTxt.RemoveText( 0.5f))
                .AppendInterval(0.3f)
                .Append(thisObjectImg.DOFade(0, 1f))
                .OnComplete(() => 
                {
                    DOTween.KillAll();
                    Bus<StageClearEvent>.Raise(new StageClearEvent(true));
                    evtObject.SetActive(false);
                });
        }

        private void OnDisable()
        {
            skipBtn.onClick.RemoveAllListeners();
            selectBtn.onClick.RemoveAllListeners();
        }

        public void HandleSelectBtn(int value, int randomValue)
        {
            if (value == 1)
            {
                storageSO.unitStates.ForEach(state =>
                {
                    state.TakeDamage(eventTexts[randomValue].value);
                });
                
                mainTxt.text = eventTexts[randomValue].FailTxt;
                
                skipBtn.gameObject.SetActive(false);
                selectBtn.gameObject.SetActive(false);
                DOTween.Sequence()
                    .Append(mainTxt.DoText(eventTexts[randomValue].FailTxt, activeTime))
                    .AppendInterval(0.3f)
                    .Append(eventImg.DOFade(0, 0.5f))
                    .Append(mainTxt.RemoveText( 0.5f))
                    .AppendInterval(0.2f)
                    .Append(thisObjectImg.DOFade(0, 0.5f))
                    .OnComplete(() => 
                    {
                        DOTween.KillAll();
                        Bus<StageClearEvent>.Raise(new StageClearEvent(true));
                        evtObject.SetActive(false);
                    });
                
                
            }
            else
            {
                mainTxt.text = eventTexts[randomValue].SuccessTxt;
                
                storageSO.unitStates.ForEach(state =>
                {
                    state.Heal(eventTexts[randomValue].value);
                });
                skipBtn.gameObject.SetActive(false);
                selectBtn.gameObject.SetActive(false);
                DOTween.Sequence()
                    .Append(mainTxt.DoText(eventTexts[randomValue].SuccessTxt, activeTime))
                    .AppendInterval(0.3f)
                    .Append(eventImg.DOFade(0, 0.5f))
                    .Append(mainTxt.RemoveText( 0.5f))
                    .AppendInterval(0.2f)
                    .Append(thisObjectImg.DOFade(0, 0.5f))
                    .OnComplete(() => 
                    {
                        DOTween.KillAll();
                        Bus<StageClearEvent>.Raise(new StageClearEvent(true));
                        evtObject.SetActive(false);
                    });
                 
            }
        }
        
    }
}