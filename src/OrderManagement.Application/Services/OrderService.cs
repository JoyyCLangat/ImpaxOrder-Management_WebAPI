using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ICustomerRepository _customerRepo;

    public OrderService(IOrderRepository orderRepo, ICustomerRepository customerRepo)
    {
        _orderRepo = orderRepo;
        _customerRepo = customerRepo;
    }

    public async Task<List<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepo.GetAllAsync();
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<List<OrderDto>> GetByCustomerIdAsync(int customerId)
    {
        var orders = await _orderRepo.GetByCustomerIdAsync(customerId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<List<OrderDto>> GetByStatusAsync(OrderStatus status)
    {
        var orders = await _orderRepo.GetByStatusAsync(status);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        var existing = await _orderRepo.GetByIdempotencyKeyAsync(dto.IdempotencyKey);
        if (existing != null)
            throw new InvalidOperationException($"An order with idempotency key '{dto.IdempotencyKey}' already exists.");

        var customer = await _customerRepo.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            throw new InvalidOperationException($"Customer with ID {dto.CustomerId} not found.");

        var order = new Order
        {
            IdempotencyKey = dto.IdempotencyKey,
            CustomerId = dto.CustomerId,
            LineItems = dto.LineItems.Select(li => new LineItem
            {
                ProductName = li.ProductName,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice
            }).ToList()
        };

        var created = await _orderRepo.CreateAsync(order);
        created.Customer = customer;
        return MapToDto(created);
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        IdempotencyKey = o.IdempotencyKey,
        OrderDate = o.OrderDate,
        Status = o.Status,
        CustomerId = o.CustomerId,
        CustomerName = o.Customer?.Name ?? "",
        LineItems = o.LineItems.Select(li => new LineItemDto
        {
            Id = li.Id,
            ProductName = li.ProductName,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice
        }).ToList(),
        TotalValue = o.LineItems.Sum(li => li.Quantity * li.UnitPrice)
    };
}
