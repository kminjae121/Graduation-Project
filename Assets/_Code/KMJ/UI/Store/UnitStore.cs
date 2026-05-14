using System.Collections.Generic;
using Code.Core.Managers;
using _Code.KMJ.UnitSystem.involveUnitSO;
using Code.Core.Events.Bus;
using Code.Items;
using Code.Managers;
using Code.SkillSystem;
using DG.Tweening;
using Input;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.UI   
{
    public class UnitStore : MonoBehaviour
    {
        private int skillCount = 5;
        private int itemCount = 3;
        [SerializeField] private GameObject storePanelObject;
        [SerializeField] private GameObject storeObject;

        [SerializeField] private Transform storePos;
        [SerializeField] private Transform upPos;
        
        [SerializeField] private InputReader input;
        
        [SerializeField] private List<SkillSO> skills;

        [SerializeField] private HavingSkillSO havingSkillSO;

        [SerializeField] private List<ItemSO> items;

        [SerializeField] private TextMeshProUGUI goldTxt;


        [SerializeField] private StoreSkillBtn skillUI;

        [SerializeField] private StoreItemBtn itemUI;

        private List<StoreSkillBtn> skillBtns = new List<StoreSkillBtn>();
        private List<StoreItemBtn> itemBtns = new List<StoreItemBtn>();

        private void OnEnable()
        {
            skills.RemoveAll(skill => havingSkillSO.HaveSkills.Contains(skill));

            storeObject.transform.DOMove(storePos.position, 1f);
            Show();
            
            RandomChild();

            input.OnCancelEvent += CancelUI;

            goldTxt.text = $"골드 : {PlayerManager.Instance.Gold.ToString()}";    
        }

        private void OnDisable()
        {
            input.OnCancelEvent -= CancelUI;
        }

        public void CancelUI()
        {
            DOTween.Sequence()
                .Append(storeObject.transform.DOMove(upPos.position, 1f))
                .OnComplete(() =>
                {
                    storePanelObject.SetActive(false);
                });
           
            GoodsManager.Instance.AddSkill();
            Bus<StageClearEvent>.Raise(new StageClearEvent(true));
        }


        private void RandomChild()
        {
            var parent = transform;
            int n = parent.childCount;

            for (int i = 0; i < n; i++)
            {
                parent.GetChild(0).SetSiblingIndex(Random.Range(0, n));
            }
        }
        
        public void Show()
        {
            int[] randomIdx = SetRandomIdxSkill();
            int[] randomItemIdx = SetRandomIdxItem();
            
            SpawnItem(randomItemIdx,randomIdx);
        }

        private void SpawnItem(int[] itemRandom, int[] skillRandom)
        {
            for (int i = 0; i < skillCount; i++)
            {
                StoreSkillBtn skillBtn = Instantiate(skillUI,transform);
                
                skillBtns.Add(skillBtn);
            }

            for (int i = 0; i < itemCount; i++)
            {
                StoreItemBtn itemBtn = Instantiate(itemUI, transform);
                
                itemBtns.Add(itemBtn);
            }

            SetSkillUI(skillRandom);
            SetItemUI(itemRandom);
        }

        private int[] SetRandomIdxSkill()
        {
            int maxCount = skills.Count;
            
            int[] idx = new int[10];

            if (maxCount <= 0)
                return idx;
            
            int pickCount = Mathf.Min(maxCount, 5);
            
            int[] pool = new int[maxCount];
            
            for (int i = 0; i < maxCount; i++)
                pool[i] = i;
            
            for (int i = 0; i < pickCount; i++)
            {
                int j = Random.Range(i, maxCount); 
                (pool[i], pool[j]) = (pool[j], pool[i]);
                idx[i] = pool[i];
            }

            return idx;
        }
        
        private int[] SetRandomIdxItem()
        {
            int maxCount = items.Count;
            
            int[] idx = new int[10];

            if (maxCount <= 0)
                return idx;
            
            int pickCount = Mathf.Min(maxCount, 5);
            
            int[] pool = new int[maxCount];
            
            for (int i = 0; i < maxCount; i++)
                pool[i] = i;
            
            for (int i = 0; i < pickCount; i++)
            {
                int j = Random.Range(i, maxCount); 
                (pool[i], pool[j]) = (pool[j], pool[i]);
                idx[i] = pool[i];
            }

            return idx;
        }
        
        private void SetItemUI(int[] randomIdx)
        {
            for (int i = 0; i < itemBtns.Count; i++)
            {
                if (i >= items.Count)
                {
                    itemBtns[i].gameObject.SetActive(false);
                    continue;
                }
                
                itemBtns[i].SetItem(items[randomIdx[i]],goldTxt);
            }
        }
        
        private void SetSkillUI(int[] ran)
        {
            for (int i = 0; i < skillBtns.Count; i++)
            {
                if (i >= skills.Count)
                {
                    skillBtns[i].gameObject.SetActive(false);
                    continue;
                }
                
                skillBtns[i].SetSkill(skills[ran[i]], goldTxt);
            }
        }
    }
}