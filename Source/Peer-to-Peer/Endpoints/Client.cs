using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using ClientStream.Constants;

namespace ClientStream.Endpoints
{
    internal class Client : Endpoint
    {
        private readonly TcpListener _streamServer = new TcpListener(IPAddress.Any, Ports.Streaming);
        private readonly TcpClient _streamClient = new TcpClient();
        private readonly BackgroundWorker _streamServerWorker = new BackgroundWorker();
        private readonly BackgroundWorker _streamClientWorker = new BackgroundWorker();

        public override void Start()
        {
            _streamServerWorker.DoWork +=_streamServerWorker_DoWork;
            _streamServerWorker.RunWorkerAsync();

            _streamClientWorker.DoWork +=_streamClientWorker_DoWork;
            _streamClientWorker.RunWorkerAsync();
        }

        public override void Stop()
        {
            _streamServerWorker.CancelAsync();
            _streamServerWorker.DoWork -= _streamServerWorker_DoWork;

            _streamClientWorker.CancelAsync();
            _streamClientWorker.DoWork -= _streamClientWorker_DoWork;
        }

        private void _streamServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void _streamClientWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}