using System.Text.Json.Serialization;

namespace Employees.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmployeeRoles
    {
        Employee,
        Manager
    }
}