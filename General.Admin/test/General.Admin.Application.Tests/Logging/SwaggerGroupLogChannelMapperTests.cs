using General.Admin.Infrastructure;
using General.Admin.Logging;
using Shouldly;
using Xunit;

namespace General.Admin.Logging;

public class SwaggerGroupLogChannelMapperTests
{
    private readonly SwaggerGroupLogChannelMapper _mapper = new();

    [Theory]
    [InlineData(ApiDocGroups.Platform, LoggingChannelConstants.Platform)]
    [InlineData(ApiDocGroups.Project, LoggingChannelConstants.Project)]
    [InlineData(ApiDocGroups.Business, LoggingChannelConstants.Business)]
    [InlineData(null, LoggingChannelConstants.Platform)]
    [InlineData("", LoggingChannelConstants.Platform)]
    [InlineData("unknown", LoggingChannelConstants.Platform)]
    public void ResolveChannel_Should_Map_Swagger_Group_To_Managed_Channel(string? groupName, string expectedChannel)
    {
        _mapper.ResolveChannel(groupName).ShouldBe(expectedChannel);
    }
}
