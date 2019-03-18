using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Receive
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello RabbitMQ - RECEIVE!");

            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: "hello",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine($"- Received \"{message}\"");

                    if (message.EndsWith("."))
                    {
                        var i = 0;

                        while (i++ < message.Length - 1 && message[message.Length - 1 - i] == '.') ;

                        while (i > 0)
                        {
                            Console.WriteLine($"  - {message} - {i--}s...");
                            await Task.Delay(1000);
                        }

                        Console.WriteLine("Done!");
                    }
                };

                channel.BasicConsume(
                    queue: "hello",
                    autoAck: true,
                    consumer: consumer);

                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
