using System.Text.Json.Serialization;

namespace ClienteService.Models
{
    public class Endereco
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Rua { get; set; }

        public string Numero { get; set; }

        public string Complemento { get; set; }

        public string Bairro { get; set; }

        public string Cidade { get; set; }

        public string Estado { get; set; }

        public string Cep { get; set; }

        public Guid ClienteId { get; set; }

        [JsonIgnore]
        public Cliente Cliente { get; set; }
    }
}