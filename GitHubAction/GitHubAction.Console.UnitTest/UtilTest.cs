namespace GitHubAction.Console.UnitTest
{
    public class UtilTest
    {
        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", null);
            Environment.SetEnvironmentVariable("CI_PROJECT_URL", null);
        }

        [Test]
        public void Test_GetSourceHost_GitHub()
        {
            // When
            Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "SomeGitHubUrl");

            // Then
            Assert.AreEqual(Util.SourceHost.GitHub, Util.GetSourceHost());
        }

        [Test]
        public void Test_GetSourceHost_GitLab()
        {
            // When
            Environment.SetEnvironmentVariable("CI_PROJECT_URL", "SomeGitLabUrl");

            // Then
            Assert.AreEqual(Util.SourceHost.GitLab, Util.GetSourceHost());
        }

        [Test]
        public void Test_GetSourceHost_Default()
        {
            // When Nothing

            // Then
            Assert.AreEqual(Util.SourceHost.Undefined, Util.GetSourceHost());
        }
    }
}