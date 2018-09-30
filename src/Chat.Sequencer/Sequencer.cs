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

        public Sequencer()
        {
            Server = new Server();
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
    }
}