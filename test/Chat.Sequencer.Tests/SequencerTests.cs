using System;
using Xunit;

using System.IO;

namespace Chat.Sequencer.Tests
{
    public class SequencerTests
    {
        [Fact]
        public void Setup_Usual_Works()
        {
            var ipAddress = "127.0.0.1";
            var port = 8888;

            var configFile = MockConfigFile(ipAddress, port);

            var seq = new Sequencer();
            seq.Setup(configFile);

            Assert.Equal(ipAddress, seq.IpAddress);
            Assert.Equal(port, seq.Port);
        }

        private FileStream MockConfigFile(string ipAddress, int port)
        {
            var configFile = new FileStream("mock.json", FileMode.Create);
            var writer = new StreamWriter(configFile);

            writer.Write("{\"sequencer\" : { \"ipAddress\" : \"" + ipAddress + "\", \"port\" : " + port + " }}");
            writer.Flush();

            configFile.Seek(0, SeekOrigin.Begin);

            return configFile;
        }
    }
}
