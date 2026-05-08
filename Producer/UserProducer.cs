using ClienteService.DTOs.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ClienteService.Services
{
    public class UserProducer
    {
        public void EnviarParaDlq(IModel channel, string filaOrigem, Exception ex, string json, string tipoErro)
        {
            var dlq = new DLQSupportDTO(
                TipoMensagem: "PEDIDO_STATUS_UPDATE",
                FilaDeOrigem: filaOrigem,
                TipoErro: tipoErro,
                MensagemDeErro: ex.Message,
                MensagemOriginal: json,
                TimeStamp: DateTime.UtcNow
            );

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(dlq, options)
            );

            channel.BasicPublish(
                exchange: "dead-letter-exchange",
                routingKey: "dead-message",
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"[DLQ] {JsonSerializer.Serialize(dlq, options)}");
        }
    }
}
