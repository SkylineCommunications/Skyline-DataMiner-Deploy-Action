using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Package.Application.UnitTest;

using NUnit.Framework.Legacy;

[TestFixture]
public class GitLabSourceUriServiceTest
{
    private GitLabEnvironmentVariableService _service = null!;
    private Dictionary<string, string?> _envVariables = new();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _envVariables.Add("CI_PROJECT_URL", Environment.GetEnvironmentVariable("CI_PROJECT_URL"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Environment.SetEnvironmentVariable("CI_PROJECT_URL", _envVariables["CI_PROJECT_URL"]);
    }

    [SetUp]
    public void Setup()
    {
        _service = new GitLabEnvironmentVariableService();
    }

    [Test]
    public void Test_GetSourceUri_Success()
    {
        Environment.SetEnvironmentVariable("CI_PROJECT_URL", "https://gitlab.com/ziinecorp/paris-ip-flow-management");
        var uri = _service.GetSourceUri();

        ClassicAssert.IsNotNull(uri);
        ClassicAssert.AreEqual("https://gitlab.com/ziinecorp/paris-ip-flow-management", uri.AbsoluteUri);
    }

    [Test]
    public void Test_GetSourceUri_EnvNotSet()
    {
        Environment.SetEnvironmentVariable("CI_PROJECT_URL", null);

        var uri = _service.GetSourceUri();

        ClassicAssert.IsNull(uri);
    }
}