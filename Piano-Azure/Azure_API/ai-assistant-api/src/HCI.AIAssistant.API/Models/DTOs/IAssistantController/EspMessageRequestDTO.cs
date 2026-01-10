namespace HCI.AIAssistant.API.Models.DTOs.IAssistantController;

public class EspMessageRequestDTO
{
    public string DeviceId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SensorType { get; set; }
    public float? Value { get; set; }
    public string? Unit { get; set; }
}