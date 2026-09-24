using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _db.Customers.ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _db.Customers.FindAsync(id);
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _db.Customers.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer != null)
        {
            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
        }
    }

    // LEFT JOIN: includes customers with zero orders
    public async Task<List<CustomerReportDto>> GetCustomerReportAsync()
    {
        return await _db.Customers
            .GroupJoin(
                _db.Orders.Include(o => o.LineItems),
                customer => customer.Id,
                order => order.CustomerId,
                (customer, orders) => new { customer, orders })
            .SelectMany(
                x => x.orders.DefaultIfEmpty(),
                (x, order) => new { x.customer, order })
            .GroupBy(x => new { x.customer.Id, x.customer.Name, x.customer.Email })
            .Select(g => new CustomerReportDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Email = g.Key.Email,
                OrderCount = g.Count(x => x.order != null),
                TotalSpend = g.Sum(x => x.order != null
                    ? x.order.LineItems.Sum(li => li.Quantity * li.UnitPrice)
                    : 0)
            })
            .ToListAsync();
    }
}
