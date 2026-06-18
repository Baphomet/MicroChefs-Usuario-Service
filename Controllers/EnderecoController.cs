using ClienteService.DTOs;
using ClienteService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClienteService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EnderecoController : ControllerBase
    {
        private readonly EnderecoService _service;

        public EnderecoController(EnderecoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? string.Empty;
            if (role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                var enderecos = await _service.GetAllEnderecos();
                return Ok(enderecos);
            }

            var clienteId = ResolveClienteId();
            if (clienteId == null)
                return Ok(Array.Empty<object>());

            var doCliente = await _service.GetEnderecosByClienteId(clienteId.Value);
            return Ok(doCliente);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var endereco = await _service.GetEnderecoById(id);
                if (!CanAccessEndereco(endereco.ClienteId))
                    return Forbid();

                return Ok(endereco);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Erro ao buscar endereço.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] EnderecoDTO dto)
        {
            try
            {
                var clienteId = ResolveClienteId();
                if (clienteId == null || dto.ClienteId != clienteId)
                    return Forbid();

                await _service.AddEndereco(dto);
                return StatusCode(201, "Endereço criado com sucesso.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao acessar o banco de dados.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] EnderecoDTO dto)
        {
            try
            {
                var existing = await _service.GetEnderecoById(id);
                if (!CanAccessEndereco(existing.ClienteId))
                    return Forbid();

                if (ResolveClienteId() is long clienteId && dto.ClienteId != clienteId)
                    return Forbid();

                await _service.UpdateEndereco(id, dto);
                return Ok("Endereço atualizado com sucesso.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao atualizar o endereço.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var existing = await _service.GetEnderecoById(id);
                if (!CanAccessEndereco(existing.ClienteId))
                    return Forbid();

                await _service.DeleteEndereco(id);
                return Ok("Endereço deletado com sucesso.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Erro ao acessar o banco de dados.");
            }
        }

        private long? ResolveClienteId()
        {
            var claim = User.FindFirst("ClienteId")?.Value ?? User.FindFirst("clienteId")?.Value;
            if (long.TryParse(claim, out var id))
                return id;
            return null;
        }

        private bool CanAccessEndereco(long clienteId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? string.Empty;
            if (role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                return true;

            return ResolveClienteId() == clienteId;
        }
    }
}
