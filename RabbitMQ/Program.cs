using System;
using RabbitMQ.Client;

public class Program
{
    private const string HostName = "localhost";
    private const string QueueName = "hello";
    private const string DeadLetterExchangeName = "dead-letter-exchange";
    private const string DeadLetterQueueName = "dead-letter-queue";

    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = HostName };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declare the DLX and DLQ
            channel.ExchangeDeclare(DeadLetterExchangeName, ExchangeType.Fanout);
            channel.QueueDeclare(DeadLetterQueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(DeadLetterQueueName, DeadLetterExchangeName, "");

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.DeliveryMode = 2; // persistent

            // Declare the main queue with DLX/DLQ arguments
            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DeadLetterExchangeName },
                { "x-dead-letter-routing-key", "" }
            };

            channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: queueArgs);

            var message = "Hello, RabbitMQ!";
            var body = System.Text.Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: properties, body: body);
            Console.WriteLine($" [x] Sent '{message}'");
        }

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}
