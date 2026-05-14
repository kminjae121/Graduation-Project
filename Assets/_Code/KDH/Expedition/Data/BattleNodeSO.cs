using System.Collections.Generic;
using _00.Core._02.Scripts._06.SO;
using Code.UnitSystem;
using UnityEngine;

namespace Code.Expedition.Data
{
    [CreateAssetMenu(fileName = "NewBattleNode", menuName = "SO/Expedition/BattleNode")]
    public class BattleNodeSO : ExpeditionNodeSO
    {
        [Header("Battle Config")]
        public StageSO stageData;
        public List<UnitSpawnSO> enemiesToSpawn;
        public string battleSceneName = "BattleScene";
        public List<string> battleSceneNames = new();

        private void OnEnable()
        {
            nodeType = ExpeditionNodeType.Battle;
        }

        public string GetRandomBattleSceneName()
        {
            if (battleSceneNames is not { Count: > 0 })
                return battleSceneName;
            
            List<string> validSceneNames = new();

            foreach (var sceneName in battleSceneNames)
                if (!string.IsNullOrWhiteSpace(sceneName))
                    validSceneNames.Add(sceneName);

            if (validSceneNames.Count <= 0)
                return battleSceneName;
            
            int randomIndex = Random.Range(0, validSceneNames.Count);
            return validSceneNames[randomIndex];
        }
    }
}