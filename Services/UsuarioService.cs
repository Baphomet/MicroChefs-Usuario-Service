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

        public async Task<Usuario> GetUsuarioById(long id)
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

            var email = NormalizeEmail(dto.Email);
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Senha))
                throw new ArgumentException("Senha é obrigatória.");

            var emailEmUso = await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == email);
            if (emailEmUso)
                throw new InvalidOperationException("Este email já está cadastrado. Faça login.");

            CreatePasswordHash(dto.Senha, out string hash, out string salt);

            var usuario = new Usuario
            {
                Username = dto.Username.Trim(),
                Email = email,
                SenhaHash = hash,
                SenhaSalt = salt,
                Role = dto.Role
            };

            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateUsuario(long id, UsuarioDTO dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new ArgumentException("Username é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email é obrigatório.");

            usuario.Username = dto.Username;
            usuario.Email = dto.Email;
            usuario.Role = dto.Role;

            if (!string.IsNullOrWhiteSpace(dto.Senha))
            {
                CreatePasswordHash(dto.Senha, out string hash, out string salt);
                usuario.SenhaHash = hash;
                usuario.SenhaSalt = salt;
            }

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUsuario(long id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        private static string NormalizeEmail(string? email) =>
            email?.Trim().ToLowerInvariant() ?? string.Empty;

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