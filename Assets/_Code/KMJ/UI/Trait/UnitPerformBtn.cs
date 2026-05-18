using System;
using Code.Core.Events.Bus;
using Code.Core.Events.Bus.Trait;
using Code.UnitSystem.TraitSystem;
using Input;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UnitPerformBtn : MonoBehaviour
    {
        [SerializeField] private Button _performBtn;
        [SerializeField] private InputReader _inputReader;

        private UnitTrait _traitCompo;
        
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
            if (_traitCompo.IsNeedTarget)
            {
                _traitCompo.SetTargeting();
            }
            else
            {
                _traitCompo.Perform();
            }
        }
    }
}