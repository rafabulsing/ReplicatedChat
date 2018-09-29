using System.Net.Sockets;

namespace Chat.Net
{
    public class Connection
    {
        private TcpClient Socket { get; set; }
        private NetworkStream Stream { get; set; }
        private IMessenger Messenger { get; set; }

        public Connection(TcpClient socket)
        {
            Socket = socket;
            Stream = socket.GetStream();
            Messenger = new Messenger();
        }

        public void Send(string message)
        {
            Messenger.Send(Stream, message);
        }

        public string Receive()
        {
            return Messenger.Receive(Stream);
        }

        public void Disconnect()
        {
            Stream.Close();         
            Socket.Close();  
        }
    }
}