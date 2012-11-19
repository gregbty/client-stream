using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ClientStream.Constants;
using Message = ClientStream.Constants.Message;

namespace ClientStream.Endpoints
{
    public class Client : Endpoint
    {
        private readonly IPEndPoint _router;
        private readonly TcpListener _streamServer = new TcpListener(IPAddress.Any, Ports.Streaming);
        private readonly BackgroundWorker _streamServerWorker = new BackgroundWorker();
        private readonly BackgroundWorker _streamClientWorker = new BackgroundWorker();
        private readonly ManualResetEvent _streamServerWorkerReset = new ManualResetEvent(false);
        private readonly ManualResetEvent _streamClientWorkerReset = new ManualResetEvent(false);

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
            _streamServerWorkerReset.Reset();
            _streamServerWorker.DoWork += _streamServerWorker_DoWork;
            _streamServerWorker.RunWorkerAsync();

            _streamClientWorkerReset.Reset();
            _streamClientWorker.DoWork += _streamClientWorker_DoWork;
            _streamClientWorker.RunWorkerAsync();
            Program.MainForm.WriteOutput("Client/server started");
        }

        public override void Stop()
        {
            _streamServerWorker.CancelAsync();
            _streamServer.Stop();
            _streamServerWorker.DoWork -= _streamServerWorker_DoWork;
            _streamServerWorkerReset.WaitOne();
            Program.MainForm.WriteOutput("Streaming service stopped");

            _streamClientWorker.CancelAsync();
            _streamClientWorker.DoWork -= _streamClientWorker_DoWork;
            _streamClientWorkerReset.WaitOne();
            Program.MainForm.WriteOutput("Downloading service stopped");
        }

        private FileInfo GetFile()
        {
            var fileDir = new DirectoryInfo(Directories.Files);
            FileInfo[] files = fileDir.GetFiles();
            if (files.Length == 0)
                return null;

            var random = new Random();
            return files[random.Next(0, files.Length - 1)];
        }

        private byte[] GetFileBytes(FileInfo file)
        {
            return File.ReadAllBytes(file.FullName);
        }

        private void _streamServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
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
                                data = Security.DecryptBytes(data, received);
                                string input = Encoding.ASCII.GetString(data, 0, data.Length);
                                if (!input.Equals(Message.GetFile)) break;

                                FileInfo file = GetFile();
                                data = Encoding.ASCII.GetBytes(String.Concat(Message.File, string.Format("{0}|{1}", file.Name, file.Length)));
                                data = Security.EncryptBytes(data);
                                inputStream.Write(data, 0, data.Length);

                                data = GetFileBytes(file);
                                data = Security.EncryptBytes(data);
                                inputStream.Write(data, 0, data.Length);
                            }
                        }
                    }
                    catch
                    {
                        int ms = random.Next(10, 150);
                        Thread.Sleep(ms);
                    }
                }
            }
            finally
            {
                _streamServerWorkerReset.Set();
            }
        }

        private void _streamClientWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var serverRequestClient = new UdpClient())
                {
                    serverRequestClient.Client.ReceiveTimeout = 1000;

                    while (!_streamClientWorker.CancellationPending)
                    {
                        try
                        {
                            serverRequestClient.Connect(_router);

                            var data = Encoding.ASCII.GetBytes(Message.GetServer);
                            data = Security.EncryptBytes(data);
                            serverRequestClient.Send(data, data.Length);

                            Program.MainForm.WriteOutput("Waiting for next available server");
                            var client = new IPEndPoint(IPAddress.Any, 0);
                            data = serverRequestClient.Receive(ref client);
                            data = Security.DecryptBytes(data, data.Length);

                            string input = Encoding.ASCII.GetString(data);
                            if (input.Equals(Message.NoServers))
                            {
                                Program.MainForm.WriteOutput("No servers available");
                                Thread.Sleep(2000);
                                continue;
                            }

                            var server = IPAddress.Parse(input);
                            Program.MainForm.WriteOutput(string.Format("Connecting to server@{0} ", server));

                            using (var streamClient = new TcpClient())
                            {
                                streamClient.Connect(server, Ports.Streaming);

                                data = Encoding.ASCII.GetBytes(Message.GetFile);
                                data = Security.EncryptBytes(data);

                                var clientStream = streamClient.GetStream();
                                clientStream.Write(data, 0, data.Length);

                                data = new byte[1024];
                                int received;

                                while ((received = clientStream.Read(data, 0, data.Length)) != 0)
                                {
                                    data = Security.DecryptBytes(data, received);
                                    input = Encoding.ASCII.GetString(data, 0, data.Length);
                                    if (input.Equals(Message.NoFiles))
                                    {
                                        Program.MainForm.WriteOutput("No files to download", true);
                                        Thread.Sleep(100);
                                        break;
                                    }
                                    if (!input.Contains(Message.File)) break;

                                    var fileinfo = input.Replace(Message.File, String.Empty).Split('|');
                                    string filename = fileinfo[0];
                                    string filesize = fileinfo[1];

                                    Program.MainForm.WriteOutput(string.Format("Incoming file size: {0} bytes", filesize));
                                    using (
                                        var fileStream =
                                            new FileStream(Path.Combine(Directories.Downloads, filename),
                                                           FileMode.Create))
                                    {

                                        int total = 0;
                                        var bytes = new List<byte>();
                                        while ((received = clientStream.Read(data, 0, data.Length)) != 0)
                                        {
                                            total += received;
                                            bytes.AddRange(data);
                                        }

                                        data = bytes.ToArray();
                                        data = Security.DecryptBytes(data, total);
                                        fileStream.Write(data, 0, data.Length);
                                    }
                                    using (var videoPlayer = Process.Start(Path.Combine(Directories.Downloads, filename)))
                                    {
                                        if (videoPlayer == null) return;
                                        videoPlayer.WaitForExit();
                                    }

                                    break;
                                }

                                Thread.Sleep(300);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            finally
            {
                _streamClientWorkerReset.Set();
            }
        }
    }
}