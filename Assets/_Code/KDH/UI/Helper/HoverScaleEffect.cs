using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HoverScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverScaleMultiplier = 1.2f;
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private Ease easeType = Ease.OutQuad;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = _originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_originalScale * hoverScaleMultiplier, duration).SetEase(easeType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_originalScale, duration).SetEase(easeType);
    }
}