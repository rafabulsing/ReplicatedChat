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

        private Dictionary<int, Connection> Clients;

        private int Id;
        private string IpAddress;
        private int Port;
        private Server Server;

        public Replica(int id)
        {
            Id = id;

            History = new List<string>();
            Clients = new Dictionary<int, Connection>();
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
        }

        public void Start()
        {
            Server = new Server();
            Server.Start(IpAddress, Port);
            
            Console.WriteLine(String.Format("Replica {0} started.\nIP: {1}\nPort: {2}\n", Id, IpAddress, Port));

            ConnectToSequencer();

            while(true)
            {
                Console.WriteLine("Waiting for client connections...");
                AcceptClient();
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

        private void AcceptClient()
        {
            var client = Server.AcceptConnection();
            var msgStr = client.Receive();

            while(msgStr == "")
            {
                Thread.Sleep(50);
                msgStr = client.Receive();
            }

            var msg = new Message(msgStr);

            Clients.Add(msg.ProcessId, client);

            Console.WriteLine("Connected to client " + msg.ProcessId + ".");
        }

    }
}