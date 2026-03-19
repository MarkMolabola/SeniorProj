using MDRNJ.Shared.DTOs.User;
using System.Threading.Tasks;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetCurrentUserAsync();
        Task<UserProfileDto> GetProfileAsync();
        Task<UserProfileDto> UpdateProfileAsync(UpdateUserProfileDto dto);
    }
}
