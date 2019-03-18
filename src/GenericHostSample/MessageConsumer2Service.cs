using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
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

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("----- Received @#{Consumer} \"{message}\"", _consumer, message);

                if (message.Equals("shutdown", StringComparison.InvariantCultureIgnoreCase))
                {
                    _appLifetime.StopApplication();
                }
            };

            _channel.BasicConsume(
                queue: "hello",
                autoAck: true,
                consumer: consumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("----- Shutting down consumer #{Consumer}", _consumer);

            _channel.Close();

            return Task.CompletedTask;
        }
    }
}