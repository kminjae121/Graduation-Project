using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public readonly struct EnemyMovePick
    {
        public readonly Unit Target;
        public readonly Vector2Int Tile;

        public EnemyMovePick(Unit target, Vector2Int tile)
        {
            Target = target;
            Tile = tile;
        }
    }
}