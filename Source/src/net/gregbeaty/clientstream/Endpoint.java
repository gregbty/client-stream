package net.gregbeaty.clientstream;

public interface Endpoint {
	public void stop();

	public String getAddress();

	public int getPort();

	public void printFileList();
}
