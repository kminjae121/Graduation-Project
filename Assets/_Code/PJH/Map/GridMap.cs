using Code.Core;
using Code.Core.Interfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Code.Map
{
    public class GridMap : MonoSingleton<GridMap>, IGridMap
    {
        [Header("Map Settings")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private float tileSize = 30f;

        [Header("Tile Prefab")]
        [SerializeField] private GameObject tilePrefab;

        [Header("Decal Settings")]
        [SerializeField] private Material walkableMaterial;
        [SerializeField] private Material nonWalkableMaterial;
        [SerializeField] private Material enemyMaterial;
        [SerializeField] private Material obstacleMaterial;
        [SerializeField] private float decalHeight = 1f;
        [SerializeField] private float projectionDepth = 1f;
        [SerializeField] private uint decalRenderingLayerMask = 6;

        [SerializeField, HideInInspector] private MapTile[] serializedTiles;

        private MapTile[,] tiles;

        public int Width => width;
        public int Height => height;
        public float TileSize => tileSize;

        protected override void Awake()
        {
            RebuildTileArray();
        }

        private void RebuildTileArray()
        {
            if (serializedTiles == null || serializedTiles.Length == 0)
                return;

            tiles = new MapTile[width, height];

            int count = Mathf.Min(serializedTiles.Length, width * height);

            for (int i = 0; i < count; ++i)
            {
                var tile = serializedTiles[i];
                
                if (tile == null)
                    continue;

                int x = i % width;
                int y = i / width;

                tiles[x, y] = tile;
            }
        }

        public void GenerateMap()
        {
            ClearMap();

            tiles = new MapTile[width, height];
            serializedTiles = new MapTile[width * height];

            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                    CreateTile(x, y);
        }

        private void CreateTile(int x, int y)
        {
            Vector3 worldPosition = GridToWorldPos(x, y);

            GameObject tileObject;

            if (tilePrefab != null)
            {
#if UNITY_EDITOR
                tileObject = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(tilePrefab, transform);
                tileObject.transform.position = worldPosition;
#else
                tileObject = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);
#endif
            }
            else
            {
                tileObject = new GameObject($"Tile_{x}_{y}")
                {
                    transform =
                    {
                        position = worldPosition,
                        parent = transform
                    }
                };
            }

            var tile = tileObject.GetComponent<MapTile>();

            if (tile == null)
                tile = tileObject.AddComponent<MapTile>();

            tile.Initialize(new Vector2Int(x, y));

            tiles[x, y] = tile;
            serializedTiles[y * width + x] = tile;

            CreateDecal(tileObject, x, y);
        }

        private void CreateDecal(GameObject tileObject, int x, int y)
        {
            var decalObject = new GameObject("Decal")
            {
                transform =
                {
                    parent = tileObject.transform,
                    localPosition = Vector3.up * decalHeight,
                    localRotation = Quaternion.Euler(90f, 0f, 0f)
                }
            };

            var projector = decalObject.AddComponent<DecalProjector>();

            projector.size = new Vector3(tileSize * 0.9f, tileSize * 0.9f, projectionDepth);
            projector.pivot = new Vector3(0f, 0f, projectionDepth * 0.5f);

            var visual = decalObject.AddComponent<MapTileVisual>();

            visual.Initialize(walkableMaterial, nonWalkableMaterial, enemyMaterial, obstacleMaterial,
                projectionDepth, decalRenderingLayerMask);
        }

        private void ClearMap()
        {
            if (serializedTiles != null)
                foreach (var tile in serializedTiles)
                    if (tile != null)
                        DestroyImmediate(tile.gameObject);

            int childCount = transform.childCount;

            for (int i = childCount - 1; i >= 0; --i)
                DestroyImmediate(transform.GetChild(i).gameObject);

            tiles = null;
            serializedTiles = null;
        }

        public void SetGridVisible(bool visible)
        {
            if (tiles == null)
                RebuildTileArray();

            if (tiles == null)
                return;

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                    tiles[x, y]?.SetDecalActive(visible);
        }

        public Vector3 GridToWorldPos(int x, int y)
        {
            return new Vector3(x * tileSize, 0f, y * tileSize) + transform.position;
        }

        public Vector2Int WorldToGridPos(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - transform.position;

            int x = Mathf.RoundToInt(localPos.x / tileSize);
            int y = Mathf.RoundToInt(localPos.z / tileSize);

            return new Vector2Int(x, y);
        }

        public IMapTile GetTile(Vector2Int position)
        {
            return GetTile(position.x, position.y);
        }

        public IMapTile GetTile(int x, int y)
        {
            if (!IsValidPosition(new Vector2Int(x, y)))
                return null;

            if (tiles == null)
                RebuildTileArray();

            return tiles?[x, y];
        }

        public bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < width &&
                   position.y >= 0 && position.y < height;
        }

        public bool CanMoveTo(Vector2Int position)
        {
            IMapTile tile = GetTile(position);
            return tile != null &&
                   !tile.HasState(TileState.Obstacle) &&
                   !tile.HasState(TileState.Enemy);
        }
    }
}