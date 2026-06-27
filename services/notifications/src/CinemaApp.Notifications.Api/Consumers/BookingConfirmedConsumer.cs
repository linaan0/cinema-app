using System.Text;
using System.Text.Json;
using CinemaApp.Notifications.Domain.Events;
using CinemaApp.Notifications.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace CinemaApp.Notifications.Api.Consumers;

// Background worker: subscribes to the "booking.confirmed" queue published
// by the Bookings service and turns each event into a stored + "sent" notification.
public class BookingConfirmedConsumer : BackgroundService
{
    private const string QueueName = "booking.confirmed";

    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingConfirmedConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public BookingConfirmedConsumer(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<BookingConfirmedConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"] ?? "rabbitmq",
            Port = int.TryParse(_configuration["RabbitMq:Port"], out var port) ? port : 5672,
            UserName = _configuration["RabbitMq:Username"] ?? "guest",
            Password = _configuration["RabbitMq:Password"] ?? "guest"
        };

        // RabbitMQ may still be starting up when this service does - retry for a while.
        for (var attempt = 1; attempt <= 12 && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                _connection = factory.CreateConnection();
                break;
            }
            catch (BrokerUnreachableException)
            {
                _logger.LogWarning("RabbitMQ not reachable yet (attempt {Attempt}/12), retrying in 5s...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        if (_connection is null)
        {
            _logger.LogError("Could not connect to RabbitMQ. The notification consumer will not run.");
            return;
        }

        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<BookingConfirmedEvent>(json);

                if (evt is not null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.HandleBookingConfirmedAsync(evt);
                }

                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process booking.confirmed message, requeueing.");
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("Listening for messages on queue '{Queue}'.", QueueName);

        // Keep the background service alive until cancellation.
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    public override void Dispose()
    {
        if (_channel?.IsOpen == true) _channel.Close();
        if (_connection?.IsOpen == true) _connection.Close();
        base.Dispose();
    }
}
