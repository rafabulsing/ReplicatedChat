using System;

namespace Chat.Core
{
    public class Message
    {
        public int TotalOrder   { get; private set; }
        public ProcessType Type { get; private set; }
        public int ProcessId    { get; private set; }
        public int MessageId    { get; private set; }
        public Command Command  { get; private set; }
        public string[] Args    { get; private set; }

        public Message(string messageText)
        {
            var parts = messageText.Split("|",StringSplitOptions.None);

            TotalOrder = Int32.Parse(parts[0]);
            Type       = ParseProcessType(parts[1]);
            ProcessId  = Int32.Parse(parts[2]);
            MessageId  = Int32.Parse(parts[3]);
            Command    = ParseCommand(parts[4]);
            Args       = GetArgs(parts);
        }

        private ProcessType ParseProcessType(string str)
        {
            str = str.ToLower();
            switch(str)
            {
                case "client":
                    return ProcessType.Client;
                
                case "replica":
                    return ProcessType.Replica;
                
                case "sequencer":
                    return ProcessType.Sequencer;
                
                default:
                    throw ArgumentException(String.Format("{0} is not a valid ProcessType.", str));
            }
        }
    
        private CommandType ParseCommand(string str)
        {
            str = str.ToLower();
            switch(str)
            {
                case "send":
                    return Command.Send;
                
                case "connect":
                    return Command.Connect;
                
                case "disconnect":
                    return Command.Disconnect;
                
                case "catchup":
                    return Command.CatchUp;

                default:
                    throw ArgumentException(String.Format("{0} is not a valid Command.", str));
            }
        }

        private string[] GetArgs(string[] parts)
        {
            int length = parts.Length-6;
            var args = new string[lenght];
            Array.Copy(parts, 5, args, 0, length);
            return args;
        }
    }
}
