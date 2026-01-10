namespace HCI.AIAssistant.API.Models.DTOs; 
 
public class ErrorResponseDTO 
{ 
    public required string TextErrorTitle { get; set; } 
    public required string TextErrorMessage { get; set; } 
    public required string TextErrorTrace { get; set; } 
} 