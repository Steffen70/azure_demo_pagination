using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.NotificationHubs;
using SPPaginationDemo.Services;

#pragma warning disable CA2254
#pragma warning disable IDE0290

namespace SPPaginationDemo.Controllers;

[ApiController]
[Route("register-token")]
public class RegisterTokenController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly Appsettings _appsettings;

    public RegisterTokenController(ILogger logger, Appsettings appsettings)
    {
        _logger = logger;
        _appsettings = appsettings;
    }

    [HttpPost("{token}")]
    public async Task<IActionResult> Post([FromRoute] string token)
    {
        token = Encoding.UTF8.GetString(Convert.FromBase64String(token));

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is null or empty.");

        //var tokenTag = token.GenerateMd5Hash();

        // get tags from post body object { tags: base64string }

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);

        var json = await reader.ReadToEndAsync();

        var tokenTags = JsonSerializer.Deserialize<ISet<string>>(json);

        if(tokenTags == null || !tokenTags.Any())
            return BadRequest("Token tags are null or empty.");

        var registrationId = await _appsettings.NotificationHubClient.CreateRegistrationIdAsync();

        var registration = new FcmRegistrationDescription(token)
        {
            RegistrationId = registrationId,
            Tags = tokenTags
        };

        _logger.LogInformation($"Registering token {token} with tag {json}");

        await _appsettings.NotificationHubClient.CreateOrUpdateRegistrationAsync(registration);

        _appsettings.RedisDatabase.StringSet("NotificationToken", json);

        return Ok();
    }

    [HttpDelete("{token}")]
    public async Task<IActionResult> Delete([FromRoute] string token)
    {
        token = Encoding.UTF8.GetString(Convert.FromBase64String(token));

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is null or empty.");

        var registration = await _appsettings.NotificationHubClient.GetRegistrationAsync<FcmRegistrationDescription>(token);

        if (registration == null)
            return BadRequest("Token not found.");

        await _appsettings.NotificationHubClient.DeleteRegistrationAsync(token);

        _appsettings.RedisDatabase.StringGetDelete("NotificationToken");

        return Ok();
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestNotification()
    {
        var json = _appsettings.RedisDatabase.StringGet("NotificationToken");

        var tokenTags = json.HasValue ? JsonSerializer.Deserialize<ISet<string>>(json!) : null;

        if (tokenTags == null || !tokenTags.Any())
            return BadRequest("No Tags saved in Redis");

        const string message = $@"
            {{ 
                ""notification"" : {{
                    ""body"" : ""This is a notification with body and title."",
                    ""title"" : ""Hello World""
                }},
                ""data"" : {{
                    ""body"" : ""This is a notification with body and title."",
                    ""message"" : ""Hello World"",
                }}
            }}
        ";

        _ = await _appsettings.NotificationHubClient.SendFcmNativeNotificationAsync(message, string.Join(" || ", tokenTags));

        return Accepted();
    }

}
