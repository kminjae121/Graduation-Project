using Code.Core.Interfaces;
using Code.UnitSystem;
using UnityEngine;

namespace Code.Map
{
    public class MapTile : MonoBehaviour, IMapTile
    {
        [SerializeField] private Vector2Int gridPos;
        [SerializeField] private TileState tileState;

        public Vector2Int GridPos => gridPos;
        public Vector3 WorldPos => transform.position;

        private MapTileVisual _visual;

        public Unit _unit;

        private void Awake()
        {
            _visual = GetComponentInChildren<MapTileVisual>();
            RefreshVisual();
        }

        private void OnValidate()
        {
            RefreshVisual();
        }

        public void Initialize(Vector2Int pos)
        {
            gridPos = pos;

            if (tileState == TileState.None)
                tileState = TileState.Walkable;

            RefreshVisual();
        }

        public Unit GetTileUnit()
        {
            return _unit;
        }

        public void SetTileUnit(Unit unit)
        {
            _unit = unit;
        }

        public bool HasState(TileState state)
            => (tileState & state) == state;

        public bool HasAnyState(TileState state)
            => (tileState & state) != 0;

        public void SetState(TileState state, bool value)
        {
            if (value)
                tileState |= state;
            else
                tileState &= ~state;

            RefreshVisual();
        }

        public void SetDecalActive(bool isActive)
        {
            _visual?.SetDecalActive(isActive);
        }

        public void SetOverlay(TileOverlayType overlayType)
        {
            _visual?.SetOverlay(overlayType);
        }

        public void ClearOverlay()
        {
            _visual?.ClearOverlay();
        }

        private void RefreshVisual()
        {
            _visual?.HandleTileChanged(this);
        }
    }
}