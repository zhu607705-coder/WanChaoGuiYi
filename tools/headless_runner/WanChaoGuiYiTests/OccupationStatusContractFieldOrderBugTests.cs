using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: RegionState in C# now has a property
    /// `occupationStatus` that auto-normalises contribution caps. But
    /// the JSON deserializer in NonUnityJsonDataRepository uses
    /// IncludeFields, and System.Text.Json sets fields/properties
    /// in declaration order. If a JSON save has both
    /// `occupationStatus` and `taxContributionPercent` set, the
    /// order of writes determines the final value: status sets
    /// first then contrib reads it (clamped down), or contrib first
    /// then status (clamped down). Either way the result MAY differ
    /// from what the save author intended.
    ///
    /// Pinned invariant: round-tripping a RegionState through JSON
    /// must produce the same observable state (status + contrib)
    /// as the original.
    /// </summary>
    public sealed class OccupationStatusContractFieldOrderBugTests
    {
        private readonly ITestOutputHelper output;

        public OccupationStatusContractFieldOrderBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void RegionState_RoundTrip_Json_Preserves_Status_And_Contribution()
        {
            RegionState original = new RegionState
            {
                id = "test_region",
                ownerFactionId = "p"
            };
            // Simulate a Controlled region with full contribution.
            original.occupationStatus = OccupationStatus.Controlled;
            original.integration = 100;
            original.taxContributionPercent = 100;
            original.foodContributionPercent = 100;

            System.Text.Json.JsonSerializerOptions opts = new System.Text.Json.JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = false
            };
            string json = System.Text.Json.JsonSerializer.Serialize(original, opts);
            RegionState revived = System.Text.Json.JsonSerializer.Deserialize<RegionState>(json, opts);

            output.WriteLine("original status / tax / food: " +
                original.occupationStatus + " " + original.taxContributionPercent + " " + original.foodContributionPercent);
            output.WriteLine("revived status / tax / food:  " +
                revived.occupationStatus + " " + revived.taxContributionPercent + " " + revived.foodContributionPercent);

            Assert.Equal(original.occupationStatus, revived.occupationStatus);
            Assert.Equal(original.taxContributionPercent, revived.taxContributionPercent);
            Assert.Equal(original.foodContributionPercent, revived.foodContributionPercent);
        }
    }
}
