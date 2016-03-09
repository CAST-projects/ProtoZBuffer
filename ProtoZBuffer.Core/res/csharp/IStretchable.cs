namespace %NAMESPACE%
{
    interface IStretchable<T>
    {
        T this[int index] { get; set; }

        int Count { get; }

        void Add(T item);
    }
}
