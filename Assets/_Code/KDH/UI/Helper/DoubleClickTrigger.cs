using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI
{
    public class DoubleClickTrigger : MonoBehaviour, IPointerClickHandler
    {
        public Action OnDoubleClick;
        public Func<bool> CanDoubleClick; 

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                if (CanDoubleClick == null || CanDoubleClick.Invoke())
                {
                    OnDoubleClick?.Invoke();
                }
            }
        }
    }
}