using System.Collections.Generic;
using UnityEngine;

namespace Code.UnitSystem.TraitSystem
{
    public class ArcherCondition : MonoBehaviour, IUnitCondition
    {
        [SerializeField] private ArcherMark archerMark;

        public void Initialize(Unit unit)
        {
        }

        public bool CheckCondition(GameObject target)
        {
            return archerMark.SetMarkEnemy(target);
        }
    }
}