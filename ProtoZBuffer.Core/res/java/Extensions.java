package %NAMESPACE%;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Queue;
import java.util.Set;
import java.util.Stack;

/**
 * Helper for common collections methods
 *
 * @author CTZ
 * @version 1.0
 */
public class Extensions
{
    private Extensions()
    {
        // empty
    }

    /**
     * counts the number of items of a collection
     *
     * @param <T>
     *            the input collection
     *
     * @param theCollection
     *            the collection
     * @return the count
     */
    @SuppressWarnings("unchecked")
    public static <T> int count(final Iterable<T> theCollection)
    {
        final Iterable<Object> collection = (Iterable<Object>)theCollection;
        return countObjects(collection);
    }

    /**
     *
     * @param <T>
     *            the type of the extracted
     * @param <U>
     *            the type of the collection
     * @param type
     *            the class of the extracted
     * @param collection
     *            the collection
     * @return the count
     */
    public static <T, U> int countExtract(final Class<T> type, final Iterable<U> collection)
    {
        return count(extract(type, collection));
    }

    /**
     * counts the number of items of a collection
     *
     * @param collection
     *            the collection
     * @return the count
     */
    public static int countObjects(final Iterable<Object> collection)
    {
        int cnt = 0;

        for (final Iterator<Object> it = collection.iterator(); it.hasNext(); it.next())
        {
            cnt++;
        }

        return cnt;
    }

    /**
     * @param <T>
     *            T le type d'extraction
     * @param <U>
     *            Le type de la collection a scanner
     * @param type
     *            le classe de T car on ne peut pas écrire T.getClass()!
     * @param collection
     *            la collection à filtrer
     * @return le résultat filtré
     */
    @SuppressWarnings("unchecked")
    // diag buggé
    public static <T, U> Iterable<T> extract(final Class<T> type, final Iterable<U> collection)
    {
        return (Iterable<T>)extract(collection, new IFilter<U>()
            {


            @Override
                public boolean keepIt(U item)
                {
                    return (type.isAssignableFrom(item.getClass()));
                }

            });
    }

    /**
     * @param <T>
     *            T le type d'extraction
     * @param <U>
     *            Le type de la collection a scanner
     * @param type
     *            le classe de T car on ne peut pas écrire T.getClass()!
     * @param collection
     *            la collection à filtrer
     * @return le résultat filtré
     */
    public static <T, U> T extractFirst(final Class<T> type, final Iterable<U> collection)
    {
        return first(extract(type, collection));
    }

    /**
     * @param <T>
     *            Type of the collection
     * @param collection
     *            the collection
     * @return the first item
     */
    public static <T> T first(final Iterable<T> collection)
    {
        for (final T t : collection)
            return t;

        return null;
    }

    /**
     * @param <T>
     *            Type of the collection
     * @param collection
     *            the collection
     * @return the last item
     */
    public static <T> T last(final Iterable<T> collection)
    {
        T currentItem = null;
        for (final T t : collection)
            currentItem = t;

        return currentItem;
    }

    /**
     *
     * @param <T>
     *            the type of the collection
     * @param collection
     *            the collection
     * @return true if empty
     */
    @SuppressWarnings({ "unused" })
    public static <T> boolean isEmpty(final Iterable<T> collection)
    {
        for (final T t : collection)
            return false;

        return true;
    }

    /**
     * @param collection
     *            the collection
     * @param <T>
     *            the type of the collection
     * @return the flatten collection as a list
     *
     */
    public static <T> List<T> toList(Iterable<T> collection)
    {
        if (collection.getClass().isAssignableFrom(List.class))
            return (List<T>)collection;

        final ArrayList<T> l = new ArrayList<T>();

        for (final T t : collection)
            l.add(t);

        return l;
    }

    /**
     * @param collection
     *            the collection
     * @param <T>
     *            the type of the collection
     * @return the flatten collection as a stack
     *
     */
    public static <T> Stack<T> toStack(Iterable<T> collection)
    {
        final Stack<T> l = new Stack<T>();

        for (final T t : collection)
            l.push(t);

        return l;
    }

    /**
     * @param collection
     *            the collection
     * @param <T>
     *            the type of the collection
     * @return the flatten collection as a list
     *
     */
    public static <T> List<T> toList(Collection<T> collection)
    {
        final ArrayList<T> l = new ArrayList<T>(collection.size());

        for (final T t : collection)
            l.add(t);

        return l;
    }

    /**
     * @param collection
     *            the collection
     * @param <T>
     *            the type of the collection
     * @return the flatten collection as a list
     *
     */
    public static <T> List<T> toList(T... collection)
    {
        final ArrayList<T> l = new ArrayList<T>();

        for (final T t : collection)
            l.add(t);

        return l;
    }

    /**
     * @param <T>
     *            the type of the collection
     * @param collection
     *            the collection
     * @return the collection flattened into a set
     */
    public static <T> Set<T> toSet(Iterable<T> collection)
    {
        Set<T> l = new HashSet<T>();

        for (final T t : collection)
            l.add(t);

        return l;
    }

    /**
     * @param <T>
     *            the type of the collection
     * @param collection
     *            the collection
     * @return the collection flattened into a set
     */
    public static <T> Set<T> toSet(T... collection)
    {
        Set<T> l = new HashSet<T>();

        for (final T t : collection)
            l.add(t);

        return l;
    }

    /**
     * @param <T>
     *            type of the collection
     * @param collection
     *            the collection
     * @return the collection as an array
     */
    public static <T> Object[] toArray(Iterable<T> collection)
    {
        return toList(collection).toArray();
    }

    /**
     *
     * @param <T>
     *            type of the collection
     * @param item
     *            an item
     * @param collection
     *            a collection
     * @return true if item is in the collection
     */
    public static <T> boolean contains(T item, Iterable<T> collection)
    {
        for (T t : collection)
            if (item.equals(t))
                return true;
        return false;
    }

    /**
     * merges collections
     *
     * @param <T>
     *            the type of the collection
     * @param iterables
     *            the collections
     * @return the union all
     */
    public static <T> Iterable<T> merge(final Iterable<T>... iterables)
    {
        return new Iterable<T>()
        {


            @Override
            public Iterator<T> iterator()
            {
                return new Iterator<T>()
                {
                    Iterable<T>[] m_array = iterables;
                    int m_currentIterable = 0;
                    Iterator<T> m_currentIterator;

                    {
                        m_currentIterator = (m_array.length > 0) ? m_array[0].iterator() : null;
                    }


                    @Override
                    public boolean hasNext()
                    {
                        if (m_currentIterator == null)
                            return false;
                        while (!m_currentIterator.hasNext())
                        {
                            m_currentIterable++;
                            if (m_currentIterable >= m_array.length)
                                return false;
                            m_currentIterator = m_array[m_currentIterable].iterator();
                        }
                        return true;
                    }


                    @Override
                    public T next()
                    {
                        return m_currentIterator.next();
                    }


                    @Override
                    public void remove()
                    {
                        // do nothing
                    }
                };
            }

        };
    }

    /**
     * @param <T>
     *            the type of the collection
     * @param collection
     *            the collection
     * @param filter
     *            the where clause
     * @return the filtered collection
     */
    public static <T> Iterable<T> extract(final Iterable<T> collection, final IFilter<T> filter)
    {
        return new Iterable<T>()
        {
            final Iterable<T> m_collection = collection;


            @Override
            public Iterator<T> iterator()
            {
                return new Iterator<T>()
                {
                    Iterator<T> m_iterator = m_collection.iterator();

                    T m_current = null;


                    @Override
                    public boolean hasNext()
                    {
                        while (true)
                        {
                            if (!m_iterator.hasNext())
                                break;

                            // ESCA-JAVA0282: ce diag est buggé
                            T item = m_iterator.next();
                            if (filter.keepIt(item))
                            {
                                m_current = item;
                                return true;
                            }
                        }
                        return false;
                    }


                    @Override
                    public T next()
                    {
                        return m_current;
                    }


                    @Override
                    public void remove()
                    {
                        // non implémenté
                    }

                };
            }

        };
    }

    /**
     * intersection
     *
     * @param <T>
     *            the type of the collection
     * @param plus
     *            the positive collection
     * @param minus
     *            the negative collection
     * @return the intersect
     */
    public static <T> Iterable<T> intersect(final Iterable<T> plus, final Iterable<T> minus)
    {
        return extract(plus, new IFilter<T>()
        {
            Set<T> m_minus = toSet(minus);


            @Override
            public boolean keepIt(T item)
            {
                return m_minus.contains(item);
            }

        });
    }

    /**
     * intersection
     *
     * @param <T>
     *            the type of the collection
     * @param plus
     *            the positive collection
     * @param minus
     *            the negative collection
     * @return the intersect
     */
    public static <T> Iterable<T> exclude(final Iterable<T> plus, final Iterable<T> minus)
    {
        return extract(plus, new IFilter<T>()
            {
                Set<T> m_minus = toSet(minus);


            @Override
                public boolean keepIt(T item)
                {
                    return !m_minus.contains(item);
                }

            });
    }

    /**
     * make a single item collection
     *
     * @param <T>
     *            the type of the collection
     * @param item
     *            the item
     * @return the collection containing only that item
     */
    public static <T> Iterable<T> iterable(final T item)
    {
        return new Iterable<T>()
        {


            @Override
            public Iterator<T> iterator()
            {
                return new Iterator<T>()
                {
                    T m_item = item;


                    @Override
                    public boolean hasNext()
                    {
                        return m_item != null;
                    }


                    @Override
                    public T next()
                    {
                        T t = m_item;
                        m_item = null;
                        return t;
                    }


                    @Override
                    public void remove()
                    {// Do nothing
                    }

                };
            }

        };
    }

    /**
     * returns an empty collection
     *
     * @param <T>
     *            the type of the collection
     * @return an empty collection
     */
    public static <T> Iterable<T> emptyCollection()
    {
        return new Iterable<T>()
        {


            @Override
            public Iterator<T> iterator()
            {
                return new Iterator<T>()
                {


                    @Override
                    public boolean hasNext()
                    {
                        return false;
                    }


                    @Override
                    public T next()
                    {
                        // do nothing
                        return null;
                    }


                    @Override
                    public void remove()
                    {
                        // do nothing
                    }

                };
            }

        };
    }

    /**
     *
     * @param <T>
     *            the type of the collection
     * @param collection
     *            the collection to reverse
     * @return the reverted collection
     */
    public static <T> List<T> revert(Iterable<T> collection)
    {
        Stack<T> s = Extensions.toStack(collection);
        List<T> l = new ArrayList<T>();
        while (!s.isEmpty())
            l.add(s.pop());
        return l;
    }

    /**
     *
     * @param values
     *            int values
     * @return the max
     */
    public static int max(Iterable<Integer> values)
    {
        boolean hasSet = false;
        int max = -1;
        for (int v : values)
        {
            if ((!hasSet) || (max < v))
                max = v;

            hasSet = true;
        }
        return max;
    }

    /**
     * forces iteration over a collection
     *
     * @param <T>
     *            the type of items
     * @param collection
     *            the collection
     */
    public static <T> void iterate(Iterable<T> collection)
    {
        for (@SuppressWarnings("unused")
        T item : collection)
        {
            // do nothing
        }
    }

    /**
     * copies a stream to another
     *
     * @param input
     *            the input stream
     * @param output
     *            the output stream
     * @param n
     *            the number of bytes to copy
     * @throws IOException
     *             if fails
     */
    public static void copy(InputStream input, OutputStream output, int n) throws IOException
    {
        byte[] b = new byte[n];
        input.read(b);
        output.write(b);
    }

    /**
     * @param input
     *            iterable
     * @return list with no repetition
     */
    public static <T> List<T> unique(Iterable<T> input)
	{
        List<T> l = new ArrayList<T>();
        Set<T> done = new HashSet<T>();

        for (T i : input)
        {
            if (done.contains(i))
                continue;

            done.add(i);
            l.add(i);
        }

        return l;
	}

    /**
     * @param res
     *            collection
     * @param toAdd
     *            elements to add to collection
     */
	public static <T> void addAll(Collection<T> res, Iterable<T> toAdd)
	{
		for (T item : toAdd)
			res.add(item);
	}

    /**
     * @param prefix
     *            prefix
     * @param right
     *            right
     * @param delegate
     *            delegate
     * @return iterable
     */
    public static <T, U, V> Iterable<V> prefix(final T prefix, final Iterable<U> right,
        final IProduct<T, U, V> delegate)
    {
        return map(right, new IMapper<U, V>()
        {

            @Override
            public Iterable<V> map(U item)
            {
                return delegate.product(prefix, item);
            }
        });

    }

    /**
     * @param left
     *            first collection
     * @param right
     *            right collection
     * @param delegate
     *            delegate
     * @return the cartesian product
     */
    public static <T, U, V> Iterable<V> cartesian(final Iterable<T> left, final Iterable<U> right,
        final IProduct<T, U, V> delegate)
    {
        return map(left, new IMapper<T, V>()
        {

            @Override
            public Iterable<V> map(T item)
            {
                return prefix(item, right, delegate);
            }
        });
    }

    /**
     * @param collection
     *            collection to map
     * @param delegate
     *            mapper
     * @return iterable
     */
	public static <T, U> Iterable<U> map(final Iterable<T> collection, final IMapper<T, U> delegate)
	{
		return new Iterable<U>()
		{
            @Override
            public Iterator<U> iterator()
			{
				return new Iterator<U>()
				{
					Iterator<T> internalCollection = collection.iterator();
					Iterator<U> currentIterator = null;


                    @Override
                    public boolean hasNext()
					{
						if ((currentIterator==null)||(!currentIterator.hasNext()))
						{
							if (internalCollection.hasNext())
							{
								T item = internalCollection.next();
								Iterable<U> coll = delegate.map(item);
								currentIterator = coll.iterator();
							}
						}
						return (currentIterator!=null)&&(currentIterator.hasNext());
					}


                    @Override
                    public U next()
					{
						return currentIterator.next();
					}


                    @Override
                    public void remove()
					{
						// not implemented
					}

				};
			}

		};
	}


    /**
     * compare if collections contains the same items in the same order
     *
     * @param first
     *            first collection
     * @param second
     *            second collection
     * @return true if equal
     */
    public static <T> boolean areEquals(Iterable<T> first, Iterable<T> second)
    {
        Object[] l1 = Extensions.toArray(first);
        Object[] l2 = Extensions.toArray(second);
        int n = l1.length;
        int l2n = l2.length;

        if (n != l2n)
            return false;

        for (int i = 0; i < n; i++)
            if (!l1[i].equals(l2[i]))
                return false;
        return true;
    }

    /**
     *
     * @param cnt
     *            the counter
     * @param mod
     *            the modulo
     * @return true one time per modulo
     */
    public static boolean every(int cnt, int mod)
    {
        return (cnt % mod) == (mod - 1);
    }

    /**
     * @param collection
     *            a collection
     * @return the corresponding queue
     */
    public static <T> Queue<T> toQueue(Iterable<T> collection)
    {
        Queue<T> q = new LinkedList<T>();

        for (T c : collection)
        {
            q.add(c);
        }

        return q;
    }

}
