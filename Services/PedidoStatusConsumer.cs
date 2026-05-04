using ClienteService.DTOs.Eventos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        public PedidoStatusConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var uri = _configuration["RabbitMQ:Uri"];

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(uri)
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("pedido-exchange", ExchangeType.Direct, true);
            channel.ExchangeDeclare("pedido-dlx", ExchangeType.Direct, true);

            channel.QueueDeclare("cliente-dlq", true, false, false);
            channel.QueueBind("cliente-dlq", "pedido-dlx", "pedido-key.update");

            var args = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "pedido-dlx" }
            };

            channel.QueueDeclare("cliente-queue", true, false, false, args);
            channel.QueueBind("cliente-queue", "pedido-exchange", "pedido-key.update");

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var evento = JsonSerializer.Deserialize<PedidoStatusEvento>(json);

                    if (evento == null)
                        throw new Exception("Evento inválido");

                    if (evento.StatusPedido == "PRONTO")
                    {
                        using var scope = _serviceProvider.CreateScope();

                        var historicoService = scope.ServiceProvider
                            .GetRequiredService<HistoricoPedidoService>();

                        await historicoService.SalvarAsync(
                            evento.Id,
                            Guid.NewGuid(), // temp
                            evento.StatusPedido,
                            stoppingToken
                        );

                        Console.WriteLine($"Pedido {evento.Id} salvo no histórico");
                    }

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");

                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            channel.BasicConsume("cliente-queue", false, consumer);

            Console.WriteLine("Consumer rodando...");

            return Task.CompletedTask;
        }
    }
}