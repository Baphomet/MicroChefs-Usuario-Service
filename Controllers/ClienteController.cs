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
    public class ClienteController : ControllerBase
    {
        private readonly ClienteService.Services.ClienteService _service;

        public ClienteController(ClienteService.Services.ClienteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? string.Empty;
            if (role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                var clientes = await _service.GetAllClientes();
                return Ok(clientes);
            }

            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (!int.TryParse(usuarioId, out var uid))
                return Ok(Array.Empty<object>());

            var cliente = await _service.GetClienteByUsuarioId(uid);
            return Ok(cliente != null ? new[] { cliente } : Array.Empty<object>());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var cliente = await _service.GetClienteById(id);
                if (!CanAccessCliente(cliente.UsuarioId))
                    return Forbid();

                return Ok(cliente);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Erro ao buscar cliente.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ClienteDTO dto)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
                if (int.TryParse(usuarioId, out var uid) && dto.UsuarioId != uid)
                    return Forbid();

                await _service.AddCliente(dto);
                return StatusCode(201, "Cliente criado com sucesso.");
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
        public async Task<IActionResult> Update(long id, [FromBody] ClienteDTO dto)
        {
            try
            {
                var cliente = await _service.GetClienteById(id);
                if (!CanAccessCliente(cliente.UsuarioId))
                    return Forbid();

                await _service.UpdateCliente(id, dto);
                return Ok("Cliente atualizado com sucesso.");
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
                return StatusCode(500, "Erro ao atualizar o cliente.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var cliente = await _service.GetClienteById(id);
                if (!CanAccessCliente(cliente.UsuarioId))
                    return Forbid();

                await _service.DeleteCliente(id);
                return Ok("Cliente deletado com sucesso.");
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

        private bool CanAccessCliente(long usuarioId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? string.Empty;
            if (role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                return true;

            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return int.TryParse(claim, out var uid) && uid == usuarioId;
        }
    }
}
