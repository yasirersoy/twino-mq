using System;

namespace Twino.Protocols.TMQ
{
    /// <summary>
    /// Message content serializer implementation for TmqMessage objects
    /// </summary>
    public interface IMessageContentSerializer
    {
        /// <summary>
        /// Serializes message content and converts the result to byte array
        /// </summary>
        /// <param name="model">Model that will be serialized into the tmq message</param>
        /// <param name="message">Message</param>
        void Serialize(TmqMessage message, object model);

        /// <summary>
        /// Deserializes message content and returns the object
        /// </summary>
        /// <param name="type">Model type</param>
        /// <param name="message">Message</param>
        object Deserialize(TmqMessage message, Type type);
    }
}