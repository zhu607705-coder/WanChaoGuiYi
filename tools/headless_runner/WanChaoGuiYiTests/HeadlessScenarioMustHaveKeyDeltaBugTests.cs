using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: HeadlessSimulationRunner ships several
    /// scenarios whose printed report shows "keyDelta: none".  These
    /// scenarios run, exit Pass(), and contribute to the 16/16 green
    /// count without recording a single state delta.  The PowerShell
    /// verifier accepts that, so any silent regression in those
    /// scenarios — for example, governance forecasts no longer
    /// applying — would never be detected.
    ///
    /// Pinned invariant: every scenario report MUST emit at least one
    /// HeadlessKeyDelta.  If a scenario truly has nothing to record,
    /// it is not a useful scenario and should be pruned.
    ///
    /// To avoid running the full simulation here, we lift the assertion
    /// to source level: any method that returns a Pass(...) call must
    /// have at least one AddKeyDelta(...) earlier in the same method.
    /// </summary>
    public sealed class HeadlessScenarioMustHaveKeyDeltaBugTests
    {
        private readonly ITestOutputHelper output;

        public HeadlessScenarioMustHaveKeyDeltaBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Every_RunXxx_Scenario_Must_Emit_At_Least_One_KeyDelta()
        {
            string sourcePath = LocateSource();
            string text = File.ReadAllText(sourcePath);

            // Find scenario methods. They follow the pattern:
            //   public HeadlessSimulationResult Run<Name>(IDataRepository data, string playerFactionId)
            // Body extends to matching closing brace.
            Regex header = new Regex(@"public\s+HeadlessSimulationResult\s+(Run\w+)\s*\(\s*IDataRepository", RegexOptions.Compiled);
            MatchCollection matches = header.Matches(text);

            List<string> offenders = new List<string>();
            int scenariosInspected = 0;

            foreach (Match m in matches)
            {
                string name = m.Groups[1].Value;
                int braceStart = text.IndexOf('{', m.Index + m.Length);
                if (braceStart < 0) continue;
                int braceEnd = FindMatchingBrace(text, braceStart);
                if (braceEnd < 0) continue;
                string body = text.Substring(braceStart, braceEnd - braceStart + 1);

                // Skip the orchestration entry point itself
                if (name == "RunSingleLaneWar" || name == "RunAllScenarios") continue;
                scenariosInspected++;

                bool hasKeyDelta = body.Contains("AddKeyDelta(");
                bool reachesPass = body.Contains("return Pass(");
                if (reachesPass && !hasKeyDelta)
                {
                    offenders.Add(name + " reaches Pass() but never calls AddKeyDelta");
                }
            }

            output.WriteLine("scenarios inspected: " + scenariosInspected);
            output.WriteLine("offenders: " + offenders.Count);
            foreach (string o in offenders) output.WriteLine("  " + o);

            Assert.Empty(offenders);
        }

        private static int FindMatchingBrace(string text, int openIndex)
        {
            int depth = 0;
            bool inString = false;
            bool inChar = false;
            bool inLineComment = false;
            bool inBlockComment = false;
            for (int i = openIndex; i < text.Length; i++)
            {
                char c = text[i];
                char next = i + 1 < text.Length ? text[i + 1] : '\0';
                if (inLineComment) { if (c == '\n') inLineComment = false; continue; }
                if (inBlockComment) { if (c == '*' && next == '/') { inBlockComment = false; i++; } continue; }
                if (inString) { if (c == '\\') { i++; continue; } if (c == '"') inString = false; continue; }
                if (inChar) { if (c == '\\') { i++; continue; } if (c == '\'') inChar = false; continue; }
                if (c == '/' && next == '/') { inLineComment = true; i++; continue; }
                if (c == '/' && next == '*') { inBlockComment = true; i++; continue; }
                if (c == '"') { inString = true; continue; }
                if (c == '\'') { inChar = true; continue; }
                if (c == '{') depth++;
                else if (c == '}') { depth--; if (depth == 0) return i; }
            }
            return -1;
        }

        private static string LocateSource()
        {
            string baseDir = Path.GetDirectoryName(typeof(HeadlessScenarioMustHaveKeyDeltaBugTests).GetTypeInfo().Assembly.Location);
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
