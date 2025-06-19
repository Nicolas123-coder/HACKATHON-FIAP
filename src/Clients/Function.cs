using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SQS;
using Clients.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Utils;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Clients
{
    public class Function
    {
        private readonly IServiceProvider _serviceProvider;
        
        public Function()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }
        
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton<DynamoDbHelper>();
            services.AddAWSService<IAmazonSQS>();
            services.AddSingleton<PasswordHasherService>();
            services.AddSingleton<Authorizer>();
            services.AddTransient<Login>();
            services.AddTransient<SearchMenuItems>();
            services.AddTransient<CreateOrder>();
        }
        
        public async Task<APIGatewayProxyResponse> FunctionHandler(
            APIGatewayProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
            var path   = apigProxyEvent.Path?.ToLowerInvariant() ?? "/";
            var method = apigProxyEvent.HttpMethod        ?? "UNKNOWN";

            try
            {
                switch (path)
                {
                    case "/clients/login"
                        when method.Equals("POST", StringComparison.OrdinalIgnoreCase):
                    
                        var loginHandler = _serviceProvider.GetRequiredService<Login>();
                        return await loginHandler.HandleAsync(apigProxyEvent, context);
                
                    case "/clients/search-products"
                        when method.Equals("GET", StringComparison.OrdinalIgnoreCase):
                    
                        var auth = _serviceProvider.GetRequiredService<Authorizer>();
                        auth.ValidateClient(apigProxyEvent, context);
                    
                        var searcher = _serviceProvider.GetRequiredService<SearchMenuItems>();
                        return await searcher.HandleAsync(apigProxyEvent, context);
                    
                    case "/clients/create-order"
                        when method.Equals("POST", StringComparison.OrdinalIgnoreCase):
                    
                        var authOrder = _serviceProvider.GetRequiredService<Authorizer>();
                        authOrder.ValidateClient(apigProxyEvent, context);
                    
                        var createOrderHandler = _serviceProvider.GetRequiredService<CreateOrder>();
                        return await createOrderHandler.HandleAsync(apigProxyEvent, context);
                    default:
                        context.Logger.LogLine($"Orchestrator → Case não mapeado: Método={method}, Path={path}");
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
                context.Logger.LogLine($"[Error] {ex.Message}");
                return Response.InternalError("Ocorreu um erro interno.");
            }
        }
    }
}
