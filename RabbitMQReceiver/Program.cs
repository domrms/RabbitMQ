using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Program
{
    private const string HostName = "localhost";
    private const string QueueName = "hello";

    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = HostName };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received '{message}'");
            };

            channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
