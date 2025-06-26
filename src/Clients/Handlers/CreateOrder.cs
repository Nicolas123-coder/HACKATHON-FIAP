using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Clients.DTOs;
using Clients.Validators;
using Utils;

namespace Clients.Handlers
{
    public class CreateOrder
    {
        private readonly IAmazonSQS _sqs;
        private readonly string    _queueUrl;

        public CreateOrder(IAmazonSQS sqs)
        {
            _sqs      = sqs;
            _queueUrl = Environment
                .GetEnvironmentVariable("ORDERS_QUEUE_URL")
                ?? throw new InvalidOperationException("ORDERS_QUEUE_URL não definido");
        }

        public async Task<APIGatewayProxyResponse> HandleAsync(
            APIGatewayProxyRequest request,
            ILambdaContext         context)
        {
            context.Logger.LogLine("[CreateOrder] Iniciando HandleAsync");
            context.Logger.LogLine($"Payload recebido: {request.Body}");

            // 1) Desserializa
            CreateOrderDTO dto;
            try
            {
                dto = JsonSerializer
                      .Deserialize<CreateOrderDTO>(request.Body)
                      ?? throw new JsonException("Body vazio");
            }
            catch (JsonException ex)
            {
                context.Logger.LogLine($"Erro de desserialização JSON: {ex.Message}");
                return Response.BadRequest("Formato de JSON inválido.");
            }

            // 2) Validações
            var errors = CreateOrderValidator.Validate(dto);
            if (errors.Count > 0)
                return Response.BadRequest(string.Join("teste teste", errors));

            // 3) Monta mensagem com um OrderId único
            var orderMessage = new
            {
                OrderId      = Guid.NewGuid().ToString(),
                Items        = dto.Items,
                DeliveryType = dto.DeliveryType,
                ClientId    = dto.ClientId,
                CreatedAt    = DateTime.UtcNow
            };

            var messageBody = JsonSerializer.Serialize(orderMessage);

            try
            {
                await _sqs.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl    = _queueUrl,
                    MessageBody = messageBody,
                    MessageGroupId = dto.ClientId,
                    MessageDeduplicationId = orderMessage.OrderId
                });
                context.Logger.LogLine($"Pedido enviado para SQS (OrderId={orderMessage.OrderId})");
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Erro ao enviar pedido para SQS: {ex.Message}");
                return Response.InternalError("Erro ao processar o pedido.");
            }

            return Response.Created(new
            {
                message = "Pedido recebido com sucesso.",
                orderId = orderMessage.OrderId
            });
        }
    }
}
