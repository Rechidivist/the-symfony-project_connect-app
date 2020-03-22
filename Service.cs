using System;
using System.Configuration;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SiteConnectorService
{
    class Service
    {
        private IConnection connection;
        private IModel channel;

        public Service()
        {
            string host;
            int port;
            try
            {
                host = ConfigurationManager.AppSettings["ServerHost"].ToString();
                port = int.Parse(ConfigurationManager.AppSettings["ServerPort"].ToString());
            }catch
            {
                throw new Exception("Неверный конфиг");
            }


            var factory = new ConnectionFactory() { HostName = host, Port = port };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(queue: "tcp_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: "udp_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            channel.ExchangeDeclare("tcp", "direct");

            channel.QueueBind("tcp_queue", "tcp", "");

            if (connection.IsOpen)
            {
                Console.WriteLine(string.Format("Connected to {0}:{1}",connection.Endpoint.HostName, connection.Endpoint.Port));
            }
        }

        public void Start()
        {
            var consumer_udp = new EventingBasicConsumer(channel);
            channel.BasicConsume(consumer_udp, "udp_queue", true);
            consumer_udp.Received += UDP_Consumer_Received;

            channel.BasicQos(0, 1, false);
            var consumer_tcp = new EventingBasicConsumer(channel);
            channel.BasicConsume(consumer_tcp,"tcp_queue", false);
            consumer_tcp.Received += TCP_Consumer_Received;
        }

        private void TCP_Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var props = e.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            SendWithReply(Encoding.UTF8.GetString(e.Body), e.DeliveryTag);
            string data = Recive(e.DeliveryTag);
            channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,basicProperties: replyProps, body: Encoding.UTF8.GetBytes(data));
            channel.BasicAck(deliveryTag: e.DeliveryTag,multiple: false);
        }

        private void UDP_Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            Send(Encoding.UTF8.GetString(e.Body));
        }

        private void Send(string message)
        {
            Console.WriteLine(message);
        }

        private void SendWithReply(string message, ulong dtag)
        {
            Send(message + dtag.ToString());
        }

        private string Recive(ulong dtag)
        {
            return "ok" + dtag.ToString();
        }

        public void Stop()
        {
            channel.Close(0,"ServiceStoped");
            connection.Close(0, "ServiceStoped");
        }
    }
}
