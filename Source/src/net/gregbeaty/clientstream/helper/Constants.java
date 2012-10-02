package net.gregbeaty.clientstream.helper;

public class Constants {
	public final static String BROADCAST_ADDRESS = "224.0.0.1";
	public final static int BROADCAST_PORT = 8000;
	public final static String BROADCAST_MSG = "FIND_SERVERS";
	public final static String BROADCAST_RESPONSE = "SERVER_AVAIL";

	public final static int STREAMING_PORT = 8800;

	public final static String SERVER = "SERVER";
	public final static String CLIENT = "CLIENT";
	public final static String ROUTER = "ROUTER";

	private Constants() {
		throw new AssertionError();
	}
}
