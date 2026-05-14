using UnityEngine;

namespace Code.Utils
{
    public static class GridCoordUtils
    {
        public static Vector3Int GridToCell(Vector2Int gridPos)
            => new(gridPos.x, gridPos.y, 0);

        public static Vector2Int CellToGrid(Vector3Int cellPos)
            => new(cellPos.x, cellPos.y);
    }
}
