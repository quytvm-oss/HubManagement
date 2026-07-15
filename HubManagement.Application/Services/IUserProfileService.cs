// using HubManagement.Application.DTOs;
//
// namespace HubManagement.Application.Services;
//
// public interface IUserProfileService
// {
//     Task<UserDto> GetAsync(string userId, CancellationToken ct = default);
//     
//     Task<List<UserDto>> GetListAsync(CancellationToken ct = default);
//     
//     Task<int> GetCountAsync(CancellationToken ct = default);
//
//     Task UpdateAsync(string userId, string firstName, string lastName, string phoneNumber,
//         bool deleteCurrentImage, CancellationToken ct = default);
//     
//     Task SetImageUrlAsync(string userId, string? imageUrl, CancellationToken ct = default);
//
//     Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null, CancellationToken ct = default);
//     
//     Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default);
//     
//     Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null, CancellationToken ct = default);
// }