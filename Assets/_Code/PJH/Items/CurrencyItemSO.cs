using UnityEngine;

namespace Code.Items
{
    [CreateAssetMenu(fileName = "Currency Item", menuName = "SO/Item/Currency", order = 0)]
    public class CurrencyItemSO : ItemSO
    {
        public int amount;
    }
}