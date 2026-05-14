using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public readonly struct EnemyMoveTile
    {
        public readonly Vector2Int Pos;
        public readonly int Cost;

        public EnemyMoveTile(Vector2Int pos, int cost)
        {
            Pos = pos;
            Cost = cost;
        }
    }
}