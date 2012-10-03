package net.gregbeaty.clientstream;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.UnknownHostException;
import java.util.ArrayList;

import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;
import net.gregbeaty.clientstream.helper.Operations;

public class Router implements Endpoint {
	private ArrayList<String> servers;
	private ServerThread serverThread;

	public Router() {
		servers = new ArrayList<String>();

		serverThread = new ServerThread();
		serverThread.start();
		Logger.debug("Discovery thread started");
	}

	private class ServerThread extends Thread {
		DatagramSocket socket;
		volatile boolean stop = false;

		@Override
		public void run() {
			try {
				socket = new DatagramSocket(Constants.PORT);

				while (!stop) {
					listenForIncoming();
				}
			} catch (IOException e) {
				if (!socket.isClosed()) {
					Logger.error("Discovery socket error");
					Logger.debug(e.toString());
				}
			}
		}

		private void listenForIncoming() {
			byte[] inputBuf = new byte[1000];
			DatagramPacket incoming = new DatagramPacket(inputBuf,
					inputBuf.length);

			try {
				socket.receive(incoming);

				String data = new String(incoming.getData(),
						incoming.getOffset(), incoming.getLength());
				parseData(incoming, data);
			} catch (IOException e) {
				if (!socket.isClosed()) {
					Logger.error("Failed to receive message");
					Logger.debug(e.toString());
				}
			}
		}

		private void parseData(DatagramPacket packet, String data) {
			String request = "";
			if (data.contains("|")) {
				request = data.split("|")[0];
			} else {
				request = data;
			}

			Logger.debug("Received operation: " + request);

			if (request.equalsIgnoreCase(Operations.SEND_FILE_LIST)) {
			} else if (request.equalsIgnoreCase(Operations.GET_FILE_LIST)) {
				getFileList();
			} else if (request.equalsIgnoreCase(Constants.BROADCAST_MSG)) {
				addServer(packet.getAddress().getHostAddress());
			}
		}

		private void addServer(String address) {
			int port = Constants.STREAM_PORT;

			boolean serverExists = false;
			for (String server : servers) {
				if (server.split(":")[0].equalsIgnoreCase(address.toString())) {
					serverExists = true;
					break;
				}
			}

			if (!serverExists) {
				String server = address + ":" + port;
				servers.add(server);
			}
		}

		public synchronized void getFileList() {
			for (String server : servers) {
				InetAddress address;
				try {
					address = InetAddress.getByName(server.split(":")[0]);
					int port = Constants.PORT;

					byte[] outputBuf = new byte[1024];
					String fileCommand = Operations.GET_FILE_LIST;
					outputBuf = fileCommand.getBytes();

					DatagramPacket outgoing = new DatagramPacket(outputBuf,
							outputBuf.length, address, port);

					try {
						socket.send(outgoing);

					} catch (IOException e) {
						Logger.error("Failed to send message");
						Logger.debug(e.toString());
					}
				} catch (UnknownHostException e) {
					Logger.error("Failed to parse server address");
					Logger.debug(e.toString());
				}
			}

		}

		public synchronized void cancel() {
			socket.close();
			stop = true;
		}
	}

	public void getServerList() {
		for (String server : servers) {
			System.out.println(server);
		}
	}

	@Override
	public void stop() {
		serverThread.cancel();
	}

	@Override
	public String getHostInfo() {
		InetAddress address = null;
		try {
			address = InetAddress.getLocalHost();
		} catch (UnknownHostException e) {
			Logger.error("Failed to get host information");
			Logger.debug(e.toString());
		}

		if (address == null) {
			return null;
		} else {
			return address.getHostAddress() + ":" + Constants.PORT;
		}
	}
}
