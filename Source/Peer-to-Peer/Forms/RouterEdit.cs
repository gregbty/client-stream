using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ClientStream.Constants;
using Message = ClientStream.Constants.Message;

namespace ClientStream.Forms
{
    public partial class RouterEdit : Form
    {
        public RouterEdit(IEnumerable<IPAddress> routers)
        {
            InitializeComponent();
            foreach (var router in routers)
            {
                routersBox.Items.Add(router);
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
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

                                                   byte[] data = Encoding.ASCII.GetBytes(Message.AddRouter);
                                                   data = Security.EncryptBytes(data);

                                                   try
                                                   {
                                                       client.Send(data, data.Length,
                                                                   new IPEndPoint(address, Ports.Discovery));
                                                   }
                                                   catch (SocketException)
                                                   {
                                                       Invoke(new MethodInvoker(() => MessageBox.Show(this, "Failed to connect. Try another IP address")));
                                                       return;
                                                   }

                                                   //TODO: Wait for response back

                                                   bool bound =
                                                       routersBox.Items.Cast<string>()
                                                                 .Any(item => item == address.ToString());

                                                   if (!bound)
                                                       BeginInvoke(
                                                           new MethodInvoker(
                                                               () => routersBox.Items.Add(address.ToString())));

                                                   client.Close();

                                               }
                                               finally
                                               {
                                                   doneEvent.Set();
                                               }
                                           };

            backgroundWorker.RunWorkerAsync();
            Cursor.Current = Cursors.WaitCursor;
            doneEvent.WaitOne();
        }

        private void routerInput_TextChanged(object sender, EventArgs e)
        {
            addBtn.Enabled = !string.IsNullOrEmpty(routerTxt.Text) &&
                             !routerTxt.Text.Equals(Program.MainForm.GetIPAddress());
        }
    }
}