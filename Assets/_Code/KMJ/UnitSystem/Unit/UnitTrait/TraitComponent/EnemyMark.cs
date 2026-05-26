using System.Runtime.InteropServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UnitSystem.TraitSystem
{
    public class EnemyMark : MonoBehaviour
    {
        [SerializeField] private Image targetMark;
        [SerializeField] private int maxCnt = 4;

        private int _markCnt;
        private bool _isMarked;

        public void SetMark()
        {
            if (_isMarked) return;
            
            if (targetMark == null || maxCnt <= 0) return;
            
            if (_markCnt == 0)
            {
                targetMark.gameObject.SetActive(true);
                targetMark.fillAmount = 0f;
            }
            
            _markCnt = Mathf.Min(_markCnt + 1, maxCnt);
            
            float value = (float)_markCnt / maxCnt;
            
            targetMark.DOKill();
            targetMark.DOFillAmount(value, 0.5f);
            
            if (_markCnt >= maxCnt)
                _isMarked = true;
        }
        
        public int GetCurrentMark() => _markCnt;
        
        
        public void ResetMark()
        {
            _markCnt = 0;
            _isMarked = false;

            if (targetMark == null) return;

            targetMark.DOKill();
            targetMark.fillAmount = 0f;
        }
    }
}