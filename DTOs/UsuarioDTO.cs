namespace ClienteService.DTOs
{
    public class UsuarioDTO
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string SenhaHash { get; set; }

        public string SenhaSalt { get; set; }

        public string Role { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
