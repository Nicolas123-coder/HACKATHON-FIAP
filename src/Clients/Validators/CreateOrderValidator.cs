using System.Collections.Generic;
using Clients.DTOs;

namespace Clients.Validators
{
    public static class CreateOrderValidator
    {
        public static List<string> Validate(CreateOrderDTO dto)
        {
            var errors = new List<string>();

            if (dto == null)
            {
                errors.Add("Corpo da requisição inválido (dto nulo).");
                return errors;
            }

            if (string.IsNullOrEmpty(dto.ClientId))
                errors.Add("ClientId é obrigatório.");

            if (dto.Items == null || dto.Items.Count == 0)
                errors.Add("É necessário enviar ao menos um item no pedido.");

            if (string.IsNullOrEmpty(dto.DeliveryType))
                errors.Add("DeliveryType é obrigatório.");

            return errors;
        }
    }
}