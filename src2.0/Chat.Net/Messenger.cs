using System;
using System.IO;
using System.Text;

namespace Chat.Net 
{
    public class Messenger : IMessenger
    {
        public void Send(Stream stream, string message)
        {
            var uniEncoding = new UnicodeEncoding();
            
            int messageLength = uniEncoding.GetByteCount(message);
            byte[] lengthBuffer = BitConverter.GetBytes(messageLength);
            stream.Write(lengthBuffer, 0, 4);

            Byte[] buffer = uniEncoding.GetBytes(message);
            
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        public string Receive(Stream stream)
        {
            var sizeBuffer = new Byte[4];
            stream.Read(sizeBuffer, 0, 4);
            var sizeResult = BitConverter.ToInt32(sizeBuffer, 0);

            Byte[] buffer = new Byte[sizeResult];
            stream.Read(buffer, 0, sizeResult);
            
            var uniEncoding = new UnicodeEncoding();
            var chars = new char[uniEncoding.GetCharCount(buffer)];
            uniEncoding.GetDecoder().GetChars(buffer, 0, buffer.Length, chars, 0);
            var result = new string(chars);

            return result;
        }
    }
}