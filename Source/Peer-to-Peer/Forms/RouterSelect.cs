using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ClientStream.Constants;
using Message = ClientStream.Constants.Message;

namespace ClientStream.Forms
{
    public partial class RouterSelect : Form
    {
        public RouterSelect()
        {
            InitializeComponent();
        }

        public IPEndPoint Router { get; private set; }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            IPAddress address;

            try
            {
                address = IPAddress.Parse(routerTxt.Text);
            }
            catch
            {
                MessageBox.Show(this, "Please enter a valid IP address.");
                return;
            }

            bool connected = false;
            var doneEvent = new ManualResetEvent(false);
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (o, args) =>
                                           {
                                               try
                                               {
                                                   var client = new UdpClient
                                                                    {
                                                                        Client =
                                                                            {
                                                                                ReceiveTimeout = 1000,
                                                                                SendTimeout = 1000
                                                                            }
                                                                    };

                                                   byte[] data = Encoding.ASCII.GetBytes(Message.AddServer);
                                                   data = Security.EncryptBytes(data);

                                                   try
                                                   {
                                                       client.Send(data, data.Length,
                                                                   new IPEndPoint(address, Ports.Discovery));

                                                       var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                                                       data = client.Receive(ref remoteEndpoint);
                                                       data = Security.DecryptBytes(data, data.Length);

                                                       if (
                                                           Encoding.ASCII.GetString(data, 0, data.Length)
                                                                   .Equals(Message.AddServer))
                                                       {
                                                           Router = new IPEndPoint(address, Ports.ServerRequest);
                                                           connected = true;
                                                           return;
                                                       }

                                                       throw new SocketException();
                                                   }
                                                   catch (SocketException)
                                                   {
                                                       Invoke(new MethodInvoker(()=>MessageBox.Show(this, "Failed to connect. Try another IP address")));
                                                   }
                                               }
                                               finally
                                               {
                                                   doneEvent.Set();
                                               }
                                           };

            backgroundWorker.RunWorkerAsync();
            Cursor.Current = Cursors.WaitCursor;
            doneEvent.WaitOne();

            if (!connected)
                return;

            Close();
        }
    }
}