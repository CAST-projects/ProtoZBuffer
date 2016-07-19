using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStructure
{
    public class CircularBuffer<T>
    {
        private long _capacity;
        private List<Queue<T>> _queues;

        public CircularBuffer(long capacity)
        {
            var i = 0;
            while (_queues == null)
            {
                ++i;
                var ringCapacity = _capacity / i;
                if (ringCapacity >= int.MaxValue) continue;
                _queues = new List<Queue<T>>(i);
                try
                {
                    for (var j = 0; j < i; ++j)
                        _queues.Add(new Queue<T>((int)ringCapacity));
                }
                catch (OutOfMemoryException)
                {
                    _queues = null;
                }
            }
            _capacity = capacity;
        }

        private long Low
        {
            get { return (long)(0.8 * Capacity); }
        }

        private long High
        {
            get { return (long)(0.9 * Capacity); }
        }

        public long Count
        {
            get { return _queues.Sum(_ => (long)_.Count); }
        }

        public long Capacity
        {
            get { return _capacity; }
            set
            {
                var result = new CircularBuffer<T>(value);
                foreach (var queue in _queues)
                {
                    result.Enqueue(queue);
                }
                _queues = result._queues;
                _capacity = value;
            }
        }

        public void Enqueue(T item)
        {
            _queues.First(_ => (long)_.Count < Capacity / (long)_queues.Count).Enqueue(item);
            Discard();
        }

        public void Clear()
        {
            foreach (var queue in _queues)
                queue.Clear();
        }

        private void Enqueue(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _queues.First(_ => (long)_.Count < Capacity / (long)_queues.Count).Enqueue(item);
            }
        }

        private void Discard()
        {
            if (Count < High) return;
            while (Count > Low)
            {
                _queues.First().Dequeue();
            }
            _queues = _queues.OrderByDescending(_ => _.Count).ToList();
        }

        public static CircularBuffer<T> OnFreeMemory(long itemSize)
        {
            var processAvaibleMemory = Process.GetCurrentProcess().WorkingSet64 + 50000000; // Arbitrary value
            if (!Environment.Is64BitProcess && processAvaibleMemory > 2 * 1024 * 1024 * 1024L)
                processAvaibleMemory = 2 * 1024 * 1024 * 1024L;

            var maxItemsInAvailableMemory = (processAvaibleMemory - GC.GetTotalMemory(false)) / itemSize;
            // 1Gb (2 000 000 items) looks sufficient for most cases. However, when there is not much available memory, we should take less.
            // Proposed algorithm: 
            // - We aim for 1Gb, but we don't use more than half the available memory (to let place for other allocations)
            // - If the available memory is less than 100Mb, we use as much memory as available
            var itemsCount = maxItemsInAvailableMemory < 200000 ? // 200 000
                maxItemsInAvailableMemory :
                Math.Min(maxItemsInAvailableMemory / 2, 2000000); // 2 000 000

            return new CircularBuffer<T>(itemsCount);
        }
    }



    public class ApplicationCache : IDisposable
    {
        private static readonly CircularBuffer<object> ItemCache = CircularBuffer<object>.OnFreeMemory(512);
        public static void Cache(object item)
        {
            lock (ItemCache)
            {
                ItemCache.Enqueue(item);
            }
        }

        public static void Clear()
        {
            lock (ItemCache)
            {
                ItemCache.Clear();
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
