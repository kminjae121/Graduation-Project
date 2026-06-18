using System.Collections.Generic;
using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UI
{
    public class UnitSpecUI : MonoBehaviour
    {
        [Header("Spec UI")]
        [SerializeField] private List<SpecUI> specUIs = new List<SpecUI>();
        [SerializeField] private bool includeSceneSpecs = true;
        [SerializeField] private bool deactivateHiddenSpecs = true;

        private GameObject _currentSpecUI;

        private void Awake()
        {
            BuildSpecList();
            HideAllSpecs();
            Bus<WhatUnitTurnEvent>.Subscribe(ShowSpecUI);
        }

        private void OnDestroy()
        {
            Bus<WhatUnitTurnEvent>.Unsubscribe(ShowSpecUI);
        }

        public void ShowSpecUI(WhatUnitTurnEvent evt)
        {
            _currentSpecUI = null;

            for (int i = 0; i < specUIs.Count; ++i)
            {
                SpecUI spec = specUIs[i];
                if (spec == null)
                    continue;

                bool shouldShow = spec.UnitType == evt.UnitType && evt.UnitType != UnitType.None;
                SetSpecVisible(spec, shouldShow);

                if (shouldShow)
                    _currentSpecUI = spec.gameObject;
            }
        }

        private void BuildSpecList()
        {
            for (int i = specUIs.Count - 1; i >= 0; --i)
            {
                if (specUIs[i] == null)
                    specUIs.RemoveAt(i);
            }

            if (specUIs.Count == 0)
                specUIs.AddRange(GetComponentsInChildren<SpecUI>(true));

            AddMissingSpecs(GetComponentsInChildren<SpecUI>(true));

            if (includeSceneSpecs)
                AddMissingSpecs(Object.FindObjectsByType<SpecUI>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        }

        private void HideAllSpecs()
        {
            for (int i = 0; i < specUIs.Count; ++i)
            {
                if (specUIs[i] != null)
                    SetSpecVisible(specUIs[i], false);
            }
        }

        private void SetSpecVisible(SpecUI spec, bool isVisible)
        {
            if (spec == null)
                return;

            CanvasGroup canvasGroup = spec.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
            }

            if (deactivateHiddenSpecs)
                spec.gameObject.SetActive(isVisible);
        }

        private void AddMissingSpecs(IReadOnlyList<SpecUI> specs)
        {
            if (specs == null)
                return;

            for (int i = 0; i < specs.Count; ++i)
            {
                SpecUI spec = specs[i];
                if (spec != null && !specUIs.Contains(spec))
                    specUIs.Add(spec);
            }
        }
    }
}
