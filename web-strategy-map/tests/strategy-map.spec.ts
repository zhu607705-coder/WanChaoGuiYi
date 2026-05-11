import { expect, test, type Page } from '@playwright/test';

type DebugState = {
  mode: 'governance' | 'war';
  selectedRegionId: string;
  regionMeshCount: number;
  armyMarkerCount: number;
  terrainFeatureCount: number;
  buildingMarkerCount: number;
  routeVisible: boolean;
  cameraDistance: number;
  cameraX: number;
  cameraZ: number;
  targetX: number;
  targetZ: number;
  drawCalls: number;
  routeRaidSegmentCount: number;
  routeConvoyMarkerCount: number;
  routeDragHandleCount: number;
  occupationBadgeCount: number;
  enemyThreatMarkerCount: number;
  enemyThreatMovingCount: number;
  enemyThreatDampenedCount: number;
  friendlyCountermeasureMarkerCount: number;
  friendlyCountermeasureMovingCount: number;
  logisticsMapObjectCount: number;
  selectedLogisticsObjectId: string;
  selectedEnemyInterdictionId: string;
  activeArmyId: string;
  activeArmyTargetId: string;
  activeArmyWaypointId: string;
  visibleLabels: number;
  sidebarCollapsed: boolean;
  audio: {
    enabled: boolean;
    mode: 'governance' | 'war';
    currentMusicCue: string;
    currentNarration: string;
    currentVoice: string;
    sceneCueCount: number;
    emperorThemeCount: number;
    chronicleEventCount: number;
    lastError: string;
  };
  ui: {
    food: number;
    money: number;
    army: number;
    legitimacy: number;
    governanceQueueLength: number;
    logisticsQueueLength: number;
    commandQueueLength: number;
    occupationQueueLength: number;
    enemyInterdictionOrderLength: number;
    occupationStageSummary: string;
    enemyInterdictionSummary: string;
    enemyCountermeasureSummary: string;
    selectedEnemyInterdictionId: string;
    selectedEnemyInterdictionSummary: string;
    enemyInterdictionExport: {
      selectedOrderId: string;
      doctrine: string;
      successfulRaids: number;
      failedRaids: number;
      lastTargetRegionId: string;
      lastReasoning: string;
      pressureByRegion: Record<string, number>;
      activeOrders: Array<{
        id: string;
        routeLabel: string;
        targetRegionId: string;
        chokeRouteId?: string;
        chokePointLabel?: string;
        chokeNetworkLabel?: string;
        risk: number;
        supplyDamage: number;
        lastCountermeasure: string;
      }>;
    };
    enemyInterdictionDoctrine: string;
    enemyInterdictionMemory: string;
    logisticsStationCount: number;
    routeCapacityCount: number;
    routeCapacitySummary: string;
    routeRoadSummary: string;
    routeAlternativeSummary: string;
    selectedRouteAlternativeId: string;
    transportConvoyCount: number;
    transportConvoySummary: string;
    occupationSupplyTaskCount: number;
    occupationSupplySummary: string;
    logisticsMapObjectCount: number;
    selectedLogisticsObjectId: string;
    selectedLogisticsObjectSummary: string;
    governanceLogisticsSummary: string;
    routePressureSummary: string;
    routePressureCompactSummary: string;
    routePressureDetail: string;
    selectedControlStage: string;
    selectedContribution: number;
    selectedRisk: number;
    selectedIntegration: number;
    selectedLegitimacy: number;
    governanceFocus: string;
    governanceMicroSummary: string;
    warTurn: number;
    latestInterceptionAlert: string;
    nextCommandSummary: string;
    latestOperation: string;
    selectedEmperorId: string;
    selectedEmperorMechanic: string;
    armyOrder: string;
    armyFormation: string;
    activeArmySupply: number;
    activeArmyMorale: number;
    activeArmySoldiers: number;
    activeArmyGeneral: string;
    activeArmyUnitMix: string;
    playerArmyCount: number;
    activeArmyId: string;
    activeArmyTargetId: string;
    activeArmyWaypointId: string;
    routePickMode: 'target' | 'waypoint';
    saveSlotMessage: string;
    saveSlotError: string;
    saveSlotCount: number;
  };
};

type WarLogisticsExportState = {
  schemaVersion: number;
  turn: number;
  activeArmyId: string;
  activeArmyTargetId: string;
  activeArmyWaypointId: string;
  selectedLogisticsObjectId: string;
  selectedEnemyInterdictionId: string;
  routeAlternatives: Array<{
    id: string;
    routeLabel: string;
    waypointRegionId: string;
    capacity: number;
    supplyCost: number;
    interceptionRisk: number;
    selected: boolean;
  }>;
  enemyStrategyPhase: {
    phase: string;
    phaseLabel: string;
    doctrine: string;
    activeTargetCount: number;
    selectedOrderId: string;
    pressureRegionCount: number;
    reasoning: string;
  };
  enemyInterdiction: DebugState['ui']['enemyInterdictionExport'];
  logisticsMapObjects: Array<{
    id: string;
    kind: string;
    regionId: string;
    routeLabel: string;
    status: string;
    progress: number;
    priority: number;
    details: string;
  }>;
  routeNetworks: Array<{
    id: string;
    label: string;
    nodes: string[];
    roadClass: string;
    baseCapacity: number;
    supplyFactor: number;
    interceptionModifier: number;
    reason: string;
    blockade: {
      initialStrengthFloor: number;
      refreshStrengthGain: number;
      guardFoodCost: number;
      guardMoneyCost: number;
      guardStrengthGain: number;
      guardBlockadeReduction: number;
      guardRiskReduction: number;
      guardDamageReduction: number;
      clearFoodCost: number;
      clearMoneyCost: number;
      clearGuardStrengthGain: number;
      clearRiskReduction: number;
    };
  }>;
  routeBlockades: Array<{
    id: string;
    orderId: string;
    chokeRouteId: string;
    fromRegionId: string;
    toRegionId: string;
    targetRegionId: string;
    chokePointLabel: string;
    networkId?: string;
    networkLabel: string;
    status: string;
    strength: number;
    guardStrength: number;
    lastAction: string;
  }>;
  logisticsStations: Array<{
    id: string;
    regionId: string;
    isActive: boolean;
  }>;
  governanceLogisticsEffects: Array<{
    regionId: string;
    capacityBonus: number;
    supplyRelief: number;
    interdictionRelief: number;
    occupationBandwidthBonus: number;
    sources: string[];
  }>;
  transportConvoys: Array<{
    id: string;
    fromRegionId: string;
    targetRegionId: string;
    status: string;
    priority: number;
    roadClass: string;
    bottleneckLabel: string;
    routeUsage: number;
    routeCapacity: number;
    plannedSupplyReserve: number;
    routeLegs: Array<{
      routeId: string;
      fromRegionId: string;
      toRegionId: string;
      routeUsage: number;
      routeCapacity: number;
    }>;
  }>;
  occupationSupplyTasks: Array<{
    id: string;
    convoyId: string;
    regionId: string;
    routeLabel: string;
    status: string;
    supplyNeeded: number;
    routeUsageClaimed: boolean;
  }>;
  routeCapacities: Array<{
    routeId: string;
    fromRegion: string;
    toRegion: string;
    roadClass: string;
    bottleneckLabel: string;
    terrainReason: string;
    networkLabel?: string;
    currentUsage: number;
    maxArmies: number;
  }>;
};

type GameExportState = {
  schemaVersion: number;
  mode: 'governance' | 'war';
  selectedRegionId: string;
  selectedEmperorId: string;
  sidebarCollapsed: boolean;
  routePickMode: 'target' | 'waypoint';
  warTab: 'route' | 'army' | 'logistics' | 'report';
  armyOrder: {
    stance: string;
    formation: string;
    route: string;
    last: string;
  };
  nationState: {
    food: number;
    money: number;
    army: number;
    legitimacy: number;
  };
  regions: Array<{
    id: string;
    owner: string;
    controlStage: string;
    specialization: string;
    governanceFocus: string;
    integration: number;
    contribution: number;
    risk: number;
    legitimacy: number;
  }>;
  armies: Array<{
    id: string;
    name: string;
    faction: string;
    fromRegionId: string;
    targetRegionId: string;
    waypointRegionId?: string;
    soldiers: number;
    supply: number;
    morale: number;
    general: string;
    generalId?: string;
    unitId: string;
    unitMix: Record<string, number>;
  }>;
  queues: {
    governance: string[];
    logistics: string[];
    operation: string[];
    occupation: unknown[];
    commands: unknown[];
    battleReports: unknown[];
  };
  warLogistics: WarLogisticsExportState;
};

test.describe('code-driven strategy map', () => {
  test('loads real 56-region map and supports core decisions', async ({ page }) => {
    test.setTimeout(90_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await expect.poll(() => debug(page).then((state) => state.regionMeshCount)).toBe(56);
    await expect.poll(() => debug(page).then((state) => state.terrainFeatureCount)).toBe(56);
    await expect.poll(() => debug(page).then((state) => state.buildingMarkerCount)).toBe(56);
    await expect(page.locator('#selected-name')).not.toHaveText('--');
    await expect(page.getByTestId('map-legend')).toContainText('山地');
    await expect(page.getByTestId('map-legend')).toContainText('建设');
    await expect(page.getByTestId('geography-building')).toContainText('地理与建设');
    await expect(page.getByTestId('geography-building')).toContainText('地图层');
    await expect(page.getByTestId('real-geography')).toContainText('现实地貌');
    await expect(page.locator('.landform-label:not(.hidden)').first()).toBeVisible();
    await expect(page.locator('.building-label:not(.hidden)').first()).toBeVisible();
    await expect(page.getByTestId('governance-metrics')).toContainText('人口');
    await expect(page.getByTestId('governance-actions')).toContainText('收益');
    await expect(page.getByTestId('governance-actions')).toContainText('副作用');
    await expect(page.getByTestId('emperor-dock')).toContainText('帝皇');
    await expect(page.getByTestId('emperor-dock')).toContainText('六合同轨');

    await page.locator('[data-emperor-id="li_shi_min"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.selectedEmperorId)).toBe('li_shi_min');
    await expect.poll(() => debug(page).then((state) => state.ui.selectedEmperorMechanic)).toContain('贞观');
    await expect(page.getByTestId('emperor-dock')).toContainText('李世民');
    await expect(page.getByTestId('audio-hud')).toContainText('启用音频');

    await page.locator('.audio-summary').click();
    await page.locator('#audio-enable').click();
    await expect.poll(() => debug(page).then((state) => state.audio.enabled)).toBe(true);
    await expect.poll(() => debug(page).then((state) => state.audio.sceneCueCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.audio.emperorThemeCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.audio.chronicleEventCount)).toBeGreaterThan(0);
    await page.locator('[data-audio-action="emperor"]').click();
    await expect.poll(() => debug(page).then((state) => state.audio.currentMusicCue)).toContain('emperor');

    const beforeGovernance = await debug(page);
    await page.locator('[data-action="governance_build"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.governanceQueueLength)).toBeGreaterThan(beforeGovernance.ui.governanceQueueLength);
    await expect.poll(() => debug(page).then((state) => state.ui.money)).toBeLessThan(beforeGovernance.ui.money);
    await expect(page.getByTestId('governance-queue')).not.toContainText('暂无经营队列');

    const beforeZoom = await debug(page);
    await moveMouseToMap(page);
    await page.mouse.wheel(0, -560);
    await page.waitForTimeout(350);
    await expect.poll(() => debug(page).then((state) => state.cameraDistance)).toBeLessThan(beforeZoom.cameraDistance);

    const beforePan = await debug(page);
    await dragMap(page);
    const afterPan = await debug(page);
    const panDelta = Math.abs(afterPan.targetX - beforePan.targetX) + Math.abs(afterPan.targetZ - beforePan.targetZ);
    expect(panDelta).toBeGreaterThan(0.02);

    const clickedRegionId = await clickVisibleRegion(page);
    await expect.poll(() => debug(page).then((state) => state.selectedRegionId)).toBe(clickedRegionId);
    await expect(page.locator('#selected-name')).not.toHaveText('--');

    await page.locator('#war-mode').click();
    await expect.poll(() => debug(page).then((state) => state.mode)).toBe('war');
    await expect.poll(() => debug(page).then((state) => state.routeVisible)).toBe(true);
    await expect.poll(() => debug(page).then((state) => state.routeRaidSegmentCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.routeConvoyMarkerCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.routeDragHandleCount)).toBeGreaterThan(0);
    await expect(page.getByTestId('war-route')).toContainText('行军线');
    await expect(page.getByTestId('war-forecast')).toContainText('补给');
    await expect(page.getByTestId('war-forecast')).toContainText('接敌');
    await expect(page.getByTestId('war-forecast')).toContainText('占领');
    await expect(page.getByTestId('army-selector')).toContainText('河西骑军');
    await expect(page.getByTestId('route-plan')).toContainText('路线编辑');
    await expect(page.getByTestId('war-tabs')).toContainText('编组');
    await expect(page.getByTestId('war-forecast')).toContainText('战术修正');

    await page.locator('[data-army-id="army_player_2"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyId)).toBe('army_player_2');
    await expect.poll(() => debug(page).then((state) => state.activeArmyId)).toBe('army_player_2');
    const retargetedRegionId = await clickVisibleRegion(page);
    await expect.poll(() => debug(page).then((state) => state.activeArmyTargetId)).toBe(retargetedRegionId);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyTargetId)).toBe(retargetedRegionId);
    await page.locator('[data-route-mode="waypoint"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.routePickMode)).toBe('waypoint');
    const waypointRegionId = await clickVisibleRegion(page);
    await expect.poll(() => debug(page).then((state) => state.activeArmyWaypointId)).toBe(waypointRegionId);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyWaypointId)).toBe(waypointRegionId);
    await expect.poll(() => debug(page).then((state) => state.routeDragHandleCount)).toBeGreaterThan(1);
    await expect.poll(() => debug(page).then((state) => state.activeArmyTargetId)).toBe(retargetedRegionId);
    const draggedWaypointRegionId = await dragRouteWaypointToVisibleRegion(page, waypointRegionId, [retargetedRegionId]);
    await expect.poll(() => debug(page).then((state) => state.activeArmyWaypointId)).toBe(draggedWaypointRegionId);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyWaypointId)).toBe(draggedWaypointRegionId);
    await page.locator('[data-action="route_waypoint_clear"]').click();
    await expect.poll(() => debug(page).then((state) => state.activeArmyWaypointId)).toBe('');

    await page.locator('[data-war-tab="army"]').click();
    await expect(page.getByTestId('army-micro')).toContainText('军队微操');
    await expect(page.getByTestId('army-organization')).toContainText('编组与将领');
    const beforeWar = await debug(page);
    await page.locator('[data-action="army_order_forced_march"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.armyOrder)).toBe('急行抢道');
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmySupply)).toBeLessThan(beforeWar.ui.activeArmySupply);

    const beforeOrganization = await debug(page);
    await page.locator('[data-action="army_split"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.playerArmyCount)).toBe(beforeOrganization.ui.playerArmyCount + 1);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyId)).toContain('army_player_detached');
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmySoldiers)).toBeLessThan(beforeOrganization.ui.activeArmySoldiers);
    const beforeGeneral = await debug(page);
    await page.locator('[data-action="army_general_next"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyGeneral)).not.toBe(beforeGeneral.ui.activeArmyGeneral);
    await page.locator('[data-action="army_mix_cavalry"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyUnitMix)).toContain('骑兵');
    const beforeMerge = await debug(page);
    await page.locator('[data-action="army_merge"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.playerArmyCount)).toBe(beforeMerge.ui.playerArmyCount - 1);

    await page.locator('[data-war-tab="logistics"]').click();
    await expect(page.getByTestId('logistics-queue')).toBeVisible();
    await expect(page.getByTestId('war-command-queue')).toBeVisible();
    await page.locator('[data-action="war_deploy"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.logisticsQueueLength)).toBeGreaterThan(beforeWar.ui.logisticsQueueLength);
    await expect.poll(() => debug(page).then((state) => state.ui.commandQueueLength)).toBeGreaterThan(beforeWar.ui.commandQueueLength);
    await expect(page.getByTestId('logistics-queue')).not.toContainText('暂无后勤队列');
    const afterDeploy = await debug(page);
    await page.locator('[data-action="war_supply"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.logisticsQueueLength)).toBeGreaterThanOrEqual(afterDeploy.ui.logisticsQueueLength);
    await expect.poll(() => debug(page).then((state) => state.ui.commandQueueLength)).toBeGreaterThanOrEqual(afterDeploy.ui.commandQueueLength);
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('运输队');
    const afterSupply = await debug(page);
    await page.locator('[data-war-tab="route"]').click();
    await page.locator('[data-action="route_queue_promote"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.logisticsQueueLength)).toBe(afterSupply.ui.logisticsQueueLength);
    await expect.poll(() => debug(page).then((state) => state.ui.commandQueueLength)).toBe(afterSupply.ui.commandQueueLength);
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('后勤队列已上移');
    await page.locator('[data-action="route_queue_cancel"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.logisticsQueueLength)).toBe(afterSupply.ui.logisticsQueueLength - 1);
    await expect.poll(() => debug(page).then((state) => state.ui.commandQueueLength)).toBe(afterSupply.ui.commandQueueLength - 1);
    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_attack"]').click();
    await expect.poll(() => debug(page).then((state) => state.audio.currentNarration)).toContain('war');
    const beforeAdvance = await debug(page);
    await page.locator('[data-action="war_advance_turn"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.warTurn)).toBe(beforeAdvance.ui.warTurn + 1);
    await expect.poll(() => debug(page).then((state) => state.ui.commandQueueLength)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.ui.latestInterceptionAlert)).toContain('截粮');
    await expect.poll(() => debug(page).then((state) => state.ui.enemyInterdictionOrderLength)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.ui.enemyInterdictionSummary)).toContain('截粮命令');
    await expect.poll(() => debug(page).then((state) => state.enemyThreatMarkerCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.enemyThreatMovingCount)).toBeGreaterThan(0);
    await expect(page.getByTestId('enemy-interdiction-orders')).toContainText('敌方截粮命令');
    await expect(page.getByTestId('interdiction-countermeasures')).toContainText('护送车队');
    const beforeCountermeasure = await debug(page);
    await page.getByTestId('interdiction-countermeasures').scrollIntoViewIfNeeded();
    await page.locator('[data-action="war_counter_escort"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.enemyCountermeasureSummary)).toBe('护粮队');
    await expect.poll(() => debug(page).then((state) => state.ui.enemyInterdictionSummary)).toContain('反制 护粮队');
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('护粮队');
    await expect.poll(() => debug(page).then((state) => state.ui.food)).toBeLessThan(beforeCountermeasure.ui.food);
    await expect.poll(() => debug(page).then((state) => state.ui.routePressureSummary)).toContain('截粮');
    await expect.poll(() => debug(page).then((state) => state.ui.routePressureSummary)).toContain('反制 护粮队');
    await expect.poll(() => debug(page).then((state) => state.ui.routePressureCompactSummary)).toContain('护粮队');
    await expect.poll(() => debug(page).then((state) => state.ui.routePressureDetail)).toContain('敌方截粮点');
    await expect.poll(() => debug(page).then((state) => state.ui.routePressureDetail)).toContain('己方护粮队');
    await expect(page.locator('#route-summary')).toContainText('反制 护粮队');
    await page.locator('#route-summary').hover();
    await expect(page.locator('#route-pressure-detail')).toBeVisible();
    await expect(page.locator('#route-pressure-detail')).toContainText('敌方截粮点');
    await expect(page.locator('[data-testid="route-pressure-card"]')).toBeVisible();
    await expect(page.locator('[data-route-pressure-row="enemy"]')).toContainText('敌方');
    await expect(page.locator('[data-route-pressure-row="friendly"]')).toContainText('护粮队');
    await expect(page.locator('[data-route-pressure-row="forecast"]')).toContainText('预测');
    await expect(page.locator('#route-pressure-detail')).toContainText('位置详情');
    await page.getByTestId('interdiction-countermeasures').scrollIntoViewIfNeeded();
    await page.locator('[data-action="war_counter_reroute"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.enemyCountermeasureSummary)).toBe('改道');
    await expect.poll(() => debug(page).then((state) => state.ui.routePressureDetail)).toContain('己方改道');
    await expect(page.locator('#route-pressure-detail')).toContainText('绕开高危段');
    await page.getByTestId('interdiction-countermeasures').scrollIntoViewIfNeeded();
    await page.locator('[data-action="war_counter_scout"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.enemyCountermeasureSummary)).toBe('反斥候');
    await expect.poll(() => debug(page).then((state) => state.ui.routePressureDetail)).toContain('己方反斥候');
    await expect(page.locator('#route-pressure-detail')).toContainText('清查伏点');
    await page.getByTestId('interdiction-countermeasures').scrollIntoViewIfNeeded();
    await page.locator('[data-action="war_counter_decoy"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.enemyCountermeasureSummary)).toBe('诱敌');
    await expect.poll(() => debug(page).then((state) => state.ui.routePressureDetail)).toContain('己方诱敌');
    await expect.poll(() => page.locator('#route-pressure-detail').textContent()).toContain('弃车设伏');
    await expect.poll(() => debug(page).then((state) => state.enemyThreatMarkerCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.enemyThreatDampenedCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.friendlyCountermeasureMarkerCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.friendlyCountermeasureMovingCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.ui.nextCommandSummary)).toContain('回合');
    await expect.poll(() => debug(page).then((state) => state.routeRaidSegmentCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.routeConvoyMarkerCount)).toBeGreaterThan(0);
    for (let i = 0; i < 8; i += 1) {
      const state = await debug(page);
      if (state.ui.occupationQueueLength > 0) break;
      await page.locator('[data-action="war_advance_turn"]').click();
    }
    await expect.poll(() => debug(page).then((state) => state.ui.occupationQueueLength)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.occupationBadgeCount)).toBeGreaterThan(0);
    await expect(page.getByTestId('occupation-aftercare')).toContainText('军管');
    await expect(page.getByTestId('occupation-aftercare')).toContainText('地图徽标');
    await page.locator('[data-action="occupation_aftercare"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.occupationStageSummary)).toContain('尚余 1 回合');
    await expect.poll(() => debug(page).then((state) => state.occupationBadgeCount)).toBeGreaterThan(0);
    await page.locator('[data-action="occupation_aftercare"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.selectedControlStage)).toBe('pacify');
    await expect.poll(() => debug(page).then((state) => state.ui.occupationStageSummary)).toContain('安抚');
    await expect.poll(() => debug(page).then((state) => state.occupationBadgeCount)).toBeGreaterThan(0);
    await page.locator('[data-war-tab="report"]').click();
    await expect(page.getByTestId('battle-report')).toContainText('战术修正');
    await expect(page.getByTestId('battle-report')).toContainText('敌方截粮命令');
    await expect(page.getByTestId('battle-report')).toContainText('截粮');
    await expect(page.getByTestId('battle-report')).toContainText('占后队列');
    await expect(page.getByTestId('battle-report')).toContainText('第');
    await expect(page.getByTestId('battle-report-history')).toContainText('主将');
    await expect(page.getByTestId('battle-report-history')).toContainText('配比');
    await expect(page.getByTestId('battle-report-history')).toContainText('战术');
    await expect(page.getByTestId('battle-report-history')).toContainText('截粮风险');
    await expect(page.getByTestId('tactic-badge-row').first()).toContainText('补给');
    await expect(page.getByTestId('tactic-badge-row').first()).toContainText('接敌');
    await expect(page.getByTestId('tactic-badge-row').first()).toContainText('截粮');
    await expect(page.getByTestId('tactic-badge-row').first()).toContainText('占领');
    await expect(page.locator('.battle-outcome-card').first()).toBeVisible();

    await page.locator('#sidebar-toggle').click();
    await expect.poll(() => debug(page).then((state) => state.sidebarCollapsed)).toBe(true);
    await page.locator('#sidebar-toggle').click();
    await expect.poll(() => debug(page).then((state) => state.sidebarCollapsed)).toBe(false);

    expect(await labelOverlapCount(page)).toBe(0);
    expect(consoleErrors).toEqual([]);
  });

  test('delivers supply convoy by route segments over war turns', async ({ page }) => {
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.locator('#war-mode').click();
    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();

    await expect.poll(() => debug(page).then((state) => state.ui.commandQueueLength)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.ui.nextCommandSummary)).toContain('分段 0/');
    await expect(page.getByTestId('war-command-queue')).toContainText('送达 0/');

    const beforeAdvance = await debug(page);
    await page.locator('[data-action="war_advance_turn"]').click();

    await expect.poll(() => debug(page).then((state) => state.ui.warTurn)).toBe(beforeAdvance.ui.warTurn + 1);
    await expect.poll(() => debug(page).then((state) => state.ui.nextCommandSummary)).toContain('分段 1/');
    await expect(page.getByTestId('logistics-queue')).toContainText('运输队第 1/');
    await expect(page.getByTestId('war-command-queue')).toContainText('送达');
    expect(consoleErrors).toEqual([]);
  });

  test('supports region governance specialization micro-management', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('hexi'));
    await expect(page.getByTestId('governance-micro')).toContainText('区域微操');
    await expect(page.getByTestId('governance-micro')).toContainText('粮仓');
    await expect(page.getByTestId('governance-micro')).toContainText('军府');
    await expect(page.getByTestId('governance-micro')).toContainText('史据');

    const beforeTax = await debug(page);
    await page.locator('[data-action="governance_focus_tax"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.governanceFocus)).toBe('tax');
    await expect.poll(() => debug(page).then((state) => state.ui.money)).toBeGreaterThan(beforeTax.ui.money);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedRisk)).toBeGreaterThan(beforeTax.ui.selectedRisk);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedContribution)).toBeGreaterThan(beforeTax.ui.selectedContribution);
    await expect(page.locator('#selected-tags')).toContainText('商税漕运');
    await expect(page.getByTestId('governance-queue')).toContainText('商税漕运');

    const afterTax = await debug(page);
    await page.locator('[data-action="governance_focus_relief"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.governanceFocus)).toBe('relief');
    await expect.poll(() => debug(page).then((state) => state.ui.selectedRisk)).toBeLessThan(afterTax.ui.selectedRisk);
    await expect.poll(() => debug(page).then((state) => state.ui.food)).toBeLessThan(afterTax.ui.food);
    await expect(page.locator('#selected-tags')).toContainText('安抚民生');

    const saved = await exportGameState(page);
    const savedHexi = saved.regions.find((region) => region.id === 'hexi');
    expect(savedHexi?.governanceFocus).toBe('relief');
    expect(savedHexi?.specialization).toBe('安抚民生');
    await openApp(page);
    expect(await importGameState(page, saved)).toBe(true);
    await expect.poll(() => debug(page).then((state) => state.selectedRegionId)).toBe('hexi');
    await expect.poll(() => debug(page).then((state) => state.ui.governanceFocus)).toBe('relief');
    await expect(page.getByTestId('governance-micro')).toContainText('安抚民生');
    expect(consoleErrors).toEqual([]);
  });

  test('manages transport convoys as cancellable route-capacity entities', async ({ page }) => {
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.locator('#war-mode').click();
    await page.locator('[data-war-tab="route"]').click();
    await expect(page.getByTestId('prewar-route-capacity')).toContainText('路线容量');
    await expect.poll(() => debug(page).then((state) => state.ui.routeRoadSummary)).toContain('容量');

    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();
    await page.locator('[data-action="war_supply"]').click();

    await expect.poll(() => debug(page).then((state) => state.ui.transportConvoyCount)).toBe(2);
    await expect(page.getByTestId('transport-convoy-list')).toContainText('运输队实体');
    await expect(page.getByTestId('transport-convoy-list')).toContainText('路线');

    await page.locator('[data-action="transport_convoy_promote"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.transportConvoySummary)).toContain('优先 2');
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('已上移');

    const beforeCancel = await debug(page);
    await page.locator('[data-action="transport_convoy_cancel"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.transportConvoyCount)).toBe(beforeCancel.ui.transportConvoyCount - 1);
    await expect.poll(() => debug(page).then((state) => state.ui.commandQueueLength)).toBe(beforeCancel.ui.commandQueueLength - 1);
    await expect(page.getByTestId('transport-convoy-list')).toContainText('已取消');
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('释放');
    expect(consoleErrors).toEqual([]);
  });

  test('selects real logistics map objects and applies convoy actions to the selected target', async ({ page }) => {
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.locator('#war-mode').click();
    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();
    await page.locator('[data-action="war_supply"]').click();

    await expect.poll(() => debug(page).then((state) => state.ui.transportConvoyCount)).toBe(2);
    await expect.poll(() => debug(page).then((state) => state.logisticsMapObjectCount)).toBeGreaterThanOrEqual(2);
    await expect.poll(() => debug(page).then((state) => state.ui.logisticsMapObjectCount)).toBeGreaterThanOrEqual(2);

    const selectedId = '运输队-2';
    const point = await getLogisticsObjectPoint(page, selectedId);
    expect(point?.visible).toBe(true);
    if (!point) throw new Error('No logistics object point found.');
    await page.mouse.click(point.x, point.y);

    await expect.poll(() => debug(page).then((state) => state.selectedLogisticsObjectId)).toBe(selectedId);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedLogisticsObjectId)).toBe(selectedId);
    await expect(page.getByTestId('selected-logistics-object')).toContainText(selectedId);

    await page.locator('[data-action="transport_convoy_promote"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.selectedLogisticsObjectSummary)).toContain('优先 2');
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain(selectedId);

    await page.locator('[data-action="transport_convoy_cancel"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain(selectedId);
    await expect.poll(() => debug(page).then((state) => state.ui.transportConvoyCount)).toBe(1);
    await expect.poll(() => debug(page).then((state) => state.logisticsMapObjectCount)).toBe(1);
    expect(consoleErrors).toEqual([]);
  });

  test('selects enemy interdiction threats and applies countermeasures to the selected line', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.locator('#war-mode').click();
    await page.locator('[data-war-tab="logistics"]').click();

    const firstTarget = await clickVisibleRegion(page);
    await page.locator('[data-action="war_supply"]').click();
    const secondTarget = await clickVisibleRegion(page, [firstTarget]);
    await page.locator('[data-action="war_supply"]').click();

    for (let i = 0; i < 5; i += 1) {
      const state = await debug(page);
      if (state.ui.enemyInterdictionOrderLength >= 2) break;
      await page.locator('[data-action="war_advance_turn"]').click();
    }

    await expect.poll(() => debug(page).then((state) => state.ui.enemyInterdictionOrderLength)).toBeGreaterThanOrEqual(2);
    await expect.poll(() => debug(page).then((state) => state.enemyThreatMarkerCount)).toBeGreaterThanOrEqual(2);

    const targetOrderId = 'enemy_interdiction_2';
    await expect.poll(async () => (await getEnemyInterdictionPoint(page, targetOrderId))?.visible ?? false).toBe(true);
    const point = await getEnemyInterdictionPoint(page, targetOrderId);
    expect(point).not.toBeNull();
    if (!point) throw new Error('No enemy interdiction point found.');
    await page.mouse.click(point.x, point.y);

    await expect.poll(() => debug(page).then((state) => state.selectedEnemyInterdictionId)).toBe(targetOrderId);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedEnemyInterdictionId)).toBe(targetOrderId);
    await expect(page.getByTestId('selected-enemy-interdiction')).toContainText(targetOrderId);

    await page.locator('[data-action="war_counter_reroute"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.selectedEnemyInterdictionId)).toBe(targetOrderId);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedEnemyInterdictionSummary)).toContain('反制 改道');
    await expect(page.getByTestId('selected-enemy-interdiction')).toContainText(targetOrderId);
    await expect(page.getByTestId('selected-enemy-interdiction')).toContainText('反制 改道');
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('改道绕避');
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).not.toContain(firstTarget);
    expect(secondTarget).not.toBe(firstTarget);
    expect(consoleErrors).toEqual([]);
  });

  test('connects governance policies and buildings to route logistics', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.locator('#war-mode').click();
    await page.locator('[data-army-id="army_player_2"]').click();
    const beforeGovernance = await debug(page);
    const beforeCapacity = routeCapacityFromSummary(beforeGovernance.ui.routeRoadSummary);

    await page.locator('#governance-mode').click();
    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('hexi'));
    await expect(page.getByTestId('building-logistics-preview')).toContainText('容量');
    await page.locator('[data-action="governance_build"]').click();
    await page.locator('[data-action="governance_policy"]').click();

    await expect.poll(() => debug(page).then((state) => state.ui.governanceLogisticsSummary)).toContain('河西');
    await expect.poll(() => debug(page).then((state) => state.ui.governanceLogisticsSummary)).toContain('容量');
    await expect(page.getByTestId('governance-logistics-effect')).toContainText('截粮');

    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('liangzhou'));
    await page.locator('#war-mode').click();
    await page.locator('[data-army-id="army_player_2"]').click();
    const afterGovernance = await debug(page);
    const afterCapacity = routeCapacityFromSummary(afterGovernance.ui.routeRoadSummary);

    expect(afterCapacity).toBeGreaterThan(beforeCapacity);
    expect(afterGovernance.ui.routeRoadSummary).toContain('治理');
    expect(afterGovernance.ui.routeRoadSummary).toContain('截粮 -');

    await page.locator('[data-war-tab="logistics"]').click();
    await expect(page.getByTestId('governance-logistics-list')).toContainText('河西');
    await page.locator('[data-action="war_supply"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.nextCommandSummary)).toContain(`/${afterCapacity}`);
    expect(consoleErrors).toEqual([]);
  });

  test('compares route alternatives before committing war commands', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.locator('#war-mode').click();
    await page.locator('[data-army-id="army_player_2"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyId)).toBe('army_player_2');
    await expect(page.getByTestId('route-alternatives')).toContainText('路线方案对比');
    await expect(page.getByTestId('route-alternatives')).toContainText('容量');
    await expect(page.getByTestId('route-alternatives')).toContainText('补给');
    await expect(page.getByTestId('route-alternatives')).toContainText('截粮');
    await expect.poll(() => debug(page).then((state) => state.ui.routeAlternativeSummary)).toContain('direct');

    const viaOption = page.locator('[data-route-alternative-id^="via-"]').first();
    await expect(viaOption).toBeVisible();
    const chosenAlternativeId = await viaOption.getAttribute('data-route-alternative-id');
    if (!chosenAlternativeId) throw new Error('No route alternative id found.');
    expect(chosenAlternativeId).toContain('via-');
    await viaOption.click();

    await expect.poll(() => debug(page).then((state) => state.ui.selectedRouteAlternativeId)).toBe(chosenAlternativeId);
    await expect.poll(() => debug(page).then((state) => state.activeArmyWaypointId)).not.toBe('');
    await expect(page.getByTestId('route-alternatives')).toContainText('已采用');
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('采用路线方案');

    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.nextCommandSummary)).toContain('经');
    const exported = await exportWarLogistics(page);
    expect(exported.activeArmyId).toBe('army_player_2');
    expect(exported.activeArmyWaypointId).not.toBe('');
    expect(exported.routeAlternatives.some((alternative) => alternative.id === chosenAlternativeId && alternative.selected)).toBe(true);
    const waypointConvoy = exported.transportConvoys.find((convoy) => convoy.routeLegs.length > 1);
    expect(waypointConvoy).toBeTruthy();
    if (!waypointConvoy) throw new Error('No waypoint convoy with split route legs found.');
    expect(waypointConvoy.routeLegs.every((leg) => leg.routeUsage > 0 && leg.routeCapacity > 0)).toBe(true);
    for (const leg of waypointConvoy.routeLegs) {
      expect(leg.routeId).toBe([leg.fromRegionId, leg.toRegionId].sort().join('->'));
      expect(exported.routeCapacities.some((capacity) => capacity.routeId === leg.routeId && capacity.currentUsage === leg.routeUsage)).toBe(true);
    }
    await openApp(page);
    expect(await importWarLogistics(page, exported)).toBe(true);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyId)).toBe(exported.activeArmyId);
    await expect.poll(() => debug(page).then((state) => state.activeArmyId)).toBe(exported.activeArmyId);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyTargetId)).toBe(exported.activeArmyTargetId);
    await expect.poll(() => debug(page).then((state) => state.activeArmyTargetId)).toBe(exported.activeArmyTargetId);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyWaypointId)).toBe(exported.activeArmyWaypointId);
    await expect.poll(() => debug(page).then((state) => state.activeArmyWaypointId)).toBe(exported.activeArmyWaypointId);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedRouteAlternativeId)).toBe(chosenAlternativeId);
    expect(consoleErrors).toEqual([]);
  });

  test('uses named strategic route network for route capacity and convoy export', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('hanzhong'));
    await page.locator('#war-mode').click();
    await expect.poll(() => debug(page).then((state) => state.ui.routeRoadSummary)).toContain('秦岭栈道');
    await expect(page.getByTestId('route-alternatives')).toContainText('路网 秦岭栈道');

    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.nextCommandSummary)).toContain('路线 1/1');

    const exported = await exportWarLogistics(page);
    const qinlingRoute = exported.routeCapacities.find((capacity) => capacity.routeId === 'guanzhong->hanzhong');
    expect(qinlingRoute).toBeTruthy();
    if (!qinlingRoute) throw new Error('No Qinling route capacity found.');
    expect(qinlingRoute.networkLabel).toBe('秦岭栈道');
    expect(qinlingRoute.maxArmies).toBe(1);
    expect(qinlingRoute.terrainReason).toContain('已标注路网');
    expect(exported.transportConvoys.some((convoy) => convoy.bottleneckLabel === '秦岭栈道')).toBe(true);
    expect(consoleErrors).toEqual([]);
  });

  test('enemy interdiction targets named route-network bottlenecks', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('hanzhong'));
    await page.locator('#war-mode').click();
    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();
    await page.locator('[data-action="war_advance_turn"]').click();

    await expect.poll(() => debug(page).then((state) => state.ui.enemyInterdictionOrderLength)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.ui.latestInterceptionAlert)).toContain('秦岭栈道');
    await expect(page.getByTestId('selected-enemy-interdiction')).toContainText('秦岭栈道');
    await expect(page.getByTestId('logistics-dispatcher')).toContainText('秦岭栈道');

    const exported = await exportWarLogistics(page);
    const order = exported.enemyInterdiction.activeOrders.find((candidate) => candidate.chokeNetworkLabel === '秦岭栈道');
    expect(order).toBeTruthy();
    if (!order) throw new Error('No enemy interdiction order targeting Qinling bottleneck.');
    expect(order.chokeRouteId).toBe('guanzhong->hanzhong');
    expect(order.chokePointLabel).toContain('秦岭栈道');
    expect(consoleErrors).toEqual([]);
  });

  test('creates selectable blockade objects for route-network choke points', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('hanzhong'));
    await page.locator('#war-mode').click();
    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();
    await page.locator('[data-action="war_advance_turn"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.latestInterceptionAlert)).toContain('秦岭栈道');

    const initialExport = await exportWarLogistics(page);
    const qinlingNetwork = initialExport.routeNetworks.find((candidate) => candidate.id === 'qinling_plank_roads');
    expect(qinlingNetwork).toBeTruthy();
    if (!qinlingNetwork) throw new Error('No Qinling route network data found.');
    expect(qinlingNetwork.blockade.guardStrengthGain).toBe(24);
    const blockade = initialExport.routeBlockades.find((candidate) => candidate.networkLabel === '秦岭栈道');
    expect(blockade).toBeTruthy();
    if (!blockade) throw new Error('No Qinling route blockade found.');
    expect(blockade.networkId).toBe(qinlingNetwork.id);
    expect(blockade.status).toBe('enemy-blockade');
    expect(initialExport.logisticsMapObjects.some((object) => object.id === blockade.id && object.kind === 'route-blockade')).toBe(true);

    const point = await getLogisticsObjectPoint(page, blockade.id);
    expect(point?.visible).toBe(true);
    if (!point) throw new Error('No blockade logistics object screen point found.');
    await page.mouse.click(point.x, point.y);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedLogisticsObjectId)).toBe(blockade.id);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedLogisticsObjectSummary)).toContain('秦岭栈道');

    await page.locator('[data-action="route_blockade_guard"]').click();
    const guardedExport = await exportWarLogistics(page);
    const guarded = guardedExport.routeBlockades.find((candidate) => candidate.id === blockade.id);
    expect(guarded?.status).toBe('guarded');
    expect(guarded?.guardStrength).toBe(qinlingNetwork.blockade.guardStrengthGain);
    expect(guarded?.strength).toBe(Math.max(0, blockade.strength - qinlingNetwork.blockade.guardBlockadeReduction));
    expect(guardedExport.enemyInterdiction.activeOrders.some((order) => order.lastCountermeasure === '瓶颈守备')).toBe(true);

    await openApp(page);
    expect(await importWarLogistics(page, guardedExport)).toBe(true);
    const restoredExport = await exportWarLogistics(page);
    const restoredBlockade = restoredExport.routeBlockades.find((candidate) => candidate.id === blockade.id);
    expect(restoredBlockade?.status).toBe('guarded');
    expect(restoredExport.selectedLogisticsObjectId).toBe(blockade.id);
    expect(restoredExport.enemyInterdiction.activeOrders.some((order) => order.chokeRouteId === 'guanzhong->hanzhong' && order.lastCountermeasure === '瓶颈守备')).toBe(true);
    expect(restoredExport.logisticsMapObjects.some((object) => object.id === blockade.id && object.kind === 'route-blockade')).toBe(true);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedLogisticsObjectId)).toBe(blockade.id);

    await page.locator('[data-action="route_blockade_clear"]').click();
    const clearedExport = await exportWarLogistics(page);
    const cleared = clearedExport.routeBlockades.find((candidate) => candidate.id === blockade.id);
    expect(cleared?.status).toBe('cleared');
    expect(cleared?.strength).toBe(0);
    expect(cleared?.guardStrength).toBe(qinlingNetwork.blockade.guardStrengthGain + qinlingNetwork.blockade.clearGuardStrengthGain);
    expect(clearedExport.logisticsMapObjects.some((object) => object.id === blockade.id)).toBe(false);
    expect(consoleErrors).toEqual([]);
  });

  test('exports recoverable war logistics state across convoys, governance, and interdiction memory', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.locator('#war-mode').click();
    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();
    await page.locator('[data-action="war_supply"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.transportConvoyCount)).toBe(2);

    for (let i = 0; i < 5; i += 1) {
      const state = await debug(page);
      if (state.ui.enemyInterdictionOrderLength > 0) break;
      await page.locator('[data-action="war_advance_turn"]').click();
    }

    await expect.poll(() => debug(page).then((state) => state.ui.enemyInterdictionOrderLength)).toBeGreaterThan(0);
    await page.locator('[data-enemy-interdiction-id]').first().click();
    const selectedThreat = (await debug(page)).ui.selectedEnemyInterdictionId;
    expect(selectedThreat).toContain('enemy_interdiction_');
    await page.locator('[data-action="war_counter_escort"]').click();

    await page.locator('#governance-mode').click();
    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('hexi'));
    await page.locator('[data-action="governance_build"]').click();
    await page.locator('[data-action="governance_policy"]').click();

    await page.locator('#war-mode').click();
    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();
    const interimExport = await exportWarLogistics(page);
    const selectableObject = interimExport.logisticsMapObjects.find((object) => object.kind === 'transport-convoy');
    expect(selectableObject).toBeTruthy();
    if (!selectableObject) throw new Error('No transport logistics object in export.');
    const objectPoint = await getLogisticsObjectPoint(page, selectableObject.id);
    expect(objectPoint?.visible).toBe(true);
    if (!objectPoint) throw new Error('No logistics object screen point found.');
    await page.mouse.click(objectPoint.x, objectPoint.y);

    const exported = await exportWarLogistics(page);
    expect(exported.schemaVersion).toBe(1);
    expect(exported.turn).toBeGreaterThan(1);
    expect(exported.activeArmyId).toBeTruthy();
    expect(exported.selectedLogisticsObjectId).toBe(selectableObject.id);
    expect(exported.selectedEnemyInterdictionId).toBe(selectedThreat);
    expect(exported.enemyStrategyPhase.activeTargetCount).toBeGreaterThan(0);
    expect(exported.enemyStrategyPhase.phaseLabel).not.toBe('');
    expect(exported.enemyStrategyPhase.reasoning).not.toBe('');
    expect(exported.enemyInterdiction.selectedOrderId).toBe(selectedThreat);
    expect(exported.enemyInterdiction.activeOrders.some((order) => order.lastCountermeasure === '护粮队')).toBe(true);
    expect(exported.logisticsMapObjects.some((object) => object.id === selectableObject.id && object.kind === 'transport-convoy')).toBe(true);
    expect(exported.transportConvoys.some((convoy) => convoy.id === selectableObject.id && convoy.routeCapacity > 0 && convoy.bottleneckLabel)).toBe(true);
    expect(exported.routeCapacities.some((route) => route.currentUsage > 0 && route.terrainReason)).toBe(true);
    expect(exported.governanceLogisticsEffects.some((effect) => effect.regionId === 'hexi' && effect.capacityBonus > 0 && effect.sources.length >= 2)).toBe(true);
    expect(consoleErrors).toEqual([]);
  });

  test('exports and imports full game state across governance, army, logistics, and UI chrome', async ({ page }) => {
    test.setTimeout(75_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.locator('[data-emperor-id="li_shi_min"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.selectedEmperorId)).toBe('li_shi_min');

    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('hexi'));
    const beforeGovernance = await debug(page);
    await page.locator('[data-action="governance_build"]').click();
    await page.locator('[data-action="governance_policy"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.money)).toBeLessThan(beforeGovernance.ui.money);

    await page.locator('#war-mode').click();
    await page.locator('[data-army-id="army_player_2"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyId)).toBe('army_player_2');
    const viaOption = page.locator('[data-route-alternative-id^="via-"]').first();
    await expect(viaOption).toBeVisible();
    const chosenAlternativeId = await viaOption.getAttribute('data-route-alternative-id');
    if (!chosenAlternativeId) throw new Error('No route alternative id found.');
    await viaOption.click();
    await expect.poll(() => debug(page).then((state) => state.ui.selectedRouteAlternativeId)).toBe(chosenAlternativeId);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyWaypointId)).not.toBe('');

    await page.locator('[data-war-tab="army"]').click();
    await page.locator('[data-action="army_order_forced_march"]').click();
    await page.locator('[data-action="army_split"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyId)).toContain('army_player_detached_');
    await page.locator('[data-action="army_general_next"]').click();
    await page.locator('[data-action="army_mix_cavalry"]').click();

    await page.locator('[data-war-tab="logistics"]').click();
    await page.locator('[data-action="war_supply"]').click();
    await page.locator('[data-action="war_attack"]').click();
    await page.locator('[data-action="war_advance_turn"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.commandQueueLength)).toBeGreaterThan(0);
    await page.locator('#sidebar-toggle').click();
    await expect.poll(() => debug(page).then((state) => state.sidebarCollapsed)).toBe(true);

    const savedDebug = await debug(page);
    const saved = await exportGameState(page);
    expect(saved.schemaVersion).toBe(1);
    expect(saved.mode).toBe('war');
    expect(saved.warTab).toBe('logistics');
    expect(saved.sidebarCollapsed).toBe(true);
    expect(saved.selectedEmperorId).toBe('li_shi_min');
    expect(saved.nationState.money).toBe(savedDebug.ui.money);
    expect(saved.regions.some((region) => region.id === 'hexi' && region.contribution > beforeGovernance.ui.selectedContribution)).toBe(true);
    expect(saved.armies.some((army) => army.id === savedDebug.ui.activeArmyId && army.id.startsWith('army_player_detached_'))).toBe(true);
    expect(saved.queues.governance.length).toBeGreaterThan(0);
    expect(saved.queues.commands.length).toBeGreaterThan(0);
    expect(saved.warLogistics.transportConvoys.length).toBeGreaterThan(0);

    await openApp(page);
    expect(await importGameState(page, saved)).toBe(true);
    await expect.poll(() => debug(page).then((state) => state.mode)).toBe(saved.mode);
    await expect.poll(() => debug(page).then((state) => state.sidebarCollapsed)).toBe(true);
    await expect.poll(() => debug(page).then((state) => state.selectedRegionId)).toBe(saved.selectedRegionId);
    await expect.poll(() => debug(page).then((state) => state.ui.selectedEmperorId)).toBe(saved.selectedEmperorId);
    await expect.poll(() => debug(page).then((state) => state.ui.money)).toBe(saved.nationState.money);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyId)).toBe(savedDebug.ui.activeArmyId);
    await expect.poll(() => debug(page).then((state) => state.activeArmyId)).toBe(savedDebug.ui.activeArmyId);
    await expect.poll(() => debug(page).then((state) => state.ui.activeArmyWaypointId)).toBe(savedDebug.ui.activeArmyWaypointId);
    await expect.poll(() => debug(page).then((state) => state.ui.commandQueueLength)).toBe(saved.queues.commands.length);
    await expect.poll(() => debug(page).then((state) => state.ui.transportConvoyCount)).toBe(saved.warLogistics.transportConvoys.length);
    await expect(page.getByTestId('logistics-queue')).toContainText('运输队');

    await page.locator('#sidebar-toggle').click();
    await expect.poll(() => debug(page).then((state) => state.sidebarCollapsed)).toBe(false);
    const beforeAdvance = await debug(page);
    await page.locator('[data-action="war_advance_turn"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.warTurn)).toBe(beforeAdvance.ui.warTurn + 1);
    expect(consoleErrors).toEqual([]);
  });

  test('saves and loads full game state through local slots with schema errors', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);
    await page.evaluate(() => localStorage.clear());
    await openApp(page);

    await expect.poll(() => debug(page).then((state) => state.ui.saveSlotCount)).toBe(3);
    await expect(page.getByTestId('save-panel')).toContainText('空槽');

    await page.locator('[data-emperor-id="li_shi_min"]').click();
    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('hexi'));
    const beforeGovernance = await debug(page);
    await page.locator('[data-action="governance_build"]').click();
    await page.locator('[data-action="governance_policy"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.money)).toBeLessThan(beforeGovernance.ui.money);
    const savedState = await debug(page);

    await page.locator('[data-action="save_slot_1"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.saveSlotMessage)).toContain('一号槽 已保存');
    await expect(page.getByTestId('save-panel')).toContainText('河西');
    await expect(page.getByTestId('save-panel')).toContainText('李世民');

    await page.evaluate(() => (window as any).__WANCHAO_APP__.selectRegion('qilu'));
    await page.locator('[data-emperor-id="qin_shi_huang"]').click();
    await expect.poll(() => debug(page).then((state) => state.selectedRegionId)).toBe('qilu');

    await openApp(page);
    await expect(page.getByTestId('save-panel')).toContainText('河西');
    await page.locator('[data-action="load_slot_1"]').click();
    await expect.poll(() => debug(page).then((state) => state.selectedRegionId)).toBe('hexi');
    await expect.poll(() => debug(page).then((state) => state.ui.selectedEmperorId)).toBe('li_shi_min');
    await expect.poll(() => debug(page).then((state) => state.ui.money)).toBe(savedState.ui.money);
    await expect.poll(() => debug(page).then((state) => state.ui.saveSlotError)).toBe('');

    await page.evaluate(() => {
      localStorage.setItem('wanchao:strategy-map:save:slot_2', JSON.stringify({
        schemaVersion: 1,
        slotId: 'slot_2',
        savedAtIso: new Date().toISOString(),
        summary: { mode: 'war', regionName: '坏档', emperorName: '坏档', warTurn: 1, food: 0, money: 0, legitimacy: 0 },
        state: { schemaVersion: 999 }
      }));
    });
    await openApp(page);
    await expect(page.getByTestId('save-panel')).toContainText('版本或结构异常');
    await page.locator('[data-action="load_slot_2"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.saveSlotError)).toContain('游戏状态版本不匹配');
    await expect(page.getByTestId('save-panel')).toContainText('需要 1');

    await page.locator('[data-action="delete_slot_1"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.saveSlotMessage)).toContain('一号槽 已清除');
    expect(consoleErrors).toEqual([]);
  });

  test('coordinates depots, route capacity, interdiction targeting, and occupation supply', async ({ page }) => {
    test.setTimeout(60_000);
    const consoleErrors = captureConsoleErrors(page);
    await openApp(page);

    await page.locator('#war-mode').click();
    await clickVisibleRegion(page);
    await page.locator('[data-war-tab="logistics"]').click();
    await expect(page.getByTestId('logistics-dispatcher')).toContainText('后勤调度中心');

    await page.locator('[data-action="war_fortify"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.routeCapacityCount)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.ui.nextCommandSummary)).toContain('路线');
    await expect.poll(() => debug(page).then((state) => state.ui.routeRoadSummary)).toContain('容量');
    for (let i = 0; i < 3; i += 1) {
      const state = await debug(page);
      if (state.ui.logisticsStationCount > 0) break;
      await page.locator('[data-action="war_advance_turn"]').click();
    }
    await expect.poll(() => debug(page).then((state) => state.ui.logisticsStationCount)).toBeGreaterThan(0);
    await expect(page.getByTestId('logistics-dispatcher')).toContainText('兵站');

    await page.locator('[data-action="war_supply"]').click();
    await page.locator('[data-action="war_attack"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.routeCapacitySummary)).toContain('/');
    await expect(page.getByTestId('war-command-queue')).toContainText('路线');

    await page.locator('[data-action="war_advance_turn"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.enemyInterdictionOrderLength)).toBeGreaterThan(0);
    await expect.poll(() => debug(page).then((state) => state.ui.enemyInterdictionDoctrine)).not.toBe('');
    await expect.poll(() => debug(page).then((state) => state.ui.enemyInterdictionMemory)).toContain('成功');
    await expect(page.getByTestId('logistics-dispatcher')).toContainText('截粮威胁优先级');

    for (let i = 0; i < 8; i += 1) {
      const state = await debug(page);
      if (state.ui.occupationSupplyTaskCount > 0) break;
      await page.locator('[data-action="war_advance_turn"]').click();
    }
    await expect.poll(() => debug(page).then((state) => state.ui.occupationSupplyTaskCount)).toBeGreaterThan(0);
    await expect(page.getByTestId('logistics-dispatcher')).toContainText('安抚运输任务');

    await page.locator('[data-action="war_advance_turn"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.occupationSupplySummary)).toContain('运输');
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('安抚运输');
    await page.locator('[data-action="occupation_aftercare"]').click();
    await page.locator('[data-action="occupation_aftercare"]').click();
    await expect(page.getByTestId('occupation-supply-controls')).toBeVisible();
    await page.locator('[data-action="occupation_supply_promote"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('已上移');
    await page.locator('[data-action="occupation_supply_cancel"]').click();
    await expect.poll(() => debug(page).then((state) => state.ui.latestOperation)).toContain('已取消');
    expect(consoleErrors).toEqual([]);
  });

  for (const viewport of [
    { width: 1280, height: 720, name: '1280x720' },
    { width: 1024, height: 576, name: '1024x576' }
  ]) {
    test(`shows geography and building layer at ${viewport.name}`, async ({ page }) => {
      const consoleErrors = captureConsoleErrors(page);
      await page.setViewportSize(viewport);
      await openApp(page);

      await expect.poll(() => debug(page).then((state) => state.terrainFeatureCount)).toBe(56);
      await expect.poll(() => debug(page).then((state) => state.buildingMarkerCount)).toBe(56);
      await expect(page.getByTestId('geography-building')).toContainText('地理与建设');
      await expect(page.getByTestId('real-geography')).toContainText('现实地貌');
      await expect(page.locator('.landform-label:not(.hidden)').first()).toBeVisible();
      await expect(page.locator('.building-label:not(.hidden)').first()).toBeVisible();
      expect(await labelOverlapCount(page)).toBe(0);

      const screenshot = await page.screenshot({
        path: `test-results/strategy-map-governance-${viewport.name}.png`,
        fullPage: false
      });
      expect(screenshot.length).toBeGreaterThan(20_000);
      expect(consoleErrors).toEqual([]);
    });

    test(`keeps labels and panels usable at ${viewport.name}`, async ({ page }) => {
      const consoleErrors = captureConsoleErrors(page);
      await page.setViewportSize(viewport);
      await openApp(page);

      await page.locator('#war-mode').click();
      await expect.poll(() => debug(page).then((state) => state.visibleLabels)).toBeGreaterThan(0);
      expect(await labelOverlapCount(page)).toBe(0);
      await expect(page.locator('#sidebar')).toBeVisible();
      await expect(page.locator('.command-strip')).toBeVisible();
      if (viewport.width <= 1024) {
        await expect(page.locator('#route-summary .route-summary-compact')).toBeVisible();
        await expect(page.locator('#route-summary .route-summary-full')).toBeHidden();
        await expect(page.locator('#route-summary .route-summary-compact')).toContainText('补');
        await page.locator('#route-summary').hover();
        await expect(page.locator('#route-pressure-detail')).toBeVisible();
        await expect(page.locator('#route-pressure-detail')).toContainText('位置详情');
      }

      const screenshot = await page.screenshot({
        path: `test-results/strategy-map-${viewport.name}.png`,
        fullPage: false
      });
      expect(screenshot.length).toBeGreaterThan(20_000);
      expect(consoleErrors).toEqual([]);
    });
  }
});

async function openApp(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForFunction(() => document.documentElement.dataset.appReady === 'true');
}

function captureConsoleErrors(page: Page): string[] {
  const errors: string[] = [];
  page.on('console', (message) => {
    if (message.type() === 'error') {
      const text = message.text();
      // Filter out audio file fetch errors (expected when audio files not served)
      if (!text.includes('Failed to fetch') && !text.includes('net::ERR')) {
        errors.push(text);
      }
    }
  });
  page.on('pageerror', (error) => errors.push(error.message));
  return errors;
}

async function debug(page: Page): Promise<DebugState> {
  return page.evaluate(() => (window as any).__WANCHAO_APP__.getDebugState());
}

async function getLogisticsObjectPoint(page: Page, objectId: string): Promise<{ x: number; y: number; visible: boolean } | null> {
  return page.evaluate((id) => (window as any).__WANCHAO_APP__.getLogisticsObjectScreenPoint(id), objectId);
}

async function getEnemyInterdictionPoint(page: Page, orderId: string): Promise<{ x: number; y: number; visible: boolean } | null> {
  return page.evaluate((id) => (window as any).__WANCHAO_APP__.getEnemyInterdictionScreenPoint(id), orderId);
}

async function exportWarLogistics(page: Page): Promise<WarLogisticsExportState> {
  return page.evaluate(() => (window as any).__WANCHAO_APP__.exportWarLogisticsState());
}

async function importWarLogistics(page: Page, snapshot: WarLogisticsExportState): Promise<boolean> {
  return page.evaluate((state) => (window as any).__WANCHAO_APP__.importWarLogisticsState(state), snapshot);
}

async function exportGameState(page: Page): Promise<GameExportState> {
  return page.evaluate(() => (window as any).__WANCHAO_APP__.exportGameState());
}

async function importGameState(page: Page, snapshot: GameExportState): Promise<boolean> {
  return page.evaluate((state) => (window as any).__WANCHAO_APP__.importGameState(state), snapshot);
}

function routeCapacityFromSummary(summary: string): number {
  const match = summary.match(/容量\s+(\d+)/);
  if (!match) throw new Error(`Route capacity not found in summary: ${summary}`);
  return Number(match[1]);
}

async function dragMap(page: Page): Promise<void> {
  const box = await page.locator('#map-canvas').boundingBox();
  expect(box).not.toBeNull();
  if (!box) return;
  const startX = box.x + box.width * 0.46;
  const startY = box.y + box.height * 0.48;
  await page.mouse.move(startX, startY);
  await page.mouse.down();
  await page.mouse.move(startX + 120, startY + 46, { steps: 8 });
  await page.mouse.up();
  await page.waitForTimeout(220);
}

async function moveMouseToMap(page: Page): Promise<void> {
  const box = await page.locator('#map-canvas').boundingBox();
  expect(box).not.toBeNull();
  if (!box) return;
  await page.mouse.move(box.x + box.width * 0.42, box.y + box.height * 0.52);
}

async function clickVisibleRegion(page: Page, excludedRegionIds: string[] = []): Promise<string> {
  const initial = await debug(page);
  const candidates = [
    'hanzhong',
    'guanzhong',
    'zhongyuan',
    'luoyang',
    'bashu',
    'hexi',
    'jingxiang',
    'qilu',
    'hedong',
    'huainan',
    'jiangdong',
    'liangzhou',
    'youyan',
    'longxi'
  ];
  const target = await page.evaluate(
    ({ candidates, initialId, excludedRegionIds }) => {
      const app = (window as any).__WANCHAO_APP__;
      const canvas = document.getElementById('map-canvas');
      const excluded = new Set([initialId, ...excludedRegionIds]);
      for (const id of candidates) {
        if (excluded.has(id)) continue;
        const point = app.getRegionScreenPoint(id);
        const topElement = point ? document.elementFromPoint(point.x, point.y) : null;
        if (
          point?.visible &&
          point.x > 40 &&
          point.y > 40 &&
          point.x < window.innerWidth - 40 &&
          point.y < window.innerHeight - 40 &&
          topElement === canvas
        ) {
          return { id, point };
        }
      }
      return null;
    },
    { candidates, initialId: initial.selectedRegionId, excludedRegionIds }
  );

  expect(target).not.toBeNull();
  if (!target) throw new Error('No visible region candidate found.');
  await page.mouse.click(target.point.x, target.point.y);
  await page.waitForTimeout(160);
  return target.id;
}

async function dragRouteWaypointToVisibleRegion(page: Page, waypointRegionId: string, excludedRegionIds: string[] = []): Promise<string> {
  const candidates = [
    'qilu',
    'zhongyuan',
    'luoyang',
    'guanzhong',
    'jingxiang',
    'bashu',
    'hexi',
    'longxi',
    'hedong',
    'huainan',
    'jiangdong',
    'liangzhou'
  ];
  const target = await page.evaluate(
    ({ waypointRegionId, candidates, excludedRegionIds }) => {
      const app = (window as any).__WANCHAO_APP__;
      const canvas = document.getElementById('map-canvas');
      const start = app.getRegionScreenPoint(waypointRegionId);
      const excluded = new Set([waypointRegionId, ...excludedRegionIds]);
      for (const id of candidates) {
        if (excluded.has(id)) continue;
        const point = app.getRegionScreenPoint(id);
        const topElement = point ? document.elementFromPoint(point.x, point.y) : null;
        if (
          point?.visible &&
          start?.visible &&
          point.x > 40 &&
          point.y > 40 &&
          point.x < window.innerWidth - 40 &&
          point.y < window.innerHeight - 40 &&
          topElement === canvas
        ) {
          return { id, start, point };
        }
      }
      return null;
    },
    { waypointRegionId, candidates, excludedRegionIds }
  );

  expect(target).not.toBeNull();
  if (!target) throw new Error('No visible waypoint drag target found.');
  await page.mouse.move(target.start.x, target.start.y);
  await page.mouse.down();
  await page.mouse.move(target.point.x, target.point.y, { steps: 10 });
  await page.mouse.up();
  await page.waitForTimeout(180);
  return target.id;
}

async function labelOverlapCount(page: Page): Promise<number> {
  return page.evaluate(() => {
    const rects = [...document.querySelectorAll<HTMLElement>('.map-label:not(.hidden)')]
      .map((element) => element.getBoundingClientRect())
      .filter((rect) => rect.width > 0 && rect.height > 0);
    let overlaps = 0;
    for (let i = 0; i < rects.length; i += 1) {
      for (let j = i + 1; j < rects.length; j += 1) {
        const a = rects[i];
        const b = rects[j];
        if (!(a.right <= b.left || a.left >= b.right || a.bottom <= b.top || a.top >= b.bottom)) {
          overlaps += 1;
        }
      }
    }
    return overlaps;
  });
}
