namespace GitHubAction.Console.UnitTest
{
	using NUnit.Framework.Legacy;

	public class UtilTest
    {
        private Dictionary<string, string?> _envVariables = new();

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _envVariables.Add("GITHUB_SERVER_URL", Environment.GetEnvironmentVariable("GITHUB_SERVER_URL"));
            _envVariables.Add("GITHUB_REPOSITORY", Environment.GetEnvironmentVariable("GITHUB_REPOSITORY"));
            _envVariables.Add("CI_PROJECT_URL", Environment.GetEnvironmentVariable("CI_PROJECT_URL"));

            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
            Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", null);
            Environment.SetEnvironmentVariable("CI_PROJECT_URL", null);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", _envVariables["GITHUB_SERVER_URL"]);
            Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", _envVariables["GITHUB_REPOSITORY"]);
            Environment.SetEnvironmentVariable("CI_PROJECT_URL", _envVariables["CI_PROJECT_URL"]);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
            Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", null);
            Environment.SetEnvironmentVariable("CI_PROJECT_URL", null);
        }

        [Test]
        public void Test_GetSourceHost_GitHub()
        {
            // When
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "SomeGitHubUrl");

            // Then
            ClassicAssert.AreEqual(Util.SourceHost.GitHub, Util.GetSourceHost());
        }

        [Test]
        public void Test_GetSourceHost_GitLab()
        {
            // When
            Environment.SetEnvironmentVariable("CI_PROJECT_URL", "SomeGitLabUrl");

            // Then
            ClassicAssert.AreEqual(Util.SourceHost.GitLab, Util.GetSourceHost());
        }

        [Test]
        public void Test_GetSourceHost_Default()
        {
            // When Nothing

            // Then
            ClassicAssert.AreEqual(Util.SourceHost.Undefined, Util.GetSourceHost());
        }
    }
}