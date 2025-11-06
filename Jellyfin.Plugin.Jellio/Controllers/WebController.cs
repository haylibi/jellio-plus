using System.Net.Mime;
using System.Reflection;
using Jellyfin.Plugin.Jellio.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.Jellio.Controllers;

[ApiController]
[Route("jellio")]
public class WebController : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly IUserViewManager _userViewManager;
    private readonly IDtoService _dtoService;
    private readonly IServerApplicationHost _serverApplicationHost;
    private readonly Assembly _executingAssembly = Assembly.GetExecutingAssembly();

    public WebController(
        IUserManager userManager,
        IUserViewManager userViewManager,
        IDtoService dtoService,
        IServerApplicationHost serverApplicationHost
    )
    {
        _userManager = userManager;
        _userViewManager = userViewManager;
        _dtoService = dtoService;
        _serverApplicationHost = serverApplicationHost;
    }

    [HttpGet]
    [HttpGet("configure")]
    [HttpGet("{config?}/configure")]
    public IActionResult GetIndex(string? config = null)
    {
        // Check if user is authenticated
        var userId = RequestHelpers.GetCurrentUserId(User);
        if (userId == null)
        {
            // Redirect to Jellyfin web UI with a message
            return Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>Jellio - Authentication Required</title>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; text-align: center; background: #101010; color: #fff; }
        .container { max-width: 600px; margin: 0 auto; }
        h1 { color: #00a4dc; }
        .button { display: inline-block; padding: 12px 24px; background: #00a4dc; color: white; text-decoration: none; border-radius: 4px; margin-top: 20px; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>Authentication Required</h1>
        <p>Please access Jellio through Jellyfin's plugin configuration page.</p>
        <p>Go to: <strong>Dashboard → Plugins → Jellio → Settings</strong></p>
        <a href='/web/index.html#!/dashboard' class='button'>Go to Jellyfin Dashboard</a>
    </div>
</body>
</html>", "text/html");
        }

        const string ResourceName = "Jellyfin.Plugin.Jellio.Web.config.html";

        var resourceStream = _executingAssembly.GetManifestResourceStream(ResourceName);

        if (resourceStream == null)
        {
            return NotFound($"Resource {ResourceName} not found.");
        }

        return new FileStreamResult(resourceStream, "text/html");
    }

    [HttpGet("server-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(MediaTypeNames.Application.Json)]
    public IActionResult GetServerInfo()
    {
        // Try to get user from claims (works when accessed from Jellyfin web UI)
        var userId = RequestHelpers.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { error = "Not authenticated. Please access this page through Jellyfin's web interface." });
        }

        var friendlyName = _serverApplicationHost.FriendlyName;
        var libraries = LibraryHelper.GetUserLibraries(userId.Value, _userManager, _userViewManager, _dtoService);

        return Ok(new { name = friendlyName, libraries });
    }
}
