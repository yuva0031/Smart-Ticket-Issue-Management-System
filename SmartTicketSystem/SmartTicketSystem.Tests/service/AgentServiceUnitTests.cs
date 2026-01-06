using Moq;
using Xunit;
using AutoMapper;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Services.Implementations;
using SmartTicketSystem.Application.Mapping; 

namespace SmartTicketSystem.Tests.Services;

public class AgentServiceUnitTests
{
    private readonly Mock<IAgentRepository> _mockRepo;
    private readonly IMapper _mapper;
    private readonly AgentService _service;

    public AgentServiceUnitTests()
    {
        _mockRepo = new Mock<IAgentRepository>();
        
        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<AgentProfile, AgentProfileDto>();
            cfg.CreateMap<AgentCategorySkill, AgentCategorySkillDto>();
        });
        _mapper = config.CreateMapper();

        _service = new AgentService(_mockRepo.Object, _mapper);
    }

    [Fact]
    public async Task AddCategorySkill_ValidRequest_ReturnsSuccessMessage()
    {
        var agentId = Guid.NewGuid();
        var dto = new AddAgentSkillDto { AgentProfileId = agentId, CategoryId = 10 };
        var agentProfile = new AgentProfile { Id = agentId, Skills = new List<AgentCategorySkill>() };
        
        _mockRepo.Setup(r => r.GetByIdAsync(agentId, true)).ReturnsAsync(agentProfile);
        _mockRepo.Setup(r => r.CategoryExistsAsync(dto.CategoryId)).ReturnsAsync(true);

        var result = await _service.AddCategorySkill(dto);

        Assert.Equal("Skill added successfully", result);
        Assert.Single(agentProfile.Skills);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCategorySkill_CategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        var dto = new AddAgentSkillDto { AgentProfileId = Guid.NewGuid(), CategoryId = 999 };
        var agentProfile = new AgentProfile { Id = dto.AgentProfileId, Skills = new List<AgentCategorySkill>() };

        _mockRepo.Setup(r => r.GetByIdAsync(dto.AgentProfileId, true)).ReturnsAsync(agentProfile);
        _mockRepo.Setup(r => r.CategoryExistsAsync(dto.CategoryId)).ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AddCategorySkill(dto));
    }

    [Fact]
    public async Task RemoveCategorySkill_SkillExists_RemovesAndSaves()
    {
        var agentId = Guid.NewGuid();
        var categoryId = 5;
        var dto = new RemoveAgentSkillDto { AgentProfileId = agentId, CategoryId = categoryId };
        var agentProfile = new AgentProfile { 
            Id = agentId, 
            Skills = new List<AgentCategorySkill> { new AgentCategorySkill { CategoryId = categoryId } } 
        };

        _mockRepo.Setup(r => r.GetByIdAsync(agentId, true)).ReturnsAsync(agentProfile);

        var result = await _service.RemoveCategorySkill(dto);

        Assert.Equal("Skill removed successfully", result);
        Assert.Empty(agentProfile.Skills);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveCategorySkill_SkillDoesNotExist_ReturnsNotFoundMessage()
    {
        var dto = new RemoveAgentSkillDto { AgentProfileId = Guid.NewGuid(), CategoryId = 1 };
        var agentProfile = new AgentProfile { Id = dto.AgentProfileId, Skills = new List<AgentCategorySkill>() };

        _mockRepo.Setup(r => r.GetByIdAsync(dto.AgentProfileId, true)).ReturnsAsync(agentProfile);

        var result = await _service.RemoveCategorySkill(dto);

        Assert.Equal("Skill not found", result);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAgentSkills_InvalidCategoryInList_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var categoryIds = new List<int> { 1, 2, 3 }; 
        var agentProfile = new AgentProfile { Id = Guid.NewGuid() };

        _mockRepo.Setup(r => r.GetByUserIdAsync(userId, true)).ReturnsAsync(agentProfile);
        _mockRepo.Setup(r => r.GetCategoriesByIdsAsync(categoryIds))
                 .ReturnsAsync(new List<Category> { new Category(), new Category() });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAgentSkills(userId, categoryIds));
    }

    [Fact]
    public async Task UpdateAgentSkills_EmptyList_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var emptyList = new List<int>();

        _mockRepo.Setup(r => r.GetByUserIdAsync(userId, true)).ReturnsAsync(new AgentProfile());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAgentSkills(userId, emptyList));
    }

    [Fact]
    public async Task GetAgentProfileById_WhenNotExists_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((AgentProfile?)null);

        var result = await _service.GetAgentProfileById(Guid.NewGuid());

        Assert.Null(result);
    }
}