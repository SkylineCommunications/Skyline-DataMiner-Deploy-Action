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
            var result = info.GetCurrentBranch("test");
            Assert.IsNotNull(result);
        }
    }
}