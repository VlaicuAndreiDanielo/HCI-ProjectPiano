using Microsoft.AspNetCore.Mvc;
using HCI.AIAssistant.API.Models.DTOs.IAssistantController;
using HCI.AIAssistant.API.Services;
using HCI.AIAssistant.API.Models.DTOs;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System.Text;

namespace HCI.AIAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIAssistantController : ControllerBase
{
    private readonly IAIAssistantService _aIAssistantService;
    private readonly IParametricFunctions _parametricFunctions;
    private readonly IAppConfigurationsService _appConfigurationsService;
    private readonly ISecretsService _secretsService;

    public AIAssistantController(
        IAIAssistantService aIAssistantService,
        IParametricFunctions parametricFunctions,
        IAppConfigurationsService appConfigurationsService,
        ISecretsService secretsService
    )
    {
        _aIAssistantService = aIAssistantService;
        _parametricFunctions = parametricFunctions;
        _appConfigurationsService = appConfigurationsService;
        _secretsService = secretsService;
    }

    [HttpPost("message")]
    [ProducesResponseType(typeof(AIAssistantControllerPostMessageResponseDTO), 200)]
    [ProducesResponseType(typeof(ErrorResponseDTO), 400)]
    public async Task<ActionResult> PostMessage([FromBody] AIAssistantControllerPostMessageRequestDTO request)
    {
        if (!_parametricFunctions.ObjectExistsAndHasNoNullPublicProperties(request))
        {
            return BadRequest(
                new ErrorResponseDTO()
                {
                    TextErrorTitle = "AtLeastOneNullParameter",
                    TextErrorMessage = "Some parameters are null/missing.",
                    TextErrorTrace = _parametricFunctions.GetCallerTrace()
                }
            );
        }

        string messageToSendToAssistant = "Instruction: " + _appConfigurationsService.Instruction + "\nMessage: " + request.TextMessage;

#pragma warning disable CS8604
        string textMessageResponse = await _aIAssistantService.SendMessageAndGetResponseAsync(messageToSendToAssistant);
#pragma warning restore CS8604

        AIAssistantControllerPostMessageResponseDTO response = new()
        {
            TextMessage = textMessageResponse
        };

        string? ioTHubConnectionString = _secretsService?.IoTHubSecrets?.ConnectionString;
        if (ioTHubConnectionString != null)
        {
            var serviceClientForIoTHub = ServiceClient.CreateFromConnectionString(ioTHubConnectionString);
            var seralizedMessage = JsonConvert.SerializeObject(textMessageResponse);

            var ioTMessage = new Message(Encoding.UTF8.GetBytes(seralizedMessage));
            await serviceClientForIoTHub.SendAsync(_appConfigurationsService.IoTDeviceName, ioTMessage);
        }

        return Ok(response);
    }

    [HttpPost("esp-message")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ErrorResponseDTO), 400)]
    public async Task<ActionResult> PostEspMessage([FromBody] EspMessageRequestDTO request)
    {
        if (!_parametricFunctions.ObjectExistsAndHasNoNullPublicProperties(request))
        {
            return BadRequest(
                new ErrorResponseDTO()
                {
                    TextErrorTitle = "AtLeastOneNullParameter",
                    TextErrorMessage = "Some parameters are null/missing.",
                    TextErrorTrace = _parametricFunctions.GetCallerTrace()
                }
            );
        }

        // Format message for AI Assistant
        string messageToSendToAssistant = $"Device {request.DeviceId} reports: {request.Message}. " +
                                         _appConfigurationsService.Instruction;

        try
        {
            string aiResponse = await _aIAssistantService.SendMessageAndGetResponseAsync(messageToSendToAssistant);
            
            // Return simple response for ESP32
            return Ok(new { 
                success = true, 
                response = aiResponse,
                timestamp = DateTime.UtcNow,
                deviceId = request.DeviceId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponseDTO()
            {
                TextErrorTitle = "AIAssistantError",
                TextErrorMessage = ex.Message,
                TextErrorTrace = _parametricFunctions.GetCallerTrace()
            });
        }
    }
}