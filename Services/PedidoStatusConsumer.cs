using ClienteService.DTOs.Eventos;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ClienteService.Services
{
    public class PedidoStatusConsumer : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("RabbitMQ:Uri")
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare("pedido-exchange", ExchangeType.Direct, true);

            channel.ExchangeDeclare("pedido-dlx", ExchangeType.Direct, true);

            channel.QueueDeclare(
                queue: "cliente-dlq",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            channel.QueueBind(
                queue: "cliente-dlq",
                exchange: "pedido-dlx",
                routingKey: "pedido-key.update"
            );

            var args = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "pedido-dlx" }
            };

            channel.QueueDeclare(
                queue: "cliente-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: args
            );

            channel.QueueBind(
                queue: "cliente-queue",
                exchange: "pedido-exchange",
                routingKey: "pedido-key.update"
            );

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var evento = JsonSerializer.Deserialize<PedidoStatusEvento>(json);

                    if (evento.StatusPedido == "PRONTO")
                    {
                        Console.WriteLine($"Pedido {evento.Id} pronto para usuário {evento.UsuarioId}");

                    }

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");

                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            channel.BasicConsume(
                queue: "cliente-queue",
                autoAck: false,
                consumer: consumer
            );

            return Task.CompletedTask;
        }
    }
}