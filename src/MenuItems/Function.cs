using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.DependencyInjection;
using MenuItems.Handlers;
using Utils;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MenuItems
{
    public class Function
    {
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

            services.AddSingleton<Authorizer>();

            services.AddTransient<CreateMenu>();
            services.AddTransient<UpdateMenu>();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            var path   = request.Path?.ToLowerInvariant() ?? "/";
            var method = request.HttpMethod?.ToUpperInvariant() ?? "UNKNOWN";

            context.Logger.LogLine($"[Orchestrator] Método HTTP: {method}; Path: {path}");

            try
            {
                switch ($"{method} {path}")
                {
                    case "POST /menu/create":
                        var auth1 = _serviceProvider.GetRequiredService<Authorizer>();
                        auth1.ValidateManager(request, context);

                        var creator = _serviceProvider.GetRequiredService<CreateMenu>();
                        return await creator.HandleAsync(request, context);

                    case "PUT /menu/update":
                        var auth2 = _serviceProvider.GetRequiredService<Authorizer>();
                        auth2.ValidateManager(request, context);
                    
                        var updater = _serviceProvider.GetRequiredService<UpdateMenu>();
                        return await updater.HandleAsync(request, context);

                    default:
                        context.Logger.LogLine($"Rota não mapeada: {method} {path}");
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
