using Xunit;

using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using Chat.Net;

namespace Chat.Net.Tests
{
    public class MessengerTests
    {
        [Fact]
        public void SendReceive_Usual_Works()
        {            
            var message = "Hello World!";
            var messenger = new Messenger();
            var stream = new MemoryStream();

            messenger.Send(stream, message);
            
            stream.Seek(0, SeekOrigin.Begin);
            var result = messenger.Receive(stream);

            Assert.Equal(message, result);
        }

        [Fact]
        public void SendReceive_EmptyMessage_Works()
        {
            var emptyMessage = "";
            var messenger = new Messenger();
            var stream = new MemoryStream();

            messenger.Send(stream, emptyMessage);

            stream.Seek(0, SeekOrigin.Begin);
            var result = messenger.Receive(stream);

            stream.Dispose();

            Assert.Equal(emptyMessage, result);
        }

        [Fact]
        public void SendReceive_MultipleMessages_Works()
        {
            string[] messages = {"Hello", "Oi", "Hola"};
            var messenger = new Messenger();
            var stream = new MemoryStream();

            foreach(var m in messages)
            {
                messenger.Send(stream, m);
            }

            stream.Seek(0, SeekOrigin.Begin);

            var results = new string[3];
            for(int i=0; i<messages.Length; ++i)
            {
                results[i] = messenger.Receive(stream);
            }

            stream.Dispose();

            Assert.Equal(messages, results);
        }

        [Fact]
        public void SendReceive_Unicode_Works()
        {
            var unicodeMessage = "áãâ éẽê íĩî óõô úũû ñ";
            var messenger = new Messenger();
            var stream = new MemoryStream();

            messenger.Send(stream, unicodeMessage);
            stream.Seek(0, SeekOrigin.Begin);

            var result = messenger.Receive(stream);

            stream.Dispose();

            Assert.Equal(unicodeMessage, result);
        }

       [Fact]
       public void Receive_EmptyStream_ReturnsEmptyString()
       {
            var messenger = new Messenger();
            var stream = new MemoryStream();

            var result = messenger.Receive(stream);

            stream.Dispose();

            Assert.Equal("", result);
       }
    }
}
