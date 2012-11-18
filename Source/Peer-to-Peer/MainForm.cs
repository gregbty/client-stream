using System;
using System.Windows.Forms;
using ClientStream.Endpoints;

namespace ClientStream
{
    public partial class MainForm : Form
    {
        private Endpoint _endpoint;

        public MainForm()
        {
            InitializeComponent();
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

        private void startBtn_Click(object sender, System.EventArgs e)
        {
            if (startBtn.Text.Equals("Start"))
            {
                startBtn.Text = "Stop";

                if (routerBox.Checked)
                    _endpoint = new Router();
                else if (clientBox.Checked)
                    _endpoint = new Client();
                _endpoint.Start();
            }
            else
            {
                startBtn.Text = "Start";
                _endpoint.Stop();
            }
        }
    }
}