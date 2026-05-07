namespace ClienteService.DTOs.Events;
using System.Text.Json.Serialization;

public class PedidoStatusEvento
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("usuarioId")]
    public long UsuarioId { get; set; }

    [JsonPropertyName("statusPedido")]
    public string StatusPedido { get; set; }
}