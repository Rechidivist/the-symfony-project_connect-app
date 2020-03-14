using System;
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
            var factory = new ConnectionFactory() { HostName = "localhost", Port=5627};
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(consumer, "controllerQueue",true);
            consumer.Received += Consumer_Received;

        }

        public void Start()
        { 
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            Send(Encoding.UTF8.GetString(e.Body));
        }

        private void Send(string message)
        {
            Console.WriteLine(message);
        }

        public void Stop()
        {
            channel.Close(0,"ServiceStoped");
            connection.Close(0, "ServiceStoped");
        }
    }
}
