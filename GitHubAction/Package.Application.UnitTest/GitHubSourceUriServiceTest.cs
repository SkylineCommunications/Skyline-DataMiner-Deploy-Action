using System;
using System.Collections.Generic;
using System.Security;
using NUnit.Framework;

namespace Package.Application.UnitTest;

[TestFixture]
public class GitHubSourceUriServiceTest
{
    private GitHubEnvironmentVariableService _service = null!;
    private Dictionary<string, string?> _envVariables = new();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _envVariables.Add("GITHUB_SERVER_URL", Environment.GetEnvironmentVariable("GITHUB_SERVER_URL"));
        _envVariables.Add("GITHUB_REPOSITORY", Environment.GetEnvironmentVariable("GITHUB_REPOSITORY"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", _envVariables["GITHUB_SERVER_URL"]);
        Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", _envVariables["GITHUB_REPOSITORY"]);
    }


    [SetUp]
    public void Setup()
    {
        _service = new GitHubEnvironmentVariableService();
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
        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
        Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", null);

        var uri = _service.GetSourceUri();

        Assert.IsNull(uri);
    }
}