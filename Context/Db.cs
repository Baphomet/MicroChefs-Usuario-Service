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

    }
}
