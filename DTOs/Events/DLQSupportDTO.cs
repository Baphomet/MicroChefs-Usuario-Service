using System.Text.Json.Serialization;

namespace ClienteService.DTOs.Events
{
    public record DLQSupportDTO
    (
        [property: JsonPropertyName("tipoMensagem")] string TipoMensagem,
        [property: JsonPropertyName("filaDeOrigem")] string FilaDeOrigem,
        [property: JsonPropertyName("tipoErro")] string TipoErro,
        [property: JsonPropertyName("mensagemDeErro")] string MensagemDeErro,
        [property: JsonPropertyName("mensagemOriginal")] string MensagemOriginal,
        [property: JsonPropertyName("timestamp")] DateTime TimeStamp
    );
}
