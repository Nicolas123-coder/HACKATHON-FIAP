namespace Clients.DTOs;

public class LoginRequestDTO
{
    /// <summary>
    /// Podem vir Neste campo CPF (somente dígitos) ou Email
    /// </summary>
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}