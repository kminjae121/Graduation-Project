using UnityEngine;

namespace Code.Core.Interfaces
{
    public interface IGridMap
    {
        int Width { get; }
        int Height { get; }
    
        IMapTile GetTile(Vector2Int position);
        IMapTile GetTile(int x, int y);
        bool IsValidPosition(Vector2Int position);
        bool CanMoveTo(Vector2Int position);
    }
}