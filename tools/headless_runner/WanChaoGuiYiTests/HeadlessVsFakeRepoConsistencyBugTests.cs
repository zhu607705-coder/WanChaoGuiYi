using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: the FakeDataRepository used in xunit
    /// tests differs from NonUnityJsonDataRepository (the production
    /// headless adapter) in subtle ways. The most important: the
    /// production adapter REJECTS duplicate ids in JSON, missing ids,
    /// and missing items arrays. The fake adapter just stores whatever
    /// is added.
    ///
    /// Today this lets unit tests pass with malformed data that real
    /// headless runs would reject. We pin a contract that any test
    /// fixture intended to mirror "what production would do" must
    /// itself enforce uniqueness on insert.
    ///
    /// We can't change the public contract of IDataRepository (other
    /// real impls might rely on permissive add). We can pin that
    /// callers who want production parity should use a wrapper.
    /// This test merely DOCUMENTS the divergence — it should fail
    /// today, and someone fixing it can either tighten
    /// FakeDataRepository or add a clear seam.
    /// </summary>
    public sealed class HeadlessVsFakeRepoConsistencyBugTests
    {
        private readonly ITestOutputHelper output;

        public HeadlessVsFakeRepoConsistencyBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void FakeRepository_Should_Reject_Duplicate_Region_Ids_Like_Production()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.RegionMap["guanzhong"] = new RegionDefinition { id = "guanzhong", name = "关中" };

            // Production NonUnityJsonDataRepository.Register<T> throws
            // "Duplicate region id: guanzhong". The fake silently
            // overwrites. Pin the production behaviour.
            System.Exception caught = null;
            try
            {
                // The most common test pattern: assigning to indexer.
                // Production would block; fake silently overwrites.
                data.RegionMap["guanzhong"] = new RegionDefinition { id = "guanzhong", name = "关中-DUP" };
            }
            catch (System.Exception ex)
            {
                caught = ex;
            }

            string second = data.RegionMap["guanzhong"].name;
            output.WriteLine("second write threw? " + (caught == null ? "<no>" : caught.GetType().Name));
            output.WriteLine("after write, region name: " + second);

            // This is documenting the CURRENT divergence.  When the
            // bug is fixed (e.g. FakeRepository becomes a wrapper
            // that mirrors production), 'second' should still be
            // '关中' (first write wins) OR an exception thrown.
            //
            // Today: caught == null AND name == "关中-DUP".
            Assert.True(caught != null || second == "关中",
                "FakeRepository silently allowed a duplicate region id. " +
                "Production NonUnityJsonDataRepository rejects this. Test fixtures lie about runtime contract.");
        }
    }
}
