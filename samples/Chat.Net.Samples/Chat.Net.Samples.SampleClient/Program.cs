using System;

using Chat.Net;

namespace Chat.Net.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new SampleClient("127.0.0.1", 8888);
            client.Main();
        }
    }
}
