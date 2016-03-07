using System;
using System.Collections.Generic;
using System.Threading;

namespace %NAMESPACE%
{
    public class WeakStretchable<T> : IStretchable<T> where T : class
    {
        private readonly List<WeakReference<T>> _internList = new List<WeakReference<T>>();

        public T this[int index]
        {
            get
            {
				if (index >= _internList.Count) return default(T);
				T result;
				return _internList[index].TryGetTarget(out result) ? result : default(T);
            }
            set
            {
				// ReSharper disable once LoopVariableIsNeverChangedInsideLoop
				while (_internList.Count <= index)
					_internList.Add(new WeakReference<T>(default(T)));

				ApplicationCache.Cache(value);
				_internList[index] = new WeakReference<T>(value);
            }
        }

        public void Add(T item)
        {
            _internList.Add(new WeakReference<T>(item));
        }

        public int Count
        {
            get
            {
                return _internList.Count;
            }
        }
    }
}