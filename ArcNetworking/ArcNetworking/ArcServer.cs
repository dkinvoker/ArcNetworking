using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ArcNetworking
{
    class ArcServer : IDisposable
    {
        #region Props
        private TcpListener TcpServer { get; set; }

        /// <summary>
        /// Size of byte buffer used in communication.
        /// Can only be set via constructor
        /// </summary>

        private byte[] Buffer { get; }
        public int BufferSize => this.Buffer.Length;
        private Thread AcceptConnectionThread { get; set; }
        private Thread ListeningThread { get; set; }
        private Encoding Encoding { get; set; }

        private List<ArcClient> Connections { get; } = new List<ArcClient>();

        public ArcClient[] ActiveConnections => this.Connections.Where(c => c.IsConnected).ToArray();

        public bool IsStarted { get; private set; } = false;

        #endregion

        #region Events
        public event EventHandler<ArcMessageInfo> MessageRecived;
        #endregion

        public ArcServer(int port, Encoding encoding, int bufferSize = 10240)
        {
            this.TcpServer = new TcpListener(Dns.GetHostEntry("localhost").AddressList[0], port);
            this.Encoding = encoding;
            this.Buffer = new byte[bufferSize];
            this.AcceptConnectionThread = new Thread(new ThreadStart(AcceptConnectionLoop));
            this.ListeningThread = new Thread(new ThreadStart(ListeningLoop));
        }

        public void Start()
        {
            this.TcpServer.Start();
            this.AcceptConnectionThread.Start();
            this.ListeningThread.Start();

            this.IsStarted = true;
        }

        public void Stop()
        {
            this.ListeningThread.Abort();
            this.AcceptConnectionThread.Abort();
            this.TcpServer.Stop();

            this.IsStarted = false;
        }

        private void AcceptConnectionLoop()
        {
            while (true)
            {
                var client = this.TcpServer.AcceptTcpClient();
                lock (this.Connections)
                {
                    this.Connections.Add(new ArcClient(this.Encoding, client));
                }
            }
        }

        private void ListeningLoop()
        {
            var randomizer = new Random();
            while (true)
            {
                var connections = this.ActiveConnections.OrderBy(c => randomizer.Next());               
                foreach (var conn in connections)
                {
                    var networkStream = conn.TcpClient.GetStream();
                    int bytesCount;
                    bytesCount = networkStream.Read(this.Buffer, 0, this.BufferSize);
                    if (bytesCount > 0)
                    {
                        byte[] bufferCopy = new byte[this.BufferSize];
                        this.Buffer.CopyTo(bufferCopy, 0);
                        this.MessageRecived?.Invoke(this, new ArcMessageInfo(conn, bufferCopy));
                    }
                }
            }       
        }

        public void Dispose()
        {
            if (this.IsStarted)
            {
                this.Stop();
            }
        }
    }
}
