using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Chat.Net;

namespace Chat.Sequencer
{
    public class Sequencer
    {
        private Server Server;
        private List<Connection> Connections {
            get => Server.Connections;
        }

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
                    Server.AcceptConnection();
                }

                string message, sequenced;
                foreach(var c in Connections)
                {
                    try{
                        message = c.Receive();
                        
                        if (message != "")
                        {
                            sequenced = SequencedMessage(message);
                            History.Add(sequenced);
                            Server.Broadcast(sequenced);
                            Console.WriteLine(sequenced);
                        }
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Server disconnected.");
                    }
                }
     
            }
        }

        private string SequencedMessage(string message)
        {
            var sequenced = "[" + SeqNumber + "]" + message;
            ++SeqNumber;
            return sequenced;
        }

        
    
    }
}