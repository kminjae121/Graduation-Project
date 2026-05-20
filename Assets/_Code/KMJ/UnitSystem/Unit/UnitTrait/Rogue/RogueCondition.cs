using Code.Effects;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class RogueCondition : MonoBehaviour, IUnitCondition
    {
        [SerializeField] private UnitVFXCompo vfxCompo;
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
                vfxCompo.PlayVFX("DarkHeal", pos, Quaternion.identity);
                return true;
            }
            
            return false;
        }
    }
}