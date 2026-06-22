using System;
using System.Collections;
using System.Collections.Generic;
using Code.Core;
using Code.Core.Managers;
using Code.Item;
using Code.Items;
using Code.Tower;
using Code.UnitSystem;
using Code.UnitSystem.ArtifactSystem;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Code.KMJ
{
    public class RewardCompo : MonoBehaviour
    {
        [SerializeField] private UnitAnimationTrigger triggerCompo;

        [SerializeField] private ArtifactStorageSO artifactStorage;
        [SerializeField] private List<EquipmentItemSO> equippedItems;

        [SerializeField] private ParticleSystem particleSystem;
        [SerializeField] private ParticleSystem particleSystem2;
        [SerializeField] private ParticleSystem particleSystem3;
        [SerializeField] private ParticleSystem particleSystem4;

        [Header("UI")] [SerializeField] private List<GameObject> uis;
        [SerializeField] private List<Image> itemImgs;
        [SerializeField] private List<TextMeshProUGUI> itemNameTxts;
        [SerializeField] private List<TextMeshProUGUI> itemStats;

        [Header("Button")] [SerializeField] private List<Button> btns;

        [Header("Reward")]
        [SerializeField] private RectTransform rewardUI;
        [SerializeField] private RectTransform uiTrm;
        
        private EquipmentItemSO item1 = null;
        private EquipmentItemSO item2 = null;
        
        
        private void Awake()
        {
            triggerCompo.OnAnimationEndTrigger += BoxOpen;
            uis[0].SetActive(false);
            uis[1].SetActive(false);
            
            btns[0].onClick.AddListener(ClickFirst);
            btns[1].onClick.AddListener(ClickSecond);
        }

        private void OnDestroy()
        {
            triggerCompo.OnAnimationEndTrigger -= BoxOpen;
            btns[0].onClick.RemoveListener(ClickFirst);
            btns[1].onClick.RemoveListener(ClickSecond);
        }

        private void BoxOpen()
        {
            StartCoroutine(RandomItem());
        }

        private IEnumerator RandomItem()
        {
            yield return new WaitForSeconds(0.5f);  
            SoundManager.Instance.PlayClip("ChestSound");
            yield return new WaitForSeconds(0.5f);
            particleSystem.Play();
            yield return new WaitForSeconds(0.45f);
            particleSystem2.Play();
            particleSystem3.Play();
            particleSystem4.Play();

            yield return new WaitForSeconds(0.5f);
            int rand = Random.Range(0, equippedItems.Count);
            int rand2 = Random.Range(0, equippedItems.Count);
            
            while (rand == rand2)
            {
                rand = Random.Range(0, equippedItems.Count);
                rand2 = Random.Range(0, equippedItems.Count);

                if (rand != rand2)
                    break;
            }
            
            item1 =  equippedItems[rand];
            item2 =  equippedItems[rand2];

            itemImgs[0].sprite = item1.itemIcon;
            itemImgs[1].sprite = item2.itemIcon;
            
            itemNameTxts[0].text = item1.itemName;
            itemNameTxts[1].text = item2.itemName;

            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    switch (item1.Stats.Count)
                    {
                        case 4:
                            itemStats[i].text = 
                                $"{item1.Stats[3].StatInfo} : {item1.Stats[3].StatValue}\n{item1.Stats[2].StatInfo} : {item1.Stats[2].StatValue}\n{item1.Stats[1].StatInfo} : {item1.Stats[1].StatValue}\n{item1.Stats[0].StatInfo} x: {item1.Stats[0].StatValue}";
                            break;
                        case 3:
                            itemStats[i].text = 
                                $"{item1.Stats[2].StatInfo} : {item1.Stats[2].StatValue}\n{item1.Stats[1].StatInfo} : {item1.Stats[1].StatValue}\n{item1.Stats[0].StatInfo} : {item1.Stats[0].StatValue}";
                            break;
                        case 2:
                            itemStats[i].text = $"{item1.Stats[1].StatInfo} : {item1.Stats[1].StatValue}\n{item1.Stats[0].StatInfo} : {item1.Stats[0].StatValue}";
                            break;
                        case 1:
                            itemStats[i].text = $"{item1.Stats[0].StatInfo} : {item1.Stats[0].StatValue}";
                            break;
                        default:
                            break;
                    }   
                }
                
                if (i == 1)
                {
                    switch (item2.Stats.Count)
                    {
                        case 4:
                            itemStats[i].text = 
                                $"{item2.Stats[3].StatInfo} : {item2.Stats[3].StatValue}\n{item2.Stats[2].StatInfo} : {item2.Stats[2].StatValue}\n{item2.Stats[1].StatInfo} : {item2.Stats[1].StatValue}\n{item2.Stats[0].StatInfo} : {item2.Stats[0].StatValue}";
                            break;
                        case 3:
                            itemStats[i].text = 
                                $"{item2.Stats[2].StatInfo} : {item2.Stats[2].StatValue}\n{item2.Stats[1].StatInfo} : {item2.Stats[1].StatValue}\n{item2.Stats[0].StatInfo} : {item2.Stats[0].StatValue}";
                            break;
                        case 2:
                            itemStats[i].text = $"{item2.Stats[1].StatInfo} : {item2.Stats[1].StatValue}\n{item2.Stats[0].StatInfo} : {item2.Stats[0].StatValue}";
                            break;
                        case 1:
                            itemStats[i].text = $"{item2.Stats[0].StatInfo} : {item2.Stats[0].StatValue}";
                            break;
                        default:
                            break;
                    }   
                }
            }
            
            uis[0].SetActive(true);
            uis[1].SetActive(true);

            DOTween.KillAll();
            rewardUI.DOMove(uiTrm.position, 0.5f);
        }

        private void ClickFirst()
        {
            artifactStorage.artifacts.Add(item1);
            item1 = null;
            item2 = null;
            uis[0].SetActive(false);
            uis[1].SetActive(false);
            
            DOTween.KillAll();
            TowerRunSession.CompleteCurrentRoom();
            SceneChangeManager.Instance.ChangeSelectScene("TowerMapScene");
        }

        private void ClickSecond()
        {
            artifactStorage.artifacts.Add(item2);
            item1 = null;
            item2 = null;
            uis[0].SetActive(false);
            uis[1].SetActive(false);
            
            DOTween.KillAll();
            TowerRunSession.CompleteCurrentRoom();
            SceneChangeManager.Instance.ChangeSelectScene("TowerMapScene");
        }
    }
}
