using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientStream.Endpoints
{
    internal class Router : Endpoint
    {
        private readonly List<IPEndPoint> _routers = new List<IPEndPoint>();
        private readonly List<IPEndPoint> _servers = new List<IPEndPoint>();
        private readonly Socket _routerDiscoveryServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly Socket _serverReqestServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly BackgroundWorker _routerDiscoveryWorker = new BackgroundWorker();
        private readonly BackgroundWorker _serverRequestWorker = new BackgroundWorker();

        public Router()
        {
            _routerDiscoveryWorker.WorkerSupportsCancellation = true;
            _serverRequestWorker.WorkerSupportsCancellation = true;

            _routerDiscoveryServer.ReceiveTimeout = 100;
            _serverReqestServer.ReceiveTimeout = 100;
        }

        public bool CheckIfRouterExists(IPEndPoint router)
        {
            return _routers.Any(r => Equals(r.Address, router.Address));
        }

        public bool CheckIfServerExists(IPEndPoint server)
        {
            return _servers.Any(s => Equals(s.Address, server.Address));
        }

        public void AddRouter(IPEndPoint router)
        {
            if (CheckIfRouterExists(router))
                return;

            _routers.Add(router);
        }

        public void AddServer(IPEndPoint server)
        {
            if (CheckIfServerExists(server))
                return;

            _servers.Add(server);
        }

        public override void Start()
        {
            _routerDiscoveryWorker.DoWork += serverRequestWorker_DoWork;
            _routerDiscoveryWorker.RunWorkerAsync();

            _serverRequestWorker.DoWork += _routerDiscoveryWorker_DoWork;
            _serverRequestWorker.RunWorkerAsync();
        }

        public override void Stop()
        {
            _routerDiscoveryWorker.CancelAsync();
            _routerDiscoveryWorker.DoWork -= _routerDiscoveryWorker_DoWork;

            _serverRequestWorker.CancelAsync();
            _serverRequestWorker.DoWork -= serverRequestWorker_DoWork;
        }

        private void _routerDiscoveryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
        }

        private void serverRequestWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var data = new byte[1024];
            var localEndpoint = new IPEndPoint(IPAddress.Any, Ports.Router);

            _serverReqestServer.Bind(localEndpoint);
            Program.MainForm.WriteOutput("Waiting for client...");

            var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            var client = (EndPoint) remoteEndpoint;

            while (!_serverRequestWorker.CancellationPending)
            {
                int received;

                try
                {
                    received = _serverReqestServer.ReceiveFrom(data, ref client);
                }
                catch (SocketException)
                {
                    Thread.Sleep(100);
                    Program.MainForm.WriteOutput("Receive timeout", true);
                    continue;
                }

                string input = Encoding.ASCII.GetString(data, 0, received);
                Program.MainForm.WriteOutput(String.Format("Request received from: {0}, data: {1}", client, input));

                if (!input.Equals(Commands.GetServer)) continue;

                var random = new Random();
                int index = random.Next(0, 1);

                string output = _servers.ElementAt(index).Address.ToString(); 
                data = Encoding.ASCII.GetBytes(output);

                _serverReqestServer.SendTo(data, data.Length, SocketFlags.None, client);
            }

            _serverReqestServer.Close();
            Program.MainForm.WriteOutput("Socket closed", true);
        }
    }
}