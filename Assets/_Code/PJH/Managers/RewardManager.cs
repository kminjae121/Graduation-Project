using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.Items;
using Code.Tower;
using Code.UI;
using UnityEngine;

namespace Code.Managers
{
    public class RewardManager : MonoBehaviour
    {
        [Header("Rewards Data")]
        [SerializeField] private List<ItemSO> itemList;
        
        [Header("UI References")]
        [SerializeField] private BattleRewardUI battleRewardUI;
        
        private void Awake()
        {
            Bus<StageClearEvent>.Subscribe(HandleStageClear);
        }

        private void OnDestroy()
        {
            Bus<StageClearEvent>.Unsubscribe(HandleStageClear);
        }

        private void HandleStageClear(StageClearEvent evt)
        {
            List<ItemSO> rewardItems = new();

            if (battleRewardUI == null)
                battleRewardUI = FindAnyObjectByType<BattleRewardUI>(FindObjectsInactive.Include);

            if (battleRewardUI == null)
            {
                Debug.LogWarning("[RewardManager] BattleRewardUI is not assigned. Returning to tower map.");
                TowerRunSession.CompleteCurrentRoom();
                TowerSceneLoader.LoadScene(TowerRunSession.TowerSceneName);
                return;
            }

            if (itemList == null || itemList.Count == 0)
            {
                Debug.LogWarning("보상 아이템 리스트가 비어있습니다.");
                battleRewardUI.Open(rewardItems);
                return;
            }

            rewardItems.Add(itemList[Random.Range(0, itemList.Count)]);
            rewardItems.Add(itemList[Random.Range(0, itemList.Count)]);
            
            battleRewardUI.Open(rewardItems);
        }
    }
}
