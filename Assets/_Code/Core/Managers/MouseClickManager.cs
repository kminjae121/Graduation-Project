using System;
using Code.Core;
using Input;
using Nova;
using UnityEngine;

public class MouseClickManager : MonoSingleton<MouseClickManager>
{
    [SerializeField] private InputReader input;

    protected override void Awake()
    {
        base.Awake();

        input.OnClickEvent += OnClick;
        input.OnClickCancelEvent += OnCacel;
    }


    private void OnDestroy()
    {
        input.OnClickEvent -= OnClick;
        input.OnClickCancelEvent -= OnCacel;
    }

    private void OnClick()
    {
        SoundManager.Instance.PlayClip("ClickUpSound");
    }
    private void OnCacel()
    {
        //SoundManager.Instance.PlayClip("ClickDownSound");
    }
}
