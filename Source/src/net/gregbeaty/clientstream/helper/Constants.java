package net.gregbeaty.clientstream.helper;

public class Constants {
	public final static int PORT = 8000;
	public final static int STREAM_PORT = 8800;
	public final static String BROADCAST_MSG = "SERVER_AVAIL";

	public final static String CLIENT = "CLIENT";
	public final static String ROUTER = "ROUTER";

	public final static String DOWNLOAD_DIR = "Downloads";

	private Constants() {
		throw new AssertionError();
	}
}
