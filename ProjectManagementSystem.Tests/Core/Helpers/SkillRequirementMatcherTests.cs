using ProjectManagementSystem.Core.Helpers;

namespace ProjectManagementSystem.Tests.Core.Helpers;

public class SkillRequirementMatcherTests
{
    [Fact]
    public void ScoreProfile_MatchesTypeScriptKeyword()
    {
        var score = SkillRequirementMatcher.ScoreProfile(
            "need someone with typescript experience",
            "TypeScript, React, Node.js",
            "Engineering",
            "API development");

        Assert.True(score > 0);
    }

    [Fact]
    public void ScoreProfile_ReturnsZeroWhenNoKeywordOverlap()
    {
        var score = SkillRequirementMatcher.ScoreProfile(
            "need java backend developer",
            "Python, Django",
            "QA",
            "manual testing");

        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoreProfile_MatchesRecentActivity()
    {
        var score = SkillRequirementMatcher.ScoreProfile(
            "kubernetes devops",
            "Linux",
            "DevOps",
            "Kubernetes, CI/CD");

        Assert.True(score > 0);
    }

    [Fact]
    public void ExtractKeywords_IgnoresStopWords()
    {
        var keywords = SkillRequirementMatcher.ExtractKeywords("I need a developer with java");

        Assert.Contains(keywords, k => k.Equals("java", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(keywords, k => k.Equals("need", StringComparison.OrdinalIgnoreCase));
    }
}
