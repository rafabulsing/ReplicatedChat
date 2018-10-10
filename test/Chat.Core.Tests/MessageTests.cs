using System;
using Xunit;

using Chat.Core;

namespace Chat.Core.Tests
{
    public class MessageTests
    {
        [Fact]
        public void CreateByString_Usual_Works()
        {
            var msgStr = "1|client|0|0|send|Rafael";

            var msg = new Message(msgStr);

            Assert.Equal(1, msg.TotalOrder);
            Assert.Equal(ProcessType.Client, msg.Type);
            Assert.Equal(0, msg.ProcessId);
            Assert.Equal(0, msg.MessageId);
            Assert.Equal(Command.Send, msg.Command);
            Assert.Equal("Rafael", msg.Args[0]);
        }

        [Fact]
        public void CreateByParts_Usual_Works()
        {
            var totalOrder = 0;
            var type       = ProcessType.Client;
            var processId  = 0;
            var messageId  = 0;
            var command    = Command.Send;
            var args       = new string[1] {"Testing"};

            var msg = new Message(totalOrder, type, processId, messageId, command, args);

            Assert.Equal(0, msg.TotalOrder);
            Assert.Equal(ProcessType.Client, msg.Type);
            Assert.Equal(0, msg.ProcessId);
            Assert.Equal(0, msg.MessageId);
            Assert.Equal(Command.Send, msg.Command);
            Assert.Equal("Testing", msg.Args[0]);
        }

        [Fact]
        public void CreateByParts_OneArgumentWithoutArray_Works()
        {
            var totalOrder = 0;
            var type       = ProcessType.Client;
            var processId  = 0;
            var messageId  = 0;
            var command    = Command.Send;
            var args       = "Testing";

            var msg = new Message(totalOrder, type, processId, messageId, command, args);

            Assert.Equal(0, msg.TotalOrder);
            Assert.Equal(ProcessType.Client, msg.Type);
            Assert.Equal(0, msg.ProcessId);
            Assert.Equal(0, msg.MessageId);
            Assert.Equal(Command.Send, msg.Command);
            Assert.Equal("Testing", msg.Args[0]);
        }
    }
}
