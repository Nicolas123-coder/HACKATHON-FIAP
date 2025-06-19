using System;
using System.Collections.Generic;
using MenuItems.DTOs;

public class CreateMenuItemValidator
{
    public static List<string> Validate(List<CreateMenuItemRequestDTO> dtos)
    {
        var errors = new List<string>();

        if (dtos == null || dtos.Count == 0)
        {
            errors.Add("É necessário enviar pelo menos um item no array.");
            return errors;
        }

        for (int i = 0; i < dtos.Count; i++)
        {
            var dto = dtos[i];
            var prefix = $"Item[{i}]";

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors.Add($"{prefix}: Name é obrigatório.");
                
            if (string.IsNullOrWhiteSpace(dto.Description))
                errors.Add($"{prefix}: Description é obrigatória.");
                
            if (dto.Price <= 0)
                errors.Add($"{prefix}: Price deve ser maior que zero.");
        }

        return errors;
    }
}