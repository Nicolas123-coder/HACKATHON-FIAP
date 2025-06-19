using Utils.Entities;

namespace Clients.DTOs;

public class CreateOrderDTO
{
    public string ClientId        { get; set; } = null!; 
    public List<Order> Items        { get; set; } = new();
    public string             DeliveryType { get; set; } = null!; 
}