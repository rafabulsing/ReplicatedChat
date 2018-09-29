using Xunit;

using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using Chat.Net;

namespace Chat.Net.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void SendReceive_Usual_Works()
        {            
            var message = "Hello World!";
            var ipAddress = "127.0.0.1";
            var port = 8888;

            var server = new Server();
            server.Start(ipAddress, port);
            
            var client = new Client();
        
            var clientCon = client.Connect(ipAddress, port);
            var serverCon = server.AcceptConnection();

            clientCon.Send(message);
            serverCon.Send(serverCon.Receive());
            var reply = clientCon.Receive();

            server.Stop();
            Assert.Equal(message, reply);
        }


        [Fact]
        public void SendReceive_MultipleConnections_Works()
        {            
            string[] messages = {"Hello", "Oi", "Hola"};
            string[] replies = new string[messages.Length];

            var ipAddress = "127.0.0.1";
            var port = 8888;

            var server = new Server();
            server.Start(ipAddress, port);
            
            Client[] clients = {new Client(), new Client(), new Client()};
            Connection[] clientCons = new Connection[clients.Length];
            Connection[] serverCons = new Connection[clients.Length];

            for(int i=0; i<clients.Length; ++i)
            {
                clientCons[i] = clients[i].Connect(ipAddress, port);
                serverCons[i] = server.AcceptConnection();
            }

            for(int i=0; i<clients.Length; ++i)
            {
                clientCons[i].Send(messages[i]);
            }

            for(int i=0; i<clients.Length; ++i)
            {
                serverCons[i].Send(serverCons[i].Receive());
            }

            for(int i=0; i<clients.Length; ++i)
            {
                replies[i] = clientCons[i].Receive();
            }

            server.Stop();
            Assert.Equal(messages, replies);
        }

        [Fact]
        public void SendReceive_MultipleMessages_Works()
        {            
            string[] messages = {"Hello", "Oi", "Hola"};
            string[] replies = new string[messages.Length];

            var ipAddress = "127.0.0.1";
            var port = 8888;

            var server = new Server();
            server.Start(ipAddress, port);
            
            var client = new Client();
        
            var clientCon = client.Connect(ipAddress, port);
            var serverCon = server.AcceptConnection();

            foreach(var m in messages)
            {
                clientCon.Send(m);
            }

            foreach(var m in messages)
            {
                serverCon.Send(serverCon.Receive());
            }

            for (int i=0; i<messages.Length; ++i)
            {
                replies[i] = clientCon.Receive();
            }

            server.Stop();

            Assert.Equal(messages, replies);
        }
    }
}
