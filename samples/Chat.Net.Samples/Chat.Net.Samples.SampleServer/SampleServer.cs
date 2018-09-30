using System;
using System.Threading;

using Chat.Net;

namespace Chat.Net.Samples
{
    class SampleServer
    {
        private Server Server { get; set; }

        public SampleServer(string ipAddress, int port)
        {
            Server = new Server();
            Server.Start("127.0.0.1", 8888);
        }

        public void Main()
        {
            while(true)
            {
                var connection = Server.AcceptConnection();
                
                Thread t = new Thread(()=>HandleConnection(connection));
                t.Start();
            }
        }

        private void HandleConnection(Connection connection)
        {
            string message;
            try
            {
                while(true)
                {
                    message = connection.Receive();
                    Console.WriteLine("Received: " + message);
                    connection.Send(message);
                    Console.WriteLine ("\n-----------------\n");
                }
            }
            catch(System.IO.IOException)
            {
                connection.Disconnect();
                Console.WriteLine("Disconnected.");
            }

        }

    }
}