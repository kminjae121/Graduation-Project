using DG.Tweening;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CostIconUI : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolingItemSO poolingType;
        
        private CanvasGroup _canvasGroup;
        private Tween _blinkTween;
        private Pool _pool;

        public PoolingItemSO PoolingType => poolingType;
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }

        public void ResetItem()
        {
            StopBlink();
        }

        public void ReturnToPool()
        {
            if (_pool != null)
                _pool.Push(this);
            else
                Destroy(gameObject);
        }

        public void SetActiveState(bool isActive)
        {
            StopBlink();
            _canvasGroup.alpha = isActive ? 1f : 0.2f;
        }

        public void SetPreviewState()
        {
            StopBlink();
            _canvasGroup.alpha = 1f;
            _blinkTween = _canvasGroup.DOFade(0.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        private void StopBlink()
        {
            _blinkTween?.Kill();
            _blinkTween = null;
        }

        private void OnDestroy()
        {
            _blinkTween?.Kill();
        }
    }
}