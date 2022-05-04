using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace apply_neural_network.RabbitMq
{
    public class RabbitMqService : IRabbitMqService
    {

        public void SendMessage(string message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var ind = message.IndexOf('#');
            Console.WriteLine(message);
            var taskId = message.Substring(0, ind);
            Console.WriteLine(taskId);
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "MyQueue",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                var byteMessage = Encoding.UTF8.GetBytes(message);
                
                Console.WriteLine(message);
                channel.BasicPublish(exchange: "",
                            routingKey: "MyQueue",
                            basicProperties: null,
                            body: byteMessage);
            }
        }
    }
}