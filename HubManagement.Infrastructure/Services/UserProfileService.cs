using HubManagement.Application.DTOs;
using HubManagement.Application.Services;
using HubManagement.BuildingBlock.Core.Exceptions;
using HubManagement.BuildingBlock.Infrastructure.FileStorage;
using HubManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HubManagement.Infrastructure.Services;

public class UserProfileService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IStorageService storageService,
  //  IOptions<OriginOptions> originOptions,
    IHttpContextAccessor httpContextAccessor) : IUserProfileService
{
    public async Task<UserDto> GetAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        
        _ = user ?? throw new NotFoundException("user not found");

        return new UserDto()
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            TwoFactorEnabled = user.TwoFactorEnabled
        };
    }

    public async Task<List<UserDto>> GetListAsync(CancellationToken ct = default)
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync(ct);
        var result = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(new UserDto()
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                //ImageUrl = ResolveImageUrl(user.ImageUrl),
                PhoneNumber = user.PhoneNumber,
            });
        }
        return result;
    }

    public Task<int> GetCountAsync(CancellationToken ct = default)
        => userManager.Users.AsNoTracking().CountAsync(ct);

    public async  Task UpdateAsync(string userId, string firstName, string lastName, string phoneNumber, bool deleteCurrentImage,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new NotFoundException("user not found");

        // var oldImagePath = user.ImageUrl?.ToString();
        // string? newImagePath = null;
        //
        // if (image?.Stream is not null)
        // {
        //     newImagePath = await storageService.UploadAsync<User>(image, FileType.Image, ct);
        //     user.ImageUrl = new Uri(newImagePath, UriKind.Relative);
        // }
        // else if (deleteCurrentImage)
        // {
        //     user.ImageUrl = null;
        // }

        user.FirstName = firstName;
        user.LastName = lastName;

        var currentPhoneNumber = await userManager.GetPhoneNumberAsync(user);
        if (phoneNumber != currentPhoneNumber)
        {
            await userManager.SetPhoneNumberAsync(user, phoneNumber);
        }

        var result = await userManager.UpdateAsync(user);
        
        await signInManager.RefreshSignInAsync(user);
    }

    public async Task SetImageUrlAsync(string userId, string? imageUrl, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new NotFoundException("user not found");
        
        user.ImageUrl = string.IsNullOrWhiteSpace(imageUrl) 
            ? null : new Uri(imageUrl, UriKind.RelativeOrAbsolute);
        
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new CustomException("Update profile image failed");
        }
        
        await signInManager.RefreshSignInAsync(user);
    }

    public async Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null, CancellationToken ct = default)
    {
        return await userManager.FindByEmailAsync(email.Normalize()) is { } user && user.Id != exceptId;
    }

    public async Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default)
    {
        return await userManager.FindByNameAsync(name) is not null;
    }

    public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null, CancellationToken ct = default)
    {
        return await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, ct) is { } user && user.Id != exceptId;
    }
}