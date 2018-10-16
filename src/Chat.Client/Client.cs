using System;
using System.IO;
using System.Collections.Generic;

using System.Threading;

using Newtonsoft.Json.Linq;

using Chat.Core;
using Chat.Net;

using System.Net;

using System.Net.Sockets;

using System.Text;

namespace Chat.Client
{
    public class Client
    {
        private List<string> ReplicasIps;
        private List<int> ReplicasPorts;
        private int ReplicasCount;
        private List<Connection> Replicas;

        private string SequencerIp;
        private int SequencerPort;
        private Connection Sequencer;

        private int Id;
        private int MessageId;

        private int NextMessageOrder;

        private string Username;

        public Client(int id)
        {
            Id = id;
            MessageId = 0;
            NextMessageOrder = 0;

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

            ReplicasCount = configs["replicasCount"].Value<int>();

            for (int i=0; i<ReplicasCount; ++i)
            {
                ReplicasIps.Add(configs["replicas"][i]["ipAddress"].Value<string>());
                ReplicasPorts.Add(configs["replicas"][i]["port"].Value<int>());
            }
        }

        public void Start()
        {
            ConnectToSequencer();

            Console.WriteLine("------------------------------\n");
            Console.WriteLine("Pick a username:");
            Username = Console.ReadLine();
            Console.WriteLine("------------------------------\n");

            Menu();            
        }
        
        
        private void Menu()
        {
            string input;
            string[] parts;
            while(true)
            {
                Console.Write("> ");
                input = Console.ReadLine();
                parts = input.Split(' ', 2);
                
                string args;
                if (parts.Length == 2)
                {
                    args = parts[1];
                }
                else
                {
                    args = "";
                }

                switch (parts[0])
                {
                    case "send":
                        SendCommand(args);
                        break;

                    case "read":
                        ReadCommand(args);
                        break;

                    case "random":
                        RandomCommand(Int32.Parse(args));
                        break;

                    default:
                        break;
                }
            }
        }

        private void SendCommand(string arg)
        {
            Send(Sequencer,Command.Send, new string[]{Username + ": " + arg});
        }

        private void ReadCommand(string arg)
        {
            switch (arg)
            {
                case "":
                    ConnectToReplicas();                    
                    Listen();
                    break;
                
                case "all":
                    NextMessageOrder = 0;
                    ConnectToReplicas();                    
                    Listen();
                    break;
                
                default:
                    break;
            }
        }

        private void RandomCommand(int reps)
        {
            string[] names = {"Alpha","Bravo","Charlie","Delta","Echo","Foxtrot","Golf","Hotel","India","Juliet","Kilo","Lima","Mike","November","Oscar","Papa","Quebec","Romeo","Sierra","Tango","Uniform","Victor","Whiskey","X-ray","Yankee","Zulu"};

            Random rng = new Random();

            string msg;

            for (int i=0; i<reps; ++i)
            {
                msg = String.Format("{0}: [{1}] {2}", Username, i, names[rng.Next(0, names.Length)]);

                Console.WriteLine(msg);
                Send(Sequencer,Command.Send, new string[]{msg});
                Thread.Sleep(rng.Next(100));
            }
        }

        private void Listen()
        {
            foreach (Connection replica in Replicas)
            {
                try
                {
                    Send(replica,Command.CatchUp, new string[]{NextMessageOrder.ToString()});
                }
                catch(System.IO.IOException)
                {
                    Console.WriteLine("send error");
                }
                
            }

            Console.WriteLine("\n------------------------------\n");

            foreach (Connection replica in Replicas)
            {
                try
                {
                    string messageStr = replica.Receive();
                    while(messageStr == "")
                    {
                        messageStr = replica.Receive();
                    }
                    var message = new Message(messageStr);

                    foreach(var s in message.Args)
                    {
                        var parts = s.Split(' ', 2);
                        var order = Int32.Parse(parts[0]);
                        
                        if (order == NextMessageOrder)
                        {
                            Console.WriteLine(s);
                            ++NextMessageOrder;
                        }
                    }
                }
                catch(System.IO.IOException)
                {
                    Console.WriteLine("Receive error");
                }
            }

            Console.WriteLine("\n------------------------------\n");
        }

        private void ConnectToSequencer()
        {
            var c = new Chat.Net.Client();
            Sequencer = c.Connect(SequencerIp, SequencerPort);
            Send(Sequencer, Command.Connect);
        }

        private void Send(Connection destin, Command command, string[] args=null)
        {
            var msg = new Message(0, ProcessType.Client, Id, MessageId++, command, args);
            destin.Send(msg.ToString());
        }

        private void ConnectToReplicas()
        {
            DisconnectFromReplicas();
            for (int i=0; i<ReplicasCount; i++)
            {
                try
                {
                    var c = new Chat.Net.Client();
                    var replica = c.Connect(ReplicasIps[i], ReplicasPorts[i]);
                    Replicas.Add(replica);
                    Send(replica, Command.Connect);
                    //Console.WriteLine("Connected to replica " + i + ".");
                }
                catch
                {
                    //Console.WriteLine("Failed to connect to replica " + i + ".");
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