package net.gregbeaty.clientstream;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.UnknownHostException;
import java.util.Arrays;
import java.util.List;

import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;
import net.gregbeaty.clientstream.helper.Operations;

public class Client implements Endpoint {

	private ClassLoader classLoader;
	private InetAddress routerAddress;
	private RouterThread routerThread;
	private StreamThread streamThread;

	public Client(InetAddress r) {
		classLoader = Client.class.getClassLoader();
		routerAddress = r;

		routerThread = new RouterThread();
		routerThread.start();
		Logger.debug("Discovery thread started");

		streamThread = new StreamThread();
		streamThread.start();
		Logger.debug("Stream thread started");
	}

	@Override
	public void stop() {
		routerThread.cancel();
		streamThread.cancel();
	}

	private class RouterThread extends Thread {
		private DatagramSocket socket;
		private volatile boolean stop = false;

		@Override
		public void run() {

			try {
				socket = new DatagramSocket();

				while (!stop) {
					broadcastAvailability();
					listenForIncoming();
				}

			} catch (IOException e) {
				Logger.error("Broadcast socket error");
				Logger.debug(e.toString());
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
				parseData(data);
			} catch (IOException e) {
				Logger.error("Failed to receive message");
				Logger.debug(e.toString());
			}
		}

		private void parseData(String data) {
			String request = "";
			if (data.contains("|")) {
				request = data.split("|")[0];
			} else {
				request = data;
			}

			Logger.debug("Received operation: " + request);

			if (request.equalsIgnoreCase(Operations.SEND_FILE_LIST)) {
				List<String> fileList = Arrays.asList(data.split("|")[1]
						.split(";"));
				for (String file : fileList) {
					System.out.println(file);
				}
			} else if (request.equalsIgnoreCase(Operations.GET_FILE_LIST)) {
				sendFileList();
			}
		}

		public synchronized void broadcastAvailability() {
			byte[] outputBuf = new byte[1024];
			String broadcast = Constants.BROADCAST_MSG;
			outputBuf = broadcast.getBytes();

			DatagramPacket packet = new DatagramPacket(outputBuf,
					outputBuf.length, routerAddress, Constants.BROADCAST_PORT);

			try {
				socket.send(packet);
			} catch (IOException e) {
				Logger.error("Failed to send message");
				Logger.debug(e.toString());
			}
		}

		public synchronized void getFileList() {
			byte[] outputBuf = new byte[1024];
			String fileCommand = Operations.GET_FILE_LIST;
			outputBuf = fileCommand.getBytes();

			DatagramPacket outgoing = new DatagramPacket(outputBuf,
					outputBuf.length, routerAddress, Constants.BROADCAST_PORT);

			try {
				socket.send(outgoing);

			} catch (IOException e) {
				Logger.error("Failed to send message");
				Logger.debug(e.toString());
			}
		}

		public synchronized void sendFileList() {
			BufferedReader reader = getResourcesAsStream();
			String fileList = "";
			String file;
			try {
				while ((file = reader.readLine()) != null) {
					if (file.endsWith(".txt")) {
						fileList += file + ";";
					}
				}
				reader.close();

				byte[] outputBuf = new byte[1024];
				String fileCommand = Operations.SEND_FILE_LIST + "|" + fileList;
				outputBuf = fileCommand.getBytes();

				DatagramPacket outgoing = new DatagramPacket(outputBuf,
						outputBuf.length, routerAddress,
						Constants.BROADCAST_PORT);

				try {
					socket.send(outgoing);

				} catch (IOException e) {
					Logger.error("Failed to send message");
					Logger.debug(e.toString());
				}

			} catch (IOException e) {
				Logger.error("Failed to get file list");
				Logger.debug(e.toString());
			}
		}

		public synchronized void cancel() {
			socket.close();
			stop = true;
		}
	}

	private class StreamThread extends Thread {
		ServerSocket server;
		PrintWriter output = null;
		volatile boolean stop = false;

		@Override
		public void run() {
			Socket socket = null;

			try {
				server = new ServerSocket(Constants.STREAMING_PORT);

				while (!stop) {
					socket = server.accept();
					output = new PrintWriter(socket.getOutputStream(), true);

					Logger.debug("Connected to: " + socket.getInetAddress()
							+ ":" + socket.getPort());

					try {
						BufferedReader input = new BufferedReader(
								new InputStreamReader(socket.getInputStream()));
						while (!input.ready()) {
							Logger.debug("Received: " + input.readLine());
						}

						input.close();
					} catch (IOException e) {
						Logger.error("Failed to get input from connected socket");
						Logger.debug(e.toString());
					}

					output.close();
					socket.close();
				}
			} catch (IOException e) {
				if (!server.isClosed()) {
					Logger.error("Failed to bind/close server socket");
					Logger.debug(e.toString());
				}
			}
		}

		public synchronized void cancel() {
			try {
				server.close();
			} catch (IOException e) {
				Logger.error("Failed to close server socket");
				Logger.debug(e.toString());
			}
			stop = true;
		}
	}

	public BufferedReader getResourceStream(String resource) {
		InputStream stream = classLoader.getResourceAsStream(resource);
		BufferedReader reader = new BufferedReader(
				new InputStreamReader(stream));
		return reader;
	}

	public BufferedReader getResourcesAsStream() {
		return getResourceStream(".");
	}

	public void getFileList() {
		routerThread.getFileList();
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
			return address.getHostAddress() + ":" + Constants.STREAMING_PORT;
		}
	}

}
