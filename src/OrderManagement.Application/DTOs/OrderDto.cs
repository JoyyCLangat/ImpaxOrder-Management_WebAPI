using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<LineItemDto> LineItems { get; set; } = new();
    public decimal TotalValue { get; set; }
}

public class CreateOrderDto
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public List<CreateLineItemDto> LineItems { get; set; } = new();
}

public class LineItemDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateLineItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
