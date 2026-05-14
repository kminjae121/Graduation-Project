using System;
using Code.Core.Events.Bus;
using Input;
using TMPro;
using UnityEngine;

public class WarningUI : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    [SerializeField] private GameObject warningPanel;

    [SerializeField] private TextMeshProUGUI warningTxt;

    private bool _isPanelOpen = false;
    private void Awake()
    {
        inputReader.OnClickEvent += OnClickWarningPanel;
        
        Bus<WarningUIEvent>.Subscribe(OnWarningPanel);
    }

    private void OnDisable()
    {
        
        inputReader.OnClickEvent -= OnClickWarningPanel;
        Bus<WarningUIEvent>.Unsubscribe(OnWarningPanel);
    }

    private void OnWarningPanel(WarningUIEvent evt)
    {
        warningPanel.SetActive(true);
        _isPanelOpen = true;
        warningTxt.text = evt.message;
    }


    private void OnClickWarningPanel()
    {
        if (_isPanelOpen)
        {
            _isPanelOpen = false;
            warningPanel.SetActive(false);
        }
    }
}
