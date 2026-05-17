using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameState.ChangeRegionOwner is documented
    /// as atomic — either the transfer happens or it doesn't. In its
    /// current form:
    ///
    ///     if (previousOwner != null)
    ///         previousOwner.regionIds.RemoveAll(id => id == regionId);
    ///     newOwner.regionIds.RemoveAll(id => id == regionId);
    ///     newOwner.regionIds.Add(regionId);
    ///     region.ownerFactionId = newOwnerFactionId;
    ///
    /// If someone provides a `newOwner` that exists but has had its
    /// regionIds list set to null (e.g. through a buggy save load that
    /// didn't initialise empty collections), the RemoveAll call on
    /// null throws NullReferenceException AFTER previousOwner has
    /// already been mutated. The state is now half-transferred:
    /// previous owner lost the region, new owner doesn't have it,
    /// region.ownerFactionId still points to the previous owner.
    ///
    /// Pinned invariant: if any step throws, no observable state
    /// should have changed (transactional semantics) OR the function
    /// must defensively guard against null collections it doesn't
    /// own.
    /// </summary>
    public sealed class GameStateAtomicityBugTests
    {
        private readonly ITestOutputHelper output;

        public GameStateAtomicityBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ChangeRegionOwner_Must_Be_Atomic_When_NewOwner_Has_Null_RegionIds()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(2, out data);
            FactionState player = state.factions[0];
            int playerRegionsBefore = player.regionIds.Count;

            // Construct a malformed faction with null regionIds.
            FactionState malformed = new FactionState
            {
                id = "faction_malformed",
                name = "M",
                emperorId = "test_player",
                regionIds = null
            };
            state.factions.Add(malformed);

            string ownerBefore = state.FindRegion("r0").ownerFactionId;

            try
            {
                state.ChangeRegionOwner("r0", malformed.id);
            }
            catch (System.Exception ex)
            {
                output.WriteLine("threw " + ex.GetType().Name + ": " + ex.Message);
            }

            string ownerAfter = state.FindRegion("r0").ownerFactionId;
            int playerRegionsAfter = player.regionIds.Count;

            output.WriteLine("region.ownerFactionId before: " + ownerBefore);
            output.WriteLine("region.ownerFactionId after:  " + ownerAfter);
            output.WriteLine("player.regionIds count before: " + playerRegionsBefore);
            output.WriteLine("player.regionIds count after:  " + playerRegionsAfter);

            // Atomic: nothing changed.
            Assert.Equal(ownerBefore, ownerAfter);
            Assert.Equal(playerRegionsBefore, playerRegionsAfter);
        }
    }
}
