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
        private Dictionary<int, Message> EarlyMessages;
        private int NextMessageOrder;


        public Replica(int id)
        {
            Id = id;

            History = new List<string>();
            Clients = new List<Connection>();
            ReplicasIps = new List<string>();
            ReplicasPorts = new List<int>();
            Replicas = new List<Connection>();
            EarlyMessages = new Dictionary<int, Message>();

            NextMessageOrder = 0;
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
            
            Recover();
            
            DisconnectFromReplicas();

            while(true)
            {
                Console.WriteLine("Waiting for client connections...");
                var newConnection = Server.AcceptConnection();
                var connectionType = CategorizeConnection(newConnection);
                
                Thread t = new Thread(()=>HandleConnection(newConnection, connectionType));
                t.Start();
                
            }       
        }

        public void Recover()
        {
            Console.WriteLine("Recovering...");

            string line;
            using (StreamReader r = new StreamReader(File.OpenRead(LogFilePath)))
            {
                while(!r.EndOfStream)
                {
                    line = r.ReadLine();
                    Console.WriteLine(line);
                    History.Add(line);
                }
            }

            if (History.Count > 0)
            {
                var lastLine = History[History.Count-1];
                NextMessageOrder = Int32.Parse(lastLine.Split(' ', 2)[0])+1;
            }
            else
            {
                NextMessageOrder = 0;
            }

            Console.WriteLine("Recovering from replicas...");
            foreach (Connection replica in Replicas)
            {
                RecoverFromReplica(replica);
            }
        }
        

        private void HandleConnection(Connection connection, ProcessType connectionType)
        {

            try
            {
                string messageStr = connection.Receive();
                while("".Equals(messageStr))
                {
                    messageStr = connection.Receive();
                }
                Console.WriteLine(messageStr);

                var message = new Message(messageStr);

                if(message.Command == Command.CatchUp)
                { 
                    var lastMessage = Int32.Parse(message.Args[0]);

                    var messages = new List<string>();
                    for(int i=lastMessage; i<History.Count; ++i)
                    {
                        messages.Add(History[i]);
                    }


                    var reply = new Message(NextMessageOrder-1, ProcessType.Replica, Id, message.MessageId, Command.CatchUp, messages.ToArray());
                    
                    connection.Send(reply.ToString());

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
            History.Add(String.Format("{0} {1}", NextMessageOrder, message.Args[0]));
            ++NextMessageOrder;
        }

        
        private void RecoverFromReplica(Connection connection)
        {
            try
            {
                var catchUpMsg = new Message(0, ProcessType.Replica, Id, 0, Command.CatchUp, NextMessageOrder.ToString());

                connection.Send(catchUpMsg.ToString());

                var replyStr = connection.Receive();
                while(replyStr == "")
                {
                    replyStr = connection.Receive();
                }

                var reply = new Message(replyStr);

                using (StreamWriter w = File.AppendText(LogFilePath))
                {
                    foreach(var s in reply.Args)
                    {
                        var parts = s.Split(' ', 2);
                        var order = Int32.Parse(parts[0]);
                        
                        if (order == NextMessageOrder)
                        {
                            Console.WriteLine(s);
                            w.WriteLine(s);
                            History.Add(s);
                            ++NextMessageOrder;
                        }
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
                        if (message.TotalOrder == NextMessageOrder)
                        {
                            LogMessage(message);

                            while (EarlyMessages.ContainsKey(NextMessageOrder))
                            {
                                message = EarlyMessages[NextMessageOrder];
                                EarlyMessages.Remove(NextMessageOrder);
                                LogMessage(message);
                            }
                        }
                        else
                        {
                            EarlyMessages.Add(message.TotalOrder, message);
                        }
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


        private ProcessType CategorizeConnection(Connection connection)
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

            return message.Type;
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