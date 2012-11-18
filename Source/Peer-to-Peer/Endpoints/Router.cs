using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClientStream.Constants;

namespace ClientStream.Endpoints
{
    internal class Router : Endpoint
    {
        private readonly List<IPEndPoint> _routers = new List<IPEndPoint>();
        private readonly List<IPEndPoint> _servers = new List<IPEndPoint>();
        private readonly Socket _discoveryServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly Socket _serverRequestServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly BackgroundWorker _discoveryServerWorker = new BackgroundWorker();
        private readonly BackgroundWorker _serverRequestServerWorker = new BackgroundWorker();

        public Router()
        {
            _discoveryServerWorker.WorkerSupportsCancellation = true;
            _serverRequestServerWorker.WorkerSupportsCancellation = true;

            _discoveryServer.ReceiveTimeout = 100;
            _serverRequestServer.ReceiveTimeout = 100;
        }

        public string GetNextAvailableServer(IPEndPoint client)
        {
            var random = new Random();
            var server = _servers.Where(t => !Equals(t.Address, client.Address)).ElementAt(random.Next(0, _servers.Count - 1));
            return server.Address.ToString();
        }

        private bool CheckIfRouterExists(IPEndPoint router)
        {
            return _routers.Any(r => Equals(r.Address, router.Address));
        }

        private bool CheckIfServerExists(IPEndPoint server)
        {
            return _servers.Any(s => Equals(s.Address, server.Address));
        }

        private void AddRouter(IPEndPoint router)
        {
            if (CheckIfRouterExists(router))
                return;

            _routers.Add(router);

            var data = Encoding.ASCII.GetBytes(Commands.AddRouter);
            _serverRequestServer.SendTo(data, data.Length, SocketFlags.None, router);
        }

        private void AddServer(IPEndPoint server)
        {
            if (CheckIfServerExists(server))
                return;

            _servers.Add(server);
        }

        public override void Start()
        {
            _discoveryServerWorker.DoWork += _discoveryServerWorker_DoWork;
            _discoveryServerWorker.RunWorkerAsync();

            _serverRequestServerWorker.DoWork += serverRequestServerWorker_DoWork;
            _serverRequestServerWorker.RunWorkerAsync();
        }

        public override void Stop()
        {
            _discoveryServerWorker.CancelAsync();
            _discoveryServerWorker.DoWork -= _discoveryServerWorker_DoWork;

            _serverRequestServerWorker.CancelAsync();
            _serverRequestServerWorker.DoWork -= serverRequestServerWorker_DoWork;
        }

        private void _discoveryServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var random = new Random();
            var data = new byte[1024];
            var localEndpoint = new IPEndPoint(IPAddress.Any, Ports.Discovery);

            _discoveryServer.Bind(localEndpoint);

            var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            var client = (EndPoint) remoteEndpoint;

            while (!_discoveryServerWorker.CancellationPending)
            {
                int received;

                try
                {
                    received = _discoveryServer.ReceiveFrom(data, ref client);
                }
                catch (Exception)
                {
                    int ms = random.Next(10, 150);
                    Thread.Sleep(ms);
                    continue;
                }

                string input = Encoding.ASCII.GetString(data, 0, received);

                switch (input)
                {
                    case Commands.AddRouter: 
                        AddRouter(remoteEndpoint);
                        break;
                    case Commands.AddServer:
                        AddServer(remoteEndpoint);
                        break;
                }
            }

            _discoveryServer.Close();
            Program.MainForm.WriteOutput("Discovery socket closed", true);
        }

        private void serverRequestServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var random = new Random();
            var data = new byte[1024];
            var localEndpoint = new IPEndPoint(IPAddress.Any, Ports.ServerRequest);

            _serverRequestServer.Bind(localEndpoint);
            Program.MainForm.WriteOutput("Waiting for client...");

            var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            var client = (EndPoint) remoteEndpoint;

            while (!_serverRequestServerWorker.CancellationPending)
            {
                int received;

                try
                {
                    received = _serverRequestServer.ReceiveFrom(data, ref client);
                }
                catch (SocketException)
                {
                    Program.MainForm.WriteOutput("Receive timeout", true);

                    int ms = random.Next(10, 150);
                    Thread.Sleep(ms);
                    continue;
                }

                string input = Encoding.ASCII.GetString(data, 0, received);
                Program.MainForm.WriteOutput(String.Format("Request received from: {0}, data: {1}", client, input));

                if (!input.Equals(Commands.GetServer)) continue;

                string output = GetNextAvailableServer(remoteEndpoint); 
                data = Encoding.ASCII.GetBytes(output);

                _serverRequestServer.SendTo(data, data.Length, SocketFlags.None, client);
            }

            _serverRequestServer.Close();
            Program.MainForm.WriteOutput("Server request socket closed", true);
        }
    }
}