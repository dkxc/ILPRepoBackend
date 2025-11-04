using System.Text.Json.Serialization;

namespace IlpRepoBackend.Domain.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AttendanceStatus
    {
        P,
        A,
        NA
    }
}
