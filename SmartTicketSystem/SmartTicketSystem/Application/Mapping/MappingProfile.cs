using AutoMapper;

using SmartTicketSystem.Application.DTOs.Auth;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterUserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

        CreateMap<RegisterUserDto, UserProfile>();
    }
}