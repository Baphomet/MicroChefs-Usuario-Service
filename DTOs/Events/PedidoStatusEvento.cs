namespace ClienteService.DTOs.Eventos
{
    public class PedidoStatusEvento
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public string StatusPedido { get; set; }
    }
}