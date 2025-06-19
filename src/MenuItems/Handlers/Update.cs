// src/MenuItems/Handlers/UpdateMenu.cs
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
    public class UpdateMenu
    {
        private readonly DynamoDbHelper _dynamoDbHelper;
        private readonly string _tableName;

        public UpdateMenu(DynamoDbHelper dynamoDbHelper)
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
            context.Logger.LogLine("[UpdateMenu] Iniciando HandleAsync");
            context.Logger.LogLine($"Payload recebido: {request.Body}");

            List<UpdateMenuItemRequestDTO> dtos;
            try
            {
                dtos = JsonSerializer.Deserialize<List<UpdateMenuItemRequestDTO>>(request.Body)
                       ?? throw new JsonException("Body vazio");
            }
            catch (JsonException ex)
            {
                context.Logger.LogLine($"Erro de desserialização JSON: {ex.Message}");
                return Response.BadRequest("Formato de JSON inválido.");
            }

            var errors = UpdateMenuItemValidator.Validate(dtos);
            if (errors.Count > 0)
            {
                context.Logger.LogLine($"Validação falhou: {string.Join("; ", errors)}");
                return Response.BadRequest(string.Join(" ", errors));
            }
            
            var updatedItems = new List<MenuItem>();
            foreach (var dto in dtos)
            {
                // Carrega o documento existente
                Document? existing;
                try
                {
                    existing = await _dynamoDbHelper.GetDocumentAsync(
                        _tableName,
                        new Primitive(dto.Id)
                    );
                }
                catch (Exception ex)
                {
                    context.Logger.LogLine($"Erro ao carregar item {dto.Id}: {ex.Message}");
                    return Response.InternalError($"Erro ao buscar item {dto.Id}.");
                }

                if (existing == null)
                    return Response.NotFound($"Item não encontrado: {dto.Id}");

                var item = new MenuItem
                {
                    Id          = dto.Id,
                    Name        = dto.Name,
                    Description = dto.Description,
                    Price       = dto.Price,
                    Type        = dto.Type,
                    IsAvailable = dto.IsAvailable,
                    CreatedAt   = DateTime.Parse(existing["CreatedAt"].AsString()),
                    UpdatedAt   = DateTime.UtcNow
                };
                
                var doc = Document.FromJson(JsonSerializer.Serialize(item));

                // Persiste
                try
                {
                    await _dynamoDbHelper.PutDocumentAsync(_tableName, doc);
                    context.Logger.LogLine($"MenuItem atualizado: {dto.Id}");
                }
                catch (Exception ex)
                {
                    context.Logger.LogLine($"Erro ao salvar item {dto.Id}: {ex.Message}");
                    return Response.InternalError($"Erro ao salvar item {dto.Id}.");
                }

                // Converte de volta para entidade e adiciona à lista de retorno
                var updated = JsonSerializer.Deserialize<MenuItem>(doc.ToJson());
                if (updated != null)
                    updatedItems.Add(updated);
            }

            return Response.Ok(new
            {
                message = "Itens de cardápio atualizados com sucesso.",
                items   = updatedItems
            });
        }
    }
}
