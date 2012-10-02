package net.gregbeaty.clientstream.threading;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.InetAddress;
import java.net.MulticastSocket;
import java.net.SocketException;
import java.net.UnknownHostException;

import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;

public class BroadcastThread extends Thread {
	MulticastSocket mSocket;
	byte[] mOutput = new byte[1024];

	public BroadcastThread() throws SocketException, UnknownHostException {
	}

	@Override
	public void run() {
		String broadcastMsg = Constants.BROADCAST_MSG;
		mOutput = broadcastMsg.getBytes();

		InetAddress group;
		try {
			mSocket = new MulticastSocket();
			group = InetAddress.getByName(Constants.BROADCAST_ADDRESS);
			DatagramPacket packet = new DatagramPacket(mOutput, mOutput.length,
					group, Constants.BROADCAST_PORT);
			mSocket.send(packet);

			try {
				sleep(100);
			} catch (InterruptedException e) {

			}
		} catch (UnknownHostException e) {
			Logger.error("Failed to join multicast broadcast group");
			Logger.debug(e.toString());
		} catch (IOException e) {
			Logger.error("Failed to create multicast socket");
			Logger.debug(e.toString());
		}

		mSocket.close();
	}
}
