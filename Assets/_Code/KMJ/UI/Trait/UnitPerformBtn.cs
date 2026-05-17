using System;
using Code.Core.Events.Bus;
using Code.Core.Events.Bus.Trait;
using Code.UnitSystem.UnitAttributes;
using Input;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UnitPerformBtn : MonoBehaviour
    {
        [SerializeField] private Button _performBtn;

        private UnitTrait _traitCompo;

        private InputReader _inputReader;
        private void Awake()
        {
            Bus<UnitPerformEvent>.Subscribe(HandleAddUnit);
            _performBtn.onClick.AddListener(HandleClickBtn);
        }

        private void OnDestroy()
        {
            Bus<UnitPerformEvent>.Unsubscribe(HandleAddUnit);
            _performBtn.onClick.RemoveListener(HandleClickBtn);
        }

        private void HandleAddUnit(UnitPerformEvent evt)
        { 
            _traitCompo = evt.TraitCompo;
        }

        private void HandleClickBtn()
        {
            if (_traitCompo.IsHasEnemy)
            {
                _traitCompo.Perform();
            }
            else
            {
                _traitCompo.Perform();
            }
        }
    }
}