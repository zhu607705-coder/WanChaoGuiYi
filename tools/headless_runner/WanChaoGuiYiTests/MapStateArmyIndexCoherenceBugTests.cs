using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapState maintains two views of every
    /// army's location:
    ///   ArmiesById[armyId].locationRegionId
    ///   armyIdsByRegionId[regionId] -> List of armyIds
    /// The two are kept in sync by RemoveArmyLocation / IndexArmyLocation.
    /// But:
    ///   - If an ArmyRuntimeState's locationRegionId is mutated DIRECTLY
    ///     (not via MoveArmyToRegion), the reverse index goes stale.
    ///   - RemoveArmy calls RemoveArmyLocation(armyId, army.locationRegionId)
    ///     using the CURRENT location, so any direct prior mutation
    ///     leaks the army into the OLD region's army list and never
    ///     cleans it.
    ///
    /// Several subsystems do mutate locationRegionId directly today —
    /// for example GameStateFactory.CreateInitialArmies sets it during
    /// construction, the post-battle rout path in DomainMapWarResolution
    /// calls mapState.MoveArmyToRegion(...) but ALSO assigns
    /// legacyArmy.regionId = retreatRegionId on the legacy state, and
    /// sufficiently old saves may carry stale runtime locations.
    ///
    /// Pinned invariant (chosen because it should always hold whether
    /// or not callers misbehave): for every army in ArmiesById, the
    /// reverse index armyIdsByRegionId[army.locationRegionId] must
    /// contain the army's id; and no other region's list may contain it.
    /// </summary>
    public sealed class MapStateArmyIndexCoherenceBugTests
    {
        private readonly ITestOutputHelper output;

        public MapStateArmyIndexCoherenceBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Reverse_Index_Must_Stay_Coherent_After_Direct_Location_Mutation()
        {
            MapState map = new MapState();
            map.AddRegion(new RegionRuntimeState { id = "home", ownerFactionId = "p" });
            map.AddRegion(new RegionRuntimeState { id = "front", ownerFactionId = "p" });

            ArmyRuntimeState army = new ArmyRuntimeState
            {
                id = "ghost",
                ownerFactionId = "p",
                locationRegionId = "home",
                task = ArmyTask.Idle
            };
            map.AddArmy(army);

            // Simulate a subsystem that updates the army's location
            // directly (which production code paths historically do)
            // without using MoveArmyToRegion.
            army.locationRegionId = "front";

            // Now ask the map who's in each region.  The reverse index
            // is now stale: it still has 'ghost' at 'home'.
            List<ArmyRuntimeState> atHome = map.GetArmiesInRegion("home");
            List<ArmyRuntimeState> atFront = map.GetArmiesInRegion("front");

            output.WriteLine("atHome count:  " + atHome.Count);
            output.WriteLine("atFront count: " + atFront.Count);

            // The desired invariant: the army's reported location and
            // its presence in the reverse index must agree.  Either
            // GetArmiesInRegion should resolve via ArmiesById and not
            // trust the cached list, OR locationRegionId should be a
            // private setter that forces re-indexing.
            bool atFrontConsistent = atFront.Exists(a => a.id == army.id);
            bool homeIsClean = !atHome.Exists(a => a.id == army.id);

            Assert.True(atFrontConsistent && homeIsClean,
                "MapState reverse index disagrees with ArmyRuntimeState.locationRegionId. " +
                "atFront contains ghost? " + atFrontConsistent +
                "; home cleaned? " + homeIsClean);
        }
    }
}
