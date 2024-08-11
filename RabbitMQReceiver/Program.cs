using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class Program
{
    private const string HostName = "localhost";
    private const string QueueName = "hello";
    private const string DeadLetterExchangeName = "dead-letter-exchange";

    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = HostName };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declare the DLX
            channel.ExchangeDeclare(DeadLetterExchangeName, ExchangeType.Fanout);

            // Declare the main queue with DLX/DLQ arguments
            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DeadLetterExchangeName },
                { "x-dead-letter-routing-key", "" }
            };

            channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: queueArgs);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);

                try
                {
                    // Process the message (simulated by a simple console write here)
                    //int.Parse(message);
                    Console.WriteLine($" [x] Received '{message}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" [!] Error processing message '{message}': {ex.Message}");

                    // If processing fails, nack the message to move it to the DLQ
                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer); // autoAck: false to manually acknowledge messages

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
