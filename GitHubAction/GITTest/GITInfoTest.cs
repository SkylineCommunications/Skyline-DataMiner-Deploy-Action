namespace GITTest
{
    using System;
    using System.Threading.Tasks;

    using GIT;

    using NUnit.Framework;

    public class GITInfoTest
    {

        [Test]
        public async Task GitInfoTest_GetCurrentBranch_HappyFlow()
        {
            GitInfo info = new GitInfo();
            info.Initialize(String.Empty);
            var result = info.GetCurrentBranch("v1.0.20");
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task GitInfoTest_GetCommitterMail_HappyFlow()
        {
            GitInfo info = new GitInfo();
            info.Initialize(String.Empty);
            var result = info.GetCommitterMail();
            Assert.IsNotNull(result);
        }
    }
}