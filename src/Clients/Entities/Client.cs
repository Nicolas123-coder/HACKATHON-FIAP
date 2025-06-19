using System;
using Amazon.DynamoDBv2.DataModel;

namespace Clients.Entities
{
    [DynamoDBTable("PosTech-Clients")]
    public class Client
    {
        [DynamoDBHashKey]
        public string Id { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string CPF { get; set; } = null!;

        [DynamoDBIgnore]
        public string Password { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}