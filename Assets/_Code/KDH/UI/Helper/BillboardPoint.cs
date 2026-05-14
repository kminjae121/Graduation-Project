using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class BillboardPoint : MonoBehaviour
{
    [Header("Billboard Settings")]
    [SerializeField] private Sprite targetSprite;

    [Header("Floating Animation")]
    [SerializeField] private float floatDistance = 0.3f;
    [SerializeField] private float floatDuration = 1.0f;

    private SpriteRenderer _spriteRenderer;
    private Camera _mainCam;
    private Tween _floatTween;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (targetSprite != null)
        {
            _spriteRenderer.sprite = targetSprite;
        }
    }

    private void Start()
    {
        _mainCam = Camera.main;
        StartFloatingAnimation();
    }

    private void StartFloatingAnimation()
    {
        float targetY = transform.localPosition.y + floatDistance;
        _floatTween = transform.DOLocalMoveY(targetY, floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);  
    }

    private void LateUpdate()
    {
        if (_mainCam != null)
        {
            transform.rotation = _mainCam.transform.rotation;
        }
    }

    private void OnDestroy()
    {
        if (_floatTween != null && _floatTween.IsActive())
        {
            _floatTween.Kill();
        }
    }

    public void SetSprite(Sprite newSprite)
    {
        targetSprite = newSprite;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = targetSprite;
        }
    }
}