using System;
using System.Collections.Generic;

namespace Code.Utils
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private readonly List<T> _heap = new();

        public int Count => _heap.Count;

        public void Clear()
            => _heap?.Clear();

        public T Contains(T data)
        {
            int index = _heap.IndexOf(data);
            return index < 0 ? default : _heap[index];
        }

        public void Push(T data)
        {
            // 데이터 삽입
            _heap.Add(data);
            int now = _heap.Count - 1;

            while (now > 0)
            {
                int next = (now - 1) / 2;

                // 우선순위 확인
                if (_heap[now].CompareTo(_heap[next]) < 0)
                    break;

                (_heap[now], _heap[next]) = (_heap[next], _heap[now]);
                now = next;
            }
        }

        public T Pop()
        {
            T ret = _heap[0];

            int lastIndex = _heap.Count - 1;
            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);
            --lastIndex;

            int now = 0;

            while (true)
            {
                int left = 2 * now + 1, right = 2 * now + 2;
                int next = now;

                // 왼쪽이 크면
                if (left <= lastIndex && _heap[next].CompareTo(_heap[left]) < 0)
                    next = left;

                // 오른쪽이 크면
                if (right <= lastIndex && _heap[next].CompareTo(_heap[right]) < 0)
                    next = right;

                // 교환 없을 때
                if (next == now)
                    break;

                (_heap[now], _heap[next]) = (_heap[next], _heap[now]);
                now = next;
            }

            return ret;
        }

        public T Peek()
            => _heap.Count == 0 ? default : _heap[0];
    }
}