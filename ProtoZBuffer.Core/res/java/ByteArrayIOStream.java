package %NAMESPACE%;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

public class ByteArrayIOStream implements IIOStream {

	private ByteArrayInputStream _innerInputStream;
	private ByteArrayOutputStream _innerOutputStream = new ByteArrayOutputStream();

	public ByteArrayIOStream()
	{
	}
	
	public ByteArrayIOStream(byte[] buf) throws IOException {
		_innerOutputStream.write(buf);
	}
	
	@Override
	public int getPosition() {
		return _innerOutputStream.size();
	}

	@Override
	public InputStream getInputStreamAt(int pos) {
		return getInputStreamAt(pos, E_SeekOrigin.Begin);
	}

	@Override
	public InputStream getInputStreamAt(int pos, E_SeekOrigin seekDir) {
		_innerInputStream = new ByteArrayInputStream(_innerOutputStream.toByteArray());
		_innerInputStream.reset();
		
		switch(seekDir)
		{
		case Begin:
			_innerInputStream.skip(pos);
			break;
			
		case End:
			_innerInputStream.skip(_innerOutputStream.size() + pos);
			break;
		}
		
		return _innerInputStream;
	}

	@Override
	public OutputStream getOutputStream() {
		return _innerOutputStream;
	}

	@Override
	public void close() throws IOException {
		// NOP
	}
	
	public byte[] toByteArray()
	{
		return _innerOutputStream.toByteArray();
	}

}
