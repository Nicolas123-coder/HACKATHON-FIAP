using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.DependencyInjection;
using Orders.Handlers;
using Utils;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Orders
{
    public class Function
    {
        static Function()
        {
            // X-Ray: init only on cold start
            AWSXRayRecorder.InitializeInstance();
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        private readonly IServiceProvider _serviceProvider;

        public Function()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton<DynamoDbHelper>();
            services.AddAWSService<IAmazonSQS>();
            services.AddSingleton<PasswordHasherService>();

            services.AddTransient<Authorizer>();
            services.AddTransient<ProcessOrder>();
            services.AddTransient<List>();
            services.AddTransient<Accept>();
            services.AddTransient<Reject>();
        }

        /// <summary>
        /// Invocado pelo API Gateway em GET /orders
        /// </summary>
        public async Task<APIGatewayProxyResponse> FunctionHandler(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            var path   = request.Path?.ToLowerInvariant() ?? "/";
            var method = request.HttpMethod        ?? "UNKNOWN";

            context.Logger.LogLine($"[Orchestrator] HTTP {method} {path}");
            try
            {
                switch (path)
                {
                    case "/orders" 
                        when method.Equals("GET", StringComparison.OrdinalIgnoreCase):
                        
                        var auth0 = _serviceProvider.GetRequiredService<Authorizer>();
                        auth0.ValidateOrderAccess(request, context);

                        var listHandler = _serviceProvider.GetRequiredService<List>();
                        return await listHandler.HandleAsync(request, context);
                    
                    case "/orders/accept"
                        when method.Equals("POST", StringComparison.OrdinalIgnoreCase):
                        
                        var auth = _serviceProvider.GetRequiredService<Authorizer>();
                        auth.ValidateOrderAccess(request, context);
                        
                        var acceptHandler = _serviceProvider.GetRequiredService<Accept>();
                        return await acceptHandler.HandleAsync(request, context);

                    case "/orders/reject"
                        when method.Equals("POST", StringComparison.OrdinalIgnoreCase):
                        
                        var auth2 = _serviceProvider.GetRequiredService<Authorizer>();
                        auth2.ValidateOrderAccess(request, context);
                        
                        var rejectHandler = _serviceProvider.GetRequiredService<Reject>();
                        return await rejectHandler.HandleAsync(request, context);

                    default:
                        context.Logger.LogLine($"[Orchestrator] Endpoint não mapeado: {method} {path}");
                        return Response.NotFound($"Endpoint não encontrado: {method} {path}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                context.Logger.LogLine($"[Auth] {ex.Message}");
                return Response.Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"[Orchestrator] Erro: {ex.Message}");
                return Response.InternalError("Ocorreu um erro interno.");
            }
        }

        /// <summary>
        /// Invocado automaticamente pelo trigger SQS
        /// </summary>
        public async Task SQSEventHandler(
            SQSEvent sqsEvent,
            ILambdaContext context)
        {
            var processor = _serviceProvider.GetRequiredService<ProcessOrder>();
            await processor.FunctionHandler(sqsEvent, context);
        }
    }
}
