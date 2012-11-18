using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using ClientStream.Endpoints;

namespace ClientStream.Forms
{
    public partial class MainForm : Form
    {
        private Endpoint _endpoint;

        public MainForm()
        {
            InitializeComponent();

            addressLbl.Text = string.Format("IP Address: {0}", GetIPAddress());
            downloadProgressLbl.Visible = downloadProgress.Visible = false;
        }

        public void WriteOutput(string output, bool debug = false)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => WriteOutput(output, debug)));
                return;
            }

#if !DEBUG
            if (debug) return;
#endif

            if (debug)
            {
                outputTxt.AppendText("Debug: ");
            }
            
            outputTxt.AppendText(output);
            outputTxt.AppendText(Environment.NewLine);
        }

        private string GetIPAddress()
        {
            string localIP = "127.0.0.1";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            return localIP;
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "files")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files"));

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "downloads")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "downloads"));

            if (startBtn.Text.Equals("Start"))
            {
                outputTxt.Clear();

                if (routerBox.Checked)
                {
                    _endpoint = new Router();

                    downloadProgressLbl.Visible = downloadProgress.Visible = false;
                }
                else if (clientBox.Checked)
                {
                    using (var routerSelect = new RouterSelect())
                    {
                        routerSelect.ShowDialog(this);

                        if (routerSelect.Router == null)
                            return;

                        _endpoint = new Client(routerSelect.Router);
                    }

                    downloadProgressLbl.Visible = downloadProgress.Visible = true;
                }

                routerBox.Enabled = clientBox.Enabled = false;
                _endpoint.Start();
               
                startBtn.Text = "Stop";
            }
            else
            {
                
                _endpoint.Stop();

                startBtn.Text = "Start";
                downloadProgressLbl.Visible = downloadProgress.Visible = false;
                routerBox.Enabled = clientBox.Enabled = true;
            }
        }
    }
}