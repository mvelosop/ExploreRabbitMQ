using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace GenericHostSample
{
    public static class Consumer
    {
        public static async Task ReceiveMessageAsync(ILogger logger, IApplicationLifetime appLifetime, string consumer, IModel channel, object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);

            try
            {
                logger.LogInformation("----- Received @#{Consumer} \"{message}\"", consumer, message);

                if (message.Equals("shutdown", StringComparison.InvariantCultureIgnoreCase))
                {
                    appLifetime.StopApplication();
                }
                else if (message.Contains("throw", StringComparison.InvariantCultureIgnoreCase) ||
                    message.Contains("exception", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException($"Exception requested ({message})");
                }
                else if (message.EndsWith("."))
                {
                    var sw = new Stopwatch();
                    var delay = 0;

                    while (delay++ < message.Length - 1 && message[message.Length - 1 - delay] == '.') ;

                    logger.LogWarning("----- \"Processing\" message \"{Message}\" @#{Consumer} will take {Delay}ms", message, consumer, delay * 100);

                    sw.Start();
                    while (delay-- > 0)
                    {
                        await Task.Delay(100);
                    }

                    logger.LogInformation("----- Processing message: {Message} @#{Consumer} - DONE! ({Delay:n0})", message, consumer, sw.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\" @#{Consumer}", message, consumer);
            }

            channel.BasicAck(ea.DeliveryTag, multiple: false);
        }
    }
}