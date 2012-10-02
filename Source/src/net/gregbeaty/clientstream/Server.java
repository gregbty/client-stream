package net.gregbeaty.clientstream;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.InetAddress;
import java.net.MulticastSocket;
import java.net.UnknownHostException;

import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;

public class Server implements Endpoint {

	private BroadcastThread broadcastThread;

	public Server() {
		broadcastThread = new BroadcastThread();
		broadcastThread.start();

		Logger.debug("Broadcast thread started");
	}

	@Override
	public void stop() {
		broadcastThread.cancel();
	}

	public class BroadcastThread extends Thread {
		MulticastSocket socket;
		InetAddress group;
		byte[] input = new byte[256];
		byte[] output = new byte[256];

		volatile boolean stop = false;

		@Override
		public void run() {
			try {
				socket = new MulticastSocket();

				group = InetAddress.getByName(Constants.BROADCAST_ADDRESS);
				socket.joinGroup(group);

				receiveBroadcast();

			} catch (UnknownHostException e) {
				Logger.error("Failed to join multicast broadcast group");
				Logger.debug(e.toString());
			} catch (IOException e) {
				Logger.error("Failed to create multicast socket");
				Logger.debug(e.toString());
			}
		}

		private void receiveBroadcast() {
			while (!stop) {
				DatagramPacket packet = new DatagramPacket(input, input.length);
				try {
					socket.receive(packet);

					String data = new String(packet.getData());
					if (data.equalsIgnoreCase(Constants.BROADCAST_MSG)) {
						InetAddress address = packet.getAddress();
						int port = packet.getPort();
						sendResponse(address, port);
					}
				} catch (IOException e) {
					if (!socket.isClosed()) {
						Logger.error("Failed to recieve packet");
						Logger.debug(e.toString());
					}
				}
			}
		}

		private void sendResponse(InetAddress address, int port) {
			String response = Constants.BROADCAST_RESPONSE;
			output = response.getBytes();

			DatagramPacket packet = new DatagramPacket(output, output.length,
					address, port);
			try {
				socket.send(packet);
			} catch (IOException e) {
				Logger.error("Failed to send response");
				Logger.debug(e.toString());
			}
		}

		public synchronized void cancel() {
			socket.close();
			stop = true;
		}
	}

	@Override
	public String getAddress() {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public int getPort() {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public void printFileList() {
		// TODO Auto-generated method stub
		
	}
}
