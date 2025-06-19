using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2.DocumentModel;
using MenuItems.DTOs;
using Utils.Entities;
using Utils;

namespace MenuItems.Handlers
{
    public class CreateMenu
    {
        private readonly DynamoDbHelper _dynamoDbHelper;
        private readonly string _tableName;

        public CreateMenu(DynamoDbHelper dynamoDbHelper)
        {
            _dynamoDbHelper = dynamoDbHelper;
            _tableName = Environment
                .GetEnvironmentVariable("MENU_ITEMS_TABLE")
                ?? throw new InvalidOperationException("MENU_ITEMS_TABLE não definido");
        }

        public async Task<APIGatewayProxyResponse> HandleAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            context.Logger.LogLine("[CreateMenu] Iniciando HandleAsync");
            context.Logger.LogLine($"Payload recebido: {request.Body}");

            List<CreateMenuItemRequestDTO> dtos;
            try
            {
                dtos = JsonSerializer.Deserialize<List<CreateMenuItemRequestDTO>>(request.Body)
                       ?? throw new JsonException("Body vazio");
            }
            catch (JsonException ex)
            {
                context.Logger.LogLine($"Erro de desserialização JSON: {ex.Message}");
                return Response.BadRequest("Formato de JSON inválido.");
            }
            
            var errors = CreateMenuItemValidator.Validate(dtos);
            if (errors.Count > 0)
            {
                context.Logger.LogLine($"Validação falhou: {string.Join("; ", errors)}");
                return Response.BadRequest(string.Join(" ", errors));
            }

            var createdItems = new List<MenuItem>();
            foreach (var dto in dtos)
            {
                // Mapeia para a entidade
                var item = new MenuItem
                {
                    Id          = Guid.NewGuid().ToString(),
                    Name        = dto.Name,
                    Description = dto.Description,
                    Price       = dto.Price,
                    Type        = dto.Type, 
                    IsAvailable = dto.IsAvailable,
                    CreatedAt   = DateTime.UtcNow,
                    UpdatedAt   = DateTime.UtcNow
                };

                try
                {
                    // Persistência – especifica a tabela via nome
                    var doc = Document.FromJson(JsonSerializer.Serialize(item));
                    await _dynamoDbHelper.PutDocumentAsync(_tableName, doc);
                    context.Logger.LogLine($"MenuItem salvo: {item.Id}");
                    createdItems.Add(item);
                }
                catch (Exception ex)
                {
                    context.Logger.LogLine($"Erro ao salvar item {item.Id}: {ex.Message}");
                    return Response.InternalError($"Erro ao salvar item {item.Name}.");
                }
            }

            // Retorna 201 Created com todos os itens
            return Response.Created(new
            {
                message = "Itens de cardápio criados com sucesso.",
                items   = createdItems
            });
        }
    }
}
