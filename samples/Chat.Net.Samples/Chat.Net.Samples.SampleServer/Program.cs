using System;

using Chat.Net;

namespace Chat.Net.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SampleServer("127.0.0.1", 8888);
            server.Main();
        }
    }
}
