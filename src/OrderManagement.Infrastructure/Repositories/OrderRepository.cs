using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.LineItems)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetByCustomerIdAsync(int customerId)
    {
        return await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.LineItems)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdempotencyKeyAsync(string key)
    {
        return await _db.Orders.FirstOrDefaultAsync(o => o.IdempotencyKey == key);
    }

    // INNER JOIN: orders filtered by status, joined to customer details
    public async Task<List<Order>> GetByStatusAsync(OrderStatus status)
    {
        return await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.LineItems)
            .Where(o => o.Status == status)
            .ToListAsync();
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order != null)
        {
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
        }
    }
}
