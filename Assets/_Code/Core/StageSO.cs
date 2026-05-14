using UnityEngine;

namespace _00.Core._02.Scripts._06.SO
{
    [CreateAssetMenu(fileName = "SO/StageSO", menuName = "SO/Map/StageSO", order = 0)]
    public class StageSO : ScriptableObject
    {
        public int endCount;

        public float behaviorCost;
    }
}