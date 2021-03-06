using System;
using System.Threading.Tasks;
using Twino.Protocols.TMQ;

namespace Twino.Client.TMQ.Internal
{
    internal class QueueConsumerExecuter<TModel> : ConsumerExecuter
    {
        private readonly Type _consumerType;
        private readonly IQueueConsumer<TModel> _consumer;
        private readonly Func<IConsumerFactory> _consumerFactoryCreator;

        public QueueConsumerExecuter(Type consumerType, IQueueConsumer<TModel> consumer, Func<IConsumerFactory> consumerFactoryCreator)
        {
            _consumerType = consumerType;
            _consumer = consumer;
            _consumerFactoryCreator = consumerFactoryCreator;
            ResolveAttributes(consumerType, typeof(TModel));
        }

        public override async Task Execute(TmqClient client, TmqMessage message, object model)
        {
            TModel t = (TModel) model;
            Exception exception = null;
            IConsumerFactory consumerFactory = null;

            try
            {
                if (_consumer != null)
                    await _consumer.Consume(message, t, client);
                else if (_consumerFactoryCreator != null)
                {
                    consumerFactory = _consumerFactoryCreator();
                    object consumerObject = await consumerFactory.CreateConsumer(_consumerType);
                    IQueueConsumer<TModel> consumer = (IQueueConsumer<TModel>) consumerObject;
                    await consumer.Consume(message, t, client);
                }
                else
                    throw new ArgumentNullException("There is no consumer defined");

                if (SendAck)
                    await client.SendAck(message);
            }
            catch (Exception e)
            {
                if (SendNack)
                    await client.SendNegativeAck(message, TmqHeaders.NACK_REASON_ERROR);

                Type exceptionType = e.GetType();
                var kv = PushExceptions.ContainsKey(exceptionType)
                             ? PushExceptions[exceptionType]
                             : DefaultPushException;

                if (!string.IsNullOrEmpty(kv.Key))
                {
                    string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(e);
                    await client.Queues.Push(kv.Key, kv.Value, serialized, false);
                }

                exception = e;
                throw;
            }
            finally
            {
                if (consumerFactory != null)
                    consumerFactory.Consumed(exception);
            }
        }
    }
}