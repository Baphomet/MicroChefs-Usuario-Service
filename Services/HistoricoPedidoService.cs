using ClienteService.Context;
using ClienteService.Models;
using Microsoft.EntityFrameworkCore;

namespace ClienteService.Services
{
    public class HistoricoPedidoService
    {
        private readonly Db _db;

        public HistoricoPedidoService(Db db)
        {
            _db = db;
        }

        public async Task SalvarAsync(long pedidoId, long usuarioId, string status, CancellationToken cancellationToken = default)
        {
            if (pedidoId <= 0)
                throw new ArgumentException("PedidoId inválido");

            if (usuarioId == 0)
                throw new ArgumentException("UsuarioId inválido");

            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status inválido");

            var jaProcessado = await _db.HistoricoPedidos
                .AnyAsync(h => h.PedidoId == pedidoId && h.Status == status, cancellationToken);

            if (jaProcessado)
            {
                Console.WriteLine($"O status '{status}' do pedido {pedidoId} já foi processado anteriormente. Ignorando duplicidade.");
                return;
            }

            var historico = new HistoricoPedido
            {
                PedidoId = pedidoId,
                UsuarioId = usuarioId,
                Status = status,
                DataAtualizacao = DateTime.UtcNow
            };

            await _db.HistoricoPedidos.AddAsync(historico, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}