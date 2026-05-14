using Code.Map;
using Code.UnitSystem;
using UnityEngine;

namespace Code.Core.Interfaces
{
    public interface IMapTile
    {
        Vector2Int GridPos { get; }
        Vector3 WorldPos { get; }

        Unit GetTileUnit();
        void SetTileUnit(Unit unit);
        
        bool HasState(TileState state);
        bool HasAnyState(TileState state);

        void SetState(TileState state, bool value);

        void SetDecalActive(bool isActive);
        void SetOverlay(TileOverlayType overlayType);
        void ClearOverlay();
    }
}