using ClienteService.Context;
using ClienteService.DTOs;
using ClienteService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClienteService.Services
{
    public class AuthService
    {
        private readonly Db _context;
        private readonly IConfiguration _configuration;

        public AuthService(Db context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> Login(LoginDTO dto)
        {
            var email = NormalizeEmail(dto.Email);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(dto.Senha))
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

            var usuarios = await _context.Usuarios
                .Where(u => u.Email.ToLower() == email)
                .OrderByDescending(u => u.Id)
                .ToListAsync();

            if (usuarios.Count == 0)
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

            foreach (var usuario in usuarios)
            {
                if (VerifyPassword(dto.Senha, usuario.SenhaHash, usuario.SenhaSalt))
                    return GenerateToken(usuario);
            }

            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
        }

        private static string NormalizeEmail(string? email) =>
            email?.Trim().ToLowerInvariant() ?? string.Empty;

        private bool VerifyPassword(string senha, string hash, string salt)
        {
            var key = Convert.FromBase64String(salt);

            using var hmac = new System.Security.Cryptography.HMACSHA512(key);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(senha));

            return Convert.ToBase64String(computedHash) == hash;
        }

        private string GenerateToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role ?? "User")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}