using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Orders.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Utils;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Orders
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
            services.AddTransient<ProcessOrder>();
        }
        
        public async Task SQSEventHandler(SQSEvent sqsEvent, ILambdaContext context)
        {
            var processor = _serviceProvider.GetRequiredService<ProcessOrder>();
            await processor.FunctionHandler(sqsEvent, context);
        }
    }
}
