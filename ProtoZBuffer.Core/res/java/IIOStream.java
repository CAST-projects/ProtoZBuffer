package %NAMESPACE%;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

/**
 * Interface used to access a stream in both read and write mode.
 */
public interface IIOStream {

	/**
	 * Reference point used to obtain a new position
	 */
	public enum E_SeekOrigin
	{
		Begin, 
		End
	}
	
	/**
	 * The current position within the stream. 
	 * @return the current position within the stream
	 */
	public int getPosition();
	
	/**
	 * Closes the stream, in both read and write mode.
	 * @throws IOException
	 */
	public void close() throws IOException;

	/**
	 * Position the stream at absolute position pos (from its beginning) and returns it in read mode.
	 * @param pos byte offset relative to the beginning of the stream
	 * @return stream in read mode
	 */
	public InputStream getInputStreamAt(int pos);

	/**
	 * Position the stream at absolute position pos, either from its beginning or its end (based on origin), 
	 * and returns it in read mode.
	 * @param pos byte offset relative to the origin parameter
	 * @param origin reference point used to obtain the new position
	 * @return stream in read mode
	 */
	public InputStream getInputStreamAt(int pos, E_SeekOrigin origin);

	/**
	 * Position the stream at its end, and return it in write mode
	 * @return stream in write mode
	 */
	public OutputStream getOutputStream();
	
}
