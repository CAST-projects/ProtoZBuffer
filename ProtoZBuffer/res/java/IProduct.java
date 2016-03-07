package %NAMESPACE%;

/**
 * @param <T>
 *            left
 * @param <U>
 *            right
 * @param <V>
 *            product
 */
public interface IProduct<T, U, V>
{
    /**
     * @param left
     *            left
     * @param right
     *            right
     * @return product
     */
    Iterable<V> product(T left, U right);
}