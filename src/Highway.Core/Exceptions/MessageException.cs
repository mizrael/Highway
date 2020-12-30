using System;

namespace Highway.Core.Exceptions
{
    public class MessageException : Exception
    {
        public IMessage Message { get; }
        public MessageException(IMessage message, string errorMessage) : base(errorMessage)
        {
            Message = message;
        }
    }
}