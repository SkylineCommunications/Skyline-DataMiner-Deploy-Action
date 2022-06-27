using System;
using NUnit.Framework;

namespace Package.Application.UnitTest;

[TestFixture]
public class GitHubSourceUriServiceTest
{
    private GitHubSourceUriService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new GitHubSourceUriService();
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
        Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", null);
    }

    [Test]
    public void Test_GetSourceUri_Success()
    {
        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com/");
        Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", "ZiineCorp/Paris-IP-Flow-Management");
        var uri = _service.GetSourceUri();

        Assert.IsNotNull(uri);
        Assert.AreEqual("https://github.com/ZiineCorp/Paris-IP-Flow-Management", uri.AbsoluteUri);
    }

    [Test]
    public void Test_GetSourceUri_EnvNotSet()
    {
        var uri = _service.GetSourceUri();

        Assert.IsNull(uri);
    }
}