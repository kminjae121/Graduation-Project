using System;
using System.Collections.Generic;
using Code.Core.Events.Bus;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class UnitsHpBar : MonoBehaviour
    {
        [SerializeField] private List<Slider> healthSliders;

        [SerializeField] private List<Image> unitCharacterImages;

        private void Awake()
        {
            Bus<SetUpUnitHealthBar>.Subscribe(HandleUnitHealthBar);
        }

        private void OnDisable()
        {
            Bus<SetUpUnitHealthBar>.Unsubscribe(HandleUnitHealthBar);
            
        }

        private void HandleUnitHealthBar(SetUpUnitHealthBar evt)
        {
            healthSliders[evt.unitCount].value = evt.finalValue;
            unitCharacterImages[evt.unitCount].sprite = evt.unitImage;
        }
    }
}