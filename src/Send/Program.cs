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
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var message = "Startup message";

                do
                {
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "hello",
                        basicProperties: null,
                        body: body);

                    Console.WriteLine($"- Sent \"{message}\"");

                    message = Console.ReadLine();

                } while (!message.Equals("exit", StringComparison.InvariantCultureIgnoreCase));

            }
        }
    }
}
