using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Code.Map
{
    [ExecuteAlways]
    [RequireComponent(typeof(DecalProjector))]
    public class MapTileVisual : MonoBehaviour
    {
        [SerializeField] private Material walkableMat;
        [SerializeField] private Material nonWalkableMat;
        [SerializeField] private Material enemyMat;
        [SerializeField] private Material obstacleMat;

        private DecalProjector _decalProjector;
        private TileOverlayType _overlayType;
        private bool _isVisible = true;

        private void Awake()
        {
            _decalProjector = GetComponent<DecalProjector>();
            _decalProjector.enabled = false;
        }

        public void Initialize(Material walkable, Material nonWalkable, Material enemy, Material obstacle, float projectionDepth, uint renderingLayerMask)
        {
            walkableMat = walkable;
            nonWalkableMat = nonWalkable;
            enemyMat = enemy;
            obstacleMat = obstacle;

            Vector3 size = _decalProjector.size;
            size.z = projectionDepth;
            
            _decalProjector.size = size;
            _decalProjector.pivot = new Vector3(0f, 0f, projectionDepth * 0.5f);

            _decalProjector.renderingLayerMask = renderingLayerMask;
            _decalProjector.enabled = false;
        }

        public void SetDecalActive(bool isActive)
        {
            if (_decalProjector == null)
                return;

            _isVisible = isActive;
            _decalProjector.enabled = _isVisible && _overlayType != TileOverlayType.None;
        }

        public void SetOverlay(TileOverlayType overlayType)
        {
            _overlayType = overlayType;

            if (_decalProjector == null)
                return;

            _decalProjector.material = GetOverlayMaterial();
            _decalProjector.enabled = _isVisible && overlayType != TileOverlayType.None;
        }

        public void ClearOverlay()
        {
            _overlayType = TileOverlayType.None;

            if (_decalProjector == null)
                return;

            _decalProjector.enabled = false;
        }

        private Material GetTileMaterial(MapTile tile)
        {
            if (tile.HasState(TileState.Enemy))
                return enemyMat;
            
            if (tile.HasState(TileState.Obstacle))
                return obstacleMat;
            
            if (!tile.HasState(TileState.Walkable))
                return nonWalkableMat;
            
            return walkableMat;
        }

        private Material GetOverlayMaterial()
        {
            return _overlayType switch
            {
                TileOverlayType.Move => walkableMat,
                TileOverlayType.Attack => enemyMat,
                TileOverlayType.Blocked => obstacleMat,
                TileOverlayType.Target => nonWalkableMat,
                _ => walkableMat
            };
        }

        public void HandleTileChanged(MapTile tile)
        {
            if (_decalProjector == null)
                return;

            if (_overlayType == TileOverlayType.None)
                _decalProjector.material = GetTileMaterial(tile);
        }
    }
}