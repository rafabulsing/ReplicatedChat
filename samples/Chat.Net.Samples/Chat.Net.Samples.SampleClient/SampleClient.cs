using System;

using Chat.Net;

namespace Chat.Net.Samples
{
    class SampleClient
    {
        private Client Client { get; set; }
        private Connection Connection { get; set; }

        public SampleClient(string ipAddress, int port)
        {
            Client = new Client();
            Connection = Client.Connect(ipAddress, port);
        }

        public void Main()
        {
            Console.WriteLine("Selecione o modo de entrada:\n 1 - Manual\n 2 - Aleatório\n 3 - Estático\n");

            var mode = Console.ReadLine();

            switch(mode)
            {
                case "1":
                    Manual();
                    break;

                case "2":
                    Random();
                    break;
                
                default:
                    Static();
                    break;
            }
        }

        public void Manual()
        {
            string msg;
            bool error = false;
            while(!error)
            {
                msg = Console.ReadLine();
                error = SendReceive(msg);
            }
        }

        public void Random()
        {
            string[] names = {"Alpha","Bravo","Charlie","Delta","Echo","Foxtrot","Golf","Hotel","India","Juliet","Kilo","Lima","Mike","November","Oscar","Papa","Quebec","Romeo","Sierra","Tango","Uniform","Victor","Whiskey","X-ray","Yankee","Zulu"};

            Random rng = new Random();

            string msg;
            bool error = false;
            while(!error)
            {
                msg = names[rng.Next(0, names.Length)];
                error = SendReceive(msg);
            }
        }
    
        public void Static()
        {
            var msg = Console.ReadLine();
            bool error = false;
            while(!error)
            {
                error = SendReceive(msg);
            }
        }

        public bool SendReceive(string message)
        {
            try
            {
                Connection.Send(message);
                Console.WriteLine("Sent: " + message);

                string reply = Connection.Receive();
                Console.WriteLine("Received: " + reply);

                if (reply != message)
                {
                    Console.WriteLine("###  ERRO  ###");
                    return true;
                }

                Console.WriteLine("\n-----------------\n");
                return false;
            }
            catch(System.IO.IOException)
            {
                Console.WriteLine("Disconnected.\n");
                return true;
            }
        }
    }
}