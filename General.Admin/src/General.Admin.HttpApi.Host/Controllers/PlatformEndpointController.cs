using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/platform/endpoint")]
public class PlatformEndpointController : ControllerBase
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

    public PlatformEndpointController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    }

    [HttpGet("options")]
    public ActionResult<ApiResponse<List<PlatformEndpointGroupDto>>> GetOptions()
    {
        var items = _actionDescriptorCollectionProvider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Select(action => new
            {
                Action = action,
                ActionKey = PlatformEndpointKeyHelper.GetActionKey(action),
                CapabilityKey = PlatformEndpointKeyHelper.GetCapabilityKey(action)
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.ActionKey))
            .Select(x => new
            {
                GroupName = x.Action.ControllerName,
                Item = new PlatformEndpointOptionDto
                {
                    CapabilityKey = x.CapabilityKey,
                    Key = x.ActionKey!,
                    Label = string.IsNullOrWhiteSpace(x.CapabilityKey)
                        ? $"{x.ActionKey} · {x.Action.ActionName}"
                        : $"{x.ActionKey} · {x.Action.ActionName} · {x.CapabilityKey}"
                }
            })
            .GroupBy(x => x.GroupName)
            .OrderBy(x => x.Key)
            .Select(group => new PlatformEndpointGroupDto
            {
                GroupName = group.Key,
                Items = group
                    .Select(x => x.Item)
                    .DistinctBy(x => x.Key)
                    .OrderBy(x => x.Key)
                    .ToList()
            })
            .ToList();

        return ApiResponse<List<PlatformEndpointGroupDto>>.Ok(items);
    }
}
