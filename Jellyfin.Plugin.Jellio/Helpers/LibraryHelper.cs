using System;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities; // BaseItem
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Library;

namespace Jellyfin.Plugin.Jellio.Helpers;

public static class LibraryHelper
{
    internal static BaseItemDto[] GetUserLibraries(Guid userId, IUserManager userManager, IUserViewManager userViewManager, IDtoService dtoService)
    {
        // Defensive: check for empty Guid
        if (userId == Guid.Empty)
        {
            // LOG: Invalid userId (empty Guid) encountered in GetUserLibraries
            return Array.Empty<BaseItemDto>();
        }

        // Defensive: try/catch for ArgumentException (empty Guid, etc.)
        try
        {
            var user = userManager.GetUserById(userId);
            if (user == null)
            {
                // LOG: User not found for userId in GetUserLibraries
                return Array.Empty<BaseItemDto>();
            }

            var query = new UserViewQuery { User = user };
            BaseItem[] folders;
            try
            {
                folders = userViewManager.GetUserViews(query);
            }
            catch (ArgumentException)
            {
                // LOG: Invalid user in UserViewQuery (user may not exist)
                return Array.Empty<BaseItemDto>();
            }
            catch (Exception)
            {
                // LOG: Unexpected error in GetUserViews (possible DB or plugin error)
                return Array.Empty<BaseItemDto>();
            }

            var dtoOptions = new DtoOptions(false);
            var dtos = Array.ConvertAll(folders, i => dtoService.GetBaseItemDto(i, dtoOptions, user));
            // Only return TV and Movie collection folders
            return Array.FindAll(
                dtos,
                dto =>
                    dto.Type is BaseItemKind.CollectionFolder
                    && dto.CollectionType is CollectionType.tvshows or CollectionType.movies
            );
        }
        catch (ArgumentException)
        {
            // LOG: ArgumentException in GetUserById (invalid Guid)
            return Array.Empty<BaseItemDto>();
        }
        catch (Exception)
        {
            // LOG: Unexpected error in GetUserLibraries
            return Array.Empty<BaseItemDto>();
        }
    }
}
