using System;
using System.IO;
using System.Collections.Generic;

using System.Threading;

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

        Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1);
        Semaphore replicaSem = new Semaphore(1, 1);
        public Sequencer()
        {
            Server = new Server();
            SeqNumber = 0;
            History = new List<string>();
            Clients = new List<Connection>();
            Replicas = new List<Connection>();
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
                Console.WriteLine("Esperando clientes");
                var newConnection = Server.AcceptConnection();
                string option = CategorizeConnection(newConnection);
                if ("client".Equals(option))
                {
                    Thread t = new Thread(()=>HandleConnectionClients(newConnection));
                    t.Start();
                }
            }
        }

        private void HandleConnectionClients(Connection connection)
        {
            string message;
            while(true)
            {
                message = connection.Receive();
                if("".Equals(message))
                {
                    connection.Disconnect();
                    Console.WriteLine("Client Disconnected.");
                    break;
                }
                else
                {

                    try
                    {
                        new Message(message);
                    }
                    catch(Exception)
                    {
                        continue;
                    }
                
                    message = AddToSequence(message);
                    Console.WriteLine("Received: " + message);

                    replicaSem.WaitOne();
                    for (int i = 0; i < Replicas.Count; i++)
                    {
                        try
                        {
                            Replicas[i].Send(message);
                        }
                        catch(Exception)
                        {
                            Replicas[i].Disconnect();
                            Replicas.RemoveAt(i);
                            i--;
                            Console.WriteLine("Replica Disconnected.");
                        }
                    }
                    replicaSem.Release();
                    Console.WriteLine ("-----------------");
                }
            }
        }

        private string CategorizeConnection(Connection connection)
        {
            var messageText = connection.Receive();
            var message = new Message(messageText);

            if (message.Type == ProcessType.Client)
            {
                Clients.Add(connection);
                return "client";
            }
            else if (message.Type == ProcessType.Replica)
            {
                Replicas.Add(connection);
                return "replica";
            }
            else 
            {
                connection.Disconnect();
                return "nothing";
            }
        }

        private string AddToSequence(string message)
        {
            semaphoreObject.WaitOne();
            var sequenced = Message.CreateWithNewOrder(message, SeqNumber);
            ++SeqNumber;
            History.Add(sequenced);
            semaphoreObject.Release();
            return sequenced;
        }
    }
}