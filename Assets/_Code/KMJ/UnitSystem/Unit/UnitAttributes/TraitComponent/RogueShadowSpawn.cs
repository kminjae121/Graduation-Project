using System.Collections.Generic;
using UnityEngine;

namespace Code.UnitSystem.UnitAttributes
{
    public class RogueShadowSpawn : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private List<RogueShadow> shadows = new List<RogueShadow>();
        [SerializeField] private int maxShadowCnt = 3;

        private int _shadowCnt = 0;
        private int _currentIdx = 0;
        private int _maxIdx = 0;

        public void Initialize(Unit owner)
        {
            _maxIdx = shadows.Count;
            _currentIdx = 0;
            _shadowCnt = 0;

            foreach (var shadow in shadows)
                shadow.gameObject.SetActive(false);
        }

        public int GetMaxShadowCnt() => maxShadowCnt;

        public int GetShadowCnt() => _shadowCnt;

        public List<RogueShadow> GetShadows() => shadows;

        public void SetShadow(Transform trm)
        {
            if (_maxIdx <= 0) return; 

            var shadow = shadows[_currentIdx];
            shadow.gameObject.SetActive(true);
            shadow.transform.position = trm.position;

            if (_shadowCnt < maxShadowCnt)
                _shadowCnt++;

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