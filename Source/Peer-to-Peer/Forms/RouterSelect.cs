using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ClientStream.Constants;

namespace ClientStream.Forms
{
    public partial class RouterSelect : Form
    {
        public IPEndPoint Router { get; private set; }

        public RouterSelect()
        {
            InitializeComponent();
        }

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
                        var client = new UdpClient();
                        client.Connect(address.ToString(), Ports.Discovery);

                        var data = Encoding.ASCII.GetBytes(Constants.Message.AddServer);
                        data = Security.EncryptBytes(data);
                        client.Send(data, data.Length);

                        Router = new IPEndPoint(address, Ports.ServerRequest);
                        connected = true;
                    }
                    catch (SocketException)
                    {
                        MessageBox.Show(this, "Failed to connect. Try another IP address");
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