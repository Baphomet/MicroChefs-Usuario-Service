using ClienteService.DTOs;
using ClienteService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClienteService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuarioController(UsuarioService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _service.GetAllUsuarios();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var usuario = await _service.GetUsuarioById(id);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Erro ao buscar usuário.");
            }
        }
<<<<<<< HEAD

=======
        [AllowAnonymous]
>>>>>>> 1b2f2d5 (feat: adiciona o AuthService no Program)
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] UsuarioDTO dto)
        {
            try
            {
                await _service.AddUsuario(dto);
                return StatusCode(201, "Usuário criado com sucesso.");
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UsuarioDTO dto)
        {
            try
            {
                await _service.UpdateUsuario(id, dto);
                return Ok("Usuário atualizado com sucesso.");
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
                return StatusCode(500, "Erro ao atualizar o usuário.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteUsuario(id);
                return Ok("Usuário deletado com sucesso.");
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
    }
}