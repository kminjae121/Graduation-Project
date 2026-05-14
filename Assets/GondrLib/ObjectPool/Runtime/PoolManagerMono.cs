using GondrLib.Dependencies;
using UnityEngine;

namespace GondrLib.ObjectPool.Runtime
{
    [Provide]
    public class PoolManagerMono : MonoBehaviour, IDependencyProvider
    {
        [SerializeField] private PoolManagerSO poolManager;

        private void Awake()
        {
            poolManager.Initialize(transform);
        }

        public T Pop<T>(PoolingItemSO item) where T : IPoolable
        {
            return (T)poolManager.Pop(item);
        }

        public void Push(IPoolable target)
        {
            poolManager.Push(target);
        }
    }
}