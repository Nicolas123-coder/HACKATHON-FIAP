using System;
using Amazon.DynamoDBv2.DataModel;

namespace Employees.Entities
{
    [DynamoDBTable("PosTech-Employees")]
    public class Employee
    {
        [DynamoDBHashKey]
        public string Id { get; set; } = null!;
        
        public string Name { get; set; } = null!;
        
        public string Email { get; set; } = null!;
        
        [DynamoDBIgnore]
        public string Password { get; set; } = null!;
        
        public string PasswordHash { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
    }
}