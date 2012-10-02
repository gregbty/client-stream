package net.gregbeaty.clientstream;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;

import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;

public class Server {

	public class ServerThread extends Thread {
		DatagramSocket socket = new DatagramSocket(Constants.STREAMING_PORT);
		byte[] input = new byte[1024];
		byte[] output = new byte[1024];

		volatile boolean stop = false;

		public ServerThread() throws SocketException {

		}

		@Override
		public void run() {
			while (!stop) {
				DatagramPacket incomingPacket = new DatagramPacket(input,
						input.length);

				try {
					socket.receive(incomingPacket);
				} catch (IOException e) {
					Logger.error("Failed to receive incoming packet");
					Logger.debug(e.toString());
				}

				output = parseInput(incomingPacket.getData());

				InetAddress address = incomingPacket.getAddress();
				int port = incomingPacket.getPort();

				DatagramPacket outgoingPacket = new DatagramPacket(output,
						output.length, address, port);

				try {
					socket.send(outgoingPacket);
				} catch (IOException e) {
					Logger.error("Failed to send output packet");
					Logger.debug(e.toString());
				}
			}
		}

		private byte[] parseInput(byte[] input) {
			String data = new String(input);

			// TODO: Parse input

			return data.getBytes();
		}

		public synchronized void cancel() {
			stop = true;
		}
	}
}
