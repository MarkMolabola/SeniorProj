using AutoMapper;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Shared.DTOs.Character;

namespace MDRNJ.Api.Mapping
{
    public class CharacterMappingProfile : Profile
    {
        public CharacterMappingProfile()
        {
            CreateMap<CharacterStateEntity, CharacterStateDto>()
                .ForMember(d => d.BodyType, o => o.MapFrom(s => (int)s.BodyType))
                .ForMember(d => d.SpriteConfig, o => o.MapFrom(s => new SpriteConfigDto
                {
                    BodySpritePath = $"sprites/body/{s.BodyType}",
                    OutfitSpritePath = $"sprites/outfit/{s.OutfitSlot}",
                    EquipmentSpritePath = $"sprites/equipment/{s.EquipmentSlot}",
                    AccessorySpritePath = s.AccessorySlot != null ? $"sprites/accessory/{s.AccessorySlot}" : null
                }));

            CreateMap<ActivityScoreEntity, ActivityScoreDto>()
                .ForMember(d => d.DominantSport, o => o.MapFrom(s => s.DominantSport.ToString()))
                .ForMember(d => d.UnspentStatPoints, o => o.Ignore());
        }
    }
}
