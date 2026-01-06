using AutoMapper;

using Moq;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Services.Implementations;

using Xunit;

namespace SmartTicketSystem.Tests.service;

public class UserProfileServiceUnitTests
{
    private readonly Mock<IUserProfileRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UserProfileService _service;

    public UserProfileServiceUnitTests()
    {
        _mockRepo = new Mock<IUserProfileRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new UserProfileService(_mockRepo.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetMyProfileAsync_ExistingUser_ReturnsMappedProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new UserProfile { UserId = userId, PhoneNumber = "123456" };
        var expectedDto = new UserProfileDto { PhoneNumber = "123456" };

        _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(profile);
        _mockMapper.Setup(m => m.Map<UserProfileDto>(profile)).Returns(expectedDto);

        // Act
        var result = await _service.GetMyProfileAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123456", result.PhoneNumber);
        _mockRepo.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetMyProfileAsync_NonExistentUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((UserProfile?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetMyProfileAsync(userId));
        Assert.Contains(userId.ToString(), ex.Message);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_ValidDto_UpdatesPropertiesAndSaves()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingProfile = new UserProfile { UserId = userId, PhoneNumber = "Old", Address = "Old" };
        var updateDto = new UpdateUserProfileDto { PhoneNumber = "New", Address = "New" };

        _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingProfile);

        // Act
        await _service.UpdateMyProfileAsync(userId, updateDto);

        // Assert
        Assert.Equal("New", existingProfile.PhoneNumber);
        Assert.Equal("New", existingProfile.Address);
        _mockRepo.Verify(r => r.UpdateAsync(existingProfile), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_NullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.UpdateMyProfileAsync(Guid.NewGuid(), null!));
    }

    [Fact]
    public async Task UpdateMyProfileAsync_NonExistentUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((UserProfile?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateMyProfileAsync(userId, new UpdateUserProfileDto()));
        
        _mockRepo.Verify(r => r.SaveAsync(), Times.Never);
    }

    [Theory]
    [InlineData("", "New Address")]
    [InlineData("12345", "")]
    [InlineData(null, null)]
    public async Task UpdateMyProfileAsync_PartialOrNullDataInDto_UpdatesAccordingly(string? phone, string? address)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new UserProfile { UserId = userId, PhoneNumber = "Original", Address = "Original" };
        var dto = new UpdateUserProfileDto { PhoneNumber = phone, Address = address };

        _mockRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(profile);

        // Act
        await _service.UpdateMyProfileAsync(userId, dto);

        // Assert
        Assert.Equal(phone, profile.PhoneNumber);
        Assert.Equal(address, profile.Address);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }
}