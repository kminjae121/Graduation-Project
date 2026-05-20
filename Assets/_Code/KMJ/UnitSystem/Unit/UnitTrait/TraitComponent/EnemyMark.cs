using UnityEngine;
using UnityEngine.UI;

namespace Code.UnitSystem.TraitSystem
{
    public class EnemyMark : MonoBehaviour
    {
        [SerializeField] private GameObject targetMark;
        [SerializeField] private int maxCnt;
        
        private int _markCnt = 0;

        private bool _isMarked = false;
        
        public void SetMark()
        {
            if (_markCnt >= maxCnt)
                _isMarked = true;

            if (_isMarked)
                return;
            
            if (_markCnt == 0)
                targetMark.SetActive(true);
            
            _markCnt += 1;
        }
    }
}