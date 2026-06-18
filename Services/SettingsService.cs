using ClienteService.Context;
using ClienteService.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClienteService.Services
{
    public class SettingsService
    {
        private readonly Db _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public SettingsService(Db context, IHttpClientFactory httpClientFactory = null)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SystemSettings> GetSettingsAsync()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSettings();
                await _context.Settings.AddAsync(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        public async Task<SystemSettings> UpdateSettingsAsync(SystemSettings dto)
        {
            var settings = await GetSettingsAsync();

            settings.NomeRestaurante = dto.NomeRestaurante?.Trim() ?? "MicroChefs";
            settings.Telefone = dto.Telefone?.Trim() ?? string.Empty;
            settings.HorarioFuncionamento = dto.HorarioFuncionamento?.Trim() ?? string.Empty;
            settings.Logo = dto.Logo?.Trim() ?? string.Empty;

            var oldAddress = settings.Endereco;
            settings.Endereco = dto.Endereco?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(oldAddress) || oldAddress != settings.Endereco || settings.Latitude == 0)
            {
                var (lat, lng) = await GeocodeAddressAsync(settings.Endereco);

                if (lat != 0 && lng != 0)
                {
                    settings.Latitude = lat;
                    settings.Longitude = lng;
                }
            }
            else
            {
                if (dto.Latitude != 0) settings.Latitude = dto.Latitude;
                if (dto.Longitude != 0) settings.Longitude = dto.Longitude;
            }

            _context.Settings.Update(settings);
            await _context.SaveChangesAsync();

            return settings;
        }

        public async Task<(double lat, double lng)> GeocodeAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return (0, 0);

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "MicroChefs-App");

                var url =
                    $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);

                    var array = doc.RootElement;

                    if (array.ValueKind == JsonValueKind.Array && array.GetArrayLength() > 0)
                    {
                        var first = array[0];

                        if (first.TryGetProperty("lat", out var latProp) &&
                            first.TryGetProperty("lon", out var lonProp))
                        {
                            if (double.TryParse(latProp.GetString(),
                                    System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out double lat) &&
                                double.TryParse(lonProp.GetString(),
                                    System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out double lng))
                            {
                                return (lat, lng);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao geocodificar endereço: {ex.Message}");
            }

            // fallback: coordenadas padrão
            var random = new Random();
            double defaultLat = -23.5505 + (random.NextDouble() - 0.5) * 0.01;
            double defaultLng = -46.6333 + (random.NextDouble() - 0.5) * 0.01;

            return (defaultLat, defaultLng);
        }
    }
}