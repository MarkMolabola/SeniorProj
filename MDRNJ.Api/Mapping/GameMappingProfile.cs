using AutoMapper;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Shared.DTOs.Game;

namespace MDRNJ.Api.Mapping
{
    public class GameMappingProfile : Profile
    {
        public GameMappingProfile()
        {
            CreateMap<GameStatsEntity, GameStatsDto>()
                .ForMember(d => d.XPToNextLevel, o => o.Ignore());
            CreateMap<XPTransactionEntity, XPTransactionDto>()
                .ForMember(d => d.Reason, o => o.MapFrom(s => s.Reason.ToString()));
            CreateMap<AchievementEntity, AchievementDto>()
                .ForMember(d => d.Rarity, o => o.MapFrom(s => s.Rarity.ToString()))
                .ForMember(d => d.UnlockedAt, o => o.Ignore());
        }
    }
}
