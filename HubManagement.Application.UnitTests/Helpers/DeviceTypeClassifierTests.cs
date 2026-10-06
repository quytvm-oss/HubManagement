using HubManagement.Application.Helpers;

namespace HubManagement.Application.UnitTests.Helpers;

public sealed class DeviceTypeClassifierTests
{
    [Theory]
    [InlineData(null, "Desktop")]
    [InlineData("", "Desktop")]
    [InlineData("Other", "Desktop")]
    [InlineData("Windows", "Desktop")]
    [InlineData("iPhone", "Mobile")]
    [InlineData("Android Phone", "Mobile")]
    [InlineData("iPad", "Tablet")]
    [InlineData("Android Tablet", "Tablet")]
    public void Classify_returns_expected_device_type(string? deviceFamily, string expected)
    {
        Assert.Equal(expected, DeviceTypeClassifier.Classify(deviceFamily));
    }
}
