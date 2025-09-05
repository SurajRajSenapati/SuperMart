using Xunit;
using FluentAssertions;

namespace SuperMartApp.Tests
{
    public class SampleTest
    {
        [Fact]
        public void True_ShouldBeTrue()
        {
            true.Should().BeTrue();
        }
    }
}
