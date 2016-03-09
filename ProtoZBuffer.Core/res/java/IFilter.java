package %NAMESPACE%;

/**
 * a filter
 * 
 * @author CTZ
 * @version 1.0
 * 
 * @param <T>
 *            the type of the item
 */
public interface IFilter<T>
{
	/**
	 *
	 * @param item
	 *            the item to filter
	 * @return true = keep it
	 */
	boolean keepIt(T item);
}
