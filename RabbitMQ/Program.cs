using RabbitMQ.Client;

namespace RabbitMQSender
{
    public class Program
    {
        private const string HostName = "localhost";
        private const string QueueName = "hello";
        private const string Message = "Hello, RabbitMQ!";
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = HostName };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var body = System.Text.Encoding.UTF8.GetBytes(Message);

                channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: null, body: body);
                Console.WriteLine($" [x] Sent '{Message}'");
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

    }
}