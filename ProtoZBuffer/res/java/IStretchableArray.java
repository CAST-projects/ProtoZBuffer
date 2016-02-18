package %NAMESPACE%;

/**
 * @param <T>
 *            type in list
 */
public interface IStretchableArray<T> extends Iterable<T>
{
    /**
     * @param index
     *            index
     * @param value
     *            value
     */
    void set(int index, T value);

    /**
     * @param index
     *            index
     * @return element
     */
    T get(int index);

    /**
     * @return size of the collection
     */
    int size();

    /**
     * @param item
     *            to add to the collection
     */
    void add(T item);

    /**
     * empty the collection
     */
    void clear();
}
