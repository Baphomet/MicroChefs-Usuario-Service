using ClienteService.Context;
using ClienteService.DTOs;
using ClienteService.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ClienteService.Services
{
    public class UsuarioService
    {
        private readonly Db _context;

        public UsuarioService(Db context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> GetAllUsuarios()
        {
            return await _context.Usuarios
                .Include(u => u.Cliente)
                .ToListAsync();
        }

        public async Task<Usuario> GetUsuarioById(Guid id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Cliente)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            return usuario;
        }

        public async Task AddUsuario(UsuarioDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new ArgumentException("Username é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Senha))
                throw new ArgumentException("Senha é obrigatória.");

            CreatePasswordHash(dto.Senha, out string hash, out string salt);

            var usuario = new Usuario
            {
                Username = dto.Username,
                Email = dto.Email,
                SenhaHash = hash,
                SenhaSalt = salt,
                Role = dto.Role
            };

            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUsuario(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        private void CreatePasswordHash(string senha, out string hash, out string salt)
        {
            using var hmac = new HMACSHA512();

            salt = Convert.ToBase64String(hmac.Key);

            hash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(senha))
            );
        }
    }
}