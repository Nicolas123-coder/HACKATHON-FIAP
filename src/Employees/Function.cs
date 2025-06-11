using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Employees.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Utils;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Employees
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
            services.AddSingleton<PasswordHasherService>();
            services.AddTransient<Create>();
            services.AddTransient<Login>();
        }
        
        public async Task<APIGatewayProxyResponse> FunctionHandler(
            APIGatewayProxyRequest apigProxyEvent,
            ILambdaContext context)
        {
            var path   = apigProxyEvent.Path?.ToLowerInvariant() ?? "/";
            var method = apigProxyEvent.HttpMethod        ?? "UNKNOWN";

            context.Logger.LogLine($"[Orchestrator] Método HTTP: {method}; Path: {path}");

            switch (path)
            {
                case "/employees/login"
                    when method.Equals("POST", StringComparison.OrdinalIgnoreCase):
                    context.Logger.LogLine("Enviando para LoginHandler");
                    
                    var loginHandler = _serviceProvider.GetRequiredService<Login>();
                    return await loginHandler.HandleAsync(apigProxyEvent, context);
                
                case "/employees/create"
                    when method.Equals("POST", StringComparison.OrdinalIgnoreCase):
                    context.Logger.LogLine("Enviando para CreateEmployeeHandler");
                    
                    var createHandler = _serviceProvider.GetRequiredService<Create>();
                    return await createHandler.HandleAsync(apigProxyEvent, context);
               
                default:
                    context.Logger.LogLine($"Orchestrator → Case não mapeado: Método={method}, Path={path}");
                    return Response.NotFound($"Endpoint não encontrado: {method} {path}");
            }
        }
    }
}
