using System;
using System.IO;

namespace Chat.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var configsPath = "../configs.json";
            var configsFile = new FileStream(configsPath, FileMode.Open);

            var c = new Client(Int32.Parse(args[0]));
            c.Setup(configsFile);
            c.Start();
        }
    }
}
