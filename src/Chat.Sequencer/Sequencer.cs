using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Chat.Core;
using Chat.Net;

namespace Chat.Sequencer
{
    public class Sequencer
    {
        private Server Server;
        private List<Connection> Clients;
        private List<Connection> Replicas;

        public string IpAddress { get; private set; }
        public int Port { get; private set; }

        private int SeqNumber { get; set; }

        private List<string> History { get; set; }

        public Sequencer()
        {
            Server = new Server();
            SeqNumber = 0;
            History = new List<string>();
        }

        public void Setup(FileStream configsFile)
        {
            string configsText;
            configsFile.Position = 0;
            using (StreamReader streamReader = new StreamReader(configsFile))
            {
                configsText = streamReader.ReadToEnd();
            }

            var configs = JObject.Parse(configsText);
            IpAddress = configs["sequencer"]["ipAddress"].Value<string>();
            Port = configs["sequencer"]["port"].Value<int>();
        }
    
        public void Start()
        {
            Server.Start(IpAddress, Port);

            Console.WriteLine("Sequencer started.");

            Main();
        }

        public void Main()
        {
            while(true)
            {
                if (Server.Pending())
                {
                    var newConnection = Server.AcceptConnection();
                    CategorizeConnection(newConnection);
                }

                string message, sequenced;
                foreach(var client in Clients)
                {
                    try{
                        message = client.Receive();
                        
                        if (message != "")
                        {
                            sequenced = AddToSequence(message);
                            
                            foreach(var replica in Replicas)
                            {
                                replica.Send(sequenced);
                            }

                            Console.WriteLine(sequenced);
                        }
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Client disconnected.");
                    }
                }
     
            }
        }

        private void CategorizeConnection(Connection connection)
        {
            var messageText = connection.Receive();
            var message = new Message(messageText);

            if (message.Type == ProcessType.Client)
            {
                Clients.Add(connection);
            }
            else if (message.Type == ProcessType.Replica)
            {
                Replicas.Add(connection);
            }
            else 
            {
                connection.Disconnect();
            }
        }

        private string AddToSequence(string message)
        {
            var sequenced = Message.CreateWithNewOrder(message, SeqNumber);
            ++SeqNumber;
            History.Add(sequenced);
            return sequenced;
        }

        
    
    }
}