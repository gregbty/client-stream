using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClientStream.Constants;

namespace ClientStream.Endpoints
{
    public class Client : Endpoint
    {
        private readonly IPEndPoint _router;
        private readonly UdpClient _serverRequestClient = new UdpClient();
        private readonly TcpClient _streamClient = new TcpClient();
        private readonly TcpListener _streamServer = new TcpListener(IPAddress.Any, Ports.Streaming);
        private readonly BackgroundWorker _streamServerWorker = new BackgroundWorker();
        private readonly BackgroundWorker _streamClientWorker = new BackgroundWorker();

        public Client(IPEndPoint router)
        {
            _router = router;

            _streamServerWorker.WorkerSupportsCancellation = true;
            _streamClientWorker.WorkerSupportsCancellation = true;

            _streamServer.Server.ReceiveTimeout = 1000;
            _streamServer.Server.SendTimeout = 1000;
        }

        public override void Start()
        {
            _streamServerWorker.DoWork += _streamServerWorker_DoWork;
            _streamServerWorker.RunWorkerAsync();

            _streamClientWorker.DoWork += _streamClientWorker_DoWork;
            _streamClientWorker.RunWorkerAsync();
        }

        public override void Stop()
        {
            _streamServerWorker.CancelAsync();
            _streamServerWorker.DoWork -= _streamServerWorker_DoWork;

            _streamClientWorker.CancelAsync();
            _streamClientWorker.DoWork -= _streamClientWorker_DoWork;
        }

        private byte[] GetFileBytes()
        {
            var fileDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "files"));
            FileInfo[] files = fileDir.GetFiles();
            if (files.Length == 0)
                return Encoding.ASCII.GetBytes(Message.NoFiles);

            var random = new Random();
            return File.ReadAllBytes(files[random.Next(0, files.Length - 1)].FullName);
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
                        var data = new byte[1024];
                        int received;

                        NetworkStream inputStream = client.GetStream();
                        while ((received = inputStream.Read(data, 0, data.Length)) != 0)
                        {
                            string input = Encoding.ASCII.GetString(data, 0, received);
                            if (!input.Equals(Message.GetFile)) break;

                            data = GetFileBytes();
                            inputStream.Write(data, 0, data.Length);
                        }
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

        private void _streamClientWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_streamClientWorker.CancellationPending)
            {
                _serverRequestClient.Connect(_router);

                var data = Encoding.ASCII.GetBytes(Message.GetServer);
                _serverRequestClient.Send(data, data.Length);

                var client = new IPEndPoint(IPAddress.Any, 0);
                data = _serverRequestClient.Receive(ref client);

                string input = Encoding.ASCII.GetString(data);
                if (input.Equals(Message.NoServers))
                {
                    Program.MainForm.WriteOutput("No servers available");
                    Thread.Sleep(100);
                    continue;
                }

                var server = IPAddress.Parse(input);
                Program.MainForm.WriteOutput(string.Format("Connecting to server@{0} ", server));

                _streamClient.Connect(server, Ports.Streaming);

                data = Encoding.ASCII.GetBytes(Message.GetFile);
                var clientStream = _streamClient.GetStream();
                clientStream.Write(data, 0, data.Length);

                data = new byte[1024];
                int received;

                while ((received = clientStream.Read(data, 0, data.Length)) != 0)
                {
                    input = Encoding.ASCII.GetString(data, 0, received);
                    if (!input.Equals(Message.NoFiles))
                    {
                        Program.MainForm.WriteOutput("No files to download", true);
                        Thread.Sleep(100);
                        break;
                    }

                    using (var fileStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "downloads"),
                                                           FileMode.Create))
                    {
                        data = Encoding.ASCII.GetBytes(input);
                        fileStream.Write(data, 0, data.Length);
                        break;
                    }
                }

                _streamClient.Client.Disconnect(true);
                Thread.Sleep(300);
            }
        }
    }
}