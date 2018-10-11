using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Chat.Core;
using Chat.Net;

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

        public Client(int id)
        {
            Id = id;
            MessageId = 0;

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
            ConnectToReplicas();            
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
            for (int i=0; i<ReplicasCount; ++i)
            {
                try
                {
                    var c = new Chat.Net.Client();
                    var replica = c.Connect(ReplicasIps[i], ReplicasPorts[1]);
                    Replicas.Add(replica);
                    Send(replica, Command.Connect);
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
        }
    }
}