using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Orders.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Utils;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Orders
{
    public class Function
    {
        static Function()
        {
            // 1) Inicializa o X-Ray recorder
            AWSXRayRecorder.InitializeInstance();
            // 2) Instrumenta todas as chamadas do AWS SDK para gerar subsegments
            AWSSDKHandler.RegisterXRayForAllServices();
        }
        
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
