using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: IDataRepository documents four lookup
    /// methods (GetEmperor, GetRegion, GetPolicy, GetUnit) but doesn't
    /// pin their behaviour for missing ids. Some implementations
    /// throw, others return null. Domain code mixes both styles. The
    /// bug we pin: a missing region lookup should return null, but
    /// MUST also produce identical behaviour across DataRepository
    /// (Unity adapter) and NonUnityJsonDataRepository (headless
    /// adapter) and FakeDataRepository (test). If they diverge, a
    /// test that passes in headless can silently fail in Unity.
    ///
    /// We can't directly compare the three impls here (DataRepository
    /// requires Unity types), but we can pin the contract on
    /// FakeDataRepository and require it match the documented behaviour
    /// of the headless adapter (return null, no throw, no log).
    /// </summary>
    public sealed class DataRepoFakeContractBugTests
    {
        private readonly ITestOutputHelper output;

        public DataRepoFakeContractBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void GetUnknown_Returns_Null_Without_Throwing()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.RegionMap["known"] = new RegionDefinition { id = "known" };

            RegionDefinition known = data.GetRegion("known");
            RegionDefinition unknown = data.GetRegion("does_not_exist");
            EmperorDefinition unknownEmp = data.GetEmperor("does_not_exist");
            UnitDefinition unknownUnit = data.GetUnit("does_not_exist");
            PolicyDefinition unknownPol = data.GetPolicy("does_not_exist");

            output.WriteLine("known: " + (known == null ? "<null>" : known.id));
            output.WriteLine("unknown: " + (unknown == null ? "<null>" : unknown.id));

            Assert.NotNull(known);
            Assert.Null(unknown);
            Assert.Null(unknownEmp);
            Assert.Null(unknownUnit);
            Assert.Null(unknownPol);
        }
    }
}
