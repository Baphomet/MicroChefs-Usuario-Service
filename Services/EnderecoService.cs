using ClienteService.Context;
using ClienteService.DTOs;
using ClienteService.Models;
using Microsoft.EntityFrameworkCore;

namespace ClienteService.Services
{
    public class EnderecoService
    {
        private readonly Db _context;

        public EnderecoService(Db context)
        {
            _context = context;
        }

        public async Task<List<Endereco>> GetAllEnderecos()
        {
            return await _context.Enderecos
                .Include(e => e.Cliente)
                .ToListAsync();
        }

        public async Task<List<Endereco>> GetEnderecosByClienteId(long clienteId)
        {
            return await _context.Enderecos
                .Where(e => e.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<Endereco> GetEnderecoById(long id)
        {
            var endereco = await _context.Enderecos
                .Include(e => e.Cliente)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (endereco == null)
                throw new KeyNotFoundException("Endereço não encontrado.");

            return endereco;
        }

        public async Task AddEndereco(EnderecoDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Rua))
                throw new ArgumentException("A rua é obrigatória.");

            if (string.IsNullOrWhiteSpace(dto.Cep))
                throw new ArgumentException("O CEP é obrigatório.");

            var endereco = new Endereco
            {
                Rua = dto.Rua.Trim(),
                Numero = dto.Numero?.Trim() ?? string.Empty,
                Complemento = dto.Complemento?.Trim() ?? string.Empty,
                Bairro = dto.Bairro?.Trim() ?? string.Empty,
                Cidade = dto.Cidade?.Trim() ?? string.Empty,
                Estado = dto.Estado?.Trim() ?? string.Empty,
                Cep = dto.Cep.Trim(),
                ClienteId = dto.ClienteId
            };

            if (dto.Latitude.HasValue && dto.Longitude.HasValue && dto.Latitude != 0 && dto.Longitude != 0)
            {
                endereco.Latitude = dto.Latitude;
                endereco.Longitude = dto.Longitude;
            }
            else
            {
                var (lat, lng) = await GeocodeAddressAsync($"{endereco.Rua}, {endereco.Numero}, {endereco.Bairro}, {endereco.Cidade}, {endereco.Estado}, {endereco.Cep}");
                if (lat == 0 && lng == 0)
                    throw new ArgumentException("Não foi possível geocodificar o endereço. Verifique os dados informados.");
                endereco.Latitude = lat;
                endereco.Longitude = lng;
            }

            try
            {
                await _context.Enderecos.AddAsync(endereco);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new DbUpdateException("Erro ao inserir endereço no banco de dados.");
            }
        }

        public async Task UpdateEndereco(long id, EnderecoDTO dto)
        {
            var endereco = await _context.Enderecos.FirstOrDefaultAsync(e => e.Id == id);

            if (endereco == null)
                throw new KeyNotFoundException("Endereço não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Rua))
                throw new ArgumentException("A rua é obrigatória.");

            endereco.Rua = dto.Rua.Trim();
            endereco.Numero = dto.Numero?.Trim() ?? string.Empty;
            endereco.Complemento = dto.Complemento?.Trim() ?? string.Empty;
            endereco.Bairro = dto.Bairro?.Trim() ?? string.Empty;
            endereco.Cidade = dto.Cidade?.Trim() ?? string.Empty;
            endereco.Estado = dto.Estado?.Trim() ?? string.Empty;
            endereco.Cep = dto.Cep.Trim();

            if (dto.Latitude.HasValue && dto.Longitude.HasValue && dto.Latitude != 0 && dto.Longitude != 0)
            {
                endereco.Latitude = dto.Latitude;
                endereco.Longitude = dto.Longitude;
            }
            else
            {
                var (lat, lng) = await GeocodeAddressAsync($"{dto.Rua}, {dto.Numero}, {dto.Bairro}, {dto.Cidade}, {dto.Estado}, {dto.Cep}");
                if (lat == 0 && lng == 0)
                    throw new ArgumentException("Não foi possível geocodificar o endereço. Verifique os dados informados.");
                endereco.Latitude = lat;
                endereco.Longitude = lng;
            }

            try
            {
                _context.Enderecos.Update(endereco);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new DbUpdateException("Erro ao atualizar endereço no banco de dados.");
            }
        }

        public async Task DeleteEndereco(long id)
        {
            var endereco = await _context.Enderecos.FindAsync(id);

            if (endereco == null)
                throw new KeyNotFoundException("Endereço não encontrado.");

            try
            {
                _context.Enderecos.Remove(endereco);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Erro de integridade ao tentar deletar o endereço.");
            }
        }

        private async Task<(double lat, double lng)> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return (0, 0);

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                client.DefaultRequestHeaders.Add("User-Agent", "MicroChefs-App");

                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(content);
                    var array = doc.RootElement;

                    if (array.ValueKind == System.Text.Json.JsonValueKind.Array && array.GetArrayLength() > 0)
                    {
                        var first = array[0];
                        if (first.TryGetProperty("lat", out var latProp) && first.TryGetProperty("lon", out var lonProp))
                        {
                            if (double.TryParse(latProp.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                                double.TryParse(lonProp.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lng))
                            {
                                return (lat, lng);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao geocodificar endereço de cliente: {ex.Message}");
            }

            return (0, 0);
        }
    }
}