using System.Text.Json.Serialization;

namespace Utils.Components
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmployeeRoles
    {
        Employee,
        Manager
    }
}