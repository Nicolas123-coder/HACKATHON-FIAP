using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Orders.DTOs;

namespace Orders.Handlers
{
    public class ProcessOrder
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string         _tableName;

        public ProcessOrder(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb  = dynamoDb;
            _tableName = Environment
                .GetEnvironmentVariable("ORDERS_TABLE")
                ?? throw new InvalidOperationException("ORDERS_TABLE não definido");
        }

        public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
        {
            foreach (var record in sqsEvent.Records)
            {
                context.Logger.LogLine($"[ProcessOrder] Mensagem recebida: {record.Body}");

                OrderMessageDTO order;
                try
                {
                    order = JsonSerializer
                        .Deserialize<OrderMessageDTO>(record.Body)
                        ?? throw new JsonException("Body vazio");
                }
                catch (JsonException ex)
                {
                    context.Logger.LogLine($"Erro de deserialização JSON: {ex.Message}");
                    // pula para a próxima mensagem
                    continue;
                }

                // Monta o item para o DynamoDB
                var item = new Dictionary<string, AttributeValue>
                {
                    ["Id"]      = new AttributeValue { S = order.OrderId },
                    ["ClientId"]     = new AttributeValue { S = order.ClientId },
                    ["DeliveryType"] = new AttributeValue { S = order.DeliveryType },
                    ["CreatedAt"]    = new AttributeValue { S = order.CreatedAt.ToString("o") },
                    ["Items"]        = new AttributeValue
                    {
                        L = order.Items.Select(i => new AttributeValue
                        {
                            M = new Dictionary<string, AttributeValue>
                            {
                                ["ItemId"]   = new AttributeValue { S = i.ItemId },
                                ["Quantity"] = new AttributeValue { N = i.Quantity.ToString() }
                            }
                        }).ToList()
                    }
                };

                try
                {
                    await _dynamoDb.PutItemAsync(_tableName, item);
                    context.Logger.LogLine($"Pedido salvo em DynamoDB (OrderId={order.OrderId})");
                }
                catch (Exception ex)
                {
                    context.Logger.LogLine($"Erro ao salvar pedido no DynamoDB: {ex.Message}");
                }
            }
        }
    }
}
