using System.Security.Cryptography;
using System.Text;

using AutoMapper;

using SmartTicketSystem.API.Events;
using SmartTicketSystem.Application.DTOs.Auth;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Domain.Enums;
using SmartTicketSystem.Infrastructure.Events;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _repo;
    private readonly IAgentRepository _agentRepo;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwt;
    private readonly IEventQueue _eventQueue;

    public AuthService(
        IAuthRepository repo,
        IAgentRepository agentRepo,
        IMapper mapper,
        IJwtService jwt,
        IEventQueue eventQueue)
    {
        _repo = repo;
        _agentRepo = agentRepo;
        _mapper = mapper;
        _jwt = jwt;
        _eventQueue = eventQueue;
    }

    public async Task<string> Register(RegisterUserDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var existing = await _repo.GetByEmail(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("User already exists");

        CreatePassword(dto.Password, out var hash, out var salt);

        var requiresApproval =
            dto.RoleId == (int)UserRoleEnum.SupportAgent ||
            dto.RoleId == (int)UserRoleEnum.SupportManager;

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.IsActive = !requiresApproval;
        user.CreatedAt = DateTime.UtcNow;

        user.UserRole = new UserRole
        {
            RoleId = dto.RoleId
        };

        await _repo.AddUser(user);
        await _repo.Save();

        if (dto.RoleId == (int)UserRoleEnum.SupportAgent)
        {
            await CreateAgentProfileAsync(user.Id, dto.CategorySkillIds);
        }

        await _eventQueue.PublishAsync(
            new UserRegisteredEvent(
                user.Id,
                user.Email,
                user.FullName,
                dto.RoleId,
                requiresApproval
            )
        );

        return "Registered Successfully";
    }

    public async Task<AuthResponseDto?> Login(LoginDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var user = await _repo.GetByEmailWithRole(dto.Email);

        if (user == null ||
            !VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
            return null;

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account pending admin approval");

        return new AuthResponseDto
        {
            Name = user.FullName,
            Role = user.UserRole.Role.RoleName,
            Token = _jwt.GenerateToken(user)
        };
    }

    private async Task CreateAgentProfileAsync(Guid userId, List<int>? skillIds)
    {
        var profile = new AgentProfile
        {
            UserId = userId,
        };

        await _agentRepo.AddProfileAsync(profile);

        if (skillIds?.Any() == true)
        {
            foreach (var categoryId in skillIds)
            {
                await _agentRepo.AddSkillAsync(new AgentCategorySkill
                {
                    AgentProfileId = profile.Id,
                    CategoryId = categoryId
                });
            }
        }

        await _agentRepo.SaveAsync();
    }

    private static void CreatePassword(string password, out byte[] hash, out byte[] salt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty");

        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private static bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(hash);
    }
}
