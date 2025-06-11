// src/MenuItems/Handlers/UpdateMenu.cs
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2.DocumentModel;
using MenuItems.DTOs;
using MenuItems.Entities;
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
                if (dtos.Count == 0)
                    return Response.BadRequest("É necessário enviar pelo menos um item no array.");
            }
            catch (JsonException ex)
            {
                context.Logger.LogLine($"Erro de desserialização JSON: {ex.Message}");
                return Response.BadRequest("Formato de JSON inválido.");
            }

            var updatedItems = new List<MenuItem>();
            foreach (var dto in dtos)
            {
                // Validações básicas
                if (string.IsNullOrEmpty(dto.Id))
                    return Response.BadRequest("Id é obrigatório em cada item.");
                if (string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Description))
                    return Response.BadRequest("Name e Description são obrigatórios.");
                if (dto.Price <= 0)
                    return Response.BadRequest("Price deve ser maior que zero.");

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

                // Atualiza os campos e a data
                existing["Name"]        = dto.Name;
                existing["Description"] = dto.Description;
                existing["Price"]       = dto.Price;
                existing["IsAvailable"] = dto.IsAvailable;
                existing["UpdatedAt"]   = DateTime.UtcNow;

                // Persiste
                try
                {
                    await _dynamoDbHelper.PutDocumentAsync(_tableName, existing);
                    context.Logger.LogLine($"MenuItem atualizado: {dto.Id}");
                }
                catch (Exception ex)
                {
                    context.Logger.LogLine($"Erro ao salvar item {dto.Id}: {ex.Message}");
                    return Response.InternalError($"Erro ao salvar item {dto.Id}.");
                }

                // Converte de volta para entidade e adiciona à lista de retorno
                var updated = JsonSerializer.Deserialize<MenuItem>(existing.ToJson());
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
