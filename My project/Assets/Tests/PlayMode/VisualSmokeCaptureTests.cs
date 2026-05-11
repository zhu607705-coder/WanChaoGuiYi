using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

            yield return SetVisualResolution(1600, 900);

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

            yield return Capture(Path.Combine(outputDir, "unity-map-hud.png"), 1600, 900);
            yield return CaptureViewport(outputDir, "unity-1024-map-hud.png", 1024, 576);
            yield return CaptureViewport(outputDir, "unity-1280-map-hud.png", 1280, 720);
            yield return SetVisualResolution(1600, 900);

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
            yield return Capture(Path.Combine(outputDir, "unity-governance-default.png"), 1600, 900);
            yield return Capture(Path.Combine(outputDir, "unity-governance-forecast.png"), 1600, 900);
            yield return Capture(Path.Combine(outputDir, "unity-outliner.png"), 1600, 900);

            Button riskLensButton = GameObject.Find("LensRiskButton").GetComponent<Button>();
            Assert.IsNotNull(riskLensButton, "Visual smoke requires risk lens button.");
            riskLensButton.onClick.Invoke();
            yield return null;
            yield return Capture(Path.Combine(outputDir, "unity-map-lens-risk.png"), 1600, 900);

            Button terrainLensButton = GameObject.Find("LensTerrainButton").GetComponent<Button>();
            Assert.IsNotNull(terrainLensButton, "Visual smoke requires terrain lens button.");
            terrainLensButton.onClick.Invoke();
            yield return null;
            AssertTerrainLensVisible();
            yield return Capture(Path.Combine(outputDir, "unity-terrain-lens.png"), 1600, 900);
            yield return CaptureViewport(outputDir, "unity-1024-terrain-lens.png", 1024, 576);
            yield return CaptureViewport(outputDir, "unity-1280-terrain-lens.png", 1280, 720);
            yield return SetVisualResolution(1600, 900);

            regionPanel.Show(playerRegionDefinition, playerRegion, manager.Context, playerFaction, manager.GetComponent<BuildingSystem>());
            Button buildButton = GameObject.Find("BuildRegionBuildingButton").GetComponent<Button>();
            buildButton.onClick.Invoke();
            yield return null;
            yield return Capture(Path.Combine(outputDir, "unity-region-building-panel.png"), 1600, 900);
            yield return CaptureViewport(outputDir, "unity-1024-region-building-panel.png", 1024, 576);
            yield return CaptureViewport(outputDir, "unity-1280-region-building-panel.png", 1280, 720);
            yield return SetVisualResolution(1600, 900);

            WeatherPanel weatherPanel = Object.FindObjectOfType<WeatherPanel>();
            Assert.IsNotNull(weatherPanel, "Visual smoke requires WeatherPanel.");
            regionPanel.Hide();
            weatherPanel.Show(manager.Context, manager.GetComponent<WeatherSystem>(), manager.GetComponent<CelestialEventSystem>());
            yield return null;
            yield return Capture(Path.Combine(outputDir, "unity-weather-panel.png"), 1600, 900);
            yield return CaptureViewport(outputDir, "unity-1024-weather-panel.png", 1024, 576);
            yield return CaptureViewport(outputDir, "unity-1280-weather-panel.png", 1280, 720);
            yield return SetVisualResolution(1600, 900);
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
            AssertMechanismActionIconsVisible();
            yield return Capture(Path.Combine(outputDir, "unity-mechanism-actions.png"), 1600, 900);
            yield return CaptureViewport(outputDir, "unity-1024-mechanism-actions.png", 1024, 576);
            yield return CaptureViewport(outputDir, "unity-1280-mechanism-actions.png", 1280, 720);
            yield return SetVisualResolution(1600, 900);
            yield return Capture(Path.Combine(outputDir, "unity-diplomacy-bridge.png"), 1600, 900);
            yield return CaptureViewport(outputDir, "unity-1024-diplomacy-bridge.png", 1024, 576);
            yield return CaptureViewport(outputDir, "unity-1280-diplomacy-bridge.png", 1280, 720);
            yield return SetVisualResolution(1600, 900);

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
            DemoEntityVisualSpawner spawner = Object.FindObjectOfType<DemoEntityVisualSpawner>();
            Assert.IsNotNull(spawner, "Visual smoke requires entity overlay stats.");
            spawner.ApplyLabelDensityForCurrentZoom();
            Assert.GreaterOrEqual(spawner.LastWarRouteObjectCount, 3, "Visual smoke war route should expose route, underlay, and projected contact node.");
            Assert.GreaterOrEqual(spawner.LastWarTargetObjectCount, 1, "Visual smoke war route should expose target highlight.");
            Assert.LessOrEqual(spawner.LastWarOverlayObjectCount, 14, "Visual smoke should keep single-route overlay object count bounded.");
            Debug.Log("[VisualSmokeOverlayStats] scenario=single_route " + spawner.FormatOverlayStats());
            yield return Capture(Path.Combine(outputDir, "unity-war-route.png"), 1600, 900);
            yield return CaptureViewport(outputDir, "unity-1024-war-route.png", 1024, 576);
            yield return CaptureViewport(outputDir, "unity-1280-war-route.png", 1280, 720);
            yield return SetVisualResolution(1600, 900);
            yield return Capture(Path.Combine(outputDir, "unity-war-route-risk.png"), 1600, 900);

            terrainLensButton.onClick.Invoke();
            yield return null;
            AssertTerrainLensVisible();
            spawner.ApplyLabelDensityForCurrentZoom();
            Assert.GreaterOrEqual(spawner.LastWarRouteObjectCount, 3, "Terrain plus war overlay should retain route, underlay, and projected contact node.");
            Assert.GreaterOrEqual(spawner.LastWarTargetObjectCount, 1, "Terrain plus war overlay should retain the selected war target highlight.");
            Assert.LessOrEqual(spawner.LastWarOverlayObjectCount, 14, "Terrain plus war overlay should keep the single-route object budget bounded.");
            yield return Capture(Path.Combine(outputDir, "unity-terrain-war-overlay.png"), 1600, 900);
            yield return CaptureViewport(outputDir, "unity-1024-terrain-war-overlay.png", 1024, 576);
            yield return CaptureViewport(outputDir, "unity-1280-terrain-war-overlay.png", 1280, 720);
            yield return SetVisualResolution(1600, 900);

            CameraController terrainWarCamera = Object.FindObjectOfType<CameraController>();
            Assert.IsNotNull(terrainWarCamera, "Terrain plus war zoom-band VisualSmoke needs a camera controller.");
            terrainWarCamera.ConfigureZoomLimits(3f, 15f);
            terrainWarCamera.ConfigureZoomBands(5f, 8f);
            yield return CaptureTerrainWarZoomBand(outputDir, terrainWarCamera, spawner, MapZoomBand.Detail, 4f, "unity-terrain-war-detail-zoom.png");
            yield return CaptureTerrainWarZoomBand(outputDir, terrainWarCamera, spawner, MapZoomBand.Operation, 7f, "unity-terrain-war-operation-zoom.png");
            yield return CaptureTerrainWarZoomBand(outputDir, terrainWarCamera, spawner, MapZoomBand.Overview, 14f, "unity-terrain-war-overview-zoom.png");
            yield return SetVisualResolution(1600, 900);
            yield return LogCameraInteractionStats(terrainWarCamera, "terrain_war_1024", 1024, 576);
            yield return LogCameraInteractionStats(terrainWarCamera, "terrain_war_1280", 1280, 720);
            yield return SetVisualResolution(1600, 900);

            ArmyRuntimeState denseArmyA = CreateVisualSmokeStressArmy(manager, playerArmy, "army_visual_dense_front_a", 72);
            ArmyRuntimeState denseArmyB = CreateVisualSmokeStressArmy(manager, playerArmy, "army_visual_dense_front_b", 48);
            string denseTargetRegionId = enemyArmy.locationRegionId;
            Assert.IsTrue(manager.StartPlayerAttack(denseArmyA.id, denseTargetRegionId), "Dense VisualSmoke needs a second attack route.");
            Assert.IsTrue(manager.StartPlayerAttack(denseArmyB.id, denseTargetRegionId), "Dense VisualSmoke needs a third attack route.");
            yield return null;

            CameraController cameraController = Object.FindObjectOfType<CameraController>();
            Assert.IsNotNull(cameraController, "Dense VisualSmoke needs a camera controller.");
            cameraController.ConfigureZoomLimits(3f, 15f);
            cameraController.ConfigureZoomBands(5f, 8f);
            cameraController.SetZoom(14f);
            yield return SetVisualResolution(1024, 576);
            spawner.ApplyLabelDensityForCurrentZoom();
            Assert.GreaterOrEqual(spawner.LastWarRouteObjectCount, 9, "Dense VisualSmoke should expose three route, underlay, and contact-node sets.");
            Assert.GreaterOrEqual(spawner.LastWarLabelObjectCount, 7, "Dense VisualSmoke should create enough labels to pressure avoidance.");
            Assert.LessOrEqual(spawner.LastVisibleLabelCount, 5, "Dense VisualSmoke should keep the tight visible label budget.");
            Assert.Greater(spawner.LastHiddenLabelCount, spawner.LastVisibleLabelCount, "Dense VisualSmoke should hide more labels than it shows at overview zoom.");
            Assert.AreEqual(spawner.LastHiddenLabelCount, spawner.LastHiddenByZoomCount + spawner.LastHiddenByBudgetCount + spawner.LastHiddenByOverlapCount, "Dense VisualSmoke hidden-label reasons should add up.");
            Assert.LessOrEqual(spawner.LastActivePulseCount, spawner.LastPulseBudget, "Dense VisualSmoke should throttle active war pulses.");
            Assert.AreEqual(3, spawner.LastPulseBudget, "Dense 1024x576 overview should use the tightest pulse budget.");
            Assert.Greater(spawner.LastInactivePulseCount, 0, "Dense VisualSmoke should pause pulse effects beyond budget.");
            Assert.LessOrEqual(spawner.LastWarOverlayObjectCount, 22, "Dense VisualSmoke overlay should stay inside the bounded object budget.");
            Debug.Log("[VisualSmokeOverlayStats] scenario=dense_frontline " + spawner.FormatOverlayStats());
            yield return Capture(Path.Combine(outputDir, "unity-1024-dense-frontline-war-overlay.png"), 1024, 576);
            yield return SetVisualResolution(1280, 720);
            spawner.ApplyLabelDensityForCurrentZoom();
            yield return Capture(Path.Combine(outputDir, "unity-1280-dense-frontline-war-overlay.png"), 1280, 720);
            yield return SetVisualResolution(1600, 900);
            spawner.ApplyLabelDensityForCurrentZoom();
            yield return Capture(Path.Combine(outputDir, "unity-dense-frontline-war-overlay.png"), 1600, 900);
            yield return SetVisualResolution(1600, 900);
            Assert.IsTrue(manager.StopArmy(denseArmyA.id), "Dense VisualSmoke should clear the second stress route before battle-report capture.");
            Assert.IsTrue(manager.StopArmy(denseArmyB.id), "Dense VisualSmoke should clear the third stress route before battle-report capture.");
            yield return null;

            manager.NextTurn();
            yield return null;
            manager.NextTurn();
            yield return null;

            GameObject battleReport = GameObject.Find("BattleReportPanel");
            Assert.IsNotNull(battleReport, "Visual smoke requires BattleReportPanel after battle resolution.");
            Assert.IsTrue(battleReport.activeInHierarchy, "Visual smoke battle report should be visible.");
            Assert.AreEqual(battleReport.transform.parent.childCount - 1, battleReport.transform.GetSiblingIndex(), "Visual smoke battle report should render above previously opened governance or mechanism panels.");
            yield return Capture(Path.Combine(outputDir, "unity-battle-report.png"), 1600, 900);
            yield return CaptureViewport(outputDir, "unity-1024-battle-report.png", 1024, 576);
            yield return CaptureViewport(outputDir, "unity-1280-battle-report.png", 1280, 720);
            yield return SetVisualResolution(1600, 900);
            yield return Capture(Path.Combine(outputDir, "unity-occupation-chain.png"), 1600, 900);

            Object.Destroy(bootstrapRoot);
            if (manager != null)
            {
                Object.Destroy(manager.gameObject);
            }
            yield return null;
        }

        private static IEnumerator CaptureTerrainWarZoomBand(string outputDir, CameraController cameraController, DemoEntityVisualSpawner spawner, MapZoomBand expectedBand, float zoom, string fileName)
        {
            yield return SetVisualResolution(1600, 900);
            cameraController.SetZoom(zoom);
            yield return null;
            spawner.ApplyLabelDensityForCurrentZoom();

            Assert.AreEqual(expectedBand, cameraController.CurrentZoomBand, "Terrain plus war overlay should capture the requested zoom band.");
            Assert.GreaterOrEqual(spawner.LastWarRouteObjectCount, 3, "Terrain plus war zoom band should retain route, underlay, and projected contact node.");
            Assert.GreaterOrEqual(spawner.LastWarTargetObjectCount, 1, "Terrain plus war zoom band should retain the selected war target highlight.");
            Assert.GreaterOrEqual(spawner.LastVisibleLabelCount, 1, "Terrain plus war zoom band should keep at least one actionable label visible.");
            Assert.AreEqual(spawner.LastHiddenLabelCount, spawner.LastHiddenByZoomCount + spawner.LastHiddenByBudgetCount + spawner.LastHiddenByOverlapCount, "Terrain plus war zoom-band hidden-label reasons should add up.");
            Assert.LessOrEqual(spawner.LastActivePulseCount, spawner.LastPulseBudget, "Terrain plus war zoom band should keep active pulses within the current budget.");
            Assert.LessOrEqual(spawner.LastWarOverlayObjectCount, 14, "Terrain plus war zoom band should keep the single-route object budget bounded.");

            Debug.Log("[VisualSmokeZoomBandStats] scenario=terrain_war_" + expectedBand + " zoomBand=" + expectedBand + " zoom=" + zoom + " " + spawner.FormatOverlayStats());
            yield return Capture(Path.Combine(outputDir, fileName), 1600, 900);
        }

        private static IEnumerator LogCameraInteractionStats(CameraController cameraController, string scenario, int width, int height)
        {
            Rect originalBounds = cameraController.WorldBounds;
            Vector3 originalPosition = cameraController.transform.position;
            float originalZoom = cameraController.CurrentZoom;

            yield return SetVisualResolution(width, height);
            cameraController.ConfigureBounds(Rect.MinMaxRect(-64f, -36f, 64f, 36f));
            cameraController.ConfigureZoomLimits(3f, 15f);
            cameraController.ConfigureZoomBands(5f, 8f);
            cameraController.SetZoom(8f);
            cameraController.CenterOnRegion(Vector2.zero);

            Camera camera = Camera.main;
            Assert.IsNotNull(camera, "VisualSmoke camera stats require a main camera.");
            Vector3 dragStart = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            Vector3 dragEnd = new Vector3(Screen.width * 0.625f, Screen.height * 0.375f, 0f);
            Vector3 dragDelta = cameraController.CalculateScreenDragWorldDelta(dragStart, dragEnd);
            Assert.Less(dragDelta.x, -0.1f, "VisualSmoke camera drag should move opposite the pointer x direction.");
            Assert.Greater(dragDelta.y, 0.1f, "VisualSmoke camera drag should move opposite the pointer y direction.");

            cameraController.PanByScreenDrag(dragStart, dragEnd);
            AssertCameraViewportInsideBounds(cameraController, "VisualSmoke camera drag should remain inside sampled bounds.");

            Vector3 focusPoint = new Vector3(Screen.width * 0.72f, Screen.height * 0.38f, 0f);
            Vector3 worldBeforeZoom = camera.ScreenToWorldPoint(focusPoint);
            cameraController.ZoomAroundScreenPoint(focusPoint, 6f);
            Vector3 worldAfterZoom = camera.ScreenToWorldPoint(focusPoint);
            float anchorError = Vector2.Distance(new Vector2(worldBeforeZoom.x, worldBeforeZoom.y), new Vector2(worldAfterZoom.x, worldAfterZoom.y));
            Assert.LessOrEqual(anchorError, 0.03f, "VisualSmoke camera zoom should keep the pointer-anchored map location stable.");
            AssertCameraViewportInsideBounds(cameraController, "VisualSmoke camera zoom should remain inside sampled bounds.");

            Debug.Log("[VisualSmokeCameraStats] scenario=" + scenario +
                      " viewport=" + width + "x" + height +
                      " startZoom=8" +
                      " endZoom=" + cameraController.CurrentZoom.ToString("0.###", CultureInfo.InvariantCulture) +
                      " dragX=" + dragDelta.x.ToString("0.###", CultureInfo.InvariantCulture) +
                      " dragY=" + dragDelta.y.ToString("0.###", CultureInfo.InvariantCulture) +
                      " anchorError=" + anchorError.ToString("0.####", CultureInfo.InvariantCulture) +
                      " zoomBand=" + cameraController.CurrentZoomBand);

            cameraController.ConfigureBounds(originalBounds);
            cameraController.SetZoom(originalZoom);
            cameraController.transform.position = originalPosition;
            cameraController.ClampToBounds();
        }

        private static void AssertCameraViewportInsideBounds(CameraController controller, string message)
        {
            Rect viewport = controller.ViewportWorldRect;
            Rect bounds = controller.WorldBounds;
            const float tolerance = 0.04f;
            Assert.GreaterOrEqual(viewport.xMin, bounds.xMin - tolerance, message + " xMin");
            Assert.LessOrEqual(viewport.xMax, bounds.xMax + tolerance, message + " xMax");
            Assert.GreaterOrEqual(viewport.yMin, bounds.yMin - tolerance, message + " yMin");
            Assert.LessOrEqual(viewport.yMax, bounds.yMax + tolerance, message + " yMax");
        }

        private static IEnumerator SetVisualResolution(int width, int height)
        {
            Screen.SetResolution(width, height, false);
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        private static IEnumerator CaptureViewport(string outputDir, string fileName, int width, int height)
        {
            yield return SetVisualResolution(width, height);
            yield return Capture(Path.Combine(outputDir, fileName), width, height);
            Debug.Log("[VisualSmokeViewport] " + fileName + " " + width + "x" + height);
        }

        private static IEnumerator Capture(string path, int width, int height)
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

            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
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

        private static void AssertMechanismActionIconsVisible()
        {
            string[] iconNames =
            {
                "PolicyActionIcon",
                "DiplomacyActionIcon",
                "BorderActionIcon",
                "EspionageActionIcon",
                "EnterWarModeActionIcon"
            };

            foreach (string iconName in iconNames)
            {
                GameObject icon = GameObject.Find(iconName);
                Assert.IsNotNull(icon, "Mechanism action icon should exist in visual smoke: " + iconName);
                Assert.IsTrue(icon.activeInHierarchy, "Mechanism action icon should be visible in visual smoke: " + iconName);

                Image image = icon.GetComponent<Image>();
                Assert.IsNotNull(image, "Mechanism action icon should use an Image background: " + iconName);
                Assert.IsTrue(image.enabled, "Mechanism action icon Image should be enabled: " + iconName);

                RectTransform rect = icon.GetComponent<RectTransform>();
                Assert.IsNotNull(rect, "Mechanism action icon should have a RectTransform: " + iconName);
                Assert.GreaterOrEqual(rect.rect.width, 12f, "Mechanism action icon should keep a readable width: " + iconName);
                Assert.GreaterOrEqual(rect.rect.height, 12f, "Mechanism action icon should keep a readable height: " + iconName);

                Text glyph = icon.GetComponentInChildren<Text>(true);
                Assert.IsNotNull(glyph, "Mechanism action icon should include a glyph label: " + iconName);
                Assert.IsFalse(string.IsNullOrWhiteSpace(glyph.text), "Mechanism action icon glyph should not be empty: " + iconName);
            }
        }

        private static void AssertTerrainLensVisible()
        {
            RegionController[] regions = Object.FindObjectsOfType<RegionController>();
            Assert.AreEqual(56, regions.Length, "Terrain lens visual smoke should keep the real 56 region surfaces.");

            int visibleRegionSurfaces = 0;
            int terrainShadowCount = 0;
            for (int i = 0; i < regions.Length; i++)
            {
                RegionController region = regions[i];
                if (region == null || region.gameObject == null) continue;
                if (!region.gameObject.name.StartsWith("RegionSurface_")) continue;

                MeshRenderer renderer = region.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.enabled)
                {
                    visibleRegionSurfaces++;
                }

                if (region.HasTerrainShadowVisual)
                {
                    terrainShadowCount++;
                }
            }

            Assert.AreEqual(56, visibleRegionSurfaces, "Terrain lens should reveal all real region surfaces over the generated map.");
            Assert.AreEqual(56, terrainShadowCount, "Terrain lens should retain one 2.5D shadow layer per real region.");
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

        private static ArmyRuntimeState CreateVisualSmokeStressArmy(GameManager manager, ArmyRuntimeState template, string id, int supply)
        {
            Assert.IsNotNull(manager, "GameManager is required for dense VisualSmoke army creation.");
            Assert.IsNotNull(manager.World, "WorldState is required for dense VisualSmoke army creation.");
            Assert.IsNotNull(manager.World.Map, "MapState is required for dense VisualSmoke army creation.");
            Assert.IsNotNull(template, "A template army is required for dense VisualSmoke army creation.");

            ArmyRuntimeState existing;
            if (manager.World.Map.TryGetArmy(id, out existing) && existing != null)
            {
                existing.ownerFactionId = manager.State.playerFactionId;
                existing.locationRegionId = template.locationRegionId;
                existing.targetRegionId = null;
                existing.route = new List<string>();
                existing.task = ArmyTask.Idle;
                existing.unitId = template.unitId;
                existing.soldiers = Mathf.Max(1, template.soldiers);
                existing.morale = template.morale;
                existing.supply = supply;
                existing.movementPoints = Mathf.Max(template.movementPoints, 1);
                existing.engagementId = null;
                manager.World.Map.MoveArmyToRegion(existing.id, template.locationRegionId);
                return existing;
            }

            ArmyRuntimeState army = new ArmyRuntimeState
            {
                id = id,
                ownerFactionId = manager.State.playerFactionId,
                locationRegionId = template.locationRegionId,
                targetRegionId = null,
                route = new List<string>(),
                task = ArmyTask.Idle,
                unitId = template.unitId,
                soldiers = Mathf.Max(1, template.soldiers),
                morale = template.morale,
                supply = supply,
                movementPoints = Mathf.Max(template.movementPoints, 1),
                engagementId = null
            };
            manager.World.Map.AddArmy(army);
            return army;
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
