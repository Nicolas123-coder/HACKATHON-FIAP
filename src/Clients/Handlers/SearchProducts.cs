using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Utils.Entities;
using Utils;
using Amazon.DynamoDBv2.DocumentModel;

namespace Clients.Handlers
{
    public class SearchMenuItems
    {
        private readonly DynamoDbHelper _db;

        public SearchMenuItems(DynamoDbHelper db)
        {
            _db = db;
        }

        public async Task<APIGatewayProxyResponse> HandleAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            context.Logger.LogLine("[SearchMenuItems] Iniciando HandleAsync");
            context.Logger.LogLine($"QueryStringParameters: {JsonSerializer.Serialize(request.QueryStringParameters)}");

            // Extrai filtro 'type' da query string
            var qs = request.QueryStringParameters ?? new Dictionary<string, string>();
            qs.TryGetValue("type", out var typeFilter);
            context.Logger.LogLine($"Filtro 'type' recebido: {typeFilter}");

            try
            {
                // Monta condições de scan: sempre IsAvailable = true
                var scanConditions = new List<ScanCondition>
                {
                    new ScanCondition("IsAvailable", ScanOperator.Equal, true)
                };

                if (!string.IsNullOrEmpty(typeFilter))
                {
                    scanConditions.Add(new ScanCondition("Type", ScanOperator.Equal, typeFilter));
                }

                // Executa o Scan usando o helper
                var items = await _db.ScanAsync<MenuItem>(scanConditions);

                // Apenas para garantir: filtra nulos (caso haja) e ordena por Name
                var result = items?
                    .Where(item => item != null)
                    .OrderBy(item => item.Name)
                    .ToList()
                    ?? new List<MenuItem>();

                // Retorna 200 OK
                return Response.Ok(new
                {
                    message = "Itens encontrados com sucesso.",
                    items   = result
                });
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"[SearchMenuItems] Erro ao buscar itens: {ex.Message}");
                return Response.InternalError("Erro ao buscar itens de cardápio.");
            }
        }
    }
}
