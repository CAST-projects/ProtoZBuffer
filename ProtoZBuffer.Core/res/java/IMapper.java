package %NAMESPACE%;

/**
 * @param <T>
 *            key
 * @param <U>
 *            value
 */
public interface IMapper<T, U>
{
    /**
     * @param item
     *            key to map
     * @return values
     */
	Iterable<U> map(T item);
}