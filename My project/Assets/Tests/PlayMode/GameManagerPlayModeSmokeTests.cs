using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace WanChaoGuiYi.Tests
{
    public sealed class GameManagerPlayModeSmokeTests
    {
        [UnityTearDown]
        public IEnumerator TearDownPlayModeSmokeObjects()
        {
            DestroyDemoBootstrapRuntimeObjects();
            yield return null;
        }

        [Test]
        public void RegionMeshBuilderRejectsInvalidPolygonsInsteadOfUsingFanFallback()
        {
            MapRegionShapeDefinition invalidShape = new MapRegionShapeDefinition
            {
                id = "invalid_bow_tie_shape",
                regionId = "invalid_bow_tie",
                boundary = new[]
                {
                    new MapPoint { x = 0f, y = 0f },
                    new MapPoint { x = 1f, y = 1f },
                    new MapPoint { x = 0f, y = 1f },
                    new MapPoint { x = 1f, y = 0f }
                }
            };

            Assert.IsNull(RegionMeshBuilder.Build(invalidShape), "Self-intersecting region boundaries should fail mesh build instead of silently falling back to fan triangulation.");
        }

        [UnityTest]
        public IEnumerator GameManagerBootstrapsWarLoopAndRebindsAfterRestart()
        {
            GameObject root = new GameObject("PlayModeSmoke_GameManager");
            GameManager manager = root.AddComponent<GameManager>();

            yield return null;

            AssertBootstrapped(manager);
            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_player_1", out playerArmy), "Player army should exist.");
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_enemy_1", out enemyArmy), "Enemy army should exist.");
            Assert.IsTrue(manager.StartPlayerAttack(enemyArmy.locationRegionId), "Initial map-led war command should be issued from Unity attack interface.");
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
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_player_1", out playerArmy), "Player army should exist after restart.");
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_enemy_1", out enemyArmy), "Enemy army should exist after restart.");
            Assert.IsTrue(manager.StartPlayerAttack(enemyArmy.locationRegionId), "War command should still work after StartNewGame rebinding.");
            manager.NextTurn();
            AssertHasLog(manager.State, "接敌");
            AssertNoLog(manager.State, "战斗结束");
            manager.NextTurn();
            AssertHasLog(manager.State, "战斗结束");
            AssertHasLog(manager.State, "收入 金钱");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DefaultStartUsesContiguousOwnersAndFogOfWar()
        {
            GameObject root = new GameObject("PlayModeSmoke_DefaultStartFog");
            GameManager manager = root.AddComponent<GameManager>();

            yield return null;

            AssertBootstrapped(manager);
            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist.");
            AssertOwnedRegionsAreConnected(manager, playerFaction);
            for (int i = 0; i < manager.State.factions.Count; i++)
            {
                FactionState faction = manager.State.factions[i];
                if (faction != null && faction.regionIds.Count > 0)
                {
                    AssertOwnedRegionsAreConnected(manager, faction);
                }
            }

            string adjacentForeignRegionId = FirstAdjacentForeignRegion(manager, playerFaction);
            RegionState adjacentForeignRegion = manager.State.FindRegion(adjacentForeignRegionId);
            Assert.AreEqual(VisibilityState.Known, adjacentForeignRegion.visibilityState, "Adjacent foreign frontier should be known at start.");

            string distantForeignRegionId = FirstDistantForeignRegion(manager, playerFaction);
            RegionState distantForeignRegion = manager.State.FindRegion(distantForeignRegionId);
            Assert.AreEqual(VisibilityState.Hidden, distantForeignRegion.visibilityState, "Non-adjacent foreign region should stay hidden until scouting.");

            FactionState targetFaction = manager.State.FindFaction(distantForeignRegion.ownerFactionId);
            Assert.IsNotNull(targetFaction, "Distant foreign region should have an owner faction.");
            FactionState player = manager.State.FindFaction(manager.State.playerFactionId);
            player.talentIds.Add("playmode_scout");
            player.money = 999;
            EspionageSystem espionage = manager.GetComponent<EspionageSystem>();
            EspionageResult result = espionage.StartOperation(manager.Context, player.id, targetFaction.id, EspionageActionType.ScoutIntel, distantForeignRegionId);
            Assert.IsTrue(result.success, "Scout operation should start against a hidden target region.");
            EspionageOperation operation = manager.State.activeOperations[manager.State.activeOperations.Count - 1];
            operation.progress = 100;
            operation.detectionRisk = 0;
            espionage.ExecuteTurn(manager.Context);

            Assert.AreEqual(VisibilityState.Scouted, distantForeignRegion.visibilityState, "Scout intel should mark the target region as scouted.");
            RegionRuntimeState runtimeRegion;
            Assert.IsTrue(manager.World.Map.TryGetRegion(distantForeignRegionId, out runtimeRegion), "Runtime region should exist for scouted region.");
            Assert.AreEqual(VisibilityState.Scouted, runtimeRegion.visibilityState, "Scout intel should keep runtime visibility in sync.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WarCommandsDriveArmyStateAndEngagementRules()
        {
            GameObject root = new GameObject("PlayModeSmoke_WarCommands");
            GameManager manager = root.AddComponent<GameManager>();

            yield return null;

            AssertBootstrapped(manager);
            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_player_1", out playerArmy), "Player army should exist.");
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_enemy_1", out enemyArmy), "Enemy army should exist.");

            string startRegionId = playerArmy.locationRegionId;
            string enemyRegionId = enemyArmy.locationRegionId;
            Assert.IsTrue(manager.MoveArmy(playerArmy.id, enemyRegionId), "MoveArmy should issue a route command.");
            Assert.AreEqual(ArmyTask.Move, playerArmy.task);
            Assert.AreEqual(enemyRegionId, playerArmy.targetRegionId);
            Assert.GreaterOrEqual(playerArmy.route.Count, 2);

            Assert.IsTrue(manager.StopArmy(playerArmy.id), "StopArmy should cancel a non-engaged route.");
            Assert.AreEqual(ArmyTask.Idle, playerArmy.task);
            Assert.IsNull(playerArmy.targetRegionId);
            Assert.AreEqual(0, playerArmy.route.Count);

            Assert.IsTrue(manager.MoveArmy(playerArmy.id, enemyRegionId), "MoveArmy should be reusable after stop.");
            manager.NextTurn();
            Assert.AreEqual(enemyRegionId, playerArmy.locationRegionId);
            Assert.IsNotNull(playerArmy.engagementId, "Arriving at the hostile army should create an engagement.");
            Assert.IsFalse(manager.MoveArmy(playerArmy.id, startRegionId), "Engaged armies should reject normal movement.");
            Assert.IsFalse(manager.StopArmy(playerArmy.id), "Engaged armies should reject stop commands.");

            bool retreatIssued = manager.RetreatArmy(playerArmy.id, startRegionId);
            if (retreatIssued)
            {
                Assert.AreEqual(ArmyTask.Retreat, playerArmy.task);
                Assert.AreEqual(startRegionId, playerArmy.targetRegionId);
                manager.NextTurn();
                Assert.AreEqual(startRegionId, playerArmy.locationRegionId);
                Assert.IsNull(playerArmy.engagementId);
            }
            else
            {
                Assert.IsNotNull(playerArmy.engagementId, "Failed retreat should not clear engagement state.");
                manager.NextTurn();
                AssertHasLog(manager.State, "战斗结束");
            }

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WarModeOperationalLayerShowsRouteContactOccupationAndBattleReport()
        {
            GameObject root = new GameObject("PlayModeSmoke_WarOperationalLayer");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);
            GameObject visualsRoot = new GameObject("PlayModeSmoke_WarEntityVisuals");
            DemoEntityVisualSpawner spawner = visualsRoot.AddComponent<DemoEntityVisualSpawner>();
            spawner.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            MainMapUI mainMapUI = Object.FindObjectOfType<MainMapUI>();
            Assert.IsNotNull(mainMapUI, "UISetup should create MainMapUI for war operational smoke.");

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_player_1", out playerArmy), "Player army should exist.");
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_enemy_1", out enemyArmy), "Enemy army should exist.");
            playerArmy.soldiers = 20000;
            enemyArmy.soldiers = 100;
            playerArmy.morale = 100;
            enemyArmy.morale = 30;
            playerArmy.supply = StrategyCausalRules.WarMovementSupplyCost;
            enemyArmy.supply = 80;
            SyncLegacyArmySoldiers(manager, playerArmy.id, playerArmy.soldiers);
            SyncLegacyArmySoldiers(manager, enemyArmy.id, enemyArmy.soldiers);

            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, enemyArmy.locationRegionId, null));
            yield return null;
            Button mechanismButton = GameObject.Find("MechanismButton").GetComponent<Button>();
            mechanismButton.onClick.Invoke();
            yield return null;
            Button enterWarModeButton = GameObject.Find("EnterWarModeButton").GetComponent<Button>();
            Assert.IsTrue(enterWarModeButton.interactable, "Adjacent target should allow explicit war mode entry.");
            enterWarModeButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(MapInteractionMode.War, mainMapUI.CurrentMode, "War operational layer must be reached through SelectionContext.");
            Assert.AreEqual(enemyArmy.locationRegionId, mainMapUI.CurrentSelectionContext.selectedRegionId);
            Assert.AreEqual(playerArmy.id, mainMapUI.CurrentSelectionContext.selectedArmyId, "War SelectionContext should bind the operable army as well as the target.");
            Assert.IsTrue(mainMapUI.CurrentSelectionContext.HasAvailableAction("dispatch_attack"));
            Button attackButton = GameObject.Find("AttackButton").GetComponent<Button>();
            Assert.IsTrue(attackButton.interactable, "Attack button should enable only after explicit war mode entry.");
            attackButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(ArmyTask.Move, playerArmy.task, "War dispatch should move the operable player army.");
            Assert.IsNotNull(GameObject.Find("Army_army_player_1"), "Moving player army should be visible.");
            Assert.IsNull(GameObject.Find("Army_army_enemy_1"), "Idle enemy army should still stay hidden before contact.");
            GameObject routeObject = GameObject.Find("WarRoute_army_player_1");
            Assert.IsNotNull(routeObject, "Dispatch should render a route line.");
            LineRenderer routeLine = routeObject.GetComponent<LineRenderer>();
            Assert.IsNotNull(routeLine, "War route should keep a LineRenderer.");
            Assert.Greater(routeLine.startWidth, 0.08f, "Low projected supply should make the route line visually heavier.");
            Assert.IsNotNull(GameObject.Find("WarRouteUnderlay_army_player_1"), "Dispatch should render a wider pressure underlay.");
            GameObject routeContactNode = GameObject.Find("WarRouteContactNode_army_player_1");
            Assert.IsNotNull(routeContactNode, "Dispatch should render a contact node on the projected route.");
            LineRenderer routeContactRing = routeContactNode.GetComponent<LineRenderer>();
            Assert.IsNotNull(routeContactRing, "Projected contact node should use a pulsing ring renderer.");
            float routeContactWidthBefore = routeContactRing.startWidth;
            GameObject routePressureObject = GameObject.Find("WarRoutePressureLabel_army_player_1");
            Assert.IsNotNull(routePressureObject, "Dispatch should render a route pressure label.");
            TextMesh routePressure = routePressureObject.GetComponent<TextMesh>();
            Assert.IsNotNull(routePressure, "Route pressure label should use TextMesh.");
            Assert.IsTrue(routePressure.text.Contains("补给压力"), "Route label should expose supply pressure on the map.");
            Assert.IsTrue(routePressure.text.Contains(StrategyCausalRules.DepletedSupplyBattlePowerPercent + "%"), "Route label should show depleted-supply battle power pressure.");
            Assert.IsNotNull(GameObject.Find("WarTargetHighlight_" + enemyArmy.locationRegionId), "Dispatch should highlight the selected target region.");
            TextMesh armyInfo = GameObject.Find("ArmyInfo_army_player_1").GetComponent<TextMesh>();
            Assert.IsTrue(armyInfo.text.Contains("主将"), "Operable army label should include general information.");
            Assert.IsTrue(armyInfo.text.Contains("兵力"), "Operable army label should include soldier information.");
            yield return AssertLineWidthPulse(routeContactRing, routeContactWidthBefore, 0.001f, "Projected contact node should pulse to show route pressure is active.");

            manager.NextTurn();
            yield return null;

            Assert.IsNotNull(playerArmy.engagementId, "Arrival should create an engagement.");
            Assert.IsNotNull(GameObject.Find("Army_army_enemy_1"), "Enemy army should appear once contact makes it relevant.");
            GameObject contactMarker = FindObjectNameStartsWith("WarContactMarker_");
            Assert.IsNotNull(contactMarker, "Contact should create a map marker.");
            LineRenderer contactRing = contactMarker.GetComponent<LineRenderer>();
            Assert.IsNotNull(contactRing, "Contact marker should use a pulsing ring renderer.");
            float contactWidthBefore = contactRing.startWidth;
            Assert.IsNotNull(GameObject.Find("WarOccupationMarker_" + enemyArmy.locationRegionId), "Contested target should create an occupation marker.");
            yield return AssertLineWidthPulse(contactRing, contactWidthBefore, 0.001f, "Contact marker should keep a sustained frontline pressure pulse.");

            manager.NextTurn();
            yield return null;

            GameObject battleReport = GameObject.Find("BattleReportPanel");
            Assert.IsNotNull(battleReport, "Battle resolution should open the battle report panel.");
            Text details = battleReport.transform.Find("DetailsText").GetComponent<Text>();
            Assert.IsTrue(details.text.Contains("兵力变化"), "Battle report should show soldier changes.");
            Assert.IsTrue(details.text.Contains("补给修正"), "Battle report should expose supply pressure as a first-class result line.");
            Assert.IsTrue(details.text.Contains(StrategyCausalRules.DepletedSupplyBattlePowerPercent + "%"), "Battle report should show depleted-supply combat modifier.");
            Assert.IsTrue(details.text.Contains("最低补给 0"), "Battle report should show the lowest supply at battle resolution.");
            Assert.IsTrue(details.text.Contains("占领结果"), "Battle report should show occupation result.");
            Assert.IsTrue(details.text.Contains("→"), "Battle report should name the occupation owner change.");
            Assert.IsTrue(details.text.Contains("治理影响"), "Battle report should show governance impact.");
            Assert.IsTrue(details.text.Contains("税粮"), "Battle report should explain occupied-region tax and food impact.");
            Assert.IsTrue(details.text.Contains("负反馈"), "Battle report should explain negative governance feedback.");
            Assert.IsTrue(details.text.Contains("合法性"), "Battle report should explain occupation legitimacy pressure.");
            AssertBattleReportVisualComponents(true);
            Assert.AreEqual(manager.State.playerFactionId, manager.State.FindRegion(enemyArmy.locationRegionId).ownerFactionId, "Boosted player army should occupy the target for this smoke.");
            Assert.AreEqual(MapInteractionMode.Governance, mainMapUI.CurrentMode, "Occupied selected target should rebuild into governance mode.");
            Assert.AreEqual(enemyArmy.locationRegionId, mainMapUI.CurrentSelectionContext.selectedRegionId, "Selection should remain on the occupied region after context refresh.");
            Assert.IsTrue(mainMapUI.CurrentSelectionContext.isFriendly, "Occupied selected target should become a friendly governance selection.");
            Assert.IsNull(mainMapUI.CurrentSelectionContext.selectedArmyId, "Occupied selected target should clear the stale dispatch army.");
            Assert.IsFalse(attackButton.interactable, "Occupied selected target should disable stale attack dispatch.");
            Assert.IsNotNull(GameObject.Find("WarOccupationMarker_" + enemyArmy.locationRegionId), "Occupied target should keep an occupation marker after battle resolution.");

            Object.Destroy(visualsRoot);
            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BattleReportSettlesNoOccupationPathWithoutPendingPlaceholders()
        {
            GameObject root = new GameObject("PlayModeSmoke_NoOccupationBattleReport");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_player_1", out playerArmy), "Player army should exist.");
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_enemy_1", out enemyArmy), "Enemy army should exist.");

            BattleResult result = new BattleResult
            {
                attackerArmyId = playerArmy.id,
                defenderArmyId = enemyArmy.id,
                battleRegionId = enemyArmy.locationRegionId,
                attackerPower = 420,
                defenderPower = 680,
                attackerSupplyPowerPercent = StrategyCausalRules.LowSupplyBattlePowerPercent,
                defenderSupplyPowerPercent = 100,
                attackerLowestSupply = 10,
                defenderLowestSupply = 80,
                attackerSoldiersBefore = 1200,
                attackerSoldiersAfter = 540,
                defenderSoldiersBefore = 1600,
                defenderSoldiersAfter = 1440,
                attackerWon = false
            };

            manager.Events.Publish(new GameEvent(GameEventType.BattleResolved, "no_occupation_smoke", result));
            yield return null;

            GameObject battleReport = GameObject.Find("BattleReportPanel");
            Assert.IsNotNull(battleReport, "BattleResolved should open the battle report panel.");
            Text details = battleReport.transform.Find("DetailsText").GetComponent<Text>();
            Assert.IsFalse(details.text.Contains("待结算"), "No-occupation battle report should not leave pending placeholders.");
            Assert.IsTrue(details.text.Contains("本次战斗未改变地区归属"), "No-occupation battle report should settle the occupation line.");
            Assert.IsTrue(details.text.Contains("无新增占领治理影响"), "No-occupation battle report should settle the governance line.");
            Assert.IsTrue(details.text.Contains("补给修正"), "Battle report should still show supply pressure on no-occupation outcomes.");
            AssertBattleReportVisualComponents(false);
            GameObject.Find("FocusBattleReportRegionButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            AssertPanelVisible("RegionPanel", "Battle report focus should select and open the battle region.");
            RegionDefinition focusedDefinition = manager.Data.GetRegion(enemyArmy.locationRegionId);
            Assert.IsTrue(GameObject.Find("RegionNameText").GetComponent<Text>().text.Contains(focusedDefinition.name), "Battle report focus should keep the battle region as the selected region.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DemoBootstrapCreatesRunnableMapSkeleton()
        {
            List<string> fallbackLogs = new List<string>();
            Application.LogCallback logHandler = (condition, stackTrace, type) =>
            {
                if (condition != null && (condition.Contains("Using legacy node fallback") || condition.Contains("Missing usable map region shape")))
                {
                    fallbackLogs.Add(condition);
                }
            };
            Application.logMessageReceived += logHandler;

            try
            {
                GameObject bootstrapRoot = new GameObject("PlayModeSmoke_DemoBootstrap");
                bootstrapRoot.AddComponent<DemoSceneBootstrap>();

                yield return null;

                GameManager manager = Object.FindObjectOfType<GameManager>();
                AssertBootstrapped(manager);
                MapSetup mapSetup = Object.FindObjectOfType<MapSetup>();
                Assert.IsNotNull(mapSetup, "Demo bootstrap should create MapSetup.");
                Assert.IsNotNull(Object.FindObjectOfType<MapRenderer>(), "Demo bootstrap should create MapRenderer.");
                Assert.IsNotNull(Object.FindObjectOfType<CameraController>(), "Demo bootstrap should create CameraController.");
                Assert.IsNotNull(Object.FindObjectOfType<UISetup>(), "Demo bootstrap should create UISetup.");
                Assert.IsNotNull(Object.FindObjectOfType<MainMapUI>(), "UISetup should wire MainMapUI.");
                Assert.IsNotNull(Object.FindObjectOfType<TechPanel>(), "UISetup should wire TechPanel.");
                Assert.IsNotNull(Object.FindObjectOfType<WeatherPanel>(), "UISetup should wire WeatherPanel.");
                Assert.IsNotNull(Object.FindObjectOfType<MechanismPanel>(), "UISetup should wire MechanismPanel.");
                Assert.IsNotNull(Object.FindObjectOfType<AudioManager>(), "Demo bootstrap should create layered AudioManager.");
                Assert.IsNotNull(Object.FindObjectOfType<AudioEventBridge>(), "Demo bootstrap should bind game events to the audio system.");
                Assert.IsNotNull(Object.FindObjectOfType<AudioDebugHUD>(), "Demo bootstrap should create the runtime audio debug HUD.");
                Assert.IsNotNull(GameObject.Find("AudioDebugHUD"), "Audio debug HUD root should exist for mixer/pool inspection.");
                Assert.IsNotNull(GameObject.Find("AudioDebugHUDText"), "Audio debug HUD should expose live audio state text.");
                Assert.IsNotNull(EventSystem.current, "Runtime UI should create an EventSystem for real pointer-driven UGUI input.");
                Assert.IsNotNull(EventSystem.current.GetComponent<BaseInputModule>(), "Runtime EventSystem should include an input module for buttons and panels.");
                Assert.IsNotNull(GameObject.Find("Generated_Jiuzhou_Map_Background_Runtime"), "Demo bootstrap should use the generated full-map image as the runtime map background.");
                Assert.IsTrue(mapSetup.UsedGeneratedMapResource, "Runtime map background should load from Resources before falling back to Application.dataPath.");
                Assert.IsTrue(mapSetup.UsedMapRenderMetadataResource, "Runtime map projection metadata should load from Resources before falling back to Application.dataPath.");
                Assert.IsTrue(Resources.Load<Sprite>("Map/jiuzhou_generated_map") != null || Resources.Load<Texture2D>("Map/jiuzhou_generated_map") != null, "Generated map should be available as a build-safe Resources image.");
                Assert.IsNotNull(Resources.Load<TextAsset>("Data/map_render_metadata"), "Generated map metadata should be available as a build-safe Resources text asset.");
                Assert.IsTrue(Resources.Load<Sprite>("Art/Icons/Units/infantry") != null || Resources.Load<Texture2D>("Art/Icons/Units/infantry") != null, "Unit icon art should be available from Resources for build-safe war overlays.");
                Assert.IsTrue(Resources.Load<Sprite>("Art/Portraits/qin_shi_huang") != null || Resources.Load<Texture2D>("Art/Portraits/qin_shi_huang") != null, "Emperor portrait art should be available from Resources for build-safe portrait strips.");
                Assert.IsNull(GameObject.Find("Emperor_qin_shi_huang"), "Demo bootstrap should not place every emperor portrait on the map before a gameplay call asks for it.");
                Assert.IsNull(GameObject.Find("Army_army_player_1"), "Demo bootstrap should not show idle armies before a gameplay command makes them relevant.");
                Assert.IsNull(GameObject.Find("Army_army_enemy_1"), "Demo bootstrap should not show idle enemy armies before contact or a command makes them relevant.");
                AssertRegionShapeDataIsRefined(manager);
                Assert.IsFalse(mapSetup.AllowNodeFallback, "Stage A must not rely on legacy node fallback.");
                Assert.AreEqual(56, manager.Data.Regions.Count, "Stage A closure requires the full 56-region map data set.");
                Assert.AreEqual(56, mapSetup.RegionSurfaceCount, "MapSetup should build one interaction surface for each real region.");
                Assert.AreEqual(0, mapSetup.LegacyNodeFallbackCount, "MapSetup should not create legacy fallback nodes.");
                Assert.AreEqual(0, mapSetup.MissingUsableShapeCount, "Every region should have a usable refined shape.");
                Assert.AreEqual(0, mapSetup.MeshBuildFailureCount, "Every refined region shape should build a mesh.");
                Assert.AreEqual(0, fallbackLogs.Count, "Map bootstrap logs should not contain legacy fallback warnings or missing-shape errors.");

                RegionController[] regions = Object.FindObjectsOfType<RegionController>();
                Assert.AreEqual(56, regions.Length, "Demo bootstrap should build exactly the 56 playable map surfaces.");
                int regionSurfaceCount = 0;
                int fallbackNodeCount = 0;
                int hiddenInteractionSurfaces = 0;
                for (int i = 0; i < regions.Length; i++)
                {
                    if (regions[i] == null || regions[i].gameObject == null) continue;
                    if (!regions[i].gameObject.name.StartsWith("RegionSurface_"))
                    {
                        if (regions[i].gameObject.name.StartsWith("Region_"))
                        {
                            fallbackNodeCount++;
                        }
                        continue;
                    }

                    regionSurfaceCount++;

                    MeshRenderer meshRenderer = regions[i].GetComponent<MeshRenderer>();
                    if (meshRenderer != null && !meshRenderer.enabled)
                    {
                        hiddenInteractionSurfaces++;
                    }
                }

                Assert.AreEqual(56, regionSurfaceCount, "Runtime map should use RegionSurface_* objects rather than fake node geography.");
                Assert.AreEqual(0, fallbackNodeCount, "Runtime map should not create legacy Region_* fallback nodes.");
                Assert.AreEqual(56, hiddenInteractionSurfaces, "Generated region surfaces should act as a hidden interaction layer over the full-map image.");
                Assert.AreEqual(56, CountSceneObjectsStartingWith("RegionTerrainShadow_"), "2.5D terrain depth should add one shadow layer per real region.");
                Assert.AreEqual(56, CountSceneObjectsStartingWith("SelectedRegionElevation_"), "2.5D selection should add one elevation layer per real region.");
                Assert.AreEqual(0, CountActiveSceneObjectsStartingWith("SelectedRegionElevation_"), "Selection elevation layers should stay hidden before a region is selected.");

                string elevationRegionId = manager.State.FindFaction(manager.State.playerFactionId).regionIds[0];
                manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, elevationRegionId, null));
                yield return null;
                GameObject selectedElevation = FindSceneObjectByNameIncludingInactive("SelectedRegionElevation_" + elevationRegionId);
                Assert.IsNotNull(selectedElevation, "Selected region should keep an elevation overlay object.");
                Assert.IsTrue(selectedElevation.activeInHierarchy, "Selected region elevation should become visible after RegionSelected.");
                float selectedElevationBaseScale = selectedElevation.transform.localScale.x;
                BattleResult focusResult = new BattleResult
                {
                    attackerArmyId = "army_player_1",
                    defenderArmyId = "army_enemy_1",
                    battleRegionId = elevationRegionId,
                    attackerPower = 720,
                    defenderPower = 520,
                    attackerSupplyPowerPercent = 100,
                    defenderSupplyPowerPercent = StrategyCausalRules.LowSupplyBattlePowerPercent,
                    attackerLowestSupply = 60,
                    defenderLowestSupply = 20,
                    attackerSoldiersBefore = 1200,
                    attackerSoldiersAfter = 1040,
                    defenderSoldiersBefore = 1000,
                    defenderSoldiersAfter = 780,
                    attackerWon = true
                };
                manager.Events.Publish(new GameEvent(GameEventType.BattleResolved, "focus_pulse_smoke", focusResult));
                yield return null;
                GameObject.Find("FocusBattleReportRegionButton").GetComponent<Button>().onClick.Invoke();
                yield return null;
                Assert.IsTrue(selectedElevation.activeInHierarchy, "Battle report focus should keep the real region elevation visible.");
                Assert.Greater(selectedElevation.transform.localScale.x, selectedElevationBaseScale, "Battle report focus should pulse the selected real-region elevation layer.");
                AssertAllProjectedRegionHits(manager);

                ArmyRuntimeState playerArmy;
                ArmyRuntimeState enemyArmy;
                Assert.IsTrue(manager.World.Map.TryGetArmy("army_player_1", out playerArmy), "Player army should exist for command-driven map visuals.");
                Assert.IsTrue(manager.World.Map.TryGetArmy("army_enemy_1", out enemyArmy), "Enemy army should exist for contact-driven map visuals.");
                Assert.IsTrue(manager.StartPlayerAttack(enemyArmy.locationRegionId), "Issuing a player attack should drive army visuals through a real command path.");
                yield return null;
                Assert.IsNotNull(GameObject.Find("Army_army_player_1"), "Moving player army should appear after ArmyMoveStarted.");
                Assert.IsNull(GameObject.Find("Army_army_enemy_1"), "Idle enemy army should remain hidden before contact.");

                manager.NextTurn();
                yield return null;
                Assert.IsNotNull(GameObject.Find("Army_army_player_1"), "Engaged player army should remain visible after contact.");
                Assert.IsNotNull(GameObject.Find("Army_army_enemy_1"), "Enemy army should appear once engagement makes it relevant.");
            }
            finally
            {
                DestroyDemoBootstrapRuntimeObjects();
                Application.logMessageReceived -= logHandler;
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator CameraControllerClampsZoomAndCenterToConfiguredBounds()
        {
            GameObject bootstrap = new GameObject("PlayModeSmoke_CameraController");
            bootstrap.AddComponent<DemoSceneBootstrap>();

            yield return null;

            CameraController controller = Object.FindObjectOfType<CameraController>();
            Assert.IsNotNull(controller, "Demo bootstrap should create CameraController.");

            Rect configuredBounds = Rect.MinMaxRect(-2f, -1f, 2f, 1f);
            controller.ConfigureBounds(configuredBounds);
            controller.ConfigureZoomLimits(4f, 8f);
            controller.SetZoom(12f);

            Assert.IsTrue(controller.UseBounds, "CameraController should enable bounds after ConfigureBounds.");
            Assert.AreEqual(8f, controller.CurrentZoom, 0.0001f, "Camera zoom should clamp to the configured maximum.");
            controller.SetZoom(1f);
            Assert.AreEqual(4f, controller.CurrentZoom, 0.0001f, "Camera zoom should clamp to the configured minimum.");
            Assert.AreEqual(-2f, controller.WorldBounds.xMin, 0.0001f);
            Assert.AreEqual(2f, controller.WorldBounds.xMax, 0.0001f);
            Assert.AreEqual(-1f, controller.WorldBounds.yMin, 0.0001f);
            Assert.AreEqual(1f, controller.WorldBounds.yMax, 0.0001f);

            controller.transform.position = new Vector3(9f, 9f, controller.transform.position.z);
            controller.ClampToBounds();
            Assert.LessOrEqual(controller.transform.position.x, configuredBounds.xMax + 0.0001f, "Camera x should clamp to the configured map bounds.");
            Assert.GreaterOrEqual(controller.transform.position.x, configuredBounds.xMin - 0.0001f, "Camera x should stay inside the configured map bounds.");
            Assert.LessOrEqual(controller.transform.position.y, configuredBounds.yMax + 0.0001f, "Camera y should clamp to the configured map bounds.");
            Assert.GreaterOrEqual(controller.transform.position.y, configuredBounds.yMin - 0.0001f, "Camera y should stay inside the configured map bounds.");
            Assert.AreEqual(configuredBounds.center.x, controller.transform.position.x, 0.0001f, "Camera should center when the viewport is wider than the configured bounds.");
            Assert.AreEqual(configuredBounds.center.y, controller.transform.position.y, 0.0001f, "Camera should center when the viewport is taller than the configured bounds.");

            controller.CenterOnRegion(new Vector2(8f, -3f));
            Assert.LessOrEqual(controller.transform.position.x, configuredBounds.xMax + 0.0001f, "CenterOnRegion should keep x within bounds.");
            Assert.GreaterOrEqual(controller.transform.position.x, configuredBounds.xMin - 0.0001f, "CenterOnRegion should clamp x within bounds.");
            Assert.LessOrEqual(controller.transform.position.y, configuredBounds.yMax + 0.0001f, "CenterOnRegion should keep y within bounds.");
            Assert.GreaterOrEqual(controller.transform.position.y, configuredBounds.yMin - 0.0001f, "CenterOnRegion should clamp y within bounds.");
            Assert.AreEqual(configuredBounds.center.x, controller.transform.position.x, 0.0001f, "CenterOnRegion should center when the viewport exceeds bounds.");
            Assert.AreEqual(configuredBounds.center.y, controller.transform.position.y, 0.0001f, "CenterOnRegion should center when the viewport exceeds bounds.");

            Rect largeBounds = Rect.MinMaxRect(-100f, -100f, 100f, 100f);
            controller.ConfigureBounds(largeBounds);
            controller.ConfigureZoomLimits(3f, 12f);
            controller.transform.position = new Vector3(0f, 0f, controller.transform.position.z);
            controller.SetZoom(10f);
            Camera camera = Camera.main;
            Assert.IsNotNull(camera, "CameraController smoke should have a main camera.");
            Vector3 focusPoint = new Vector3(Screen.width * 0.68f, Screen.height * 0.42f, 0f);
            Vector3 worldBeforeZoom = camera.ScreenToWorldPoint(focusPoint);
            controller.ZoomAroundScreenPoint(focusPoint, 5f);
            Vector3 worldAfterZoom = camera.ScreenToWorldPoint(focusPoint);
            Assert.AreEqual(worldBeforeZoom.x, worldAfterZoom.x, 0.02f, "Mouse-wheel zoom should keep the pointed map location stable on x.");
            Assert.AreEqual(worldBeforeZoom.y, worldAfterZoom.y, 0.02f, "Mouse-wheel zoom should keep the pointed map location stable on y.");

            controller.ConfigureSmoothFocus(0.05f, true);
            controller.SmoothCenterOnRegion(new Vector2(30f, -20f));
            Assert.IsTrue(controller.SmoothFocusActive, "Smooth focus should start an in-flight camera transition.");
            Assert.Greater(Mathf.Abs(controller.transform.position.x - 30f), 0.01f, "Smooth focus should not teleport to the target in the same frame.");
            yield return new WaitForSeconds(0.08f);
            Assert.IsFalse(controller.SmoothFocusActive, "Smooth focus should complete after its configured duration.");
            Assert.AreEqual(30f, controller.transform.position.x, 0.15f, "Smooth focus should reach the requested x when it is inside bounds.");
            Assert.AreEqual(-20f, controller.transform.position.y, 0.15f, "Smooth focus should reach the requested y when it is inside bounds.");

            controller.ConfigureSmoothFocus(0.5f, true);
            controller.SmoothCenterOnRegion(new Vector2(-30f, 20f));
            Assert.IsTrue(controller.SmoothFocusActive, "A second smooth focus should also become active.");
            controller.PanBy(Vector3.right);
            Assert.IsFalse(controller.SmoothFocusActive, "Manual pan should cancel an active smooth focus so the user keeps control.");

            controller.ConfigureZoomLimits(3f, 12f);
            controller.ConfigureZoomBands(5f, 8f);
            controller.SetZoom(4f);
            Assert.AreEqual(MapZoomBand.Detail, controller.CurrentZoomBand, "Close zoom should use the detail label density band.");
            controller.SetZoom(7f);
            Assert.AreEqual(MapZoomBand.Operation, controller.CurrentZoomBand, "Mid zoom should use the operation label density band.");
            controller.SetZoom(11f);
            Assert.AreEqual(MapZoomBand.Overview, controller.CurrentZoomBand, "Far zoom should use the overview label density band.");

            Vector3 referenceDragDelta = Vector3.zero;
            int[] widths = { 1024, 1280 };
            int[] heights = { 576, 720 };
            for (int i = 0; i < widths.Length; i++)
            {
                yield return WaitForResolutionAndLayout(widths[i], heights[i]);

                controller.ConfigureBounds(Rect.MinMaxRect(-64f, -36f, 64f, 36f));
                controller.ConfigureZoomLimits(3f, 12f);
                controller.SetZoom(8f);
                controller.CenterOnRegion(Vector2.zero);

                camera = Camera.main;
                Assert.IsNotNull(camera, "CameraController drag smoke should have a main camera.");
                Vector3 dragStart = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
                Vector3 dragEnd = new Vector3(Screen.width * 0.625f, Screen.height * 0.375f, 0f);
                Vector3 dragDelta = controller.CalculateScreenDragWorldDelta(dragStart, dragEnd);

                Assert.Less(dragDelta.x, -0.1f, widths[i] + "px drag should move the camera opposite the rightward pointer motion.");
                Assert.Greater(dragDelta.y, 0.1f, widths[i] + "px drag should move the camera opposite the downward pointer motion.");
                if (i == 0)
                {
                    referenceDragDelta = dragDelta;
                }
                else
                {
                    Assert.AreEqual(referenceDragDelta.x, dragDelta.x, 0.03f, "Same-aspect small viewports should keep horizontal drag feel consistent.");
                    Assert.AreEqual(referenceDragDelta.y, dragDelta.y, 0.03f, "Same-aspect small viewports should keep vertical drag feel consistent.");
                }

                Vector3 positionBeforeDrag = controller.transform.position;
                controller.PanByScreenDrag(dragStart, dragEnd);
                Assert.AreEqual(positionBeforeDrag.x + dragDelta.x, controller.transform.position.x, 0.03f, "Screen drag helper should apply the sampled world delta on x.");
                Assert.AreEqual(positionBeforeDrag.y + dragDelta.y, controller.transform.position.y, 0.03f, "Screen drag helper should apply the sampled world delta on y.");
                AssertCameraViewportInsideBounds(controller, "Screen drag should keep the full map viewport inside configured bounds at " + widths[i] + "x" + heights[i] + ".");

                Vector3 focusPointAtViewport = new Vector3(Screen.width * 0.72f, Screen.height * 0.38f, 0f);
                Vector3 worldBeforeViewportZoom = camera.ScreenToWorldPoint(focusPointAtViewport);
                controller.ZoomAroundScreenPoint(focusPointAtViewport, 6f);
                Vector3 worldAfterViewportZoom = camera.ScreenToWorldPoint(focusPointAtViewport);
                Assert.AreEqual(worldBeforeViewportZoom.x, worldAfterViewportZoom.x, 0.03f, "Viewport zoom should keep the pointed map location stable on x at " + widths[i] + "x" + heights[i] + ".");
                Assert.AreEqual(worldBeforeViewportZoom.y, worldAfterViewportZoom.y, 0.03f, "Viewport zoom should keep the pointed map location stable on y at " + widths[i] + "x" + heights[i] + ".");
                AssertCameraViewportInsideBounds(controller, "Viewport zoom should keep the full map viewport inside configured bounds at " + widths[i] + "x" + heights[i] + ".");

                controller.PanBy(new Vector3(-999f, 999f, 0f));
                AssertCameraViewportInsideBounds(controller, "Extreme pan should clamp the full map viewport inside configured bounds at " + widths[i] + "x" + heights[i] + ".");
            }

            DestroyDemoBootstrapRuntimeObjects(bootstrap);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MainMapUiBuildsSelectionContextWithoutImplicitAttack()
        {
            GameObject root = new GameObject("PlayModeSmoke_MapSelectionContext");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            MainMapUI mainMapUI = Object.FindObjectOfType<MainMapUI>();
            Assert.IsNotNull(mainMapUI, "UISetup should create MainMapUI.");
            Assert.AreEqual(MapInteractionMode.Governance, mainMapUI.CurrentMode, "Map should open in Governance mode.");
            Assert.IsNotNull(mainMapUI.CurrentSelectionContext, "MainMapUI should expose an empty SelectionContext before region selection.");
            Assert.IsNull(mainMapUI.CurrentSelectionContext.selectedRegionId, "Default selection context should not pick a region implicitly.");

            Button attackButton = GameObject.Find("AttackButton").GetComponent<Button>();
            Text selectionText = GameObject.Find("SelectionContextText").GetComponent<Text>();
            Assert.IsFalse(attackButton.interactable, "Attack button should stay disabled before explicit war dispatch mode.");
            Assert.IsTrue(selectionText.text.Contains("治理"), "HUD should expose the default map mode in player-facing copy.");
            Assert.IsTrue(selectionText.text.Contains("未选区"), "HUD should explain that no region is selected.");
            AssertNoVisibleUiCopyTokens();

            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction is required for selection context smoke.");
            string friendlyRegionId = playerFaction.regionIds[0];
            RegionState friendlyRegion = manager.State.FindRegion(friendlyRegionId);
            Assert.IsNotNull(friendlyRegion, "Friendly region should exist.");

            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, friendlyRegionId, null));
            yield return null;

            SelectionContext friendlyContext = mainMapUI.CurrentSelectionContext;
            Assert.AreEqual(MapInteractionMode.Governance, mainMapUI.CurrentMode, "Friendly region selection should remain in Governance mode.");
            Assert.AreEqual(friendlyRegionId, friendlyContext.selectedRegionId);
            Assert.AreEqual(friendlyRegion.ownerFactionId, friendlyContext.ownerFactionId);
            Assert.AreEqual(friendlyRegion.ownerFactionId, friendlyContext.targetFactionId);
            Assert.IsTrue(friendlyContext.isFriendly, "Friendly selection context should mark ownership.");
            Assert.IsFalse(friendlyContext.isHostile, "Friendly selection context should not be hostile.");
            Assert.AreEqual("friendly_region", friendlyContext.modeEntryReason);
            Assert.IsTrue(friendlyContext.HasAvailableAction("open_governance"), "Friendly selection should open governance actions.");
            Assert.IsTrue(friendlyContext.HasDisabledReasonContaining("war_mode"), "Friendly selection should not dispatch an attack.");
            Assert.IsFalse(attackButton.interactable, "Friendly selection should not enable map attack.");
            Assert.IsTrue(selectionText.text.Contains("己方"), "HUD should expose friendly selection feedback.");
            AssertNoVisibleUiCopyTokens();

            string enemyRegionId = FirstAdjacentForeignRegion(manager, playerFaction);
            RegionState enemyRegion = manager.State.FindRegion(enemyRegionId);
            ArmyRuntimeState playerArmy;
            Assert.IsTrue(manager.World.Map.TryGetArmy("army_player_1", out playerArmy), "Player runtime army should exist.");
            Assert.AreEqual(ArmyTask.Idle, playerArmy.task, "Selection context smoke starts with an idle army.");
            Assert.IsNull(playerArmy.targetRegionId, "Selection context smoke starts with no target.");

            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, enemyRegionId, null));
            yield return null;

            SelectionContext enemyContext = mainMapUI.CurrentSelectionContext;
            Assert.AreEqual(MapInteractionMode.Diplomacy, mainMapUI.CurrentMode, "Adjacent foreign region selection should enter diplomacy bridge mode first.");
            Assert.AreEqual(enemyRegionId, enemyContext.selectedRegionId);
            Assert.AreEqual(enemyRegion.ownerFactionId, enemyContext.ownerFactionId);
            Assert.AreEqual(enemyRegion.ownerFactionId, enemyContext.targetFactionId);
            Assert.IsFalse(enemyContext.isFriendly, "Foreign target should not be friendly.");
            Assert.IsTrue(enemyContext.isNeighbor, "Selected foreign target should be adjacent to player territory.");
            Assert.IsTrue(enemyContext.isHostile, "Foreign target should be marked hostile for map feedback.");
            Assert.AreEqual("neighbor_region", enemyContext.modeEntryReason);
            Assert.IsTrue(enemyContext.HasAvailableAction("open_diplomacy"), "Foreign selection should expose diplomacy first.");
            Assert.IsTrue(enemyContext.HasAvailableAction("enter_war_mode"), "Adjacent foreign selection should expose explicit war-mode entry.");
            Assert.IsTrue(enemyContext.HasDisabledReasonContaining("explicit_war_mode"), "Attack should remain disabled until explicit war mode.");
            Assert.IsFalse(attackButton.interactable, "Ordinary foreign region click should not enable attack.");
            Assert.IsTrue(selectionText.text.Contains("邻接"), "HUD should expose neighbor selection feedback.");
            AssertNoVisibleUiCopyTokens();

            attackButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(ArmyTask.Idle, playerArmy.task, "Clicking the attack listener outside war mode must not call StartPlayerAttack.");
            Assert.IsNull(playerArmy.targetRegionId, "Ordinary selection must not assign a player army target.");
            AssertHasLog(manager.State, "explicit war dispatch mode");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DistantHostileSelectionDisablesDispatchWithReason()
        {
            GameObject root = new GameObject("PlayModeSmoke_DistantHostileSelection");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            MainMapUI mainMapUI = Object.FindObjectOfType<MainMapUI>();
            Assert.IsNotNull(mainMapUI, "UISetup should create MainMapUI.");
            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist.");

            string distantRegionId = FirstDistantForeignRegion(manager, playerFaction);
            RegionState distantRegion = manager.State.FindRegion(distantRegionId);
            Assert.IsNotNull(distantRegion, "Distant hostile region should exist.");

            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, distantRegionId, null));
            yield return null;

            Button attackButton = GameObject.Find("AttackButton").GetComponent<Button>();
            SelectionContext distantContext = mainMapUI.CurrentSelectionContext;
            Assert.AreEqual(MapInteractionMode.Diplomacy, mainMapUI.CurrentMode, "Distant hostile selection should inspect diplomacy first.");
            Assert.AreEqual(distantRegionId, distantContext.selectedRegionId);
            Assert.AreEqual(distantRegion.ownerFactionId, distantContext.targetFactionId);
            Assert.IsFalse(distantContext.isFriendly, "Distant hostile target should not be friendly.");
            Assert.IsFalse(distantContext.isNeighbor, "Distant hostile target should not be adjacent.");
            Assert.IsTrue(distantContext.isHostile, "Distant foreign region should be hostile.");
            Assert.AreEqual("distant_foreign_region", distantContext.modeEntryReason);
            Assert.IsFalse(distantContext.HasAvailableAction("dispatch_attack"), "Distant hostile selection must not expose dispatch.");
            Assert.IsFalse(distantContext.HasAvailableAction("enter_war_mode"), "Distant hostile selection must not expose war-mode entry.");
            Assert.IsTrue(distantContext.HasDisabledReasonContaining("dispatch_requires_adjacent_target"), "Distant hostile selection should explain adjacency requirement.");
            Assert.IsFalse(attackButton.interactable, "Distant hostile selection should keep attack disabled.");

            mainMapUI.SetInteractionMode(MapInteractionMode.War);
            yield return null;

            SelectionContext warContext = mainMapUI.CurrentSelectionContext;
            Assert.AreEqual(MapInteractionMode.War, mainMapUI.CurrentMode, "Manual war mode should preserve the selected distant target context.");
            Assert.AreEqual(distantRegionId, warContext.selectedRegionId);
            Assert.IsFalse(warContext.HasAvailableAction("dispatch_attack"), "Distant hostile target should still not dispatch in war mode.");
            Assert.IsTrue(warContext.HasDisabledReasonContaining("dispatch_requires_adjacent_target"), "War mode should still explain adjacency requirement.");
            Assert.IsFalse(attackButton.interactable, "Attack should remain disabled for non-adjacent target.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RegionSidebarRefreshesStateChangesAndButtonPrerequisites()
        {
            GameObject root = new GameObject("PlayModeSmoke_RegionSidebarRefreshAndPrereqs");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            MainMapUI mainMapUI = Object.FindObjectOfType<MainMapUI>();
            Assert.IsNotNull(mainMapUI, "UISetup should create MainMapUI.");
            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist.");

            string friendlyRegionId = playerFaction.regionIds[0];
            RegionState friendlyRegion = manager.State.FindRegion(friendlyRegionId);
            Assert.IsNotNull(friendlyRegion, "Friendly region should exist.");
            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, friendlyRegionId, null));
            yield return null;
            Canvas.ForceUpdateCanvases();

            playerFaction.money = 0;
            playerFaction.food = 0;
            mainMapUI.SetInteractionMode(MapInteractionMode.Governance);
            yield return null;
            Canvas.ForceUpdateCanvases();

            Button pacifyButton = GameObject.Find("PacifyRegionButton").GetComponent<Button>();
            Button buildButton = GameObject.Find("BuildRegionBuildingButton").GetComponent<Button>();
            Text governanceOverview = GameObject.Find("GovernanceOverviewText").GetComponent<Text>();
            Assert.IsFalse(pacifyButton.interactable, "Pacification should be disabled before click when money or food is missing.");
            Assert.IsFalse(buildButton.interactable, "Building should be disabled before click when money is missing.");
            Assert.IsTrue(governanceOverview.text.Contains("安抚 不可用"), "Governance overview should explain disabled pacification before click.");
            Assert.IsTrue(governanceOverview.text.Contains("建造 不可用"), "Governance overview should explain disabled construction before click.");

            playerFaction.money = 1000;
            playerFaction.food = 1000;
            friendlyRegion.buildings = new List<string> { "slot_a", "slot_b", "slot_c" };
            mainMapUI.SetInteractionMode(MapInteractionMode.Governance);
            yield return null;
            Canvas.ForceUpdateCanvases();

            Assert.IsTrue(pacifyButton.interactable, "Pacification should become available once money and food exist.");
            Assert.IsFalse(buildButton.interactable, "Building should remain disabled when the region has no free building slot.");
            Assert.IsTrue(governanceOverview.text.Contains("建造 不可用"), "Governance overview should keep construction availability aligned with slots.");

            string targetRegionId = FirstAdjacentForeignRegion(manager, playerFaction);
            RegionState targetRegion = manager.State.FindRegion(targetRegionId);
            Assert.IsNotNull(targetRegion, "Foreign target region should exist.");
            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, targetRegionId, null));
            yield return null;

            targetRegion.ownerFactionId = playerFaction.id;
            targetRegion.integration = 25;
            targetRegion.rebellionRisk = 72;
            targetRegion.taxContributionPercent = StrategyCausalRules.OccupiedContributionPercent;
            targetRegion.foodContributionPercent = StrategyCausalRules.OccupiedContributionPercent;
            manager.Events.Publish(new GameEvent(GameEventType.RegionOccupied, targetRegionId, new RegionOccupiedPayload
            {
                regionId = targetRegionId,
                previousOwnerFactionId = "former_owner",
                newOwnerFactionId = playerFaction.id,
                engagementId = "test_refresh"
            }));
            manager.Events.Publish(new GameEvent(GameEventType.GovernanceImpactApplied, targetRegionId, new GovernanceImpactPayload
            {
                regionId = targetRegionId,
                integration = targetRegion.integration,
                taxContributionPercent = targetRegion.taxContributionPercent,
                foodContributionPercent = targetRegion.foodContributionPercent,
                rebellionRisk = targetRegion.rebellionRisk,
                localPower = targetRegion.localPower,
                annexationPressure = targetRegion.annexationPressure,
                legitimacyBefore = playerFaction.legitimacy,
                legitimacyAfter = playerFaction.legitimacy
            }));
            yield return null;
            Canvas.ForceUpdateCanvases();

            Assert.IsTrue(GameObject.Find("OwnerText").GetComponent<Text>().text.Contains(playerFaction.name), "Selected sidebar should refresh owner after occupation.");
            Assert.IsTrue(GameObject.Find("IntegrationText").GetComponent<Text>().text.Contains("25%"), "Selected sidebar should refresh integration after governance impact.");
            Assert.IsTrue(GameObject.Find("RebellionText").GetComponent<Text>().text.Contains("72%"), "Selected sidebar should refresh rebellion after governance impact.");

            GameObject.Find("NextTurnButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Canvas.ForceUpdateCanvases();
            AssertPanelHidden("RegionPanel", "Advance turn should close the selected region sidebar and not reopen it from turn-end refresh.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MechanismPanelUsesSelectionContextForDiplomacyBridgeAndWarMode()
        {
            GameObject root = new GameObject("PlayModeSmoke_DiplomacyBridge");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            MainMapUI mainMapUI = Object.FindObjectOfType<MainMapUI>();
            Assert.IsNotNull(mainMapUI, "UISetup should create MainMapUI.");
            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist.");
            FactionState firstOtherFaction = FirstOtherFaction(manager, playerFaction.id);
            Assert.IsNotNull(firstOtherFaction, "Stage C smoke needs a non-player faction.");

            string targetRegionId = FirstAdjacentForeignRegionExcludingFaction(manager, playerFaction, firstOtherFaction.id);
            RegionState targetRegion = manager.State.FindRegion(targetRegionId);
            Assert.IsNotNull(targetRegion, "Selected diplomacy target region should exist.");
            FactionState selectedTargetFaction = manager.State.FindFaction(targetRegion.ownerFactionId);
            Assert.IsNotNull(selectedTargetFaction, "Selected diplomacy target faction should exist.");
            Assert.AreNotEqual(firstOtherFaction.id, selectedTargetFaction.id, "Stage C smoke must prove selection target is not the legacy first-other faction.");

            DiplomacySystem diplomacySystem = manager.GetComponent<DiplomacySystem>();
            DiplomaticRelation selectedRelation = diplomacySystem.FindRelation(manager.Context, playerFaction.id, selectedTargetFaction.id);
            DiplomaticRelation firstOtherRelation = diplomacySystem.FindRelation(manager.Context, playerFaction.id, firstOtherFaction.id);
            Assert.IsNotNull(selectedRelation, "Selected target relation should exist.");
            Assert.IsNotNull(firstOtherRelation, "First-other relation should exist.");

            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, targetRegionId, null));
            yield return null;

            Assert.AreEqual(MapInteractionMode.Diplomacy, mainMapUI.CurrentMode, "Foreign neighbor selection should open diplomacy bridge before war.");
            Assert.AreEqual(targetRegionId, mainMapUI.CurrentSelectionContext.selectedRegionId);
            Assert.AreEqual(selectedTargetFaction.id, mainMapUI.CurrentSelectionContext.targetFactionId);

            Button mechanismButton = GameObject.Find("MechanismButton").GetComponent<Button>();
            mechanismButton.onClick.Invoke();
            yield return null;

            Text details = GameObject.Find("MechanismPanel").transform.Find("DetailsText").GetComponent<Text>();
            Assert.IsTrue(details.text.Contains(selectedTargetFaction.name), "Diplomacy bridge should display the selected target faction.");
            Assert.IsTrue(details.text.Contains(targetRegionId), "Diplomacy bridge should display the selected region context.");
            Assert.IsTrue(details.text.Contains("外交来源:"), "Diplomacy bridge should expose diplomacy source notes.");
            Assert.IsTrue(details.text.Contains("封关来源:"), "Diplomacy bridge should expose border source notes.");
            Assert.IsTrue(details.text.Contains("封关代价:"), "Diplomacy bridge should expose border action costs.");

            Button espionageButton = GameObject.Find("EspionageActionButton").GetComponent<Button>();
            Assert.IsTrue(espionageButton.interactable, "Selected target should enable scout intel.");
            int scoutedBefore = CountScoutedRegions(manager, selectedTargetFaction);
            playerFaction.talentIds.Add("selection_scout_agent");
            playerFaction.money = 999;
            int operationsBefore = manager.State.activeOperations.Count;
            espionageButton.onClick.Invoke();
            Assert.Greater(manager.State.activeOperations.Count, operationsBefore, "Selected espionage action should create an operation.");
            EspionageOperation scoutOperation = manager.State.activeOperations[manager.State.activeOperations.Count - 1];
            Assert.AreEqual(targetRegionId, scoutOperation.targetEntityId, "Selected espionage should bind scout intel to the selected region.");
            scoutOperation.progress = 100;
            scoutOperation.detectionRisk = 0;
            manager.GetComponent<EspionageSystem>().ExecuteTurn(manager.Context);
            Assert.AreEqual(VisibilityState.Scouted, targetRegion.visibilityState, "Selected espionage should scout the selected target region.");
            Assert.AreEqual(scoutedBefore + 1, CountScoutedRegions(manager, selectedTargetFaction), "Selected espionage should not reveal the whole target faction.");

            Button borderButton = GameObject.Find("BorderControlButton").GetComponent<Button>();
            Assert.IsTrue(borderButton.interactable, "Adjacent target should enable border control.");
            int moneyBeforeBorder = playerFaction.money;
            int foodBeforeBorder = playerFaction.food;
            int grudgeBeforeBorder = selectedRelation.grudge;
            borderButton.onClick.Invoke();
            Assert.Less(playerFaction.money, moneyBeforeBorder, "Border control should spend money.");
            Assert.Less(playerFaction.food, foodBeforeBorder, "Border control should spend food.");
            Assert.Greater(selectedRelation.grudge, grudgeBeforeBorder, "Border control should affect the selected relation.");
            Assert.AreNotEqual(DiplomacyStatus.AtWar, firstOtherRelation.status, "Border control must not mutate the legacy first-other target.");
            AssertHasLog(manager.State, "边境管控执行");

            Button diplomacyButton = GameObject.Find("DiplomacyActionButton").GetComponent<Button>();
            diplomacyButton.onClick.Invoke();
            Assert.AreEqual(DiplomacyStatus.AtWar, selectedRelation.status, "Diplomacy action should declare war on the selected target faction.");
            Assert.AreNotEqual(DiplomacyStatus.AtWar, firstOtherRelation.status, "Diplomacy action must not declare war on the legacy first-other target.");
            AssertHasLog(manager.State, selectedTargetFaction.name);

            Button enterWarModeButton = GameObject.Find("EnterWarModeButton").GetComponent<Button>();
            Assert.IsTrue(enterWarModeButton.interactable, "Adjacent diplomacy bridge should enable explicit war-mode entry.");
            enterWarModeButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(MapInteractionMode.War, mainMapUI.CurrentMode, "War mode entry should update MainMapUI mode.");
            Assert.AreEqual(targetRegionId, mainMapUI.CurrentSelectionContext.selectedRegionId);
            Assert.IsTrue(mainMapUI.CurrentSelectionContext.HasAvailableAction("dispatch_attack"), "War mode target should enable dispatch attack.");
            Assert.IsTrue(GameObject.Find("AttackButton").GetComponent<Button>().interactable, "Attack button should enable only after explicit war-mode entry.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnityUiExposesMechanismPanelsAndActions()
        {
            GameObject root = new GameObject("PlayModeSmoke_MechanismUI");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            RegionPanel regionPanel = Object.FindObjectOfType<RegionPanel>();
            TechPanel techPanel = Object.FindObjectOfType<TechPanel>();
            WeatherPanel weatherPanel = Object.FindObjectOfType<WeatherPanel>();
            MechanismPanel mechanismPanel = Object.FindObjectOfType<MechanismPanel>();
            EmperorPanel emperorPanel = Object.FindObjectOfType<EmperorPanel>();
            CourtPanel courtPanel = Object.FindObjectOfType<CourtPanel>();
            Assert.IsNotNull(regionPanel, "UISetup should create RegionPanel.");
            Assert.IsNotNull(techPanel, "UISetup should create TechPanel.");
            Assert.IsNotNull(weatherPanel, "UISetup should create WeatherPanel.");
            Assert.IsNotNull(mechanismPanel, "UISetup should create MechanismPanel.");
            Assert.IsNotNull(emperorPanel, "UISetup should create EmperorPanel.");
            Assert.IsNotNull(courtPanel, "UISetup should create CourtPanel.");
            Assert.IsNotNull(GameObject.Find("TechButton"), "HUD should expose technology.");
            Assert.IsNotNull(GameObject.Find("WeatherButton"), "HUD should expose weather and celestial state.");
            Assert.IsNotNull(GameObject.Find("GovernanceModeButton"), "HUD should expose explicit governance mode.");
            Assert.IsNotNull(GameObject.Find("WarModeButton"), "HUD should expose explicit war mode.");
            Assert.IsNotNull(GameObject.Find("ModeStateText"), "HUD should describe the current map mode.");
            Assert.IsNotNull(GameObject.Find("OverlayBudgetText"), "HUD should expose war overlay label and pulse budgets.");
            Assert.IsNotNull(GameObject.Find("LogisticsQueuePanel"), "HUD should expose the logistics queue panel.");
            Assert.IsNotNull(GameObject.Find("LogisticsPriorityUpButton"), "HUD should expose logistics expedite control.");
            Assert.IsNotNull(GameObject.Find("LogisticsPriorityDownButton"), "HUD should expose logistics delay control.");
            Assert.IsNotNull(GameObject.Find("LogisticsPauseButton"), "HUD should expose logistics pause/resume control.");
            Assert.IsNotNull(GameObject.Find("LogisticsCancelButton"), "HUD should expose logistics cancel control.");
            Assert.IsNotNull(GameObject.Find("PacifyRegionButton"), "Region UI should expose pacification.");
            Assert.IsNotNull(GameObject.Find("BuildRegionBuildingButton"), "Region UI should expose building construction.");
            Assert.IsNotNull(GameObject.Find("CollapseRegionPanelButton"), "Region UI should expose sidebar collapse.");
            Assert.IsNotNull(GameObject.Find("CollapsedRegionTab"), "Region UI should keep a minimized sidebar tab.");
            Assert.IsNotNull(GameObject.Find("CollapsedRegionTabButton"), "Region UI should expose sidebar expansion.");
            Assert.IsNotNull(GameObject.Find("MechanismButton"), "HUD should expose policy and victory state.");
            Assert.IsNotNull(GameObject.Find("SetResearchButton"), "Tech UI should expose research selection.");
            Assert.IsNotNull(GameObject.Find("RecruitArmyButton"), "Court UI should expose army recruitment.");
            Assert.IsNotNull(GameObject.Find("EquipArmyButton"), "Court UI should expose army equipment.");
            Assert.IsNotNull(GameObject.Find("ResolveSuccessionButton"), "Emperor UI should expose succession resolution.");
            Assert.IsNotNull(GameObject.Find("DiplomacyActionButton"), "Mechanism UI should expose diplomacy actions.");
            Assert.IsNotNull(GameObject.Find("BorderControlButton"), "Mechanism UI should expose border control actions.");
            Assert.IsNotNull(GameObject.Find("EnterWarModeButton"), "Mechanism UI should expose explicit war-mode entry.");
            Assert.IsNotNull(GameObject.Find("EspionageActionButton"), "Mechanism UI should expose espionage actions.");
            Assert.IsNotNull(GameObject.Find("UseEmperorSkillButton"), "Emperor UI should expose active skill actions.");

            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist.");

            RegionState playerRegion = manager.State.FindRegion(playerFaction.regionIds[0]);
            Assert.IsNotNull(playerRegion, "Player region should exist for building UI smoke.");
            RegionDefinition playerRegionDefinition = manager.Data.GetRegion(playerRegion.id);
            playerFaction.money = Mathf.Max(playerFaction.money, 1000);
            playerFaction.food = Mathf.Max(playerFaction.food, 1000);
            playerFaction.completedTechIds.Add("granary_system");
            PolicyDefinition firstPolicy = manager.Data.GetPolicy("standardization");
            Assert.IsNotNull(firstPolicy, "Policy source smoke requires standardization policy.");
            Assert.IsFalse(string.IsNullOrEmpty(firstPolicy.sourceReference), "Policy definitions should carry sourceReference.");
            BuildingDefinition sourceBuilding = manager.Data.Buildings["granary"];
            Assert.IsFalse(string.IsNullOrEmpty(sourceBuilding.sourceReference), "Building definitions should carry sourceReference.");
            mechanismPanel.Show(manager.Context, playerFaction, manager.GetComponent<ReformSystem>(), manager.GetComponent<VictorySystem>(), manager.GetComponent<DiplomacySystem>(), manager.GetComponent<EspionageSystem>());
            Text mechanismDetails = GameObject.Find("MechanismPanel").transform.Find("DetailsText").GetComponent<Text>();
            Assert.IsTrue(mechanismDetails.text.Contains(firstPolicy.sourceReference), "Mechanism UI should expose policy sourceReference.");
            AssertMechanismActionIcons();
            playerRegion.integration = 35;
            playerRegion.rebellionRisk = 70;
            playerRegion.localPower = 40;
            regionPanel.Show(playerRegionDefinition, playerRegion, manager.Context, playerFaction, manager.GetComponent<BuildingSystem>());
            Text governanceOverview = GameObject.Find("GovernanceOverviewText").GetComponent<Text>();
            Text governanceSource = GameObject.Find("GovernanceSourceText").GetComponent<Text>();
            AssertGovernanceOverviewCoversStageB(governanceOverview, governanceSource, firstPolicy.sourceReference, sourceBuilding.sourceReference, playerRegionDefinition.legitimacyMemory[0]);
            AssertNoVisibleUiCopyTokens();
            Assert.IsTrue(GameObject.Find("ModeStateText").GetComponent<Text>().text.Contains("治理模式"), "HUD should make governance mode explicit.");
            Assert.IsTrue(GameObject.Find("RegionPanelModeText").GetComponent<Text>().text.Contains("治理模式"), "Region panel should make governance mode explicit.");
            Button collapseRegionPanel = GameObject.Find("CollapseRegionPanelButton").GetComponent<Button>();
            collapseRegionPanel.onClick.Invoke();
            Canvas.ForceUpdateCanvases();
            AssertPanelVisible("CollapsedRegionTab", "Collapsed region tab should be visible after minimizing the governance sidebar.");
            AssertPanelHidden("RegionPanel", "Region panel should hide its full sidebar after minimizing.");
            Text collapsedText = GameObject.Find("CollapsedRegionTabText").GetComponent<Text>();
            Assert.IsTrue(collapsedText.text.Contains("治理模式"), "Collapsed sidebar should preserve the current mode.");
            Assert.IsTrue(collapsedText.text.Contains(playerRegionDefinition.name), "Collapsed sidebar should preserve the selected region.");
            AssertNoVisibleUiCopyTokens();
            GameObject.Find("CollapsedRegionTabButton").GetComponent<Button>().onClick.Invoke();
            Canvas.ForceUpdateCanvases();
            AssertPanelVisible("RegionPanel", "Region panel should restore after expanding the minimized sidebar.");
            AssertPanelHidden("CollapsedRegionTab", "Collapsed tab should hide after expansion.");
            Assert.IsTrue(GameObject.Find("SelectionContextText").activeInHierarchy, "Governance panel should not hide core map selection feedback.");
            Button pacifyRegion = GameObject.Find("PacifyRegionButton").GetComponent<Button>();
            pacifyRegion.onClick.Invoke();
            Assert.Greater(playerRegion.integration, 35, "Pacification action should improve integration through Unity UI.");
            Assert.Less(playerRegion.rebellionRisk, 70, "Pacification action should reduce rebellion risk through Unity UI.");
            Assert.Less(playerRegion.localPower, 40, "Pacification action should reduce local power through Unity UI.");
            AssertHasLog(manager.State, "安抚");

            int buildingsBefore = playerRegion.buildings != null ? playerRegion.buildings.Count : 0;
            regionPanel.Show(playerRegionDefinition, playerRegion, manager.Context, playerFaction, manager.GetComponent<BuildingSystem>());
            Button buildRegionBuilding = GameObject.Find("BuildRegionBuildingButton").GetComponent<Button>();
            buildRegionBuilding.onClick.Invoke();
            Assert.IsNotNull(playerRegion.buildings, "Building action should initialize the region building list.");
            Assert.Greater(playerRegion.buildings.Count, buildingsBefore, "Building action should add a building through Unity UI.");
            AssertHasLog(manager.State, "建造");
            techPanel.Show(manager.Context, playerFaction, manager.GetComponent<TechSystem>());
            Button setResearch = GameObject.Find("SetResearchButton").GetComponent<Button>();
            playerFaction.currentResearchId = null;
            playerFaction.researchPoints = 12;
            setResearch.onClick.Invoke();
            Assert.IsFalse(string.IsNullOrEmpty(playerFaction.currentResearchId), "Research action should select an available technology through Unity UI.");
            Assert.AreEqual(0, playerFaction.researchPoints, "Research action should reset progress through TechSystem.");
            AssertHasLog(manager.State, "选择研究");

            ArmyState playerArmy = FirstArmyForFaction(manager, playerFaction.id);
            Assert.IsNotNull(playerArmy, "Equipment UI smoke requires a player army.");
            EmperorDefinition courtEmperor = manager.Data.GetEmperor(playerFaction.emperorId);
            int armiesBeforeRecruit = manager.State.armies.Count;
            int manpowerBeforeRecruit = playerRegion.manpower;
            courtPanel.Show(manager.Context, playerFaction, courtEmperor != null ? courtEmperor.name : "未知", manager.State.turnLog, manager.GetComponent<EquipmentSystem>());
            GameObject generalPortraitGrid = GameObject.Find("GeneralPortraitGridContent");
            Assert.IsNotNull(generalPortraitGrid, "Court UI should create a general portrait grid.");
            Assert.AreEqual(manager.Data.Generals.Count, generalPortraitGrid.transform.childCount, "Court UI should create one portrait card per general.");
            foreach (GeneralDefinition general in manager.Data.Generals.Values)
            {
                GameObject portraitObject = GameObject.Find("GeneralPortrait_" + general.id);
                Assert.IsNotNull(portraitObject, "Court UI should create portrait image for general: " + general.id);
                Image portraitImage = portraitObject.GetComponent<Image>();
                Assert.IsNotNull(portraitImage, "General portrait object should use a UI Image: " + general.id);
                Assert.IsNotNull(portraitImage.sprite, "General portrait should load portraitAssetPath into a Sprite: " + general.id);
            }
            Button recruitArmy = GameObject.Find("RecruitArmyButton").GetComponent<Button>();
            recruitArmy.onClick.Invoke();
            Assert.Greater(manager.State.armies.Count, armiesBeforeRecruit, "Recruitment action should create a new army through Unity UI.");
            Assert.Less(playerRegion.manpower, manpowerBeforeRecruit, "Recruitment action should spend regional manpower through Unity UI.");
            AssertHasLog(manager.State, "募兵");

            playerFaction.completedTechIds.Add("bronze_casting");
            courtPanel.Show(manager.Context, playerFaction, courtEmperor != null ? courtEmperor.name : "未知", manager.State.turnLog, manager.GetComponent<EquipmentSystem>());
            Button equipArmy = GameObject.Find("EquipArmyButton").GetComponent<Button>();
            equipArmy.onClick.Invoke();
            Assert.IsFalse(string.IsNullOrEmpty(playerArmy.weaponSlot), "Equipment action should equip the first available weapon through Unity UI.");
            AssertHasLog(manager.State, "装备");

            weatherPanel.Show(manager.Context, manager.GetComponent<WeatherSystem>(), manager.GetComponent<CelestialEventSystem>());
            mechanismPanel.Show(manager.Context, playerFaction, manager.GetComponent<ReformSystem>(), manager.GetComponent<VictorySystem>(), manager.GetComponent<DiplomacySystem>(), manager.GetComponent<EspionageSystem>());
            emperorPanel.Show(manager.Context, manager.Data.GetEmperor(playerFaction.emperorId), playerFaction, manager.GetComponent<EmperorSkillSystem>(), manager.GetComponent<SuccessionSystem>());

            Button resolveSuccession = GameObject.Find("ResolveSuccessionButton").GetComponent<Button>();
            int legitimacyBeforeSuccession = playerFaction.legitimacy;
            int stableSuccessionsBefore = playerFaction.stableSuccessions;
            playerFaction.successionRisk = 75;
            resolveSuccession.onClick.Invoke();
            Assert.AreEqual(20, playerFaction.successionRisk, "Succession action should reset succession risk through Unity UI.");
            Assert.IsTrue(playerFaction.legitimacy != legitimacyBeforeSuccession || playerFaction.stableSuccessions != stableSuccessionsBefore, "Succession action should change dynasty stability state through Unity UI.");
            AssertHasLog(manager.State, "继承");

            Button useSkill = GameObject.Find("UseEmperorSkillButton").GetComponent<Button>();
            int moneyBeforeSkill = playerFaction.money;
            useSkill.onClick.Invoke();
            Assert.Less(playerFaction.money, moneyBeforeSkill, "Emperor skill action should spend resources through Unity UI.");
            AssertHasLog(manager.State, "发动技能");

            Button applyPolicy = GameObject.Find("ApplyPolicyButton").GetComponent<Button>();
            int reformCountBefore = playerFaction.completedReformIds.Count;
            applyPolicy.onClick.Invoke();
            Assert.Greater(playerFaction.completedReformIds.Count, reformCountBefore, "Policy action should record completed reform state through Unity UI.");
            AssertHasLog(manager.State, "执行政策");

            FactionState diplomacyTarget = FirstOtherFaction(manager, playerFaction.id);
            Assert.IsNotNull(diplomacyTarget, "Diplomacy UI smoke requires another faction.");
            DiplomaticRelation relation = manager.GetComponent<DiplomacySystem>().FindRelation(manager.Context, playerFaction.id, diplomacyTarget.id);
            Assert.IsNotNull(relation, "DiplomacySystem should initialize player relation.");
            Button diplomacyAction = GameObject.Find("DiplomacyActionButton").GetComponent<Button>();
            diplomacyAction.onClick.Invoke();
            Assert.AreEqual(DiplomacyStatus.AtWar, relation.status, "Diplomacy action should declare war through Unity UI.");
            AssertHasLog(manager.State, "宣战");

            Button espionageAction = GameObject.Find("EspionageActionButton").GetComponent<Button>();
            int operationsBefore = manager.State.activeOperations.Count;
            playerFaction.talentIds.Add("ui_smoke_agent");
            int moneyBeforeEspionage = playerFaction.money;
            espionageAction.onClick.Invoke();
            Assert.Greater(manager.State.activeOperations.Count, operationsBefore, "Espionage action should create an operation through Unity UI.");
            Assert.Less(playerFaction.money, moneyBeforeEspionage, "Espionage action should spend resources through Unity UI.");
            AssertHasLog(manager.State, "刺探情报");

            EventPanel eventPanel = Object.FindObjectOfType<EventPanel>();
            EventDefinition eventDefinition = FirstEvent(manager);
            Assert.IsNotNull(eventDefinition, "At least one event definition is required for UI smoke.");
            Assert.IsNotNull(eventDefinition.choices, "UI event smoke requires event choices.");
            Assert.Greater(eventDefinition.choices.Length, 0, "UI event smoke requires at least one event choice.");
            manager.Events.Publish(new GameEvent(GameEventType.EventTriggered, eventDefinition.id, eventDefinition));
            yield return null;
            Assert.IsTrue(eventPanel.gameObject.activeInHierarchy, "EventTriggered should surface the event panel.");
            AssertPanelVisible("EventPanel", "EventTriggered should make the event panel visible.");
            Button firstChoice = GameObject.Find("EventChoiceButton0").GetComponent<Button>();
            Assert.IsTrue(firstChoice.gameObject.activeInHierarchy, "Event panel should expose the first event choice as a button.");
            firstChoice.onClick.Invoke();
            yield return null;
            AssertPanelHidden("EventPanel", "Choosing an event option should close the event panel.");

            manager.Events.Publish(new GameEvent(GameEventType.EventTriggered, eventDefinition.id, eventDefinition));
            yield return null;
            AssertPanelVisible("EventPanel", "Event panel should reopen for close-button smoke.");
            Button closeEvent = GameObject.Find("CloseEventPanelButton").GetComponent<Button>();
            closeEvent.onClick.Invoke();
            yield return null;
            AssertPanelHidden("EventPanel", "Close button should dismiss the event panel without choosing an option.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator HostileSelectionUsesModeAwareSidebarAndDisablesGovernanceActions()
        {
            GameObject root = new GameObject("PlayModeSmoke_ModeAwareSidebar");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            MainMapUI mainMapUI = Object.FindObjectOfType<MainMapUI>();
            Assert.IsNotNull(mainMapUI, "UISetup should create MainMapUI.");
            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist.");

            string targetRegionId = FirstAdjacentForeignRegion(manager, playerFaction);
            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, targetRegionId, null));
            yield return null;
            Canvas.ForceUpdateCanvases();

            Assert.AreEqual(MapInteractionMode.Diplomacy, mainMapUI.CurrentMode, "Hostile neighbor selection should open a diplomacy bridge before war.");
            Text modeState = GameObject.Find("ModeStateText").GetComponent<Text>();
            Assert.IsTrue(modeState.text.Contains("外交过渡"), "HUD should make the diplomacy bridge mode explicit.");
            Text regionMode = GameObject.Find("RegionPanelModeText").GetComponent<Text>();
            Assert.IsTrue(regionMode.text.Contains("外交过渡"), "Sidebar should make the diplomacy bridge mode explicit.");
            Text overview = GameObject.Find("GovernanceOverviewText").GetComponent<Text>();
            Assert.IsTrue(overview.text.Contains("外交过渡"), "Hostile selection should not show the default governance decision text.");
            Assert.IsFalse(GameObject.Find("PacifyRegionButton").GetComponent<Button>().interactable, "Hostile selection should disable pacification.");
            Assert.IsFalse(GameObject.Find("BuildRegionBuildingButton").GetComponent<Button>().interactable, "Hostile selection should disable building construction.");

            GameObject.Find("WarModeButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Canvas.ForceUpdateCanvases();

            Assert.AreEqual(MapInteractionMode.War, mainMapUI.CurrentMode, "War mode button should switch a selected hostile neighbor into war mode.");
            Assert.IsTrue(GameObject.Find("ModeStateText").GetComponent<Text>().text.Contains("战争模式"), "HUD should make war mode explicit.");
            Assert.IsTrue(GameObject.Find("RegionPanelModeText").GetComponent<Text>().text.Contains("战争模式"), "Sidebar should make war mode explicit.");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("战争态势"), "War selection should use a readable Chinese war-status heading.");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("路线可见"), "War selection should show a localized visibility line.");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("补给-" + StrategyCausalRules.WarMovementSupplyCost), "War selection should preview supply cost before dispatch.");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("战力修正"), "War selection should preview supply-driven combat power modifiers.");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("风险等级"), "War selection should expose a coarse risk grade before dispatch.");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("前线补给规划"), "War selection should expose a frontline supply plan before dispatch.");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("占后粮政"), "War selection should reserve occupation administration food pressure.");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("合法性 -" + StrategyCausalRules.OccupationLegitimacyCost), "War selection should preview occupation legitimacy cost.");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("税粮贡献降至" + StrategyCausalRules.OccupiedContributionPercent + "%"), "War selection should preview occupied tax-food contribution.");
            Text warSource = GameObject.Find("GovernanceSourceText").GetComponent<Text>();
            Assert.IsTrue(warSource.text.Contains("战争来源:"), "War mode should move historical war source notes into the source detail block.");
            Assert.IsTrue(warSource.text.Contains("占领来源:"), "War mode should keep occupation source notes visible beside the pressure preview.");
            Assert.IsFalse(GameObject.Find("PacifyRegionButton").GetComponent<Button>().interactable, "War selection should keep governance actions disabled.");

            Button prepareFrontlineButton = GameObject.Find("PrepareFrontlineButton").GetComponent<Button>();
            Assert.IsTrue(prepareFrontlineButton.interactable, "War selection should expose an actionable frontline preparation command.");
            ArmyRuntimeState selectedArmy = manager.MapQueries.GetArmy(mainMapUI.CurrentSelectionContext.selectedArmyId);
            Assert.IsNotNull(selectedArmy, "Frontline preparation needs the selected operable army.");
            selectedArmy.supply = StrategyCausalRules.WarMovementSupplyCost;
            int supplyBeforePrepare = selectedArmy.supply;
            int foodBeforePrepare = playerFaction.food;
            int expectedOccupationReserve = StrategyMapRulebook.MilitaryGovernFoodCost + StrategyMapRulebook.PacifyFoodCost + StrategyMapRulebook.RegisterFoodCost;
            prepareFrontlineButton.onClick.Invoke();
            yield return null;
            Canvas.ForceUpdateCanvases();

            Assert.Greater(selectedArmy.supply, supplyBeforePrepare, "Frontline preparation should convert food into army supply when the route is under-supplied.");
            Assert.AreEqual(targetRegionId, selectedArmy.frontlinePreparedTargetRegionId, "Frontline preparation should remember the prepared target.");
            Assert.GreaterOrEqual(selectedArmy.frontlineReservedFood, expectedOccupationReserve, "Frontline preparation should reserve the occupation administration food chain.");
            Assert.Less(playerFaction.food, foodBeforePrepare, "Frontline preparation should spend food now instead of hiding the cost until occupation.");
            AssertHasLog(manager.State, "前线整备");
            Assert.IsTrue(GameObject.Find("GovernanceOverviewText").GetComponent<Text>().text.Contains("已预留"), "War forecast should show the prepared reserve after frontline preparation.");
            AssertHasOutlinerCategory(manager.State, manager.World, "frontline_prepared", "Frontline preparation should surface in the right outliner as a visible follow-up.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WarOverlayLabelsCullByZoomDensity()
        {
            yield return WaitForResolutionAndLayout(1024, 576);

            GameObject bootstrapRoot = new GameObject("PlayModeSmoke_LabelDensity");
            bootstrapRoot.AddComponent<DemoSceneBootstrap>();

            yield return null;

            GameManager manager = Object.FindObjectOfType<GameManager>();
            AssertBootstrapped(manager);
            DemoEntityVisualSpawner spawner = Object.FindObjectOfType<DemoEntityVisualSpawner>();
            Assert.IsNotNull(spawner, "Demo bootstrap should create entity visual spawner.");
            CameraController cameraController = Object.FindObjectOfType<CameraController>();
            Assert.IsNotNull(cameraController, "Demo bootstrap should create camera controller.");

            ArmyRuntimeState attackArmy;
            string targetRegionId = PreparePlayerAttackRoute(manager, out attackArmy);
            Assert.IsTrue(manager.StartPlayerAttack(attackArmy.id, targetRegionId), "Label density smoke needs an active war route.");
            yield return null;

            TextMesh armyInfo = GameObject.Find("ArmyInfo_" + attackArmy.id).GetComponent<TextMesh>();
            TextMesh targetLabel = GameObject.Find("WarTargetLabel_" + targetRegionId).GetComponent<TextMesh>();
            TextMesh routePressureLabel = GameObject.Find("WarRoutePressureLabel_" + attackArmy.id).GetComponent<TextMesh>();
            Assert.IsNotNull(armyInfo, "Moving army should create an army info label.");
            Assert.IsNotNull(targetLabel, "Active attack should create a target label.");
            Assert.IsNotNull(routePressureLabel, "Active attack should create a route pressure label.");
            Assert.IsNotNull(GameObject.Find("WarRouteUnderlay_" + attackArmy.id), "Active attack should create a route pressure underlay.");
            Assert.IsNotNull(GameObject.Find("WarRouteContactNode_" + attackArmy.id), "Active attack should create a projected contact node.");
            Assert.IsTrue(routePressureLabel.text.Contains("补给压力"), "Route pressure label should keep a readable supply-pressure prefix.");

            cameraController.ConfigureZoomLimits(3f, 15f);
            cameraController.ConfigureZoomBands(5f, 8f);
            cameraController.SetZoom(14f);
            spawner.ApplyLabelDensityForCurrentZoom();

            Assert.IsFalse(armyInfo.GetComponent<MeshRenderer>().enabled, "Overview zoom should hide low-priority army detail labels.");
            Assert.IsTrue(targetLabel.GetComponent<MeshRenderer>().enabled, "Overview zoom should preserve high-priority war target labels.");
            Assert.IsTrue(routePressureLabel.GetComponent<MeshRenderer>().enabled || spawner.LastVisibleLabelCount > 0, "Overview zoom should preserve high-priority route pressure information unless it directly collides with a higher-priority label.");
            Assert.LessOrEqual(spawner.LastVisibleLabelCount, 5, "1024x576 overview zoom should enforce a tight visible label budget.");
            Assert.Greater(spawner.LastHiddenLabelCount, 0, "Label density pass should hide at least one label at overview zoom.");
            Assert.Greater(spawner.LastHiddenByZoomCount, 0, "Overview zoom should explain hidden low-priority labels as zoom-gated.");
            Assert.AreEqual(spawner.LastHiddenLabelCount, spawner.LastHiddenByZoomCount + spawner.LastHiddenByBudgetCount + spawner.LastHiddenByOverlapCount, "Hidden label reasons should add up to the hidden label count.");
            Assert.AreEqual(3, spawner.LastPulseBudget, "1024x576 overview zoom should use the tightest war pulse budget.");
            Assert.Greater(spawner.LastActivePulseCount + spawner.LastInactivePulseCount, 0, "Active attack should expose pulse candidates for budgeting.");
            Assert.LessOrEqual(spawner.LastActivePulseCount, spawner.LastPulseBudget, "1024x576 overview zoom should throttle active war pulses.");
            Assert.GreaterOrEqual(spawner.LastWarRouteObjectCount, 3, "Active attack should count route, underlay, and projected contact node.");
            Assert.GreaterOrEqual(spawner.LastWarTargetObjectCount, 1, "Active attack should count the target highlight.");
            Assert.LessOrEqual(spawner.LastWarOverlayObjectCount, 14, "1024x576 single-route overlay should stay within a bounded object budget.");
            Assert.LessOrEqual(spawner.LastWarLabelObjectCount, spawner.LastLabelCandidateCount, "Overlay label count should stay aligned with label density candidates.");

            DestroyDemoBootstrapRuntimeObjects(bootstrapRoot, manager.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WarOverlayDenseFrontlineExplainsLabelSuppression()
        {
            yield return WaitForResolutionAndLayout(1024, 576);

            GameObject bootstrapRoot = new GameObject("PlayModeSmoke_DenseFrontlineLabels");
            bootstrapRoot.AddComponent<DemoSceneBootstrap>();

            yield return null;

            GameManager manager = Object.FindObjectOfType<GameManager>();
            AssertBootstrapped(manager);
            DemoEntityVisualSpawner spawner = Object.FindObjectOfType<DemoEntityVisualSpawner>();
            Assert.IsNotNull(spawner, "Demo bootstrap should create entity visual spawner.");
            CameraController cameraController = Object.FindObjectOfType<CameraController>();
            Assert.IsNotNull(cameraController, "Dense frontline label smoke needs a camera controller.");

            ArmyRuntimeState baseArmy;
            string targetRegionId = PreparePlayerAttackRoute(manager, out baseArmy);
            ArmyRuntimeState stressArmyA = CreatePlayerStressArmy(manager, baseArmy, "army_dense_front_a", 72);
            ArmyRuntimeState stressArmyB = CreatePlayerStressArmy(manager, baseArmy, "army_dense_front_b", 48);

            Assert.IsTrue(manager.StartPlayerAttack(baseArmy.id, targetRegionId), "Dense frontline smoke needs the first route.");
            Assert.IsTrue(manager.StartPlayerAttack(stressArmyA.id, targetRegionId), "Dense frontline smoke needs a second route.");
            Assert.IsTrue(manager.StartPlayerAttack(stressArmyB.id, targetRegionId), "Dense frontline smoke needs a third route.");
            yield return null;

            cameraController.ConfigureZoomLimits(3f, 15f);
            cameraController.ConfigureZoomBands(5f, 8f);
            cameraController.SetZoom(14f);
            spawner.ApplyLabelDensityForCurrentZoom();

            Assert.GreaterOrEqual(spawner.LastWarRouteObjectCount, 9, "Three dense routes should expose route, underlay, and contact-node counters.");
            Assert.GreaterOrEqual(spawner.LastWarLabelObjectCount, 7, "Dense frontline should create enough labels to pressure the avoidance budget.");
            Assert.LessOrEqual(spawner.LastVisibleLabelCount, 5, "1024x576 dense frontline should keep the tight visible label budget.");
            Assert.Greater(spawner.LastHiddenLabelCount, spawner.LastVisibleLabelCount, "Dense frontline should hide more labels than it shows at overview zoom.");
            Assert.Greater(spawner.LastHiddenByZoomCount + spawner.LastHiddenByBudgetCount + spawner.LastHiddenByOverlapCount, 0, "Dense frontline should expose why labels were suppressed.");
            Assert.AreEqual(spawner.LastHiddenLabelCount, spawner.LastHiddenByZoomCount + spawner.LastHiddenByBudgetCount + spawner.LastHiddenByOverlapCount, "Dense frontline hidden-label reasons should add up.");
            Assert.LessOrEqual(spawner.LastActivePulseCount, spawner.LastPulseBudget, "Dense frontline should still throttle active war pulses.");
            Assert.AreEqual(3, spawner.LastPulseBudget, "1024x576 dense overview should use the tightest pulse budget.");
            Assert.Greater(spawner.LastInactivePulseCount, 0, "Dense frontline should pause pulse effects beyond the small-viewport budget.");
            Assert.LessOrEqual(spawner.LastWarOverlayObjectCount, 22, "Dense frontline overlay should stay inside a bounded object budget.");
            yield return null;
            AssertOverlayBudgetHudExplainsSpawnerStats(spawner);

            DestroyDemoBootstrapRuntimeObjects(bootstrapRoot, manager.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WarOverlayPulseBudgetUsesMediumViewportAt1280()
        {
            yield return WaitForResolutionAndLayout(1280, 720);

            GameObject bootstrapRoot = new GameObject("PlayModeSmoke_PulseBudget1280");
            bootstrapRoot.AddComponent<DemoSceneBootstrap>();

            yield return null;

            GameManager manager = Object.FindObjectOfType<GameManager>();
            AssertBootstrapped(manager);
            DemoEntityVisualSpawner spawner = Object.FindObjectOfType<DemoEntityVisualSpawner>();
            Assert.IsNotNull(spawner, "Demo bootstrap should create entity visual spawner for pulse budget smoke.");
            CameraController cameraController = Object.FindObjectOfType<CameraController>();
            Assert.IsNotNull(cameraController, "Demo bootstrap should create camera controller for pulse budget smoke.");

            ArmyRuntimeState attackArmy;
            string targetRegionId = PreparePlayerAttackRoute(manager, out attackArmy);
            Assert.IsTrue(manager.StartPlayerAttack(attackArmy.id, targetRegionId), "Pulse budget smoke needs an active war route.");
            yield return null;

            cameraController.ConfigureZoomLimits(3f, 15f);
            cameraController.ConfigureZoomBands(5f, 8f);
            cameraController.SetZoom(14f);
            spawner.ApplyLabelDensityForCurrentZoom();

            int expectedPulseBudget = Screen.width <= 1024 ? 3 : 4;
            Assert.AreEqual(expectedPulseBudget, spawner.LastPulseBudget, "Requested 1280x720 overview zoom should use the pulse budget for the actual PlayMode viewport.");
            Assert.LessOrEqual(spawner.LastVisibleLabelCount, 7, "1280x720 overview zoom should keep the medium visible label budget.");
            Assert.Greater(spawner.LastActivePulseCount + spawner.LastInactivePulseCount, 0, "1280x720 attack route should expose pulse candidates for budgeting.");
            Assert.LessOrEqual(spawner.LastActivePulseCount, spawner.LastPulseBudget, "1280x720 overview zoom should throttle active war pulses.");
            Assert.GreaterOrEqual(spawner.LastWarRouteObjectCount, 3, "1280x720 route should expose route, underlay, and contact node counters.");
            Assert.LessOrEqual(spawner.LastWarOverlayObjectCount, 14, "1280x720 single-route overlay should stay within a bounded object budget.");

            DestroyDemoBootstrapRuntimeObjects(bootstrapRoot, manager.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GovernancePanelKeepsSourcesAndSelectionReadableAtSmallViewport()
        {
            yield return WaitForResolutionAndLayout(1280, 720);

            GameObject root = new GameObject("PlayModeSmoke_GovernanceSmallViewport");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist.");
            playerFaction.money = Mathf.Max(playerFaction.money, 1000);
            playerFaction.food = Mathf.Max(playerFaction.food, 1000);
            playerFaction.completedTechIds.Add("granary_system");

            string regionId = playerFaction.regionIds[0];
            RegionDefinition definition = manager.Data.GetRegion(regionId);
            Assert.IsNotNull(definition, "Friendly region definition should exist.");
            RegionState regionState = manager.State.FindRegion(regionId);
            Assert.IsNotNull(regionState, "Friendly region state should exist.");
            regionState.occupationStatus = OccupationStatus.Occupied;
            regionState.integration = 35;
            regionState.taxContributionPercent = 35;
            regionState.foodContributionPercent = 35;
            regionState.rebellionRisk = 70;
            PolicyDefinition policy = manager.Data.GetPolicy("standardization");
            BuildingDefinition building = manager.Data.Buildings["granary"];

            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, regionId, null));
            yield return null;
            Canvas.ForceUpdateCanvases();

            Text governanceOverview = GameObject.Find("GovernanceOverviewText").GetComponent<Text>();
            Text governanceSource = GameObject.Find("GovernanceSourceText").GetComponent<Text>();
            AssertGovernanceOverviewCoversStageB(governanceOverview, governanceSource, policy.sourceReference, building.sourceReference, definition.legitimacyMemory[0]);
            AssertRectFitsWithinCanvas("RegionPanel");
            AssertRectFitsWithinCanvas("CollapsedRegionTab");
            AssertRectFitsWithinCanvas("ModeStateText");
            AssertRectFitsWithinCanvas("GovernanceModeButton");
            AssertRectFitsWithinCanvas("WarModeButton");
            AssertRectFitsWithinCanvas("SelectionContextText");
            AssertChildRectFitsParent("RegionPanel", "GovernanceOverviewText");
            AssertChildRectFitsParent("RegionPanel", "GovernanceSourceText");
            AssertChildRectFitsParent("RegionPanel", "PacifyRegionButton");
            AssertChildRectFitsParent("RegionPanel", "BuildRegionBuildingButton");
            AssertChildRectFitsParent("RegionPanel", "GovernancePacifyActionHintText");
            AssertChildRectFitsParent("RegionPanel", "GovernanceBuildActionHintText");
            AssertChildRectFitsParent("RegionPanel", "CollapseRegionPanelButton");
            AssertChildRectFitsParent("RegionPanel", "CloseButton");
            AssertRectsDoNotOverlap("GovernanceOverviewText", "GovernanceSourceText", "Governance source details should sit below the action summary.");
            AssertRectsDoNotOverlap("GovernanceOverviewText", "PacifyRegionButton", "Governance overview text should not cover the pacify action at the smaller viewport.");
            AssertRectsDoNotOverlap("GovernanceSourceText", "PacifyRegionButton", "Governance source details should not cover the pacify action at the smaller viewport.");
            AssertRectsDoNotOverlap("GovernanceOverviewText", "BuildRegionBuildingButton", "Governance overview text should not cover the building action at the smaller viewport.");
            AssertRectsDoNotOverlap("GovernanceSourceText", "BuildRegionBuildingButton", "Governance source details should not cover the building action at the smaller viewport.");
            AssertRectsDoNotOverlap("GovernanceSourceText", "GovernancePacifyActionHintText", "Governance source details should not cover the pacify forecast hint at the smaller viewport.");
            AssertRectsDoNotOverlap("GovernanceSourceText", "GovernanceBuildActionHintText", "Governance source details should not cover the building forecast hint at the smaller viewport.");
            AssertRectsDoNotOverlap("BuildRegionBuildingButton", "CloseButton", "Building and close actions should remain distinct after action hints widen the buttons.");
            AssertRectsDoNotOverlap("GovernanceOverviewText", "CloseButton", "Governance overview text should not cover the close action at the smaller viewport.");
            AssertRectsDoNotOverlap("GovernanceSourceText", "CloseButton", "Governance source details should not cover the close action at the smaller viewport.");
            AssertRectsDoNotOverlap("RegionPanel", "SelectionContextText", "Governance panel should not cover the HUD selection feedback at the smaller viewport.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MechanismPanelActionIconsFitAtSmallViewports()
        {
            int[] widths = { 1024, 1280 };
            int[] heights = { 576, 720 };

            for (int i = 0; i < widths.Length; i++)
            {
                yield return WaitForResolutionAndLayout(widths[i], heights[i]);

                GameObject root = new GameObject("PlayModeSmoke_MechanismActionIcons_" + widths[i] + "x" + heights[i]);
                GameManager manager = root.AddComponent<GameManager>();
                UISetup uiSetup = root.AddComponent<UISetup>();
                uiSetup.Bind(manager);

                yield return null;

                AssertBootstrapped(manager);
                FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
                Assert.IsNotNull(playerFaction, "Player faction should exist for mechanism icon smoke.");
                playerFaction.money = Mathf.Max(playerFaction.money, 1000);
                playerFaction.food = Mathf.Max(playerFaction.food, 1000);

                MechanismPanel mechanismPanel = Object.FindObjectOfType<MechanismPanel>();
                Assert.IsNotNull(mechanismPanel, "MechanismPanel should exist for action icon smoke.");
                mechanismPanel.Show(manager.Context, playerFaction, manager.GetComponent<ReformSystem>(), manager.GetComponent<VictorySystem>(), manager.GetComponent<DiplomacySystem>(), manager.GetComponent<EspionageSystem>());
                yield return null;
                Canvas.ForceUpdateCanvases();

                AssertPanelVisible("MechanismPanel", "Mechanism panel should be visible for small-viewport icon smoke.");
                AssertRectFitsWithinCanvas("MechanismPanel");
                AssertChildRectFitsParent("MechanismPanel", "ApplyPolicyButton");
                AssertChildRectFitsParent("MechanismPanel", "DiplomacyActionButton");
                AssertChildRectFitsParent("MechanismPanel", "BorderControlButton");
                AssertChildRectFitsParent("MechanismPanel", "EspionageActionButton");
                AssertChildRectFitsParent("MechanismPanel", "EnterWarModeButton");
                AssertMechanismActionIcons();

                Object.Destroy(root);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator OccupiedGovernancePanelRecommendsMilitaryGovernBeforePacify()
        {
            yield return WaitForResolutionAndLayout(1280, 720);

            GameObject root = new GameObject("PlayModeSmoke_OccupationGovernanceForecast");
            GameManager manager = root.AddComponent<GameManager>();
            UISetup uiSetup = root.AddComponent<UISetup>();
            uiSetup.Bind(manager);

            yield return null;

            AssertBootstrapped(manager);
            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist for occupation governance smoke.");
            playerFaction.money = Mathf.Max(playerFaction.money, 1000);
            playerFaction.food = Mathf.Max(playerFaction.food, 1000);
            playerFaction.legitimacy = Mathf.Max(playerFaction.legitimacy, 50);

            string regionId = playerFaction.regionIds[0];
            RegionState regionState = manager.State.FindRegion(regionId);
            Assert.IsNotNull(regionState, "Friendly region state should exist.");
            regionState.occupationStatus = OccupationStatus.Occupied;
            regionState.controlStage = ControlStage.NewlyAttached;
            regionState.integration = 20;
            regionState.taxContributionPercent = 25;
            regionState.foodContributionPercent = 25;
            regionState.rebellionRisk = 80;
            regionState.localPower = 70;
            regionState.localAcceptance = 30;

            int moneyBefore = playerFaction.money;
            int foodBefore = playerFaction.food;
            int legitimacyBefore = playerFaction.legitimacy;

            manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, regionId, null));
            yield return null;
            Canvas.ForceUpdateCanvases();

            Text governanceOverview = GameObject.Find("GovernanceOverviewText").GetComponent<Text>();
            Assert.IsTrue(governanceOverview.text.Contains("军管"), "Newly attached regions should recommend military governance before pacification.");
            Assert.IsTrue(governanceOverview.text.Contains("下一阶段 军管"), "Expected effect should preview the military-governed control stage.");
            Assert.IsFalse(governanceOverview.text.Contains("预计效果: 安抚"), "Newly attached forecast should not short-circuit to pacification.");

            Button governanceAction = GameObject.Find("PacifyRegionButton").GetComponent<Button>();
            governanceAction.onClick.Invoke();
            yield return null;
            Canvas.ForceUpdateCanvases();

            Assert.AreEqual(ControlStage.MilitaryGoverned, regionState.controlStage, "First occupation governance action should advance only to military-governed.");
            Assert.Less(regionState.rebellionRisk, 80, "Military governance should suppress immediate rebellion pressure.");
            Assert.Less(playerFaction.money, moneyBefore, "Military governance should spend money.");
            Assert.Less(playerFaction.food, foodBefore, "Military governance should spend food.");
            Assert.Less(playerFaction.legitimacy, legitimacyBefore, "Military governance should carry a legitimacy cost.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CollapsedGovernanceSidebarKeepsCoreControlsUsableAtNarrowViewports()
        {
            int[] widths = { 1280, 1024 };
            int[] heights = { 720, 576 };

            for (int i = 0; i < widths.Length; i++)
            {
                yield return WaitForResolutionAndLayout(widths[i], heights[i]);

                GameObject root = new GameObject("PlayModeSmoke_NarrowViewport_" + widths[i] + "x" + heights[i]);
                GameManager manager = root.AddComponent<GameManager>();
                UISetup uiSetup = root.AddComponent<UISetup>();
                uiSetup.Bind(manager);

                yield return null;

                AssertBootstrapped(manager);
                FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
                Assert.IsNotNull(playerFaction, "Player faction should exist for narrow viewport smoke.");
                string regionId = playerFaction.regionIds[0];
                manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, regionId, null));
                yield return null;
                Canvas.ForceUpdateCanvases();

                AssertRectFitsWithinCanvas("RegionPanel");
                AssertPanelVisible("StrategyOutlinerCollapsed", "Expanded region panel should force the right outliner into its compact tab at " + widths[i] + "x" + heights[i] + ".");
                AssertPanelHidden("StrategyOutlinerPanel", "Expanded region panel should hide the full outliner at " + widths[i] + "x" + heights[i] + ".");
                AssertRectFitsWithinCanvas("StrategyOutlinerCollapsed");
                AssertRectsDoNotOverlap("RegionPanel", "StrategyOutlinerCollapsed", "Compact outliner tab should dock beside the region panel at " + widths[i] + "x" + heights[i] + ".");
                AssertChildRectFitsParent("RegionPanel", "GovernanceOverviewText");
                AssertChildRectFitsParent("RegionPanel", "GovernanceSourceText");
                AssertChildRectFitsParent("RegionPanel", "PacifyRegionButton");
                AssertChildRectFitsParent("RegionPanel", "BuildRegionBuildingButton");
                AssertChildRectFitsParent("RegionPanel", "GovernancePacifyActionHintText");
                AssertChildRectFitsParent("RegionPanel", "GovernanceBuildActionHintText");
                AssertRectsDoNotOverlap("GovernanceOverviewText", "GovernanceSourceText", "Governance source details should stay separate from the action summary at " + widths[i] + "x" + heights[i] + ".");
                AssertRectsDoNotOverlap("GovernanceSourceText", "GovernancePacifyActionHintText", "Governance source details should not cover the pacify forecast hint at " + widths[i] + "x" + heights[i] + ".");
                AssertRectsDoNotOverlap("GovernanceSourceText", "GovernanceBuildActionHintText", "Governance source details should not cover the building forecast hint at " + widths[i] + "x" + heights[i] + ".");
                AssertWorldRectAtLeast("GovernanceModeButton", 40f, 24f);
                AssertWorldRectAtLeast("WarModeButton", 40f, 24f);
                AssertWorldRectAtLeast("AttackButton", 40f, 24f);
                AssertWorldRectAtLeast("NextTurnButton", 40f, 24f);

                GameObject.Find("CollapseRegionPanelButton").GetComponent<Button>().onClick.Invoke();
                yield return null;
                Canvas.ForceUpdateCanvases();

                AssertPanelVisible("CollapsedRegionTab", "Collapsed sidebar should stay visible at " + widths[i] + "x" + heights[i] + ".");
                AssertPanelHidden("RegionPanel", "Full sidebar should stay hidden after collapse at " + widths[i] + "x" + heights[i] + ".");
                AssertRectFitsWithinCanvas("CollapsedRegionTab");
                AssertRectFitsWithinCanvas("GovernanceModeButton");
                AssertRectFitsWithinCanvas("WarModeButton");
                AssertRectFitsWithinCanvas("ModeStateText");
                AssertRectFitsWithinCanvas("SelectionContextText");
                AssertRectFitsWithinCanvas("AttackButton");
                AssertRectFitsWithinCanvas("NextTurnButton");
                AssertRectsDoNotOverlap("CollapsedRegionTab", "SelectionContextText", "Collapsed sidebar should not cover mode selection feedback at narrow viewport.");

                Object.Destroy(root);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator StrategyOutlinerEntriesSelectRegionsAndAvoidExpandedSidebar()
        {
            yield return WaitForResolutionAndLayout(1024, 576);

            GameObject bootstrapRoot = new GameObject("PlayModeSmoke_StrategyOutliner");
            bootstrapRoot.AddComponent<DemoSceneBootstrap>();

            yield return null;

            GameManager manager = Object.FindObjectOfType<GameManager>();
            AssertBootstrapped(manager);
            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist for outliner smoke.");
            foreach (RegionState state in manager.State.regions)
            {
                state.rebellionRisk = 0;
                state.localPower = 0;
                state.localAcceptance = 70;
                state.occupationStatus = OccupationStatus.Controlled;
                state.controlStage = ControlStage.Controlled;
            }
            foreach (ArmyRuntimeState army in manager.World.Map.ArmiesById.Values)
            {
                army.task = ArmyTask.Idle;
                army.targetRegionId = null;
                army.supply = 100;
            }

            string targetRegionId = playerFaction.regionIds[0];
            RegionState targetRegion = manager.State.FindRegion(targetRegionId);
            Assert.IsNotNull(targetRegion, "Outliner smoke needs a target region.");
            targetRegion.rebellionRisk = 90;
            targetRegion.localPower = 75;

            RegionState occupiedRegion = manager.State.FindRegion(playerFaction.regionIds[1]);
            Assert.IsNotNull(occupiedRegion, "Outliner smoke needs a second region for occupation grouping.");
            occupiedRegion.occupationStatus = OccupationStatus.Occupied;
            occupiedRegion.controlStage = ControlStage.NewlyAttached;

            ArmyRuntimeState movingArmy = null;
            foreach (ArmyRuntimeState army in manager.World.Map.ArmiesById.Values)
            {
                if (army.ownerFactionId == playerFaction.id)
                {
                    movingArmy = army;
                    break;
                }
            }
            Assert.IsNotNull(movingArmy, "Outliner smoke needs a player army for army grouping.");
            movingArmy.task = ArmyTask.Move;
            movingArmy.targetRegionId = targetRegionId;
            movingArmy.supply = StrategyCausalRules.LowSupplyBattleThreshold - 1;
            manager.State.AddLog("war", targetRegionId + "发生接敌：outliner smoke report.");

            GameObject.Find("LensRiskButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Canvas.ForceUpdateCanvases();

            AssertPanelVisible("StrategyOutlinerPanel", "Outliner should start expanded when the region sidebar is not open.");
            Text outlinerText = GameObject.Find("StrategyOutlinerText").GetComponent<Text>();
            Assert.IsTrue(outlinerText.text.Contains("高风险地区"), "Outliner summary should group high-risk regions.");
            Assert.IsTrue(outlinerText.text.Contains("新占治理"), "Outliner summary should group occupation-chain work.");
            Assert.IsTrue(outlinerText.text.Contains("行军军队"), "Outliner summary should group marching and low-supply armies.");
            Assert.IsTrue(outlinerText.text.Contains("最新战报"), "Outliner summary should group recent reports with a region target.");
            Button firstEntry = GameObject.Find("StrategyOutlinerEntryButton_0").GetComponent<Button>();
            Assert.IsNotNull(firstEntry, "Critical region should create a clickable outliner entry.");
            Text firstEntryText = firstEntry.GetComponentInChildren<Text>();
            Assert.IsTrue(firstEntryText.text.Contains("高风险地区"), "Highest-priority outliner entry should expose its group label.");
            GameObject focusedElevation = FindSceneObjectByNameIncludingInactive("SelectedRegionElevation_" + targetRegionId);
            Assert.IsNotNull(focusedElevation, "Outliner focus should use the real region elevation overlay on the 56-region map.");
            float elevationBaseScale = focusedElevation.transform.localScale.x;
            CameraController cameraController = Object.FindObjectOfType<CameraController>();
            Assert.IsNotNull(cameraController, "Outliner focus should have a camera controller.");
            cameraController.ConfigureSmoothFocus(0.08f, true);
            cameraController.SetZoom(4f);
            Vector2 focusedCenter;
            Assert.IsTrue(TryGetRegionSurfaceWorldCenter(targetRegionId, out focusedCenter), "Outliner focus should resolve a real projected region center.");
            cameraController.CenterOnRegion(ResolveOffsetFocusStart(cameraController, focusedCenter));
            Vector3 cameraBeforeOutlinerFocus = cameraController.transform.position;
            Assert.Greater(Vector2.Distance(new Vector2(cameraBeforeOutlinerFocus.x, cameraBeforeOutlinerFocus.y), focusedCenter), 0.25f, "Outliner smoke should stage the camera away from the target so smooth focus is observable.");
            firstEntry.onClick.Invoke();
            Assert.IsTrue(focusedElevation.activeInHierarchy, "Outliner click should activate the matching real-region elevation layer.");
            Assert.Greater(focusedElevation.transform.localScale.x, elevationBaseScale, "Outliner click should pulse the matching real-region elevation layer.");
            Assert.IsTrue(cameraController.SmoothFocusActive, "Outliner click should start a smooth camera focus instead of snapping instantly.");
            yield return null;
            Canvas.ForceUpdateCanvases();

            MainMapUI mainMapUI = Object.FindObjectOfType<MainMapUI>();
            Assert.IsNotNull(mainMapUI, "MainMapUI should exist for outliner selection smoke.");
            Assert.AreEqual(targetRegionId, mainMapUI.SelectedRegionId, "Clicking an outliner region entry should publish the matching region selection.");
            AssertPanelVisible("RegionPanel", "Outliner click should open the selected region panel.");
            AssertPanelVisible("StrategyOutlinerCollapsed", "Region panel should force the outliner into compact mode after selection.");
            AssertPanelHidden("StrategyOutlinerPanel", "Full outliner should hide after a region selection opens the sidebar.");
            AssertRectsDoNotOverlap("RegionPanel", "StrategyOutlinerCollapsed", "Compact outliner should not overlap the expanded region panel at 1024x576.");

            DestroyDemoBootstrapRuntimeObjects(bootstrapRoot, manager.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllStrategyLensesSwitchStateAndReuseRegionSurfaces()
        {
            yield return WaitForResolutionAndLayout(1280, 720);

            GameObject root = new GameObject("PlayModeSmoke_AllStrategyLenses");
            root.AddComponent<DemoSceneBootstrap>();

            yield return null;

            GameManager manager = Object.FindObjectOfType<GameManager>();
            AssertBootstrapped(manager);
            MainMapUI mainMapUI = Object.FindObjectOfType<MainMapUI>();
            Assert.IsNotNull(mainMapUI, "UISetup should create MainMapUI for lens smoke.");

            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction should exist for lens smoke.");
            string sampleRegionId = playerFaction.regionIds[0];
            GameObject sampleSurface = GameObject.Find("RegionSurface_" + sampleRegionId);
            Assert.IsNotNull(sampleSurface, "Lens smoke needs a real region surface.");
            MeshRenderer sampleRenderer = sampleSurface.GetComponent<MeshRenderer>();
            Assert.IsNotNull(sampleRenderer, "Lens smoke needs a mesh renderer on the real region surface.");

            RegionController[] controllers = Object.FindObjectsOfType<RegionController>();
            Assert.AreEqual(56, controllers.Length, "Lens smoke should keep the real 56 region surfaces.");

            Color governanceColor = sampleRenderer.material.color;
            bool governanceVisible = sampleRenderer.enabled;

            string[] lensButtons = { "LensGovernanceButton", "LensRiskButton", "LensEconomyButton", "LensLegitimacyButton", "LensWarButton", "LensTerrainButton" };
            MapLensMode[] lenses = { MapLensMode.Governance, MapLensMode.Risk, MapLensMode.Economy, MapLensMode.Legitimacy, MapLensMode.War, MapLensMode.Terrain };
            bool sawVisualChange = false;

            for (int i = 0; i < lensButtons.Length; i++)
            {
                GameObject.Find(lensButtons[i]).GetComponent<Button>().onClick.Invoke();
                yield return null;
                Canvas.ForceUpdateCanvases();

                Assert.AreEqual(lenses[i], mainMapUI.CurrentLens, "Lens button should switch the current map lens.");
                Assert.AreEqual(56, Object.FindObjectsOfType<RegionController>().Length, "Lens switching must keep the same 56 region surfaces.");

                if (lenses[i] != MapLensMode.Governance)
                {
                    Assert.IsTrue(sampleRenderer.enabled, "Non-governance lenses should keep the mesh surface visible for readable overlays.");
                    if (!ColorsApproximatelyEqual(governanceColor, sampleRenderer.material.color))
                    {
                        sawVisualChange = true;
                    }
                }
            }

            Assert.IsFalse(governanceVisible && sampleRenderer.enabled && ColorsApproximatelyEqual(governanceColor, sampleRenderer.material.color),
                "Lens switching should change the sampled region surface state from governance to at least one alternate lens.");
            Assert.IsTrue(sawVisualChange, "At least one lens should visibly change a real region surface.");

            GameObject.Find("LensGovernanceButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Canvas.ForceUpdateCanvases();
            Assert.AreEqual(MapLensMode.Governance, mainMapUI.CurrentLens, "Lens smoke should return to governance state before teardown.");

            DestroyDemoBootstrapRuntimeObjects(root, manager.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WarDispatchAndBattleReportStayUsableAtNarrowViewports()
        {
            int[] widths = { 1280, 1024 };
            int[] heights = { 720, 576 };

            for (int i = 0; i < widths.Length; i++)
            {
                yield return WaitForResolutionAndLayout(widths[i], heights[i]);

                GameObject root = new GameObject("PlayModeSmoke_WarNarrowViewport_" + widths[i] + "x" + heights[i]);
                GameManager manager = root.AddComponent<GameManager>();
                UISetup uiSetup = root.AddComponent<UISetup>();
                uiSetup.Bind(manager);

                yield return null;

                AssertBootstrapped(manager);
                FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
                Assert.IsNotNull(playerFaction, "Player faction should exist for narrow war viewport smoke.");
                ArmyRuntimeState enemyArmy;
                Assert.IsTrue(manager.World.Map.TryGetArmy("army_enemy_1", out enemyArmy), "Enemy army should exist for narrow war viewport smoke.");
                string targetRegionId = enemyArmy.locationRegionId;
                enemyArmy.soldiers = 300;
                SyncLegacyArmySoldiers(manager, enemyArmy.id, enemyArmy.soldiers);

                manager.Events.Publish(new GameEvent(GameEventType.RegionSelected, targetRegionId, null));
                yield return null;
                GameObject.Find("WarModeButton").GetComponent<Button>().onClick.Invoke();
                yield return null;
                Canvas.ForceUpdateCanvases();

                Button attackButton = GameObject.Find("AttackButton").GetComponent<Button>();
                Assert.IsTrue(attackButton.interactable, "War attack should be available at " + widths[i] + "x" + heights[i] + ".");
                AssertRectFitsWithinCanvas("AttackButton");
                AssertRectFitsWithinCanvas("ModeStateText");
                AssertRectFitsWithinCanvas("SelectionContextText");
                AssertRectFitsWithinCanvas("RegionPanel");

                attackButton.onClick.Invoke();
                yield return null;
                manager.NextTurn();
                yield return null;
                manager.NextTurn();
                yield return null;
                Canvas.ForceUpdateCanvases();

                AssertPanelVisible("BattleReportPanel", "Battle report should open after narrow viewport war resolution.");
                AssertRectFitsWithinCanvas("BattleReportPanel");
                AssertChildRectFitsParent("BattleReportPanel", "DetailsText");
                AssertChildRectFitsParent("BattleReportPanel", "FocusBattleReportRegionButton");
                AssertRectsDoNotOverlap("BattleReportPanel", "ModeStateText", "Battle report should not cover mode text at narrow viewport.");
                AssertRectsDoNotOverlap("BattleReportPanel", "SelectionContextText", "Battle report should not cover selection feedback at narrow viewport.");
                AssertRectsDoNotOverlap("BattleReportPanel", "AttackButton", "Battle report should not cover attack action at narrow viewport.");
                Text details = GameObject.Find("BattleReportPanel").transform.Find("DetailsText").GetComponent<Text>();
                Assert.IsTrue(details.text.Contains("补给修正"), "Narrow viewport battle report should keep supply pressure readable.");
                AssertBattleReportVisualComponents(true);

                Object.Destroy(root);
                yield return null;
            }
        }

        private static IEnumerator WaitForResolutionAndLayout(int width, int height)
        {
            Screen.SetResolution(width, height, false);
            for (int i = 0; i < 10; i++)
            {
                yield return null;
                if (Screen.width == width && Screen.height == height)
                {
                    break;
                }
            }

            Canvas.ForceUpdateCanvases();
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        private static void DestroyDemoBootstrapRuntimeObjects(params GameObject[] extraRoots)
        {
            HashSet<GameObject> objectsToDestroy = new HashSet<GameObject>();
            AddDestroyCandidate(objectsToDestroy, extraRoots);
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("DemoSceneBootstrap"));
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("GameManager"));
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("MapRenderer"));
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("MapRoot"));
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("EntityVisuals"));
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("UISetup"));
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("AudioManager"));
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("AudioEventBridge"));
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("GameCanvas"));
            AddDestroyCandidate(objectsToDestroy, GameObject.Find("Main Camera"));

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = Object.FindObjectOfType<EventSystem>();
            }

            if (eventSystem != null)
            {
                AddDestroyCandidate(objectsToDestroy, eventSystem.gameObject);
            }

            foreach (GameObject candidate in Object.FindObjectsOfType<GameObject>())
            {
                if (candidate == null || candidate.name == null) continue;
                if (candidate.name.StartsWith("PlayModeSmoke_"))
                {
                    AddDestroyCandidate(objectsToDestroy, candidate);
                }
            }

            foreach (GameObject candidate in objectsToDestroy)
            {
                if (candidate != null)
                {
                    Object.Destroy(candidate);
                }
            }
        }

        private static void AddDestroyCandidate(HashSet<GameObject> objectsToDestroy, params GameObject[] candidates)
        {
            if (candidates == null) return;
            for (int i = 0; i < candidates.Length; i++)
            {
                GameObject candidate = candidates[i];
                if (candidate != null)
                {
                    objectsToDestroy.Add(candidate);
                }
            }
        }

        private static EventDefinition FirstEvent(GameManager manager)
        {
            foreach (EventDefinition eventDefinition in manager.Data.Events.Values)
            {
                return eventDefinition;
            }

            return null;
        }

        private static ArmyState FirstArmyForFaction(GameManager manager, string factionId)
        {
            for (int i = 0; i < manager.State.armies.Count; i++)
            {
                ArmyState army = manager.State.armies[i];
                if (army != null && army.ownerFactionId == factionId) return army;
            }

            return null;
        }

        private static string FirstAdjacentForeignRegion(GameManager manager, FactionState playerFaction)
        {
            Assert.IsNotNull(manager, "GameManager is required.");
            Assert.IsNotNull(playerFaction, "Player faction is required.");

            for (int i = 0; i < playerFaction.regionIds.Count; i++)
            {
                RegionDefinition regionDefinition = manager.Data.GetRegion(playerFaction.regionIds[i]);
                if (regionDefinition == null || regionDefinition.neighbors == null) continue;

                for (int j = 0; j < regionDefinition.neighbors.Length; j++)
                {
                    string neighborId = regionDefinition.neighbors[j];
                    RegionState neighbor = manager.State.FindRegion(neighborId);
                    if (neighbor != null && neighbor.ownerFactionId != playerFaction.id)
                    {
                        return neighbor.id;
                    }
                }
            }

            Assert.Fail("Expected at least one adjacent foreign region for selection context smoke.");
            return null;
        }

        private static string FirstAdjacentForeignRegionExcludingFaction(GameManager manager, FactionState playerFaction, string excludedFactionId)
        {
            Assert.IsNotNull(manager, "GameManager is required.");
            Assert.IsNotNull(playerFaction, "Player faction is required.");

            for (int i = 0; i < playerFaction.regionIds.Count; i++)
            {
                RegionDefinition regionDefinition = manager.Data.GetRegion(playerFaction.regionIds[i]);
                if (regionDefinition == null || regionDefinition.neighbors == null) continue;

                for (int j = 0; j < regionDefinition.neighbors.Length; j++)
                {
                    string neighborId = regionDefinition.neighbors[j];
                    RegionState neighbor = manager.State.FindRegion(neighborId);
                    if (neighbor != null &&
                        neighbor.ownerFactionId != playerFaction.id &&
                        neighbor.ownerFactionId != excludedFactionId)
                    {
                        return neighbor.id;
                    }
                }
            }

            Assert.Fail("Expected at least one adjacent foreign region outside the excluded faction.");
            return null;
        }

        private static string FirstDistantForeignRegion(GameManager manager, FactionState playerFaction)
        {
            Assert.IsNotNull(manager, "GameManager is required.");
            Assert.IsNotNull(playerFaction, "Player faction is required.");

            for (int i = 0; i < manager.State.regions.Count; i++)
            {
                RegionState candidate = manager.State.regions[i];
                if (candidate == null || candidate.ownerFactionId == playerFaction.id) continue;
                if (!IsAdjacentToPlayerTerritory(manager, playerFaction, candidate.id))
                {
                    return candidate.id;
                }
            }

            Assert.Fail("Expected at least one non-adjacent foreign region for Stage C smoke.");
            return null;
        }

        private static bool IsAdjacentToPlayerTerritory(GameManager manager, FactionState playerFaction, string regionId)
        {
            for (int i = 0; i < playerFaction.regionIds.Count; i++)
            {
                RegionDefinition playerRegion = manager.Data.GetRegion(playerFaction.regionIds[i]);
                if (playerRegion == null || playerRegion.neighbors == null) continue;
                for (int j = 0; j < playerRegion.neighbors.Length; j++)
                {
                    if (playerRegion.neighbors[j] == regionId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void AssertOwnedRegionsAreConnected(GameManager manager, FactionState faction)
        {
            Assert.IsNotNull(manager, "GameManager is required for ownership connectivity assertion.");
            Assert.IsNotNull(faction, "Faction is required for ownership connectivity assertion.");
            if (faction.regionIds.Count <= 1) return;

            HashSet<string> owned = new HashSet<string>(faction.regionIds);
            Queue<string> frontier = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();
            frontier.Enqueue(faction.regionIds[0]);
            visited.Add(faction.regionIds[0]);

            while (frontier.Count > 0)
            {
                string current = frontier.Dequeue();
                RegionDefinition definition = manager.Data.GetRegion(current);
                if (definition == null || definition.neighbors == null) continue;

                for (int i = 0; i < definition.neighbors.Length; i++)
                {
                    string neighborId = definition.neighbors[i];
                    if (!owned.Contains(neighborId) || visited.Contains(neighborId)) continue;

                    visited.Add(neighborId);
                    frontier.Enqueue(neighborId);
                }
            }

            Assert.AreEqual(owned.Count, visited.Count, "Faction opening regions should form a contiguous bloc: " + faction.id);
        }

        private static int CountScoutedRegions(GameManager manager, FactionState faction)
        {
            Assert.IsNotNull(manager, "GameManager is required for scout count.");
            Assert.IsNotNull(faction, "Faction is required for scout count.");

            int count = 0;
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = manager.State.FindRegion(faction.regionIds[i]);
                if (region != null && region.visibilityState == VisibilityState.Scouted)
                {
                    count++;
                }
            }

            return count;
        }

        private static FactionState FirstOtherFaction(GameManager manager, string factionId)
        {
            for (int i = 0; i < manager.State.factions.Count; i++)
            {
                FactionState faction = manager.State.factions[i];
                if (faction != null && faction.id != factionId) return faction;
            }

            return null;
        }

        private static string PreparePlayerAttackRoute(GameManager manager, out ArmyRuntimeState army)
        {
            Assert.IsNotNull(manager, "GameManager is required.");
            Assert.IsNotNull(manager.State, "GameState is required.");
            Assert.IsNotNull(manager.World, "WorldState is required.");
            Assert.IsNotNull(manager.World.Map, "MapState is required.");
            Assert.IsNotNull(manager.Data, "DataRepository is required.");

            FactionState playerFaction = manager.State.FindFaction(manager.State.playerFactionId);
            Assert.IsNotNull(playerFaction, "Player faction is required.");

            army = null;
            foreach (ArmyRuntimeState candidate in manager.World.Map.ArmiesById.Values)
            {
                if (candidate != null && candidate.ownerFactionId == playerFaction.id && candidate.task == ArmyTask.Idle)
                {
                    army = candidate;
                    break;
                }
            }

            Assert.IsNotNull(army, "Player attack smoke requires an idle player army.");

            string directTarget = FindAttackTargetForArmy(manager, playerFaction, army);
            if (!string.IsNullOrEmpty(directTarget))
            {
                return directTarget;
            }

            string stagingRegionId = FindAttackStagingRegion(manager, playerFaction);
            Assert.IsFalse(string.IsNullOrEmpty(stagingRegionId), "Player attack smoke requires a frontline staging region.");
            if (army.locationRegionId != stagingRegionId)
            {
                manager.World.Map.MoveArmyToRegion(army.id, stagingRegionId);
                SyncLegacyArmyRegion(manager, army.id, stagingRegionId);
            }

            army.task = ArmyTask.Idle;
            army.targetRegionId = null;
            army.engagementId = null;
            army.route = new List<string>();
            army.supply = Mathf.Max(army.supply, StrategyCausalRules.WarMovementSupplyCost + 20);

            string stagedTarget = FindAttackTargetForArmy(manager, playerFaction, army);
            Assert.IsFalse(string.IsNullOrEmpty(stagedTarget), "Player attack smoke could not find a startable enemy after staging.");
            return stagedTarget;
        }

        private static ArmyRuntimeState CreatePlayerStressArmy(GameManager manager, ArmyRuntimeState template, string id, int supply)
        {
            Assert.IsNotNull(manager, "GameManager is required for stress army creation.");
            Assert.IsNotNull(manager.World, "WorldState is required for stress army creation.");
            Assert.IsNotNull(manager.World.Map, "MapState is required for stress army creation.");
            Assert.IsNotNull(manager.State, "GameState is required for stress army creation.");
            Assert.IsNotNull(template, "A staged template army is required for stress army creation.");

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

        private static string FindAttackTargetForArmy(GameManager manager, FactionState playerFaction, ArmyRuntimeState army)
        {
            if (manager == null || manager.MapQueries == null || playerFaction == null || army == null) return null;

            for (int i = 0; i < manager.State.regions.Count; i++)
            {
                RegionState candidate = manager.State.regions[i];
                if (candidate == null || candidate.ownerFactionId == playerFaction.id) continue;

                RegionDefinition definition = manager.Data.GetRegion(candidate.id);
                if (definition == null || definition.neighbors == null) continue;

                bool stagedNextToTarget = false;
                for (int j = 0; j < definition.neighbors.Length; j++)
                {
                    if (definition.neighbors[j] == army.locationRegionId)
                    {
                        stagedNextToTarget = true;
                        break;
                    }
                }

                if (!stagedNextToTarget) continue;
                if (!manager.MapQueries.HasRoute(army.locationRegionId, candidate.id)) continue;
                return candidate.id;
            }

            return null;
        }

        private static string FindAttackStagingRegion(GameManager manager, FactionState playerFaction)
        {
            if (manager == null || playerFaction == null) return null;

            for (int i = 0; i < playerFaction.regionIds.Count; i++)
            {
                string ownedRegionId = playerFaction.regionIds[i];
                RegionDefinition definition = manager.Data.GetRegion(ownedRegionId);
                if (definition == null || definition.neighbors == null) continue;

                for (int j = 0; j < definition.neighbors.Length; j++)
                {
                    RegionState neighbor = manager.State.FindRegion(definition.neighbors[j]);
                    if (neighbor != null && neighbor.ownerFactionId != playerFaction.id)
                    {
                        return ownedRegionId;
                    }
                }
            }

            return null;
        }

        private static GameObject FindObjectNameStartsWith(string prefix)
        {
            GameObject[] objects = Object.FindObjectsOfType<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null && objects[i].name.StartsWith(prefix))
                {
                    return objects[i];
                }
            }

            return null;
        }

        private static int CountSceneObjectsStartingWith(string prefix)
        {
            int count = 0;
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject obj = objects[i];
                if (obj == null || !obj.scene.IsValid()) continue;
                if (obj.name.StartsWith(prefix)) count++;
            }

            return count;
        }

        private static int CountActiveSceneObjectsStartingWith(string prefix)
        {
            int count = 0;
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject obj = objects[i];
                if (obj == null || !obj.scene.IsValid()) continue;
                if (obj.name.StartsWith(prefix) && obj.activeInHierarchy) count++;
            }

            return count;
        }

        private static GameObject FindSceneObjectByNameIncludingInactive(string objectName)
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject obj = objects[i];
                if (obj == null || !obj.scene.IsValid()) continue;
                if (obj.name == objectName) return obj;
            }

            return null;
        }

        private static bool TryGetRegionSurfaceWorldCenter(string regionId, out Vector2 center)
        {
            center = Vector2.zero;
            if (string.IsNullOrEmpty(regionId)) return false;

            GameObject regionObject = GameObject.Find("RegionSurface_" + regionId);
            if (regionObject == null) return false;

            MeshFilter meshFilter = regionObject.GetComponent<MeshFilter>();
            Vector3 world = regionObject.transform.position;
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                world = regionObject.transform.TransformPoint(meshFilter.sharedMesh.bounds.center);
            }

            center = new Vector2(world.x, world.y);
            return true;
        }

        private static Vector2 ResolveOffsetFocusStart(CameraController cameraController, Vector2 targetCenter)
        {
            Assert.IsNotNull(cameraController, "CameraController is required for focus staging.");
            Rect bounds = cameraController.WorldBounds;
            Vector2 boundsCenter = bounds.center;
            Vector2 direction = targetCenter - boundsCenter;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector2.right;
            }

            direction.Normalize();
            return targetCenter - direction * 24f;
        }

        private static void SyncLegacyArmySoldiers(GameManager manager, string armyId, int soldiers)
        {
            Assert.IsNotNull(manager, "GameManager is required for army sync.");
            for (int i = 0; i < manager.State.armies.Count; i++)
            {
                ArmyState army = manager.State.armies[i];
                if (army != null && army.id == armyId)
                {
                    army.soldiers = soldiers;
                    return;
                }
            }

            Assert.Fail("Expected legacy army for soldier sync: " + armyId);
        }

        private static void SyncLegacyArmyRegion(GameManager manager, string armyId, string regionId)
        {
            Assert.IsNotNull(manager, "GameManager is required for army region sync.");
            for (int i = 0; i < manager.State.armies.Count; i++)
            {
                ArmyState army = manager.State.armies[i];
                if (army != null && army.id == armyId)
                {
                    army.regionId = regionId;
                    return;
                }
            }

            Assert.Fail("Expected legacy army for region sync: " + armyId);
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

        private static void AssertGovernanceOverviewCoversStageB(Text governanceOverview, Text governanceSource, string expectedPolicySource, string expectedBuildingSource, string expectedGovernanceSource)
        {
            Assert.IsNotNull(governanceOverview, "RegionPanel should expose GovernanceOverviewText.");
            Assert.IsNotNull(governanceSource, "RegionPanel should expose GovernanceSourceText.");
            string text = governanceOverview.text;
            string sourceText = governanceSource.text;
            Assert.IsTrue(text.Contains("本回合摘要"), "Governance overview should prioritize a turn summary.");
            Assert.IsTrue(text.Contains("状态徽标"), "Governance overview should expose compact status badges.");
            Assert.IsTrue(text.Contains("治理条"), "Governance overview should expose compact governance meters.");
            Assert.IsTrue(text.Contains("产出条"), "Governance overview should expose compact contribution meters.");
            AssertGovernanceVisualComponents();
            AssertGovernanceActionHintsExposeForecast(expectedBuildingSource);
            Assert.IsTrue(text.Contains("政务"), "Governance overview should group politics.");
            Assert.IsTrue(text.Contains("民生"), "Governance overview should group civic/livelihood.");
            Assert.IsTrue(text.Contains("粮政"), "Governance overview should group grain.");
            Assert.IsTrue(text.Contains("人口"), "Governance overview should group population.");
            Assert.IsTrue(text.Contains("法统"), "Governance overview should expose legitimacy.");
            Assert.IsTrue(text.Contains("风险"), "Governance overview should group risks.");
            Assert.IsTrue(text.Contains("最大风险"), "Governance overview should make the main risk readable.");
            Assert.IsTrue(text.Contains("决策"), "Governance overview should expose a first-screen decision layer.");
            Assert.IsTrue(text.Contains("推荐"), "Governance overview should expose a recommendation layer.");
            Assert.IsTrue(text.Contains("专精路线"), "Governance overview should expose the regional specialization route.");
            Assert.IsTrue(text.Contains("路线收益"), "Governance overview should explain the specialization route payoff.");
            Assert.IsTrue(text.Contains("最优行动"), "Governance overview should expose a decision recommendation.");
            Assert.IsTrue(text.Contains("可执行政务"), "Governance overview should expose action context.");
            Assert.IsTrue(text.Contains("预计效果"), "Governance overview should label expected effects in Chinese.");
            Assert.IsTrue(text.Contains("按钮状态"), "Governance overview should bind recommendations to button state.");
            Assert.IsTrue(text.Contains("按钮"), "Governance overview should make action button availability explicit.");
            Assert.IsTrue(text.Contains("安抚 可用"), "Governance overview should show pacification availability when resources allow it.");
            Assert.IsTrue(text.Contains("建造 可用"), "Governance overview should show building availability when a buildable building exists.");
            Assert.IsTrue(text.Contains("整合 +10"), "Governance overview should preview pacification integration gain.");
            Assert.IsTrue(text.Contains("民变 -12"), "Governance overview should preview pacification rebellion reduction.");
            Assert.IsTrue(text.Contains("建设"), "Governance overview should expose building entry context.");
            Assert.IsTrue(text.Contains("政策"), "Governance overview should expose policy entry context.");
            Assert.IsTrue(text.Contains("取舍"), "Governance overview should expose the specialization tradeoff.");
            Assert.IsFalse(text.Contains("治理来源:"), "Governance overview should keep source notes out of the first-screen action summary.");
            Assert.IsTrue(sourceText.Contains("因果"), "Governance source details should separate causal explanation from action choice.");
            Assert.IsTrue(sourceText.Contains("负反馈"), "Governance source details should expose negative feedback explanations.");
            Assert.IsTrue(sourceText.Contains("来源:"), "Governance source details should expose source notes.");
            Assert.IsTrue(sourceText.Contains(expectedPolicySource), "Governance source details should include policy sourceReference.");
            Assert.IsTrue(sourceText.Contains("占领来源:"), "Governance source details should include occupation source notes.");
            Assert.IsTrue(sourceText.Contains("建设来源:"), "Governance source details should include building source notes even when no building is currently buildable.");
            Assert.IsTrue(sourceText.Contains(expectedBuildingSource), "Governance source details should include building sourceReference.");
            Assert.IsTrue(sourceText.Contains("治理来源:"), "Governance source details should include governance source notes.");
            Assert.IsTrue(sourceText.Contains(expectedGovernanceSource), "Governance source details should include region legitimacy memory as governance source evidence.");
            Assert.LessOrEqual(governanceOverview.preferredHeight, governanceOverview.rectTransform.rect.height + 1f, "Governance overview text should fit inside its container.");
            Assert.LessOrEqual(governanceSource.preferredHeight, governanceSource.rectTransform.rect.height + 1f, "Governance source details should fit inside their container.");
        }

        private static void AssertNoVisibleUiCopyTokens()
        {
            string[] rootNames =
            {
                "HUDBar",
                "RegionPanel",
                "CollapsedRegionTab",
                "MechanismPanel",
                "BattleReportPanel",
                "StrategyLensBar",
                "StrategyOutlinerPanel",
                "StrategyOutlinerCollapsed",
                "LogisticsQueuePanel"
            };
            string[] bannedTokens =
            {
                "Governance",
                "Politics",
                "Civic",
                "Expected",
                "Action State",
                "War Source",
                "Diplomacy Cost",
                "Border Cost",
                "Border Impact",
                "War Mode",
                "Negative feedback",
                "M:Governance",
                "M:War",
                "R:none",
                "F:",
                "N:",
                "H:"
            };

            for (int rootIndex = 0; rootIndex < rootNames.Length; rootIndex++)
            {
                GameObject root = GameObject.Find(rootNames[rootIndex]);
                if (root == null) continue;

                Text[] texts = root.GetComponentsInChildren<Text>(true);
                for (int textIndex = 0; textIndex < texts.Length; textIndex++)
                {
                    Text text = texts[textIndex];
                    if (text == null || !IsRuntimeVisible(text.gameObject)) continue;

                    string value = text.text ?? string.Empty;
                    for (int tokenIndex = 0; tokenIndex < bannedTokens.Length; tokenIndex++)
                    {
                        Assert.IsFalse(value.Contains(bannedTokens[tokenIndex]), text.name + " should not show transitional copy token: " + bannedTokens[tokenIndex]);
                    }
                }
            }
        }

        private static bool IsRuntimeVisible(GameObject target)
        {
            if (target == null || !target.activeInHierarchy) return false;

            Transform current = target.transform;
            while (current != null)
            {
                CanvasGroup group = current.GetComponent<CanvasGroup>();
                if (group != null && (group.alpha <= 0.5f || !group.interactable || !group.blocksRaycasts))
                {
                    return false;
                }

                current = current.parent;
            }

            return true;
        }

        private static void AssertGovernanceVisualComponents()
        {
            string[] badgeNames =
            {
                "GovernanceStageBadge",
                "GovernanceRiskBadge",
                "GovernanceIntegrationBadge",
                "GovernanceContributionBadge"
            };

            for (int i = 0; i < badgeNames.Length; i++)
            {
                GameObject badge = GameObject.Find(badgeNames[i]);
                Assert.IsNotNull(badge, "Governance sidebar should expose a real UI badge: " + badgeNames[i]);
                Assert.IsTrue(badge.activeInHierarchy, "Governance badge should be visible in governance mode: " + badgeNames[i]);
                Assert.IsNotNull(badge.GetComponent<Image>(), "Governance badge should use an Image background: " + badgeNames[i]);
                Assert.IsNotNull(badge.GetComponentInChildren<Text>(), "Governance badge should bind readable label text: " + badgeNames[i]);
            }

            string[] fillNames =
            {
                "GovernanceIntegrationUiBarFill",
                "GovernanceFoodUiBarFill",
                "GovernanceTaxUiBarFill",
                "GovernanceRebellionUiBarFill"
            };

            for (int i = 0; i < fillNames.Length; i++)
            {
                GameObject fillObject = GameObject.Find(fillNames[i]);
                Assert.IsNotNull(fillObject, "Governance sidebar should expose a real UI Image meter fill: " + fillNames[i]);
                Image fill = fillObject.GetComponent<Image>();
                Assert.IsNotNull(fill, "Governance meter fill should use Image rather than ASCII text: " + fillNames[i]);
                Assert.Greater(fill.rectTransform.sizeDelta.x, 0.5f, "Governance meter fill width should be data-bound: " + fillNames[i]);
            }

            AssertActionHintReadable("GovernancePacifyActionHintText");
            AssertActionHintReadable("GovernanceBuildActionHintText");
            AssertGovernanceActionIcon("GovernancePacifyActionIcon", "PacifyRegionButton", "GovernancePacifyActionHintText");
            AssertGovernanceActionIcon("GovernanceBuildActionIcon", "BuildRegionBuildingButton", "GovernanceBuildActionHintText");
        }

        private static void AssertOverlayBudgetHudExplainsSpawnerStats(DemoEntityVisualSpawner spawner)
        {
            Assert.IsNotNull(spawner, "Overlay budget HUD assertion needs entity visual stats.");
            GameObject budgetObject = GameObject.Find("OverlayBudgetText");
            Assert.IsNotNull(budgetObject, "HUD should expose overlay label suppression and pulse budget text.");
            Text budgetText = budgetObject.GetComponent<Text>();
            Assert.IsNotNull(budgetText, "Overlay budget HUD should use Text.");
            Assert.IsFalse(string.IsNullOrEmpty(budgetText.text), "Overlay budget HUD should not be empty.");
            Assert.IsTrue(budgetText.text.Contains("缩放/预算/重叠"), "Overlay budget HUD should name the three label suppression reasons.");
            Assert.IsTrue(budgetText.text.Contains(spawner.LastHiddenByZoomCount + "/" + spawner.LastHiddenByBudgetCount + "/" + spawner.LastHiddenByOverlapCount), "Overlay budget HUD should bind the current hidden-label reason counts.");
            Assert.IsTrue(budgetText.text.Contains("标签 " + spawner.LastVisibleLabelCount + "/" + spawner.LastWarLabelObjectCount), "Overlay budget HUD should bind visible and total war labels.");
            Assert.IsTrue(budgetText.text.Contains("隐" + spawner.LastHiddenLabelCount), "Overlay budget HUD should bind total hidden labels.");
            Assert.IsTrue(budgetText.text.Contains("脉冲 " + spawner.LastActivePulseCount + "/" + (spawner.LastActivePulseCount + spawner.LastInactivePulseCount)), "Overlay budget HUD should bind active and total pulses.");
            Assert.IsTrue(budgetText.text.Contains("额" + spawner.LastPulseBudget), "Overlay budget HUD should bind the pulse budget.");
            AssertRectFitsWithinCanvas("OverlayBudgetText");
            AssertRectsDoNotOverlap("OverlayBudgetText", "CourtButton", "Overlay budget HUD should not cover compact HUD action buttons.");
        }

        private static void AssertGovernanceActionHintsExposeForecast(string expectedBuildingSource)
        {
            Text pacifyHint = GameObject.Find("GovernancePacifyActionHintText").GetComponent<Text>();
            Assert.IsTrue(pacifyHint.text.Contains("金50"), "Governance action hint should expose pacification money cost.");
            Assert.IsTrue(pacifyHint.text.Contains("粮30"), "Governance action hint should expose pacification grain cost.");
            Assert.IsTrue(pacifyHint.text.Contains("整+10"), "Governance action hint should expose pacification integration gain.");
            Assert.IsTrue(pacifyHint.text.Contains("乱-12"), "Governance action hint should expose pacification unrest reduction.");

            Text buildHint = GameObject.Find("GovernanceBuildActionHintText").GetComponent<Text>();
            Assert.IsTrue(buildHint.text.Contains("金"), "Building action hint should expose building money cost.");
            Assert.IsFalse(buildHint.text.Contains(expectedBuildingSource), "Building action hint should keep source notes in the source block, not inside the action button.");
        }

        private static void AssertActionHintReadable(string objectName)
        {
            GameObject hintObject = GameObject.Find(objectName);
            Assert.IsNotNull(hintObject, "Governance action button should expose a readable forecast hint: " + objectName);
            Assert.IsTrue(hintObject.activeInHierarchy, "Governance action hint should be visible in governance mode: " + objectName);
            Text hint = hintObject.GetComponent<Text>();
            Assert.IsNotNull(hint, "Governance action hint should use Text: " + objectName);
            Assert.IsFalse(string.IsNullOrEmpty(hint.text), "Governance action hint should not be empty: " + objectName);
            Assert.LessOrEqual(hint.preferredHeight, hint.rectTransform.rect.height + 1f, "Governance action hint should fit inside its button: " + objectName);
        }

        private static void AssertGovernanceActionIcon(string objectName, string buttonName, string hintName)
        {
            GameObject iconObject = GameObject.Find(objectName);
            Assert.IsNotNull(iconObject, "Governance action button should expose a real icon object: " + objectName);
            Assert.IsTrue(iconObject.activeInHierarchy, "Governance action icon should be visible in governance mode: " + objectName);
            Image icon = iconObject.GetComponent<Image>();
            Assert.IsNotNull(icon, "Governance action icon should use an Image swatch: " + objectName);
            Text glyph = iconObject.GetComponentInChildren<Text>();
            Assert.IsNotNull(glyph, "Governance action icon should bind a compact glyph: " + objectName);
            Assert.IsFalse(string.IsNullOrEmpty(glyph.text), "Governance action icon glyph should not be empty: " + objectName);
            AssertChildRectFitsParent(buttonName, objectName);
            AssertWorldRectAtLeast(objectName, 12f, 12f);
            AssertRectsDoNotOverlap(objectName, hintName, "Governance action icon should stay above the forecast hint: " + objectName);
        }

        private static void AssertMechanismActionIcons()
        {
            AssertMechanismActionIcon("PolicyActionIcon", "ApplyPolicyButton");
            AssertMechanismActionIcon("DiplomacyActionIcon", "DiplomacyActionButton");
            AssertMechanismActionIcon("BorderActionIcon", "BorderControlButton");
            AssertMechanismActionIcon("EspionageActionIcon", "EspionageActionButton");
            AssertMechanismActionIcon("EnterWarModeActionIcon", "EnterWarModeButton");
        }

        private static void AssertMechanismActionIcon(string objectName, string buttonName)
        {
            GameObject buttonObject = GameObject.Find(buttonName);
            Assert.IsNotNull(buttonObject, "Mechanism action button should exist before icon assertion: " + buttonName);
            Transform iconTransform = FindChildRecursive(buttonObject.transform, objectName);
            Assert.IsNotNull(iconTransform, "Mechanism action button should expose a leading icon: " + objectName);
            GameObject iconObject = iconTransform.gameObject;
            Assert.IsTrue(iconObject.activeInHierarchy, "Mechanism action icon should be visible: " + objectName);
            Image icon = iconObject.GetComponent<Image>();
            Assert.IsNotNull(icon, "Mechanism action icon should use an Image swatch: " + objectName);
            Text glyph = iconObject.GetComponentInChildren<Text>();
            Assert.IsNotNull(glyph, "Mechanism action icon should bind a compact glyph: " + objectName);
            Assert.IsFalse(string.IsNullOrEmpty(glyph.text), "Mechanism action icon glyph should not be empty: " + objectName);
            AssertChildRectFitsParent(buttonName, objectName);
            AssertChildRectFitsParent(buttonName, "Label");
            AssertWorldRectAtLeast(objectName, 10f, 10f);

            Transform labelTransform = FindChildRecursive(buttonObject.transform, "Label");
            Assert.IsNotNull(labelTransform, "Mechanism action button should keep its text label: " + buttonName);
            Rect iconRect = GetWorldRect(iconObject.GetComponent<RectTransform>());
            Rect labelRect = GetWorldRect(labelTransform.GetComponent<RectTransform>());
            Assert.IsFalse(iconRect.Overlaps(labelRect), "Mechanism action icon should not overlap the button label: " + objectName);
        }

        private static void AssertBattleReportVisualComponents(bool attackerWon)
        {
            GameObject panel = GameObject.Find("BattleReportPanel");
            Assert.IsNotNull(panel, "Battle report panel should be visible before visual assertions.");
            Button focusButton = GameObject.Find("FocusBattleReportRegionButton").GetComponent<Button>();
            Assert.IsNotNull(focusButton, "Battle report should expose a focus button for returning to the battle region.");
            Assert.IsTrue(focusButton.interactable, "Battle report focus button should be enabled when the result has a battle region.");

            GameObject ribbon = GameObject.Find("BattleOutcomeRibbon");
            Assert.IsNotNull(ribbon, "Battle report should expose a visible outcome ribbon.");
            Image ribbonImage = ribbon.GetComponent<Image>();
            Assert.IsNotNull(ribbonImage, "Battle outcome ribbon should use an Image background.");
            Text ribbonText = ribbon.GetComponentInChildren<Text>();
            Assert.IsNotNull(ribbonText, "Battle outcome ribbon should bind a label.");
            Assert.IsTrue(ribbonText.text.Contains("战报反馈"), "Battle outcome ribbon should label the result as battle feedback.");
            Assert.IsTrue(attackerWon ? ribbonText.text.Contains("攻方突破") : ribbonText.text.Contains("守方稳住"), "Battle outcome ribbon should summarize the winner.");

            string[] powerFillNames =
            {
                "BattleAttackerPowerUiBarFill",
                "BattleDefenderPowerUiBarFill"
            };

            for (int i = 0; i < powerFillNames.Length; i++)
            {
                GameObject fillObject = GameObject.Find(powerFillNames[i]);
                Assert.IsNotNull(fillObject, "Battle report should expose a real power meter fill: " + powerFillNames[i]);
                Image fill = fillObject.GetComponent<Image>();
                Assert.IsNotNull(fill, "Battle power meter should use Image: " + powerFillNames[i]);
                Assert.Greater(fill.rectTransform.sizeDelta.x, 0.5f, "Battle power meter width should be data-bound: " + powerFillNames[i]);
            }

            string[] supplyBadgeNames =
            {
                "BattleAttackerSupplyBadge",
                "BattleDefenderSupplyBadge"
            };

            for (int i = 0; i < supplyBadgeNames.Length; i++)
            {
                GameObject badge = GameObject.Find(supplyBadgeNames[i]);
                Assert.IsNotNull(badge, "Battle report should expose a supply pressure badge: " + supplyBadgeNames[i]);
                Assert.IsNotNull(badge.GetComponent<Image>(), "Battle supply badge should use an Image background: " + supplyBadgeNames[i]);
                Text label = badge.GetComponentInChildren<Text>();
                Assert.IsNotNull(label, "Battle supply badge should bind readable text: " + supplyBadgeNames[i]);
                Assert.IsTrue(label.text.Contains("补给"), "Battle supply badge should keep supply pressure readable: " + supplyBadgeNames[i]);
            }
        }

        private static void AssertRectFitsWithinCanvas(string objectName)
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            Assert.IsNotNull(canvas, "Canvas is required for viewport fit assertion.");
            GameObject target = GameObject.Find(objectName);
            Assert.IsNotNull(target, "Expected UI object for viewport fit assertion: " + objectName);

            Rect canvasRect = GetWorldRect(canvas.GetComponent<RectTransform>());
            Rect targetRect = GetWorldRect(target.GetComponent<RectTransform>());
            Assert.GreaterOrEqual(targetRect.xMin, canvasRect.xMin - 1f, objectName + " should stay inside canvas left edge.");
            Assert.LessOrEqual(targetRect.xMax, canvasRect.xMax + 1f, objectName + " should stay inside canvas right edge.");
            Assert.GreaterOrEqual(targetRect.yMin, canvasRect.yMin - 1f, objectName + " should stay inside canvas bottom edge.");
            Assert.LessOrEqual(targetRect.yMax, canvasRect.yMax + 1f, objectName + " should stay inside canvas top edge.");
        }

        private static void AssertChildRectFitsParent(string parentName, string childName)
        {
            GameObject parent = GameObject.Find(parentName);
            Assert.IsNotNull(parent, "Expected parent UI object: " + parentName);
            Transform child = FindChildRecursive(parent.transform, childName);
            Assert.IsNotNull(child, "Expected child UI object: " + parentName + "/" + childName);

            Rect parentRect = GetWorldRect(parent.GetComponent<RectTransform>());
            Rect childRect = GetWorldRect(child.GetComponent<RectTransform>());
            Assert.GreaterOrEqual(childRect.xMin, parentRect.xMin - 1f, childName + " should stay inside " + parentName + " left edge.");
            Assert.LessOrEqual(childRect.xMax, parentRect.xMax + 1f, childName + " should stay inside " + parentName + " right edge.");
            Assert.GreaterOrEqual(childRect.yMin, parentRect.yMin - 1f, childName + " should stay inside " + parentName + " bottom edge.");
            Assert.LessOrEqual(childRect.yMax, parentRect.yMax + 1f, childName + " should stay inside " + parentName + " top edge.");
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null) return null;
            Transform directChild = root.Find(childName);
            if (directChild != null) return directChild;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), childName);
                if (found != null) return found;
            }

            return null;
        }

        private static void AssertRectsDoNotOverlap(string firstName, string secondName, string message)
        {
            GameObject first = GameObject.Find(firstName);
            GameObject second = GameObject.Find(secondName);
            Assert.IsNotNull(first, "Expected UI object: " + firstName);
            Assert.IsNotNull(second, "Expected UI object: " + secondName);

            Rect firstRect = GetWorldRect(first.GetComponent<RectTransform>());
            Rect secondRect = GetWorldRect(second.GetComponent<RectTransform>());
            Assert.IsFalse(firstRect.Overlaps(secondRect), message);
        }

        private static void AssertWorldRectAtLeast(string objectName, float minWidth, float minHeight)
        {
            GameObject target = GameObject.Find(objectName);
            Assert.IsNotNull(target, "Expected UI object for usable size assertion: " + objectName);

            Rect rect = GetWorldRect(target.GetComponent<RectTransform>());
            Assert.GreaterOrEqual(rect.width, minWidth, objectName + " should keep a usable screen width.");
            Assert.GreaterOrEqual(rect.height, minHeight, objectName + " should keep a usable screen height.");
        }

        private static void AssertCameraViewportInsideBounds(CameraController controller, string message)
        {
            Assert.IsNotNull(controller, "CameraController is required for viewport bounds assertion.");

            Rect viewport = controller.ViewportWorldRect;
            Rect bounds = controller.WorldBounds;
            Assert.GreaterOrEqual(viewport.xMin, bounds.xMin - 0.03f, message + " Left viewport edge escaped.");
            Assert.LessOrEqual(viewport.xMax, bounds.xMax + 0.03f, message + " Right viewport edge escaped.");
            Assert.GreaterOrEqual(viewport.yMin, bounds.yMin - 0.03f, message + " Bottom viewport edge escaped.");
            Assert.LessOrEqual(viewport.yMax, bounds.yMax + 0.03f, message + " Top viewport edge escaped.");
        }

        private static void AssertPanelVisible(string objectName, string message)
        {
            GameObject target = GameObject.Find(objectName);
            Assert.IsNotNull(target, "Expected UI object: " + objectName);
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            Assert.IsNotNull(group, objectName + " should use CanvasGroup visibility.");
            Assert.Greater(group.alpha, 0.9f, message);
            Assert.IsTrue(group.interactable, message);
            Assert.IsTrue(group.blocksRaycasts, message);
        }

        private static void AssertPanelHidden(string objectName, string message)
        {
            GameObject target = GameObject.Find(objectName);
            Assert.IsNotNull(target, "Expected UI object: " + objectName);
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            Assert.IsNotNull(group, objectName + " should use CanvasGroup visibility.");
            Assert.Less(group.alpha, 0.1f, message);
            Assert.IsFalse(group.interactable, message);
            Assert.IsFalse(group.blocksRaycasts, message);
        }

        private static Rect GetWorldRect(RectTransform rectTransform)
        {
            Assert.IsNotNull(rectTransform, "RectTransform is required for geometry assertion.");
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float xMin = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float xMax = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float yMin = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float yMax = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private static void AssertRegionShapeDataIsRefined(GameManager manager)
        {
            int nonSixPointShapes = 0;
            foreach (MapRegionShapeDefinition shape in manager.Data.MapRegionShapes.Values)
            {
                Assert.IsNotNull(shape.boundary, "Map region shape should have a boundary: " + shape.id);
                if (shape.boundary.Length != 6)
                {
                    nonSixPointShapes++;
                }
            }

            Assert.Greater(nonSixPointShapes, 0, "Map region shapes should not all remain six-point blockout cells.");
        }

        private static void AssertAllProjectedRegionHits(GameManager manager)
        {
            Assert.IsNotNull(manager, "GameManager is required for full region hit coverage.");
            Assert.IsNotNull(manager.Data, "DataRepository is required for full region hit coverage.");

            int checkedRegions = 0;
            foreach (MapRegionShapeDefinition shape in manager.Data.MapRegionShapes.Values)
            {
                if (shape == null || string.IsNullOrEmpty(shape.regionId)) continue;

                AssertProjectedRegionHit(manager, shape.regionId);
                checkedRegions++;
            }

            Assert.AreEqual(56, checkedRegions, "Full 56-region map hit coverage should verify every real region surface.");
        }

        private static bool ColorsApproximatelyEqual(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.01f &&
                   Mathf.Abs(a.g - b.g) < 0.01f &&
                   Mathf.Abs(a.b - b.b) < 0.01f &&
                   Mathf.Abs(a.a - b.a) < 0.01f;
        }

        private static void AssertProjectedRegionHit(GameManager manager, string regionId)
        {
            MapRegionShapeDefinition shape;
            Assert.IsTrue(manager.Data.TryGetMapRegionShapeByRegionId(regionId, out shape), "Expected map shape for " + regionId);

            GameObject surface = GameObject.Find("RegionSurface_" + regionId);
            Assert.IsNotNull(surface, "Expected runtime interaction surface for " + regionId);

            MeshCollider collider = surface.GetComponent<MeshCollider>();
            Assert.IsNotNull(collider, "Interaction surface should keep a MeshCollider for " + regionId);

            Physics.SyncTransforms();
            RegionController hitController = FindProjectedRegionHitController(surface, shape, regionId);

            Assert.IsNotNull(hitController, "Projected map center should raycast to its hidden interaction region: " + regionId);

            string selectedRegionId = null;
            System.Action<GameEvent> listener = evt => selectedRegionId = evt.EntityId;
            manager.Events.Subscribe(GameEventType.RegionSelected, listener);
            EventSystem eventSystem = EventSystem.current;
            try
            {
                if (eventSystem != null) Object.DestroyImmediate(eventSystem.gameObject);
                hitController.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
                Assert.AreEqual(regionId, selectedRegionId, "Projected map hit should publish RegionSelected for " + regionId);
            }
            finally
            {
                manager.Events.Unsubscribe(GameEventType.RegionSelected, listener);
            }
        }

        private static RegionController FindProjectedRegionHitController(GameObject surface, MapRegionShapeDefinition shape, string regionId)
        {
            Vector3 fallbackCenter = surface.transform.TransformPoint(new Vector3(shape.center.x, shape.center.y, 0f));
            RegionController hitController = RaycastRegionController(fallbackCenter, regionId);
            if (hitController != null) return hitController;

            MeshCollider collider = surface.GetComponent<MeshCollider>();
            if (collider != null && collider.sharedMesh != null)
            {
                Mesh mesh = collider.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                for (int i = 0; i + 2 < triangles.Length; i += 3)
                {
                    Vector3 centroid = (vertices[triangles[i]] + vertices[triangles[i + 1]] + vertices[triangles[i + 2]]) / 3f;
                    hitController = RaycastRegionController(surface.transform.TransformPoint(centroid), regionId);
                    if (hitController != null)
                    {
                        return hitController;
                    }
                }
            }

            return null;
        }

        private static RegionController RaycastRegionController(Vector3 worldPoint, string regionId)
        {
            Ray ray = new Ray(worldPoint + Vector3.back * 5f, Vector3.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, 20f);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == null) continue;
                RegionController candidate = hits[i].collider.GetComponent<RegionController>();
                if (candidate != null && candidate.RegionId == regionId)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static IEnumerator AssertLineWidthPulse(LineRenderer line, float initialWidth, float minDelta, string message)
        {
            Assert.IsNotNull(line, "LineRenderer is required for pulse assertion.");
            float minWidth = initialWidth;
            float maxWidth = initialWidth;
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForSeconds(0.04f);
                minWidth = Mathf.Min(minWidth, line.startWidth);
                maxWidth = Mathf.Max(maxWidth, line.startWidth);
            }

            Assert.Greater(maxWidth - minWidth, minDelta, message);
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

        private static void AssertHasOutlinerCategory(GameState state, WorldState world, string category, string message)
        {
            List<StrategyOutlinerEntry> entries = StrategyMapRulebook.BuildOutliner(state, world);
            for (int i = 0; i < entries.Count; i++)
            {
                StrategyOutlinerEntry entry = entries[i];
                if (entry != null && entry.category == category)
                {
                    return;
                }
            }

            Assert.Fail(message + " Missing category: " + category);
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
