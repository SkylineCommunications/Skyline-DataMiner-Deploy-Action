namespace GITTest
{
    using GIT;

    using NUnit.Framework;

    using System.Threading.Tasks;

    public class GITInfoTest
    {

        [Test]
        public async Task GitInfoTest_GetCurrentBranch_HappyFlow()
        {
            GITInfo info = new GITInfo();
            var result = info.GetCurrentBranch("v1.0.20");
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task GitInfoTest_GetCommitterMail_HappyFlow()
        {
            GITInfo info = new GITInfo();
            var result = info.GetCommitterMail();
            Assert.IsNotNull(result);
        }
    }
}