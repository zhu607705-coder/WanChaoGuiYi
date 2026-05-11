import './styles.css';
import { StrategyAudio, type AudioDebugState } from './audio';
import { loadStrategyDataset } from './data';
import { LabelManager } from './labels';
import { StrategyScene } from './scene';
import { StrategyUi } from './ui';
import type { GameMode } from './types';

declare global {
  interface Window {
    __WANCHAO_APP__?: {
      setMode: (mode: GameMode) => void;
      selectRegion: (regionId: string) => boolean;
      getDebugState: () => {
        mode: GameMode;
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
        audio: AudioDebugState;
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
          enemyInterdictionExport: unknown;
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
      getRegionScreenPoint: (regionId: string) => { x: number; y: number; visible: boolean } | null;
      getLogisticsObjectScreenPoint: (objectId: string) => { x: number; y: number; visible: boolean } | null;
      getEnemyInterdictionScreenPoint: (orderId: string) => { x: number; y: number; visible: boolean } | null;
      exportWarLogisticsState: () => unknown;
      importWarLogisticsState: (snapshot: unknown) => boolean;
      exportGameState: () => unknown;
      importGameState: (snapshot: unknown) => boolean;
    };
  }
}

async function bootstrap(): Promise<void> {
  const canvas = document.getElementById('map-canvas');
  const labelLayer = document.getElementById('label-layer');
  if (!(canvas instanceof HTMLCanvasElement) || !labelLayer) {
    throw new Error('App shell is missing required map elements.');
  }

  const dataset = await loadStrategyDataset();
  let ui: StrategyUi | undefined;
  let labelManager: LabelManager | undefined;
  let scene: StrategyScene | undefined;
  const audio = new StrategyAudio(
    dataset.audio.sceneMusic,
    dataset.audio.emperorThemes,
    dataset.audio.chronicleEvents,
    dataset.audio.narration
  );
  bindAudioHud(audio);

  const applyWarSelection = (regionId: string): void => {
    if (!scene || scene.getMode() !== 'war') return;
    if (ui?.getRoutePickMode() === 'waypoint') {
      scene.setActiveArmyWaypoint(regionId);
    } else {
      scene.retargetActiveArmy(regionId);
    }
  };

  scene = new StrategyScene(canvas, dataset, {
    onSelectRegion(region) {
      ui?.setSelectedRegion(region);
      applyWarSelection(region.definition.id);
      document.documentElement.dataset.selectedRegion = region.definition.id;
    },
    onHoverRegion(region) {
      document.documentElement.dataset.hoverRegion = region?.definition.id ?? '';
    },
    onRouteEdited(region) {
      ui?.setSelectedRegion(region);
      labelManager?.update(true);
      document.documentElement.dataset.selectedRegion = region.definition.id;
    },
    onSelectLogisticsObject(objectId) {
      if (!ui?.selectLogisticsMapObject(objectId)) return;
      scene?.selectLogisticsMapObject(objectId);
      labelManager?.update(true);
    },
    onSelectEnemyInterdiction(orderId) {
      if (!ui?.selectEnemyInterdictionTarget(orderId)) return;
      scene?.selectEnemyInterdictionTarget(orderId);
      scene?.setEnemyInterdictionTargets(ui.getEnemyInterdictionTargets());
      labelManager?.update(true);
    }
  });

  const setMode = (mode: GameMode): void => {
    if (!scene) return;
    scene.setMode(mode);
    ui?.setMode(mode);
    labelManager?.setMode(mode);
    document.documentElement.dataset.mode = mode;
    if (mode === 'war') {
      scene.retargetActiveArmy(scene.getSelectedRegion().definition.id);
      scene.setLogisticsMapObjects(ui?.getLogisticsMapObjects() ?? []);
    }
    void audio.setMode(mode).then(() => renderAudioHud(audio));
  };

  const selectRegion = (regionId: string): boolean => {
    if (!scene) return false;
    const region = scene.selectRegion(regionId);
    if (!region) return false;
    ui?.setSelectedRegion(region);
    labelManager?.update(true);
    document.documentElement.dataset.selectedRegion = region.definition.id;
    return true;
  };

  ui = new StrategyUi(dataset, {
    onModeChange: setMode,
    onSelectRegion: selectRegion,
    onEmperorChange(emperor) {
      void audio.setEmperor(emperor.id).then(() => renderAudioHud(audio));
    },
    onArmyChange(armyId) {
      const army = scene?.setActiveArmy(armyId);
      if (army) selectRegion(army.targetRegionId);
    },
    onSelectEnemyInterdiction(orderId) {
      scene?.selectEnemyInterdictionTarget(orderId);
      scene?.setEnemyInterdictionTargets(ui?.getEnemyInterdictionTargets() ?? []);
      labelManager?.update(true);
    },
    onAction(action) {
      if (action === 'war_attack') {
        void audio.playWarAction().then(() => renderAudioHud(audio));
      } else if (action.startsWith('war_') || action.startsWith('army_')) {
        void audio.playLogisticsAction().then(() => renderAudioHud(audio));
      } else {
        void audio.playGovernanceAction().then(() => renderAudioHud(audio));
      }
    },
    onStateMutated() {
      if (!scene) return;
      scene.syncArmyMarkers();
      scene.refreshActiveRoute();
      scene.setEnemyInterdictionTargets(ui?.getEnemyInterdictionTargets() ?? []);
      scene.setLogisticsMapObjects(ui?.getLogisticsMapObjects() ?? []);
      scene.setMode(scene.getMode());
      labelManager?.update(true);
    },
    onGameStateImported(selectedRegionId) {
      syncSceneAfterStateImport(selectedRegionId);
    }
  });
  labelManager = new LabelManager(labelLayer, scene);

  const syncSceneAfterStateImport = (selectedRegionId?: string): void => {
    if (!ui || !scene) return;
    const restoredState = ui.exportWarLogisticsState();
    scene.setActiveArmy(restoredState.activeArmyId);
    if (restoredState.activeArmyTargetId) {
      scene.retargetActiveArmy(restoredState.activeArmyTargetId);
    }
    scene.setActiveArmyWaypoint(restoredState.activeArmyWaypointId || null);
    scene.syncArmyMarkers();
    scene.setEnemyInterdictionTargets(ui.getEnemyInterdictionTargets());
    scene.setLogisticsMapObjects(ui.getLogisticsMapObjects());
    if (restoredState.selectedEnemyInterdictionId) {
      scene.selectEnemyInterdictionTarget(restoredState.selectedEnemyInterdictionId);
    }
    if (restoredState.selectedLogisticsObjectId) {
      scene.selectLogisticsMapObject(restoredState.selectedLogisticsObjectId);
    }
    setMode(ui.getMode());
    selectRegion(selectedRegionId || restoredState.activeArmyTargetId || scene.getSelectedRegion().definition.id);
    labelManager?.update(true);
  };

  scene.start();
  labelManager.start();
  setMode('governance');
  selectRegion(scene.getSelectedRegion().definition.id);

  window.__WANCHAO_APP__ = {
    setMode,
    selectRegion,
    getDebugState() {
      try {
        return {
          ...scene.getDebugState(),
          visibleLabels: Number(document.documentElement.dataset.visibleLabels ?? 0),
          sidebarCollapsed: document.getElementById('sidebar')?.classList.contains('collapsed') ?? false,
          audio: audio.getDebugState(),
          ui: ui?.getDebugState() ?? {
            food: 0,
            money: 0,
            army: 0,
            legitimacy: 0,
            governanceQueueLength: 0,
            logisticsQueueLength: 0,
            commandQueueLength: 0,
            occupationQueueLength: 0,
            enemyInterdictionOrderLength: 0,
            occupationStageSummary: '',
            enemyInterdictionSummary: '',
            enemyCountermeasureSummary: '',
            selectedEnemyInterdictionId: '',
            selectedEnemyInterdictionSummary: '',
            enemyInterdictionExport: {},
            enemyInterdictionDoctrine: '',
            enemyInterdictionMemory: '',
            logisticsStationCount: 0,
            routeCapacityCount: 0,
            routeCapacitySummary: '',
            routeRoadSummary: '',
            routeAlternativeSummary: '',
            selectedRouteAlternativeId: '',
            transportConvoyCount: 0,
            transportConvoySummary: '',
            occupationSupplyTaskCount: 0,
            occupationSupplySummary: '',
            logisticsMapObjectCount: 0,
            selectedLogisticsObjectId: '',
            selectedLogisticsObjectSummary: '',
            governanceLogisticsSummary: '',
            routePressureSummary: '',
            routePressureCompactSummary: '',
            routePressureDetail: '',
            selectedControlStage: 'controlled',
            selectedContribution: 0,
            selectedRisk: 0,
            selectedIntegration: 0,
            selectedLegitimacy: 0,
            governanceFocus: 'relief',
            governanceMicroSummary: '',
            warTurn: 0,
            latestInterceptionAlert: '',
            nextCommandSummary: '',
            latestOperation: '',
            selectedEmperorId: '',
            selectedEmperorMechanic: '',
            armyOrder: '',
            armyFormation: '',
            activeArmySupply: 0,
            activeArmyMorale: 0,
            activeArmySoldiers: 0,
            activeArmyGeneral: '',
            activeArmyUnitMix: '',
            playerArmyCount: 0,
            activeArmyId: '',
            activeArmyTargetId: '',
            activeArmyWaypointId: '',
            routePickMode: 'target',
            saveSlotMessage: '',
            saveSlotError: '',
            saveSlotCount: 0
          }
        };
      } catch (err) {
        console.error('[getDebugState error]', err);
        return {
          scene: null,
          ui: { warTurn: -1, commandQueueLength: -1, food: 0, money: 0, army: 0, legitimacy: 0, governanceQueueLength: 0, logisticsQueueLength: 0, occupationQueueLength: 0, enemyInterdictionOrderLength: 0, occupationStageSummary: '', enemyInterdictionSummary: '', enemyCountermeasureSummary: '', selectedEnemyInterdictionId: '', selectedEnemyInterdictionSummary: '', enemyInterdictionExport: {}, enemyInterdictionDoctrine: '', enemyInterdictionMemory: '', logisticsStationCount: 0, routeCapacityCount: 0, routeCapacitySummary: '', routeRoadSummary: '', routeAlternativeSummary: '', selectedRouteAlternativeId: '', transportConvoyCount: 0, transportConvoySummary: '', occupationSupplyTaskCount: 0, occupationSupplySummary: '', logisticsMapObjectCount: 0, selectedLogisticsObjectId: '', selectedLogisticsObjectSummary: '', governanceLogisticsSummary: '', routePressureSummary: '', routePressureCompactSummary: '', routePressureDetail: '', selectedControlStage: 'controlled', selectedContribution: 0, selectedRisk: 0, selectedIntegration: 0, selectedLegitimacy: 0, governanceFocus: 'relief', governanceMicroSummary: '', latestInterceptionAlert: '', nextCommandSummary: '', latestOperation: '', selectedEmperorId: '', selectedEmperorMechanic: '', armyOrder: '', armyFormation: '', activeArmySupply: 0, activeArmyMorale: 0, activeArmySoldiers: 0, activeArmyGeneral: '', activeArmyUnitMix: '', playerArmyCount: 0, activeArmyId: '', activeArmyTargetId: '', activeArmyWaypointId: '', routePickMode: 'target' as const, saveSlotMessage: '', saveSlotError: '', saveSlotCount: 0 },
          mode: 'war' as const,
          selectedRegionId: '',
          regionMeshCount: 0,
          armyMarkerCount: 0,
          terrainFeatureCount: 0,
          buildingMarkerCount: 0,
          routeVisible: false,
          cameraDistance: 0,
          cameraX: 0,
          cameraZ: 0,
          targetX: 0,
          targetZ: 0,
          drawCalls: 0,
          routeRaidSegmentCount: 0,
          routeConvoyMarkerCount: 0,
          routeDragHandleCount: 0,
          occupationBadgeCount: 0,
          enemyThreatMarkerCount: 0,
          enemyThreatMovingCount: 0,
          enemyThreatDampenedCount: 0,
          friendlyCountermeasureMarkerCount: 0,
          friendlyCountermeasureMovingCount: 0,
          logisticsMapObjectCount: 0,
          selectedLogisticsObjectId: '',
          selectedEnemyInterdictionId: '',
          activeArmyId: '',
          activeArmyTargetId: '',
          activeArmyWaypointId: '',
          visibleLabels: 0,
          sidebarCollapsed: false,
          audio: { enabled: false, mode: 'governance' as const, currentMusicCue: '', currentNarration: '', currentVoice: '', sceneCueCount: 0, emperorThemeCount: 0, chronicleEventCount: 0, lastError: String(err) }
        };
      }
    },
    getRegionScreenPoint: (regionId: string) => scene.getRegionScreenPoint(regionId),
    getLogisticsObjectScreenPoint: (objectId: string) => scene.getLogisticsObjectScreenPoint(objectId),
    getEnemyInterdictionScreenPoint: (orderId: string) => scene.getEnemyInterdictionScreenPoint(orderId),
    exportWarLogisticsState: () => ui?.exportWarLogisticsState() ?? {},
    importWarLogisticsState: (snapshot: unknown) => {
      if (!ui) return false;
      const restored = ui.importWarLogisticsState(snapshot as ReturnType<StrategyUi['exportWarLogisticsState']>);
      if (!restored) return false;
      syncSceneAfterStateImport();
      return true;
    },
    exportGameState: () => ui?.exportGameState() ?? {},
    importGameState: (snapshot: unknown) => {
      if (!ui) return false;
      const restored = ui.importGameState(snapshot as ReturnType<StrategyUi['exportGameState']>);
      if (!restored) return false;
      const gameState = ui.exportGameState();
      syncSceneAfterStateImport(gameState.selectedRegionId);
      return true;
    }
  };

  document.documentElement.dataset.appReady = 'true';
}

bootstrap().catch((error: unknown) => {
  const message = error instanceof Error ? error.message : String(error);
  console.error(message);
  document.body.insertAdjacentHTML(
    'beforeend',
    `<div class="fatal-error" role="alert">加载失败：${escapeHtml(message)}</div>`
  );
});

function escapeHtml(value: string): string {
  return value.replace(/[&<>"']/g, (char) => {
    const entities: Record<string, string> = {
      '&': '&amp;',
      '<': '&lt;',
      '>': '&gt;',
      '"': '&quot;',
      "'": '&#39;'
    };
    return entities[char] ?? char;
  });
}

function bindAudioHud(audio: StrategyAudio): void {
  renderAudioHud(audio);
  document.getElementById('audio-enable')?.addEventListener('click', () => {
    void audio.enable().then(() => renderAudioHud(audio));
  });
  document.querySelectorAll<HTMLButtonElement>('[data-audio-action]').forEach((button) => {
    button.addEventListener('click', () => {
      const action = button.dataset.audioAction;
      const task =
        action === 'emperor'
          ? audio.playEmperorTheme()
          : action === 'event'
            ? audio.playEventCue()
            : audio.setMode(audio.getDebugState().mode);
      void task.then(() => renderAudioHud(audio));
    });
  });
}

function renderAudioHud(audio: StrategyAudio): void {
  const state = audio.getDebugState();
  document.documentElement.dataset.audioEnabled = String(state.enabled);
  setText('audio-status', state.enabled ? '音频已启用' : '点击启用音频');
  setText('audio-mode', state.mode === 'war' ? '战争混音' : '治理混音');
  setText('audio-music', state.currentMusicCue);
  setText('audio-narration', state.currentNarration);
  setText('audio-voice', state.currentVoice);
  setText('audio-catalog', `曲目 ${state.sceneCueCount} / 帝王 ${state.emperorThemeCount} / 事件 ${state.chronicleEventCount}`);
  setText('audio-error', state.lastError);
}

function setText(id: string, text: string): void {
  const element = document.getElementById(id);
  if (element) element.textContent = text;
}
