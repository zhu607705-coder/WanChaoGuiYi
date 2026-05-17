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
    /// AddAssertion(result, id, phase, passed, expected, actual, msg)
    /// calls where the same expression is passed for both `passed` and
    /// `actual`, with `expected` set to a constant true.  Pattern:
    ///
    ///     AddAssertion(result, "summary_id", "phase",
    ///         X,                  // passed
    ///         true, X,            // expected, actual
    ///         "...");
    ///
    /// This collapses the assertion into "did X evaluate to truthy?"
    /// while pretending in the JSON report that an expected-vs-actual
    /// comparison happened.  When X is itself the AND of a long chain
    /// of sub-checks, regressions are hidden behind the very check that
    /// claims to pin them.
    ///
    /// Also flag the simpler cases:
    ///  - passed=true literal
    ///  - passed and expected both are the same literal
    ///
    /// The test inspects the source file directly because a runtime-only
    /// inspection cannot see how arguments were spelled.
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

            // 1. Walk every AddAssertion(...) call and tokenize its
            //    top-level comma-separated arguments. We can't use a
            //    full C# parser, but the codebase keeps each call
            //    visually compact: open paren, 7 arguments, close paren.
            //    Top-level tokenization tracks nesting of () and "".
            List<string[]> calls = ExtractAddAssertionArgs(text);
            output.WriteLine("AddAssertion calls inspected: " + calls.Count);

            List<string> offenders = new List<string>();
            foreach (string[] args in calls)
            {
                if (args.Length < 7) continue;
                string passed = args[3].Trim();
                string expected = args[4].Trim();
                string actual = args[5].Trim();

                // Pattern A: passed and actual are the same identifier
                //            and expected is the literal `true`.
                if (passed == actual && expected == "true" && IsIdentifier(passed))
                {
                    offenders.Add("self-affirm (passed==actual==" + passed + ", expected=true): " +
                                  Trunc(args[1]));
                    continue;
                }

                // Pattern B: passed is the literal `true`.
                if (passed == "true")
                {
                    offenders.Add("hard-coded passed=true: " + Trunc(args[1]));
                    continue;
                }

                // Pattern C: passed and expected are both the same
                //            literal (e.g. true,true,true).
                if (passed == expected && (passed == "true" || passed == "false"))
                {
                    offenders.Add("constant tautology (passed==expected==" + passed + "): " +
                                  Trunc(args[1]));
                    continue;
                }
            }

            foreach (string offender in offenders)
            {
                output.WriteLine("  " + offender);
            }
            output.WriteLine("Offenders found: " + offenders.Count);
            Assert.Empty(offenders);
        }

        private static string LocateSource()
        {
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

        private static List<string[]> ExtractAddAssertionArgs(string text)
        {
            List<string[]> result = new List<string[]>();
            int i = 0;
            while (i < text.Length)
            {
                int idx = text.IndexOf("AddAssertion(", i, StringComparison.Ordinal);
                if (idx < 0) break;
                i = idx + "AddAssertion(".Length;
                List<string> args = ParseTopLevelArgs(text, ref i);
                if (args != null) result.Add(args.ToArray());
            }
            return result;
        }

        private static List<string> ParseTopLevelArgs(string text, ref int i)
        {
            int depthParen = 0;
            int depthBracket = 0;
            int depthBrace = 0;
            bool inString = false;
            bool inChar = false;
            bool inLineComment = false;
            bool inBlockComment = false;
            int argStart = i;
            List<string> args = new List<string>();
            while (i < text.Length)
            {
                char c = text[i];
                char next = i + 1 < text.Length ? text[i + 1] : '\0';

                if (inLineComment)
                {
                    if (c == '\n') inLineComment = false;
                    i++;
                    continue;
                }
                if (inBlockComment)
                {
                    if (c == '*' && next == '/') { inBlockComment = false; i += 2; continue; }
                    i++;
                    continue;
                }
                if (inString)
                {
                    if (c == '\\') { i += 2; continue; }
                    if (c == '"') { inString = false; }
                    i++;
                    continue;
                }
                if (inChar)
                {
                    if (c == '\\') { i += 2; continue; }
                    if (c == '\'') { inChar = false; }
                    i++;
                    continue;
                }

                if (c == '/' && next == '/') { inLineComment = true; i += 2; continue; }
                if (c == '/' && next == '*') { inBlockComment = true; i += 2; continue; }
                if (c == '"') { inString = true; i++; continue; }
                if (c == '\'') { inChar = true; i++; continue; }

                if (c == '(') { depthParen++; i++; continue; }
                if (c == '[') { depthBracket++; i++; continue; }
                if (c == '{') { depthBrace++; i++; continue; }
                if (c == ']') { depthBracket--; i++; continue; }
                if (c == '}') { depthBrace--; i++; continue; }
                if (c == ')')
                {
                    if (depthParen == 0 && depthBracket == 0 && depthBrace == 0)
                    {
                        args.Add(text.Substring(argStart, i - argStart));
                        i++;
                        return args;
                    }
                    depthParen--;
                    i++;
                    continue;
                }
                if (c == ',' && depthParen == 0 && depthBracket == 0 && depthBrace == 0)
                {
                    args.Add(text.Substring(argStart, i - argStart));
                    i++;
                    argStart = i;
                    continue;
                }

                i++;
            }
            return null;
        }

        private static bool IsIdentifier(string token)
        {
            string t = token.Trim();
            if (t.Length == 0) return false;
            if (!(char.IsLetter(t[0]) || t[0] == '_')) return false;
            for (int i = 1; i < t.Length; i++)
            {
                char c = t[i];
                if (!(char.IsLetterOrDigit(c) || c == '_' || c == '.')) return false;
            }
            return true;
        }

        private static string Trunc(string s)
        {
            string t = Regex.Replace(s.Trim(), "\\s+", " ");
            return t.Length > 80 ? t.Substring(0, 80) + "..." : t;
        }
    }
}
