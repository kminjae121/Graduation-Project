using UnityEngine;

namespace GondrLib.ObjectPool.Runtime
{
    public interface IPoolable
    {
        public PoolingItemSO PoolingType { get; }
        public GameObject GameObject { get; }
        public void SetUpPool(Pool pool);
        public void ResetItem();
    }
}