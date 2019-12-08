using System;
using System.Threading;
using Sample.Mq.Models;
using Twino.Client.TMQ;
using Twino.Client.TMQ.Connectors;
using Twino.Core;
using Twino.Protocols.TMQ;

namespace Sample.Mq
{
    public class Producer
    {
        private Timer _timer;
        private readonly TmqAbsoluteConnector _connector;
        private bool _firstConnection = true;
        private int _eventCount = 1;

        public Producer()
        {
            _connector = new TmqAbsoluteConnector(TimeSpan.FromSeconds(5));
        }

        public void Start()
        {
            _connector.AddProperty(TmqHeaders.CLIENT_TYPE, "producer");
            _connector.AddProperty(TmqHeaders.CLIENT_TOKEN, "S3cr37_pr0duc3r_t0k3n");
            _connector.AddHost("tmq://localhost:48050");

            _connector.Connected += Connected;
            _connector.MessageReceived += MessageReceived;
            _connector.Run();

            _timer = new Timer(o =>
            {
                if (_connector.IsConnected)
                {
                    TmqClient client = _connector.GetClient();

                    ProducerEvent e = new ProducerEvent();
                    e.No = _eventCount;
                    e.Guid = Guid.NewGuid().ToString();
                    e.Name = "Producer Event";

                    Console.WriteLine($"Sending package #{e.No}");
                    _eventCount++;

                    client.PushJson("channel", ModelTypes.ProducerEvent, e, false);
                    client.PushJson("ack-channel", ModelTypes.ProducerEvent, e, false);
                }
            }, null, 6000, 6000);
        }

        private void Connected(SocketBase client)
        {
            TmqClient tmq = (TmqClient) client;
            if (_firstConnection)
            {
                _firstConnection = false;
                tmq.CreateQueue("channel", ModelTypes.ProducerEvent, false).Wait();
            }
        }

        private void MessageReceived(ClientSocketBase<TmqMessage> client, TmqMessage message)
        {
            if (message.Type == MessageType.Client && message.ContentType == ModelTypes.ConsumerRequest)
            {
                ConsumerRequest request = message.GetJsonContent<ConsumerRequest>().Result;
                Console.WriteLine($"Consumer request received: {request.Guid}");

                ProducerResponse model = new ProducerResponse();
                model.RequestGuid = request.Guid;
                model.ResponseGuid = Guid.NewGuid().ToString();

                TmqMessage response = message.CreateResponse();
                response.ContentType = ModelTypes.ProducerResponse;
                response.SetJsonContent(model);
                TmqClient tmq = (TmqClient) client;
                tmq.Send(response);
            }
        }
    }
}