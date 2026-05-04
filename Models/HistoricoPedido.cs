namespace ClienteService.Models
{
    public class HistoricoPedido
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public long PedidoId { get; set; }

        public Guid UsuarioId { get; set; }

        public string Status { get; set; }

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    }
}