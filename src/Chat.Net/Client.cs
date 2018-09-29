using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Chat.Net 
{
    public class Client
    {
        public Connection Connection { get; private set; }

        public Connection Connect(string ipAddress, int port)
        {
            var socket = new System.Net.Sockets.TcpClient();
            socket.Connect(ipAddress, port);
            Connection = new Connection(socket);
            return Connection;
        }
    }
}