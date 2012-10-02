package net.gregbeaty.clientstream.helper;

public class Logger {
	public final static int ERROR = 1;
	public final static int INFO = 2;
	public final static int DEBUG = 3;
	
	private static int mLevel = INFO;
	
	
	private Logger() {
		throw new AssertionError();
	}
	
	public static void setLevel(int level)
	{
		mLevel = level;
	}
	 
	public static void error(String msg) {
		if (mLevel < ERROR) return;
		
		System.err.println("ERROR: " + msg);
	}
	
	public static void info(String msg) {
		if (mLevel < INFO) return;
		
		System.out.println("INFO: " + msg);
	}
	
	public static void debug(String msg) {
		if (mLevel < DEBUG) return;
		
		System.out.println("DEBUG: " + msg);
	}
}
