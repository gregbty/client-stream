package net.gregbeaty.clientstream;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.util.ArrayList;

import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;

public class Router {
	private ArrayList<String> servers;

	public Router() {
		servers = new ArrayList<String>();
	}

	public void getServerList() {
		servers.clear();

		InetAddress group;
		try {
			DatagramSocket socket = new DatagramSocket(Constants.BROADCAST_PORT);
			group = InetAddress.getByName(Constants.BROADCAST_ADDRESS);

			byte[] output = new byte[256];
			DatagramPacket broadcast = new DatagramPacket(output,
					output.length, group, Constants.BROADCAST_PORT);
			socket.send(broadcast);

			DatagramPacket response;
			int count = 0;
			while (count < 10) {
				byte[] input = new byte[256];
				response = new DatagramPacket(input, input.length);
				socket.receive(response);

				String data = new String(response.getData());
				if (data.equalsIgnoreCase(Constants.BROADCAST_MSG)) {
					InetAddress address = response.getAddress();
					int port = response.getPort();

					boolean serverExists = false;
					for (String server : servers) {
						if (server.split(":")[0].equalsIgnoreCase(address
								.toString())) {
							serverExists = true;
							break;
						}
					}

					if (serverExists)
						continue;

					String server = address + ":" + port;
					servers.add(server);
				}

				count++;
			}

			socket.close();
		} catch (IOException e) {
			Logger.error("Failed to send broadcast");
			Logger.debug(e.toString());
		}
	}

	public void printServerList() {
		for (String server : servers) {
			System.out.println(server);
		}
	}
}
