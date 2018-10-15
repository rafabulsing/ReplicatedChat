using System.IO;

namespace Chat.Net
{
    public interface IMessenger
    {
        void Send(Stream stream, string message);
        string Receive(Stream stream);
    }
}