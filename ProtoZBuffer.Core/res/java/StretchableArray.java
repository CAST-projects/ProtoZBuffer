package %NAMESPACE%;

import java.util.ArrayList;
import java.util.Iterator;

/**
 * yet another collection.
 *
 * @author CTZ
 * @version 1.0
 * @param <T>
 *            the item type
 */
public class StretchableArray<T> implements IStretchableArray<T>
{
    ArrayList<T> content = new ArrayList<T>();

    /**
     * sets an item
     *
     * @param index
     *            item index
     * @param value
     *            item value
     */
    @Override
    public void set(int index, T value)
    {
        while (content.size() <= index)
            content.add(null);

        content.set(index, value);
    }

    /**
     * gets an item or null
     *
     * @param index
     *            the index
     * @return the item or null
     */
    @Override
    public T get(int index)
    {
        if (index < content.size())
            return content.get(index);

        return null;
    }

    @Override
    public int size()
    {
        return content.size();
    }

    @Override
    public void add(T item)
    {
        content.add(item);
    }

    @Override
    public void clear()
    {
        content.clear();
    }

    @Override
    public Iterator<T> iterator()
    {
        return content.iterator();
    }
}
