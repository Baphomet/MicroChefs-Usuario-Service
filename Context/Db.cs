using ClienteService.Models;
using Microsoft.EntityFrameworkCore;

namespace ClienteService.Context
{
    public class Db : DbContext
    {
        public Db(DbContextOptions<Db> options) : base(options) { }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Endereco> Enderecos { get; set; }
        public DbSet<HistoricoPedido> HistoricoPedidos { get; set; }
        public DbSet<SystemSettings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(u => u.Email).HasMaxLength(256);
                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
