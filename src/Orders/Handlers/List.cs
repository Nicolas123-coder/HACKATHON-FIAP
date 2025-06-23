using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Orders.DTOs;
using Utils;

namespace Orders.Handlers
{
    public class List
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string         _tableName;

        public List(IAmazonDynamoDB dynamoDb)
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
            context.Logger.LogLine("[ListOrders] Iniciando HandleAsync");

            var orders = new List<OrderMessageDTO>();

            try
            {
                var response = await _dynamoDb.ScanAsync(new ScanRequest
                {
                    TableName = _tableName
                });

                foreach (var item in response.Items)
                {
                    // 1) Sub-itens do pedido
                    var items = item["Items"].L
                        .Select(av => av.M)
                        .Select(m => new ItemMessageDTO
                        {
                            ItemId   = m["ItemId"].S,
                            Quantity = int.Parse(m["Quantity"].N)
                        })
                        .ToList();

                    // 2) Status (default para "Pending" se não existir)
                    string status = item.TryGetValue("Status", out var statusAttr) 
                        && !string.IsNullOrEmpty(statusAttr.S)
                            ? statusAttr.S
                            : "Pending";

                    // 3) Monta o DTO
                    orders.Add(new OrderMessageDTO
                    {
                        OrderId      = item["Id"].S,
                        ClientId     = item["ClientId"].S,
                        DeliveryType = item["DeliveryType"].S,
                        CreatedAt    = DateTime.Parse(item["CreatedAt"].S),
                        Status       = status,
                        Items        = items
                    });
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"[ListOrders] Erro ao scan DynamoDB: {ex.Message}");
                return Response.InternalError("Erro ao listar pedidos.");
            }

            return Response.Ok(new { orders });
        }
    }
}
