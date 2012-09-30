import java.io.*; 
import java.net.*; 
  
class UDPClient { 
    public static void main(String args[]) throws Exception 
    { 
  
      // Read message/data from keyboard or file //
		
  
      DatagramSocket clientSocket = new DatagramSocket(); 
  
      InetAddress IPAddress = InetAddress.getByName("localhost"); 
  
	  // Prepare data structure for payloads - sending and receiving //
      byte[] sendData = new byte[1024]; 
      byte[] receiveData = new byte[1024]; 
  
      // Construct the message itself - and 'pack' it //
      // Echo message to ensure it is correct //
      

      // Prepare a packet of information  and send it via the 'clientSocket' //
  
     // Prepare a packet of 'empty' payload and receive message into it via the 'clientSocket' //


	 // 'Unpack' the message into character string and print it out to verify for correctness and close the socket //

  
      String modifiedSentence = 
          new String(receivePacket.getData()); 
  
      System.out.println("FROM SERVER:" + modifiedSentence); 
      clientSocket.close(); 
      } 
} 
      

