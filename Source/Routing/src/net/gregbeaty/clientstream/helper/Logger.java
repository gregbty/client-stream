package net.gregbeaty.clientstream.helper;

import java.io.BufferedWriter;
import java.io.FileWriter;

public class Logger {
	public final static int ERROR = 1;
	public final static int INFO = 2;
	public final static int DEBUG = 3;

	private final static Object waitTimeFileLock = new Object();
	private final static Object fileSizeLock = new Object();
	private final static Object downloadTimeLock = new Object();

	private static int mLevel = INFO;

	private Logger() {
		throw new AssertionError();
	}

	public static void setLevel(int level) {
		mLevel = level;
	}

	public static void error(String msg) {
		if (mLevel < ERROR)
			return;

		System.err.println("ERROR: " + msg);
	}

	public static void info(String msg) {
		if (mLevel < INFO)
			return;

		System.out.println("INFO: " + msg);
	}

	public static void debug(String msg) {
		if (mLevel < DEBUG)
			return;

		System.out.println("DEBUG: " + msg);
	}

	public static void waitTime(double time) {
		synchronized (waitTimeFileLock) {
			try {
				FileWriter file = new FileWriter("waitTimes.txt");
				BufferedWriter out = new BufferedWriter(file);
				out.write(Double.toString(time) + "\n");
				out.close();
			} catch (Exception e) {
				Logger.error(e.getMessage());
			}
		}
	}

	public static void fileSize(int size) {
		synchronized (fileSizeLock) {
			try {
				FileWriter file = new FileWriter("fileSizes.txt", true);
				BufferedWriter out = new BufferedWriter(file);
				out.write(Integer.toString(size) + "\n");
				out.close();
			} catch (Exception e) {
				Logger.error(e.getMessage());
			}
		}
	}

	public static void downloadTime(double time) {
		synchronized (downloadTimeLock) {
			try {
				FileWriter file = new FileWriter("downloadTimes.txt", true);
				BufferedWriter out = new BufferedWriter(file);
				out.write(Double.toString(time) + "\n");
				out.close();
			} catch (Exception e) {
				Logger.error(e.getMessage());
			}
		}
	}
}
