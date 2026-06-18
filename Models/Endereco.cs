using System.Text.Json.Serialization;

namespace ClienteService.Models
{
    public class Endereco
    {
        public long Id { get; set; }

        public string Rua { get; set; }

        public string Numero { get; set; }

        public string Complemento { get; set; }

        public string Bairro { get; set; }

        public string Cidade { get; set; }

        public string Estado { get; set; }

        public string Cep { get; set; }

        public long ClienteId { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [JsonIgnore]
        public Cliente Cliente { get; set; }
    }
}