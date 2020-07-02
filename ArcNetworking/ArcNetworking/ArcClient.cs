using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ArcNetworking
{
    public class ArcClient
    {
        public Encoding Encoding { get; private set; }
        internal TcpClient TcpClient { get; set; }

        public bool IsConnected => this.TcpClient.Connected;

        internal ArcClient(Encoding encoding, TcpClient tcpClient)
        {
            this.Encoding = encoding;
            this.TcpClient = tcpClient;
        }

        private byte[] Buffer { get; }
        public int BufferSize => this.Buffer.Length;

        public ArcClient(int port, string address, Encoding encoding, int bufferSize = 10240)
        {
            this.Encoding = encoding;
            this.Buffer = new byte[bufferSize];
            this.TcpClient = new TcpClient(address, port);
        }

        public void Send(byte[] rawData)
        {
            var stream = this.TcpClient.GetStream();
            stream.Write(rawData, 0, rawData.Length);
        }
        public void Send(string message)
        {
            this.Send(this.Encoding.GetBytes(message));
        }

        public void SendAndCloseConnection(byte[] rawData)
        {
            this.Send(rawData);
            this.TcpClient.Close();
        }

        public void SendAndCloseConnection(string msg)
        {
            this.SendAndCloseConnection(this.Encoding.GetBytes(msg));
        }

        public ArcMessageInfo SendAndWeitForResponse(byte[] rawData, double timeoutInSecounds = 5)
        {
            var maxWeitTime = DateTime.Now.AddSeconds(timeoutInSecounds);
            this.Send(rawData);

            var networkStream = this.TcpClient.GetStream();
            int bytesCount;
            while (true)
            {
                if (maxWeitTime > DateTime.Now)
                {
                    bytesCount = networkStream.Read(this.Buffer, 0, this.BufferSize);
                    if (bytesCount > 0)
                    {
                        byte[] bufferCopy = new byte[this.BufferSize];
                        this.Buffer.CopyTo(bufferCopy, 0);
                        return new ArcMessageInfo(this, bufferCopy);
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }
    }
}
