package net.gregbeaty.clientstream;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.UnknownHostException;
import java.nio.ByteBuffer;
import java.util.Random;

import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;
import net.gregbeaty.clientstream.helper.Operations;

public class Client implements Endpoint {

	private InetAddress routerAddress;
	private BroadcastThread broadcastThread;
	private ServerThread serverThread;
	private ClientThread clientThread;

	public Client(InetAddress r) {
		routerAddress = r;

		broadcastThread = new BroadcastThread();
		broadcastThread.start();

		serverThread = new ServerThread();
		serverThread.start();
	}

	private class BroadcastThread extends Thread {
		private DatagramSocket broadcastSocket;
		private volatile boolean stop = false;

		@Override
		public void run() {
			try {
				broadcastSocket = new DatagramSocket();

				while (!stop) {
					byte[] outputBuf = new byte[1024];

					String broadcast = Constants.BROADCAST_MSG;
					outputBuf = broadcast.getBytes();

					DatagramPacket packet = new DatagramPacket(outputBuf,
							outputBuf.length, routerAddress,
							Constants.ROUTER_DISCOVERY_PORT);

					try {
						broadcastSocket.send(packet);
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
				server = new ServerSocket(Constants.SERVER_STREAM_PORT);

				while (!stop) {
					Socket client = server.accept();
					InputStream clientInput = client.getInputStream();
					OutputStream clientOutput = client.getOutputStream();

					ByteBuffer inputBuf = ByteBuffer.allocate(1024);
					byte[] outputBuf = new byte[1024];

					clientInput.read(inputBuf.array());
					String request = new String(inputBuf.array());
					request = request.replaceAll("\0", "");

					Logger.debug("SERVER-Received request: " + request);

					if (request.equalsIgnoreCase(Operations.GET_FILE)) {
						String dir = System.getProperty("user.dir");
						Random random = new Random();
						int randomFileNum = random.nextInt(4);

						File file = new File(dir + "/files/" + "file"
								+ randomFileNum + ".wav");
						String response = Operations.FILE_METADATA + ":"
								+ file.getName() + ":" + file.length();

						Logger.info("SERVER-Sending response: " + response);

						outputBuf = response.getBytes();
						clientOutput.write(outputBuf);
						clientOutput.flush();

						BufferedReader fileReader = new BufferedReader(
								new FileReader(file));

						String line;
						while ((line = fileReader.readLine()) != null) {
							outputBuf = line.getBytes();
							clientOutput.write(outputBuf);
							clientOutput.flush();
						}

						fileReader.close();

						Logger.info("SERVER-Transfered file: " + file.getName());
					}
				
					while (true) {
						int bytes = clientInput.read();
						Logger.debug("Available bytes: " + Integer.toString(bytes));
						if (clientInput.read() == -1)
							break;
						
						try {
							sleep(100);
						} catch (InterruptedException e) {
							e.printStackTrace();
						}
					}
					
					Logger.debug("SERVER-Client disconnected");
					
					try {
						clientInput.close();
						clientOutput.close();
						client.close();
					} catch (IOException e) {

					}
				}
			} catch (IOException e) {
				if (server == null || !server.isClosed()) {
					Logger.error("SERVER-Socket error");
					Logger.debug(e.toString());
				}
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

	private class ClientThread extends Thread {
		private Socket client;

		@Override
		public void run() {
			try {
				client = new Socket(routerAddress, Constants.ROUTER_STREAM_PORT);
				InputStream clientInput = client.getInputStream();
				OutputStream clientOutput = client.getOutputStream();

				ByteBuffer inputBuf = ByteBuffer.allocate(1024);
				byte[] outputBuf = new byte[1024];

				outputBuf = Operations.GET_FILE.getBytes();
				clientOutput.write(outputBuf);
				clientOutput.flush();

				clientInput.read(inputBuf.array());
				String response = new String(inputBuf.array());
				response = response.replaceAll("\0", "");

				Logger.debug("CLIENT-Received response: " + response);

				if (response.split(":")[0]
						.equalsIgnoreCase(Operations.FILE_METADATA)) {
					String fileName = response.split(":")[1];
					int fileSize = Integer.parseInt(response.split(":")[2]);
					Logger.info("CLIENT-Downloading - Name: " + fileName
							+ " Length: " + fileSize + " bytes");

					String dir = System.getProperty("user.dir");
					File file = new File(dir + "/downloads/" + fileName);
					FileOutputStream fileStream = new FileOutputStream(file);

					int received = 0;
					int total = fileSize;

					inputBuf.clear();
					while (received < total) {
						received += clientInput.read(inputBuf.array());
						fileStream.write(inputBuf.array());
						fileStream.flush();
					}

					Logger.info("CLIENT-Received: " + file.length() + " bytes");

					try {
						fileStream.close();
					} catch (IOException e) {

					}
					
					clientInput.close();
					clientOutput.close();
					client.close();
				}
			} catch (IOException e) {
				if (client == null || !client.isClosed()) {
					Logger.error("CLIENT-Socket error");
					Logger.debug(e.toString());
				}
			}
		}

		public synchronized void cancel() {
			try {
				client.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}

	public void downloadFile() {
		if (clientThread == null) {
			Logger.info("CLIENT-Downloading thread starting");
		} else if (!clientThread.isAlive()) {
			Logger.info("CLIENT-Downloading thread restarted");
		} else {
			Logger.info("CLIENT-Downloading already in progress");
			return;
		}

		clientThread = new ClientThread();
		clientThread.start();
	}

	@Override
	public void stop() {
		broadcastThread.cancel();
		try {
			broadcastThread.join();
		} catch (InterruptedException e) {
			e.printStackTrace();
		}

		serverThread.cancel();
		try {
			serverThread.join();
		} catch (InterruptedException e) {
			e.printStackTrace();
		}

		clientThread.cancel();
		try {
			clientThread.join();
		} catch (InterruptedException e) {
			e.printStackTrace();
		}
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
					+ Constants.SERVER_STREAM_PORT;
		}
	}
}
