using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GitHubAction.UnitTest
{
	using NUnit.Framework.Legacy;

	public class UtilTests
    {
        private int _total;
        private Task<int> CountUpAsync(int val)
        {
            _total += val;
            return Task.FromResult(_total);
        }
        
        [SetUp]
        public void Setup()
        {
            _total = 0;
        }

        [Test]
        public async Task ExecuteWithRetryAsync_InitialTry_HappyFlow()
        {
            // When
            await Utils.ExecuteWithRetryAsync(
                async () => await CountUpAsync(1),
                (output) => output == 1,
                Console.WriteLine,
                TimeSpan.FromMilliseconds(0),
                TimeSpan.FromMilliseconds(0),
                TimeSpan.FromMilliseconds(0));

            // Then
            ClassicAssert.AreEqual(1, _total);
        }

        [Test]
        public async Task ExecuteWithRetryAsync_MultipleAttempts_HappyFlow()
        {
            // When
            await Utils.ExecuteWithRetryAsync(
                async () => await CountUpAsync(1),
                (output) => output == 5,
                Console.WriteLine,
                TimeSpan.FromMilliseconds(1),
                TimeSpan.FromMilliseconds(2),
                TimeSpan.FromMilliseconds(6));

            // Then
            ClassicAssert.AreEqual(5, _total);
        }

        [Test]
        public void ExecuteWithRetryAsync_MultipleAttemptsButNeverValid_Timeout()
        {
            // When
            ClassicAssert.ThrowsAsync<TimeoutException>(
                async () => await Utils.ExecuteWithRetryAsync(
                    async () => await CountUpAsync(1),
                    (output) => output == 5,
                    Console.WriteLine,
                    TimeSpan.FromMilliseconds(1),
                    TimeSpan.FromMilliseconds(2),
                    TimeSpan.FromMilliseconds(5)));

            // Then
            ClassicAssert.AreEqual(4, _total);
        }
    }
}