using RabbitMQ.Client;
using System;
using System.Text;

namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello RabbitMQ - SEND!");
            Console.WriteLine("Write \"exit\" to exit");

            var factory = new ConnectionFactory { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: "hello",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var message = "Startup message";

                do
                {
                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();

                    properties.Persistent = true;

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "hello",
                        basicProperties: properties,
                        body: body);

                    Console.WriteLine($"- Sent \"{message}\"");

                    message = Console.ReadLine();

                } while (!message.Equals("exit", StringComparison.InvariantCultureIgnoreCase));

            }
        }
    }
}
