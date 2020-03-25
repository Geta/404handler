using BVNetwork.NotFound.Core;
using Xunit;

namespace BVNetwork.NotFound.Tests
{
    public class ColorHelperTests
    {
        [Fact]
        public void GetRedTone_returns_00_when_maxValue_minValue_and_value_are_0()
        {
            var actual = ColorHelper.GetRedTone(0, 0, 0);

            Assert.Equal("00", actual);
        }
    }
}