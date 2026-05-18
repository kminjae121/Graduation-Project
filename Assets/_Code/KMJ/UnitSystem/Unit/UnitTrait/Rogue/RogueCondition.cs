using System.Collections.Generic;
using Code.UI;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class RogueCondition : MonoBehaviour, IUnitCondition
    {
        [SerializeField] private UnitEffectCompo effectCompo;
        [SerializeField] private RogueShadowSpawn rogueShadowCompo;

        private Unit _unit;
        public void Initialize(Unit unit)
        {
            _unit = unit;
        }

        public bool CheckCondition(GameObject target)
        {
            if (rogueShadowCompo.GetMaxShadowCnt() <= rogueShadowCompo.GetShadowCnt())
            {
                Vector3 pos = _unit.transform.position;

                pos.y += 0.4f;
                effectCompo.PlayTargetEffect("DarkHeal",pos);
                return true;
            }
            return false;
        }
    }
}