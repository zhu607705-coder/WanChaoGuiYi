using System;
using System.IO;

namespace WanChaoGuiYi
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            string dataDirectory = args.Length > 0 ? args[0] : FindDefaultDataDirectory();
            string playerFactionId = args.Length > 1 ? args[1] : "faction_qin_shihuang";

            try
            {
                NonUnityJsonDataRepository repository = new NonUnityJsonDataRepository();
                repository.Load(dataDirectory);

                HeadlessSimulationRunner runner = new HeadlessSimulationRunner();
                HeadlessSimulationResult result = runner.RunSingleLaneWar(repository, playerFactionId);

                Console.WriteLine("dataDirectory=" + dataDirectory);
                Console.WriteLine("playerFactionId=" + playerFactionId);
                Console.WriteLine("passed=" + result.passed);
                Console.WriteLine("turnsExecuted=" + result.turnsExecuted);

                if (!result.passed)
                {
                    Console.WriteLine("failureReason=" + result.failureReason);
                    PrintLogs(result.state);
                    return 1;
                }

                PrintLogs(result.state);
                return 0;
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
                string candidate = Path.Combine(current, "WanChaoGuiYi", "Assets", "Data");
                if (Directory.Exists(candidate)) return candidate;

                DirectoryInfo parent = Directory.GetParent(current);
                if (parent == null) break;
                current = parent.FullName;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "WanChaoGuiYi", "Assets", "Data");
        }

        private static void PrintLogs(GameState state)
        {
            if (state == null || state.turnLog == null) return;

            Console.WriteLine("logs:");
            for (int i = 0; i < state.turnLog.Count; i++)
            {
                TurnLogEntry entry = state.turnLog[i];
                if (entry == null) continue;
                Console.WriteLine("[" + entry.turn + "][" + entry.category + "] " + entry.message);
            }
        }
    }
}
