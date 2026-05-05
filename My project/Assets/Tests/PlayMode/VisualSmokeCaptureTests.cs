using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace WanChaoGuiYi.Tests
{
    public sealed class VisualSmokeCaptureTests
    {
        [UnityTest]
        public IEnumerator CaptureMapHudRegionBuildAndWeatherPanels()
        {
            if (!CanRenderScreenshots())
            {
                Assert.Ignore("Visual smoke capture requires an active graphics device; skipped under headless or -nographics PlayMode.");
            }

            Screen.SetResolution(1600, 900, false);

            GameObject bootstrapRoot = new GameObject("VisualSmoke_DemoBootstrap");
            bootstrapRoot.AddComponent<DemoSceneBootstrap>();

            yield return null;
            yield return null;
            yield return null;

            GameManager manager = Object.FindObjectOfType<GameManager>();
            Assert.IsNotNull(manager, "Visual smoke requires GameManager.");
            AssertRuntimeMapVisualContract();

            string outputDir = System.Environment.GetEnvironmentVariable("VISUAL_OUTPUT_DIR");
            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = Path.Combine(FindRepositoryRoot(), ".outputs", "visual");
            }
            Directory.CreateDirectory(outputDir);

            yield return Capture(Path.Combine(outputDir, "unity-map-hud.png"));

            RegionPanel regionPanel = Object.FindObjectOfType<RegionPanel>();
            Assert.IsNotNull(regionPanel, "Visual smoke requires RegionPanel.");

            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Visual smoke requires player faction.");
            Assert.Greater(playerFaction.regionIds.Count, 0, "Visual smoke requires a player region.");

            RegionState playerRegion = manager.State.FindRegion(playerFaction.regionIds[0]);
            Assert.IsNotNull(playerRegion, "Visual smoke requires player region state.");
            RegionDefinition playerRegionDefinition = manager.Data.GetRegion(playerRegion.id);
            Assert.IsNotNull(playerRegionDefinition, "Visual smoke requires player region data.");

            playerFaction.money = Mathf.Max(playerFaction.money, 1000);
            playerFaction.food = Mathf.Max(playerFaction.food, 1000);
            if (!playerFaction.completedTechIds.Contains("granary_system"))
            {
                playerFaction.completedTechIds.Add("granary_system");
            }

            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, playerRegion.id, null));
            yield return null;
            yield return Capture(Path.Combine(outputDir, "unity-governance-default.png"));
            yield return Capture(Path.Combine(outputDir, "unity-governance-forecast.png"));
            yield return Capture(Path.Combine(outputDir, "unity-outliner.png"));

            Button riskLensButton = GameObject.Find("LensRiskButton").GetComponent<Button>();
            Assert.IsNotNull(riskLensButton, "Visual smoke requires risk lens button.");
            riskLensButton.onClick.Invoke();
            yield return null;
            yield return Capture(Path.Combine(outputDir, "unity-map-lens-risk.png"));

            regionPanel.Show(playerRegionDefinition, playerRegion, manager.Context, playerFaction, manager.GetComponent<BuildingSystem>());
            Button buildButton = GameObject.Find("BuildRegionBuildingButton").GetComponent<Button>();
            buildButton.onClick.Invoke();
            yield return null;
            yield return Capture(Path.Combine(outputDir, "unity-region-building-panel.png"));

            WeatherPanel weatherPanel = Object.FindObjectOfType<WeatherPanel>();
            Assert.IsNotNull(weatherPanel, "Visual smoke requires WeatherPanel.");
            regionPanel.Hide();
            weatherPanel.Show(manager.Context, manager.GetComponent<WeatherSystem>(), manager.GetComponent<CelestialEventSystem>());
            yield return null;
            yield return Capture(Path.Combine(outputDir, "unity-weather-panel.png"));
            weatherPanel.Hide();

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_player_1", out playerArmy), "Visual smoke requires a player army.");
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_enemy_1", out enemyArmy), "Visual smoke requires an enemy army.");
            playerArmy.soldiers = 5200;
            playerArmy.morale = 95;
            enemyArmy.soldiers = 500;
            enemyArmy.morale = 35;
            SyncLegacyArmy(manager, playerArmy.id, playerArmy.soldiers);
            SyncLegacyArmy(manager, enemyArmy.id, enemyArmy.soldiers);

            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, enemyArmy.locationRegionId, null));
            yield return null;
            Button mechanismButton = GameObject.Find("MechanismButton").GetComponent<Button>();
            Assert.IsNotNull(mechanismButton, "Visual smoke requires MechanismButton.");
            mechanismButton.onClick.Invoke();
            yield return null;
            yield return Capture(Path.Combine(outputDir, "unity-diplomacy-bridge.png"));

            Button enterWarModeButton = GameObject.Find("EnterWarModeButton").GetComponent<Button>();
            Assert.IsNotNull(enterWarModeButton, "Visual smoke requires EnterWarModeButton.");
            Assert.IsTrue(enterWarModeButton.interactable, "Visual smoke target should allow war-mode entry.");
            enterWarModeButton.onClick.Invoke();
            yield return null;

            Button attackButton = GameObject.Find("AttackButton").GetComponent<Button>();
            Assert.IsNotNull(attackButton, "Visual smoke requires AttackButton.");
            Assert.IsTrue(attackButton.interactable, "Visual smoke target should allow attack dispatch.");
            attackButton.onClick.Invoke();
            yield return null;
            yield return Capture(Path.Combine(outputDir, "unity-war-route.png"));
            yield return Capture(Path.Combine(outputDir, "unity-war-route-risk.png"));

            manager.NextTurn();
            yield return null;
            manager.NextTurn();
            yield return null;

            GameObject battleReport = GameObject.Find("BattleReportPanel");
            Assert.IsNotNull(battleReport, "Visual smoke requires BattleReportPanel after battle resolution.");
            Assert.IsTrue(battleReport.activeInHierarchy, "Visual smoke battle report should be visible.");
            yield return Capture(Path.Combine(outputDir, "unity-battle-report.png"));
            yield return Capture(Path.Combine(outputDir, "unity-occupation-chain.png"));

            Object.Destroy(bootstrapRoot);
            if (manager != null)
            {
                Object.Destroy(manager.gameObject);
            }
            yield return null;
        }

        private static IEnumerator Capture(string path)
        {
            if (!CanRenderScreenshots())
            {
                Assert.Ignore("Screenshot capture requires an active graphics device; skipped under headless or -nographics PlayMode.");
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            yield return null;
            Canvas.ForceUpdateCanvases();

            Camera camera = Camera.main != null ? Camera.main : Object.FindObjectOfType<Camera>();
            Assert.IsNotNull(camera, "Screenshot capture requires a camera.");

            Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
            RenderMode[] previousRenderModes = new RenderMode[canvases.Length];
            Camera[] previousCameras = new Camera[canvases.Length];
            float[] previousPlaneDistances = new float[canvases.Length];

            for (int i = 0; i < canvases.Length; i++)
            {
                previousRenderModes[i] = canvases[i].renderMode;
                previousCameras[i] = canvases[i].worldCamera;
                previousPlaneDistances[i] = canvases[i].planeDistance;
                if (canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                    canvases[i].worldCamera = camera;
                    canvases[i].planeDistance = 1f;
                }
            }

            RenderTexture renderTexture = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Object.Destroy(renderTexture);
                Object.Destroy(texture);

                for (int i = 0; i < canvases.Length; i++)
                {
                    if (canvases[i] == null) continue;
                    canvases[i].renderMode = previousRenderModes[i];
                    canvases[i].worldCamera = previousCameras[i];
                    canvases[i].planeDistance = previousPlaneDistances[i];
                }
            }

            Assert.IsTrue(File.Exists(path) && new FileInfo(path).Length > 0, "Screenshot was not created: " + path);
        }

        private static void AssertRuntimeMapVisualContract()
        {
            MapSetup mapSetup = Object.FindObjectOfType<MapSetup>();
            Assert.IsNotNull(mapSetup, "Visual smoke should bootstrap MapSetup.");
            Assert.IsTrue(mapSetup.UsedGeneratedMapResource, "Visual smoke should load the generated map from Resources.");
            Assert.IsTrue(mapSetup.UsedMapRenderMetadataResource, "Visual smoke should load map projection metadata from Resources.");

            GameObject background = GameObject.Find("Generated_Jiuzhou_Map_Background_Runtime");
            Assert.IsNotNull(background, "Visual smoke should render the generated full-map background.");

            SpriteRenderer backgroundRenderer = background.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(backgroundRenderer, "Generated map background should use a SpriteRenderer.");
            Assert.IsNotNull(backgroundRenderer.sprite, "Generated map background should have a sprite.");
            Assert.AreEqual(2048, backgroundRenderer.sprite.texture.width, "Generated map background should use the full source image width.");
            Assert.AreEqual(1536, backgroundRenderer.sprite.texture.height, "Generated map background should use the full source image height.");

            Assert.IsNull(GameObject.Find("Emperor_qin_shi_huang"), "Visual smoke should not show the old all-emperor portrait strip.");
            Assert.IsNull(GameObject.Find("Army_army_player_1"), "Visual smoke should not show idle armies before a command, contact, or battle state asks for them.");
            Assert.IsNull(GameObject.Find("Army_army_enemy_1"), "Visual smoke should not show idle enemy armies before a command, contact, or battle state asks for them.");

            RegionController[] regions = Object.FindObjectsOfType<RegionController>();
            int visibleRegionSurfaces = 0;
            for (int i = 0; i < regions.Length; i++)
            {
                if (regions[i] == null || regions[i].gameObject == null) continue;
                if (!regions[i].gameObject.name.StartsWith("RegionSurface_")) continue;

                MeshRenderer renderer = regions[i].GetComponent<MeshRenderer>();
                if (renderer != null && renderer.enabled)
                {
                    visibleRegionSurfaces++;
                }
            }

            Assert.AreEqual(0, visibleRegionSurfaces, "Visual smoke should not render debug region mesh surfaces over the full-map image.");
        }

        private static void SyncLegacyArmy(GameManager manager, string armyId, int soldiers)
        {
            Assert.IsNotNull(manager, "GameManager is required for legacy army sync.");
            Assert.IsNotNull(manager.State, "GameState is required for legacy army sync.");

            for (int i = 0; i < manager.State.armies.Count; i++)
            {
                ArmyState army = manager.State.armies[i];
                if (army != null && army.id == armyId)
                {
                    army.soldiers = soldiers;
                    return;
                }
            }

            Assert.Fail("Expected legacy army for visual smoke sync: " + armyId);
        }

        private static bool CanRenderScreenshots()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return false;
            string deviceName = SystemInfo.graphicsDeviceName;
            return !string.IsNullOrEmpty(deviceName) &&
                   deviceName.IndexOf("Null", System.StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static string FindRepositoryRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "tools")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "My project")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
        }
    }
}
