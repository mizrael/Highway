using System;
using System.Collections.Generic;
using System.Text;
using Highway.Core;
using RabbitMQ.Client;

namespace Highway.Transport.RabbitMQ
{
    public class MessageResolver : IMessageResolver
    {
        private readonly IDecoder _decoder;
        private readonly IEnumerable<System.Reflection.Assembly> _assemblies;

        public MessageResolver(IDecoder encoder, IEnumerable<System.Reflection.Assembly> assemblies)
        {
            _decoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            _assemblies = assemblies ?? throw new ArgumentNullException(nameof(encoder));
        }

        public TM Resolve<TM>(IBasicProperties basicProperties, ReadOnlyMemory<byte> body) 
            where TM : IMessage
        {
            if (basicProperties is null)
                throw new ArgumentNullException(nameof(basicProperties));
            if (basicProperties.Headers is null)
                throw new ArgumentNullException(nameof(IBasicProperties.Headers), "message headers are missing");

            if (!basicProperties.Headers.TryGetValue(HeaderNames.MessageType, out var tmp) ||
                tmp is not byte[] messageTypeBytes ||
                messageTypeBytes is null)
                throw new ArgumentException("invalid message type");

            var messageTypeName = Encoding.UTF8.GetString(messageTypeBytes);

            Type dataType = null;
            foreach (var assembly in _assemblies)
                dataType = assembly.GetType(messageTypeName, throwOnError: false, ignoreCase: true);
            if (null == dataType)
                throw new TypeLoadException($"unable to resolve type '{messageTypeName}' ");

            var decodedObj = _decoder.Decode(body, dataType);
            if (decodedObj is not TM message)
                throw new ArgumentException($"type '{messageTypeName}' is not a valid message");
            return message;
        }
    }
}