using System;
using System.IO;
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

            downloadProgressLbl.Visible = downloadProgress.Visible = false;
        }

        public void WriteOutput(string output, bool debug = false)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => WriteOutput(output, debug)));
                return;
            }

#if DEBUG
            if (debug)
            {
                outputTxt.AppendText("Debug: ");
            }
#endif
            outputTxt.AppendText(output);
            outputTxt.AppendText(Environment.NewLine);
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "output")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "output"));

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "downloads")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "downloads"));

            if (startBtn.Text.Equals("Start"))
            {
                outputTxt.Clear();
                startBtn.Text = "Stop";

                if (routerBox.Checked)
                {
                    downloadProgressLbl.Visible = downloadProgress.Visible = false;
                    _endpoint = new Router();
                }
                else if (clientBox.Checked)
                {
                    downloadProgressLbl.Visible = downloadProgress.Visible = true;
                    _endpoint = new Client();
                }

                routerBox.Enabled = clientBox.Enabled = false;
                _endpoint.Start();
            }
            else
            {
                startBtn.Text = "Start";
                _endpoint.Stop();

                downloadProgressLbl.Visible = downloadProgress.Visible = false;
                routerBox.Enabled = clientBox.Enabled = true;
            }
        }
    }
}