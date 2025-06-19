using System;
using Amazon.DynamoDBv2.DataModel;

namespace Utils.Entities
{
    [DynamoDBTable("PosTech-MenuItems")]
    public class MenuItem
    {
        [DynamoDBHashKey]
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Type { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}