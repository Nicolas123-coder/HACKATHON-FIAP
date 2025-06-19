namespace MenuItems.DTOs;

public class UpdateMenuItemRequestDTO
{
    public string  Id          { get; set; } = string.Empty;
    public string  Name        { get; set; } = string.Empty;
    public string  Description { get; set; } = string.Empty;
    public string  Type { get; set; } = string.Empty;
    public decimal Price       { get; set; }
    public bool    IsAvailable { get; set; }
}