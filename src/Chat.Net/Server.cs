using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;


namespace Chat.Net 
{
    public class Server
    {
        private TcpListener ServerSocket { get; set; }
        private List<Connection> Connections { get; set; } = new List<Connection>();

        public void Start(string ipAddress, int port)
        {
            ServerSocket = new TcpListener(IPAddress.Parse(ipAddress), port);
            ServerSocket.Start();
        }

        public Connection AcceptConnection()
        {
            var socket = ServerSocket.AcceptTcpClient();
            var connection = new Connection(socket);
            Connections.Add(connection);
            return connection;
        }

        public void Stop()
        {
            foreach(var c in Connections)
            {
                c.Disconnect();
            }
            ServerSocket.Stop();
        }
    }
}