using System.Collections.Generic;

namespace %NAMESPACE%
{
    public class Stretchable<T> : IStretchable<T> where T : class
    {
        private readonly List<T> _internList = new List<T>();

        public T this[int index]
        {
            get { return index < Count ? _internList[index] : default(T); }
            set
            {
                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                while (Count <= index)
                    Add(default(T));

                _internList[index] = value;
            }
        }

        public void Add(T item)
        {
            _internList.Add(item);
        }

        public int Count
        {
            get { return _internList.Count; }
        }
    }
}