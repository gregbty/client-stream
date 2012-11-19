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
    public class Router : Endpoint
    {
        private readonly BackgroundWorker _discoveryServerWorker = new BackgroundWorker();
        private readonly ManualResetEvent _discoveryServerWorkerReset = new ManualResetEvent(false);
        private readonly List<IPAddress> _routers = new List<IPAddress>();

        private readonly Socket _serverRequestServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                                                                  ProtocolType.Udp);

        private readonly BackgroundWorker _serverRequestServerWorker = new BackgroundWorker();

        private readonly ManualResetEvent _serverRequestServerWorkerReset = new ManualResetEvent(false);
        private readonly List<IPAddress> _servers = new List<IPAddress>();

        public Router()
        {
            _discoveryServerWorker.WorkerSupportsCancellation = true;
            _serverRequestServerWorker.WorkerSupportsCancellation = true;

            _serverRequestServer.ReceiveTimeout = 3000;
        }

        public IEnumerable<IPAddress> Routers
        {
            get { return _routers; }
        }

        private string GetNextAvailableServer(IPAddress requester)
        {
            var random = new Random();
            if (!_routers.Contains(requester))
            {
                if (_routers.Count == 0)
                    return Message.NoRouters;

                try
                {
                    using (var serverRequestClient = new UdpClient())
                    {
                        IPAddress router = _routers.ElementAt(random.Next(0, _routers.Count - 1));
                        serverRequestClient.Client.ReceiveTimeout = 5000;

                        byte[] data = Encoding.ASCII.GetBytes(Message.GetServer);
                        data = Security.EncryptBytes(data);
                        serverRequestClient.Send(data, data.Length, router.ToString(), Ports.ServerRequest);
                        Program.MainForm.WriteOutput(string.Format("Requesting server from router@{0}", router));

                        var client = new IPEndPoint(IPAddress.Any, 0);
                        data = serverRequestClient.Receive(ref client);
                        data = Security.DecryptBytes(data, data.Length);
                        return Encoding.ASCII.GetString(data);
                    }
                }
                catch
                {
                    return Message.NoServers;
                }
            }

            if (_servers.Count == 0)
                return Message.NoServers;

            IPAddress server = _servers.ElementAt(random.Next(0, _servers.Count - 1));

            Program.MainForm.WriteOutput(string.Format("Server@{0} chosen", server));
            return server.ToString();
        }

        private bool CheckIfRouterExists(IPAddress router)
        {
            return _routers.Contains(router);
        }

        private bool CheckIfServerExists(IPAddress server)
        {
            return _servers.Contains(server);
        }

        private void AddRouter(IPAddress router)
        {
            if (!CheckIfRouterExists(router))
            {
                _routers.Add(router);
                Program.MainForm.WriteOutput(string.Format("Router@{0} added", router));
            }

            byte[] data = Encoding.ASCII.GetBytes(Message.AddRouter);
            data = Security.EncryptBytes(data);

            using (var client = new UdpClient())
            {
                client.Connect(router, Ports.Discovery);
                client.Send(data, data.Length);
            }
        }

        private void AddServer(IPAddress server, int port)
        {
            if (!CheckIfServerExists(server))
            {
                _servers.Add(server);
                Program.MainForm.WriteOutput(string.Format("Server@{0} added", server));
            }

            byte[] data = Encoding.ASCII.GetBytes(Message.AddServer);
            data = Security.EncryptBytes(data);

            using (var client = new UdpClient())
            {
                client.Connect(server, port);
                client.Send(data, data.Length);
            }
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
                var discoveryServer = new UdpClient(new IPEndPoint(IPAddress.Any, Ports.Discovery))
                                          {
                                              Client =
                                                  {
                                                      ReceiveTimeout = 1000
                                                  }
                                          };

                while (!_discoveryServerWorker.CancellationPending)
                {
                    var client = new IPEndPoint(IPAddress.Any, 0);

                    byte[] data;
                    try
                    {
                        data = discoveryServer.Receive(ref client);
                    }
                    catch (Exception)
                    {
                        int ms = random.Next(10, 150);
                        Thread.Sleep(ms);
                        continue;
                    }

                    data = Security.DecryptBytes(data, data.Length);
                    string input = Encoding.ASCII.GetString(data, 0, data.Length);

                    if (input.Equals(Message.AddRouter))
                        AddRouter(client.Address);
                    else if (input.Equals(Message.AddServer))
                        AddServer(client.Address, client.Port);
                }

                discoveryServer.Close();
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

                while (!_serverRequestServerWorker.CancellationPending)
                {
                    int received;
                    var client = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

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

                    data = Encoding.ASCII.GetBytes(GetNextAvailableServer(((IPEndPoint) client).Address));
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