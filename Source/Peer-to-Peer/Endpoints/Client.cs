using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClientStream.Constants;

namespace ClientStream.Endpoints
{
    internal class Client : Endpoint
    {
        private readonly TcpClient _streamClient = new TcpClient();
        private readonly TcpListener _streamServer = new TcpListener(IPAddress.Any, Ports.Streaming);
        private readonly BackgroundWorker _streamServerWorker = new BackgroundWorker();

        public Client()
        {
            _streamServerWorker.WorkerSupportsCancellation = true;

            _streamServer.Server.ReceiveTimeout = 1000;
            _streamServer.Server.SendTimeout = 1000;
        }

        public override void Start()
        {
            _streamServerWorker.DoWork += _streamServerWorker_DoWork;
            _streamServerWorker.RunWorkerAsync();
        }

        public override void Stop()
        {
            _streamServerWorker.CancelAsync();
            _streamServerWorker.DoWork -= _streamServerWorker_DoWork;
        }

        private void _streamServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _streamServer.Start();

            var random = new Random();
            while (!_streamServerWorker.CancellationPending)
            {
                try
                {
                    using (TcpClient client = _streamServer.AcceptTcpClient())
                    {
                    }
                }
                catch (SocketException)
                {
                    Program.MainForm.WriteOutput("Receive timeout", true);

                    int ms = random.Next(10, 150);
                    Thread.Sleep(ms);
                }
            }

            _streamServer.Stop();
        }
    }
}