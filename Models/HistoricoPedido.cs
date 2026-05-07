namespace ClienteService.Models
{
    public class HistoricoPedido
    {
        public long Id { get; set; }

        public long PedidoId { get; set; }

        public long UsuarioId { get; set; }

        public string Status { get; set; }

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    }
}