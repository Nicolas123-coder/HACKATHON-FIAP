using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Utils;

namespace Orders.Handlers
{
    public class Reject
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string         _tableName;

        public Reject(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb  = dynamoDb;
            _tableName = Environment
                .GetEnvironmentVariable("ORDERS_TABLE")
                ?? throw new InvalidOperationException("ORDERS_TABLE não definido");
        }

        public async Task<APIGatewayProxyResponse> HandleAsync(
            APIGatewayProxyRequest request,
            ILambdaContext         context)
        {
            context.Logger.LogLine("[RejectOrder] Iniciando HandleAsync");
            
            // 1) Extrai o OrderId do body JSON
            string orderId;
            try
            {
                var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Body)
                              ?? throw new JsonException("Body vazio");
                if (!payload.TryGetValue("orderId", out orderId) || string.IsNullOrWhiteSpace(orderId))
                    return Response.BadRequest("orderId é obrigatório.");
            }
            catch (JsonException ex)
            {
                context.Logger.LogLine($"[RejectOrder] Erro de desserialização JSON: {ex.Message}");
                return Response.BadRequest("Formato de JSON inválido.");
            }

            // 2) Prepara a atualização do campo Status → "Rejected"
            var updateRequest = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["Id"] = new AttributeValue { S = orderId }
                },
                UpdateExpression          = "SET #S = :rejected",
                ExpressionAttributeNames  = new Dictionary<string, string> { ["#S"] = "Status" },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":rejected"] = new AttributeValue { S = "Rejected" }
                },
                ReturnValues = ReturnValue.UPDATED_NEW
            };

            try
            {
                var result = await _dynamoDb.UpdateItemAsync(updateRequest);
                context.Logger.LogLine($"[RejectOrder] Status atualizado para 'Rejected' (OrderId={orderId})");

                return Response.Ok(new
                {
                    message = "Pedido rejeitado com sucesso.",
                    orderId
                });
            }
            catch (AmazonDynamoDBException ex)
            {
                context.Logger.LogLine($"[RejectOrder] Erro DynamoDB: {ex.Message}");
                return Response.InternalError("Erro ao atualizar o pedido.");
            }
        }
    }
}
