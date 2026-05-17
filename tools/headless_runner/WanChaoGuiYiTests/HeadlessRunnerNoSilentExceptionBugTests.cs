using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: HeadlessSimulationRunner has multiple
    /// scenarios that swallow exceptions silently in their setup
    /// helpers (try/catch with empty catch). The risk: a refactor
    /// breaks initialisation, the scenario reports Pass with
    /// turnsExecuted=0 but no failure reason.
    ///
    /// Pinned invariant: HeadlessSimulationRunner.cs must contain no
    /// "catch { }" empty catch blocks (which silently swallow the
    /// exception). Use targeted exception types or rethrow.
    /// </summary>
    public sealed class HeadlessRunnerNoSilentExceptionBugTests
    {
        private readonly ITestOutputHelper output;

        public HeadlessRunnerNoSilentExceptionBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void HeadlessSimulationRunner_Has_No_Empty_Catch_Blocks()
        {
            string source = LocateSource();
            string text = File.ReadAllText(source);

            // Match 'catch' followed by optional whitespace, optional
            // typed-exception, optional braces with only whitespace
            // and comments inside.
            // We accept catches that do at least one thing (rethrow,
            // log, set field, etc.).
            Regex emptyCatch = new Regex(
                @"catch\s*(\([^)]*\))?\s*\{\s*(//[^\n]*\n\s*)*\}",
                RegexOptions.Compiled);
            MatchCollection matches = emptyCatch.Matches(text);

            output.WriteLine("empty catch blocks: " + matches.Count);
            foreach (Match m in matches)
            {
                int line = text.Substring(0, m.Index).Split('\n').Length;
                output.WriteLine("  L" + line + ": " + Regex.Replace(m.Value, "\\s+", " "));
            }

            Assert.Empty(matches);
        }

        private static string LocateSource()
        {
            string baseDir = Path.GetDirectoryName(typeof(HeadlessRunnerNoSilentExceptionBugTests).GetTypeInfo().Assembly.Location);
            string current = baseDir;
            for (int i = 0; i < 10 && current != null; i++)
            {
                string candidate = Path.Combine(current, "domain-core", "src", "Domain", "Core", "HeadlessSimulationRunner.cs");
                if (File.Exists(candidate)) return candidate;
                current = Path.GetDirectoryName(current);
            }
            throw new FileNotFoundException("HeadlessSimulationRunner.cs not found near " + baseDir);
        }
    }
}
