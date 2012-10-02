package net.gregbeaty.clientstream.ui;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

import net.gregbeaty.clientstream.Client;
import net.gregbeaty.clientstream.Endpoint;
import net.gregbeaty.clientstream.Router;
import net.gregbeaty.clientstream.Server;
import net.gregbeaty.clientstream.helper.Constants;
import net.gregbeaty.clientstream.helper.Logger;

public class ClientStream {

	private static Endpoint endpoint;
	private static BufferedReader inputReader;

	public static void main(String[] args) {
		Logger.setLevel(Logger.DEBUG);
		inputReader = new BufferedReader(new InputStreamReader(System.in));

		printMenu();
		waitForExit();
	}

	private static void printMenu() {
		System.out
				.println("Type the type of endpoint you want to create (Server, Client, Router): ");
		while (true) {
			try {
				String input = inputReader.readLine();
				switch (input) {
				case "Server":
					endpoint = new Server();
					return;
				case "Client":
					endpoint = new Client();
					return;
				case "Router":
					endpoint = new Router();
					return;
				default:
					System.out.println("Invalid input: " + input);
				}
			} catch (IOException e) {
				Logger.error("Failed to read input");
			}
		}
	}

	private static void waitForExit() {
		boolean running = true;
		while (running) {
			try {
				String input = inputReader.readLine();
				if (input.equalsIgnoreCase("exit")) {
					endpoint.stop();
					running = false;
				} else {
					processCommand(input);
				}
			} catch (IOException e) {
				Logger.error("Failed to read input");
			}
		}
	}

	private static String getEndpointType() {
		if (endpoint.getClass().equals(Server.class)) {
			return Constants.SERVER;
		} else if (endpoint.getClass().equals(Client.class)) {
			return Constants.CLIENT;
		} else if (endpoint.getClass().equals(Router.class)) {
			return Constants.ROUTER;
		}

		return null;
	}

	private static void processCommand(String command) {
		if (command.equalsIgnoreCase("help")) {
			if (getEndpointType().equalsIgnoreCase(Constants.SERVER)) {
				System.out.println("filelist - get list of files");
			} else if (getEndpointType().equalsIgnoreCase(Constants.CLIENT)) {
				System.out.println("filelist - get list of hosted files");
			} else if (getEndpointType().equalsIgnoreCase(Constants.ROUTER)) {
				System.out
						.println("filelist - get list of files hosted on each server");
				System.out.println("servers - get list of servers");
			}
			System.out.println("address - get ip address");
			System.out.println("port - get port");
			System.out.println("exit - close the program");
		} else if (command.equalsIgnoreCase("filelist")) {
			endpoint.printFileList();
		} else if (command.equalsIgnoreCase("servers")) {
			if (getEndpointType().equalsIgnoreCase(Constants.ROUTER)) {
				Router router = (Router) endpoint;
				router.printServerList();
			}
		} else if (command.equalsIgnoreCase("address")) {
			System.out.println(endpoint.getAddress());
		} else if (command.equalsIgnoreCase("port")) {
			System.out.println(endpoint.getPort());
		}
	}
}
