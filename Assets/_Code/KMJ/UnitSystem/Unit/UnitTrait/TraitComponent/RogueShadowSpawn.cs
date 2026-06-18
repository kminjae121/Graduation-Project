using System.Collections.Generic;
using _Code.Core.EventBus.Events.Trait;
using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class RogueShadowSpawn : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private List<RogueShadow> shadows = new();
        [SerializeField] private int maxShadowCnt = 3;

        private RogueShadow _currentShadowObj;

        private int _shadowCnt = 0;
        private int _currentIdx = 0;

        private CharacterUnit _unit;

        public void Initialize(Unit owner)
        {
            _unit = owner as CharacterUnit;

            _currentIdx = 0;
            _shadowCnt = 0;
            _currentShadowObj = null;

            maxShadowCnt = Mathf.Min(maxShadowCnt, shadows.Count);

            foreach (var shadow in shadows)
            {
                if (shadow != null)
                    shadow.gameObject.SetActive(false);
            }

            Bus<RogueSpecEvent>.Raise(new RogueSpecEvent(_shadowCnt));
        }

        public int GetMaxShadowCnt() => maxShadowCnt;

        public int GetShadowCnt() => _shadowCnt;

        public int GetCurrentShadowIdx() => _currentIdx;

        public List<RogueShadow> GetShadows() => shadows;

        public RogueShadow GetCurrentShadow() => _currentShadowObj;

        public IMapTile GetShadowMapTile()
        {
            return _currentShadowObj != null ? _currentShadowObj.GetMapTile() : null;
        }

        public void SetShadowInfo(RogueShadow shadow, bool active)
        {
            if (shadow == null) return;
            if (!shadows.Contains(shadow)) return;

            bool wasActive = shadow.gameObject.activeSelf;

            shadow.gameObject.SetActive(active);

            if (active && !wasActive)
            {
                _shadowCnt++;
            }
            else if (!active && wasActive)
            {
                _shadowCnt--;
            }

            _shadowCnt = Mathf.Clamp(_shadowCnt, 0, maxShadowCnt);

            if (_currentShadowObj == shadow && !active)
                _currentShadowObj = null;

            Bus<RogueSpecEvent>.Raise(new RogueSpecEvent(_shadowCnt));
        }

        public void SetShadow(IMapTile tile)
        {
            if (tile == null) return;
            if (shadows == null || shadows.Count == 0) return;
            if (maxShadowCnt <= 0) return;

            RogueShadow shadow = shadows[_currentIdx];

            if (shadow == null)
            {
                MoveNextIndex();
                return;
            }

            bool wasActive = shadow.gameObject.activeSelf;

            shadow.gameObject.SetActive(true);
            shadow.SetPos(tile.WorldPos);
            shadow.SetTile(tile);

            _currentShadowObj = shadow;

            if (!wasActive)
                _shadowCnt++;

            _shadowCnt = Mathf.Clamp(_shadowCnt, 0, maxShadowCnt);

            Bus<RogueSpecEvent>.Raise(new RogueSpecEvent(_shadowCnt));

            MoveNextIndex();
        }

        private void MoveNextIndex()
        {
            
            Bus<RogueSpecEvent>.Raise(new RogueSpecEvent(_shadowCnt));
            
            _currentIdx++;

            int limit = Mathf.Min(maxShadowCnt, shadows.Count);

            if (_currentIdx >= limit)
                _currentIdx = 0;
        }

        public void ResetAllShadow()
        {
            foreach (var shadow in shadows)
            {
                if (shadow != null)
                    shadow.gameObject.SetActive(false);
            }

            _currentIdx = 0;
            _shadowCnt = 0;
            _currentShadowObj = null;

            Bus<RogueSpecEvent>.Raise(new RogueSpecEvent(_shadowCnt));
        }
    }
}