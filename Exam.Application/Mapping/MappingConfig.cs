using AutoMapper;
using Exam.Application.Dto.Identity;
using Exam.Domain.Entities.Identity;

namespace Exam.Application.Mapping
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<CreateUser, AppUser>()
                .ForMember(dest => dest.UserName,
                           opt => opt.MapFrom(src => src.Email));
        }
    }
}
