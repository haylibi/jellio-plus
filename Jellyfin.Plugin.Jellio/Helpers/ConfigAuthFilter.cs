using System.Threading.Tasks;
using Jellyfin.Plugin.Jellio.Models;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Jellyfin.Plugin.Jellio.Helpers;

public class ConfigAuthFilter(IUserManager userManager, IDeviceManager deviceManager)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        if (context.ActionArguments.TryGetValue("config", out var cfg) && cfg is ConfigModel config)
        {
            var userId = RequestHelpers.GetUserIdByAuthToken(
                config.AuthToken,
                deviceManager
            );
            if (userId == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Verify user exists
            var user = userManager.GetUserById(userId.Value);
            if (user == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            context.HttpContext.Items["JellioUserId"] = userId.Value;
        }

        await next().ConfigureAwait(false);
    }
}
