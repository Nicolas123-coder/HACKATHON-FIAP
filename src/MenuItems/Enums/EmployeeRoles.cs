using System.Text.Json.Serialization;

namespace MenuItems.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmployeeRoles
    {
        Employee,
        Manager
    }
}