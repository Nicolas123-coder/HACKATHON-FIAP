using System.Collections.Generic;
using Employees.Entities;

namespace Employees.Validators
{
    public static class CreateEmployeeValidator
    {
        public static List<string> Validate(Employee payload)
        {
            var errors = new List<string>();

            if (payload == null)
            {
                errors.Add("Payload é obrigatório.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(payload.Name))
                errors.Add("Name é obrigatório.");
            
            if (string.IsNullOrWhiteSpace(payload.Email))
                errors.Add("Email é obrigatório.");
            
            if (string.IsNullOrWhiteSpace(payload.Password))
                errors.Add("Password é obrigatório.");

            return errors;
        }
    }
}