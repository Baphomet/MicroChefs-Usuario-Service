namespace ClienteService.Models
{
    public class SystemSettings
    {
        public long Id { get; set; }
        public string NomeRestaurante { get; set; } = "MicroChefs";
        public string Endereco { get; set; } = "Rua Antonio da Veiga, 300 - Blumenau, SC";
        public double Latitude { get; set; } = -26.9181631;
        public double Longitude { get; set; } = -49.076472;
        public string Telefone { get; set; } = "(47) 99999-9999";
        public string HorarioFuncionamento { get; set; } = "18:00 às 23:00";
        public string Logo { get; set; } = "";
    }
}
