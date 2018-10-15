using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Chat.Core;
using Chat.Net;

namespace Chat.Replica
{
    public class Replica
    {
        private string SequencerIp;
        private int SequencerPort;
        private Connection Sequencer;
        private List<string> History;
        private List<string> ReplicasIps;
        private List<int> ReplicasPorts;
        private List<Connection> Replicas;
        private int ReplicasCount;

        private List<Connection> Clients;

        private int Id;
        private string IpAddress;
        private int Port;
        private Server Server;

        private string LogFilePath;


        public Replica(int id)
        {
            Id = id;

            History = new List<string>();
            Clients = new List<Connection>();
            ReplicasIps = new List<string>();
            ReplicasPorts = new List<int>();
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
            
            SequencerIp = configs["sequencer"]["ipAddress"].Value<string>();
            SequencerPort = configs["sequencer"]["port"].Value<int>();

            IpAddress = configs["replicas"][Id]["ipAddress"].Value<string>();
            Port = configs["replicas"][Id]["port"].Value<int>();

            ReplicasCount = configs["replicasCount"].Value<int>();

            for (int i=0; i<ReplicasCount; ++i)
            {
                if(configs["replicas"][i]["port"].Value<int>() != Port)
                {
                    ReplicasIps.Add(configs["replicas"][i]["ipAddress"].Value<string>());
                    ReplicasPorts.Add(configs["replicas"][i]["port"].Value<int>());
                }
            }

            LogFilePath = configs["replicas"][Id]["logFile"].Value<string>();

            if (!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath).Close();
            }
        }


        public void Start()
        {
            Server = new Server();
            Server.Start(IpAddress, Port);
            
            Console.WriteLine(String.Format("Replica {0} started.\nIP: {1}\nPort: {2}\n", Id, IpAddress, Port));

            ConnectToSequencer();
            Thread tSeq = new Thread(()=>Listen());
            tSeq.Start();

            ConnectToReplicas();
            foreach (Connection replica in Replicas)
            {
                MessegerRecover(replica);
            }
            DisconnectFromReplicas();

            while(true)
            {
                Console.WriteLine("Waiting for client connections...");
                var newConnection = Server.AcceptConnection();
                string option = CategorizeConnection(newConnection);
                
                Thread t = new Thread(()=>HandleConnection(newConnection));
                t.Start();
                
            }       
        }
        

        private void HandleConnection(Connection connection)
        {

            try
            {
                string messageStr = connection.Receive();
                while("".Equals(messageStr))
                {
                    messageStr = connection.Receive();
                }

                var message = new Message(messageStr);

                if(message.Command == Command.CatchUp)
                {
                    connection.Send(new Message(1,ProcessType.Replica,Id,0,Command.Send, "Historico").ToString());
                    /*
                     * ...::AJUSTAR::...
                     * Enviar Log para a conexão
                     */
                }
            }
            catch(System.IO.IOException)
            {
                Console.WriteLine("Disconnected.");
            }
        }


        private void LogMessage(Message message)
        {
            using (StreamWriter w = File.AppendText(LogFilePath))
            {
                w.WriteLine(message.TotalOrder + " " + message.Args[0]);
            }
        }

        
        private void MessegerRecover(Connection connection)
        {
            try
            {
                bool recover = false;
                while (recover == false)
                {
                    connection.Send("RECOVER");
                    string message = connection.Receive();
                    /*
                     * ...::AJUSTAR::...
                     * Receber e salvar Log das replicas
                     */
                    Console.WriteLine(message);
                    if (message != "")
                    {
                        recover = true;
                    }
                }
            }
            catch(System.IO.IOException)
            {
                Console.WriteLine("Disconnected.");
            }   
        }


        private void Listen()
        {
            while(true)
            {
                try
                {
                    string messageStr = Sequencer.Receive();
                    while("".Equals(messageStr) )
                    {
                        messageStr = Sequencer.Receive();
                    }

                    Console.WriteLine(messageStr);

                    var message = new Message(messageStr);

                    if (message.Command == Command.CatchUp)
                    {
                        for (int i = 0; i < Clients.Count; i++)
                        {
                            try
                            {
                                /*
                                 * ...::AJUSTAR::...
                                 * Enviar todas mensagens que faltam
                                 */
                                Clients[i].Send(messageStr);
                            }
                            catch(System.IO.IOException)
                            {
                                Clients[i].Disconnect();
                                Clients.RemoveAt(i);
                                i--;
                                Console.WriteLine("Disconnected.");
                            }
                        }
                    }
                    else if (message.Command == Command.Send)
                    {
                        /*
                         * ...::AJUSTAR::...
                         * salvar mensagem
                         */
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Sequencer disconnected.");
                }
            }
        }


        private void ConnectToSequencer()
        {
            var c = new Chat.Net.Client();
            Sequencer = c.Connect(SequencerIp, SequencerPort);
            
            var msg = new Message(0,ProcessType.Replica,Id,0,Command.Connect);
            Sequencer.Send(msg.ToString());

            Console.WriteLine("Connected to Sequencer.");
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


        private void ConnectToReplicas()
        {
            DisconnectFromReplicas();
            for (int i=0; i<ReplicasCount -1; i++)
            {
                try
                {
                    var c = new Chat.Net.Client();
                    var replica = c.Connect(ReplicasIps[i], ReplicasPorts[i]);
                    
                    var msg = new Message(0,ProcessType.Replica,Id,0,Command.Connect);
                    replica.Send(msg.ToString());
                    
                    Replicas.Add(replica);
                    Console.WriteLine("Connected to replica " + i + ".");
                }
                catch
                {
                    Console.WriteLine("Failed to connect to replica " + i + ".");
                }
            }
        }


        private void DisconnectFromReplicas()
        {
            foreach (var r in Replicas)
            {
                r.Disconnect();
            }
            Replicas.Clear();
        }

    }
}