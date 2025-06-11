namespace MenuItems.DTOs;

public class CreateMenuItemRequestDTO
{
    public string  Name        { get; set; } = string.Empty;
    public string  Description { get; set; } = string.Empty;
    public decimal Price       { get; set; }
    public bool    IsAvailable { get; set; }
}