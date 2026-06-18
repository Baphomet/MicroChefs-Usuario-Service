using ClienteService.Models;
using ClienteService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClienteService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsService _service;

        public SettingsController(SettingsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var settings = await _service.GetSettingsAsync();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao recuperar configurações: {ex.Message}");
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SystemSettings dto)
        {
            try
            {
                var settings = await _service.UpdateSettingsAsync(dto);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar configurações: {ex.Message}");
            }
        }
    }
}
