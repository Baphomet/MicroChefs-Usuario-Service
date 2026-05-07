namespace ClienteService.DTOs.Events
{
    public record DLQSupportDTO
    (
        string TipoMensagem,
        string FilaDeOrigem,
        string TipoErro,
        string MensagemDeErro,
        string MensagemOriginal,
        DateTime TimeStamp
    );
}
