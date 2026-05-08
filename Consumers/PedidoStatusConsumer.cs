using ClienteService.DTOs.Events;
using ClienteService.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
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
        private readonly UserProducer _userProducer;

        private const string Exchange = "pedido-exchange";
        private const string DlqExchange = "dead-letter-exchange";
        private const string Queue = "user-queue";
        private const string RoutingKey = "pedido-key.status";

        public PedidoStatusConsumer(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            UserProducer userProducer)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _userProducer = userProducer;
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

            ConfigurarRabbitMq(channel);

            var consumer = new EventingBasicConsumer(channel);

            var retryPolicy = Policy
                .Handle<InfraException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"[RETRY] Tentativa {retryCount} falhou por erro de infra: {exception.Message}. Tentando novamente em {timeSpan.TotalSeconds}s...");
                    });

            consumer.Received += async (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                try
                {
                    Console.WriteLine($"[RABBIT] Recebido: {json}");

                    var evento = ConverterMensagem(json);

                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        await ProcessarEventoAsync(evento, stoppingToken);
                    });

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (DataException ex)
                {
                    _userProducer.EnviarParaDlq(channel, Queue, ex, json, "DATA_ERROR");

                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
                catch (InfraException ex)
                {
                    _userProducer.EnviarParaDlq(channel, Queue, ex, json, "INFRA_ERROR");

                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
                catch (Exception ex)
                {
                    _userProducer.EnviarParaDlq(channel, Queue, ex, json, "INFRA_ERROR");

                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            channel.BasicConsume(
                queue: Queue,
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine("Consumer rodando...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private void ConfigurarRabbitMq(IModel channel)
        {
            channel.ExchangeDeclare(Exchange, ExchangeType.Topic, true);

            channel.ExchangeDeclare(DlqExchange, ExchangeType.Topic, true);

            channel.QueueDeclare("dead-letter-queue", true, false, false);

            channel.QueueBind(
                "dead-letter-queue",
                "dead-letter-exchange",
                "dead-message"
            );

            channel.QueueDeclare(
                Queue,
                true,
                false,
                false,
                null
            );

            channel.QueueBind(
                Queue,
                Exchange,
                RoutingKey
            );
        }

        private PedidoStatusEvento ConverterMensagem(string json)
        {
            try
            {
                var evento = JsonSerializer.Deserialize<PedidoStatusEvento>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (evento == null)
                    throw new DataException("Evento inválido");

                return evento;
            }
            catch (JsonException)
            {
                throw new DataException("JSON inválido");
            }
        }

        private async Task ProcessarEventoAsync(
            PedidoStatusEvento evento,
            CancellationToken cancellationToken)
        {
            if (!evento.StatusPedido.Equals("PRONTO", StringComparison.OrdinalIgnoreCase))
                return;

            using var scope = _serviceProvider.CreateScope();

            var historicoService = scope.ServiceProvider
                .GetRequiredService<HistoricoPedidoService>();

            try
            {
                await historicoService.SalvarAsync(
                    evento.Id,
                    evento.UsuarioId,
                    evento.StatusPedido,
                    cancellationToken
                );

                Console.WriteLine($"[CONSUMER] Pedido {evento.Id} salvo");
            }
            catch (Exception ex)
            {
                throw new InfraException($"Erro ao salvar no banco: {ex.Message}");
            }
        }


    }
}