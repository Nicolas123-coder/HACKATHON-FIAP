namespace Orders.DTOs;

public class OrderMessageDTO
{
    public string           OrderId      { get; set; } = null!;
    public string           ClientId     { get; set; } = null!;
    public string           DeliveryType { get; set; } = null!;
    public DateTime         CreatedAt    { get; set; }
    public List<ItemMessageDTO> Items       { get; set; } = new();
}