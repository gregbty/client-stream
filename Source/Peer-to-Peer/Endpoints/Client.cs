using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
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
        private readonly BackgroundWorker _streamClientWorker = new BackgroundWorker();
        private readonly ManualResetEvent _streamClientWorkerReset = new ManualResetEvent(false);
        private readonly BackgroundWorker _streamServerWorker = new BackgroundWorker();
        private readonly ManualResetEvent _streamServerWorkerReset = new ManualResetEvent(false);
        private readonly TcpListener _streamServer = new TcpListener(IPAddress.Any, Ports.Streaming);

        public Client(IPEndPoint router)
        {
            _router = router;

            _streamServerWorker.WorkerSupportsCancellation = true;
            _streamClientWorker.WorkerSupportsCancellation = true;
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
            try
            {
                _streamServer.Stop();
            }
            catch
            {
            }
            
            _streamServerWorker.CancelAsync();
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

            return files.Length == 0 ? null : files[new Random().Next(0, files.Length - 1)];
        }

        private byte[] GetFileBytes(FileInfo file)
        {
            return File.ReadAllBytes(file.FullName);
        }

        private void _streamServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var random = new Random();
                _streamServer.Server.ReceiveTimeout = 300;
                _streamServer.Server.SendTimeout = 300;
                _streamServer.Start();

                while (!_streamServerWorker.CancellationPending)
                {
                    try
                    {
                        using (TcpClient incomingClient = _streamServer.AcceptTcpClient())
                        {
                            var data = new byte[1024];
                            int received;

                            NetworkStream inputStream = incomingClient.GetStream();
                            while ((received = inputStream.Read(data, 0, data.Length)) != 0)
                            {
                                data = Security.DecryptBytes(data, received);
                                string input = Encoding.ASCII.GetString(data, 0, data.Length);
                                if (!input.Equals(Message.GetFile)) break;

                                FileInfo file = GetFile();

                                IPAddress clientAddress = ((IPEndPoint) incomingClient.Client.RemoteEndPoint).Address;
                                Program.MainForm.WriteOutput(string.Format("Sending file: {0} to client@{1}", file.Name,
                                                                           clientAddress));

                                data =
                                    Encoding.ASCII.GetBytes(String.Concat(Message.File,
                                                                          string.Format("{0}|{1}", file.Name,
                                                                                        file.Length)));
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
                    serverRequestClient.Client.ReceiveTimeout = 200;
                    serverRequestClient.Client.SendTimeout = 200;

                    while (!_streamClientWorker.CancellationPending)
                    {
                        var random = new Random();
                        try
                        {
                            serverRequestClient.Connect(_router);

                            byte[] data = Encoding.ASCII.GetBytes(Message.GetServer);
                            data = Security.EncryptBytes(data);
                            serverRequestClient.Send(data, data.Length);

                            Program.MainForm.WriteOutput("Waiting for next available server");
                            var client = new IPEndPoint(IPAddress.Any, 0);
                            data = serverRequestClient.Receive(ref client);
                            data = Security.DecryptBytes(data, data.Length);

                            string input = Encoding.ASCII.GetString(data);
                            if (input.Equals(Message.NoServers))
                            {
                                Program.MainForm.WriteOutput("No servers available", true);
                                Thread.Sleep(random.Next(1000, 3000));
                                continue;
                            }
                            if (input.Equals(Message.NoRouters))
                            {
                                Program.MainForm.WriteOutput("No routers available", true);
                                Thread.Sleep(random.Next(1000, 3000));
                                continue;
                            }

                            using (var remoteClient = new TcpClient())
                            {
                                remoteClient.Connect(IPAddress.Parse(input), Ports.Streaming);

                                Program.MainForm.WriteOutput(string.Format("Connected to server@{0} ",
                                           IPAddress.Parse(input)));

                                data = Encoding.ASCII.GetBytes(Message.GetFile);
                                data = Security.EncryptBytes(data);

                                NetworkStream clientStream = remoteClient.GetStream();

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
                                        Thread.Sleep(300);
                                        break;
                                    }
                                    if (!input.Contains(Message.File)) break;

                                    string[] fileinfo = input.Replace(Message.File, String.Empty).Split('|');
                                    string filename = fileinfo[0];
                                    long filesize = Convert.ToInt64(fileinfo[1]);

                                    IPAddress remoteAddress = ((IPEndPoint) remoteClient.Client.RemoteEndPoint).Address;
                                    Program.MainForm.WriteOutput(
                                        string.Format("Incoming file from server@{0} - name: {1}, size: {2} bytes",
                                                      remoteAddress, filename, filesize));

                                    string downloadedFile = Path.Combine(Directories.Downloads, filename);
                                    using (var fileStream = new FileStream(downloadedFile, FileMode.Create))
                                    {
                                        int total = 0;
                                        var bytes = new List<byte>();
                                        while ((received = clientStream.Read(data, 0, data.Length)) != 0)
                                        {
                                            total += received;
                                            Program.MainForm.UpdateProgressBar(total, filesize);
                                            bytes.AddRange(data);
                                        }

                                        data = bytes.ToArray();
                                        data = Security.DecryptBytes(data, total);
                                        fileStream.Write(data, 0, data.Length);
                                    }

                                    //(new SoundPlayer(downloadedFile)).PlaySync();

                                    Program.MainForm.ResetProgressBar();
                                    break;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        Thread.Sleep(random.Next(1000, 3000));
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