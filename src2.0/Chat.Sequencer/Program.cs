using System;
using System.IO;

namespace Chat.Sequencer
{
    class Program
    {
        static void Main(string[] args)
        {
            var configsPath = "../configs.json";
            var configsFile = new FileStream(configsPath, FileMode.Open);

            var seq = new Sequencer();
            seq.Setup(configsFile);
            seq.Start();
        }
    }
}
