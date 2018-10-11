using System;
using System.IO;

namespace Chat.Replica
{
    class Program
    {
        static void Main(string[] args)
        {
            var configsPath = "../configs.json";
            var configsFile = new FileStream(configsPath, FileMode.Open);

            var r = new Replica(Int32.Parse(args[0]));
            r.Setup(configsFile);
            r.Start();
        }
    }
}
