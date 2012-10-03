package net.gregbeaty.clientstream.ui;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.InetAddress;

import net.gregbeaty.clientstream.Client;
import net.gregbeaty.clientstream.Endpoint;
import net.gregbeaty.clientstream.Router;
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
				.println("Type the type of endpoint you want to create (Client, Router): ");
		while (true) {
			try {
				String input = inputReader.readLine();
				switch (input) {
				case "Client":
					createClient();
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

	private static void createClient() {
		System.out.println("Enter the address of the Router: ");
		while (true) {
			try {
				String input = inputReader.readLine();
				try {
					InetAddress routerAddress = InetAddress.getByName(input);
					endpoint = new Client(routerAddress);
					return;
				} catch (IOException e) {
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
		if (endpoint.getClass().equals(Client.class)) {
			return Constants.CLIENT;
		} else if (endpoint.getClass().equals(Router.class)) {
			return Constants.ROUTER;
		}

		return null;
	}

	private static void processCommand(String command) {
		if (command.equalsIgnoreCase("help")) {
			if (getEndpointType().equalsIgnoreCase(Constants.ROUTER)) {
				System.out.println("servers - get list of servers");
			} else if (getEndpointType().equalsIgnoreCase(Constants.CLIENT)) {
				System.out.println("filelist - get list of hosted files");
			}
			System.out.println("hostinfo - get host info");
			System.out.println("exit - close the program");
		} else if (command.equalsIgnoreCase("filelist")) {
			if (getEndpointType().equalsIgnoreCase(Constants.CLIENT)) {
				Client Client = (Client) endpoint;
				Client.getFileList();
			}
		} else if (command.equalsIgnoreCase("servers")) {
			if (getEndpointType().equalsIgnoreCase(Constants.ROUTER)) {
				Router router = (Router) endpoint;
				router.getServerList();
			}
		} else if (command.equalsIgnoreCase("hostinfo")) {
			System.out.println(endpoint.getHostInfo());
		}
	}
}
