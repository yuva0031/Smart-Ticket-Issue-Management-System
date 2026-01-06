using AutoMapper;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.DTOs.AddTicketCommentDto;
using SmartTicketSystem.Application.DTOs.Auth;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.UserRole.Role.RoleName));

        CreateMap<UserProfile, UserProfileDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.User.Email));


        CreateMap<RegisterUserDto, UserProfile>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<RegisterUserDto, User>()
            .ForMember(dest => dest.FullName,
        opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());

        CreateMap<RegisterUserDto, AgentProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentWorkload, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.Skills, opt => opt.Ignore());

        CreateMap<AgentCategorySkill, AgentSkillDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<AgentProfile, AgentProfileDto>()
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills));

        CreateMap<CreateTicketDto, Ticket>();

        CreateMap<UpdateTicketRequestDto, Ticket>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Ticket, TicketResponseDto>()
                    .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                        src.Category != null ? src.Category.Name : "Uncategorized"))

                    .ForMember(dest => dest.Priority, opt => opt.MapFrom(src =>
                        src.Priority != null ? src.Priority.PriorityName : "Normal"))

                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                        src.Status != null ? src.Status.StatusName : "Open"))

                    .ForMember(dest => dest.Owner, opt => opt.MapFrom(src =>
                        src.Owner != null ? src.Owner.FullName : "Unknown"))

                    .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src =>
                        src.AssignedTo != null ? src.AssignedTo.FullName : "Unassigned"));

        CreateMap<CreateTicketHistoryDto, TicketHistory>()
            .ForMember(dest => dest.ChangedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<TicketComment, CommentResponse>();

        CreateMap<AddCommentRequest, TicketComment>()
            .ForMember(dest => dest.CommentId, opt => opt.Ignore())
            .ForMember(dest => dest.Ticket, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        CreateMap<AddTicketHistoryDto, TicketHistory>()
            .ForMember(dest => dest.HistoryId, opt => opt.Ignore())
            .ForMember(dest => dest.ChangedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Ticket, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());


        CreateMap<TicketHistory, TicketHistoryResponseDto>()
            .ForMember(dest => dest.ModifiedByName,
                       opt => opt.MapFrom(src => src.User.FullName));

        CreateMap<TicketCategory, TicketCategoryDto>();
        CreateMap<TicketPriority, TicketPriorityDto>();
        CreateMap<TicketStatus, TicketStatusDto>();

        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Channel,
                opt => opt.MapFrom(n => n.Channel.ToString()));
    }
}