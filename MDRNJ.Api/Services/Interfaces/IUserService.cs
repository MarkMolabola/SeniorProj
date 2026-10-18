using MDRNJ.Shared.DTOs.User;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(Guid userId);
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto);
        Task DeleteAccountAsync(Guid userId);
        Task AcceptTermsAsync(Guid userId);
    }
}
