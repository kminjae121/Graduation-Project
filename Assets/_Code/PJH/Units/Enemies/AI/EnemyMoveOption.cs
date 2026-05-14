using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public readonly struct EnemyMoveOption
    {
        public readonly Unit Target;
        public readonly Vector2Int Tile;
        public readonly float SkillScore;
        public readonly float PosScore;
        public readonly int Cost;
        public readonly float Distance;

        public EnemyMoveOption(Unit target, Vector2Int tile, float skillScore, float posScore, int cost, float distance)
        {
            Target = target;
            Tile = tile;
            SkillScore = skillScore;
            PosScore = posScore;
            Cost = cost;
            Distance = distance;
        }

        public bool IsValid => Target != null;
    }
}