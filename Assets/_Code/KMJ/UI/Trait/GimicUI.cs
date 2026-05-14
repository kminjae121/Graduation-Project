using UnityEngine;

namespace Code.UI
{
    public abstract class GimicUI : MonoBehaviour
    {
        public UnitType UnitType;

        public abstract void OperationUI();
    }
}