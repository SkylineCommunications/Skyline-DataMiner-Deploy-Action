using System;
using NUnit.Framework;

namespace Package.Application.UnitTest;

[TestFixture]
public class GitLabSourceUriServiceTest
{
    private GitLabSourceUriService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new GitLabSourceUriService();
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("CI_PROJECT_URL", null);
    }

    [Test]
    public void Test_GetSourceUri_Success()
    {
        Environment.SetEnvironmentVariable("CI_PROJECT_URL", "https://gitlab.com/ziinecorp/paris-ip-flow-management");
        var uri = _service.GetSourceUri();

        Assert.IsNotNull(uri);
        Assert.AreEqual("https://gitlab.com/ziinecorp/paris-ip-flow-management", uri.AbsoluteUri);
    }

    [Test]
    public void Test_GetSourceUri_EnvNotSet()
    {
        Environment.SetEnvironmentVariable("CI_PROJECT_URL", null);

        var uri = _service.GetSourceUri();

        Assert.IsNull(uri);
    }
}