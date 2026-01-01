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
        CreateMap<RegisterUserDto, User>()
            .ForMember(dest => dest.FullName,
        opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());

        CreateMap<RegisterUserDto, UserProfile>();

        CreateMap<RegisterUserDto, AgentProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentWorkload, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.EscalationLevel, opt => opt.MapFrom(_ => 1))
            .ForMember(dest => dest.Skills, opt => opt.Ignore());

        CreateMap<CreateTicketDto, Ticket>();
        CreateMap<UpdateTicketDto, Ticket>()
            .ForMember(dest => dest.TicketId, opt => opt.Ignore());

        CreateMap<Ticket, TicketResponseDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(s => s.Priority.PriorityName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(s => s.Status.StatusName))
            .ForMember(dest => dest.Owner, opt => opt.MapFrom(s => s.Owner.FullName))
            .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(s =>
                s.AssignedTo != null ? s.AssignedTo.FullName : "Unassigned"
            ));


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

        CreateMap<ErrorLogDto, ErrorLog>();
    }
}