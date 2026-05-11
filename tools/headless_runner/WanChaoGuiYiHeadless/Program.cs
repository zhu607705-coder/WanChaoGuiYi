using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace WanChaoGuiYi
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            string dataDirectory = args.Length > 0 ? args[0] : FindDefaultDataDirectory();
            string playerFactionId = args.Length > 1 ? args[1] : "faction_qin_shi_huang";

            try
            {
                NonUnityJsonDataRepository repository = new NonUnityJsonDataRepository();
                repository.Load(dataDirectory);

                HeadlessSimulationRunner runner = new HeadlessSimulationRunner();
                HeadlessSimulationSuiteResult suite = runner.RunAllScenarios(repository, playerFactionId);
                suite.report.generatedAt = DateTime.UtcNow.ToString("O");

                string reportPath = FindReportPath();
                WriteJsonReport(reportPath, suite.report);
                PrintHumanSummary(suite.report, ReportRelativePath());

                return suite.passed ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("HEADLESS RUNNER ERROR: " + ex.GetType().Name + ": " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 2;
            }
        }

        private static string FindDefaultDataDirectory()
        {
            string current = Directory.GetCurrentDirectory();
            for (int i = 0; i < 8; i++)
            {
                string candidate = Path.Combine(current, "My project", "Assets", "Data");
                if (Directory.Exists(candidate)) return candidate;

                DirectoryInfo parent = Directory.GetParent(current);
                if (parent == null) break;
                current = parent.FullName;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "My project", "Assets", "Data");
        }

        private static string FindReportPath()
        {
            return Path.Combine(FindRepositoryRoot(), "tools", "headless_runner", "latest-war-report.json");
        }

        private static string ReportRelativePath()
        {
            return Path.Combine("tools", "headless_runner", "latest-war-report.json");
        }

        private static string FindRepositoryRoot()
        {
            string current = Directory.GetCurrentDirectory();
            for (int i = 0; i < 8; i++)
            {
                if (Directory.Exists(Path.Combine(current, "tools", "headless_runner")) &&
                    Directory.Exists(Path.Combine(current, "My project", "Assets", "Data")))
                {
                    return current;
                }

                DirectoryInfo parent = Directory.GetParent(current);
                if (parent == null) break;
                current = parent.FullName;
            }

            return Directory.GetCurrentDirectory();
        }

        private static void WriteJsonReport(string reportPath, HeadlessWarReport report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            File.WriteAllText(reportPath, JsonSerializer.Serialize(report, options));
        }

        private static void PrintHumanSummary(HeadlessWarReport report, string reportPath)
        {
            Console.WriteLine("Headless war verification: passed=" + FormatBool(report.passed) + " scenarioCount=" + report.scenarioCount);
            Console.WriteLine("Report: " + reportPath);
            Console.WriteLine();

            for (int i = 0; i < report.scenarios.Count; i++)
            {
                HeadlessScenarioReport scenario = report.scenarios[i];
                Console.WriteLine("[" + (scenario.passed ? "PASS" : "FAIL") + "] " + scenario.name + " turns=" + scenario.turnsExecuted);
                Console.WriteLine("  chain: " + ChainSummary(scenario));
                Console.WriteLine("  result: " + scenario.summary);
                Console.WriteLine("  keyDelta: " + KeyDeltaSummary(scenario));

                if (!scenario.passed)
                {
                    Console.WriteLine("  failureStage: " + scenario.failureStage);
                    Console.WriteLine("  failureReason: " + scenario.failureReason);
                    HeadlessPhaseResult failedPhase = FindFailedPhase(scenario);
                    if (failedPhase != null)
                    {
                        Console.WriteLine("  before: " + DictionarySummary(failedPhase.before));
                        Console.WriteLine("  after: " + DictionarySummary(failedPhase.after));
                    }
                    Console.WriteLine("  jsonReport: " + reportPath);
                }

                Console.WriteLine();
            }
        }

        private static string ChainSummary(HeadlessScenarioReport scenario)
        {
            string[] phases = { "command", "movement", "engagement", "battle", "outcome", "governance", "economy" };
            string summary = string.Empty;
            for (int i = 0; i < phases.Length; i++)
            {
                if (i > 0) summary += " ";
                summary += phases[i] + "=" + FindPhaseStatus(scenario, phases[i]);
            }
            return summary;
        }

        private static string FindPhaseStatus(HeadlessScenarioReport scenario, string phase)
        {
            for (int i = 0; i < scenario.phaseResults.Count; i++)
            {
                if (scenario.phaseResults[i].phase == phase) return scenario.phaseResults[i].status;
            }
            return "skip";
        }

        private static string KeyDeltaSummary(HeadlessScenarioReport scenario)
        {
            if (scenario.keyDeltas == null || scenario.keyDeltas.Count == 0) return "none";
            string summary = string.Empty;
            int count = scenario.keyDeltas.Count < 3 ? scenario.keyDeltas.Count : 3;
            for (int i = 0; i < count; i++)
            {
                HeadlessKeyDelta delta = scenario.keyDeltas[i];
                if (i > 0) summary += "; ";
                summary += delta.field + "(" + delta.entityId + ") " + delta.before + " -> " + delta.after;
            }
            return summary;
        }

        private static HeadlessPhaseResult FindFailedPhase(HeadlessScenarioReport scenario)
        {
            if (scenario == null || scenario.phaseResults == null) return null;
            for (int i = 0; i < scenario.phaseResults.Count; i++)
            {
                if (scenario.phaseResults[i].status == "fail") return scenario.phaseResults[i];
            }
            return null;
        }

        private static string DictionarySummary(System.Collections.Generic.Dictionary<string, object> values)
        {
            if (values == null || values.Count == 0) return "none";
            string result = string.Empty;
            int count = 0;
            foreach (System.Collections.Generic.KeyValuePair<string, object> item in values)
            {
                if (count > 0) result += ", ";
                result += item.Key + "=" + item.Value;
                count++;
            }
            return result;
        }

        private static string FormatBool(bool value)
        {
            return value ? "True" : "False";
        }
    }
}
