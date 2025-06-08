using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Utils
{
    public class DynamoDbHelper : IDisposable
    {
        private readonly IAmazonDynamoDB _client;
        private readonly DynamoDBContext _context;
        private bool _disposed;

        public DynamoDbHelper()
        {
            _client = new AmazonDynamoDBClient();

            var config = new DynamoDBContextConfig
            {
                Conversion = DynamoDBEntryConversion.V2
            };

            _context = new DynamoDBContext(_client, config);
        }

        public DynamoDbHelper(IAmazonDynamoDB dynamoDbClient)
        {
            _client = dynamoDbClient;

            var config = new DynamoDBContextConfig
            {
                Conversion = DynamoDBEntryConversion.V2
            };

            _context = new DynamoDBContext(_client, config);
        }

        public Task SaveAsync<T>(T item, CancellationToken cancellationToken = default)
            where T : class
        {
            return _context.SaveAsync(item, cancellationToken);
        }

        public Task<T?> GetAsync<T>(object hashKey, CancellationToken cancellationToken = default)
            where T : class
        {
            return _context.LoadAsync<T>(hashKey, cancellationToken);
        }

        public Task<T?> GetAsync<T>(object hashKey, object rangeKey, CancellationToken cancellationToken = default)
            where T : class
        {
            return _context.LoadAsync<T>(hashKey, rangeKey, cancellationToken);
        }

        public Task DeleteAsync<T>(object hashKey, CancellationToken cancellationToken = default)
            where T : class
        {
            return _context.DeleteAsync<T>(hashKey, cancellationToken);
        }

        public Task DeleteAsync<T>(object hashKey, object rangeKey, CancellationToken cancellationToken = default)
            where T : class
        {
            return _context.DeleteAsync<T>(hashKey, rangeKey, cancellationToken);
        }

        public async Task<List<T>> QueryAsync<T>(object hashKey, DynamoDBOperationConfig? operationConfig = null)
            where T : class
        {
            var search = _context.QueryAsync<T>(hashKey, operationConfig);
            return await search.GetRemainingAsync().ConfigureAwait(false);
        }

        public async Task<List<T>> ScanAsync<T>(List<ScanCondition>? conditions = null)
            where T : class
        {
            var search = _context.ScanAsync<T>(conditions ?? new List<ScanCondition>());
            return await search.GetRemainingAsync().ConfigureAwait(false);
        }

        public async Task<Document> PutDocumentAsync(
            string tableName,
            Document document,
            CancellationToken cancellationToken = default)
        {
            var table = Table.LoadTable(_client, tableName);
            return await table.PutItemAsync(document, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Document?> GetDocumentAsync(
            string tableName,
            Primitive hashKey,
            Primitive? rangeKey = null,
            CancellationToken cancellationToken = default)
        {
            var table = Table.LoadTable(_client, tableName);

            if (rangeKey == null)
            {
                return await table.GetItemAsync(hashKey, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await table.GetItemAsync(hashKey, rangeKey, cancellationToken).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _context.Dispose();
            _client.Dispose();
            _disposed = true;
        }
    }
}