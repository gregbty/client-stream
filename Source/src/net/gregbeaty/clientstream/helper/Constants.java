package net.gregbeaty.clientstream.helper;

public class Constants {
	public final static String BROADCAST_ADDRESS = "224.0.0.255";
	public final static int BROADCAST_PORT = 8000;
	public final static String BROADCAST_MSG = "SERVER_AVAIL";

	public final static int STREAMING_PORT = 8800;

	public final static String CLIENT = "CLIENT";
	public final static String ROUTER = "ROUTER";

	public final static String DOWNLOAD_DIR = "Downloads";

	private Constants() {
		throw new AssertionError();
	}
}
