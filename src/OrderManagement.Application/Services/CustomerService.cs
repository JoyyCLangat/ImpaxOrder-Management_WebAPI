using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        var customers = await _repo.GetAllAsync();
        return customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email
        }).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var c = await _repo.GetByIdAsync(id);
        if (c == null) return null;
        return new CustomerDto { Id = c.Id, Name = c.Name, Email = c.Email };
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var existing = await _repo.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException($"A customer with email '{dto.Email}' already exists.");

        var customer = new Customer { Name = dto.Name, Email = dto.Email };
        var created = await _repo.CreateAsync(customer);
        return new CustomerDto { Id = created.Id, Name = created.Name, Email = created.Email };
    }

    public async Task<List<CustomerReportDto>> GetReportAsync()
    {
        return await _repo.GetCustomerReportAsync();
    }
}
