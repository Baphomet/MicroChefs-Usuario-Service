using ClienteService.Context;
using ClienteService.DTOs;
using ClienteService.Models;
using Microsoft.EntityFrameworkCore;

namespace ClienteService.Services
{
    public class ClienteService
    {
        private readonly Db _context;

        public ClienteService(Db context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> GetAllClientes()
        {
            return await _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.Enderecos)
                .ToListAsync();
        }

        public async Task<Cliente> GetClienteById(Guid Id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.Enderecos)
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (cliente == null)
                throw new KeyNotFoundException("Cliente não encontrado.");

            return cliente;
        }

        public async Task AddCliente(ClienteDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new ArgumentException("O nome do cliente é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("O email do cliente é obrigatório.");

            var cliente = new Cliente
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Cpf = dto.Cpf,
                DataNascimento = dto.DataNascimento,
                DataCadastro = dto.DataCadastro,
                UsuarioId = dto.UsuarioId
            };

            try
            {
                await _context.Clientes.AddAsync(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new DbUpdateException("Erro ao inserir cliente no banco de dados.");
            }
        }

        public async Task UpdateCliente(Guid Id, ClienteDTO dto)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == Id);

            if (cliente == null)
                throw new KeyNotFoundException("Cliente não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new ArgumentException("O nome do cliente é obrigatório.");

            cliente.Nome = dto.Nome;
            cliente.Email = dto.Email;
            cliente.Telefone = dto.Telefone;
            cliente.Cpf = dto.Cpf;
            cliente.DataNascimento = dto.DataNascimento;

            try
            {
                _context.Clientes.Update(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new DbUpdateException("Erro ao atualizar cliente no banco de dados.");
            }
        }

        public async Task DeleteCliente(Guid Id)
        {
            var cliente = await _context.Clientes.FindAsync(Id);

            if (cliente == null)
                throw new KeyNotFoundException("Cliente não encontrado.");

            try
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Erro de integridade ao tentar deletar o cliente.");
            }
        }
    }
}