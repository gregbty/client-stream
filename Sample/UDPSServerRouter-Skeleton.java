import java.io.*; 
import java.net.*; 
	  
public class UDPSServerRouter { 
    
    //Build the Routing Table - here, if you so wish //
    
	// Some 'stuff' to help you - if you need them //
    DatagramPacket receivePacket = null;   
    DatagramSocket serverSocket = null;
    DatagramSocket outgoingServerSocket = null;    
    byte[] receiveData = new byte[1024]; 
    byte[] sendData  = new byte[1024]; 

    /**
     * default constructor
     */
    public UDPSServerRouter() {
        try {
            serverSocket = new DatagramSocket(21212);
        
        }catch( SocketException e ) {
            System.out.println( e.getMessage() );
        }
        
    }
    
    
    /**
     * receives data from clients and prepare it for forwarding
     * 
     * @return void
     */
    public void getAndBuildData() throws Exception {
       
       while(true) {
            receivePacket = new DatagramPacket( receiveData, receiveData.length ); 
            serverSocket.receive( receivePacket ); 
	
		   
            String sentence = new String( receivePacket.getData() );
      
		// Disassemble the received packet into: the payload-data - packed, the IP - made InetAddress type //
            
       // Loop through the Routing Table to find an IP match
            for( int i = 0 ; i < ipArray.length; i++ ) {

                if( ipArray[i][0].equals(  ipAddress ) ) {
                    this.Router( i, sendData, IPAddress, ipArray[i][2] );
                }                
            }
	}
            
    }

    public static void main(String args[]) throws Exception { 
        UDPServer udpServer = new UDPSServerRouter();
        udpServer.getAndBuildData();
    }

    /**
     * determines which interface to bind to.
     * @param int link - the interface
     * @param byte message - the message to be sent
     * @param Object ipAddress - the ip address the message to be sent to.
     * @param String ports - the port number
     * 
     * @return void
     */
    public void Router( int link, byte[] message, InetAddress ipAddress,
        String ports ) throws Exception {
        
        int port = Integer.parseInt( ports );

        switch(link) {
            case 0:
                this.sendMessage( message, ipAddress, port );
                break;
                          
            case 1:
                this.sendMessage( message, ipAddress, port );
                break;
            
            case 2:
                this.sendMessage( message, ipAddress, port );
                break;
            
            case 3:
                this.sendMessage( message, ipAddress, port );
                break;
            
            case 4:
                this.sendMessage( message, ipAddress, port );
                break;
            
            case 5:
                this.sendMessage( message, ipAddress, port );
                break;
                
            case 6:
                this.sendMessage( message, ipAddress, port );
                break;
                
            case 7:
                this.sendMessage( message, ipAddress, port );
                break;
            
            
            default : 
                System.out.println("No interface to bind to");
                break;
	}
        
    }
    
    /**
     * sends the message to the destination IP
     * @param byte message - the data to be sent
     * @param Object ipAddress - the destination ip address
     * @param int port - the port number
     *
     * @return void
     */
    public void sendMessage( byte[] message,  InetAddress ipAddress, int port ) {
        
        try {
            outgoingServerSocket = new DatagramSocket(port);
        
        }catch( SocketException e ) {
            System.out.println( e.getMessage() );
        }
        
        try {
        	    
            DatagramPacket sendPacket = new DatagramPacket( message, message.length, 
                ipAddress, port );

            outgoingServerSocket.send(sendPacket);

        }catch( IOException e ) {
            System.out.println( e.getMessage() );
        }
    }
}
