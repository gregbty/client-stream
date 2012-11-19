using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private readonly ManualResetEvent _discoveryServerWorkerReset = new ManualResetEvent(false);
        private readonly ManualResetEvent _serverRequestServerWorkerReset = new ManualResetEvent(false);

        public Router()
        {
            _discoveryServerWorker.WorkerSupportsCancellation = true;
            _serverRequestServerWorker.WorkerSupportsCancellation = true;

            _discoveryServer.ReceiveTimeout = 1000;
            _serverRequestServer.ReceiveTimeout = 1000;
        }

        private string GetNextAvailableServer(IPEndPoint client)
        {
            var random = new Random();
            var servers = _servers.Where(t => !Equals(t.Address, client.Address)).ToList();

            if (servers.Count == 0)
            {
                Program.MainForm.WriteOutput("No servers available");
                return Message.NoServers;
            }

            var server = servers.ElementAt(random.Next(0, _servers.Count - 1));

            Program.MainForm.WriteOutput(string.Format("Server@{0} chosen", server.Address));
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
            Program.MainForm.WriteOutput(string.Format("Router@{0} added", router.Address));

            var data = Encoding.ASCII.GetBytes(Message.AddRouter);
            _serverRequestServer.SendTo(data, data.Length, SocketFlags.None, router);
        }

        private void AddServer(IPEndPoint server)
        {
            if (CheckIfServerExists(server))
                return;
            
            _servers.Add(server);
            Program.MainForm.WriteOutput(string.Format("Server@{0} added", server.Address));
        }

        public override void Start()
        {
            _discoveryServerWorkerReset.Reset();
            _discoveryServerWorker.DoWork += _discoveryServerWorker_DoWork;
            _discoveryServerWorker.RunWorkerAsync();

            _serverRequestServerWorkerReset.Reset();
            _serverRequestServerWorker.DoWork += serverRequestServerWorker_DoWork;
            _serverRequestServerWorker.RunWorkerAsync();
            Program.MainForm.WriteOutput("Router started");
        }

        public override void Stop()
        {
            _discoveryServerWorker.CancelAsync();
            _discoveryServerWorker.DoWork -= _discoveryServerWorker_DoWork;
            _discoveryServerWorkerReset.WaitOne();
            Program.MainForm.WriteOutput("Discovery service stopped");

            _serverRequestServerWorker.CancelAsync();
            _serverRequestServerWorker.DoWork -= serverRequestServerWorker_DoWork;
            _serverRequestServerWorkerReset.WaitOne();
            Program.MainForm.WriteOutput("Server request service stopped");
        }

        private void _discoveryServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var random = new Random();
                var data = new byte[1024];
                var localEndpoint = new IPEndPoint(IPAddress.Any, Ports.Discovery);

                _discoveryServer.Bind(localEndpoint);

                var client = (EndPoint) new IPEndPoint(IPAddress.Any, 0);

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

                    data = Security.DecryptBytes(data, received);
                    string input = Encoding.ASCII.GetString(data, 0, data.Length);

                    switch (input)
                    {
                        case Message.AddRouter:
                            AddRouter((IPEndPoint) client);
                            break;
                        case Message.AddServer:
                            AddServer((IPEndPoint) client);
                            break;
                    }
                }

                _discoveryServer.Close();
            }
            finally
            {
                _discoveryServerWorkerReset.Set();
            }
        }

        private void serverRequestServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var random = new Random();
                var data = new byte[1024];
                var localEndpoint = new IPEndPoint(IPAddress.Any, Ports.ServerRequest);

                _serverRequestServer.Bind(localEndpoint);

                var client = (EndPoint) new IPEndPoint(IPAddress.Any, 0);

                while (!_serverRequestServerWorker.CancellationPending)
                {
                    int received;

                    try
                    {
                        received = _serverRequestServer.ReceiveFrom(data, ref client);
                    }
                    catch (SocketException)
                    {
                        int ms = random.Next(10, 150);
                        Thread.Sleep(ms);
                        continue;
                    }

                    data = Security.DecryptBytes(data, received);
                    string input = Encoding.ASCII.GetString(data, 0, data.Length);
                    Program.MainForm.WriteOutput(String.Format("Request received from: {0}, data: {1}", client, input));

                    if (!input.Equals(Message.GetServer)) continue;

                    //TODO: Get from other router
                    data = Encoding.ASCII.GetBytes(GetNextAvailableServer((IPEndPoint) client));
                    data = Security.EncryptBytes(data);

                    _serverRequestServer.SendTo(data, data.Length, SocketFlags.None, client);
                }

                _serverRequestServer.Close();
            }
            finally
            {
                _serverRequestServerWorkerReset.Set();
            }
        }
    }
}