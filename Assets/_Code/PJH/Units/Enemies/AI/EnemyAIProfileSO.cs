using UnityEngine;

namespace Code.UnitSystem.Enemies.AI
{
    public enum EnemyCombatStyle
    {
        Melee,
        Ranged
    }

    [CreateAssetMenu(menuName = "SO/Enemy/AI Profile")]
    public sealed class EnemyAIProfileSO : ScriptableObject
    {
        public EnemyCombatStyle Style = EnemyCombatStyle.Melee;
        public bool Kite;
        public float DmgWeight = 1f;
        public float PrioWeight = 10f;
        public float PosWeight = 15f;
        public float ClosePenalty = 1000f;

        public bool WantsSpace =>
            Kite && Style == EnemyCombatStyle.Ranged;
    }
}
