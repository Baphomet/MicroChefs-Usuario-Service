using ClienteService.DTOs.Eventos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ClienteService.Services
{
    public class PedidoStatusConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public PedidoStatusConsumer(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var uri = _configuration["RabbitMQ:Uri"];

            var factory = new ConnectionFactory
            {
                Uri = new Uri(uri)
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: "pedido-exchange",
                type: ExchangeType.Topic,
                durable: true
            );

            channel.ExchangeDeclare(
                exchange: "pedido-dlx",
                type: ExchangeType.Direct,
                durable: true
            );

            channel.QueueDeclare(
                queue: "cliente-dlq",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            channel.QueueBind(
                queue: "cliente-dlq",
                exchange: "pedido-dlx",
                routingKey: "pedido.key-updates"
            );

            var args = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "pedido-dlx" }
            };

            channel.QueueDeclare(
                queue: "service-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: args
            );

            channel.QueueBind(
                queue: "service-queue",
                exchange: "pedido-exchange",
                routingKey: "pedido.key-updates"
            );

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    Console.WriteLine($"[RABBIT] Recebido: {json}");

                    var evento = JsonSerializer.Deserialize<PedidoStatusEvento>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (evento == null)
                        throw new Exception("Evento inválido");

                    if (evento.StatusPedido?.Equals("PRONTO", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        using var scope = _serviceProvider.CreateScope();

                        var historicoService = scope.ServiceProvider
                            .GetRequiredService<HistoricoPedidoService>();


                        await historicoService.SalvarAsync(
                            evento.Id,
                            evento.UsuarioId,
                            evento.StatusPedido,
                            stoppingToken
                        );

                        Console.WriteLine($"[CONSUMER] Pedido {evento.Id} salvo no histórico");
                    }

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO] {ex}");

                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            channel.BasicConsume(
                queue: "service-queue",
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine("Consumer rodando...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}