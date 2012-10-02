package net.gregbeaty.clientstream.ui;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

import net.gregbeaty.clientstream.Client;
import net.gregbeaty.clientstream.Router;
import net.gregbeaty.clientstream.Server;
import net.gregbeaty.clientstream.helper.Logger;

public class ClientStream {

	public static void main(String[] args) {
		printMenu();
		waitForExit();
	}

	private static void printMenu() {
		BufferedReader reader = new BufferedReader(new InputStreamReader(
				System.in));

		System.out
				.println("Type the type of machine you want to create (Server, Client, Router): ");
		while (true) {
			try {
				String input = reader.readLine();
				switch (input) {
				case "Server":
					Server server = new Server();
					return;
				case "Client":
					Client client = new Client();
					return;
				case "Router":
					Router router = new Router();
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
		BufferedReader reader = new BufferedReader(new InputStreamReader(
				System.in));
		boolean running = true;
		while (running) {
			try {
				String input = reader.readLine();
				if (input.equals("exit")) {
					running = false;
				} else {
					processCommand();
				}
			} catch (IOException e) {
				Logger.error("Failed to read input");
			}
		}
	}

	private static void processCommand() {

	}
}
