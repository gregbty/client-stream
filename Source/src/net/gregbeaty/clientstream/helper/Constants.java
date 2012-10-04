package net.gregbeaty.clientstream.helper;

public class Constants {
	public final static int ROUTER_DISCOVERY_PORT = 8808;
	public final static int SERVER_BROADCAST_PORT = 8809;
	
	public final static int ROUTER_STREAM_PORT = 8810;
	public final static int SERVER_STREAM_PORT = 8811;
	public final static int CLIENT_STREAM_PORT = 8812;
	public final static String BROADCAST_MSG = "SERVER_AVAIL";

	public final static String CLIENT = "CLIENT";
	public final static String ROUTER = "ROUTER";

	public final static String DOWNLOAD_DIR = "Downloads";

	private Constants() {
		throw new AssertionError();
	}
}
