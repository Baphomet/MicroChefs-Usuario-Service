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

        public async Task<Endereco> GetEnderecoById(Guid id)
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
                Rua = dto.Rua,
                Numero = dto.Numero,
                Complemento = dto.Complemento,
                Bairro = dto.Bairro,
                Cidade = dto.Cidade,
                Estado = dto.Estado,
                Cep = dto.Cep,
                ClienteId = dto.ClienteId
            };

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

        public async Task UpdateEndereco(Guid id, EnderecoDTO dto)
        {
            var endereco = await _context.Enderecos.FirstOrDefaultAsync(e => e.Id == id);

            if (endereco == null)
                throw new KeyNotFoundException("Endereço não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Rua))
                throw new ArgumentException("A rua é obrigatória.");

            endereco.Rua = dto.Rua;
            endereco.Numero = dto.Numero;
            endereco.Complemento = dto.Complemento;
            endereco.Bairro = dto.Bairro;
            endereco.Cidade = dto.Cidade;
            endereco.Estado = dto.Estado;
            endereco.Cep = dto.Cep;

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

        public async Task DeleteEndereco(Guid id)
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
    }
}