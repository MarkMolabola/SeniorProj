using AutoMapper;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Shared.DTOs.Challenge;
using MDRNJ.Shared.Enums;

namespace MDRNJ.Api.Mapping
{
    public class ChallengeMappingProfile : Profile
    {
        public ChallengeMappingProfile()
        {
            CreateMap<ChallengeParticipantEntity, ChallengeParticipantDto>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.User.Username))
                .ForMember(d => d.IsCompleted, o => o.MapFrom(s => s.Status == ChallengeStatus.Completed));
        }
    }
}
