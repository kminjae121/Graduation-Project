using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using Code.Map;
using Code.UI;
using Code.UnitSystem;
using Code.UnitSystem.Enemies;
using EnemySystem;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.Core.Managers
{
    public class StageManager : MonoBehaviour
    {
        [System.Serializable]
        public struct EnemySpawnData
        {
            public PoolingItemSO enemyPrefab;
            public Vector2Int spawnCoord;
        }

        [Header("Enemy Spawning")] [SerializeField]
        private List<EnemySpawnData> enemySpawns = new();

        [Header("State")] [SerializeField] private List<GameObject> enemies = new();

        public int playerCount;
        public static StageManager Instance { get; private set; }

        [SerializeField] private GameObject gameClearUI;
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private GameObject cam;

        [Inject] private PoolManagerMono _poolManager;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            foreach (var data in enemySpawns)
            {
                if (data.enemyPrefab == null)
                    continue;

                IMapTile tile = GridMap.Instance.GetTile(data.spawnCoord);

                if (tile == null)
                {
                    Debug.LogWarning($"적 스폰 좌표 {data.spawnCoord}가 유효하지 않습니다.");
                    continue;
                }

                Vector3 spawnPos = GridMap.Instance.GridToWorldPos(data.spawnCoord.x, data.spawnCoord.y);
                
                GameObject enemyObj = _poolManager.Pop<Unit>(data.enemyPrefab).gameObject;

                enemyObj.transform.position = spawnPos;
                
                enemyObj.transform.rotation = Quaternion.identity;

                tile.SetState(TileState.Enemy | TileState.Obstacle, true);
                AbstractEnemyUnit enemy = enemyObj.GetComponent<AbstractEnemyUnit>();
                
                if (enemy != null)
                    Injector.InjectInto(enemy);

                Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(enemy));

                enemies.Add(enemyObj);
            }
        }

        public void RemoveEnemy(GameObject enemy)
        {
            if (enemies.Contains(enemy))
                enemies.Remove(enemy);

            if (enemies.Count <= 0)
                if (gameClearUI != null)
                {
                    Bus<StageClearEvent>.Raise(new StageClearEvent(true));
                }
        }

        public void AddPlayerCnt()
        {
            playerCount += 1;
        }

        public void PlayerDie()
        {
            playerCount -= 1;

            if (playerCount <= 0 && gameOverUI != null)
                gameOverUI.SetActive(true);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
