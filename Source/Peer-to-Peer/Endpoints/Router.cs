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
    public class Router : Endpoint
    {
        private volatile List<IPEndPoint> _routers = new List<IPEndPoint>();
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

        public IEnumerable<IPEndPoint> Routers
        {
            get { return _routers; }
        }

        private string GetNextAvailableServer(IPEndPoint requester)
        {
            var random = new Random();
            if (!_routers.Any(t => Equals(t.Address, requester.Address)))
            {
                if (_routers.Count == 0)
                    return Message.NoServers;

                while (!_serverRequestServerWorker.CancellationPending)
                {
                    try
                    {
                        using (var serverRequestClient = new UdpClient())
                        {
                            var router = _routers.ElementAt(random.Next(0, _routers.Count - 1));
                            serverRequestClient.Client.ReceiveTimeout = 1000;
                            serverRequestClient.Connect(router);

                            Program.MainForm.WriteOutput("Requesting server from router@" + router);
                            var data = Encoding.ASCII.GetBytes(Message.GetServer);
                            data = Security.EncryptBytes(data);
                            serverRequestClient.Send(data, data.Length);

                            var client = new IPEndPoint(IPAddress.Any, 0);
                            data = serverRequestClient.Receive(ref client);
                            data = Security.DecryptBytes(data, data.Length);
                            return Encoding.ASCII.GetString(data);
                        }
                    }
                    catch
                    {

                    }
                }
            }

            var servers = _servers.Where(t => !Equals(t.Address, requester.Address)).ToList();

            if (servers.Count == 0)
                return Message.NoServers;

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
            data = Security.EncryptBytes(data);

            router.Port = Ports.Discovery;
            _discoveryServer.SendTo(data, data.Length, SocketFlags.None, router);
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
            _discoveryServerWorker.DoWork += discoveryServerWorker_DoWork;
            _discoveryServerWorker.RunWorkerAsync();

            _serverRequestServerWorkerReset.Reset();
            _serverRequestServerWorker.DoWork += serverRequestServerWorker_DoWork;
            _serverRequestServerWorker.RunWorkerAsync();
            Program.MainForm.WriteOutput("Router started");
        }

        public override void Stop()
        {
            _discoveryServerWorker.CancelAsync();
            _discoveryServerWorker.DoWork -= discoveryServerWorker_DoWork;
            _discoveryServerWorkerReset.WaitOne();
            Program.MainForm.WriteOutput("Discovery service stopped");

            _serverRequestServerWorker.CancelAsync();
            _serverRequestServerWorker.DoWork -= serverRequestServerWorker_DoWork;
            _serverRequestServerWorkerReset.WaitOne();
            Program.MainForm.WriteOutput("Server request service stopped");
        }

        private void discoveryServerWorker_DoWork(object sender, DoWorkEventArgs e)
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