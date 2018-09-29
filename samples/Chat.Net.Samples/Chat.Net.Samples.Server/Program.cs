using System;

using Chat.Net;

namespace Chat.Net.Sample.SampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            server.Start("127.0.0.1", 8888);
            var connection = server.AcceptConnection();

            while(true)
            {
                var reply = "Message received: " + connection.Receive();
                Console.WriteLine(reply);
                connection.Send(reply);    
            }
        }
    }
}
