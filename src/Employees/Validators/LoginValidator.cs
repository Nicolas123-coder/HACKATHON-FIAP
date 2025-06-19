using System.Collections.Generic;
using Employees.DTOs;

public class LoginValidator
{
    public static List<string> Validate(LoginRequestDTO dto)
    {
        var errors = new List<string>();

        if (dto == null)
        {
            errors.Add("Payload é obrigatório.");
            return errors;
        }
            
        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add("Email é obrigatório.");
            
        if (string.IsNullOrWhiteSpace(dto.Password))
            errors.Add("Password é obrigatório.");

        return errors;
    }
}