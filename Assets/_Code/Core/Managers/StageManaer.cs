using _00.Core._02.Scripts._06.SO;
using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.Core
{
    public class StageManaer : MonoBehaviour
    {
        [SerializeField] private StageSO stageSO;

        private int _stageClearCount => stageSO.endCount;

        private int _currentStageCount;
        
        private void Awake()
        {
        }

        private void OnDestroy()
        {
        }

        private void Start()
        {
            Bus<GageEvent>.Raise(new GageEvent(stageSO.behaviorCost));
        }

    }
}