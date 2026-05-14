using UnityEngine;

namespace Code.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "SO/Item", order = 0)]
    public class ItemSO : ScriptableObject
    {
        public string itemName;
        public Sprite itemIcon;
        public string itemDesc;
    }
}