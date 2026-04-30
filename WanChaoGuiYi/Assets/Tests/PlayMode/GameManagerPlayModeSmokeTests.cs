using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace WanChaoGuiYi.Tests
{
    public sealed class GameManagerPlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator GameManagerBootstrapsWarLoopAndRebindsAfterRestart()
        {
            GameObject root = new GameObject("PlayModeSmoke_GameManager");
            GameManager manager = root.AddComponent<GameManager>();

            yield return null;

            AssertBootstrapped(manager);
            Assert.IsTrue(manager.RunSingleLaneWarSmokeTest(), "Initial map-led war smoke command should be issued.");
            manager.NextTurn();
            AssertHasLog(manager.State, "接敌");
            AssertNoLog(manager.State, "战斗结束");
            manager.NextTurn();
            AssertHasLog(manager.State, "战斗结束");
            AssertHasLog(manager.State, "收入 金钱");

            WorldState firstWorld = manager.World;
            manager.StartNewGame();
            yield return null;

            AssertBootstrapped(manager);
            Assert.AreNotSame(firstWorld, manager.World, "StartNewGame should replace WorldState.");
            Assert.IsTrue(manager.RunSingleLaneWarSmokeTest(), "War smoke command should still work after StartNewGame rebinding.");
            manager.NextTurn();
            AssertHasLog(manager.State, "接敌");
            AssertNoLog(manager.State, "战斗结束");
            manager.NextTurn();
            AssertHasLog(manager.State, "战斗结束");
            AssertHasLog(manager.State, "收入 金钱");

            Object.Destroy(root);
            yield return null;
        }

        private static void AssertBootstrapped(GameManager manager)
        {
            Assert.IsNotNull(manager, "GameManager is required.");
            Assert.IsNotNull(manager.Data, "DataRepository should be created by GameManager.");
            Assert.IsTrue(manager.Data.IsLoaded, "DataRepository should load JSON tables in PlayMode.");
            Assert.IsNotNull(manager.State, "GameState should be created.");
            Assert.IsNotNull(manager.World, "WorldState should be created.");
            Assert.IsNotNull(manager.MapQueries, "MapQueryService should be created.");
            Assert.IsNotNull(manager.MapCommands, "MapCommandService should be created.");
            Assert.IsNotNull(manager.State.FindFaction("faction_qin_shi_huang"), "Default Qin player faction should exist.");
        }

        private static void AssertHasLog(GameState state, string token)
        {
            Assert.IsNotNull(state, "GameState is required for log assertion.");
            Assert.IsNotNull(state.turnLog, "Turn log is required for log assertion.");

            for (int i = 0; i < state.turnLog.Count; i++)
            {
                TurnLogEntry entry = state.turnLog[i];
                if (entry != null && entry.message != null && entry.message.Contains(token))
                {
                    return;
                }
            }

            Assert.Fail("Expected turn log containing: " + token);
        }

        private static void AssertNoLog(GameState state, string token)
        {
            Assert.IsNotNull(state, "GameState is required for negative log assertion.");
            Assert.IsNotNull(state.turnLog, "Turn log is required for negative log assertion.");

            for (int i = 0; i < state.turnLog.Count; i++)
            {
                TurnLogEntry entry = state.turnLog[i];
                if (entry != null && entry.message != null && entry.message.Contains(token))
                {
                    Assert.Fail("Unexpected turn log containing: " + token);
                }
            }
        }
    }
}
