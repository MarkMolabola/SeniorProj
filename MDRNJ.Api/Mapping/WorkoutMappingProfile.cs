using AutoMapper;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Shared.DTOs.Workout;

namespace MDRNJ.Api.Mapping
{
    public class WorkoutMappingProfile : Profile
    {
        public WorkoutMappingProfile()
        {
            CreateMap<WorkoutSessionEntity, WorkoutDto>()
                .ForMember(d => d.WorkoutType, o => o.MapFrom(s => s.WorkoutType.ToString()));
            CreateMap<ExerciseEntity, ExerciseDto>();
        }
    }
}
