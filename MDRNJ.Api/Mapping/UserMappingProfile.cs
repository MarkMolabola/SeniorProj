using AutoMapper;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Shared.DTOs.User;

namespace MDRNJ.Api.Mapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserEntity, UserDto>();
            CreateMap<UserProfileEntity, UserProfileDto>()
                .ForMember(d => d.FitnessGoal, o => o.MapFrom(s => s.FitnessGoal.ToString()));
        }
    }
}
