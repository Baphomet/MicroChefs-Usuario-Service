using System.Text.Json.Serialization;

namespace ClienteService.Models
{
    public class Cliente
    {
        public Guid id { get; set; } = Guid.NewGuid();

        public string Nome { get; set; }

        public string Email { get; set; }

        public string Telefone { get; set; }

        public string Cpf { get; set; }

        public DateTime DataNascimento { get; set; }

        public DateTime DataCadastro { get; set; }

        public Guid UsuarioId { get; set; }

        public Usuario Usuario { get; set; }

        [JsonIgnore]
        public ICollection<Endereco> Enderecos { get; set; }
    }
}
