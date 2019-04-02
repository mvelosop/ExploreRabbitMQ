using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenericHostSample
{
    internal class MessageConsumer2Service : IHostedService
    {
        private readonly IApplicationLifetime _appLifetime;
        private readonly IModel _channel;
        private readonly int _consumer;
        private readonly ILogger<MessageConsumer2Service> _logger;

        public MessageConsumer2Service(
            ILogger<MessageConsumer2Service> logger,
            IApplicationLifetime appLifetime,
            IModel channel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));

            _consumer = 2;
            _logger.LogTrace("Creating consumer #{Consumer}", _consumer);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("----- Starting up consumer #{Consumer}", _consumer);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += Consumer_Received;

            _channel.BasicConsume(
                queue: "hello",
                autoAck: false,
                consumer: consumer);

            _channel.CallbackException += Channel_CallbackException;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("----- Shutting down consumer #{Consumer}", _consumer);

            _channel.Close();

            return Task.CompletedTask;
        }

        private void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            Debugger.Break();

            throw new NotImplementedException();
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);

            try
            {
                _logger.LogInformation("----- Received @#{Consumer} \"{message}\"", _consumer, message);

                if (message.Equals("shutdown", StringComparison.InvariantCultureIgnoreCase))
                {
                    _appLifetime.StopApplication();
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

                    _logger.LogWarning("----- \"Processing\" message \"{Message}\" @#{Consumer} will take {Delay}ms", message, _consumer, delay * 100);

                    sw.Start();
                    while (delay-- > 0)
                    {
                        await Task.Delay(100);
                    }

                    _logger.LogInformation("----- Processing message: {Message} @#{Consumer} - DONE! ({Delay:n0})", message, _consumer, sw.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\" @#{Consumer}", message, _consumer);
            }

            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        }
    }
}