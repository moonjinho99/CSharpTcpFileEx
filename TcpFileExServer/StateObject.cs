using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace TcpFileExServer
{
    class StateObject
    {
        public Socket workSocket = null;

        public const int BufferSize = 4096;

        public byte[] buffer = new byte[BufferSize];
    }
}
