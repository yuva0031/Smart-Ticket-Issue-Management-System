using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Domain.Enums;
using SmartTicketSystem.Infrastructure.Services.Implementations;

using Xunit;

namespace SmartTicketSystem.Tests.service;

public class LookupServiceUnitTests
{
    private readonly LookupService _service;

    public LookupServiceUnitTests()
    {
        _service = new LookupService();
    }

    [Theory]
    [InlineData(UserRoleEnum.SupportAgent, "Support Agent")]
    [InlineData(UserRoleEnum.Admin, "Admin")]
    [InlineData(UserRoleEnum.SupportManager, "Support Manager")]
    public async Task GetRolesAsync_FormatsPascalCaseToFriendlyNames(UserRoleEnum role, string expectedName)
    {
        // Act
        var result = await _service.GetRolesAsync();
        var item = result.FirstOrDefault(x => x.Id == (int)role);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(expectedName, item.Name);
        Assert.Equal(role.ToString(), item.Code);
    }

    [Fact]
    public async Task GetStatusesAsync_CheckCompleteness_ReturnsAllValues()
    {
        // Arrange
        var enumValues = Enum.GetValues<TicketStatusEnum>();

        // Act
        var result = await _service.GetStatusesAsync();

        // Assert
        Assert.Equal(enumValues.Length, result.Count());
        foreach (var value in enumValues)
        {
            Assert.Contains(result, x => x.Id == (int)value && x.Code == value.ToString());
        }
    }

    [Fact]
    public async Task GetPrioritiesAsync_IDsMatchEnumValue()
    {
        // Act
        var result = await _service.GetPrioritiesAsync();

        // Assert
        var highPriority = result.FirstOrDefault(x => x.Code == TicketPriorityEnum.High.ToString());
        Assert.NotNull(highPriority);
        Assert.Equal((int)TicketPriorityEnum.High, highPriority.Id);
    }

    [Fact]
    public async Task LookupMethods_ReturnUniqueIds()
    {
        // Act
        var roles = await _service.GetRolesAsync();
        var categories = await _service.GetCategoriesAsync();

        // Assert
        Assert.Equal(roles.Count(), roles.Select(r => r.Id).Distinct().Count());
        Assert.Equal(categories.Count(), categories.Select(c => c.Id).Distinct().Count());
    }

    [Fact]
    public async Task GetCategoriesAsync_ProjectedNamesAreNotNullOrEmpty()
    {
        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        Assert.All(result, item => {
            Assert.NotEmpty(item.Name);
            Assert.NotEmpty(item.Code);
            Assert.Equal(item.Name.Trim(), item.Name); 
        });
    }
}