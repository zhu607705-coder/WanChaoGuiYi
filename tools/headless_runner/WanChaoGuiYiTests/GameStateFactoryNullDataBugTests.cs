using System;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameStateFactory.CreateDefault dives
    /// straight into data.Emperors.Values without verifying that data
    /// is non-null. A typo in the bootstrap path, a deleted JSON
    /// table, or a unit-test seam producing null Emperors will throw
    /// NullReferenceException with no domain-meaningful message.
    ///
    /// Pinned invariant: CreateDefault should fail with a documented
    /// InvalidOperationException whose message points at the missing
    /// table, not a raw NRE that callers can't act on.
    /// </summary>
    public sealed class GameStateFactoryNullDataBugTests
    {
        private readonly ITestOutputHelper output;

        public GameStateFactoryNullDataBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void CreateDefault_With_Null_Data_Throws_Documented_Error()
        {
            Exception caught = null;
            try
            {
                GameStateFactory.CreateDefault(null, "faction_anything");
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            output.WriteLine("threw: " + (caught == null ? "<nothing>" : caught.GetType().Name + ": " + caught.Message));

            // Today: NullReferenceException, no useful message.
            Assert.NotNull(caught);
            Assert.IsType<InvalidOperationException>(caught);
        }

        [Fact]
        public void CreateDefault_With_Empty_Emperors_Throws_Documented_Error()
        {
            FakeDataRepository data = new FakeDataRepository();
            // No emperors registered.
            Exception caught = null;
            try
            {
                GameStateFactory.CreateDefault(data, "faction_anything");
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            output.WriteLine("threw: " + (caught == null ? "<nothing>" : caught.GetType().Name + ": " + caught.Message));

            // The factory should detect "no factions can be built"
            // and throw a documented InvalidOperationException, not
            // produce a half-built GameState that crashes downstream.
            Assert.NotNull(caught);
            Assert.IsType<InvalidOperationException>(caught);
        }
    }
}
