using System;

namespace Chat.Core
{
    public class Message
    {
        public int TotalOrder   { get; set; }
        public ProcessType Type { get; set; }
        public int ProcessId    { get; set; }
        public int MessageId    { get; set; }
        public Command Command  { get; set; }
        public string[] Args    { get; set; }

        private string OriginalText { get; set; }

        public Message(string messageText)
        {
            OriginalText = messageText;

            var separators = new char[]{'|'};
            var parts = messageText.Split(separators, StringSplitOptions.None);

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
                    throw new ArgumentException(String.Format("{0} is not a valid ProcessType.", str));
            }
        }
    
        private Command ParseCommand(string str)
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
                    throw new ArgumentException(String.Format("{0} is not a valid Command.", str));
            }
        }

        private string[] GetArgs(string[] parts)
        {
            int length = parts.Length-6;
            var args = new string[length];
            Array.Copy(parts, 5, args, 0, length);
            return args;
        }
    
        override public string ToString()
        {
            return OriginalText;
        }
    }
}
