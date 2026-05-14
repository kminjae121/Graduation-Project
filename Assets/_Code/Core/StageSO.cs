using UnityEngine;

namespace Code.Stage
{
    [CreateAssetMenu(fileName = "SO/StageSO", menuName = "SO/Map/StageSO", order = 0)]
    public class StageSO : ScriptableObject
    {
        public int endCount;

        public float behaviorCost;
    }
}