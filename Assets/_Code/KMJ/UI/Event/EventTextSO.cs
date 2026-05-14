using Code.Items;
using UnityEngine;


public enum EventTextType
{
    Health,
    Item
}
namespace Code.UI
{
    [CreateAssetMenu(fileName = "EventSO", menuName = "SO/Event/EventSO", order = 0)]
    public class EventTextSO : ScriptableObject
    {
        [TextArea(3, 10)]
        public string MainTxt;
        public string ApplyTxt;
        public string CancelTxt;
        public string SuccessTxt;
        public string FailTxt;
        public string SkipTxt;

        public EventTextType eventTextType;

        public Sprite EventImg;
        
        public float value;

        public ItemSO item;
    }
}