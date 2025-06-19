using Clients.DTOs;

namespace Clients.Validators;

public class LoginValidator
{
    /// <summary>
    /// Valida o DTO de requisição de login e retorna lista de erros.
    /// </summary>
    /// <param name="dto">DTO de login.</param>
    /// <returns>Lista de mensagens de erro; vazia se válido.</returns>
    public static List<string> Validate(LoginRequestDTO dto)
    {
        var errors = new List<string>();

        if (dto == null)
        {
            errors.Add("Corpo da requisição inválido.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(dto.Login))
            errors.Add("Login (CPF ou Email) é obrigatório.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            errors.Add("Password é obrigatório.");

        return errors;
    }
}