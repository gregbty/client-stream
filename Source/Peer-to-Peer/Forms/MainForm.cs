using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using ClientStream.Constants;
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_endpoint == null)
                return;

            _endpoint.Stop();
        }

        public void WriteOutput(string output, bool debug = false)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => WriteOutput(output, debug)));
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
            if (!Directory.Exists(Directories.Files))
                Directory.CreateDirectory(Directories.Files);

            if (!Directory.Exists(Directories.Downloads))
                Directory.CreateDirectory(Directories.Downloads);

            if (startBtn.Text.Equals("Start"))
            {
                outputTxt.Clear();

                if (routerBox.Checked)
                {
                    _endpoint = new Router();

                    downloadProgressLbl.Visible = downloadProgress.Visible = false;
                    routerBtn.Enabled = routerBtn.Visible = true;
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

                    routerBtn.Enabled = routerBtn.Visible = false;
                    downloadProgressLbl.Visible = downloadProgress.Visible = true;
                }

                routerBox.Enabled = clientBox.Enabled = false;
                _endpoint.Start();
               
                startBtn.Text = "Stop";
            }
            else
            {
                routerBtn.Enabled = routerBtn.Visible = false;
                startBtn.Enabled = false;
                _endpoint.Stop();
                startBtn.Enabled = true;

                startBtn.Text = "Start";
                downloadProgressLbl.Visible = downloadProgress.Visible = false;
                routerBox.Enabled = clientBox.Enabled = true;
            }
        }

        private void routerBtn_Click(object sender, EventArgs e)
        {
            if (!(_endpoint is Router)) return;

            var router = (Router)_endpoint;
            using (var routerEdit = new RouterEdit(router.Routers))
            {
                routerEdit.ShowDialog(this);
            }
        }
    }
}