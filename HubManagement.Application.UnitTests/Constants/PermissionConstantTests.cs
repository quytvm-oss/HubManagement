using HubManagement.Application.Constants;

namespace HubManagement.Application.UnitTests.Constants;

public sealed class PermissionConstantTests
{
    [Fact]
    public void Permission_names_are_unique_and_well_formed()
    {
        var names = PermissionConstant.All.Select(permission => permission.Name).ToArray();

        Assert.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count());
        Assert.All(names, name => Assert.StartsWith("Permissions.", name));
        Assert.All(names, name => Assert.Equal(3, name.Split('.').Length));
    }
}
