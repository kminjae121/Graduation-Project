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
using VHierarchy.Libs;

namespace Code.UI
{
    public abstract class EventUI : MonoBehaviour
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

        [SerializeField] protected List<EventTextSO> eventTexts;

        [SerializeField] private Image thisObjectImg;

        [SerializeField] protected UnitStorageSO storageSO;
        
        private void OnEnable()
        {
            ResolveEventObject();

            if (eventTexts == null || eventTexts.Count == 0)
            {
                Debug.LogWarning("[EventUI] Event text data is empty.");
                CloseEventUI();
                return;
            }

            ResetVisualState();

            int randValue = Random.Range(0, eventTexts.Count);
            thisObjectImg = GetComponent<Image>();
            selectBtn.gameObject.SetActive(true);
            skipBtn.gameObject.SetActive(true);
            
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
            
            skipBtn.onClick.RemoveAllListeners();
            selectBtn.onClick.RemoveAllListeners();
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
                .Append(mainTxt.DoText(string.Empty, 0))
                .AppendInterval(0.3f)
                .Append(thisObjectImg.DOFade(0, 1f))
                .OnComplete(() => 
                {
                    Bus<StageClearEvent>.Raise(new StageClearEvent(true));
                    CloseEventUI();
                    DOTween.KillAll();
                    SceneChangeManager.Instance.ChangeSelectScene("TowerMapScene");
                });
        }

        private void OnDisable()
        {
            skipBtn.onClick.RemoveAllListeners();
            selectBtn.onClick.RemoveAllListeners();

            DOTween.KillAll();
        }

        protected abstract void Buff(int randValue);

        protected abstract void DeBuff(int randValue);
        
        

        private void HandleSelectBtn(int value, int randomValue)
        {
            if (value == 1)
            {

                DeBuff(randomValue);
                mainTxt.text = eventTexts[randomValue].FailTxt;
                
                skipBtn.gameObject.SetActive(false);
                selectBtn.gameObject.SetActive(false);
                DOTween.Sequence()
                    .Append(mainTxt.DoText(eventTexts[randomValue].FailTxt, activeTime))
                    .AppendInterval(0.3f)
                    .Append(eventImg.DOFade(0, 0.5f))
                    .Append(mainTxt.DoText(string.Empty, 0))
                    .AppendInterval(0.2f)
                    .Append(thisObjectImg.DOFade(0, 0.5f))
                    .OnComplete(() => 
                    {
                        Bus<StageClearEvent>.Raise(new StageClearEvent(true));
                        CloseEventUI();
                        DOTween.KillAll();
                        SceneChangeManager.Instance.ChangeSelectScene("TowerMapScene");
                    });
            }
            else
            {
                mainTxt.text = eventTexts[randomValue].SuccessTxt;
                Buff(randomValue);
                skipBtn.gameObject.SetActive(false);
                selectBtn.gameObject.SetActive(false);
                DOTween.Sequence()
                    .Append(mainTxt.DoText(eventTexts[randomValue].SuccessTxt, activeTime))
                    .AppendInterval(0.3f)
                    .Append(eventImg.DOFade(0, 0.5f))
                    .Append(mainTxt.DoText(string.Empty, 0))
                    .AppendInterval(0.2f)
                    .Append(thisObjectImg.DOFade(0, 0.5f))
                    .OnComplete(() => 
                    {
                        Bus<StageClearEvent>.Raise(new StageClearEvent(true));
                        CloseEventUI();
                        DOTween.KillAll();
                        SceneChangeManager.Instance.ChangeSelectScene("TowerMapScene");
                    });
                 
            }
        }

        private void ResolveEventObject()
        {
            if (evtObject != null)
                return;

            evtObject = transform.parent != null ? transform.parent.gameObject : gameObject;
        }

        private void ResetVisualState()
        {
            if (thisObjectImg == null)
                thisObjectImg = GetComponent<Image>();

            if (mainTxt != null)
            {
                mainTxt.DOKill();
                mainTxt.text = string.Empty;
                SetGraphicAlpha(mainTxt, 1f);
            }

            if (popUpTxt != null)
            {
                popUpTxt.DOKill();
                popUpTxt.transform.DOKill();
                popUpTxt.transform.localScale = Vector3.zero;
                SetGraphicAlpha(popUpTxt, 1f);
            }

            if (thisObjectImg != null)
            {
                thisObjectImg.DOKill();
                SetGraphicAlpha(thisObjectImg, 0f);
            }

            if (eventImg != null)
            {
                eventImg.DOKill();
                SetGraphicAlpha(eventImg, 0f);
            }

            ResetButton(selectBtn);
            ResetButton(skipBtn);
        }

        private static void ResetButton(Button button)
        {
            if (button == null)
                return;

            button.transform.DOKill();
            button.gameObject.SetActive(true);
            button.transform.localScale = Vector3.zero;
        }

        private static void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
                return;

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private void CloseEventUI()
        {
            ResolveEventObject();
            evtObject.SetActive(false);
        }
        
    }
}
