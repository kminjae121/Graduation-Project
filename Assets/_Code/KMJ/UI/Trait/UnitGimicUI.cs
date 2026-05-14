using System;
using System.Collections.Generic;
using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UI
{
    public class UnitGimicUI : MonoBehaviour
    {
        [SerializeField] private List<GimicUI> gimicUIs =  new List<GimicUI>();
        
        private Dictionary<UnitType, GimicUI> _gimicUIsDict = new Dictionary<UnitType, GimicUI>();

        private GameObject _currentGimicUI;

        private void Awake()
        {
            Bus<WhatUnitTurnEvent>.Subscribe(ShowGimicUI);
        }

        private void OnDestroy()
        {
            Bus<WhatUnitTurnEvent>.Unsubscribe(ShowGimicUI);
        }

        private void Start()
        {
            foreach (var gimic in gimicUIs)
            {
                _gimicUIsDict.Add(gimic.UnitType, gimic);
                //gimic.gameObject.SetActive(false);
            }
        }

        public void ShowGimicUI(WhatUnitTurnEvent evt)
        {
            //if(_currentGimicUI != null)
            //    _currentGimicUI.SetActive(false);
            //
            //if(_gimicUIsDict.TryGetValue(evt.UnitType, out GimicUI gimicUI));
            //{
            //    if (gimicUI != null)
            //    {
            //        _currentGimicUI = gimicUI.gameObject;
            //        _currentGimicUI.SetActive(true);
            //    }
            //}
        }
    }
}