using ProjectManagementSystem.Core.DTOs.AI;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.Helpers;

namespace ProjectManagementSystem.Tests.Application;

public class AiCandidateRankerTests
{
    [Fact]
    public void SortSkillMatches_OrdersByProficiencyThenDesignationThenAvailability()
    {
        var candidates = new Dictionary<int, SkillMatchCandidateDto>
        {
            [1] = new()
            {
                EmployeeId = 1,
                Name = "Junior Dev",
                Designation = "JSE",
                SkillsWithProficiency = "Java (Beginner)",
                AvailabilityPercent = 100
            },
            [2] = new()
            {
                EmployeeId = 2,
                Name = "Senior Dev",
                Designation = "SSE",
                SkillsWithProficiency = "Java (Advanced)",
                AvailabilityPercent = 40
            },
            [3] = new()
            {
                EmployeeId = 3,
                Name = "Mid Dev",
                Designation = "SE",
                SkillsWithProficiency = "Java (Intermediate)",
                AvailabilityPercent = 80
            }
        };

        var matches = new List<AIMatchedEmployeeDto>
        {
            new() { EmployeeId = 1, Name = "Junior Dev", AvailabilityPercentage = 100 },
            new() { EmployeeId = 2, Name = "Senior Dev", AvailabilityPercentage = 40 },
            new() { EmployeeId = 3, Name = "Mid Dev", AvailabilityPercentage = 80 }
        };

        var sorted = AiCandidateRanker.SortSkillMatches(matches, "Java developer", candidates);

        Assert.Equal(2, sorted[0].EmployeeId);
        Assert.Equal(3, sorted[1].EmployeeId);
        Assert.Equal(1, sorted[2].EmployeeId);
    }

    [Fact]
    public void CompleteSkillMatches_IncludesAllQualifyingCandidatesNotOnlyAiPick()
    {
        var candidates = new List<SkillMatchCandidateDto>
        {
            new()
            {
                EmployeeId = 1,
                Name = "Hari Adhikari",
                Designation = "SE",
                Skills = "Dotnet",
                SkillsWithProficiency = "Dotnet (Advanced)",
                AvailabilityPercent = 50
            },
            new()
            {
                EmployeeId = 2,
                Name = "Other Dev",
                Designation = "JSE",
                Skills = "Dotnet",
                SkillsWithProficiency = "Dotnet (Intermediate)",
                AvailabilityPercent = 80
            }
        };

        var aiMatches = new List<AIMatchedEmployeeDto>
        {
            new()
            {
                EmployeeId = 1,
                Name = "Hari Adhikari",
                Reason = "AI picked Hari only."
            }
        };

        var completed = AiCandidateRanker.CompleteSkillMatches(
            aiMatches, "dotnet developer", candidates);

        Assert.Equal(2, completed.Count);
    }

    [Fact]
    public void DesignationRanker_RanksSseAboveSeAboveJse()
    {
        Assert.True(DesignationRanker.GetRank("SSE") > DesignationRanker.GetRank("SE"));
        Assert.True(DesignationRanker.GetRank("SE") > DesignationRanker.GetRank("JSE"));
    }
}
