package net.gregbeaty.clientstream;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.InetAddress;
import java.net.MulticastSocket;
import java.util.ArrayList;

import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;

public class Router {
	private ArrayList<String> mServers;

	public Router() {
		mServers = new ArrayList<String>();
	}

	public void getServerList() {
		mServers.clear();

		InetAddress group;
		try {
			MulticastSocket socket = new MulticastSocket(
					Constants.BROADCAST_PORT);
			group = InetAddress.getByName(Constants.BROADCAST_ADDRESS);
			socket.joinGroup(group);

			DatagramPacket packet;
			int count = 0;
			while (count < 10) {
				byte[] input = new byte[256];
				packet = new DatagramPacket(input, input.length);
				socket.receive(packet);

				String data = new String(packet.getData());
				if (data.equalsIgnoreCase(Constants.BROADCAST_MSG)) {
					InetAddress address = packet.getAddress();
					int port = packet.getPort();

					boolean serverExists = false;
					for (String server : mServers) {
						if (server.split(":")[0].equalsIgnoreCase(address
								.toString())) {
							serverExists = true;
							break;
						}
					}

					if (serverExists)
						continue;

					String server = address + ":" + port;
					mServers.add(server);
				}
				count++;
			}

			socket.leaveGroup(group);
			socket.close();
		} catch (IOException e) {
			Logger.error("Failed to create multicast broadcast socket");
			Logger.debug(e.toString());
		}
	}
}
