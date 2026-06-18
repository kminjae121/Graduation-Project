using Code.Core.Events.Bus;
using Code.Core.Events.Bus.Trait;
using Code.UnitSystem.TraitSystem;
using UnityEngine;

namespace Code.UI
{
    public class UnitClimaxPerform : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode performKey = KeyCode.X;

        private UnitTrait _traitCompo;

        private void Awake()
        {
            Bus<UnitPerformEvent>.Subscribe(HandleAddUnit);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(performKey))
                PerformTrait();
        }

        private void OnDestroy()
        {
            Bus<UnitPerformEvent>.Unsubscribe(HandleAddUnit);
        }

        private void HandleAddUnit(UnitPerformEvent evt)
        {
            _traitCompo = evt.TraitCompo;
        }

        private void PerformTrait()
        {
            if (_traitCompo == null)
                return;

            if (_traitCompo.IsNeedTarget)
            {
                _traitCompo.SetTargeting();
                return;
            }

            _traitCompo.Perform();
        }
    }
}
