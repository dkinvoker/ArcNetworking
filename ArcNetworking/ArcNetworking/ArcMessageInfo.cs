using System;
using System.Collections.Generic;
using System.Text;

namespace ArcNetworking
{
    class ArcMessageInfo : EventArgs
    {
        public ArcClient ArcClient { get; }
        public byte[] RawMessage { get; }

        public string Message => this.ArcClient.Encoding.GetString(this.RawMessage);

        public ArcMessageInfo(ArcClient client, byte[] data)
        {
            this.ArcClient = client;
            this.RawMessage = data;
        }
    }
}
