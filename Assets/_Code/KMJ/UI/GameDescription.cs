using System;
using Code.Core;
using UnityEngine;

public class GameDescription : MonoSingleton<GameDescription>
{
    [SerializeField] private GameObject descriptionUI;

    private bool _isOpen = false;

    protected override void Awake()
    {
        base.Awake();
        descriptionUI.SetActive(false);
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Tab) && !_isOpen)
        {
            descriptionUI.SetActive(true);
            _isOpen = true;
            Time.timeScale = 0;
        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Tab) && _isOpen)
        {
            descriptionUI.SetActive(false);
            _isOpen = false;
            Time.timeScale = 1;
        }
    }
}
