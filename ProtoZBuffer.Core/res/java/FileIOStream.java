package %NAMESPACE%;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.RandomAccessFile;

public class FileIOStream implements IIOStream {

	private RandomAccessFile _file;
	private FileInputStream _innerInputStream;
	private FileOutputStream _innerOutputStream;
	
	private class FileInputStream extends InputStream
	{
		RandomAccessFile _fileToRead = null;
		long m_mark = 0;

		public FileInputStream(RandomAccessFile file)
		{
			_fileToRead = file;
		}

		@Override
		public int read() throws IOException
		{
			return _fileToRead.read();
		}

		@Override
		public void close() throws IOException
		{
			// NOP: we should close the file only at IO stream level
		}

		@Override
		public synchronized void mark(int arg0)
		{
			m_mark = arg0;
		}

		@Override
		public boolean markSupported()
		{
			return true;
		}

		@Override
		public int read(byte[] arg0) throws IOException
		{
			return _fileToRead.read(arg0);
		}

		@Override
		public int read(byte[] arg0, int arg1, int arg2) throws IOException
		{
			return _fileToRead.read(arg0, arg1, arg2);
		}

		@Override
		public synchronized void reset() throws IOException
		{
			_fileToRead.seek(m_mark);
		}

		@Override
		public long skip(long arg0) throws IOException
		{
			_fileToRead.seek(_fileToRead.getFilePointer() + arg0);
			return _fileToRead.getFilePointer();
		}
		
	}

	private class FileOutputStream extends OutputStream
	{
		RandomAccessFile _fileToWrite = null;

		public FileOutputStream(RandomAccessFile file)
		{
			_fileToWrite = file;
		}

		@Override
		public void write(int b) throws IOException {
			_fileToWrite.write(b);
		}
		
		@Override
		public void close() throws IOException {
			// NOP: we should close the file only at IO stream level
		}

		@Override
		public void flush() throws IOException {
			// NOP: RandomAccessFile doesn't have a flush method
		}

		@Override
		public void write(byte[] b, int off, int len) throws IOException {
			_fileToWrite.write(b, off, len);
		}

		@Override
		public void write(byte[] b) throws IOException {
			_fileToWrite.write(b);
		}
		
	}
	
	public FileIOStream(String path) throws IOException
	{
		this(path, false);
	}
	
	public FileIOStream(File path) throws IOException
	{
		this(path, false);
	}
	
	public FileIOStream(String path, boolean truncate) throws IOException
	{
		this(new File(path), truncate);
	}
	
	public FileIOStream(File path, boolean truncate) throws IOException
	{
		_file = new RandomAccessFile(path, "rw");
		if (truncate)
			_file.setLength(0);
		_innerInputStream = new FileInputStream(_file);
		_innerOutputStream = new FileOutputStream(_file);
	}
	
	@Override
	public int getPosition() {
		try {
			return (int) _file.getFilePointer();
		} catch (IOException e) {
			return -1;
		}
	}

	@Override
	public InputStream getInputStreamAt(int pos) {
		return getInputStreamAt(pos, E_SeekOrigin.Begin);
	}

	@Override
	public InputStream getInputStreamAt(int pos, E_SeekOrigin seekDir) {
		try {
			switch(seekDir)
			{
			case Begin:
				_file.seek(pos);
				break;
				
			case End:
				_file.seek(_file.length() + pos);
				break;
			}
			
			return _innerInputStream;
		} catch (IOException e) {
			return null;
		}
	}

	@Override
	public OutputStream getOutputStream() {
		try {
			// go to end
			_file.seek(_file.length());
			return _innerOutputStream;
		} catch (IOException e) {
			return null;
		}
	}

	@Override
	public void close() throws IOException {
		_file.close();
	}

}
