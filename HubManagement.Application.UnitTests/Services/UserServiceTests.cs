using HubManagement.Application.Contracts;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.Authorization;
using HubManagement.BuildingBlock.Infrastructure.Cache.Abstractions;
using HubManagement.Domain.Entities;
using HubManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace HubManagement.Application.UnitTests.Services;

public sealed class UserServiceTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IQueryableRoleStore<ApplicationRole> _roleStore;
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _roleStore = Substitute.For<IQueryableRoleStore<ApplicationRole>>();
        _roleManager = new RoleManager<ApplicationRole>(
            _roleStore,
            [],
            Substitute.For<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Substitute.For<ILogger<RoleManager<ApplicationRole>>>());

        _sut = new UserService(_userManager, _roleManager, _db, _cache);
    }

    [Fact]
    public async Task ExistsWithEmailAsync_trims_email_and_returns_true_for_another_user()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@example.com" };
        _userManager.FindByEmailAsync("user@example.com").Returns(user);

        var result = await _sut.ExistsWithEmailAsync("  user@example.com  ");

        Assert.True(result);
        await _userManager.Received(1).FindByEmailAsync("user@example.com");
    }

    [Fact]
    public async Task ExistsWithEmailAsync_returns_false_when_found_user_is_excepted()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@example.com" };
        _userManager.FindByEmailAsync(user.Email).Returns(user);

        var result = await _sut.ExistsWithEmailAsync(user.Email, user.Id);

        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistsWithNameAsync_returns_expected_result(bool userExists)
    {
        _userManager.FindByNameAsync("john")
            .Returns(userExists ? new ApplicationUser { UserName = "john" } : null);

        Assert.Equal(userExists, await _sut.ExistsWithNameAsync("john"));
    }

    [Fact]
    public async Task ExistsWithPhoneNumberAsync_normalizes_phone_number()
    {
        var users = new[]
        {
            new ApplicationUser { Id = Guid.NewGuid(), PhoneNumber = "+84901234567" }
        }.BuildMockDbSet();
        _userManager.Users.Returns(users);

        var result = await _sut.ExistsWithPhoneNumberAsync(" +84 (901)-234-567 ");

        Assert.True(result);
    }

    [Fact]
    public async Task GetPermissionsAsync_returns_cached_permissions_without_loading_user()
    {
        var cached = new List<string> { "Permissions.Users.View" };
        _cache.GetItemAsync<List<string>>("perm:user-1", Arg.Any<CancellationToken>())
            .Returns(cached);

        var result = await _sut.GetPermissionsAsync("user-1");

        Assert.Same(cached, result);
        await _userManager.DidNotReceive().FindByIdAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetPermissionsAsync_loads_distinct_permissions_and_caches_them()
    {
        const string userId = "user-1";
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        var adminRole = new ApplicationRole("Admin") { Id = Guid.NewGuid() };
        var auditorRole = new ApplicationRole("Auditor") { Id = Guid.NewGuid() };

        _cache.GetItemAsync<List<string>>("perm:user-1", Arg.Any<CancellationToken>())
            .Returns((List<string>?)null);
        _userManager.FindByIdAsync(userId).Returns(user);
        _userManager.GetRolesAsync(user).Returns(["Admin", "Auditor"]);
        var roles = new[] { adminRole, auditorRole }.BuildMockDbSet();
        var roleClaims = new[]
        {
            CreateClaim(adminRole.Id, "Permissions.Users.View"),
            CreateClaim(adminRole.Id, "Permissions.Users.Update"),
            CreateClaim(auditorRole.Id, "Permissions.Users.View"),
            CreateClaim(auditorRole.Id, "ignored", "other-claim")
        }.BuildMockDbSet();

        _roleStore.Roles.Returns(roles);
        _db.RoleClaims.Returns(roleClaims);

        var result = await _sut.GetPermissionsAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("Permissions.Users.View", result);
        Assert.Contains("Permissions.Users.Update", result);
        await _cache.Received(1).SetItemAsync(
            "perm:user-1",
            Arg.Is<List<string>>(items => items.Count == 2),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPermissionsAsync_throws_when_user_does_not_exist()
    {
        _cache.GetItemAsync<List<string>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((List<string>?)null);
        _userManager.FindByIdAsync("missing").Returns((ApplicationUser?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.GetPermissionsAsync("missing"));
    }

    [Theory]
    [InlineData("Permissions.Users.View", true)]
    [InlineData("Permissions.Users.Delete", false)]
    public async Task HasPermissionAsync_returns_expected_result(string requestedPermission, bool expected)
    {
        _cache.GetItemAsync<List<string>>("perm:user-1", Arg.Any<CancellationToken>())
            .Returns(["Permissions.Users.View"]);

        Assert.Equal(expected, await _sut.HasPermissionAsync("user-1", requestedPermission));
    }

    [Fact]
    public async Task InvalidatePermissionCacheAsync_removes_expected_key_and_passes_token()
    {
        using var cts = new CancellationTokenSource();

        await _sut.InvalidatePermissionCacheAsync("user-1", cts.Token);

        await _cache.Received(1).RemoveItemAsync("perm:user-1", cts.Token);
    }

    private static ApplicationRoleClaim CreateClaim(Guid roleId, string value, string? type = null)
        => new()
        {
            RoleId = roleId,
            ClaimType = type ?? ClaimConstants.Permission,
            ClaimValue = value
        };
}
