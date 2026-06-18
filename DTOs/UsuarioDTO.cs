namespace ClienteService.DTOs
{
    public class UsuarioDTO
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Senha { get; set; }

        public string Role { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}
