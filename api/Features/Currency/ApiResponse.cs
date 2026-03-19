using System.Text.Json.Serialization;

namespace api.Features.Currency
{
    public class ApiResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = string.Empty;

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = string.Empty;

        [JsonPropertyName("target_code")]
        public string TargetCode { get; set; } = string.Empty;

        [JsonPropertyName("conversion_rate")]
        public decimal ConversionRate { get; set; }
    }
}
