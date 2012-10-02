package net.gregbeaty.clientstream.helper;

public class Constants {	
	public final static String BROADCAST_ADDRESS = "255.255.255.255";
	public final static int BROADCAST_PORT = 8000;
	public final static String BROADCAST_MSG = "FIND_SERVERS";
	public final static String BROADCAST_RESPONSE = "SERVER_AVAIL";
	
	public final static int STREAMING_PORT = 8800;

	private Constants() {
		throw new AssertionError();
	}
}
