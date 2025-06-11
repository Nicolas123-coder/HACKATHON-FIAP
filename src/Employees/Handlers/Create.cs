using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Employees.Entities;
using Utils;
using Amazon.DynamoDBv2;
using Employees.DTOs;

namespace Employees.Handlers
{
    public class Create
    {
        private readonly DynamoDbHelper _dynamoDbHelper;
        private readonly PasswordHasherService _passwordHasher;
        
        public Create(
            DynamoDbHelper dynamoDbHelper,
            PasswordHasherService passwordHasher)
        {
            _dynamoDbHelper = dynamoDbHelper;
            _passwordHasher = passwordHasher;
        }

        public async Task<APIGatewayProxyResponse> HandleAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            context.Logger.LogLine("[CreateEmployeeHandler] Iniciando HandleAsync");
            context.Logger.LogLine($"Payload recebido: {request.Body}");

            Employee payload;
            try
            {
                payload = JsonSerializer.Deserialize<Employee>(request.Body);

                if (payload == null
                    || string.IsNullOrEmpty(payload.Name)
                    || string.IsNullOrEmpty(payload.Email)
                    || string.IsNullOrEmpty(payload.Password))
                {
                    context.Logger.LogLine("Payload inválido: Nome, Email ou PasswordHash ausentes.");
                    return Response.BadRequest("Name, Email e PasswordHash são obrigatórios.");
                }
            }
            catch (JsonException ex)
            {
                context.Logger.LogLine($"Erro de desserialização JSON: {ex.Message}");
                return Response.BadRequest("Formato de JSON inválido.");
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Erro inesperado ao processar payload: {ex.Message}");
                return Response.InternalError("Ocorreu um erro interno ao processar a requisição.");
            }

            payload.Id        = Guid.NewGuid().ToString();
            payload.CreatedAt = DateTime.UtcNow;
            payload.UpdatedAt = payload.CreatedAt;
            payload.PasswordHash = _passwordHasher.HashPassword(null!, payload.Password);
            payload.Password     = null!; 

            try
            {
                context.Logger.LogLine($"Salvando employee no DynamoDB: {payload.Id}");
                
                await _dynamoDbHelper.SaveAsync(payload);
                context.Logger.LogLine("Employee salvo com sucesso no DynamoDB.");
            }
            catch (AmazonDynamoDBException ex)
            {
                context.Logger.LogLine($"Erro do DynamoDB: {ex.Message}");
                return Response.InternalError($"Erro ao salvar employee no DynamoDB: {ex.Message}");
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Erro inesperado ao salvar no DynamoDB: {ex.Message}");
                return Response.InternalError("Ocorreu um erro interno ao salvar o employee.");
            }
            
            var responseDto = new CreateEmployeeResponseDTO()
            {
                Id        = payload.Id,
                Name      = payload.Name,
                Email     = payload.Email,
                Role      =  payload.Role,
                CreatedAt = payload.CreatedAt,
                UpdatedAt = payload.UpdatedAt
            };

            return Response.Created(new
            {
                message  = "Employee criado com sucesso.",
                employee = responseDto
            });
        }
    }
}