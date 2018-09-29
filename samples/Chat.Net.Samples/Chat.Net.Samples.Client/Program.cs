using System;

using Chat.Net;

namespace Chat.Net.Samples.SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client();
            var connection = client.Connect("127.0.0.1", 8888);

            var message = Console.ReadLine();

            while(true)
            {
                connection.Send(message);
                Console.WriteLine(connection.Receive());
            }
        }
    }
}
