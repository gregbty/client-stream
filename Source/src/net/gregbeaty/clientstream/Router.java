package net.gregbeaty.clientstream;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.UnknownHostException;
import java.util.ArrayList;

import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;
import net.gregbeaty.clientstream.helper.Operations;

public class Router implements Endpoint {
	private ArrayList<String> servers;
	private BroadcastThread broadcastThread;
	private ServerThread serverThread;

	public Router() {
		servers = new ArrayList<String>();

		broadcastThread = new BroadcastThread();
		broadcastThread.start();

		serverThread = new ServerThread();
		serverThread.start();
	}

	private class BroadcastThread extends Thread {
		DatagramSocket broadcastSocket;
		volatile boolean stop = false;

		@Override
		public void run() {
			try {
				broadcastSocket = new DatagramSocket(
						Constants.ROUTER_DISCOVERY_PORT);

				while (!stop) {
					listenForIncoming();
				}
			} catch (IOException e) {
				if (!broadcastSocket.isClosed()) {
					Logger.error("Discovery socket error");
					Logger.debug(e.toString());
				}
			}
		}

		private void listenForIncoming() {
			byte[] inputBuf = new byte[1024];
			DatagramPacket incoming = new DatagramPacket(inputBuf,
					inputBuf.length);

			try {
				broadcastSocket.receive(incoming);

				String request = new String(incoming.getData(),
						incoming.getOffset(), incoming.getLength());

				if (request.equalsIgnoreCase(Constants.BROADCAST_MSG)) {
					addServer(incoming.getAddress().getHostAddress());
				}
			} catch (IOException e) {
				if (!broadcastSocket.isClosed()) {
					Logger.error("Failed to receive message");
					Logger.debug(e.toString());
				}
			}
		}

		private void addServer(String address) {
			int port = Constants.SERVER_STREAM_PORT;

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

		public synchronized void cancel() {
			broadcastSocket.close();
			stop = true;
		}
	}

	private class ServerThread extends Thread {
		private ServerSocket server;
		private volatile boolean stop = false;

		@Override
		public void run() {
			try {
				server = new ServerSocket(Constants.ROUTER_STREAM_PORT);

				while (!stop) {
					Socket client = server.accept();
					InputStream clientInput = client.getInputStream();
					OutputStream clientOutput = client.getOutputStream();

					byte[] inputBuf = new byte[1024];
					byte[] outputBuf = new byte[1024];

					clientInput.read(inputBuf);
					String request = new String(inputBuf);
					request = request.replaceAll("\0", "");

					Logger.debug("ROUTER-Received request from client: " + request);
					
					if (request.equalsIgnoreCase(Operations.GET_FILE)) {
						for (String server : servers) {
							if (client.getInetAddress().getHostAddress() == server)
								continue;

							InetAddress address = InetAddress.getByName(server);
							int port = Constants.SERVER_STREAM_PORT;

							Socket routedSocket = new Socket(address, port);
							InputStream routedInput = routedSocket
									.getInputStream();
							OutputStream routedOutput = routedSocket
									.getOutputStream();

							outputBuf = request.getBytes();
							routedOutput.write(outputBuf);
							routedOutput.flush();
							
							routedInput.read(inputBuf);
							request = new String(inputBuf);
							request = request.replaceAll("\0", "");
							
							Logger.debug("ROUTER-Received response from server: " + request);
							
							if (request.split(":")[0]
									.equalsIgnoreCase(Operations.FILE_METADATA)) {
								outputBuf = request.getBytes();
								clientOutput.write(outputBuf);
								clientOutput.flush();

								while (routedInput.read(inputBuf) != -1) {
									outputBuf = inputBuf;
									clientOutput.write(outputBuf);
									clientOutput.flush();
								}

								routedInput.close();
								routedOutput.close();
								routedSocket.close();
							}
							
							break;
						}
					}

					clientInput.close();
					clientOutput.close();
					client.close();
				}
			} catch (IOException e) {
				Logger.debug(e.toString());
			}
		}

		public synchronized void cancel() {
			try {
				server.close();
			} catch (IOException e) {
			}

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
		broadcastThread.cancel();
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
			return address.getHostAddress() + ":"
					+ Constants.ROUTER_STREAM_PORT;
		}
	}
}
