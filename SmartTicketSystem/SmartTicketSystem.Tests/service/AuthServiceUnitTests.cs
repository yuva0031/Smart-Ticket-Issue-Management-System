using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Moq;
using SmartTicketSystem.API.Events;
using SmartTicketSystem.Application.DTOs.Auth;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Domain.Enums;
using SmartTicketSystem.Infrastructure.Services.Implementations;
using SmartTicketSystem.Infrastructure.Events;

using Xunit;

namespace SmartTicketSystem.Tests.service;

public class AuthServiceUnitTests
{
    private readonly Mock<IAuthRepository> _mockAuthRepo;
    private readonly Mock<IAgentRepository> _mockAgentRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IJwtService> _mockJwt;
    private readonly Mock<IEventQueue> _mockEventQueue;
    private readonly AuthService _service;

    public AuthServiceUnitTests()
    {
        _mockAuthRepo = new Mock<IAuthRepository>();
        _mockAgentRepo = new Mock<IAgentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockJwt = new Mock<IJwtService>();
        _mockEventQueue = new Mock<IEventQueue>();

        _service = new AuthService(
            _mockAuthRepo.Object,
            _mockAgentRepo.Object,
            _mockMapper.Object,
            _mockJwt.Object,
            _mockEventQueue.Object
        );
    }

    [Fact]
    public async Task Register_UserExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new RegisterUserDto { Email = "duplicate@test.com" };
        _mockAuthRepo.Setup(r => r.GetByEmail(dto.Email)).ReturnsAsync(new User());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Register(dto));
        Assert.Equal("User already exists", ex.Message);
    }

    [Fact]
    public async Task Register_SupportAgent_CreatesAgentProfileAndSkills()
    {
        // Arrange
        var dto = new RegisterUserDto 
        { 
            Email = "agent@test.com", 
            Password = "SecurePassword123", 
            RoleId = (int)UserRoleEnum.SupportAgent,
            CategorySkillIds = new List<int> { 1, 2 }
        };
        var user = new User { Id = Guid.NewGuid(), Email = dto.Email };

        _mockAuthRepo.Setup(r => r.GetByEmail(dto.Email)).ReturnsAsync((User?)null);
        _mockMapper.Setup(m => m.Map<User>(dto)).Returns(user);

        // Act
        var result = await _service.Register(dto);

        // Assert
        Assert.False(user.IsActive); 
        _mockAgentRepo.Verify(r => r.AddProfileAsync(It.Is<AgentProfile>(p => p.UserId == user.Id)), Times.Once);
        _mockAgentRepo.Verify(r => r.AddSkillAsync(It.IsAny<AgentCategorySkill>()), Times.Exactly(2));
        _mockAgentRepo.Verify(r => r.SaveAsync(), Times.Once);
        _mockEventQueue.Verify(e => e.PublishAsync(It.IsAny<UserRegisteredEvent>()), Times.Once);
    }

    [Fact]
    public async Task Register_SupportManager_RequiresApprovalButNoAgentProfile()
    {
        // Arrange
        var dto = new RegisterUserDto { Email = "mgr@test.com", Password = "Password123", RoleId = (int)UserRoleEnum.SupportManager };
        var user = new User { Id = Guid.NewGuid() };
        _mockAuthRepo.Setup(r => r.GetByEmail(dto.Email)).ReturnsAsync((User?)null);
        _mockMapper.Setup(m => m.Map<User>(dto)).Returns(user);

        // Act
        await _service.Register(dto);

        // Assert
        Assert.False(user.IsActive); 
        _mockAgentRepo.Verify(r => r.AddProfileAsync(It.IsAny<AgentProfile>()), Times.Never);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var dto = new LoginDto { Email = "user@test.com", Password = "WrongPassword" };
        var salt = new byte[128]; 
        var hash = new byte[64];  
        var user = new User { PasswordSalt = salt, PasswordHash = hash };

        _mockAuthRepo.Setup(r => r.GetByEmailWithRole(dto.Email)).ReturnsAsync(user);

        // Act
        var result = await _service.Login(dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsNull()
    {
        // Arrange
        _mockAuthRepo.Setup(r => r.GetByEmailWithRole(It.IsAny<string>())).ReturnsAsync((User?)null);

        // Act
        var result = await _service.Login(new LoginDto { Email = "none@test.com", Password = "123" });

        // Assert
        Assert.Null(result);
    }

    private (byte[] hash, byte[] salt) GeneratePasswordMeta(string password)
    {
        using var hmac = new HMACSHA512();
        return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
    }
}