using System.Collections.Generic;
using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class RogueShadowSpawn : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private List<RogueShadow> shadows = new List<RogueShadow>();
        [SerializeField] private int maxShadowCnt = 3;
        private RogueShadow _currentShadowObj;

        private int _shadowCnt = 0;
        private int _currentIdx = 0;
        private int _maxIdx = 0;

        private CharacterUnit _unit;

        public void Initialize(Unit owner)
        {
            _maxIdx = shadows.Count;
            _currentIdx = 0;
            _shadowCnt = 0;

            foreach (var shadow in shadows)
                shadow.gameObject.SetActive(false);

            _unit = owner as CharacterUnit;
        }

        public int GetMaxShadowCnt() => maxShadowCnt;

        public int GetShadowCnt() => _shadowCnt;

        public int GetCurrentShadowIdx() => _currentIdx;

        public List<RogueShadow> GetShadows() => shadows;
        
        public RogueShadow GetCurrentShadow() => _currentShadowObj;

        public void SetShadowInfo(RogueShadow shadow, bool active)
        {
            foreach (var shadowInfo in shadows)
            {
                if(shadowInfo == shadow)
                        shadowInfo.gameObject.SetActive(active);

                if (active == false)
                {
                    _currentIdx -= 1;
                }
            }
        }

        public void SetShadow(Transform trm)
        {
            if (_maxIdx <= 0) return; 

            var shadow = shadows[_currentIdx];
            shadow.gameObject.SetActive(true);
            shadow.SetPos(_unit.transform.position);
            _currentShadowObj = shadow;

            if (_shadowCnt < maxShadowCnt)
                _shadowCnt++;
            
            Bus<RogueGimicBarEvent>.Raise(new RogueGimicBarEvent(_shadowCnt));
            
            _currentIdx++;
            if (_currentIdx >= _maxIdx)
                _currentIdx = 0;
        }

        public void ResetAllShadow()
        {
            foreach (var shadow in shadows)
                shadow.gameObject.SetActive(false);

            _currentIdx = 0;
            _shadowCnt = 0;
        }
    }
}