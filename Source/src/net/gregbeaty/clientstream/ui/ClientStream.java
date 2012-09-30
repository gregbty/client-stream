package net.gregbeaty.clientstream.ui;

import java.awt.EventQueue;

import javax.swing.JFrame;
import javax.swing.JLabel;
import java.awt.BorderLayout;
import javax.swing.JButton;

public class ClientStream {

	private JFrame frame;

	/**
	 * Launch the application.
	 */
	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				try {
					ClientStream window = new ClientStream();
					window.frame.setVisible(true);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});
	}

	/**
	 * Create the application.
	 */
	public ClientStream() {
		initialize();
	}

	/**
	 * Initialize the contents of the frame.
	 */
	private void initialize() {
		frame = new JFrame();
		frame.setBounds(100, 100, 450, 300);
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		
		JLabel lblSelectTheType = new JLabel("Select the type of machine you want to create:\r\n");
		frame.getContentPane().add(lblSelectTheType, BorderLayout.NORTH);
		
		JButton btnClient = new JButton("Client");
		frame.getContentPane().add(btnClient, BorderLayout.WEST);
		
		JButton btnServer = new JButton("Server");
		frame.getContentPane().add(btnServer, BorderLayout.CENTER);
		
		JButton btnRouter = new JButton("Router");
		frame.getContentPane().add(btnRouter, BorderLayout.EAST);
	}

}
