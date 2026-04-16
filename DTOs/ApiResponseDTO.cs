using System.Text.Json.Serialization;

public class ApiResponseDTO
{
    [JsonPropertyName("msg")]
    public string Msg { get; set; }

    [JsonPropertyName("foodId")]
    public string FoodId { get; set; }
}