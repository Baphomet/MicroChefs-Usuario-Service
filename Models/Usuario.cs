using System.Text.Json.Serialization;

namespace ClienteService.Models
{
    public class Usuario
    {
        public long Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        [JsonIgnore]
        public string SenhaHash { get; set; }

        [JsonIgnore]
        public string SenhaSalt { get; set; }

        public string Role { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Cliente Cliente { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}