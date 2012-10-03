package net.gregbeaty.clientstream;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
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
	private BroadcastThread broadcastThread;
	private RouterThread routerThread;
	private StreamThread streamThread;

	public Client(InetAddress r) {
		classLoader = Client.class.getClassLoader();
		routerAddress = r;

		broadcastThread = new BroadcastThread();
		broadcastThread.start();

		routerThread = new RouterThread();
		routerThread.start();

		streamThread = new StreamThread();
		streamThread.start();
	}

	@Override
	public void stop() {
		broadcastThread.cancel();
		routerThread.cancel();
		streamThread.cancel();
	}

	private class BroadcastThread extends Thread {
		private DatagramSocket socket;
		private volatile boolean stop = false;

		@Override
		public void run() {
			try {
				socket = new DatagramSocket();

				while (!stop) {
					byte[] outputBuf = new byte[1024];
					String broadcast = Constants.BROADCAST_MSG;
					outputBuf = broadcast.getBytes();

					DatagramPacket packet = new DatagramPacket(outputBuf,
							outputBuf.length, routerAddress, Constants.PORT);

					try {
						socket.send(packet);
					} catch (IOException e) {
						Logger.error("Failed to send message");
						Logger.debug(e.toString());
					}
				}

			} catch (IOException e) {
				Logger.error("Broadcast socket error");
				Logger.debug(e.toString());
			}
		}

		public synchronized void cancel() {
			socket.close();
			stop = true;
		}
	}

	private class RouterThread extends Thread {
		private DatagramSocket socket;
		private volatile boolean stop = false;

		@Override
		public void run() {

			try {
				socket = new DatagramSocket(Constants.PORT);

				while (!stop) {
					listenForIncoming();
				}

			} catch (IOException e) {
				Logger.error("Router socket error");
				Logger.debug(e.toString());
			}
		}

		private void listenForIncoming() {
			byte[] inputBuf = new byte[1024];
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

		public synchronized void getFileList() {
			byte[] outputBuf = new byte[1024];
			String fileCommand = Operations.GET_FILE_LIST;
			outputBuf = fileCommand.getBytes();

			DatagramPacket outgoing = new DatagramPacket(outputBuf,
					outputBuf.length, routerAddress, Constants.PORT);

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
						outputBuf.length, routerAddress, Constants.PORT);

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
		private ServerSocket server;
		private volatile boolean stop = false;

		byte[] inputBuf = new byte[1024];

		@Override
		public void run() {
			while (!stop) {
				try {
					server = new ServerSocket(Constants.STREAM_PORT);
					Socket client = server.accept();

					InputStream input = client.getInputStream();
					OutputStream output = client.getOutputStream();

					boolean metadataSent = false;

					String command;

					String fileName;
					int fileSize;
					boolean fileDownloadStart = false;
					while (true) {
						int bytes = input.read(inputBuf, 0, inputBuf.length);

						if (!metadataSent) {
							String data = new String(inputBuf, 0,
									inputBuf.length);
							command = data.split("|")[0];
							if (command.equalsIgnoreCase(Operations.GET_FILE)) {
								fileName = data.split("|")[1];
								fileSize = Integer.parseInt(data.split("|")[2]);
								metadataSent = true;
								fileDownloadStart = true;
							}
						} else if (fileDownloadStart) {

						}
					}

					output.close();
					input.close();
					client.close();
					server.close();
				} catch (IOException e) {
					if (server.isClosed()) {
						Logger.error("Server socket error");
						Logger.debug(e.toString());
					}
				}
			}
		}

		public synchronized void cancel() {
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
			return address.getHostAddress() + ":" + Constants.PORT;
		}
	}
}
