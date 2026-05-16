using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: HeadlessSimulationRunner contains
    /// AddAssertion(...) calls whose 'expected' and 'actual' arguments
    /// are the same boolean expression (or two literal trues), making
    /// the assertion impossible to fail.  Any future regression that
    /// silently flips the underlying state will not be detected,
    /// because the self-comparison stays internally consistent.
    ///
    /// This test inspects the source file directly and refuses to
    /// accept obviously self-affirming assertions.
    /// </summary>
    public sealed class HeadlessSelfAffirmingAssertionBugTests
    {
        private readonly ITestOutputHelper output;

        public HeadlessSelfAffirmingAssertionBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void HeadlessSimulationRunner_Has_No_Self_Affirming_Assertions()
        {
            string sourcePath = LocateSource();
            string text = File.ReadAllText(sourcePath);

            // Look for AddAssertion(...) where expected and actual are
            // both literal `true` (typical self-affirming pattern in
            // the current codebase) or where the same identifier
            // appears in both slots adjacent to each other.  These
            // patterns are heuristic: we are trying to flag the
            // patterns we already saw, not catch every possible case.
            //
            // Pattern 1: AddAssertion(... true, identifier, ...).
            // The bug case in connected-campaign uses
            //     AddAssertion(... routeRules, true, routeRules, ...)
            // which collapses into "expected == true, actual == routeRules".
            // We additionally flag the literal "true, true" case.
            Regex literalTrueTrue = new Regex(@"AddAssertion\([^;]*?,\s*true\s*,\s*true\s*,", RegexOptions.Singleline);
            Regex sameIdentifier = new Regex(@"AddAssertion\([^;]*?,\s*([A-Za-z_][A-Za-z0-9_]*)\s*,\s*\1\s*,", RegexOptions.Singleline);

            List<string> offenders = new List<string>();
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (literalTrueTrue.IsMatch(lines[i])) offenders.Add("L" + (i + 1) + " literal-true-true: " + lines[i].Trim());
                Match m = sameIdentifier.Match(lines[i]);
                if (m.Success) offenders.Add("L" + (i + 1) + " same-identifier '" + m.Groups[1].Value + "': " + lines[i].Trim());
            }

            output.WriteLine("Inspected " + sourcePath);
            output.WriteLine("Offenders found: " + offenders.Count);
            foreach (string offender in offenders)
            {
                output.WriteLine("  " + offender);
            }

            Assert.Empty(offenders);
        }

        private static string LocateSource()
        {
            // Walk up from the test assembly location until we find
            // domain-core/src/Domain/Core/HeadlessSimulationRunner.cs.
            string baseDir = Path.GetDirectoryName(typeof(HeadlessSelfAffirmingAssertionBugTests).GetTypeInfo().Assembly.Location);
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
