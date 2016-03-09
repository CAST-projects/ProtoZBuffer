package %NAMESPACE%;

import java.lang.ref.SoftReference;
import java.util.ArrayList;
import java.util.Iterator;

/**
 * @param <T>
 *            parameter
 */
public class LazyArray<T> implements IStretchableArray<T>
{
    ArrayList<SoftReference<T>> content = new ArrayList<SoftReference<T>>();

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
            content.add(new SoftReference<T>(null));

        content.set(index, new SoftReference<T>(value));
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
            return content.get(index).get();

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
        content.add(new SoftReference<T>(item));
    }

    @Override
    public void clear()
    {
        content.clear();
    }

    @Override
    public Iterator<T> iterator()
    {
        return Extensions.map(content, new IMapper<SoftReference<T>, T>()
        {

            @Override
            public Iterable<T> map(SoftReference<T> item)
            {
                return Extensions.iterable(item.get());
            }
        }).iterator();
    }
}
