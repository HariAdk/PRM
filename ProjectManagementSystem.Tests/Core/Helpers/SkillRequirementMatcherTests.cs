using ProjectManagementSystem.Core.DTOs.AI;
using ProjectManagementSystem.Core.Helpers;

namespace ProjectManagementSystem.Tests.Core.Helpers;

public class SkillRequirementMatcherTests
{
    [Fact]
    public void TryParseMinAvailabilityPercent_Parses100Percent()
    {
        var pct = SkillRequirementMatcher.TryParseMinAvailabilityPercent(
            "I want a developer with javascript domain and 100% availability");

        Assert.Equal(100, pct);
    }

    [Fact]
    public void ScoreProfile_MatchesJavaScriptKeyword()
    {
        var score = SkillRequirementMatcher.ScoreProfile(
            "javascript developer",
            "JavaScript, HTML, CSS",
            "Engineering",
            "Frontend development");

        Assert.True(score > 0);
    }

    [Fact]
    public void ScoreProfile_DoesNotMatchDotnetForJavaScript()
    {
        var score = SkillRequirementMatcher.ScoreProfile(
            "javascript developer",
            "Dotnet, React",
            "Engineering",
            "Backend API Development");

        Assert.Equal(0, score);
    }

    [Fact]
    public void MeetsRequirement_RejectsLowAvailability()
    {
        var requirement = "javascript developer with 100% availability";
        var candidate = new SkillMatchCandidateDto
        {
            EmployeeId = 1,
            Name = "Test",
            Skills = "JavaScript",
            AvailabilityPercent = 10
        };

        Assert.False(SkillRequirementMatcher.MeetsRequirement(requirement, candidate));
    }

    [Fact]
    public void MeetsRequirement_AcceptsMatchingSkillAndAvailability()
    {
        var requirement = "javascript developer with 100% availability";
        var candidate = new SkillMatchCandidateDto
        {
            EmployeeId = 1,
            Name = "Test",
            Skills = "JavaScript",
            AvailabilityPercent = 100
        };

        Assert.True(SkillRequirementMatcher.MeetsRequirement(requirement, candidate));
    }

    [Fact]
    public void MeetsRequirement_RejectsWrongSkillEvenWithAvailability()
    {
        var requirement = "javascript developer with 80% availability";
        var candidate = new SkillMatchCandidateDto
        {
            EmployeeId = 1,
            Name = "Test",
            Skills = "Dotnet, React",
            AvailabilityPercent = 80
        };

        Assert.False(SkillRequirementMatcher.MeetsRequirement(requirement, candidate));
    }

    [Fact]
    public void ExtractSkillKeywords_ExcludesAvailabilityTokens()
    {
        var keywords = SkillRequirementMatcher.ExtractSkillKeywords(
            "javascript developer 100% availability");

        Assert.Contains(keywords, k => k.Equals("javascript", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(keywords, k => k.Equals("100", StringComparison.Ordinal));
        Assert.DoesNotContain(keywords, k => k.Equals("availability", StringComparison.OrdinalIgnoreCase));
    }
}
