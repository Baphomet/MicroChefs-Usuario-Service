namespace ClienteService.DTOs
{
    public class ClienteDTO
    {
        public string Nome { get; set; }

        public string Email { get; set; }

        public string Telefone { get; set; }

        public string Cpf { get; set; }

        public DateTime DataNascimento { get; set; }

        public DateTime DataCadastro { get; set; }

        public long UsuarioId { get; set; }
    }
}