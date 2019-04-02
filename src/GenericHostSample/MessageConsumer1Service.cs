using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GenericHostSample
{
    internal class MessageConsumer1Service : IHostedService
    {
        private readonly IApplicationLifetime _appLifetime;
        private readonly IModel _channel;
        private readonly string _consumer;
        private readonly ILogger<MessageConsumer1Service> _logger;

        public MessageConsumer1Service(
            ILogger<MessageConsumer1Service> logger,
            IApplicationLifetime appLifetime,
            IModel channel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));

            _consumer = "1";
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
            await Consumer.ReceiveMessageAsync(_logger, _appLifetime, _consumer, _channel, sender, ea);
        }
    }
}