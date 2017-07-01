using osu.Framework.Testing;
using osu.Game.Screens.AX;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCaseAx2017 : TestCase
    {
        public override string Description => "AX 2017";

        public override void Reset()
        {
            base.Reset();

            Add(new LoginScreen());
        }
    }
}