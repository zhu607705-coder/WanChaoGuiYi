import type { StrategyDataset } from './data';
import type { ArmyViewModel, BuildingDefinition, EmperorDefinition, GameMode, GeneralDefinition, GovernanceFocusId, GovernanceLaborId, LogisticsMapObject, PolicyDefinition, RegionViewModel, RouteForecast, RouteNetworkBlockadeDefinition, RouteNetworkDefinition, RouteRoadClass, UiAction, UnitDefinition } from './types';

export interface UiEvents {
  onModeChange: (mode: GameMode) => void;
  onSelectRegion: (regionId: string) => void;
  onEmperorChange: (emperor: EmperorDefinition) => void;
  onArmyChange: (armyId: string) => void;
  onSelectEnemyInterdiction?: (orderId: string) => void;
  onAction: (action: UiAction, region: RegionViewModel) => void;
  onStateMutated: () => void;
  onGameStateImported?: (selectedRegionId: string) => void;
}

type WarCommandKind = 'deploy' | 'supply' | 'scout' | 'fortify' | 'attack';
type OccupationAftercareStage = 'military-govern' | 'pacify' | 'register';
type EnemyInterdictionStage = 'planning' | 'moving' | 'striking' | 'resolved';
type EnemyInterdictionDoctrine = 'cut-supply' | 'bleed-army' | 'stall-pacification';
type EnemyInterdictionStrategicPhase = 'probing' | 'supply-strangulation' | 'field-army-harassment' | 'pacification-blockade' | 'exploitation';
type TransportConvoyStatus = 'queued' | 'moving' | 'delivered' | 'cancelled';
type RouteBlockadeStatus = 'enemy-blockade' | 'guarded' | 'cleared';

interface WarCommand {
  id: string;
  kind: WarCommandKind;
  label: string;
  armyId: string;
  fromRegionId: string;
  targetRegionId: string;
  targetName: string;
  fromName: string;
  waypointRegionId?: string;
  waypointName?: string;
  createdTurn: number;
  remainingTurns: number;
  totalTurns: number;
  interceptionRisk: number;
  supplyReserve: number;
  plannedSupplyReserve: number;
  deliveredSupply: number;
  completedSegments: number;
  segmentCount: number;
  routeCapacity: number;
  routeUsage: number;
  stationBonus: number;
  roadClass: RouteRoadClass;
  bottleneckLabel: string;
  convoyId?: string;
  convoyPriority?: number;
  convoyOrder?: number;
  alert?: string;
  routeLegs: CommandRouteLeg[];
}

interface CommandRouteLeg {
  routeId: string;
  fromRegionId: string;
  toRegionId: string;
  fromName: string;
  toName: string;
  routeUsage: number;
  routeCapacity: number;
  roadClass: RouteRoadClass;
  bottleneckLabel: string;
  terrainReason: string;
  networkId?: string;
  networkLabel?: string;
}

interface TransportConvoy {
  id: string;
  commandId: string;
  armyId: string;
  routeLabel: string;
  fromRegionId: string;
  targetRegionId: string;
  status: TransportConvoyStatus;
  priority: number;
  orderIndex: number;
  createdTurn: number;
  plannedSupplyReserve: number;
  supplyReserve: number;
  deliveredSupply: number;
  completedSegments: number;
  segmentCount: number;
  routeUsage: number;
  routeCapacity: number;
  roadClass: RouteRoadClass;
  bottleneckLabel: string;
  routeLegs: CommandRouteLeg[];
}

interface OccupationAftercareTask {
  id: string;
  regionId: string;
  regionName: string;
  stage: OccupationAftercareStage;
  remainingTurns: number;
  riskPressure: number;
  contributionCap: number;
}

interface EnemyInterdictionOrder {
  id: string;
  armyId: string;
  targetRegionId: string;
  targetName: string;
  routeLabel: string;
  chokeRouteId: string;
  chokePointLabel: string;
  chokeNetworkLabel: string;
  chokeReason: string;
  stage: EnemyInterdictionStage;
  remainingTurns: number;
  totalTurns: number;
  risk: number;
  supplyDamage: number;
  createdTurn: number;
  lastCountermeasure?: string;
  resolved?: boolean;
}

interface RouteBlockade {
  id: string;
  orderId: string;
  chokeRouteId: string;
  fromRegionId: string;
  toRegionId: string;
  targetRegionId: string;
  chokePointLabel: string;
  networkId?: string;
  networkLabel: string;
  status: RouteBlockadeStatus;
  strength: number;
  guardStrength: number;
  createdTurn: number;
  lastAction: string;
}

// ============================================
// Logistics Dispatcher System - Core Types
// ============================================

/** Station information for fortification management */
interface LogisticsStation {
  id: string;
  regionId: string;
  regionName: string;
  supplyBonus: number;
  moraleBonus: number;
  riskReduction: number;
  isActive: boolean;
}

interface GovernanceLogisticsEffect {
  regionId: string;
  regionName: string;
  capacityBonus: number;
  supplyRelief: number;
  interdictionRelief: number;
  occupationBandwidthBonus: number;
  sources: string[];
}

interface GovernanceLogisticsDelta {
  capacityBonus: number;
  supplyRelief: number;
  interdictionRelief: number;
  occupationBandwidthBonus: number;
}

/** Route capacity constraint */
interface RouteCapacityConstraint {
  routeId: string;
  fromRegion: string;
  toRegion: string;
  maxArmies: number;
  currentUsage: number;
  congestionLevel: 'low' | 'medium' | 'high' | 'critical';
  roadClass: RouteRoadClass;
  bottleneckLabel: string;
  terrainReason: string;
  networkId?: string;
  networkLabel?: string;
}

/** Interdiction priority ranking */
interface InterdictionPriority {
  orderId: string;
  targetRegion: string;
  targetName: string;
  riskLevel: number;
  supplyDamage: number;
  priority: number;
  recommendedCountermeasure: 'escort' | 'reroute' | 'counter-scout' | 'decoy';
  reasoning: string;
}

/** Occupation supply task */
interface OccupationSupplyTask {
  taskId: string;
  convoyId: string;
  fromRegionId: string;
  regionId: string;
  regionName: string;
  routeLabel: string;
  stage: 'military-govern' | 'pacify' | 'register';
  supplyNeeded: number;
  priority: number;
  orderIndex: number;
  bandwidthUsed: number;
  routeUsageClaimed: boolean;
  autoDispatchTurn: number;
  status: 'pending' | 'dispatched' | 'in-transit' | 'delivered' | 'cancelled';
}

interface RouteTerrainProfile {
  roadClass: RouteRoadClass;
  bottleneckLabel: string;
  baseCapacity: number;
  supplyFactor: number;
  interceptionModifier: number;
  terrainReason: string;
  networkId?: string;
  networkLabel?: string;
}

interface EnemyInterdictionMemory {
  doctrine: EnemyInterdictionDoctrine;
  pressureByRegion: Record<string, number>;
  lastTargetRegionId: string;
  successfulRaids: number;
  failedRaids: number;
  lastReasoning: string;
}

interface EnemyInterdictionExportState {
  selectedOrderId: string;
  doctrine: EnemyInterdictionDoctrine;
  successfulRaids: number;
  failedRaids: number;
  lastTargetRegionId: string;
  lastReasoning: string;
  pressureByRegion: Record<string, number>;
  activeOrders: Array<{
    id: string;
    armyId: string;
    targetRegionId: string;
    routeLabel: string;
    chokeRouteId: string;
    chokePointLabel: string;
    chokeNetworkLabel: string;
    chokeReason: string;
    stage: EnemyInterdictionStage;
    remainingTurns: number;
    risk: number;
    supplyDamage: number;
    lastCountermeasure: string;
  }>;
}

interface EnemyInterdictionStrategicPhaseState {
  phase: EnemyInterdictionStrategicPhase;
  phaseLabel: string;
  doctrine: EnemyInterdictionDoctrine;
  activeTargetCount: number;
  selectedOrderId: string;
  pressureRegionCount: number;
  reasoning: string;
}

interface WarLogisticsExportState {
  schemaVersion: 1;
  turn: number;
  activeArmyId: string;
  activeArmyTargetId?: string;
  activeArmyWaypointId?: string;
  selectedLogisticsObjectId: string;
  selectedEnemyInterdictionId: string;
  routeAlternatives: RouteAlternative[];
  enemyStrategyPhase: EnemyInterdictionStrategicPhaseState;
  enemyInterdiction: EnemyInterdictionExportState;
  logisticsMapObjects: LogisticsMapObject[];
  routeNetworks: RouteNetworkDefinition[];
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
    status: RouteBlockadeStatus;
    strength: number;
    guardStrength: number;
    createdTurn?: number;
    lastAction: string;
  }>;
  logisticsStations: Array<{
    id: string;
    regionId: string;
    regionName: string;
    supplyBonus: number;
    moraleBonus: number;
    riskReduction: number;
    isActive: boolean;
  }>;
  governanceLogisticsEffects: Array<{
    regionId: string;
    regionName: string;
    capacityBonus: number;
    supplyRelief: number;
    interdictionRelief: number;
    occupationBandwidthBonus: number;
    sources: string[];
  }>;
  transportConvoys: Array<{
    id: string;
    commandId: string;
    armyId: string;
    fromRegionId: string;
    targetRegionId: string;
    routeLabel: string;
    status: TransportConvoyStatus;
    priority: number;
    orderIndex: number;
    createdTurn: number;
    roadClass: RouteRoadClass;
    bottleneckLabel: string;
    routeUsage: number;
    routeCapacity: number;
    plannedSupplyReserve: number;
    deliveredSupply: number;
    supplyReserve: number;
    completedSegments: number;
    segmentCount: number;
    routeLegs: CommandRouteLeg[];
  }>;
  occupationSupplyTasks: Array<{
    id: string;
    convoyId: string;
    fromRegionId: string;
    regionId: string;
    regionName: string;
    routeLabel: string;
    stage: OccupationSupplyTask['stage'];
    status: OccupationSupplyTask['status'];
    priority: number;
    orderIndex?: number;
    supplyNeeded: number;
    bandwidthUsed: number;
    routeUsageClaimed: boolean;
    autoDispatchTurn?: number;
  }>;
  routeCapacities: Array<{
    routeId: string;
    fromRegion: string;
    toRegion: string;
    maxArmies: number;
    currentUsage: number;
    congestionLevel: RouteCapacityConstraint['congestionLevel'];
    roadClass: RouteRoadClass;
    bottleneckLabel: string;
    terrainReason: string;
    networkId?: string;
    networkLabel?: string;
  }>;
}

interface GameExportState {
  schemaVersion: 1;
  mode: GameMode;
  governanceTurn: number;
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
  nationState: StrategyDataset['nation'];
  regions: Array<{
    id: string;
    owner: RegionViewModel['owner'];
    controlStage: RegionViewModel['controlStage'];
    specialization: string;
    governanceFocus: GovernanceFocusId;
    laborFocus: GovernanceLaborId;
    integration: number;
    contribution: number;
    risk: number;
    legitimacy: number;
  }>;
  armies: Array<{
    id: string;
    name: string;
    faction: ArmyViewModel['faction'];
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
    projects: GovernanceProject[];
    logistics: string[];
    operation: string[];
    occupation: OccupationAftercareTask[];
    commands: WarCommand[];
    battleReports: BattleOutcome[];
  };
  warLogistics: WarLogisticsExportState;
}

interface GovernanceFocusDelta {
  food: number;
  money: number;
  army: number;
  legitimacy: number;
  integration: number;
  contribution: number;
  risk: number;
  logistics: GovernanceLogisticsDelta;
}

interface GovernanceFocusPlan {
  id: GovernanceFocusId;
  label: string;
  specialization: string;
  actionLabel: string;
  description: string;
  source: string;
  costs: {
    food: number;
    money: number;
  };
  delta: GovernanceFocusDelta;
}

interface GovernanceLaborDelta {
  food: number;
  money: number;
  army: number;
  legitimacy: number;
  integration: number;
  contribution: number;
  risk: number;
}

interface GovernanceLaborPlan {
  id: GovernanceLaborId;
  label: string;
  actionLabel: string;
  description: string;
  delta: GovernanceLaborDelta;
}

interface GovernanceProject {
  id: string;
  regionId: string;
  regionName: string;
  buildingName: string;
  buildingCategory: string;
  focusId: GovernanceFocusId;
  laborHint: GovernanceLaborId;
  remainingTurns: number;
  totalTurns: number;
  foodYield: number;
  moneyYield: number;
  contributionDelta: number;
  integrationDelta: number;
  riskDelta: number;
  legitimacyDelta: number;
  logistics: GovernanceLogisticsDelta;
  source: string;
}

type SaveSlotId = 'slot_1' | 'slot_2' | 'slot_3';

interface SaveSlotDefinition {
  id: SaveSlotId;
  label: string;
}

interface GameSaveEnvelope {
  schemaVersion: 1;
  slotId: SaveSlotId;
  savedAtIso: string;
  summary: {
    mode: GameMode;
    regionName: string;
    emperorName: string;
    warTurn: number;
    food: number;
    money: number;
    legitimacy: number;
  };
  state: GameExportState;
}

interface RoutePressureCopy {
  full: string;
  compact: string;
  detail: string;
  detailHtml: string;
}

interface RouteLegEstimate {
  from: RegionViewModel;
  target: RegionViewModel;
  distance: number;
  profile: RouteTerrainProfile;
  capacity: number;
  currentUsage: number;
}

interface RouteMetricEstimate {
  legs: RouteLegEstimate[];
  supplyCost: number;
  turns: number;
  contactChance: number;
  occupationCost: number;
  interceptionRisk: number;
  routeCapacity: number;
  routeUsage: number;
  roadClass: RouteRoadClass;
  bottleneckLabel: string;
  terrainReason: string;
  networkLabel: string;
}

interface RouteAlternative {
  id: string;
  label: string;
  routeLabel: string;
  waypointRegionId: string;
  capacity: number;
  currentUsage: number;
  supplyCost: number;
  interceptionRisk: number;
  turns: number;
  bottleneckLabel: string;
  terrainReason: string;
  networkLabel: string;
  score: number;
  selected: boolean;
  recommendation: string;
}

interface BattleOutcome {
  turn: number;
  regionName: string;
  kind: 'attack' | 'supply' | 'scout' | 'deploy' | 'fortify';
  result: string;
  armyName?: string;
  generalName?: string;
  unitMix?: string;
  tacticScore?: string;
  tacticText?: string;
  tacticDeltas?: TacticalModifier;
  interceptionRisk?: number;
  casualties?: number;
  supplyUsed?: number;
  success: boolean;
}

interface TacticalModifier {
  supplyDelta: number;
  contactDelta: number;
  interceptionDelta: number;
  occupationDelta: number;
}

const DEFAULT_ROUTE_BLOCKADE_RULES: RouteNetworkBlockadeDefinition = {
  initialStrengthFloor: 12,
  refreshStrengthGain: 12,
  guardFoodCost: 8,
  guardMoneyCost: 6,
  guardStrengthGain: 22,
  guardBlockadeReduction: 18,
  guardRiskReduction: 14,
  guardDamageReduction: 3,
  clearFoodCost: 10,
  clearMoneyCost: 8,
  clearGuardStrengthGain: 12,
  clearRiskReduction: 28
};

const GAME_STATE_SCHEMA_VERSION = 1;
const LOCAL_SAVE_SCHEMA_VERSION = 1;
const SAVE_SLOT_PREFIX = 'wanchao:strategy-map:save:';
const SAVE_SLOTS: SaveSlotDefinition[] = [
  { id: 'slot_1', label: '一号槽' },
  { id: 'slot_2', label: '二号槽' },
  { id: 'slot_3', label: '三号槽' }
];
const GOVERNANCE_FOCUS_IDS: GovernanceFocusId[] = ['grain', 'tax', 'military', 'frontier', 'legitimacy', 'relief'];
const GOVERNANCE_LABOR_IDS: GovernanceLaborId[] = ['balanced', 'grain', 'tax', 'military', 'stability'];

export class StrategyUi {
  private mode: GameMode = 'governance';
  private selectedRegion: RegionViewModel;
  private selectedEmperor: EmperorDefinition;
  private activeArmy: ArmyViewModel;
  private sidebarCollapsed = false;
  private routePickMode: 'target' | 'waypoint' = 'target';
  private warTab: 'route' | 'army' | 'logistics' | 'report' = 'route';
  private readonly nationState: StrategyDataset['nation'];
  private armyOrder = {
    stance: '稳进',
    formation: '中军合进',
    route: '主道补给',
    last: '待命'
  };
  private currentGovernanceTurn = 1;
  private readonly governanceQueue: string[] = [];
  private readonly governanceProjects: GovernanceProject[] = [];
  private readonly logisticsQueue: string[] = [];
  private readonly operationLog: string[] = ['朝议已开：选择地区后可执行治理、建设或部署。'];
  private readonly commandQueue: WarCommand[] = [];
  private readonly transportConvoys: TransportConvoy[] = [];
  private readonly occupationQueue: OccupationAftercareTask[] = [];
  private readonly enemyInterdictionOrders: EnemyInterdictionOrder[] = [];
  private readonly routeBlockades: RouteBlockade[] = [];
  private readonly battleReportHistory: BattleOutcome[] = [];

  // ============================================
  // Logistics Dispatcher System
  // ============================================
  private readonly logisticsStations: LogisticsStation[] = [];
  private readonly routeCapacities: Map<string, RouteCapacityConstraint> = new Map();
  private readonly occupationSupplyTasks: OccupationSupplyTask[] = [];
  private readonly governanceLogisticsEffects: Map<string, GovernanceLogisticsEffect> = new Map();
  private supplyAutomationEnabled = true;
  private autoSupplyThreshold = 60; // Auto-dispatch when army supply < 60%
  private stationCounter = 1;
  private occupationSupplyCounter = 1;
  private readonly enemyInterdictionMemory: EnemyInterdictionMemory = {
    doctrine: 'cut-supply',
    pressureByRegion: {},
    lastTargetRegionId: '',
    successfulRaids: 0,
    failedRaids: 0,
    lastReasoning: '敌军尚未形成稳定截粮偏好'
  };

  private currentWarTurn = 1;
  private commandCounter = 1;
  private governanceProjectCounter = 1;
  private transportConvoyCounter = 1;
  private routeBlockadeCounter = 1;
  private occupationCounter = 1;
  private enemyInterdictionCounter = 1;
  private selectedEnemyInterdictionId = '';
  private latestInterceptionAlert = '';
  private splitCounter = 1;
  private lastCountermeasureSummary = '';
  private lastResolvedRoutePressure: RoutePressureCopy | null = null;
  private selectedLogisticsObjectId = '';
  private saveSlotMessage = '本地槽位待保存。';
  private saveSlotError = '';

  constructor(private readonly dataset: StrategyDataset, private readonly events: UiEvents) {
    this.selectedRegion = dataset.regions[0];
    this.selectedEmperor = dataset.emperors[0];
    this.activeArmy = dataset.armies.find((army) => army.faction === 'player') ?? dataset.armies[0];
    this.nationState = { ...dataset.nation };
    this.bindControls();
    this.renderNation();
    this.render();
    this.renderEmperorDock();
  }

  setSelectedRegion(region: RegionViewModel): void {
    this.selectedRegion = region;
    this.render();
  }

  setMode(mode: GameMode): void {
    this.mode = mode;
    document.getElementById('governance-mode')?.classList.toggle('active', mode === 'governance');
    document.getElementById('war-mode')?.classList.toggle('active', mode === 'war');
    document.getElementById('governance-panel')?.classList.toggle('hidden', mode !== 'governance');
    document.getElementById('war-panel')?.classList.toggle('hidden', mode !== 'war');
    const pill = document.getElementById('mode-pill');
    if (pill) pill.textContent = mode === 'governance' ? '治理' : '战争';
    this.render();
  }

  getMode(): GameMode {
    return this.mode;
  }

  getDebugState(): {
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
    enemyInterdictionExport: EnemyInterdictionExportState;
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
    selectedControlStage: RegionViewModel['controlStage'];
    selectedContribution: number;
    selectedRisk: number;
    selectedIntegration: number;
    selectedLegitimacy: number;
    governanceFocus: GovernanceFocusId;
    governanceMicroSummary: string;
    laborFocus: GovernanceLaborId;
    laborSummary: string;
    governanceTurn: number;
    governanceProjectCount: number;
    governanceProjectSummary: string;
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
  } {
    const routePressure = this.currentRoutePressureCopy();
    const logisticsMapObjects = this.getLogisticsMapObjects();
    const selectedLogisticsObject = this.selectedLogisticsObject();
    const selectedEnemyInterdiction = this.selectedEnemyInterdictionOrder();
    const activeEnemyInterdiction = this.activeEnemyInterdictionOrder();
    const routeAlternatives = this.routeAlternativesFor(this.selectedRegion);
    return {
      ...this.nationState,
      governanceQueueLength: this.governanceQueue.length,
      logisticsQueueLength: this.logisticsQueue.length,
      commandQueueLength: this.commandQueue.length,
      occupationQueueLength: this.occupationQueue.length,
      enemyInterdictionOrderLength: this.enemyInterdictionOrders.length,
      occupationStageSummary: this.occupationQueue.length > 0 ? describeOccupationTask(this.occupationQueue[0]) : '',
      enemyInterdictionSummary: activeEnemyInterdiction ? describeEnemyInterdictionOrder(activeEnemyInterdiction) : '',
      enemyCountermeasureSummary: activeEnemyInterdiction?.lastCountermeasure ?? this.lastCountermeasureSummary,
      selectedEnemyInterdictionId: selectedEnemyInterdiction?.id ?? '',
      selectedEnemyInterdictionSummary: selectedEnemyInterdiction ? describeEnemyInterdictionOrder(selectedEnemyInterdiction) : '',
      enemyInterdictionExport: this.exportEnemyInterdictionState(),
      enemyInterdictionDoctrine: this.enemyInterdictionDoctrineName(),
      enemyInterdictionMemory: this.enemyInterdictionMemorySummary(),
      logisticsStationCount: this.logisticsStations.filter((station) => station.isActive).length,
      routeCapacityCount: this.routeCapacities.size,
      routeCapacitySummary: this.routeCapacitySummary(),
      routeRoadSummary: this.routeRoadSummary(),
      routeAlternativeSummary: summarizeRouteAlternatives(routeAlternatives),
      selectedRouteAlternativeId: routeAlternatives.find((alternative) => alternative.selected)?.id ?? '',
      transportConvoyCount: this.activeTransportConvoys().length,
      transportConvoySummary: this.activeTransportConvoys()[0] ? describeTransportConvoy(this.activeTransportConvoys()[0]) : '',
      occupationSupplyTaskCount: this.occupationSupplyTasks.length,
      occupationSupplySummary: this.occupationSupplyTasks[0] ? describeOccupationSupplyTask(this.occupationSupplyTasks[0]) : '',
      logisticsMapObjectCount: logisticsMapObjects.length,
      selectedLogisticsObjectId: selectedLogisticsObject?.id ?? '',
      selectedLogisticsObjectSummary: selectedLogisticsObject?.details ?? '',
      governanceLogisticsSummary: this.governanceLogisticsSummary(),
      routePressureSummary: routePressure.full,
      routePressureCompactSummary: routePressure.compact,
      routePressureDetail: routePressure.detail,
      selectedControlStage: this.selectedRegion.controlStage,
      selectedContribution: Math.round(this.selectedRegion.contribution),
      selectedRisk: Math.round(this.selectedRegion.risk),
      selectedIntegration: Math.round(this.selectedRegion.integration),
      selectedLegitimacy: Math.round(this.selectedRegion.legitimacy),
      governanceFocus: this.selectedRegion.governanceFocus,
      governanceMicroSummary: this.governanceFocusPlan(this.selectedRegion.governanceFocus, this.selectedRegion).description,
      laborFocus: this.selectedRegion.laborFocus,
      laborSummary: formatGovernanceLaborDelta(this.governanceLaborPlan(this.selectedRegion.laborFocus, this.selectedRegion).delta),
      governanceTurn: this.currentGovernanceTurn,
      governanceProjectCount: this.governanceProjects.length,
      governanceProjectSummary: this.governanceProjects[0] ? describeGovernanceProject(this.governanceProjects[0]) : '',
      warTurn: this.currentWarTurn,
      latestInterceptionAlert: this.latestInterceptionAlert,
      nextCommandSummary: this.commandQueue.length > 0 ? describeWarCommand(this.commandQueue[this.commandQueue.length - 1]) : '',
      latestOperation: this.operationLog[0] ?? '',
      selectedEmperorId: this.selectedEmperor.id,
      selectedEmperorMechanic: this.selectedEmperor.uniqueMechanic.name,
      armyOrder: this.armyOrder.last,
      armyFormation: this.armyOrder.formation,
      activeArmySupply: this.activeArmy.supply,
      activeArmyMorale: this.activeArmy.morale,
      activeArmySoldiers: this.activeArmy.soldiers,
      activeArmyGeneral: this.activeArmy.general,
      activeArmyUnitMix: unitMixText(this.activeArmy.unitMix, this.dataset.units),
      playerArmyCount: this.dataset.armies.filter((army) => army.faction === 'player').length,
      activeArmyId: this.activeArmy.id,
      activeArmyTargetId: this.activeArmy.targetRegionId,
      activeArmyWaypointId: this.activeArmy.waypointRegionId ?? '',
      routePickMode: this.routePickMode,
      saveSlotMessage: this.saveSlotMessage,
      saveSlotError: this.saveSlotError,
      saveSlotCount: SAVE_SLOTS.length
    };
  }

  exportEnemyInterdictionState(): EnemyInterdictionExportState {
    return {
      selectedOrderId: this.selectedEnemyInterdictionOrder()?.id ?? '',
      doctrine: this.enemyInterdictionMemory.doctrine,
      successfulRaids: this.enemyInterdictionMemory.successfulRaids,
      failedRaids: this.enemyInterdictionMemory.failedRaids,
      lastTargetRegionId: this.enemyInterdictionMemory.lastTargetRegionId,
      lastReasoning: this.enemyInterdictionMemory.lastReasoning,
      pressureByRegion: { ...this.enemyInterdictionMemory.pressureByRegion },
      activeOrders: this.enemyInterdictionOrders.map((order) => ({
        id: order.id,
        armyId: order.armyId,
        targetRegionId: order.targetRegionId,
        routeLabel: order.routeLabel,
        chokeRouteId: order.chokeRouteId,
        chokePointLabel: order.chokePointLabel,
        chokeNetworkLabel: order.chokeNetworkLabel,
        chokeReason: order.chokeReason,
        stage: order.stage,
        remainingTurns: order.remainingTurns,
        risk: order.risk,
        supplyDamage: order.supplyDamage,
        lastCountermeasure: order.lastCountermeasure ?? ''
      }))
    };
  }

  exportWarLogisticsState(): WarLogisticsExportState {
    return {
      schemaVersion: 1,
      turn: this.currentWarTurn,
      activeArmyId: this.activeArmy.id,
      activeArmyTargetId: this.activeArmy.targetRegionId,
      activeArmyWaypointId: this.activeArmy.waypointRegionId ?? '',
      selectedLogisticsObjectId: this.selectedLogisticsObject()?.id ?? '',
      selectedEnemyInterdictionId: this.selectedEnemyInterdictionOrder()?.id ?? '',
      routeAlternatives: this.routeAlternativesFor(this.selectedRegion),
      enemyStrategyPhase: this.enemyInterdictionStrategicPhase(),
      enemyInterdiction: this.exportEnemyInterdictionState(),
      logisticsMapObjects: this.getLogisticsMapObjects().map((object) => ({ ...object })),
      routeNetworks: this.dataset.routeNetworks.map((network) => ({
        ...network,
        nodes: [...network.nodes],
        blockade: { ...network.blockade }
      })),
      routeBlockades: this.routeBlockades.map((blockade) => ({ ...blockade })),
      logisticsStations: this.logisticsStations.map((station) => ({
        id: station.id,
        regionId: station.regionId,
        regionName: station.regionName,
        supplyBonus: station.supplyBonus,
        moraleBonus: station.moraleBonus,
        riskReduction: station.riskReduction,
        isActive: station.isActive
      })),
      governanceLogisticsEffects: [...this.governanceLogisticsEffects.values()].map((effect) => ({
        regionId: effect.regionId,
        regionName: effect.regionName,
        capacityBonus: effect.capacityBonus,
        supplyRelief: effect.supplyRelief,
        interdictionRelief: effect.interdictionRelief,
        occupationBandwidthBonus: effect.occupationBandwidthBonus,
        sources: [...effect.sources]
      })),
      transportConvoys: this.transportConvoys.map((convoy) => ({
        id: convoy.id,
        commandId: convoy.commandId,
        armyId: convoy.armyId,
        fromRegionId: convoy.fromRegionId,
        targetRegionId: convoy.targetRegionId,
        routeLabel: convoy.routeLabel,
        status: convoy.status,
        priority: convoy.priority,
        orderIndex: convoy.orderIndex,
        createdTurn: convoy.createdTurn,
        roadClass: convoy.roadClass,
        bottleneckLabel: convoy.bottleneckLabel,
        routeUsage: convoy.routeUsage,
        routeCapacity: convoy.routeCapacity,
        plannedSupplyReserve: convoy.plannedSupplyReserve,
        deliveredSupply: convoy.deliveredSupply,
        supplyReserve: convoy.supplyReserve,
        completedSegments: convoy.completedSegments,
        segmentCount: convoy.segmentCount,
        routeLegs: convoy.routeLegs.map((leg) => ({ ...leg }))
      })),
      occupationSupplyTasks: this.occupationSupplyTasks.map((task) => ({
        id: task.taskId,
        convoyId: task.convoyId,
        fromRegionId: task.fromRegionId,
        regionId: task.regionId,
        regionName: task.regionName,
        routeLabel: task.routeLabel,
        stage: task.stage,
        status: task.status,
        priority: task.priority,
        supplyNeeded: task.supplyNeeded,
        bandwidthUsed: task.bandwidthUsed,
        routeUsageClaimed: task.routeUsageClaimed
      })),
      routeCapacities: [...this.routeCapacities.values()].map((capacity) => ({
        routeId: capacity.routeId,
        fromRegion: capacity.fromRegion,
        toRegion: capacity.toRegion,
        maxArmies: capacity.maxArmies,
        currentUsage: capacity.currentUsage,
        congestionLevel: capacity.congestionLevel,
        roadClass: capacity.roadClass,
        bottleneckLabel: capacity.bottleneckLabel,
        terrainReason: capacity.terrainReason,
        networkId: capacity.networkId,
        networkLabel: capacity.networkLabel
      }))
    };
  }

  exportGameState(): GameExportState {
    return {
      schemaVersion: GAME_STATE_SCHEMA_VERSION,
      mode: this.mode,
      governanceTurn: this.currentGovernanceTurn,
      selectedRegionId: this.selectedRegion.definition.id,
      selectedEmperorId: this.selectedEmperor.id,
      sidebarCollapsed: this.sidebarCollapsed,
      routePickMode: this.routePickMode,
      warTab: this.warTab,
      armyOrder: { ...this.armyOrder },
      nationState: { ...this.nationState },
      regions: this.dataset.regions.map((region) => ({
        id: region.definition.id,
        owner: region.owner,
        controlStage: region.controlStage,
        specialization: region.specialization,
        governanceFocus: region.governanceFocus,
        laborFocus: region.laborFocus,
        integration: region.integration,
        contribution: region.contribution,
        risk: region.risk,
        legitimacy: region.legitimacy
      })),
      armies: this.dataset.armies.map((army) => ({
        id: army.id,
        name: army.name,
        faction: army.faction,
        fromRegionId: army.fromRegionId,
        targetRegionId: army.targetRegionId,
        waypointRegionId: army.waypointRegionId,
        soldiers: army.soldiers,
        supply: army.supply,
        morale: army.morale,
        general: army.general,
        generalId: army.generalId,
        unitId: army.unit.id,
        unitMix: { ...army.unitMix }
      })),
      queues: {
        governance: [...this.governanceQueue],
        projects: this.governanceProjects.map((project) => ({
          ...project,
          logistics: { ...project.logistics }
        })),
        logistics: [...this.logisticsQueue],
        operation: [...this.operationLog],
        occupation: this.occupationQueue.map((task) => ({ ...task })),
        commands: this.commandQueue.map((command) => ({
          ...command,
          routeLegs: command.routeLegs.map((leg) => ({ ...leg }))
        })),
        battleReports: this.battleReportHistory.map((report) => ({
          ...report,
          tacticDeltas: report.tacticDeltas ? { ...report.tacticDeltas } : undefined
        }))
      },
      warLogistics: this.exportWarLogisticsState()
    };
  }

  importGameState(snapshot: GameExportState): boolean {
    if (!snapshot || snapshot.schemaVersion !== 1) return false;

    const restoredMode = snapshot.mode;
    const restoredWarTab = snapshot.warTab ?? 'route';
    this.currentGovernanceTurn = Math.max(1, Math.floor(snapshot.governanceTurn ?? 1));
    Object.assign(this.nationState, snapshot.nationState);
    this.mode = restoredMode;
    this.sidebarCollapsed = Boolean(snapshot.sidebarCollapsed);
    this.routePickMode = snapshot.routePickMode ?? 'target';
    this.warTab = restoredWarTab;
    this.armyOrder = { ...snapshot.armyOrder };
    this.syncSidebarChrome();

    this.restoreRegionRuntimeState(snapshot.regions);
    this.restoreArmyRuntimeState(snapshot.armies);

    const selectedRegion = this.dataset.regionById.get(snapshot.selectedRegionId);
    if (selectedRegion) this.selectedRegion = selectedRegion;
    const selectedEmperor = this.dataset.emperors.find((emperor) => emperor.id === snapshot.selectedEmperorId);
    if (selectedEmperor) this.selectedEmperor = selectedEmperor;

    this.governanceQueue.splice(0, this.governanceQueue.length, ...(snapshot.queues?.governance ?? []));
    this.governanceProjects.splice(0, this.governanceProjects.length, ...(snapshot.queues?.projects ?? []).map((project) => ({
      ...project,
      logistics: { ...project.logistics }
    })));
    this.logisticsQueue.splice(0, this.logisticsQueue.length, ...(snapshot.queues?.logistics ?? []));
    this.operationLog.splice(0, this.operationLog.length, ...(snapshot.queues?.operation ?? []));
    this.occupationQueue.splice(0, this.occupationQueue.length, ...(snapshot.queues?.occupation ?? []).map((task) => ({ ...task })));
    this.commandQueue.splice(0, this.commandQueue.length, ...(snapshot.queues?.commands ?? []).map((command) => ({
      ...command,
      routeLegs: command.routeLegs.map((leg) => ({ ...leg }))
    })));
    this.battleReportHistory.splice(0, this.battleReportHistory.length, ...(snapshot.queues?.battleReports ?? []).map((report) => ({
      ...report,
      tacticDeltas: report.tacticDeltas ? { ...report.tacticDeltas } : undefined
    })));

    if (!this.importWarLogisticsState(snapshot.warLogistics)) return false;
    this.mode = restoredMode;
    this.warTab = restoredWarTab;
    if (selectedRegion) this.selectedRegion = selectedRegion;
    if (selectedEmperor) this.selectedEmperor = selectedEmperor;
    this.restoreCountersFromState();
    this.syncSidebarChrome();
    this.renderNation();
    this.renderEmperorDock();
    this.render();
    return true;
  }

  private restoreRegionRuntimeState(regions: GameExportState['regions']): void {
    for (const saved of regions ?? []) {
      const region = this.dataset.regionById.get(saved.id);
      if (!region) continue;
      region.owner = saved.owner;
      region.controlStage = saved.controlStage;
      region.specialization = saved.specialization ?? region.specialization;
      region.governanceFocus = saved.governanceFocus ?? region.governanceFocus;
      region.laborFocus = saved.laborFocus ?? region.laborFocus;
      region.integration = saved.integration;
      region.contribution = saved.contribution;
      region.risk = saved.risk;
      region.legitimacy = saved.legitimacy;
    }
  }

  private restoreArmyRuntimeState(armies: GameExportState['armies']): void {
    const savedArmyIds = new Set((armies ?? []).map((army) => army.id));
    for (let index = this.dataset.armies.length - 1; index >= 0; index -= 1) {
      const army = this.dataset.armies[index];
      if (army.id.startsWith('army_player_detached_') && !savedArmyIds.has(army.id)) {
        this.dataset.armies.splice(index, 1);
      }
    }

    for (const saved of armies ?? []) {
      const unit = this.dataset.units.find((candidate) => candidate.id === saved.unitId) ?? this.dataset.units[0];
      const existing = this.dataset.armies.find((candidate) => candidate.id === saved.id);
      const restored: ArmyViewModel = {
        id: saved.id,
        name: saved.name,
        faction: saved.faction,
        fromRegionId: saved.fromRegionId,
        targetRegionId: saved.targetRegionId,
        waypointRegionId: saved.waypointRegionId,
        soldiers: saved.soldiers,
        supply: saved.supply,
        morale: saved.morale,
        general: saved.general,
        generalId: saved.generalId,
        unit,
        unitMix: { ...saved.unitMix }
      };
      if (existing) {
        Object.assign(existing, restored);
      } else {
        this.dataset.armies.push(restored);
      }
    }
  }

  private restoreCountersFromState(): void {
    this.commandCounter = Math.max(this.commandCounter, nextCounterFromIds(this.commandQueue.map((command) => command.id), 'war_command_'));
    this.governanceProjectCounter = Math.max(this.governanceProjectCounter, nextCounterFromIds(this.governanceProjects.map((project) => project.id), 'governance_project_'));
    this.transportConvoyCounter = Math.max(this.transportConvoyCounter, nextCounterFromIds(this.transportConvoys.map((convoy) => convoy.id), '运输队-'));
    this.routeBlockadeCounter = Math.max(this.routeBlockadeCounter, nextCounterFromIds(this.routeBlockades.map((blockade) => blockade.id), '瓶颈封锁-'));
    this.occupationCounter = Math.max(this.occupationCounter, nextCounterFromIds(this.occupationQueue.map((task) => task.id), 'occupation_aftercare_'));
    this.enemyInterdictionCounter = Math.max(this.enemyInterdictionCounter, nextCounterFromIds(this.enemyInterdictionOrders.map((order) => order.id), 'enemy_interdiction_'));
    this.occupationSupplyCounter = Math.max(this.occupationSupplyCounter, nextCounterFromIds(this.occupationSupplyTasks.map((task) => task.convoyId), '安抚运输-'));
  }

  private syncSidebarChrome(): void {
    document.getElementById('sidebar')?.classList.toggle('collapsed', this.sidebarCollapsed);
    const toggle = document.getElementById('sidebar-toggle');
    if (toggle) toggle.textContent = this.sidebarCollapsed ? '‹' : '›';
  }

  private saveGameToSlot(slotId: SaveSlotId): void {
    const slot = saveSlotDefinition(slotId);
    const state = this.exportGameState();
    const envelope: GameSaveEnvelope = {
      schemaVersion: LOCAL_SAVE_SCHEMA_VERSION,
      slotId,
      savedAtIso: new Date().toISOString(),
      summary: {
        mode: state.mode,
        regionName: this.selectedRegion.definition.name,
        emperorName: this.selectedEmperor.name,
        warTurn: this.currentWarTurn,
        food: this.nationState.food,
        money: this.nationState.money,
        legitimacy: this.nationState.legitimacy
      },
      state
    };
    try {
      localStorage.setItem(saveSlotKey(slotId), JSON.stringify(envelope));
      this.saveSlotError = '';
      this.saveSlotMessage = `${slot.label} 已保存：${envelope.summary.regionName} / ${envelope.summary.emperorName}`;
      this.operationLog.unshift(`本地存档：${slot.label} 已保存，可从存档槽读回。`);
    } catch (err) {
      this.saveSlotError = `保存失败：${err instanceof Error ? err.message : String(err)}`;
      this.saveSlotMessage = `${slot.label} 保存失败`;
      this.operationLog.unshift(`本地存档：${slot.label} 保存失败。`);
    }
    trimTo(this.operationLog, 5);
  }

  private loadGameFromSlot(slotId: SaveSlotId): void {
    const slot = saveSlotDefinition(slotId);
    const raw = this.readSaveSlotRaw(slotId);
    if (!raw) {
      this.saveSlotError = `${slot.label} 为空，无法读取。`;
      this.saveSlotMessage = `${slot.label} 无存档`;
      return;
    }

    const parsed = this.parseGameSaveEnvelope(raw);
    if (!parsed.envelope) {
      this.saveSlotError = parsed.error ?? '存档无法读取。';
      this.saveSlotMessage = `${slot.label} 读取失败`;
      this.operationLog.unshift(`本地存档：${slot.label} 读取失败，${this.saveSlotError}`);
      trimTo(this.operationLog, 5);
      return;
    }

    if (!this.importGameState(parsed.envelope.state)) {
      this.saveSlotError = '游戏状态恢复失败：快照结构不完整。';
      this.saveSlotMessage = `${slot.label} 读取失败`;
      this.operationLog.unshift(`本地存档：${slot.label} 恢复失败。`);
      trimTo(this.operationLog, 5);
      return;
    }

    this.saveSlotError = '';
    this.saveSlotMessage = `${slot.label} 已读取：${parsed.envelope.summary.regionName} / ${parsed.envelope.summary.emperorName}`;
    this.operationLog.unshift(`本地存档：${slot.label} 已读取，回到第 ${parsed.envelope.summary.warTurn} 战争回合上下文。`);
    trimTo(this.operationLog, 5);
    this.events.onGameStateImported?.(parsed.envelope.state.selectedRegionId);
  }

  private deleteGameSlot(slotId: SaveSlotId): void {
    const slot = saveSlotDefinition(slotId);
    try {
      localStorage.removeItem(saveSlotKey(slotId));
      this.saveSlotError = '';
      this.saveSlotMessage = `${slot.label} 已清除`;
      this.operationLog.unshift(`本地存档：${slot.label} 已清除。`);
    } catch (err) {
      this.saveSlotError = `清除失败：${err instanceof Error ? err.message : String(err)}`;
      this.saveSlotMessage = `${slot.label} 清除失败`;
    }
    trimTo(this.operationLog, 5);
  }

  private readSaveSlotRaw(slotId: SaveSlotId): string | null {
    try {
      return localStorage.getItem(saveSlotKey(slotId));
    } catch {
      return null;
    }
  }

  private previewSaveSlot(raw: string): { title: string; detail: string; invalid?: boolean } {
    const parsed = this.parseGameSaveEnvelope(raw);
    if (!parsed.envelope) {
      return {
        title: '版本或结构异常',
        detail: parsed.error ?? '存档无法解析',
        invalid: true
      };
    }
    const { summary, savedAtIso } = parsed.envelope;
    const modeName = summary.mode === 'governance' ? '治理' : '战争';
    return {
      title: `${summary.regionName} / ${summary.emperorName}`,
      detail: `${modeName} · 战争回合 ${summary.warTurn} · ${formatSaveTime(savedAtIso)}`
    };
  }

  private parseGameSaveEnvelope(raw: string): { envelope?: GameSaveEnvelope; error?: string } {
    let parsed: unknown;
    try {
      parsed = JSON.parse(raw);
    } catch {
      return { error: '存档损坏：JSON 无法解析。' };
    }
    if (!parsed || typeof parsed !== 'object') {
      return { error: '存档损坏：内容不是对象。' };
    }

    const envelope = parsed as { schemaVersion?: unknown; state?: unknown };
    if (envelope.schemaVersion !== LOCAL_SAVE_SCHEMA_VERSION) {
      return {
        error: `存档槽版本不匹配：需要 ${LOCAL_SAVE_SCHEMA_VERSION}，实际 ${String(envelope.schemaVersion ?? '未知')}。`
      };
    }
    if (!envelope.state || typeof envelope.state !== 'object') {
      return { error: '存档损坏：缺少游戏状态。' };
    }
    const state = envelope.state as { schemaVersion?: unknown };
    if (state.schemaVersion !== GAME_STATE_SCHEMA_VERSION) {
      return {
        error: `游戏状态版本不匹配：需要 ${GAME_STATE_SCHEMA_VERSION}，实际 ${String(state.schemaVersion ?? '未知')}。`
      };
    }

    return { envelope: parsed as GameSaveEnvelope };
  }

  importWarLogisticsState(snapshot: WarLogisticsExportState): boolean {
    if (!snapshot || snapshot.schemaVersion !== 1) return false;

    this.currentWarTurn = Math.max(1, Math.floor(snapshot.turn));
    this.mode = 'war';
    this.warTab = 'logistics';
    this.selectedLogisticsObjectId = snapshot.selectedLogisticsObjectId ?? '';
    this.selectedEnemyInterdictionId = snapshot.selectedEnemyInterdictionId ?? '';
    this.restoreActiveArmyRouteContext(snapshot);

    this.routeCapacities.clear();
    for (const capacity of snapshot.routeCapacities) {
      this.routeCapacities.set(routeCapacityKey(capacity.fromRegion, capacity.toRegion), {
        ...capacity,
        routeId: routeCapacityKey(capacity.fromRegion, capacity.toRegion)
      });
    }

    this.governanceLogisticsEffects.clear();
    for (const effect of snapshot.governanceLogisticsEffects) {
      this.governanceLogisticsEffects.set(effect.regionId, { ...effect, sources: [...effect.sources] });
    }

    this.logisticsStations.splice(0, this.logisticsStations.length, ...snapshot.logisticsStations.map((station) => ({ ...station })));
    this.transportConvoys.splice(0, this.transportConvoys.length, ...snapshot.transportConvoys.map((convoy) => ({ ...convoy })));
    this.occupationSupplyTasks.splice(0, this.occupationSupplyTasks.length, ...snapshot.occupationSupplyTasks.map((task) => ({
      taskId: task.id,
      convoyId: task.convoyId,
      fromRegionId: task.fromRegionId,
      regionId: task.regionId,
      regionName: task.regionName,
      routeLabel: task.routeLabel,
      stage: task.stage,
      supplyNeeded: task.supplyNeeded,
      priority: task.priority,
      orderIndex: task.orderIndex ?? task.priority,
      bandwidthUsed: task.bandwidthUsed,
      routeUsageClaimed: task.routeUsageClaimed,
      autoDispatchTurn: task.autoDispatchTurn ?? this.currentWarTurn,
      status: task.status
    } as OccupationSupplyTask)));
    this.routeBlockades.splice(0, this.routeBlockades.length, ...snapshot.routeBlockades.map((blockade) => ({
      ...blockade,
      createdTurn: blockade.createdTurn ?? this.currentWarTurn
    })));

    this.enemyInterdictionOrders.splice(0, this.enemyInterdictionOrders.length, ...snapshot.enemyInterdiction.activeOrders.map((order) => ({
      id: order.id,
      armyId: order.armyId,
      targetRegionId: order.targetRegionId,
      targetName: this.dataset.regionById.get(order.targetRegionId)?.definition.name ?? order.targetRegionId,
      routeLabel: order.routeLabel,
      chokeRouteId: order.chokeRouteId,
      chokePointLabel: order.chokePointLabel,
      chokeNetworkLabel: order.chokeNetworkLabel,
      chokeReason: order.chokeReason,
      stage: order.stage,
      remainingTurns: order.remainingTurns,
      totalTurns: Math.max(order.remainingTurns, 3),
      risk: order.risk,
      supplyDamage: order.supplyDamage,
      createdTurn: this.currentWarTurn,
      lastCountermeasure: order.lastCountermeasure || undefined,
      resolved: false
    })));
    this.enemyInterdictionMemory.doctrine = snapshot.enemyInterdiction.doctrine;
    this.enemyInterdictionMemory.successfulRaids = snapshot.enemyInterdiction.successfulRaids;
    this.enemyInterdictionMemory.failedRaids = snapshot.enemyInterdiction.failedRaids;
    this.enemyInterdictionMemory.lastTargetRegionId = snapshot.enemyInterdiction.lastTargetRegionId ?? '';
    this.enemyInterdictionMemory.lastReasoning = snapshot.enemyInterdiction.lastReasoning;
    this.enemyInterdictionMemory.pressureByRegion = { ...snapshot.enemyInterdiction.pressureByRegion };

    this.lastCountermeasureSummary = '';
    this.lastResolvedRoutePressure = null;
    this.refreshInterceptionAlert();
    this.events.onStateMutated();
    this.render();
    return true;
  }

  private restoreActiveArmyRouteContext(snapshot: WarLogisticsExportState): void {
    const restoredArmy = this.dataset.armies.find((army) => army.id === snapshot.activeArmyId);
    if (restoredArmy) this.activeArmy = restoredArmy;

    const activeConvoy = snapshot.transportConvoys.find((convoy) => convoy.armyId === this.activeArmy.id && convoy.status !== 'cancelled' && convoy.status !== 'delivered');
    const targetRegionId = snapshot.activeArmyTargetId || activeConvoy?.targetRegionId || this.activeArmy.targetRegionId;
    if (targetRegionId && this.dataset.regionById.has(targetRegionId)) {
      this.activeArmy.targetRegionId = targetRegionId;
      const targetRegion = this.dataset.regionById.get(targetRegionId);
      if (targetRegion) this.selectedRegion = targetRegion;
    }

    const selectedAlternative = snapshot.routeAlternatives?.find((alternative) => alternative.selected);
    const waypointRegionId = snapshot.activeArmyWaypointId || selectedAlternative?.waypointRegionId || activeConvoy?.routeLegs?.[0]?.toRegionId || '';
    if (
      waypointRegionId &&
      this.dataset.regionById.has(waypointRegionId) &&
      waypointRegionId !== this.activeArmy.fromRegionId &&
      waypointRegionId !== this.activeArmy.targetRegionId
    ) {
      this.activeArmy.waypointRegionId = waypointRegionId;
    } else {
      this.activeArmy.waypointRegionId = undefined;
    }
  }

  getEnemyInterdictionTargets(): Array<{
    id: string;
    armyId: string;
    regionId: string;
    regionName: string;
    routeLabel: string;
    stage: EnemyInterdictionStage;
    remainingTurns: number;
    risk: number;
    supplyDamage: number;
    lastCountermeasure?: string;
  }> {
    return this.enemyInterdictionOrders.map((order) => ({
      id: order.id,
      armyId: order.armyId,
      regionId: order.targetRegionId,
      regionName: order.targetName,
      routeLabel: order.routeLabel,
      stage: order.stage,
      remainingTurns: order.remainingTurns,
      risk: order.risk,
      supplyDamage: order.supplyDamage,
      lastCountermeasure: order.lastCountermeasure
    }));
  }

  getRoutePickMode(): 'target' | 'waypoint' {
    return this.routePickMode;
  }

  selectEnemyInterdictionTarget(orderId: string): boolean {
    const order = this.enemyInterdictionOrders.find((candidate) => candidate.id === orderId);
    if (!order) return false;
    this.selectedEnemyInterdictionId = order.id;
    this.warTab = 'logistics';
    this.operationLog.unshift(`地图截粮：选中 ${order.routeLabel}，风险 ${order.risk}%，预计损耗 ${order.supplyDamage}`);
    trimTo(this.operationLog, 5);
    this.render();
    return true;
  }

  getLogisticsMapObjects(): LogisticsMapObject[] {
    const objects: LogisticsMapObject[] = [];

    for (const convoy of this.activeTransportConvoys()) {
      objects.push({
        id: convoy.id,
        kind: 'transport-convoy',
        regionId: convoy.targetRegionId,
        fromRegionId: convoy.fromRegionId,
        targetRegionId: convoy.targetRegionId,
        label: convoy.id,
        routeLabel: convoy.routeLabel,
        status: transportConvoyStatusName(convoy.status),
        progress: convoyMapProgress(convoy),
        priority: convoy.priority,
        details: describeTransportConvoy(convoy)
      });
    }

    for (const task of this.occupationSupplyTasks.filter((candidate) => candidate.status !== 'delivered' && candidate.status !== 'cancelled')) {
      objects.push({
        id: task.convoyId,
        kind: 'occupation-supply',
        regionId: task.regionId,
        fromRegionId: task.fromRegionId,
        targetRegionId: task.regionId,
        label: task.convoyId,
        routeLabel: task.routeLabel,
        status: occupationSupplyStatusName(task.status),
        progress: occupationSupplyMapProgress(task),
        priority: task.priority,
        details: describeOccupationSupplyTask(task)
      });
    }

    for (const blockade of this.activeRouteBlockades()) {
      objects.push({
        id: blockade.id,
        kind: 'route-blockade',
        regionId: blockade.toRegionId,
        fromRegionId: blockade.fromRegionId,
        targetRegionId: blockade.toRegionId,
        label: blockade.status === 'guarded' ? `${blockade.networkLabel || '瓶颈'}守备` : `${blockade.networkLabel || '瓶颈'}封锁`,
        routeLabel: blockade.chokePointLabel,
        status: routeBlockadeStatusName(blockade.status),
        progress: blockade.status === 'guarded' ? 0.38 : 0.34,
        priority: blockade.status === 'enemy-blockade' ? 7 : 6,
        details: describeRouteBlockade(blockade)
      });
    }

    for (const station of this.logisticsStations.filter((candidate) => candidate.isActive)) {
      objects.push({
        id: station.id,
        kind: 'logistics-station',
        regionId: station.regionId,
        label: `${station.regionName}兵站`,
        routeLabel: station.regionName,
        status: '已启用',
        progress: 1,
        priority: 4,
        details: `${station.regionName}兵站：路线容量 +${station.supplyBonus}，军心 +${station.moraleBonus}，地方风险 -${station.riskReduction}`
      });
    }

    return objects.sort((a, b) => b.priority - a.priority || logisticsObjectRank(a.kind) - logisticsObjectRank(b.kind));
  }

  selectLogisticsMapObject(objectId: string): boolean {
    const object = this.getLogisticsMapObjects().find((candidate) => candidate.id === objectId);
    if (!object) return false;
    this.selectedLogisticsObjectId = object.id;
    this.warTab = 'logistics';
    this.operationLog.unshift(`地图后勤：选中 ${object.label}，${object.details}`);
    trimTo(this.operationLog, 5);
    this.render();
    return true;
  }

  private bindControls(): void {
    document.getElementById('governance-mode')?.addEventListener('click', () => {
      this.events.onModeChange('governance');
    });
    document.getElementById('war-mode')?.addEventListener('click', () => {
      this.events.onModeChange('war');
    });
    document.getElementById('sidebar-toggle')?.addEventListener('click', () => {
      this.sidebarCollapsed = !this.sidebarCollapsed;
      this.syncSidebarChrome();
    });

    document.addEventListener('click', (event) => {
      const target = event.target;
      if (!(target instanceof HTMLElement)) return;
      const button = target.closest<HTMLButtonElement>('[data-action]');
      if (!button) return;
      const action = button.dataset.action as UiAction | undefined;
      if (!action) return;
      try {
        this.applyAction(action);
        this.render();
        this.events.onStateMutated();
      } catch (err) {
        console.error('[applyAction error]', action, err);
      }
    });

    document.addEventListener('click', (event) => {
      const target = event.target;
      if (!(target instanceof HTMLElement)) return;
      const button = target.closest<HTMLButtonElement>('[data-emperor-id]');
      if (!button) return;
      const emperor = this.dataset.emperors.find((candidate) => candidate.id === button.dataset.emperorId);
      if (!emperor) return;
      this.selectedEmperor = emperor;
      this.applyEmperorState(emperor);
      this.renderEmperorDock();
      this.render();
      this.events.onEmperorChange(emperor);
      this.events.onStateMutated();
    });

    document.addEventListener('click', (event) => {
      const target = event.target;
      if (!(target instanceof HTMLElement)) return;
      const button = target.closest<HTMLButtonElement>('[data-army-id]');
      if (!button) return;
      const army = this.dataset.armies.find((candidate) => candidate.id === button.dataset.armyId);
      if (!army || army.faction !== 'player') return;
      this.activeArmy = army;
      this.routePickMode = 'target';
      const targetRegion = this.dataset.regionById.get(army.targetRegionId);
      if (targetRegion) this.selectedRegion = targetRegion;
      this.armyOrder = {
        stance: '稳进',
        formation: '中军合进',
        route: '主道补给',
        last: '待命'
      };
      this.events.onArmyChange(army.id);
      this.events.onStateMutated();
      this.render();
    });

    document.addEventListener('click', (event) => {
      const target = event.target;
      if (!(target instanceof HTMLElement)) return;
      const button = target.closest<HTMLButtonElement>('[data-route-mode]');
      if (!button) return;
      const mode = button.dataset.routeMode as 'target' | 'waypoint' | undefined;
      if (!mode) return;
      this.routePickMode = mode;
      document.querySelectorAll<HTMLButtonElement>('[data-route-mode]').forEach((node) => {
        node.classList.toggle('active', node.dataset.routeMode === mode);
      });
      this.render();
    });

    document.addEventListener('click', (event) => {
      const target = event.target;
      if (!(target instanceof HTMLElement)) return;
      const button = target.closest<HTMLButtonElement>('[data-route-alternative-id]');
      if (!button) return;
      const alternativeId = button.dataset.routeAlternativeId;
      if (!alternativeId) return;
      this.applyRouteAlternative(alternativeId);
      this.events.onStateMutated();
      this.render();
    });

    document.addEventListener('click', (event) => {
      const target = event.target;
      if (!(target instanceof HTMLElement)) return;
      const button = target.closest<HTMLButtonElement>('[data-war-tab]');
      if (!button) return;
      const tab = button.dataset.warTab as 'route' | 'army' | 'logistics' | 'report' | undefined;
      if (!tab) return;
      this.warTab = tab;
      document.querySelectorAll<HTMLButtonElement>('[data-war-tab]').forEach((node) => {
        node.classList.toggle('active', node.dataset.warTab === tab);
      });
      this.render();
    });

    document.addEventListener('click', (event) => {
      const target = event.target;
      if (!(target instanceof HTMLElement)) return;
      const button = target.closest<HTMLButtonElement>('[data-enemy-interdiction-id]');
      if (!button) return;
      const orderId = button.dataset.enemyInterdictionId;
      if (!orderId || !this.selectEnemyInterdictionTarget(orderId)) return;
      this.events.onSelectEnemyInterdiction?.(orderId);
      this.events.onStateMutated();
    });
  }

  private renderNation(): void {
    setText('metric-food', String(this.nationState.food));
    setText('metric-money', String(this.nationState.money));
    setText('metric-army', formatNumber(this.nationState.army));
    setText('metric-legitimacy', String(this.nationState.legitimacy));
  }

  private render(): void {
    const region = this.selectedRegion;
    setText('selected-name', region.definition.name);
    setText('selected-tags', `${region.specialization} / ${terrainName(region.definition.terrain)} / ${ownerName(region.owner)}`);
    setText(
      'selection-summary',
      `${this.mode === 'governance' ? '治理' : '战争'}：${region.definition.name}，${region.specialization}，风险 ${Math.round(region.risk)}`
    );

    this.renderGovernance(region);
    this.renderWar(region);
    this.renderSavePanel();
    this.renderOutliner();
    this.renderNation();
    this.renderEmperorDock();
  }

  private renderEmperorDock(): void {
    const dock = document.getElementById('emperor-dock');
    if (!dock) return;
    const emperor = this.selectedEmperor;
    const stats = emperor.stats;
    const candidates = this.dataset.emperors.slice(0, 8);
    dock.innerHTML = `
      <div class="emperor-card">
        <div class="emperor-head">
          <span>帝皇</span>
          <b>${escapeHtml(emperor.name)} · ${escapeHtml(emperor.title)}</b>
        </div>
        <div class="emperor-mechanic">${escapeHtml(emperor.uniqueMechanic.name)}：${escapeHtml(emperor.uniqueMechanic.description)}</div>
        <div class="emperor-stat-row">
          ${compactStat('军', stats.military)}
          ${compactStat('政', stats.administration)}
          ${compactStat('改', stats.reform)}
          ${compactStat('魅', stats.charisma)}
        </div>
        <div class="source-line">${escapeHtml(emperor.sourceReference)}</div>
      </div>
      <div class="emperor-list">
        ${candidates
          .map((candidate) => `
            <button class="emperor-chip${candidate.id === emperor.id ? ' active' : ''}" type="button" data-emperor-id="${escapeHtml(candidate.id)}">
              <span>${escapeHtml(candidate.title)}</span>
              <b>${escapeHtml(candidate.name)}</b>
            </button>
          `)
          .join('')}
      </div>
    `;
  }

  private renderSavePanel(): void {
    const panel = document.getElementById('save-panel');
    if (!panel) return;
    const rows = SAVE_SLOTS.map((slot) => {
      const raw = this.readSaveSlotRaw(slot.id);
      const preview = raw ? this.previewSaveSlot(raw) : null;
      const title = preview?.title ?? '空槽';
      const detail = preview?.detail ?? '暂无本地存档';
      const invalid = Boolean(preview?.invalid);
      const actionsDisabled = raw ? '' : ' disabled';
      return `
        <div class="save-slot${invalid ? ' invalid' : ''}" data-save-slot="${escapeHtml(slot.id)}">
          <div>
            <span>${escapeHtml(slot.label)}</span>
            <b>${escapeHtml(title)}</b>
            <em>${escapeHtml(detail)}</em>
          </div>
          <div class="save-actions">
            <button type="button" data-action="${escapeHtml(saveSlotAction('save', slot.id))}">存</button>
            <button type="button" data-action="${escapeHtml(saveSlotAction('load', slot.id))}"${actionsDisabled}>读</button>
            <button type="button" data-action="${escapeHtml(saveSlotAction('delete', slot.id))}"${actionsDisabled}>删</button>
          </div>
        </div>
      `;
    }).join('');
    panel.innerHTML = `
      <section class="save-card">
        <div class="save-card-head">
          <b>本地存档</b>
          <span>${escapeHtml(this.saveSlotMessage)}</span>
        </div>
        ${this.saveSlotError ? `<div class="save-error" role="alert">${escapeHtml(this.saveSlotError)}</div>` : ''}
        <div class="save-slot-list">${rows}</div>
      </section>
    `;
  }

  private renderGovernance(region: RegionViewModel): void {
    const panel = document.getElementById('governance-panel');
    if (!panel) return;
    const policy = region.recommendedPolicy;
    const building = region.recommendedBuilding;
    const policyEffects = policy ? formatEffects(policy.effects) : '无';
    const policyRisks = policy ? formatEffects(policy.risks) : '无';
    const buildingEffects = building ? formatEffects(building.effects) : '无';
    const policyName = escapeHtml(policy?.name ?? '地方安抚');
    const policySource = escapeHtml(policy?.sourceReference ?? '史据待补');
    const buildingName = escapeHtml(building?.name ?? '基础仓储');
    const buildingCategory = escapeHtml(building?.category ?? 'agriculture');
    const buildingSource = escapeHtml(building?.sourceReference ?? '史据待补');
    const policyLogisticsPreview = escapeHtml(formatGovernanceLogisticsDelta(governanceLogisticsDeltaFromPolicy(policy)));
    const buildingLogisticsPreview = escapeHtml(formatGovernanceLogisticsDelta(governanceLogisticsDeltaFromBuilding(building)));
    const regionLogisticsSummary = escapeHtml(this.regionGovernanceLogisticsSummary(region));
    const sourceText = escapeHtml(region.sourceText || region.history?.uiSummary || '当前地区来源说明待补。');
    const governanceFocusPlans = GOVERNANCE_FOCUS_IDS.map((focusId) => this.governanceFocusPlan(focusId, region));
    const activeFocusPlan = this.governanceFocusPlan(region.governanceFocus, region);
    const laborPlans = GOVERNANCE_LABOR_IDS.map((laborId) => this.governanceLaborPlan(laborId, region));
    const activeLaborPlan = this.governanceLaborPlan(region.laborFocus, region);
    const geographyTags = formatTags([
      region.geography.label,
      climateName(region.history?.climateZone),
      ...region.geography.sourceTags.slice(0, 3),
      ...region.geography.resources.slice(0, 2)
    ]);

    panel.innerHTML = `
      <section class="info-band metrics-grid" data-testid="governance-metrics">
        ${metric('粮食', region.definition.foodOutput)}
        ${metric('财税', region.definition.taxOutput)}
        ${metric('人口', formatNumber(region.definition.population))}
        ${metric('兵源', region.definition.manpower)}
        ${metric('民变', `${Math.round(region.risk)}%`)}
        ${metric('法统', `${Math.round(region.legitimacy)}%`)}
      </section>
      <section class="decision-card" data-testid="governance-actions">
        <div class="band-title">治理操作</div>
        <div class="command-tile-grid">
          <button class="command-tile primary" type="button" data-action="governance_policy">
            <span>施政</span><b>${policyName}</b>
          </button>
          <button class="command-tile" type="button" data-action="governance_build">
            <span>建设</span><b>开工建设</b>
          </button>
          <button class="command-tile" type="button" data-action="governance_relief">
            <span>民生</span><b>赈济安抚</b>
          </button>
          <button class="command-tile" type="button" data-action="governance_registry">
            <span>户籍</span><b>编户清丈</b>
          </button>
          <button class="command-tile warning" type="button" data-action="governance_reinforce">
            <span>军政</span><b>征发整备</b>
          </button>
        </div>
        <div class="preview-line">收益：${policyEffects}</div>
        <div class="preview-line" data-testid="policy-logistics-preview">后勤：${policyLogisticsPreview}</div>
        <div class="preview-line risk">副作用：${policyRisks}</div>
        <div class="source-line">${policySource}</div>
      </section>
      <section class="decision-card" data-testid="governance-micro">
        <div class="band-title">区域微操</div>
        <div class="micro-current">
          <span>当前路线</span>
          <b>${escapeHtml(activeFocusPlan.specialization)}</b>
          <em>${escapeHtml(activeFocusPlan.description)}</em>
        </div>
        <div class="micro-focus-grid">
          ${governanceFocusPlans.map((plan) => this.renderGovernanceFocusButton(plan, region)).join('')}
        </div>
        <div class="preview-line">预期：${escapeHtml(formatGovernanceFocusDelta(activeFocusPlan.delta))}</div>
        <div class="preview-line risk">代价：粮 ${activeFocusPlan.costs.food} / 钱 ${activeFocusPlan.costs.money}；副作用已计入风险与法统变化。</div>
        <div class="source-line">史据：${escapeHtml(activeFocusPlan.source)}</div>
      </section>
      <section class="decision-card" data-testid="governance-labor">
        <div class="band-title">劳力分配</div>
        <div class="micro-current">
          <span>当前劳力</span>
          <b>${escapeHtml(activeLaborPlan.label)}</b>
          <em>${escapeHtml(activeLaborPlan.description)}</em>
        </div>
        <div class="micro-labor-grid">
          ${laborPlans.map((plan) => this.renderGovernanceLaborButton(plan, region)).join('')}
        </div>
        <div class="preview-line">每次推进：${escapeHtml(formatGovernanceLaborDelta(activeLaborPlan.delta))}</div>
        <button class="command-tile primary governance-turn-button" type="button" data-action="governance_advance_turn">
          <span>内政</span><b>推进一旬</b>
        </button>
      </section>
      <section class="info-band" data-testid="governance-projects">
        <div class="band-title">工程队列</div>
        ${this.renderGovernanceProjects(region)}
      </section>
      <section class="info-band" data-testid="geography-building">
        <div class="band-title">地理与建设</div>
        <div class="tag-row">${geographyTags}</div>
        <div class="preview-line" data-testid="real-geography">现实地貌：${escapeHtml(region.geography.label)} / ${escapeHtml(region.geography.kind)}</div>
        <div class="preview-line">地图层：地貌符号 + ${buildingName} 建设标记</div>
        <div class="preview-line">地理影响：${escapeHtml(region.geography.description)}</div>
        <div class="preview-line" data-testid="governance-logistics-effect">治理后勤：${regionLogisticsSummary}</div>
      </section>
      <section class="info-band">
        <div class="band-title">治理判断</div>
        <div class="meter-row">${meter('整合', region.integration)}</div>
        <div class="meter-row">${meter('贡献', region.contribution)}</div>
        <div class="meter-row">${meter('地方势力', region.definition.localPower)}</div>
      </section>
      <section class="info-band">
        <div class="band-title">建设方向</div>
        <div class="preview-line">${buildingName} / ${buildingCategory}</div>
        <div class="preview-line">收益：${buildingEffects}</div>
        <div class="preview-line" data-testid="building-logistics-preview">后勤：${buildingLogisticsPreview}</div>
        <div class="source-line">${buildingSource}</div>
      </section>
      <section class="info-band" data-testid="governance-queue">
        <div class="band-title">经营队列</div>
        ${queueLines(this.governanceQueue, '暂无经营队列')}
      </section>
      <section class="info-band source-band">
        <div class="band-title">历史来源</div>
        <p>${sourceText}</p>
      </section>
    `;
  }

  private renderGovernanceFocusButton(plan: GovernanceFocusPlan, region: RegionViewModel): string {
    const active = plan.id === region.governanceFocus;
    return `
      <button class="micro-focus${active ? ' active' : ''}" type="button" data-action="${escapeHtml(governanceFocusAction(plan.id))}">
        <span>${escapeHtml(plan.label)}</span>
        <b>${escapeHtml(plan.actionLabel)}</b>
        <em>${escapeHtml(formatGovernanceFocusDelta(plan.delta))}</em>
      </button>
    `;
  }

  private renderGovernanceLaborButton(plan: GovernanceLaborPlan, region: RegionViewModel): string {
    const active = plan.id === region.laborFocus;
    return `
      <button class="micro-focus labor${active ? ' active' : ''}" type="button" data-action="${escapeHtml(governanceLaborAction(plan.id))}">
        <span>${escapeHtml(plan.label)}</span>
        <b>${escapeHtml(plan.actionLabel)}</b>
        <em>${escapeHtml(formatGovernanceLaborDelta(plan.delta))}</em>
      </button>
    `;
  }

  private renderGovernanceProjects(region: RegionViewModel): string {
    const projects = this.governanceProjects.filter((project) => project.regionId === region.definition.id);
    if (projects.length === 0) {
      return `<div class="preview-line muted">暂无在建工程，开工建设会进入多回合队列。</div>`;
    }
    return `
      <div class="project-list">
        ${projects.map((project) => `
          <div class="project-row">
            <span>${escapeHtml(project.buildingName)}</span>
            <b>${project.remainingTurns}/${project.totalTurns} 旬</b>
            <em>${escapeHtml(formatGovernanceProjectYield(project))}</em>
          </div>
        `).join('')}
      </div>
    `;
  }

  private renderWar(region: RegionViewModel): void {
    const panel = document.getElementById('war-panel');
    if (!panel) return;
    const targetRoute = this.createActiveRouteForecast(region);
    const routeTargetName = targetRoute.target.definition.name;
    const waypointName = targetRoute.waypoint ? ` → ${targetRoute.waypoint.definition.name}` : '';
    const routeLine = `${targetRoute.army.name}：${targetRoute.from.definition.name}${waypointName} → ${routeTargetName}`;
    const routeRoad = this.routeTerrainProfile(targetRoute.from, targetRoute.target);
    const routeCapacityPreview = targetRoute.routeCapacity ?? this.routeCapacityLimit(targetRoute.from.definition.id, targetRoute.target.definition.id);
    const routeUsagePreview = targetRoute.routeUsage ?? this.routeCapacities.get(routeCapacityKey(targetRoute.from.definition.id, targetRoute.target.definition.id))?.currentUsage ?? 0;
    const routeBottleneckLabel = targetRoute.bottleneckLabel ?? routeRoad.bottleneckLabel;
    const routeTerrainReason = targetRoute.terrainReason ?? routeRoad.terrainReason;
    const contact = Math.round(targetRoute.contactChance);
    const occupation = targetRoute.occupationCost;
    const supply = targetRoute.supplyCost;
    const safeRouteLine = escapeHtml(routeLine);
    const safeGeneral = escapeHtml(targetRoute.army.general);
    const safeTargetSummary = escapeHtml(targetRoute.target.history?.uiSummary ?? '目标地区历史层数据待补。');
    const army = this.activeArmy;
    const general = this.currentGeneral();
    const nextGeneral = this.nextGeneral();
    const unitMix = unitMixText(army.unitMix, this.dataset.units);
    const tacticSummary = this.tacticalSummary(targetRoute.target);
    const commandLines = this.commandQueue.map((command) => describeWarCommand(command));
    const commandBadgeLines = this.commandQueue.map((command) => formatWarCommandWithBadge(command));
    const occupationLines = this.occupationQueue.map((task) => describeOccupationTask(task));
    const enemyInterdictionLines = this.enemyInterdictionOrders.map((order) => describeEnemyInterdictionOrder(order));
    const currentEnemyInterdiction = this.activeEnemyInterdictionOrder();
    const activeAlert = this.latestInterceptionAlert || '暂无截粮警报';
    const routePressure = this.currentRoutePressureCopy();
    const routePressureSummary = routePressure.full;
    const routeAlternatives = this.routeAlternativesFor(region);

    this.renderRoutePressure(routePressure);

    panel.innerHTML = `
      <div class="war-tab-bar" data-testid="war-tabs">
        <button class="${this.warTab === 'route' ? 'active' : ''}" type="button" data-war-tab="route">路线</button>
        <button class="${this.warTab === 'army' ? 'active' : ''}" type="button" data-war-tab="army">编组</button>
        <button class="${this.warTab === 'logistics' ? 'active' : ''}" type="button" data-war-tab="logistics">后勤</button>
        <button class="${this.warTab === 'report' ? 'active' : ''}" type="button" data-war-tab="report">战报</button>
      </div>
      <section class="info-band war-tab-panel${this.warTab === 'route' ? '' : ' hidden'}" data-testid="army-selector">
        <div class="band-title">军队选择</div>
        <div class="army-selector-grid">
          ${this.dataset.armies
            .filter((armyCandidate) => armyCandidate.faction === 'player')
            .map((armyCandidate) => `
              <button class="army-card${armyCandidate.id === this.activeArmy.id ? ' active' : ''}" type="button" data-army-id="${escapeHtml(armyCandidate.id)}">
                <span>${escapeHtml(armyCandidate.name)}</span>
                <b>${formatNumber(armyCandidate.soldiers)} / 补给 ${Math.round(armyCandidate.supply)}%</b>
              </button>
            `)
            .join('')}
        </div>
      </section>
      <section class="info-band war-tab-panel${this.warTab === 'route' ? '' : ' hidden'}" data-testid="route-plan">
        <div class="band-title">路线编辑</div>
        <div class="route-edit-status">
          <span>模式 <b>${this.routePickMode === 'target' ? '目标' : '中继'}</b></span>
          <span>中继 <b>${this.activeArmy.waypointRegionId ? escapeHtml(this.dataset.regionById.get(this.activeArmy.waypointRegionId)?.definition.name ?? '未设定') : '未设定'}</b></span>
        </div>
        <div class="command-tile-grid micro-grid">
          <button class="command-tile primary${this.routePickMode === 'target' ? ' active' : ''}" type="button" data-route-mode="target">
            <span>点选</span><b>改为目标</b>
          </button>
          <button class="command-tile${this.routePickMode === 'waypoint' ? ' active' : ''}" type="button" data-route-mode="waypoint">
            <span>点选</span><b>设为中继</b>
          </button>
          <button class="command-tile" type="button" data-action="route_waypoint_clear">
            <span>清除</span><b>移除中继</b>
          </button>
          <button class="command-tile" type="button" data-action="route_queue_promote">
            <span>队列</span><b>上移后勤</b>
          </button>
          <button class="command-tile" type="button" data-action="route_queue_cancel">
            <span>队列</span><b>撤销后勤</b>
          </button>
        </div>
        <div class="preview-line">当前模式下点击地图会直接改为${this.routePickMode === 'target' ? '目标' : '中继'}；选中“设为中继”后可绕行再进攻。</div>
        <div class="preview-line">地图提示：黄铜车队是运输分段，赤色加粗段是截粮高危段，目标与中继上方的立标可拖拽。</div>
      </section>
      <section class="decision-card war-tab-panel${this.warTab === 'army' ? '' : ' hidden'}" data-testid="army-micro">
        <div class="band-title">军队微操</div>
        <div class="army-status">
          <div><span>军队</span><b>${escapeHtml(army.name)}</b></div>
          <div><span>军令</span><b>${escapeHtml(this.armyOrder.last)}</b></div>
          <div><span>阵型</span><b>${escapeHtml(this.armyOrder.formation)}</b></div>
          <div><span>路线</span><b>${escapeHtml(this.armyOrder.route)}</b></div>
        </div>
        <div class="metrics-grid army-metrics">
          ${metric('兵力', formatNumber(army.soldiers))}
          ${metric('补给', `${Math.round(army.supply)}%`)}
          ${metric('军心', `${Math.round(army.morale)}%`)}
        </div>
        <div class="command-tile-grid micro-grid">
          <button class="command-tile primary" type="button" data-action="army_order_balanced">
            <span>军令</span><b>稳进压迫</b>
          </button>
          <button class="command-tile warning" type="button" data-action="army_order_forced_march">
            <span>速度</span><b>急行抢道</b>
          </button>
          <button class="command-tile" type="button" data-action="army_order_defensive">
            <span>阵地</span><b>扎营固守</b>
          </button>
          <button class="command-tile" type="button" data-action="army_order_flank">
            <span>机动</span><b>侧翼牵制</b>
          </button>
          <button class="command-tile" type="button" data-action="army_order_reserve">
            <span>预备</span><b>收拢预备队</b>
          </button>
        </div>
        <div class="preview-line">目标：${escapeHtml(routeTargetName)}；选中不同地区会改变战役目标预案。</div>
        <div class="preview-line risk">急行会耗补给，扎营会慢但稳，侧翼会提高接敌压力。</div>
      </section>
      <section class="info-band war-tab-panel${this.warTab === 'army' ? '' : ' hidden'}" data-testid="army-organization">
        <div class="band-title">编组与将领</div>
        <div class="army-status">
          <div><span>主将</span><b>${escapeHtml(army.general)}</b></div>
          <div><span>特长</span><b>${escapeHtml(general?.specialAbilityName ?? '常规统率')}</b></div>
          <div><span>兵种</span><b>${escapeHtml(army.unit.name)}</b></div>
          <div><span>配比</span><b>${escapeHtml(unitMix)}</b></div>
        </div>
        <div class="unit-mix-bars">
          ${unitMixBars(army.unitMix, this.dataset.units)}
        </div>
        <div class="command-tile-grid micro-grid">
          <button class="command-tile primary" type="button" data-action="army_split">
            <span>兵力</span><b>拆分偏师</b>
          </button>
          <button class="command-tile" type="button" data-action="army_merge">
            <span>兵力</span><b>合并同源军</b>
          </button>
          <button class="command-tile" type="button" data-action="army_general_next">
            <span>换将</span><b>${escapeHtml(nextGeneral?.name ?? '轮换主将')}</b>
          </button>
          <button class="command-tile" type="button" data-action="army_mix_balanced">
            <span>配比</span><b>步骑均衡</b>
          </button>
          <button class="command-tile" type="button" data-action="army_mix_cavalry">
            <span>配比</span><b>骑兵突进</b>
          </button>
          <button class="command-tile" type="button" data-action="army_mix_crossbow">
            <span>配比</span><b>弩步守正</b>
          </button>
          <button class="command-tile" type="button" data-action="army_mix_siege">
            <span>配比</span><b>攻城配属</b>
          </button>
        </div>
        <div class="preview-line">换将会影响军心；兵种配比会改变主兵种、补给压力与接敌侧重。</div>
        <div class="source-line">${escapeHtml(general?.sourceReference ?? '将领史据待补')}</div>
      </section>
      <section class="info-band war-tab-panel${this.warTab === 'route' ? '' : ' hidden'}" data-testid="war-route">
        <div class="band-title">行军线</div>
        <div class="route-line">${safeRouteLine}</div>
        <div class="preview-line">预计 ${targetRoute.turns} 回合抵达，军队 ${formatNumber(targetRoute.army.soldiers)}，主将 ${safeGeneral}</div>
      </section>
      <section class="info-band metrics-grid war-tab-panel${this.warTab === 'route' ? '' : ' hidden'}" data-testid="war-forecast">
        ${metric('补给消耗', supply)}
        ${metric('接敌概率', `${contact}%`)}
        ${metric('截粮风险', `${targetRoute.interceptionRisk}%`)}
        ${metric('占领代价', occupation)}
        ${metric('军心', `${targetRoute.army.morale}%`)}
        ${metric('兵种', targetRoute.army.unit.name)}
        ${metric('战术修正', tacticSummary.score)}
        ${metric('路况', routeBottleneckLabel)}
        ${metric('容量', `${routeCapacityPreview}队`)}
      </section>
      <section class="info-band war-tab-panel${this.warTab === 'route' ? '' : ' hidden'}" data-testid="prewar-route-capacity">
        <div class="band-title">战前路线预案</div>
        <div class="preview-line">路线容量 ${routeUsagePreview}/${routeCapacityPreview}，预计补给消耗 ${supply}，截粮风险 ${targetRoute.interceptionRisk}%。</div>
        <div class="preview-line">${escapeHtml(routeBottleneckLabel)}：${escapeHtml(routeTerrainReason)}</div>
      </section>
      ${this.renderRouteAlternatives(routeAlternatives)}
      <section class="decision-card war-tab-panel${this.warTab === 'logistics' ? '' : ' hidden'}">
        <div class="band-title">部署操作</div>
        <div class="war-turn-strip" data-testid="war-turn">
          <span>战时回合 <b>第 ${this.currentWarTurn} 回合</b></span>
          <span>排程 <b>${this.commandQueue.length} 项</b></span>
        </div>
        <div class="command-alert${this.latestInterceptionAlert ? '' : ' muted'}" data-testid="interception-alert">${escapeHtml(activeAlert)}</div>
        <div class="command-tile-grid war-grid">
          <button class="command-tile primary" type="button" data-action="war_deploy">
            <span>前线</span><b>部署军府</b>
          </button>
          <button class="command-tile" type="button" data-action="war_supply">
            <span>后勤</span><b>派运输队</b>
          </button>
          <button class="command-tile" type="button" data-action="war_scout">
            <span>侦察</span><b>探路线</b>
          </button>
          <button class="command-tile" type="button" data-action="war_fortify">
            <span>布防</span><b>固兵站</b>
          </button>
          <button class="command-tile danger" type="button" data-action="war_attack">
            <span>攻占</span><b>启动战役</b>
          </button>
          <button class="command-tile warning" type="button" data-action="war_advance_turn">
            <span>回合</span><b>推进一回合</b>
          </button>
          <button class="command-tile" type="button" data-action="occupation_aftercare">
            <span>占后</span><b>推进安抚</b>
          </button>
        </div>
        <div class="preview-line">路线必须依赖己方邻接区和连续补给节点。</div>
        <div class="preview-line risk">攻占后进入军管、安抚、编户队列，不能立刻完整贡献。</div>
        <div class="source-line">史据：《汉书·食货志》《唐六典·兵部》</div>
      </section>
      <section class="info-band war-tab-panel${this.warTab === 'logistics' ? '' : ' hidden'}" data-testid="occupation-aftercare">
        <div class="band-title">占后安抚队列</div>
        <div class="preview-line">地图徽标：红为军管，黄为安抚，青为编户；完成后徽标消失并转入常规治理。</div>
        ${queueLines(occupationLines, '暂无占后安抚队列')}
      </section>
      <section class="info-band war-tab-panel${this.warTab === 'logistics' ? '' : ' hidden'}" data-testid="logistics-dispatcher">
        <div class="band-title">后勤调度中心</div>
        <div class="logistics-dashboard">
          <div class="logistics-stats-row">
            <div class="logistics-stat">
              <span class="stat-label">兵站</span>
              <span class="stat-value">${this.logisticsStations.length}</span>
            </div>
            <div class="logistics-stat">
              <span class="stat-label">拥堵路线</span>
              <span class="stat-value">${[...this.routeCapacities.values()].filter(c => c.congestionLevel === 'high' || c.congestionLevel === 'critical').length}</span>
            </div>
            <div class="logistics-stat">
              <span class="stat-label">截粮威胁</span>
              <span class="stat-value badge badge-danger">${this.enemyInterdictionOrders.filter(o => !o.resolved).length}</span>
            </div>
          <div class="logistics-stat">
            <span class="stat-label">待运输</span>
            <span class="stat-value">${this.occupationSupplyTasks.filter(t => t.status === 'pending').length}</span>
          </div>
        </div>
          <div class="preview-line" data-testid="interdiction-memory">敌军意图：${escapeHtml(this.enemyInterdictionDoctrineName())} / ${escapeHtml(this.enemyInterdictionMemorySummary())}</div>
          ${this.renderSelectedLogisticsObject()}
          ${this.renderSelectedEnemyInterdiction()}
          ${this.renderGovernanceLogisticsEffects()}
          ${this.renderRouteCapacities()}
          ${this.renderTransportConvoys()}
          ${this.renderLogisticsPriority()}
          ${this.renderOccupationSupplyTasks()}
        </div>
      </section>
      <section class="info-band war-tab-panel${this.warTab === 'logistics' ? '' : ' hidden'}" data-testid="enemy-interdiction-orders">
        <div class="band-title">敌方截粮命令</div>
        <div class="preview-line">地图威胁标记：暗红为筹划，橙红为机动，亮红为袭扰；用于提前调整运输和兵站。</div>
        <div class="command-tile-grid micro-grid" data-testid="interdiction-countermeasures">
          <button class="command-tile primary" type="button" data-action="war_counter_escort">
            <span>护粮</span><b>护送车队</b>
          </button>
          <button class="command-tile" type="button" data-action="war_counter_reroute">
            <span>改道</span><b>绕避险段</b>
          </button>
          <button class="command-tile" type="button" data-action="war_counter_scout">
            <span>斥候</span><b>反查伏点</b>
          </button>
          <button class="command-tile warning" type="button" data-action="war_counter_decoy">
            <span>诱敌</span><b>弃车设伏</b>
          </button>
          <button class="command-tile primary" type="button" data-action="route_blockade_guard">
            <span>守备</span><b>加派关防</b>
          </button>
          <button class="command-tile danger" type="button" data-action="route_blockade_clear">
            <span>清障</span><b>拔除封锁</b>
          </button>
        </div>
        <div class="preview-line">当前反制目标：${escapeHtml(currentEnemyInterdiction ? `${currentEnemyInterdiction.id} ${currentEnemyInterdiction.routeLabel}` : '无')}</div>
        <div class="preview-line">当前反制：${escapeHtml(currentEnemyInterdiction?.lastCountermeasure ?? '未下达')}</div>
        <div class="preview-line">底部摘要：${escapeHtml(routePressureSummary)}</div>
        ${this.renderEnemyInterdictionOrders()}
      </section>
      <section class="info-band war-tab-panel${this.warTab === 'logistics' ? '' : ' hidden'}" data-testid="war-command-queue">
        <div class="band-title">多回合军令排程</div>
        ${this.commandQueue.length > 0 ? commandBadgeLines.join('') : '<div class="preview-line muted">暂无军令排程</div>'}
      </section>
      <section class="info-band war-tab-panel${this.warTab === 'logistics' ? '' : ' hidden'}" data-testid="logistics-queue">
        <div class="band-title">后勤记录</div>
        ${queueLines(this.logisticsQueue, '暂无后勤队列')}
      </section>
      <section class="info-band source-band war-tab-panel${this.warTab === 'report' ? '' : ' hidden'}" data-testid="battle-report">
        <div class="band-title">战报与目标说明</div>
        <div class="war-turn-strip">
          <span>战时回合 <b>第 ${this.currentWarTurn} 回合</b></span>
          <span>在途军令 <b>${this.commandQueue.length} 项</b></span>
        </div>
        <div class="command-alert${this.latestInterceptionAlert ? '' : ' muted'}">${escapeHtml(activeAlert)}</div>
        <div class="preview-line">战术修正：${escapeHtml(tacticSummary.text)}</div>
        <div class="preview-line">地图叠加：黄铜车队显示补给分段，赤色路段显示敌方最可能截粮的位置。</div>
        <div class="preview-line">敌方截粮命令：${escapeHtml(enemyInterdictionLines[0] ?? '暂无已显形命令')}</div>
        <div class="preview-line">占后队列：${escapeHtml(this.occupationQueue[0] ? `${describeOccupationTask(this.occupationQueue[0])}；地图上同步显示阶段徽标` : '暂无新附地区待处理')}</div>
        <div class="preview-line">最近军令：${escapeHtml(this.operationLog[0] ?? '暂无')}</div>
        ${queueLines(commandLines.slice(0, 4), '暂无在途军令')}
        ${this.renderBattleReportHistory()}
        ${queueLines(enemyInterdictionLines.slice(0, 3), '暂无敌方截粮命令')}
        ${queueLines(occupationLines.slice(0, 3), '暂无占后队列')}
        ${queueLines(this.operationLog.slice(0, 4), '暂无战报')}
        <p>${safeTargetSummary}</p>
      </section>
    `;
  }

  private renderBattleReportHistory(): string {
    if (this.battleReportHistory.length === 0) return '<div class="info-band"><div class="band-title">战斗结算</div><div class="preview-line muted">暂无战斗结算记录</div></div>';

    const kindLabels: Record<BattleOutcome['kind'], string> = {
      attack: '攻占',
      supply: '运输',
      scout: '侦察',
      deploy: '部署',
      fortify: '布防'
    };

    const lines = this.battleReportHistory.map((report) => {
      const kind = kindLabels[report.kind];
      const turn = report.turn;
      const region = report.regionName;
      let detail = '';
      if (report.casualties) detail += `伤亡 ${formatNumber(report.casualties)}`;
      if (report.supplyUsed && report.kind !== 'supply') detail += `${detail ? ' / ' : ''}补给 ${report.supplyUsed}`;
      const statusIcon = report.success ? '✓' : '✗';
      const risk = report.interceptionRisk === undefined ? '截粮风险待评估' : `截粮风险 ${report.interceptionRisk}%`;
      const tactic = report.tacticText ?? '战术修正待记录';
      return `
        <div class="battle-outcome-card">
          <div class="battle-outcome-main">
            <span class="badge ${report.kind === 'attack' ? 'badge-danger' : report.kind === 'supply' ? 'badge-bronze' : 'badge-jade'}">${escapeHtml(kind)}</span>
            <b>${escapeHtml(statusIcon)} 第${turn}回合 ${region}</b>
            <em>${escapeHtml(detail || report.result)}</em>
          </div>
          <div class="battle-outcome-tactics">主将 ${escapeHtml(report.generalName ?? '未记录')} / 配比 ${escapeHtml(report.unitMix ?? '未记录')}</div>
          ${tacticBadgeRow(report.tacticDeltas)}
          <div class="battle-outcome-tactics">战术 ${escapeHtml(report.tacticScore ?? '0')}：${escapeHtml(tactic)} / ${escapeHtml(risk)}</div>
        </div>
      `;
    });

    return `
      <section class="info-band" data-testid="battle-report-history">
        <div class="band-title">战斗结算</div>
        ${lines.join('')}
      </section>
    `;
  }

  private renderLogisticsPriority(): string {
    const priorities = calculateInterdictionPriority(this.enemyInterdictionOrders);
    if (priorities.length === 0) {
      return `<div class="preview-line muted">暂无截粮威胁待处理</div>`;
    }

    const rows = priorities.slice(0, 3).map(p => {
      const riskClass = p.riskLevel >= 70 ? 'badge-danger' : p.riskLevel >= 50 ? 'badge-bronze' : 'badge-jade';
      return `
        <div class="priority-row">
          <span class="badge ${riskClass}">${p.riskLevel}%</span>
          <span>${p.targetName}</span>
          <span class="badge badge-jade">${p.recommendedCountermeasure === 'escort' ? '护粮' : p.recommendedCountermeasure === 'reroute' ? '改道' : p.recommendedCountermeasure === 'counter-scout' ? '反斥' : '诱敌'}</span>
          <em>${p.reasoning}</em>
        </div>
      `;
    });

    return `
      <div class="priority-list">
        <div class="priority-header">截粮威胁优先级</div>
        ${rows.join('')}
      </div>
    `;
  }

  private selectedEnemyInterdictionOrder(): EnemyInterdictionOrder | undefined {
    if (!this.selectedEnemyInterdictionId) return undefined;
    return this.enemyInterdictionOrders.find((order) => order.id === this.selectedEnemyInterdictionId);
  }

  private activeEnemyInterdictionOrder(): EnemyInterdictionOrder | undefined {
    return this.selectedEnemyInterdictionOrder() ?? this.enemyInterdictionOrders[0];
  }

  private renderSelectedEnemyInterdiction(): string {
    const order = this.activeEnemyInterdictionOrder();
    if (!order) {
      return `<div class="preview-line muted" data-testid="selected-enemy-interdiction">地图截粮：未选中威胁</div>`;
    }
    const selectedLabel = order.id === this.selectedEnemyInterdictionId ? '已选中' : '默认目标';
    return `
      <div class="priority-row selected-logistics-object" data-testid="selected-enemy-interdiction">
        <span class="badge badge-danger">${selectedLabel}</span>
        <span>${escapeHtml(order.id)} ${escapeHtml(order.routeLabel)}</span>
        <span class="badge badge-neutral">${escapeHtml(order.chokePointLabel)}</span>
        <span class="badge badge-neutral">风险 ${order.risk}%</span>
        <em>${escapeHtml(describeEnemyInterdictionOrder(order))}</em>
      </div>
    `;
  }

  private renderEnemyInterdictionOrders(): string {
    if (this.enemyInterdictionOrders.length === 0) {
      return '<div class="queue-lines"><div>暂无敌方截粮命令</div></div>';
    }

    return `
      <div class="enemy-interdiction-list">
        ${this.enemyInterdictionOrders.map((order) => {
          const selected = order.id === this.selectedEnemyInterdictionId;
          const countermeasure = order.lastCountermeasure ? `反制 ${order.lastCountermeasure}` : '未反制';
          return `
            <button class="enemy-interdiction-row${selected ? ' active' : ''}" type="button" data-enemy-interdiction-id="${escapeHtml(order.id)}">
              <span class="badge ${selected ? 'badge-danger' : 'badge-neutral'}">${selected ? '已选' : '可选'}</span>
              <b>${escapeHtml(order.id)} ${escapeHtml(order.routeLabel)}</b>
              <span class="badge badge-neutral">${escapeHtml(order.chokePointLabel)}</span>
              <span>风险 ${order.risk}% / 损耗 ${order.supplyDamage}</span>
              <em>${escapeHtml(enemyInterdictionStageName(order.stage))} / ${escapeHtml(countermeasure)}</em>
            </button>
          `;
        }).join('')}
      </div>
    `;
  }

  private renderRouteAlternatives(alternatives: RouteAlternative[]): string {
    if (alternatives.length <= 1) {
      return `
        <section class="info-band war-tab-panel${this.warTab === 'route' ? '' : ' hidden'}" data-testid="route-alternatives">
          <div class="band-title">路线方案对比</div>
          <div class="preview-line muted">暂无可用绕行方案</div>
        </section>
      `;
    }

    const rows = alternatives.map((alternative) => {
      const riskClass = alternative.interceptionRisk >= 70 ? 'badge-danger' : alternative.interceptionRisk >= 45 ? 'badge-bronze' : 'badge-jade';
      const selectedClass = alternative.selected ? ' active' : '';
      const selectedLabel = alternative.selected ? '已采用' : alternative.recommendation;
      return `
        <button class="route-alternative-row${selectedClass}" type="button" data-route-alternative-id="${escapeHtml(alternative.id)}">
          <span class="badge ${alternative.selected ? 'badge-danger' : 'badge-neutral'}">${escapeHtml(selectedLabel)}</span>
          <b>${escapeHtml(alternative.label)}：${escapeHtml(alternative.routeLabel)}</b>
          <span>容量 ${alternative.currentUsage}/${alternative.capacity} / 补给 ${alternative.supplyCost} / ${alternative.turns} 回合</span>
          <span class="badge ${riskClass}">截粮 ${alternative.interceptionRisk}%</span>
          <em>${alternative.networkLabel ? `路网 ${escapeHtml(alternative.networkLabel)} / ` : ''}${escapeHtml(alternative.bottleneckLabel)}：${escapeHtml(alternative.terrainReason)}</em>
        </button>
      `;
    }).join('');

    return `
      <section class="info-band war-tab-panel${this.warTab === 'route' ? '' : ' hidden'}" data-testid="route-alternatives">
        <div class="band-title">路线方案对比</div>
        <div class="preview-line">点击方案可改为直达或设置中继；比较容量、补给、截粮风险后再下军令。</div>
        <div class="route-alternative-list">${rows}</div>
      </section>
    `;
  }

  private renderOccupationSupplyTasks(): string {
    if (this.occupationSupplyTasks.length === 0) {
      return `<div class="preview-line muted">暂无待安抚运输任务</div>`;
    }

    const rows = this.occupationSupplyTasks.slice(0, 3).map(t => {
      const statusClass = t.status === 'pending' ? 'badge-bronze' : t.status === 'dispatched' ? 'badge-jade' : t.status === 'in-transit' ? 'badge-danger' : t.status === 'cancelled' ? 'badge-neutral' : 'badge-jade';
      const stageLabel = t.stage === 'military-govern' ? '军管' : t.stage === 'pacify' ? '安抚' : '编户';
      const statusLabel = t.status === 'pending' ? '待运输' : t.status === 'dispatched' ? '已派遣' : t.status === 'in-transit' ? '运输中' : t.status === 'cancelled' ? '已取消' : '运输已送达';
      return `
        <div class="supply-task-row">
          <span>${escapeHtml(t.convoyId)}</span>
          <span>${t.regionName}</span>
          <span class="badge badge-neutral">${escapeHtml(t.routeLabel)}</span>
          <span class="badge badge-neutral">${stageLabel}</span>
          <span class="badge ${statusClass}">${statusLabel}</span>
          <span>需 ${t.supplyNeeded}</span>
        </div>
      `;
    });

    return `
      <div class="supply-task-list">
        <div class="priority-header">安抚运输任务</div>
        <div class="command-tile-grid micro-grid" data-testid="occupation-supply-controls">
          <button class="command-tile" type="button" data-action="occupation_supply_promote">
            <span>运输</span><b>上移车队</b>
          </button>
          <button class="command-tile warning" type="button" data-action="occupation_supply_cancel">
            <span>运输</span><b>取消车队</b>
          </button>
        </div>
        ${rows.join('')}
      </div>
    `;
  }

  private renderRouteCapacities(): string {
    if (this.routeCapacities.size === 0) {
      return `<div class="preview-line muted">暂无路线容量记录</div>`;
    }

    const rows = [...this.routeCapacities.values()]
      .sort((a, b) => b.currentUsage - a.currentUsage)
      .slice(0, 3)
      .map((capacity) => {
        const statusClass = capacity.congestionLevel === 'critical' ? 'badge-danger' : capacity.congestionLevel === 'high' ? 'badge-bronze' : 'badge-jade';
        const fromName = this.dataset.regionById.get(capacity.fromRegion)?.definition.name ?? capacity.fromRegion;
        const toName = this.dataset.regionById.get(capacity.toRegion)?.definition.name ?? capacity.toRegion;
        return `
          <div class="priority-row">
            <span class="badge ${statusClass}">${capacity.currentUsage}/${capacity.maxArmies}</span>
            <span>${escapeHtml(fromName)}→${escapeHtml(toName)}</span>
            <span class="badge badge-neutral">${escapeHtml(capacity.bottleneckLabel)}</span>
            <em>${capacity.congestionLevel === 'critical' ? '拥堵严重' : capacity.congestionLevel === 'high' ? '需要分流' : capacity.congestionLevel === 'medium' ? '运输正常' : '路线宽裕'}</em>
          </div>
        `;
      });

    return `
      <div class="priority-list">
        <div class="priority-header">路线容量</div>
        ${rows.join('')}
      </div>
    `;
  }

  private renderTransportConvoys(): string {
    if (this.transportConvoys.length === 0) {
      return `<div class="preview-line muted" data-testid="transport-convoy-list">暂无运输队实体</div>`;
    }

    const rows = this.transportConvoys
      .slice()
      .sort((a, b) => transportConvoyStatusRank(a.status) - transportConvoyStatusRank(b.status) || b.priority - a.priority || a.orderIndex - b.orderIndex)
      .slice(0, 4)
      .map((convoy) => {
        const statusClass = convoy.status === 'cancelled' ? 'badge-neutral' : convoy.status === 'delivered' ? 'badge-jade' : convoy.status === 'moving' ? 'badge-bronze' : 'badge-danger';
        return `
          <div class="priority-row">
            <span class="badge ${statusClass}">${transportConvoyStatusName(convoy.status)}</span>
            <span>${escapeHtml(convoy.id)} ${escapeHtml(convoy.routeLabel)}</span>
            <span class="badge badge-neutral">优先 ${convoy.priority}</span>
            <span class="badge badge-neutral">路线 ${convoy.routeUsage}/${convoy.routeCapacity}</span>
            <em>送达 ${convoy.deliveredSupply}/${convoy.plannedSupplyReserve}，${escapeHtml(convoy.bottleneckLabel)}</em>
          </div>
        `;
      });

    return `
      <div class="priority-list" data-testid="transport-convoy-list">
        <div class="priority-header">运输队实体</div>
        <div class="command-tile-grid micro-grid" data-testid="transport-convoy-controls">
          <button class="command-tile" type="button" data-action="transport_convoy_promote">
            <span>车队</span><b>上移运输</b>
          </button>
          <button class="command-tile warning" type="button" data-action="transport_convoy_cancel">
            <span>车队</span><b>取消运输</b>
          </button>
        </div>
        ${rows.join('')}
      </div>
    `;
  }

  private selectedLogisticsObject(): LogisticsMapObject | undefined {
    if (!this.selectedLogisticsObjectId) return undefined;
    return this.getLogisticsMapObjects().find((object) => object.id === this.selectedLogisticsObjectId);
  }

  private selectedTransportConvoy(): TransportConvoy | undefined {
    const object = this.selectedLogisticsObject();
    if (!object || object.kind !== 'transport-convoy') return undefined;
    return this.transportConvoys.find((convoy) => convoy.id === object.id);
  }

  private selectedOccupationSupplyTask(): OccupationSupplyTask | undefined {
    const object = this.selectedLogisticsObject();
    if (!object || object.kind !== 'occupation-supply') return undefined;
    return this.occupationSupplyTasks.find((task) => task.convoyId === object.id);
  }

  private renderSelectedLogisticsObject(): string {
    const object = this.selectedLogisticsObject();
    if (!object) {
      return `<div class="preview-line muted" data-testid="selected-logistics-object">地图后勤：未选中实体</div>`;
    }

    const kind = object.kind === 'transport-convoy' ? '运输队' : object.kind === 'occupation-supply' ? '安抚运输' : '兵站';
    return `
      <div class="priority-row selected-logistics-object" data-testid="selected-logistics-object">
        <span class="badge badge-jade">${kind}</span>
        <span>${escapeHtml(object.label)} ${escapeHtml(object.routeLabel)}</span>
        <span class="badge badge-neutral">${escapeHtml(object.status)}</span>
        <em>${escapeHtml(object.details)}</em>
      </div>
    `;
  }

  private renderGovernanceLogisticsEffects(): string {
    const effects = [...this.governanceLogisticsEffects.values()]
      .filter((effect) => governanceLogisticsScore(effect) > 0)
      .sort((a, b) => governanceLogisticsScore(b) - governanceLogisticsScore(a))
      .slice(0, 3);

    if (effects.length === 0) {
      return `<div class="preview-line muted" data-testid="governance-logistics-list">暂无治理后勤修正</div>`;
    }

    const rows = effects.map((effect) => `
      <div class="priority-row">
        <span class="badge badge-jade">${escapeHtml(effect.regionName)}</span>
        <span>${escapeHtml(formatGovernanceLogisticsDelta(effect))}</span>
        <em>${escapeHtml(effect.sources.slice(-2).join(' / '))}</em>
      </div>
    `);

    return `
      <div class="priority-list" data-testid="governance-logistics-list">
        <div class="priority-header">治理后勤修正</div>
        ${rows.join('')}
      </div>
    `;
  }

  private regionGovernanceLogisticsSummary(region: RegionViewModel): string {
    const effect = this.governanceLogisticsEffects.get(region.definition.id);
    if (!effect || governanceLogisticsScore(effect) <= 0) return '尚未形成路线加成';
    return `${formatGovernanceLogisticsDelta(effect)} / ${effect.sources.slice(-2).join(' / ')}`;
  }

  private governanceLogisticsSummary(): string {
    const strongest = [...this.governanceLogisticsEffects.values()]
      .filter((effect) => governanceLogisticsScore(effect) > 0)
      .sort((a, b) => governanceLogisticsScore(b) - governanceLogisticsScore(a))[0];
    if (!strongest) return '';
    return `${strongest.regionName} ${formatGovernanceLogisticsDelta(strongest)}`;
  }

  private applyGovernanceLogisticsEffect(region: RegionViewModel, source: string, delta: GovernanceLogisticsDelta): void {
    if (governanceLogisticsScore(delta) <= 0) return;
    const existing = this.governanceLogisticsEffects.get(region.definition.id);
    const effect: GovernanceLogisticsEffect = existing ?? {
      regionId: region.definition.id,
      regionName: region.definition.name,
      capacityBonus: 0,
      supplyRelief: 0,
      interdictionRelief: 0,
      occupationBandwidthBonus: 0,
      sources: []
    };

    effect.capacityBonus = Math.min(3, effect.capacityBonus + delta.capacityBonus);
    effect.supplyRelief = Math.min(12, effect.supplyRelief + delta.supplyRelief);
    effect.interdictionRelief = Math.min(18, effect.interdictionRelief + delta.interdictionRelief);
    effect.occupationBandwidthBonus = Math.min(12, effect.occupationBandwidthBonus + delta.occupationBandwidthBonus);
    effect.sources.push(source);
    if (effect.sources.length > 5) effect.sources.splice(0, effect.sources.length - 5);
    this.governanceLogisticsEffects.set(region.definition.id, effect);
    this.refreshRouteCapacityForRegion(region.definition.id);
  }

  // ============================================
  // Logistics Dispatcher - Integration Methods
  // ============================================

  /** Auto-create station when fortify command completes */
  private onStationBuilt(region: RegionViewModel): void {
    buildLogisticsStation(this.logisticsStations, region.definition.id, region.definition.name, this.stationCounter++);
    this.refreshRouteCapacityForRegion(region.definition.id);
  }

  /** Update route capacity when army is assigned */
  private onRouteAssigned(fromId: string, toId: string, usageDelta = 1): RouteCapacityConstraint {
    const routeKey = routeCapacityKey(fromId, toId);
    const existing = this.routeCapacities.get(routeKey);
    const currentUsage = Math.max(0, (existing?.currentUsage ?? 0) + usageDelta);
    return updateRouteCapacity(this.routeCapacities, fromId, toId, currentUsage, this.routeCapacityLimit(fromId, toId), this.routeTerrainProfileByIds(fromId, toId));
  }

  private registerRouteCapacity(command: WarCommand): RouteCapacityConstraint {
    const legs = this.commandRouteLegs(command);
    const constraints = legs.map((leg) => this.onRouteAssigned(leg.fromRegionId, leg.toRegionId, 1));
    this.applyRouteConstraintsToCommand(command, constraints);
    for (const constraint of constraints) {
      this.refreshCommandsForRoute(constraint);
    }
    return this.bottleneckConstraint(constraints);
  }

  private releaseRouteCapacity(command: WarCommand): void {
    const legs = this.commandRouteLegs(command);
    const constraints = legs.map((leg) => this.onRouteAssigned(leg.fromRegionId, leg.toRegionId, -1));
    this.applyRouteConstraintsToCommand(command, constraints);
    for (const constraint of constraints) {
      this.refreshCommandsForRoute(constraint);
    }
  }

  private refreshCommandsForRoute(constraint: RouteCapacityConstraint): void {
    for (const command of this.commandQueue) {
      const leg = this.commandRouteLegs(command).find((candidate) => candidate.routeId === constraint.routeId);
      if (!leg) continue;
      leg.routeCapacity = constraint.maxArmies;
      leg.routeUsage = constraint.currentUsage;
      leg.roadClass = constraint.roadClass;
      leg.bottleneckLabel = constraint.bottleneckLabel;
      leg.terrainReason = constraint.terrainReason;
      leg.networkId = constraint.networkId;
      leg.networkLabel = constraint.networkLabel;
      this.applyRouteConstraintsToCommand(command);
      command.interceptionRisk = this.commandInterceptionRisk(command);
      this.syncTransportConvoy(command);
    }
  }

  private commandRouteLegs(command: WarCommand): CommandRouteLeg[] {
    if (command.routeLegs.length > 0) return command.routeLegs;
    const from = this.dataset.regionById.get(command.fromRegionId);
    const target = this.dataset.regionById.get(command.targetRegionId);
    const waypoint = command.waypointRegionId ? this.dataset.regionById.get(command.waypointRegionId) : undefined;
    if (!from || !target) return [];
    command.routeLegs = this.commandRouteLegsFromRegions(from, target, waypoint);
    return command.routeLegs;
  }

  private commandRouteLegsFromRegions(from: RegionViewModel, target: RegionViewModel, waypoint?: RegionViewModel): CommandRouteLeg[] {
    return this.routeLegEstimates(from, target, waypoint).map((leg) => ({
      routeId: routeCapacityKey(leg.from.definition.id, leg.target.definition.id),
      fromRegionId: leg.from.definition.id,
      toRegionId: leg.target.definition.id,
      fromName: leg.from.definition.name,
      toName: leg.target.definition.name,
      routeUsage: leg.currentUsage,
      routeCapacity: leg.capacity,
      roadClass: leg.profile.roadClass,
      bottleneckLabel: leg.profile.bottleneckLabel,
      terrainReason: leg.profile.terrainReason,
      networkId: leg.profile.networkId,
      networkLabel: leg.profile.networkLabel
    }));
  }

  private applyRouteConstraintsToCommand(command: WarCommand, constraints?: RouteCapacityConstraint[]): void {
    const activeConstraints = constraints ?? this.commandRouteLegs(command)
      .map((leg) => this.routeCapacities.get(leg.routeId))
      .filter((constraint): constraint is RouteCapacityConstraint => Boolean(constraint));
    if (activeConstraints.length === 0) return;

    for (const constraint of activeConstraints) {
      const leg = this.commandRouteLegs(command).find((candidate) => candidate.routeId === constraint.routeId);
      if (!leg) continue;
      leg.routeUsage = constraint.currentUsage;
      leg.routeCapacity = constraint.maxArmies;
      leg.roadClass = constraint.roadClass;
      leg.bottleneckLabel = constraint.bottleneckLabel;
      leg.terrainReason = constraint.terrainReason;
      leg.networkId = constraint.networkId;
      leg.networkLabel = constraint.networkLabel;
    }

    const bottleneck = this.bottleneckConstraint(activeConstraints);
    command.routeCapacity = Math.max(1, Math.min(...activeConstraints.map((constraint) => constraint.maxArmies)));
    command.routeUsage = Math.max(0, ...activeConstraints.map((constraint) => constraint.currentUsage));
    command.roadClass = bottleneck.roadClass;
    command.bottleneckLabel = bottleneck.bottleneckLabel;
    command.stationBonus = Math.max(0, ...this.commandRouteLegs(command).map((leg) => this.routeStationBonus(leg.fromRegionId, leg.toRegionId)));
  }

  private bottleneckConstraint(constraints: RouteCapacityConstraint[]): RouteCapacityConstraint {
    return [...constraints].sort((a, b) => a.maxArmies - b.maxArmies || b.currentUsage - a.currentUsage)[0] ?? constraints[0];
  }

  private routeCapacityLimit(fromId: string, toId: string): number {
    const terrain = this.routeTerrainProfileByIds(fromId, toId);
    const stationBonus =
      this.logisticsStations.some((station) => station.isActive && station.regionId === fromId) ? 1 : 0;
    const targetBonus =
      this.logisticsStations.some((station) => station.isActive && station.regionId === toId) ? 1 : 0;
    const governanceBonus = this.routeGovernanceLogisticsModifier(fromId, toId).capacityBonus;
    return terrain.baseCapacity + stationBonus + targetBonus + governanceBonus;
  }

  private routeGovernanceLogisticsModifier(fromId: string, toId: string): GovernanceLogisticsDelta {
    const from = this.governanceLogisticsEffects.get(fromId);
    const to = this.governanceLogisticsEffects.get(toId);
    const effects = [from, to].filter((candidate): candidate is GovernanceLogisticsEffect => Boolean(candidate));
    return {
      capacityBonus: Math.min(3, effects.reduce((sum, effect) => sum + effect.capacityBonus, 0)),
      supplyRelief: Math.min(12, effects.reduce((sum, effect) => sum + effect.supplyRelief, 0)),
      interdictionRelief: Math.min(18, effects.reduce((sum, effect) => sum + effect.interdictionRelief, 0)),
      occupationBandwidthBonus: Math.min(12, effects.reduce((sum, effect) => sum + effect.occupationBandwidthBonus, 0))
    };
  }

  private refreshRouteCapacityForRegion(regionId: string): void {
    for (const capacity of this.routeCapacities.values()) {
      if (capacity.fromRegion !== regionId && capacity.toRegion !== regionId) continue;
      updateRouteCapacity(
        this.routeCapacities,
        capacity.fromRegion,
        capacity.toRegion,
        capacity.currentUsage,
        this.routeCapacityLimit(capacity.fromRegion, capacity.toRegion),
        this.routeTerrainProfileByIds(capacity.fromRegion, capacity.toRegion)
      );
    }

    for (const command of this.commandQueue) {
      if (!this.commandRouteLegs(command).some((leg) => leg.fromRegionId === regionId || leg.toRegionId === regionId)) continue;
      const constraints = this.commandRouteLegs(command).map((leg) => {
        const existing = this.routeCapacities.get(leg.routeId);
        return updateRouteCapacity(
          this.routeCapacities,
          leg.fromRegionId,
          leg.toRegionId,
          existing?.currentUsage ?? leg.routeUsage,
          this.routeCapacityLimit(leg.fromRegionId, leg.toRegionId),
          this.routeTerrainProfileByIds(leg.fromRegionId, leg.toRegionId)
        );
      });
      this.applyRouteConstraintsToCommand(command, constraints);
      command.interceptionRisk = this.commandInterceptionRisk(command);
      this.syncTransportConvoy(command);
    }
  }

  private commandInterceptionRisk(command: WarCommand): number {
    const army = this.dataset.armies.find((candidate) => candidate.id === command.armyId) ?? this.activeArmy;
    const target = this.dataset.regionById.get(command.targetRegionId);
    if (!target) return command.interceptionRisk;
    const modifier = this.tacticalModifierForArmy(army, target);
    const legs = this.commandRouteLegs(command);
    const routeTerrain = legs.length > 0
      ? Math.max(...legs.map((leg) => this.routeTerrainProfileByIds(leg.fromRegionId, leg.toRegionId).interceptionModifier))
      : this.routeTerrainProfileByIds(command.fromRegionId, command.targetRegionId).interceptionModifier;
    const governance = legs.length > 0
      ? {
          interdictionRelief: Math.min(18, legs.reduce((sum, leg) => sum + this.routeGovernanceLogisticsModifier(leg.fromRegionId, leg.toRegionId).interdictionRelief, 0))
        }
      : this.routeGovernanceLogisticsModifier(command.fromRegionId, command.targetRegionId);
    return Math.round(clamp(
      18 + (target.history?.weaponTraditions?.length ?? 0) * 7 + (target.definition.terrain.includes('mountain') ? 16 : 0) + Math.max(0, 72 - army.supply) * 0.25 + modifier.interceptionDelta + routeTerrain - governance.interdictionRelief,
      0,
      94
    ));
  }

  private routeTerrainProfileByIds(fromId: string, toId: string): RouteTerrainProfile {
    const from = this.dataset.regionById.get(fromId);
    const to = this.dataset.regionById.get(toId);
    if (!from || !to) return openRouteProfile('地区数据缺失，按普通驿道处理');
    return this.routeTerrainProfile(from, to);
  }

  private strategicRouteNetworkForLeg(fromId: string, toId: string): RouteNetworkDefinition | undefined {
    return this.dataset.routeNetworks.find((network) => routeNetworkHasLeg(network, fromId, toId));
  }

  private routeBlockadeRulesFor(blockade: RouteBlockade): RouteNetworkBlockadeDefinition {
    const network = blockade.networkId
      ? this.dataset.routeNetworks.find((candidate) => candidate.id === blockade.networkId)
      : this.strategicRouteNetworkForLeg(blockade.fromRegionId, blockade.toRegionId);
    return network?.blockade ?? DEFAULT_ROUTE_BLOCKADE_RULES;
  }

  private routeTerrainProfile(from: RegionViewModel, target: RegionViewModel): RouteTerrainProfile {
    const network = this.strategicRouteNetworkForLeg(from.definition.id, target.definition.id);
    if (network) {
      return {
        roadClass: network.roadClass,
        bottleneckLabel: network.label,
        baseCapacity: network.baseCapacity,
        supplyFactor: network.supplyFactor,
        interceptionModifier: network.interceptionModifier,
        terrainReason: `已标注路网：${network.reason}`,
        networkId: network.id,
        networkLabel: network.label
      };
    }

    const tags = [
      from.definition.terrain,
      target.definition.terrain,
      from.geography.kind,
      target.geography.kind,
      ...(from.history?.geographyTags ?? []),
      ...(target.history?.geographyTags ?? [])
    ].join(' ');

    if (matches(tags, ['mountain', 'pass', 'gate', 'plateau', 'upland'])) {
      return {
        roadClass: 'pass-bottleneck',
        bottleneckLabel: '关隘山道',
        baseCapacity: 1,
        supplyFactor: 1.28,
        interceptionModifier: 14,
        terrainReason: '山地、关隘和高原道路限制车队并放大截粮风险'
      };
    }

    if (matches(tags, ['river_delta', 'water_network', 'harbor'])) {
      return {
        roadClass: 'water-network',
        bottleneckLabel: '水网转运',
        baseCapacity: 2,
        supplyFactor: 1.12,
        interceptionModifier: 8,
        terrainReason: '水网转运容量中等，但渡口和仓埠容易暴露'
      };
    }

    if (matches(tags, ['hill', 'forest', 'subtropical'])) {
      return {
        roadClass: 'hill-road',
        bottleneckLabel: '丘陵曲道',
        baseCapacity: 2,
        supplyFactor: 1.1,
        interceptionModifier: 6,
        terrainReason: '丘陵林地道路弯折，运输吞吐受限'
      };
    }

    if (matches(tags, ['frontier', 'corridor', 'horse', 'arid'])) {
      return {
        roadClass: 'frontier-track',
        bottleneckLabel: '边地驿道',
        baseCapacity: 2,
        supplyFactor: 1.04,
        interceptionModifier: 4,
        terrainReason: '边地驿道可通骑军，但补给节点稀疏'
      };
    }

    if (matches(tags, ['river_plain', 'river_grain'])) {
      return {
        roadClass: 'river-road',
        bottleneckLabel: '河谷官道',
        baseCapacity: 3,
        supplyFactor: 0.98,
        interceptionModifier: 1,
        terrainReason: '河谷官道利于转运，但渡点仍需保护'
      };
    }

    return openRouteProfile('平原官道容量较高，适合连续运输');
  }

  private routeStationBonus(fromId: string, toId: string): number {
    const stationBonus = this.logisticsStations
      .filter((station) => station.isActive && (station.regionId === fromId || station.regionId === toId))
      .reduce((sum, station) => sum + station.supplyBonus, 0);
    const governance = this.routeGovernanceLogisticsModifier(fromId, toId);
    return stationBonus + Math.round((governance.supplyRelief + governance.occupationBandwidthBonus) / 2);
  }

  /** Process occupation supply automation each turn */
  private processOccupationSupplyAutomation(): string[] {
    if (!this.supplyAutomationEnabled) return [];
    const messages: string[] = [];
    let availableBandwidth = this.currentLogisticsBandwidth();
    const dueTasks = this.occupationSupplyTasks
      .filter((task) => task.status !== 'delivered' && task.status !== 'cancelled' && task.autoDispatchTurn <= this.currentWarTurn)
      .sort((a, b) => b.priority - a.priority || a.orderIndex - b.orderIndex);

    for (const task of dueTasks) {
      if (availableBandwidth <= 0 || this.nationState.food <= 0) break;
      if (task.status === 'pending') task.status = 'in-transit';

      const laneBandwidth = Math.max(2, availableBandwidth - task.bandwidthUsed + 2);
      const delivered = Math.min(task.supplyNeeded, laneBandwidth, Math.max(0, this.nationState.food));
      if (delivered <= 0) break;

      this.spend('food', delivered);
      task.supplyNeeded = Math.max(0, task.supplyNeeded - delivered);
      task.bandwidthUsed += delivered;
      availableBandwidth -= Math.max(1, Math.round(delivered / 2));

      const region = this.dataset.regionById.get(task.regionId);
      if (region) {
        region.risk = clamp(region.risk - Math.max(1, Math.round(delivered / 12)), 0, 100);
        region.integration = clamp(region.integration + Math.max(1, Math.round(delivered / 18)), 0, 100);
      }

      if (task.supplyNeeded <= 0) {
        task.status = 'delivered';
        this.releaseOccupationSupplyRoute(task);
        messages.push(`${task.regionName}安抚运输送达，占后压力下降`);
      } else {
        messages.push(`${task.regionName}安抚运输占用 ${delivered} 带宽，尚需 ${task.supplyNeeded}`);
      }
    }
    this.occupationSupplyTasks.sort((a, b) => statusRank(a.status) - statusRank(b.status) || b.priority - a.priority || a.orderIndex - b.orderIndex);
    trimTo(this.occupationSupplyTasks, 6);
    return messages;
  }

  private currentLogisticsBandwidth(): number {
    const stationLift = this.logisticsStations.filter((station) => station.isActive).length * 6;
    const governanceLift = [...this.governanceLogisticsEffects.values()].reduce((sum, effect) => sum + effect.occupationBandwidthBonus, 0);
    const congestionPenalty = [...this.routeCapacities.values()]
      .filter((capacity) => capacity.congestionLevel === 'high' || capacity.congestionLevel === 'critical')
      .length * 4;
    return Math.max(8, 18 + stationLift + governanceLift - congestionPenalty);
  }

  /** Toggle supply automation */
  setSupplyAutomation(enabled: boolean): void {
    this.supplyAutomationEnabled = enabled;
  }

  /** Get logistics dispatcher summary for debugging */
  getLogisticsDispatcherState(): {
    stationCount: number;
    routeCapacityCount: number;
    pendingSupplyTasks: number;
    automationEnabled: boolean;
  } {
    return {
      stationCount: this.logisticsStations.length,
      routeCapacityCount: this.routeCapacities.size,
      pendingSupplyTasks: this.occupationSupplyTasks.filter(t => t.status === 'pending').length,
      automationEnabled: this.supplyAutomationEnabled
    };
  }

  private routeCapacitySummary(): string {
    const active = [...this.routeCapacities.values()].sort((a, b) => b.currentUsage - a.currentUsage)[0];
    if (!active) return '';
    const fromName = this.dataset.regionById.get(active.fromRegion)?.definition.name ?? active.fromRegion;
    const toName = this.dataset.regionById.get(active.toRegion)?.definition.name ?? active.toRegion;
    return `${fromName}->${toName} ${active.currentUsage}/${active.maxArmies} ${active.bottleneckLabel} ${active.congestionLevel}`;
  }

  private routeRoadSummary(): string {
    const route = this.createActiveRouteForecast(this.selectedRegion);
    const governance = this.routeGovernanceLogisticsModifier(route.from.definition.id, route.target.definition.id);
    const governanceText = governanceLogisticsScore(governance) > 0 ? ` / 治理 ${formatGovernanceLogisticsDelta(governance)}` : '';
    return `${route.bottleneckLabel ?? '平原官道'} / 容量 ${route.routeCapacity ?? this.routeCapacityLimit(route.from.definition.id, route.target.definition.id)} / ${route.terrainReason ?? '平原官道容量较高，适合连续运输'}${governanceText}`;
  }

  private enemyInterdictionDoctrineName(): string {
    this.refreshEnemyInterdictionDoctrine();
    const names: Record<EnemyInterdictionDoctrine, string> = {
      'cut-supply': '断粮优先',
      'bleed-army': '消耗主力',
      'stall-pacification': '阻断安抚'
    };
    return names[this.enemyInterdictionMemory.doctrine];
  }

  private enemyInterdictionMemorySummary(): string {
    const target = this.enemyInterdictionMemory.lastTargetRegionId
      ? this.dataset.regionById.get(this.enemyInterdictionMemory.lastTargetRegionId)?.definition.name ?? this.enemyInterdictionMemory.lastTargetRegionId
      : '暂无';
    return `上次目标 ${target} / 成功 ${this.enemyInterdictionMemory.successfulRaids} / 失手 ${this.enemyInterdictionMemory.failedRaids} / ${this.enemyInterdictionMemory.lastReasoning}`;
  }

  private enemyInterdictionStrategicPhase(): EnemyInterdictionStrategicPhaseState {
    this.refreshEnemyInterdictionDoctrine();
    const hasPacificationTransport = this.occupationSupplyTasks.some((task) => task.status === 'pending' || task.status === 'in-transit');
    const hasSupplyConvoy = this.commandQueue.some((command) => command.kind === 'supply' && command.supplyReserve > 0);
    const hasAttack = this.commandQueue.some((command) => command.kind === 'attack');
    const activeTargetCount = this.enemyInterdictionOrders.filter((order) => !order.resolved).length;
    let phase: EnemyInterdictionStrategicPhase = 'probing';
    let phaseLabel = '试探粮道';
    if (hasPacificationTransport) {
      phase = 'pacification-blockade';
      phaseLabel = '阻断安抚';
    } else if (hasSupplyConvoy) {
      phase = 'supply-strangulation';
      phaseLabel = '压迫粮线';
    } else if (hasAttack) {
      phase = 'field-army-harassment';
      phaseLabel = '牵制主力';
    } else if (activeTargetCount > 0) {
      phase = 'exploitation';
      phaseLabel = '追击余线';
    }

    return {
      phase,
      phaseLabel,
      doctrine: this.enemyInterdictionMemory.doctrine,
      activeTargetCount,
      selectedOrderId: this.selectedEnemyInterdictionOrder()?.id ?? '',
      pressureRegionCount: Object.values(this.enemyInterdictionMemory.pressureByRegion).filter((value) => value > 0).length,
      reasoning: this.enemyInterdictionMemory.lastReasoning
    };
  }

  private refreshEnemyInterdictionDoctrine(): void {
    const hasPacificationTransport = this.occupationSupplyTasks.some((task) => task.status === 'pending' || task.status === 'in-transit');
    const hasSupplyConvoy = this.commandQueue.some((command) => command.kind === 'supply' && command.supplyReserve > 0);
    const hasAttack = this.commandQueue.some((command) => command.kind === 'attack');
    if (hasPacificationTransport) {
      this.enemyInterdictionMemory.doctrine = 'stall-pacification';
      this.enemyInterdictionMemory.lastReasoning = '发现新附地区依赖安抚运输，优先切断治理补给';
    } else if (hasSupplyConvoy) {
      this.enemyInterdictionMemory.doctrine = 'cut-supply';
      this.enemyInterdictionMemory.lastReasoning = '发现运输队在途，优先打击粮道';
    } else if (hasAttack) {
      this.enemyInterdictionMemory.doctrine = 'bleed-army';
      this.enemyInterdictionMemory.lastReasoning = '发现战役军令，转为消耗主力';
    } else if (this.enemyInterdictionMemory.failedRaids > this.enemyInterdictionMemory.successfulRaids + 1) {
      this.enemyInterdictionMemory.doctrine = 'bleed-army';
      this.enemyInterdictionMemory.lastReasoning = '连续截粮失手，改为拖慢主力行军';
    }
  }

  private renderOutliner(): void {
    const list = document.getElementById('outliner-list');
    if (!list) return;
    const risky = [...this.dataset.regions].sort((a, b) => b.risk - a.risk).slice(0, 3);
    const route = this.createActiveRouteForecast(this.selectedRegion);
    const nextCommand = this.commandQueue[this.commandQueue.length - 1];
    const nextOccupation = this.occupationQueue[0];
    const nextEnemyInterdiction = this.enemyInterdictionOrders[0];
    list.innerHTML = [
      outlinerItem('高风险', `${risky[0].definition.name} 民变 ${Math.round(risky[0].risk)}%`, risky[0].definition.id),
      outlinerItem('行军中', `${route.army.name} → ${route.target.definition.name}`, route.target.definition.id),
      outlinerItem('经营', this.governanceQueue[0] ?? `${this.selectedRegion.recommendedPolicy?.name ?? '安抚'} / ${this.selectedRegion.definition.name}`, this.selectedRegion.definition.id),
      outlinerItem('后勤', nextCommand ? describeWarCommand(nextCommand) : `${route.target.definition.name} 需粮 ${route.occupationCost}`, nextCommand?.targetRegionId ?? route.target.definition.id),
      outlinerItem('截粮', nextEnemyInterdiction ? describeEnemyInterdictionOrder(nextEnemyInterdiction) : `${route.target.definition.name} 暂无敌方截粮队列`, nextEnemyInterdiction?.targetRegionId ?? route.target.definition.id),
      outlinerItem('占后', nextOccupation ? describeOccupationTask(nextOccupation) : `${route.target.definition.name} 暂无新附队列`, nextOccupation?.regionId ?? route.target.definition.id),
      outlinerItem('最新', this.operationLog[0], this.selectedRegion.definition.id)
    ].join('');

    list.querySelectorAll<HTMLButtonElement>('[data-region-id]').forEach((button) => {
      button.addEventListener('click', () => {
        const regionId = button.dataset.regionId;
        if (regionId) this.events.onSelectRegion(regionId);
      });
    });
  }

  private createActiveRouteForecast(targetCandidate: RegionViewModel): RouteForecast {
    const { from, target } = this.activeRouteEndpoints(targetCandidate);
    const waypoint = this.activeArmy.waypointRegionId ? this.dataset.regionById.get(this.activeArmy.waypointRegionId) : undefined;
    const safeWaypoint = waypoint && waypoint.definition.id !== from.definition.id && waypoint.definition.id !== target.definition.id
      ? waypoint
      : undefined;
    const estimate = this.estimateRouteMetrics(from, target, safeWaypoint);
    return {
      army: this.activeArmy,
      from,
      target,
      waypoint: safeWaypoint,
      supplyCost: estimate.supplyCost,
      turns: estimate.turns,
      contactChance: estimate.contactChance,
      occupationCost: estimate.occupationCost,
      interceptionRisk: estimate.interceptionRisk,
      routeCapacity: estimate.routeCapacity,
      routeUsage: estimate.routeUsage,
      roadClass: estimate.roadClass,
      bottleneckLabel: estimate.bottleneckLabel,
      terrainReason: estimate.terrainReason,
      summary: `从${from.definition.name}${safeWaypoint ? `经${safeWaypoint.definition.name}` : ''}出军至${target.definition.name}，${estimate.bottleneckLabel}，粮草消耗${estimate.supplyCost}，接敌${Math.round(estimate.contactChance)}%，${this.tacticalSummary(target).text}`
    };
  }

  private activeRouteEndpoints(targetCandidate: RegionViewModel): { from: RegionViewModel; target: RegionViewModel } {
    const from = this.dataset.regionById.get(this.activeArmy.fromRegionId) ?? this.selectedRegion;
    const target =
      this.mode === 'war' && targetCandidate.definition.id !== this.activeArmy.fromRegionId
        ? targetCandidate
        : this.dataset.regionById.get(this.activeArmy.targetRegionId) ?? targetCandidate;
    return { from, target };
  }

  private routeAlternativesFor(targetCandidate: RegionViewModel): RouteAlternative[] {
    const { from, target } = this.activeRouteEndpoints(targetCandidate);
    const candidates = this.routeWaypointCandidates(from, target);
    const alternatives: RouteAlternative[] = [
      this.createRouteAlternative(from, target)
    ];

    for (const waypoint of candidates) {
      alternatives.push(this.createRouteAlternative(from, target, waypoint));
    }

    const unique = new Map<string, RouteAlternative>();
    for (const alternative of alternatives) unique.set(alternative.id, alternative);
    const ranked = [...unique.values()]
      .sort((a, b) => Number(b.selected) - Number(a.selected) || b.score - a.score)
      .slice(0, 4);
    const best = [...unique.values()].sort((a, b) => b.score - a.score)[0];
    return ranked.map((alternative) => ({
      ...alternative,
      recommendation: alternative.selected ? '已采用' : alternative.id === best?.id ? '推荐' : alternative.recommendation
    }));
  }

  private routeWaypointCandidates(from: RegionViewModel, target: RegionViewModel): RegionViewModel[] {
    const candidateIds = new Set<string>();
    const pushCandidate = (id: string | undefined) => {
      if (!id || id === from.definition.id || id === target.definition.id) return;
      const region = this.dataset.regionById.get(id);
      if (!region || region.owner !== 'player') return;
      candidateIds.add(id);
    };

    pushCandidate(this.activeArmy.waypointRegionId);
    from.definition.neighbors.forEach(pushCandidate);
    target.definition.neighbors.forEach(pushCandidate);
    this.dataset.regions
      .filter((region) => region.owner === 'player')
      .sort((a, b) => this.waypointDetourDistance(from, target, a) - this.waypointDetourDistance(from, target, b))
      .slice(0, 5)
      .forEach((region) => pushCandidate(region.definition.id));

    return [...candidateIds]
      .map((id) => this.dataset.regionById.get(id))
      .filter((region): region is RegionViewModel => Boolean(region))
      .sort((a, b) => this.createRouteAlternative(from, target, b).score - this.createRouteAlternative(from, target, a).score)
      .slice(0, 3);
  }

  private createRouteAlternative(from: RegionViewModel, target: RegionViewModel, waypoint?: RegionViewModel): RouteAlternative {
    const estimate = this.estimateRouteMetrics(from, target, waypoint);
    const id = waypoint ? `via-${waypoint.definition.id}` : 'direct';
    const selected = waypoint
      ? this.activeArmy.waypointRegionId === waypoint.definition.id
      : !this.activeArmy.waypointRegionId;
    const capacityPressure = estimate.routeCapacity - estimate.routeUsage;
    const score = Math.round(capacityPressure * 16 - estimate.supplyCost - estimate.interceptionRisk * 1.15 - estimate.turns * 4);
    const recommendation = capacityPressure >= 2 && estimate.interceptionRisk < 45
      ? '宽裕'
      : estimate.interceptionRisk >= 65
        ? '高危'
        : '可用';
    return {
      id,
      label: waypoint ? '绕行' : '直达',
      routeLabel: `${from.definition.name}${waypoint ? `→${waypoint.definition.name}` : ''}→${target.definition.name}`,
      waypointRegionId: waypoint?.definition.id ?? '',
      capacity: estimate.routeCapacity,
      currentUsage: estimate.routeUsage,
      supplyCost: estimate.supplyCost,
      interceptionRisk: Math.round(estimate.interceptionRisk),
      turns: estimate.turns,
      bottleneckLabel: estimate.bottleneckLabel,
      terrainReason: estimate.terrainReason,
      networkLabel: estimate.networkLabel,
      score,
      selected,
      recommendation
    };
  }

  private estimateRouteMetrics(from: RegionViewModel, target: RegionViewModel, waypoint?: RegionViewModel): RouteMetricEstimate {
    const legs = this.routeLegEstimates(from, target, waypoint);
    const totalDistance = legs.reduce((sum, leg) => sum + leg.distance, 0);
    const modifier = this.tacticalModifier(target);
    const governance = this.aggregateRouteGovernance(legs);
    const supplyFactor = weightedSupplyFactor(legs);
    const bottleneck = mostRestrictiveLeg(legs);
    const waypointRisk = waypoint ? Math.max(0, waypoint.risk - 20) * 0.12 : 0;
    const waypointRelief = waypoint && waypoint.owner === 'player' ? -5 : 0;
    const baseSupplyCost = 16 + totalDistance * 3 + Math.max(0, target.risk - 15) * 0.4 + waypointRisk + Math.max(0, 70 - this.activeArmy.supply) * 0.12 + modifier.supplyDelta - governance.supplyRelief;
    const supplyCost = Math.max(8, Math.round(baseSupplyCost * supplyFactor));
    const contactChance = clamp(44 + target.risk * 0.6 + (target.owner === 'rival' ? 16 : 0) + Math.max(0, this.activeArmy.morale - 70) * 0.18 + modifier.contactDelta - (waypoint ? 3 : 0), 0, 96);
    const occupationCost = Math.max(12, Math.round(42 + target.definition.localPower * 0.8 + target.definition.population / 50000 + modifier.occupationDelta));
    const interceptionRisk = clamp(
      18 + (target.history?.weaponTraditions?.length ?? 0) * 7 + (target.definition.terrain.includes('mountain') ? 16 : 0) + Math.max(0, 72 - this.activeArmy.supply) * 0.25 + modifier.interceptionDelta + bottleneck.profile.interceptionModifier + waypointRisk + waypointRelief - governance.interdictionRelief,
      0,
      94
    );
    return {
      legs,
      supplyCost,
      turns: Math.max(1, Math.round(totalDistance / 2.2 + (waypoint ? 0.4 : 0))),
      contactChance,
      occupationCost,
      interceptionRisk,
      routeCapacity: Math.max(1, Math.min(...legs.map((leg) => leg.capacity))),
      routeUsage: Math.max(0, ...legs.map((leg) => leg.currentUsage)),
      roadClass: bottleneck.profile.roadClass,
      bottleneckLabel: bottleneck.profile.bottleneckLabel,
      terrainReason: routeTerrainReason(legs),
      networkLabel: routeNetworkLabel(legs)
    };
  }

  private routeLegEstimates(from: RegionViewModel, target: RegionViewModel, waypoint?: RegionViewModel): RouteLegEstimate[] {
    const nodes = waypoint ? [from, waypoint, target] : [from, target];
    const legs: RouteLegEstimate[] = [];
    for (let index = 0; index < nodes.length - 1; index += 1) {
      const legFrom = nodes[index];
      const legTarget = nodes[index + 1];
      const profile = this.routeTerrainProfile(legFrom, legTarget);
      legs.push({
        from: legFrom,
        target: legTarget,
        distance: Math.hypot(legFrom.shape.center.x - legTarget.shape.center.x, legFrom.shape.center.y - legTarget.shape.center.y),
        profile,
        capacity: this.routeCapacityLimit(legFrom.definition.id, legTarget.definition.id),
        currentUsage: this.routeCapacities.get(routeCapacityKey(legFrom.definition.id, legTarget.definition.id))?.currentUsage ?? 0
      });
    }
    return legs;
  }

  private aggregateRouteGovernance(legs: RouteLegEstimate[]): GovernanceLogisticsDelta {
    const deltas = legs.map((leg) => this.routeGovernanceLogisticsModifier(leg.from.definition.id, leg.target.definition.id));
    return {
      capacityBonus: Math.min(3, deltas.reduce((sum, delta) => sum + delta.capacityBonus, 0)),
      supplyRelief: Math.min(12, deltas.reduce((sum, delta) => sum + delta.supplyRelief, 0)),
      interdictionRelief: Math.min(18, deltas.reduce((sum, delta) => sum + delta.interdictionRelief, 0)),
      occupationBandwidthBonus: Math.min(12, deltas.reduce((sum, delta) => sum + delta.occupationBandwidthBonus, 0))
    };
  }

  private waypointDetourDistance(from: RegionViewModel, target: RegionViewModel, waypoint: RegionViewModel): number {
    return Math.hypot(from.shape.center.x - waypoint.shape.center.x, from.shape.center.y - waypoint.shape.center.y)
      + Math.hypot(waypoint.shape.center.x - target.shape.center.x, waypoint.shape.center.y - target.shape.center.y);
  }

  private applyRouteAlternative(alternativeId: string): void {
    const alternatives = this.routeAlternativesFor(this.selectedRegion);
    const alternative = alternatives.find((candidate) => candidate.id === alternativeId);
    if (!alternative) return;
    this.activeArmy.waypointRegionId = alternative.waypointRegionId || undefined;
    this.routePickMode = 'target';
    this.armyOrder.route = alternative.label === '直达' ? '直达主道' : `经${alternative.routeLabel.split('→')[1] ?? '中继'}绕行`;
    this.armyOrder.last = `路线方案 ${alternative.label}`;
    this.operationLog.unshift(`采用路线方案：${alternative.routeLabel}，容量 ${alternative.currentUsage}/${alternative.capacity}，补给 ${alternative.supplyCost}，截粮 ${alternative.interceptionRisk}%`);
    trimTo(this.operationLog, 5);
  }

  private currentRoutePressureCopy(): RoutePressureCopy {
    const route = this.createActiveRouteForecast(this.selectedRegion);
    if (this.enemyInterdictionOrders.length === 0 && this.lastResolvedRoutePressure) return this.lastResolvedRoutePressure;
    return this.createRoutePressureCopy(route, this.activeEnemyInterdictionOrder());
  }

  private createRoutePressureCopy(route: RouteForecast, order?: EnemyInterdictionOrder): RoutePressureCopy {
    const routeLine = `${route.army.name}：${route.from.definition.name} → ${route.target.definition.name}`;
    const routeShort = this.routeShortLabel(route);
    const supply = route.supplyCost;
    const contact = Math.round(route.contactChance);
    const occupation = route.occupationCost;
    const base = `${routeLine} / 补给 ${supply} / 接敌 ${contact}% / 占领 ${occupation}`;
    const compactBase = `${routeShort} | 补${supply} 接${contact}% 占${occupation}`;
    if (!order) {
      return {
        full: `${base} / 路线压力 暂无显形截粮`,
        compact: `${compactBase} | 无显形截粮`,
        detail: `位置详情：${routeShort}；敌方截粮点未显形，己方运输队沿主道推进。当前压力主要来自目标地形、补给余量和接敌风险。`,
        detailHtml: routePressureDetailHtml(routeShort, [
          { tone: 'enemy', label: '敌方截粮点', title: '未显形', body: '继续依靠侦察和前线斥候修正风险。' },
          { tone: 'friendly', label: '己方运输', title: '运输推进', body: '运输队沿主道推进，暂无护粮、改道、反斥候或诱敌反制。' },
          { tone: 'forecast', label: '预测', title: `接敌 ${contact}%`, body: `补给 ${supply}，占领代价 ${occupation}；压力来自地形、补给余量和目标秩序。` }
        ])
      };
    }
    const counter = order.lastCountermeasure ? ` / 反制 ${order.lastCountermeasure}` : ' / 反制 未下达';
    const compactCounter = order.lastCountermeasure ? order.lastCountermeasure : '未反制';
    return {
      full: `${base} / 截粮 ${enemyInterdictionStageName(order.stage)} 风险${order.risk}% 损${order.supplyDamage}${counter}`,
      compact: `${compactBase} | 截粮${enemyInterdictionStageShortName(order.stage)}${order.risk}% 损${order.supplyDamage} | ${compactCounter}`,
      ...this.createRoutePressureDetail(routeShort, order, supply, contact, occupation)
    };
  }

  private routeShortLabel(route: RouteForecast): string {
    const middle = route.waypoint ? `→${route.waypoint.definition.name}` : '';
    return `${route.army.name} ${route.from.definition.name}${middle}→${route.target.definition.name}`;
  }

  private createRoutePressureDetail(routeShort: string, order: EnemyInterdictionOrder, supply: number, contact: number, occupation: number): { detail: string; detailHtml: string } {
    const enemyPosition = enemyInterdictionPositionName(order.stage);
    const friendlyPosition = friendlyCountermeasurePositionName(order.lastCountermeasure, order.stage);
    const enemyLine = `敌方截粮点：${enemyPosition.label}，约在路线 ${enemyPosition.progress}% 处，${enemyInterdictionStageName(order.stage)}，尚余 ${order.remainingTurns}/${order.totalTurns} 回合。`;
    const counterEffect = friendlyCountermeasureEffect(order.lastCountermeasure);
    const friendlyLine = order.lastCountermeasure
      ? `己方${order.lastCountermeasure}：${friendlyPosition.label}，约在路线 ${friendlyPosition.progress}% 处，${counterEffect}。`
      : '己方反制：尚未下达，后勤页可选择护粮、改道、反斥候或诱敌。';
    const detail = `位置详情：${routeShort}；${enemyLine} ${friendlyLine} 当前风险 ${order.risk}%，预计损耗 ${order.supplyDamage}。`;
    const detailHtml = routePressureDetailHtml(routeShort, [
      {
        tone: 'enemy',
        label: '敌方截粮点',
        title: `${enemyInterdictionStageName(order.stage)} ${enemyPosition.progress}%`,
        body: `${enemyPosition.label}；尚余 ${order.remainingTurns}/${order.totalTurns} 回合，风险 ${order.risk}%，预计损耗 ${order.supplyDamage}。`
      },
      {
        tone: 'friendly',
        label: '己方反制',
        title: order.lastCountermeasure ? `${order.lastCountermeasure} ${friendlyPosition.progress}%` : '未反制',
        body: order.lastCountermeasure
          ? `${friendlyPosition.label}；${counterEffect}。`
          : '后勤页可选择护粮、改道、反斥候或诱敌。'
      },
      {
        tone: 'forecast',
        label: '预测',
        title: `补 ${supply} / 接敌 ${contact}%`,
        body: `占领代价 ${occupation}；若截粮继续推进，下一回合优先看补给余量和护粮状态。`
      }
    ]);
    return { detail, detailHtml };
  }

  private renderRoutePressure(copy: RoutePressureCopy): void {
    const summary = document.getElementById('route-summary');
    if (summary) {
      summary.innerHTML = `<span class="route-summary-full">${escapeHtml(copy.full)}</span><span class="route-summary-compact">${escapeHtml(copy.compact)}</span>`;
      summary.setAttribute('aria-label', copy.full);
      summary.setAttribute('title', copy.detail);
    }
    const detail = document.getElementById('route-pressure-detail');
    if (detail) detail.innerHTML = copy.detailHtml;
  }

  private tacticalSummary(target: RegionViewModel): { score: string; text: string } {
    return this.tacticalSummaryForArmy(this.activeArmy, target);
  }

  private tacticalSummaryForArmy(army: ArmyViewModel, target: RegionViewModel): { score: string; text: string } {
    const modifier = this.tacticalModifierForArmy(army, target);
    const score = Math.round(modifier.contactDelta - modifier.supplyDelta - modifier.interceptionDelta * 0.5 - modifier.occupationDelta * 0.4);
    const signedScore = score >= 0 ? `+${score}` : String(score);
    const general = this.generalForArmy(army);
    const dominant = dominantUnit(army.unitMix, this.dataset.units);
    return {
      score: signedScore,
      text: `${general?.specialAbilityName ?? '常规统率'} / ${dominant?.name ?? army.unit.name}：补给${signedNumber(modifier.supplyDelta)}，接敌${signedNumber(modifier.contactDelta)}，截粮${signedNumber(modifier.interceptionDelta)}，占领${signedNumber(modifier.occupationDelta)}`
    };
  }

  private tacticalModifier(target: RegionViewModel): TacticalModifier {
    return this.tacticalModifierForArmy(this.activeArmy, target);
  }

  private tacticalModifierForArmy(army: ArmyViewModel, target: RegionViewModel): TacticalModifier {
    const general = this.generalForArmy(army);
    const mix = army.unitMix;
    const unitBonus = general
      ? Object.entries(mix).reduce((sum, [unitId, share]) => sum + (general.unitBonus[unitId] ?? 0) * share / 100, 0)
      : 0;
    const terrainTags = [target.definition.terrain, target.geography.kind, ...(target.history?.geographyTags ?? [])];
    const terrainBonus = general
      ? Math.max(0, ...terrainTags.map((tag) => general.terrainBonus[tag] ?? 0))
      : 0;
    const cavalryShare = (mix.cavalry ?? 0) + (mix.frontier_cavalry ?? 0);
    const crossbowShare = mix.crossbowmen ?? 0;
    const siegeShare = mix.siege_engineer ?? 0;
    const militaryLift = general ? (general.military - 88) / 2.5 : 0;
    const supplyMaster = general?.specialAbility === 'supply_master' ? -8 : 0;
    const raidLift = ['deep_raid', 'lightning_strike'].includes(general?.specialAbility ?? '') ? -6 : 0;

    return {
      supplyDelta: Math.round(supplyMaster - unitBonus / 5 + siegeShare * 0.08 + cavalryShare * 0.04),
      contactDelta: Math.round(militaryLift + terrainBonus / 5 + unitBonus / 4 + cavalryShare * 0.06),
      interceptionDelta: Math.round(raidLift - terrainBonus / 6 - crossbowShare * 0.08 + siegeShare * 0.08),
      occupationDelta: Math.round(-siegeShare * 0.16 - unitBonus / 6 + Math.max(0, 78 - army.morale) * 0.08)
    };
  }

  private applyAction(action: UiAction): void {
    const region = this.selectedRegion;
    switch (action) {
      case 'save_slot_1':
        this.saveGameToSlot('slot_1');
        break;
      case 'save_slot_2':
        this.saveGameToSlot('slot_2');
        break;
      case 'save_slot_3':
        this.saveGameToSlot('slot_3');
        break;
      case 'load_slot_1':
        this.loadGameFromSlot('slot_1');
        break;
      case 'load_slot_2':
        this.loadGameFromSlot('slot_2');
        break;
      case 'load_slot_3':
        this.loadGameFromSlot('slot_3');
        break;
      case 'delete_slot_1':
        this.deleteGameSlot('slot_1');
        break;
      case 'delete_slot_2':
        this.deleteGameSlot('slot_2');
        break;
      case 'delete_slot_3':
        this.deleteGameSlot('slot_3');
        break;
      case 'governance_policy':
        this.applyGovernancePolicy(region);
        break;
      case 'governance_build':
        this.applyConstruction(region);
        break;
      case 'governance_reinforce':
        this.applyReinforcement(region);
        break;
      case 'governance_relief':
        this.spend('food', 22);
        region.risk = clamp(region.risk - 8, 0, 100);
        region.legitimacy = clamp(region.legitimacy + 2, 0, 100);
        this.nationState.legitimacy = clamp(this.nationState.legitimacy + 1, 0, 100);
        this.enqueueGovernance(`${region.definition.name}：开仓赈济，民变风险降至 ${Math.round(region.risk)}%`);
        break;
      case 'governance_registry':
        this.spend('money', 12);
        region.integration = clamp(region.integration + 4, 0, 100);
        region.contribution = clamp(region.contribution + 2, 0, 100);
        region.risk = clamp(region.risk + 1, 0, 100);
        this.enqueueGovernance(`${region.definition.name}：编户清丈，贡献 ${Math.round(region.contribution)}%，整合 ${Math.round(region.integration)}%`);
        break;
      case 'governance_focus_grain':
        this.applyGovernanceFocus(region, 'grain');
        break;
      case 'governance_focus_tax':
        this.applyGovernanceFocus(region, 'tax');
        break;
      case 'governance_focus_military':
        this.applyGovernanceFocus(region, 'military');
        break;
      case 'governance_focus_frontier':
        this.applyGovernanceFocus(region, 'frontier');
        break;
      case 'governance_focus_legitimacy':
        this.applyGovernanceFocus(region, 'legitimacy');
        break;
      case 'governance_focus_relief':
        this.applyGovernanceFocus(region, 'relief');
        break;
      case 'governance_labor_balanced':
        this.applyGovernanceLabor(region, 'balanced');
        break;
      case 'governance_labor_grain':
        this.applyGovernanceLabor(region, 'grain');
        break;
      case 'governance_labor_tax':
        this.applyGovernanceLabor(region, 'tax');
        break;
      case 'governance_labor_military':
        this.applyGovernanceLabor(region, 'military');
        break;
      case 'governance_labor_stability':
        this.applyGovernanceLabor(region, 'stability');
        break;
      case 'governance_advance_turn':
        this.advanceGovernanceTurn();
        break;
      case 'war_deploy':
        this.enqueueWarCommand('deploy', region, '部署军府', `${region.definition.name}：前线军府部署，补给节点待建成`);
        this.spend('money', 18);
        break;
      case 'war_supply':
        this.enqueueWarCommand('supply', region, '运输队', `${region.definition.name}：运输队出发，粮草 -24，等待分段抵达`);
        this.spend('food', 24);
        break;
      case 'war_scout':
        this.enqueueWarCommand('scout', region, '斥候探路', `${region.definition.name}：斥候探路，伏击与截粮点进入侦察排程`);
        this.spend('money', 6);
        break;
      case 'war_fortify':
        this.enqueueWarCommand('fortify', region, '加固兵站', `${region.definition.name}：兵站加固，运输路线进入施工排程`);
        this.spend('money', 10);
        this.spend('food', 10);
        break;
      case 'war_attack':
        this.enqueueWarCommand('attack', region, '启动战役', `${region.definition.name}：攻占预案启动，战后进入军管与安抚链`);
        this.spend('food', 36);
        this.spend('money', 12);
        region.risk = clamp(region.risk + 5, 0, 100);
        break;
      case 'war_advance_turn':
        this.advanceWarTurn();
        break;
      case 'war_counter_escort':
        this.applyInterdictionCountermeasure('escort');
        break;
      case 'war_counter_reroute':
        this.applyInterdictionCountermeasure('reroute');
        break;
      case 'war_counter_scout':
        this.applyInterdictionCountermeasure('counter-scout');
        break;
      case 'war_counter_decoy':
        this.applyInterdictionCountermeasure('decoy');
        break;
      case 'route_blockade_guard':
        this.guardRouteBlockade();
        break;
      case 'route_blockade_clear':
        this.clearRouteBlockade();
        break;
      case 'occupation_aftercare':
        this.advanceOccupationQueue();
        break;
      case 'army_order_balanced':
        this.applyArmyOrder('稳进压迫', '中军合进', '主道补给', -5, 1, `${this.activeArmy.name}：稳进压迫，保持补给线并推进行军线`);
        break;
      case 'army_order_forced_march':
        this.applyArmyOrder('急行抢道', '纵队急行', '轻装绕行', -14, -5, `${this.activeArmy.name}：急行抢道，补给消耗增加，抢占接敌节奏`);
        region.risk = clamp(region.risk + 2, 0, 100);
        break;
      case 'army_order_defensive':
        this.applyArmyOrder('扎营固守', '方营拒守', '兵站固守', -4, 5, `${this.activeArmy.name}：扎营固守，军心回稳，推进速度下降`);
        break;
      case 'army_order_flank':
        this.applyArmyOrder('侧翼牵制', '两翼展开', '山道斥候', -9, 2, `${this.activeArmy.name}：侧翼牵制，暴露敌军侧翼但运输更紧`);
        region.risk = clamp(region.risk + 1, 0, 100);
        break;
      case 'army_order_reserve':
        this.applyArmyOrder('收拢预备队', '后队整补', '缓行补给', 4, 4, `${this.activeArmy.name}：预备队收拢，补给与军心小幅恢复`);
        break;
      case 'army_split':
        this.splitActiveArmy();
        break;
      case 'army_merge':
        this.mergeActiveArmy();
        break;
      case 'army_general_next':
        this.rotateGeneral();
        break;
      case 'army_mix_balanced':
        this.applyUnitMix({ infantry: 40, cavalry: 36, crossbowmen: 24 }, '步骑均衡');
        break;
      case 'army_mix_cavalry':
        this.applyUnitMix({ cavalry: 72, frontier_cavalry: 18, infantry: 10 }, '骑兵突进');
        break;
      case 'army_mix_crossbow':
        this.applyUnitMix({ crossbowmen: 52, infantry: 34, cavalry: 14 }, '弩步守正');
        break;
      case 'army_mix_siege':
        this.applyUnitMix({ siege_engineer: 46, infantry: 34, crossbowmen: 20 }, '攻城配属');
        break;
      case 'route_waypoint_clear':
        this.activeArmy.waypointRegionId = undefined;
        this.routePickMode = 'target';
        this.logisticsQueue.unshift(`${this.activeArmy.name}：中继点已清除，路线回到直达目标`);
        trimTo(this.logisticsQueue, 4);
        break;
      case 'route_queue_promote':
        this.promoteLogisticsQueue();
        break;
      case 'route_queue_cancel':
        this.cancelLogisticsQueue();
        break;
      case 'transport_convoy_promote':
        this.promoteTransportConvoy();
        break;
      case 'transport_convoy_cancel':
        this.cancelTransportConvoy();
        break;
      case 'occupation_supply_promote':
        this.promoteOccupationSupplyTask();
        break;
      case 'occupation_supply_cancel':
        this.cancelOccupationSupplyTask();
        break;
    }

    this.events.onAction(action, region);
    this.events.onStateMutated();
    this.render();
  }

  private enqueueWarCommand(kind: WarCommandKind, region: RegionViewModel, label: string, logText: string): void {
    const command = this.createWarCommand(kind, region, label);
    this.registerRouteCapacity(command);
    command.interceptionRisk = this.commandInterceptionRisk(command);
    if (command.kind === 'supply') {
      this.createTransportConvoy(command);
    }
    this.commandQueue.unshift(command);
    this.logisticsQueue.unshift(`${logText}，${command.bottleneckLabel}，预计 ${command.totalTurns} 回合，截粮风险 ${command.interceptionRisk}%，路线 ${command.routeUsage}/${command.routeCapacity}`);
    this.operationLog.unshift(`${logText}。${this.activeArmy.name}已纳入第 ${this.currentWarTurn} 回合军令。`);
    trimTo(this.commandQueue, 6);
    trimTo(this.logisticsQueue, 4);
    trimTo(this.operationLog, 5);
  }

  private createWarCommand(kind: WarCommandKind, region: RegionViewModel, label: string): WarCommand {
    const forecast = this.createActiveRouteForecast(region);
    const waypoint = this.activeArmy.waypointRegionId ? this.dataset.regionById.get(this.activeArmy.waypointRegionId) : undefined;
    const totalTurns = commandTurns(kind, forecast);
    const supplyReserve = commandSupplyReserve(kind, forecast);
    const routeLegs = this.commandRouteLegsFromRegions(forecast.from, forecast.target, forecast.waypoint);
    return {
      id: `war_command_${this.commandCounter++}`,
      kind,
      label,
      armyId: this.activeArmy.id,
      fromRegionId: forecast.from.definition.id,
      targetRegionId: region.definition.id,
      targetName: region.definition.name,
      fromName: forecast.from.definition.name,
      waypointRegionId: this.activeArmy.waypointRegionId ?? undefined,
      waypointName: waypoint?.definition.name,
      createdTurn: this.currentWarTurn,
      remainingTurns: totalTurns,
      totalTurns,
      interceptionRisk: Math.round(forecast.interceptionRisk),
      supplyReserve,
      plannedSupplyReserve: supplyReserve,
      deliveredSupply: 0,
      completedSegments: 0,
      segmentCount: commandSegmentCount(kind, totalTurns),
      routeCapacity: forecast.routeCapacity ?? 1,
      routeUsage: forecast.routeUsage ?? 0,
      stationBonus: 0,
      roadClass: (forecast.roadClass as RouteRoadClass | undefined) ?? 'open-road',
      bottleneckLabel: forecast.bottleneckLabel ?? '平原官道',
      convoyPriority: kind === 'supply' ? 1 : undefined,
      convoyOrder: kind === 'supply' ? this.transportConvoyCounter : undefined,
      routeLegs
    };
  }

  private createTransportConvoy(command: WarCommand): TransportConvoy {
    const convoy: TransportConvoy = {
      id: `运输队-${this.transportConvoyCounter++}`,
      commandId: command.id,
      armyId: command.armyId,
      routeLabel: commandRouteLabel(command),
      fromRegionId: command.fromRegionId,
      targetRegionId: command.targetRegionId,
      status: 'queued',
      priority: command.convoyPriority ?? 1,
      orderIndex: command.convoyOrder ?? this.transportConvoyCounter,
      createdTurn: this.currentWarTurn,
      plannedSupplyReserve: command.plannedSupplyReserve,
      supplyReserve: command.supplyReserve,
      deliveredSupply: command.deliveredSupply,
      completedSegments: command.completedSegments,
      segmentCount: command.segmentCount,
      routeUsage: command.routeUsage,
      routeCapacity: command.routeCapacity,
      roadClass: command.roadClass,
      bottleneckLabel: command.bottleneckLabel,
      routeLegs: command.routeLegs.map((leg) => ({ ...leg }))
    };
    command.convoyId = convoy.id;
    this.transportConvoys.unshift(convoy);
    trimTo(this.transportConvoys, 8);
    return convoy;
  }

  private syncTransportConvoy(command: WarCommand, status?: TransportConvoyStatus): void {
    if (!command.convoyId) return;
    const convoy = this.transportConvoys.find((candidate) => candidate.id === command.convoyId);
    if (!convoy) return;
    if (status) {
      convoy.status = status;
    } else if (convoy.status !== 'delivered' && convoy.status !== 'cancelled' && command.completedSegments > 0) {
      convoy.status = 'moving';
    }
    convoy.priority = command.convoyPriority ?? convoy.priority;
    convoy.orderIndex = command.convoyOrder ?? convoy.orderIndex;
    convoy.supplyReserve = command.supplyReserve;
    convoy.deliveredSupply = command.deliveredSupply;
    convoy.completedSegments = command.completedSegments;
    convoy.routeUsage = command.routeUsage;
    convoy.routeCapacity = command.routeCapacity;
    convoy.roadClass = command.roadClass;
    convoy.bottleneckLabel = command.bottleneckLabel;
    convoy.routeLegs = command.routeLegs.map((leg) => ({ ...leg }));
  }

  private activeTransportConvoys(): TransportConvoy[] {
    return this.transportConvoys
      .filter((convoy) => convoy.status === 'queued' || convoy.status === 'moving')
      .sort((a, b) => b.priority - a.priority || a.orderIndex - b.orderIndex);
  }

  private activeRouteBlockades(): RouteBlockade[] {
    return this.routeBlockades
      .filter((blockade) => blockade.status !== 'cleared')
      .sort((a, b) => b.strength - a.strength || b.guardStrength - a.guardStrength);
  }

  private advanceWarTurn(): void {
    this.currentWarTurn += 1;
    const hadEnemyInterdiction = this.enemyInterdictionOrders.length > 0;
    this.advanceEnemyInterdictionOrders();
    const resolved: string[] = this.processOccupationSupplyAutomation();
    if (this.commandQueue.length === 0) {
      if (resolved.length > 0) {
        this.operationLog.unshift(`第 ${this.currentWarTurn} 回合后勤：${resolved[0]}`);
        trimTo(this.operationLog, 5);
      } else if (!hadEnemyInterdiction && this.enemyInterdictionOrders.length === 0) {
        this.operationLog.unshift(`第 ${this.currentWarTurn} 回合：暂无在途军令，前线维持戒备。`);
        trimTo(this.operationLog, 5);
      }
      return;
    }

    const interdictionTarget = this.chooseInterdictionTarget(this.commandQueue);
    for (let index = this.commandQueue.length - 1; index >= 0; index -= 1) {
      const command = this.commandQueue[index];
      command.remainingTurns = Math.max(0, command.remainingTurns - 1);
      if (!command.alert && interdictionTarget?.id === command.id) {
        const alert = this.applyInterdiction(command);
        resolved.push(alert);
      }

      const segmentNote = this.advanceSupplySegment(command);
      if (segmentNote) resolved.push(segmentNote);

      if (command.remainingTurns <= 0) {
        resolved.push(this.resolveWarCommand(command));
        this.releaseRouteCapacity(command);
        this.commandQueue.splice(index, 1);
      }
    }

    if (resolved.length === 0) {
      const next = this.commandQueue[this.commandQueue.length - 1];
      this.operationLog.unshift(next ? `第 ${this.currentWarTurn} 回合：${next.label}继续推进，尚余 ${next.remainingTurns} 回合。` : `第 ${this.currentWarTurn} 回合：前线暂无异动。`);
    } else {
      this.operationLog.unshift(`第 ${this.currentWarTurn} 回合结算：${resolved[0]}`);
    }
    trimTo(this.operationLog, 5);
  }

  private shouldInterdict(command: WarCommand): boolean {
    const target = this.dataset.regionById.get(command.targetRegionId);
    const enemyPressure = target?.owner === 'rival' ? 8 : 0;
    const waypointRelief = command.waypointName ? -6 : 0;
    const kindPressure = command.kind === 'supply' ? 10 : command.kind === 'attack' ? 7 : command.kind === 'deploy' ? 3 : 0;
    const congestionPressure = command.routeUsage > command.routeCapacity ? 10 : command.routeUsage === command.routeCapacity ? 4 : 0;
    const chokePressure = this.commandChokePoint(command).networkLabel ? 8 : 0;
    const occupationPressure = this.occupationSupplyTasks.some((task) => task.regionId === command.targetRegionId && task.status !== 'delivered') ? 6 : 0;
    const pressure = command.interceptionRisk + enemyPressure + waypointRelief + kindPressure + congestionPressure + chokePressure + occupationPressure + Math.max(0, command.totalTurns - command.remainingTurns) * 2;
    const threshold = command.kind === 'supply' ? 18 : command.kind === 'attack' ? 34 : command.kind === 'deploy' ? 46 : 58;
    return pressure >= threshold;
  }

  private chooseInterdictionTarget(commands: WarCommand[]): WarCommand | undefined {
    this.refreshEnemyInterdictionDoctrine();
    return commands
      .filter((command) => !command.alert && this.shouldInterdict(command))
      .sort((a, b) => this.interdictionTargetScore(b) - this.interdictionTargetScore(a))[0];
  }

  private interdictionTargetScore(command: WarCommand): number {
    const kindValue = command.kind === 'supply' ? 28 : command.kind === 'attack' ? 20 : command.kind === 'deploy' ? 9 : 4;
    const reserveValue = command.kind === 'supply' ? command.supplyReserve * 0.45 : command.supplyReserve * 0.18;
    const congestionValue = command.routeUsage > command.routeCapacity ? 18 : command.routeUsage === command.routeCapacity ? 8 : 0;
    const occupationValue = this.occupationSupplyTasks.some((task) => task.regionId === command.targetRegionId && task.status !== 'delivered') ? 12 : 0;
    const doctrineValue =
      this.enemyInterdictionMemory.doctrine === 'cut-supply' && command.kind === 'supply' ? 30 :
        this.enemyInterdictionMemory.doctrine === 'bleed-army' && command.kind === 'attack' ? 24 :
          this.enemyInterdictionMemory.doctrine === 'stall-pacification' && occupationValue > 0 ? 28 : 0;
    const rememberedPressure = Math.min(22, this.enemyInterdictionMemory.pressureByRegion[command.targetRegionId] ?? 0);
    const repeatTarget = this.enemyInterdictionMemory.lastTargetRegionId === command.targetRegionId ? 6 : 0;
    const stationRelief = command.stationBonus > 0 ? -6 : 0;
    const convoyValue = command.kind === 'supply' ? (command.convoyPriority ?? 1) * 4 : 0;
    const choke = this.commandChokePoint(command);
    const networkValue = choke.networkLabel ? 20 : 0;
    const chokeValue = choke.roadClass === 'pass-bottleneck' ? 16 : choke.roadClass === 'water-network' ? 10 : choke.roadClass === 'frontier-track' ? 8 : 4;
    return command.interceptionRisk + kindValue + reserveValue + congestionValue + occupationValue + doctrineValue + rememberedPressure + repeatTarget + stationRelief + convoyValue + networkValue + chokeValue;
  }

  private commandChokePoint(command: WarCommand): CommandRouteLeg {
    const legs = this.commandRouteLegs(command);
    if (legs.length === 0) {
      return {
        routeId: routeCapacityKey(command.fromRegionId, command.targetRegionId),
        fromRegionId: command.fromRegionId,
        toRegionId: command.targetRegionId,
        fromName: command.fromName,
        toName: command.targetName,
        routeUsage: command.routeUsage,
        routeCapacity: command.routeCapacity,
        roadClass: command.roadClass,
        bottleneckLabel: command.bottleneckLabel,
        terrainReason: command.bottleneckLabel
      };
    }
    return [...legs].sort((a, b) => Number(Boolean(b.networkLabel)) - Number(Boolean(a.networkLabel)) || a.routeCapacity - b.routeCapacity || b.routeUsage - a.routeUsage)[0];
  }

  private applyInterdiction(command: WarCommand): string {
    const army = this.dataset.armies.find((candidate) => candidate.id === command.armyId) ?? this.activeArmy;
    const loss = Math.max(5, Math.round(command.interceptionRisk / 8));
    army.supply = clamp(army.supply - loss, 0, 100);
    command.supplyReserve = Math.max(0, command.supplyReserve - loss * 2);
    this.syncTransportConvoy(command, command.kind === 'supply' ? 'moving' : undefined);
    const order = this.createOrRefreshEnemyInterdiction(command, loss);
    const blockade = this.createOrRefreshRouteBlockade(order);
    this.recordEnemyInterdictionMemory(command, true);
    const alert = `敌方截粮命令：${order.chokePointLabel}显形，${blockade.id}封锁建立，${order.routeLabel}，${army.name}补给 -${loss}，运输余量 ${command.supplyReserve}`;
    command.alert = alert;
    this.refreshInterceptionAlert();
    this.operationLog.unshift(`第 ${this.currentWarTurn} 回合：${alert}`);
    trimTo(this.operationLog, 5);
    return alert;
  }

  private recordEnemyInterdictionMemory(command: WarCommand | EnemyInterdictionOrder, success: boolean): void {
    const targetRegionId = command.targetRegionId;
    const current = this.enemyInterdictionMemory.pressureByRegion[targetRegionId] ?? 0;
    this.enemyInterdictionMemory.pressureByRegion[targetRegionId] = clamp(current + (success ? 10 : -8), 0, 36);
    this.enemyInterdictionMemory.lastTargetRegionId = targetRegionId;
    if (success) {
      this.enemyInterdictionMemory.successfulRaids += 1;
      this.enemyInterdictionMemory.lastReasoning = `上次打击 ${command.targetName} 有效，保留该线压力记忆`;
    } else {
      this.enemyInterdictionMemory.failedRaids += 1;
      this.enemyInterdictionMemory.lastReasoning = `上次打击 ${command.targetName} 被反制，降低该线优先级`;
    }
  }

  private advanceSupplySegment(command: WarCommand): string | null {
    if (command.kind !== 'supply' || command.remainingTurns <= 0) return null;
    const army = this.dataset.armies.find((candidate) => candidate.id === command.armyId) ?? this.activeArmy;
    const targetCompletedSegments = Math.min(
      command.segmentCount - 1,
      Math.floor(((command.totalTurns - command.remainingTurns) * command.segmentCount) / Math.max(1, command.totalTurns))
    );
    if (targetCompletedSegments <= command.completedSegments || command.supplyReserve <= 0) return null;

    let deliveredThisTurn = 0;
    const congestionFactor = command.routeUsage > command.routeCapacity ? 0.62 : command.routeUsage === command.routeCapacity ? 0.84 : 1;
    const convoy = command.convoyId ? this.transportConvoys.find((candidate) => candidate.id === command.convoyId) : undefined;
    const priorityFactor = convoy ? Math.min(1.22, 1 + convoy.priority * 0.04) : 1;
    const segmentQuota = Math.max(4, Math.round((command.plannedSupplyReserve / Math.max(1, command.segmentCount) + command.stationBonus * 0.25) * congestionFactor * priorityFactor));
    while (command.completedSegments < targetCompletedSegments && command.supplyReserve > 0) {
      const delivered = Math.min(command.supplyReserve, segmentQuota);
      command.supplyReserve = Math.max(0, command.supplyReserve - delivered);
      command.deliveredSupply += delivered;
      deliveredThisTurn += delivered;
      command.completedSegments += 1;
    }

    if (deliveredThisTurn <= 0) return null;
    army.supply = clamp(army.supply + deliveredThisTurn, 0, 100);
    const note = `${army.name}：运输队第 ${command.completedSegments}/${command.segmentCount} 段抵达，补给 +${deliveredThisTurn}，余量 ${command.supplyReserve}，路线 ${command.routeUsage}/${command.routeCapacity}`;
    this.syncTransportConvoy(command, 'moving');
    this.logisticsQueue.unshift(note);
    trimTo(this.logisticsQueue, 4);
    return note;
  }

  private createOrRefreshEnemyInterdiction(command: WarCommand, supplyDamage: number): EnemyInterdictionOrder {
    this.lastCountermeasureSummary = '';
    this.lastResolvedRoutePressure = null;
    const routeLabel = commandRouteLabel(command);
    const choke = this.commandChokePoint(command);
    const chokePointLabel = `${choke.networkLabel ?? choke.bottleneckLabel} ${choke.fromName}→${choke.toName}`;
    const existing = this.enemyInterdictionOrders.find((order) => order.armyId === command.armyId && order.targetRegionId === command.targetRegionId);
    if (existing) {
      existing.stage = 'planning';
      existing.remainingTurns = Math.max(existing.remainingTurns, 3);
      existing.totalTurns = Math.max(existing.totalTurns, 3);
      existing.risk = Math.max(existing.risk, Math.round(command.interceptionRisk));
      existing.supplyDamage = Math.max(existing.supplyDamage, supplyDamage);
      existing.routeLabel = routeLabel;
      existing.chokeRouteId = choke.routeId;
      existing.chokePointLabel = chokePointLabel;
      existing.chokeNetworkLabel = choke.networkLabel ?? '';
      existing.chokeReason = choke.terrainReason;
      this.enemyInterdictionOrders.splice(this.enemyInterdictionOrders.indexOf(existing), 1);
      this.enemyInterdictionOrders.unshift(existing);
      return existing;
    }

    const order: EnemyInterdictionOrder = {
      id: `enemy_interdiction_${this.enemyInterdictionCounter++}`,
      armyId: command.armyId,
      targetRegionId: command.targetRegionId,
      targetName: command.targetName,
      routeLabel,
      chokeRouteId: choke.routeId,
      chokePointLabel,
      chokeNetworkLabel: choke.networkLabel ?? '',
      chokeReason: choke.terrainReason,
      stage: 'planning',
      remainingTurns: 3,
      totalTurns: 3,
      risk: Math.round(command.interceptionRisk),
      supplyDamage,
      createdTurn: this.currentWarTurn
    };
    this.enemyInterdictionOrders.unshift(order);
    trimTo(this.enemyInterdictionOrders, 4);
    return order;
  }

  private createOrRefreshRouteBlockade(order: EnemyInterdictionOrder): RouteBlockade {
    const [fromRegionId, toRegionId] = order.chokeRouteId.split('->');
    const network = this.strategicRouteNetworkForLeg(fromRegionId || order.targetRegionId, toRegionId || order.targetRegionId);
    const rules = network?.blockade ?? DEFAULT_ROUTE_BLOCKADE_RULES;
    const existing = this.routeBlockades.find((blockade) => blockade.chokeRouteId === order.chokeRouteId && blockade.status !== 'cleared');
    if (existing) {
      existing.orderId = order.id;
      existing.targetRegionId = order.targetRegionId;
      existing.chokePointLabel = order.chokePointLabel;
      existing.networkId = network?.id ?? existing.networkId;
      existing.networkLabel = order.chokeNetworkLabel || network?.label || existing.networkLabel;
      existing.status = existing.guardStrength > 0 ? 'guarded' : 'enemy-blockade';
      existing.strength = clamp(existing.strength + rules.refreshStrengthGain, 0, 100);
      existing.lastAction = '敌方持续压迫瓶颈';
      return existing;
    }

    const blockade: RouteBlockade = {
      id: `瓶颈封锁-${this.routeBlockadeCounter++}`,
      orderId: order.id,
      chokeRouteId: order.chokeRouteId,
      fromRegionId: fromRegionId || order.targetRegionId,
      toRegionId: toRegionId || order.targetRegionId,
      targetRegionId: order.targetRegionId,
      chokePointLabel: order.chokePointLabel,
      networkId: network?.id,
      networkLabel: order.chokeNetworkLabel || network?.label || '',
      status: 'enemy-blockade',
      strength: clamp(order.risk, rules.initialStrengthFloor, 100),
      guardStrength: 0,
      createdTurn: this.currentWarTurn,
      lastAction: '敌方截粮队建立瓶颈封锁'
    };
    this.routeBlockades.unshift(blockade);
    trimTo(this.routeBlockades, 6);
    return blockade;
  }

  private selectedRouteBlockade(): RouteBlockade | undefined {
    if (this.selectedLogisticsObjectId) {
      const selected = this.routeBlockades.find((blockade) => blockade.id === this.selectedLogisticsObjectId && blockade.status !== 'cleared');
      if (selected) return selected;
    }
    const selectedOrder = this.selectedEnemyInterdictionOrder();
    if (selectedOrder) {
      const byOrder = this.routeBlockades.find((blockade) => blockade.orderId === selectedOrder.id && blockade.status !== 'cleared');
      if (byOrder) return byOrder;
    }
    return this.activeRouteBlockades()[0];
  }

  private guardRouteBlockade(): void {
    const blockade = this.selectedRouteBlockade();
    if (!blockade) {
      this.operationLog.unshift('瓶颈守备：当前没有需要守备的封锁点。');
      trimTo(this.operationLog, 5);
      return;
    }
    const rules = this.routeBlockadeRulesFor(blockade);
    this.spend('food', rules.guardFoodCost);
    this.spend('money', rules.guardMoneyCost);
    blockade.status = 'guarded';
    blockade.guardStrength = clamp(blockade.guardStrength + rules.guardStrengthGain, 0, 100);
    blockade.strength = clamp(blockade.strength - rules.guardBlockadeReduction, 0, 100);
    blockade.lastAction = '已加派守备队压住封锁点';
    const order = this.enemyInterdictionOrders.find((candidate) => candidate.id === blockade.orderId);
    if (order) {
      order.risk = clamp(order.risk - rules.guardRiskReduction, 0, 100);
      order.supplyDamage = Math.max(1, order.supplyDamage - rules.guardDamageReduction);
      order.lastCountermeasure = '瓶颈守备';
    }
    this.selectedLogisticsObjectId = blockade.id;
    this.lastCountermeasureSummary = '瓶颈守备';
    this.refreshInterceptionAlert();
    this.operationLog.unshift(`瓶颈守备：${blockade.chokePointLabel} 守备 ${blockade.guardStrength}，封锁强度 ${blockade.strength}`);
    trimTo(this.operationLog, 5);
  }

  private clearRouteBlockade(): void {
    const blockade = this.selectedRouteBlockade();
    if (!blockade) {
      this.operationLog.unshift('清除封锁：当前没有可清除的瓶颈封锁。');
      trimTo(this.operationLog, 5);
      return;
    }
    const rules = this.routeBlockadeRulesFor(blockade);
    this.spend('food', rules.clearFoodCost);
    this.spend('money', rules.clearMoneyCost);
    blockade.status = 'cleared';
    blockade.guardStrength = clamp(blockade.guardStrength + rules.clearGuardStrengthGain, 0, 100);
    blockade.strength = 0;
    blockade.lastAction = '封锁已被清除，路线恢复通行';
    const order = this.enemyInterdictionOrders.find((candidate) => candidate.id === blockade.orderId);
    if (order) {
      order.risk = clamp(order.risk - rules.clearRiskReduction, 0, 100);
      order.supplyDamage = 0;
      order.lastCountermeasure = '清除封锁';
      order.resolved = true;
      order.stage = 'resolved';
      this.recordEnemyInterdictionMemory(order, false);
    }
    this.selectedLogisticsObjectId = '';
    this.lastCountermeasureSummary = '清除封锁';
    this.refreshInterceptionAlert();
    this.operationLog.unshift(`清除封锁：${blockade.chokePointLabel} 已恢复通行`);
    trimTo(this.operationLog, 5);
  }

  private advanceEnemyInterdictionOrders(): void {
    if (this.enemyInterdictionOrders.length === 0) {
      this.latestInterceptionAlert = '';
      return;
    }

    const notes: string[] = [];
    for (let index = this.enemyInterdictionOrders.length - 1; index >= 0; index -= 1) {
      const order = this.enemyInterdictionOrders[index];
      const previousStage = order.stage;
      order.remainingTurns = Math.max(0, order.remainingTurns - 1);
      order.stage = enemyInterdictionStageByRemaining(order.remainingTurns);
      if (order.remainingTurns <= 0) {
        notes.push(`敌方截粮命令撤离：${order.routeLabel}警报解除`);
        this.enemyInterdictionOrders.splice(index, 1);
      } else if (order.stage !== previousStage) {
        notes.push(`敌方截粮命令推进：${order.routeLabel}进入${enemyInterdictionStageName(order.stage)}`);
      }
    }

    if (this.selectedEnemyInterdictionId && !this.enemyInterdictionOrders.some((order) => order.id === this.selectedEnemyInterdictionId)) {
      this.selectedEnemyInterdictionId = '';
    }

    if (notes.length > 0) {
      this.operationLog.unshift(`第 ${this.currentWarTurn} 回合：${notes[0]}`);
      trimTo(this.operationLog, 5);
    }
    this.refreshInterceptionAlert();
  }

  private refreshInterceptionAlert(): void {
    const order = this.activeEnemyInterdictionOrder();
    this.latestInterceptionAlert = order ? describeEnemyInterdictionOrder(order) : '';
  }

  private applyInterdictionCountermeasure(kind: 'escort' | 'reroute' | 'counter-scout' | 'decoy'): void {
    const order = this.activeEnemyInterdictionOrder();
    if (!order) {
      this.operationLog.unshift('截粮反制：当前没有已显形的敌方截粮命令。');
      trimTo(this.operationLog, 5);
      return;
    }

    const target = this.dataset.regionById.get(order.targetRegionId) ?? this.selectedRegion;
    const army = this.dataset.armies.find((candidate) => candidate.id === order.armyId) ?? this.activeArmy;
    let note = '';
    switch (kind) {
      case 'escort':
        this.spend('food', 8);
        this.spend('money', 4);
        army.supply = clamp(army.supply - 2, 0, 100);
        order.risk = clamp(order.risk - 12, 0, 100);
        order.supplyDamage = Math.max(1, order.supplyDamage - 4);
        order.remainingTurns = Math.min(order.totalTurns + 1, order.remainingTurns + 1);
        order.stage = enemyInterdictionStageByRemaining(order.remainingTurns);
        order.lastCountermeasure = '护粮队';
        note = `护粮队压上：${order.routeLabel}截粮风险降至 ${order.risk}%，预计损耗 ${order.supplyDamage}`;
        break;
      case 'reroute':
        this.spend('food', 6);
        order.risk = clamp(order.risk - 16, 0, 100);
        order.supplyDamage = Math.max(1, order.supplyDamage - 2);
        order.remainingTurns = Math.min(order.totalTurns + 1, order.remainingTurns + 1);
        order.stage = enemyInterdictionStageByRemaining(order.remainingTurns);
        order.lastCountermeasure = '改道';
        this.logisticsQueue.unshift(`${army.name}：改道绕避${target.definition.name}险段，运输慢一拍但截粮面收窄`);
        trimTo(this.logisticsQueue, 4);
        note = `改道绕避：${order.routeLabel}风险降至 ${order.risk}%，尚余 ${order.remainingTurns} 回合`;
        break;
      case 'counter-scout':
        this.spend('money', 8);
        target.risk = clamp(target.risk - 2, 0, 100);
        order.risk = clamp(order.risk - 20, 0, 100);
        order.supplyDamage = Math.max(1, order.supplyDamage - 3);
        order.lastCountermeasure = '反斥候';
        note = `反斥候查伏：${target.definition.name}地方风险 -2，截粮风险降至 ${order.risk}%`;
        break;
      case 'decoy':
        this.spend('food', 10);
        army.morale = clamp(army.morale + 1, 0, 100);
        order.risk = clamp(order.risk - 24, 0, 100);
        order.supplyDamage = Math.max(0, order.supplyDamage - 5);
        order.remainingTurns = Math.max(1, order.remainingTurns - 1);
        order.stage = enemyInterdictionStageByRemaining(order.remainingTurns);
        order.lastCountermeasure = '诱敌';
        note = `弃车诱敌：${order.routeLabel}敌踪暴露，截粮风险降至 ${order.risk}%，军心 +1`;
        break;
    }

    this.lastCountermeasureSummary = order.lastCountermeasure ?? '';
    if (order.supplyDamage <= 0) {
      this.recordEnemyInterdictionMemory(order, false);
      order.resolved = true;
      order.stage = 'resolved';
      const resolvedPressure = this.createRoutePressureCopy(this.createActiveRouteForecast(target), order);
      this.lastResolvedRoutePressure = {
        ...resolvedPressure,
        full: `${resolvedPressure.full} / 截粮队撤离`,
        compact: `${resolvedPressure.compact} | 撤离`,
        detail: `${resolvedPressure.detail} 反制成功，敌方截粮队撤离。`,
        detailHtml: resolvedPressure.detailHtml
      };
      this.latestInterceptionAlert = `截粮反制成功：${note}，敌方截粮队撤离。`;
    } else {
      this.lastResolvedRoutePressure = null;
      this.refreshInterceptionAlert();
    }
    this.operationLog.unshift(`第 ${this.currentWarTurn} 回合：${note}`);
    trimTo(this.operationLog, 5);
  }

  private resolveWarCommand(command: WarCommand): string {
    const target = this.dataset.regionById.get(command.targetRegionId) ?? this.selectedRegion;
    const army = this.dataset.armies.find((candidate) => candidate.id === command.armyId) ?? this.activeArmy;
    const tacticalContext = this.createBattleOutcomeContext(command, target, army);
    switch (command.kind) {
      case 'deploy': {
        army.supply = clamp(army.supply + 5, 0, 100);
        target.risk = clamp(target.risk - 2, 0, 100);
        const result = `${target.definition.name}军府完成，${army.name}补给 +5，地方风险 -2`;
        this.recordBattleOutcome({ turn: this.currentWarTurn, regionName: target.definition.name, kind: 'deploy', result, success: true, ...tacticalContext });
        return result;
      }
      case 'supply': {
        const recovered = Math.max(0, command.supplyReserve);
        army.supply = clamp(army.supply + recovered, 0, 100);
        target.risk = clamp(target.risk - 1, 0, 100);
        const totalDelivered = command.deliveredSupply + recovered;
        const result = `${target.definition.name}运输队抵达，${army.name}尾段补给 +${recovered}，全程送达 ${totalDelivered}/${command.plannedSupplyReserve}`;
        command.deliveredSupply = totalDelivered;
        command.supplyReserve = 0;
        command.completedSegments = command.segmentCount;
        this.syncTransportConvoy(command, 'delivered');
        this.recordBattleOutcome({ turn: this.currentWarTurn, regionName: target.definition.name, kind: 'supply', result, supplyUsed: totalDelivered, success: true, ...tacticalContext });
        return result;
      }
      case 'scout': {
        target.risk = clamp(target.risk - 3, 0, 100);
        const result = `${target.definition.name}侦察完成，伏击点已记录，风险 -3`;
        this.recordBattleOutcome({ turn: this.currentWarTurn, regionName: target.definition.name, kind: 'scout', result, success: true, ...tacticalContext });
        return result;
      }
      case 'fortify': {
        army.morale = clamp(army.morale + 3, 0, 100);
        army.supply = clamp(army.supply + 4, 0, 100);
        target.risk = clamp(target.risk - 2, 0, 100);
        this.onStationBuilt(target);
        const routeLimit = this.routeCapacityLimit(command.fromRegionId, command.targetRegionId);
        const result = `${target.definition.name}兵站加固，路线容量提升至 ${routeLimit}，军心 +3，补给 +4`;
        this.recordBattleOutcome({ turn: this.currentWarTurn, regionName: target.definition.name, kind: 'fortify', result, success: true, ...tacticalContext });
        return result;
      }
      case 'attack': {
        const casualties = Math.max(180, Math.round(command.interceptionRisk * 9));
        const supplyUsed = Math.max(8, Math.round(command.interceptionRisk / 4));
        army.soldiers = Math.max(0, army.soldiers - casualties);
        army.supply = clamp(army.supply - supplyUsed, 0, 100);
        target.owner = 'player';
        target.controlStage = 'military-govern';
        target.integration = clamp(Math.max(target.integration, 24), 0, 100);
        target.contribution = clamp(Math.min(target.contribution, 18), 0, 100);
        target.risk = clamp(target.risk + 6, 0, 100);
        this.nationState.legitimacy = clamp(this.nationState.legitimacy - 1, 0, 100);
        this.enqueueOccupationAftercare(target);
        const result = `${target.definition.name}攻占完成，新附军管，伤亡 ${formatNumber(casualties)}，贡献暂限 ${Math.round(target.contribution)}%`;
        this.recordBattleOutcome({ turn: this.currentWarTurn, regionName: target.definition.name, kind: 'attack', result, casualties, supplyUsed, success: true, ...tacticalContext });
        return result;
      }
    }
  }

  private createBattleOutcomeContext(command: WarCommand, target: RegionViewModel, army: ArmyViewModel): Pick<BattleOutcome, 'armyName' | 'generalName' | 'unitMix' | 'tacticScore' | 'tacticText' | 'tacticDeltas' | 'interceptionRisk'> {
    const tactical = this.tacticalSummaryForArmy(army, target);
    const tacticDeltas = this.tacticalModifierForArmy(army, target);
    return {
      armyName: army.name,
      generalName: army.general,
      unitMix: unitMixText(army.unitMix, this.dataset.units),
      tacticScore: tactical.score,
      tacticText: tactical.text,
      tacticDeltas,
      interceptionRisk: command.interceptionRisk
    };
  }

  private recordBattleOutcome(outcome: BattleOutcome): void {
    this.battleReportHistory.unshift(outcome);
    trimTo(this.battleReportHistory, 8);
  }

  private enqueueOccupationAftercare(region: RegionViewModel): void {
    const existing = this.occupationQueue.find((task) => task.regionId === region.definition.id);
    if (existing) {
      existing.stage = 'military-govern';
      existing.remainingTurns = 2;
      existing.riskPressure = Math.round(region.risk);
      existing.contributionCap = Math.round(region.contribution);
    } else {
      this.occupationQueue.unshift({
        id: `occupation_aftercare_${this.occupationCounter++}`,
        regionId: region.definition.id,
        regionName: region.definition.name,
        stage: 'military-govern',
        remainingTurns: 2,
        riskPressure: Math.round(region.risk),
        contributionCap: Math.round(region.contribution)
      });
    }
    this.enqueueOccupationSupplyTask(region);
    this.operationLog.unshift(`${region.definition.name}：新附地区进入军管→安抚→编户队列，完整贡献暂缓`);
    trimTo(this.occupationQueue, 5);
    trimTo(this.operationLog, 5);
  }

  private enqueueOccupationSupplyTask(region: RegionViewModel): void {
    const stage = region.controlStage === 'pacify' || region.controlStage === 'register' ? region.controlStage : 'military-govern';
    const source = this.selectSupplySourceRegion(region);
    const routeProfile = this.routeTerrainProfileByIds(source.definition.id, region.definition.id);
    const governance = this.routeGovernanceLogisticsModifier(source.definition.id, region.definition.id);
    const supplyNeed = Math.round(Math.max(14, region.risk * 0.35 + region.definition.localPower * 0.2 - governance.occupationBandwidthBonus));
    const bandwidthUsed = this.taskBandwidthForRoute(routeProfile, governance);
    const existing = this.occupationSupplyTasks.find((task) => task.regionId === region.definition.id && task.status !== 'delivered' && task.status !== 'cancelled');
    if (existing) {
      existing.stage = stage;
      existing.supplyNeeded = Math.max(existing.supplyNeeded, supplyNeed);
      existing.priority = stage === 'military-govern' ? 3 : stage === 'pacify' ? 2 : 1;
      existing.autoDispatchTurn = Math.min(existing.autoDispatchTurn, this.currentWarTurn + 1);
      existing.routeLabel = `${source.definition.name}→${region.definition.name}`;
      existing.bandwidthUsed = Math.min(existing.bandwidthUsed, bandwidthUsed);
      return;
    }

    const task = createOccupationSupplyTask(
      this.occupationSupplyTasks,
      source.definition.id,
      region.definition.id,
      region.definition.name,
      source.definition.name,
      stage,
      Math.max(24, Math.round(region.contribution)),
      this.currentWarTurn,
      this.occupationSupplyCounter++,
      bandwidthUsed
    );
    task.supplyNeeded = Math.max(task.supplyNeeded - governance.occupationBandwidthBonus, supplyNeed);
    this.onRouteAssigned(source.definition.id, region.definition.id, 1);
  }

  private selectSupplySourceRegion(target: RegionViewModel): RegionViewModel {
    const station = this.logisticsStations.find((candidate) => candidate.isActive && candidate.regionId !== target.definition.id);
    if (station) {
      return this.dataset.regionById.get(station.regionId) ?? this.activeArmyTargetRegion();
    }
    return this.activeArmyTargetRegion();
  }

  private activeArmyTargetRegion(): RegionViewModel {
    return this.dataset.regionById.get(this.activeArmy.fromRegionId) ?? this.selectedRegion;
  }

  private taskBandwidthForRoute(profile: RouteTerrainProfile, governance: GovernanceLogisticsDelta = zeroGovernanceLogisticsDelta()): number {
    const terrainCost = profile.baseCapacity === 1 ? 6 : profile.baseCapacity === 2 ? 4 : 3;
    return Math.max(1, terrainCost - Math.floor(governance.occupationBandwidthBonus / 3));
  }

  private advanceOccupationQueue(): void {
    const task = this.occupationQueue[0];
    if (!task) {
      this.operationLog.unshift('占后队列：暂无新附地区需要推进。');
      trimTo(this.operationLog, 5);
      return;
    }

    const region = this.dataset.regionById.get(task.regionId);
    if (!region) {
      this.occupationQueue.shift();
      this.operationLog.unshift(`占后队列：${task.regionName} 数据缺失，已移出队列。`);
      trimTo(this.operationLog, 5);
      return;
    }

    task.remainingTurns = Math.max(0, task.remainingTurns - 1);
    if (task.remainingTurns > 0) {
      region.risk = clamp(region.risk - 2, 0, 100);
      task.riskPressure = Math.round(region.risk);
      this.operationLog.unshift(`占后队列：${task.regionName}${occupationStageName(task.stage)}继续执行，尚余 ${task.remainingTurns} 回合。`);
      trimTo(this.operationLog, 5);
      return;
    }

    if (task.stage === 'military-govern') {
      task.stage = 'pacify';
      task.remainingTurns = 2;
      task.contributionCap = 34;
      region.controlStage = 'pacify';
      region.integration = clamp(region.integration + 8, 0, 100);
      region.contribution = clamp(Math.max(region.contribution, 28), 0, task.contributionCap);
      region.risk = clamp(region.risk - 7, 0, 100);
      task.riskPressure = Math.round(region.risk);
      this.enqueueOccupationSupplyTask(region);
      this.operationLog.unshift(`占后队列：${task.regionName}解除纯军管，转入安抚，贡献上限 ${task.contributionCap}%`);
    } else if (task.stage === 'pacify') {
      task.stage = 'register';
      task.remainingTurns = 2;
      task.contributionCap = 58;
      region.controlStage = 'register';
      region.integration = clamp(region.integration + 10, 0, 100);
      region.contribution = clamp(Math.max(region.contribution, 42), 0, task.contributionCap);
      region.risk = clamp(region.risk - 6, 0, 100);
      task.riskPressure = Math.round(region.risk);
      this.enqueueOccupationSupplyTask(region);
      this.operationLog.unshift(`占后队列：${task.regionName}安抚见效，转入编户，贡献上限 ${task.contributionCap}%`);
    } else {
      region.controlStage = 'controlled';
      region.integration = clamp(region.integration + 14, 0, 100);
      region.contribution = clamp(Math.max(region.contribution, 72), 0, 100);
      region.risk = clamp(region.risk - 5, 0, 100);
      this.occupationQueue.shift();
      this.operationLog.unshift(`占后队列：${task.regionName}编户完成，纳入常规治理，贡献 ${Math.round(region.contribution)}%`);
    }
    trimTo(this.operationLog, 5);
  }

  private splitActiveArmy(): void {
    if (this.activeArmy.soldiers < 4200) {
      this.operationLog.unshift(`${this.activeArmy.name}：兵力不足，不能再拆分偏师`);
      trimTo(this.operationLog, 5);
      return;
    }

    const detachedSoldiers = Math.max(1800, Math.round(this.activeArmy.soldiers * 0.32));
    this.activeArmy.soldiers -= detachedSoldiers;
    const detached: ArmyViewModel = {
      id: `army_player_detached_${this.splitCounter}`,
      name: `${this.activeArmy.name}偏师${this.splitCounter}`,
      faction: this.activeArmy.faction,
      fromRegionId: this.activeArmy.fromRegionId,
      targetRegionId: this.activeArmy.targetRegionId,
      soldiers: detachedSoldiers,
      supply: clamp(this.activeArmy.supply - 6, 0, 100),
      morale: clamp(this.activeArmy.morale - 2, 0, 100),
      generalId: this.activeArmy.generalId,
      general: this.activeArmy.general,
      unit: this.activeArmy.unit,
      unitMix: { ...this.activeArmy.unitMix }
    };
    if (this.activeArmy.waypointRegionId) detached.waypointRegionId = this.activeArmy.waypointRegionId;
    this.splitCounter += 1;
    this.dataset.armies.push(detached);
    this.activeArmy = detached;
    this.enqueueLogistics(`${detached.name}：拆分 ${formatNumber(detachedSoldiers)}，可单独改道或牵制`);
    this.events.onArmyChange(detached.id);
  }

  private mergeActiveArmy(): void {
    const candidate = this.dataset.armies.find((army) =>
      army.faction === 'player' &&
      army.id !== this.activeArmy.id &&
      army.fromRegionId === this.activeArmy.fromRegionId
    ) ?? this.dataset.armies.find((army) => army.faction === 'player' && army.id !== this.activeArmy.id);

    if (!candidate) {
      this.operationLog.unshift(`${this.activeArmy.name}：没有可合并的己方军队`);
      trimTo(this.operationLog, 5);
      return;
    }

    const activeSoldiers = this.activeArmy.soldiers;
    const totalSoldiers = activeSoldiers + candidate.soldiers;
    this.activeArmy.supply = Math.round((this.activeArmy.supply * activeSoldiers + candidate.supply * candidate.soldiers) / totalSoldiers);
    this.activeArmy.morale = Math.round((this.activeArmy.morale * activeSoldiers + candidate.morale * candidate.soldiers) / totalSoldiers);
    this.mergeUnitMix(candidate, activeSoldiers);
    this.activeArmy.soldiers = totalSoldiers;
    this.dataset.armies.splice(this.dataset.armies.indexOf(candidate), 1);
    this.enqueueLogistics(`${this.activeArmy.name}：合并 ${candidate.name}，总兵力 ${formatNumber(totalSoldiers)}`);
    this.events.onArmyChange(this.activeArmy.id);
  }

  private rotateGeneral(): void {
    const next = this.nextGeneral();
    if (!next) return;
    this.activeArmy.generalId = next.id;
    this.activeArmy.general = formatGeneral(next);
    const moraleDelta = Math.round((next.military - 88) / 3);
    this.activeArmy.morale = clamp(this.activeArmy.morale + moraleDelta, 0, 100);
    this.enqueueLogistics(`${this.activeArmy.name}：改由${formatGeneral(next)}统率，${next.specialAbilityName}`);
  }

  private applyUnitMix(mix: Record<string, number>, label: string): void {
    this.activeArmy.unitMix = normalizeMix(mix);
    const dominant = dominantUnit(this.activeArmy.unitMix, this.dataset.units);
    if (dominant) this.activeArmy.unit = dominant;
    const supplyDelta = label.includes('攻城') ? -5 : label.includes('骑兵') ? -4 : label.includes('弩') ? -2 : -1;
    const moraleDelta = label.includes('骑兵') ? 2 : label.includes('攻城') ? -1 : 1;
    this.activeArmy.supply = clamp(this.activeArmy.supply + supplyDelta, 0, 100);
    this.activeArmy.morale = clamp(this.activeArmy.morale + moraleDelta, 0, 100);
    this.enqueueLogistics(`${this.activeArmy.name}：兵种配比改为${label}，主兵种 ${this.activeArmy.unit.name}`);
  }

  private applyGovernanceFocus(region: RegionViewModel, focusId: GovernanceFocusId): void {
    const plan = this.governanceFocusPlan(focusId, region);
    this.spend('food', plan.costs.food);
    this.spend('money', plan.costs.money);
    this.nationState.food = Math.max(0, this.nationState.food + plan.delta.food);
    this.nationState.money = Math.max(0, this.nationState.money + plan.delta.money);
    this.nationState.army = Math.max(0, this.nationState.army + plan.delta.army);
    this.nationState.legitimacy = clamp(this.nationState.legitimacy + plan.delta.legitimacy, 0, 100);
    region.integration = clamp(region.integration + plan.delta.integration, 0, 100);
    region.contribution = clamp(region.contribution + plan.delta.contribution, 0, 100);
    region.risk = clamp(region.risk + plan.delta.risk, 0, 100);
    region.legitimacy = clamp(region.legitimacy + plan.delta.legitimacy, 0, 100);
    region.governanceFocus = focusId;
    region.specialization = plan.specialization;
    this.refreshRegionRecommendations(region);
    this.applyGovernanceLogisticsEffect(region, `区域专精：${plan.label}`, plan.delta.logistics);
    this.enqueueGovernance(`${region.definition.name}：改定${plan.specialization}，${formatGovernanceFocusDelta(plan.delta)}。史据：${plan.source}`);
  }

  private applyGovernanceLabor(region: RegionViewModel, laborId: GovernanceLaborId): void {
    const plan = this.governanceLaborPlan(laborId, region);
    region.laborFocus = laborId;
    this.enqueueGovernance(`${region.definition.name}：劳力改投${plan.label}，推进时${formatGovernanceLaborDelta(plan.delta)}`);
  }

  private governanceLaborPlan(laborId: GovernanceLaborId, region: RegionViewModel): GovernanceLaborPlan {
    const foodBase = Math.max(1, Math.round(region.definition.foodOutput / 18));
    const moneyBase = Math.max(1, Math.round(region.definition.taxOutput / 18));
    const armyBase = Math.max(120, Math.round(region.definition.manpower * 14));
    const riskPressure = region.risk >= 25 ? 1 : 0;

    switch (laborId) {
      case 'grain':
        return {
          id: laborId,
          label: '垦田水利',
          actionLabel: '增粮稳供',
          description: '优先组织农田、水利和仓储劳力，适合前线补给紧张时使用。',
          delta: { food: foodBase + 3, money: 0, army: 0, legitimacy: 0, integration: 1, contribution: 2, risk: riskPressure }
        };
      case 'tax':
        return {
          id: laborId,
          label: '商税漕运',
          actionLabel: '增钱促运',
          description: '优先清理商税、漕运和市易，收入更高但地方压力上升。',
          delta: { food: 0, money: moneyBase + 4, army: 0, legitimacy: 0, integration: 0, contribution: 3, risk: 2 }
        };
      case 'military':
        return {
          id: laborId,
          label: '军役整备',
          actionLabel: '补兵备战',
          description: '把劳力投向军府、军械和征发，快速补兵但会压迫地方。',
          delta: { food: -2, money: -1, army: armyBase, legitimacy: -1, integration: 0, contribution: -1, risk: 3 }
        };
      case 'stability':
        return {
          id: laborId,
          label: '安抚编户',
          actionLabel: '降险整合',
          description: '优先赈济、编户和乡里调停，产出较慢但能压低民变。',
          delta: { food: -3, money: -1, army: 0, legitimacy: 1, integration: 2, contribution: 1, risk: -4 }
        };
      case 'balanced':
      default:
        return {
          id: 'balanced',
          label: '均衡轮役',
          actionLabel: '稳步推进',
          description: '在粮、钱、整合和地方风险之间保持均衡，适合默认治理。',
          delta: {
            food: Math.max(1, Math.round(foodBase * 0.6)),
            money: Math.max(1, Math.round(moneyBase * 0.6)),
            army: Math.round(armyBase * 0.25),
            legitimacy: 0,
            integration: 1,
            contribution: 1,
            risk: -1
          }
        };
    }
  }

  private governanceFocusPlan(focusId: GovernanceFocusId, region: RegionViewModel): GovernanceFocusPlan {
    const terrain = region.definition.terrain;
    const geo = region.geography.kind;
    const foodBase = Math.max(2, Math.round(region.definition.foodOutput / 9));
    const taxBase = Math.max(2, Math.round(region.definition.taxOutput / 8));
    const manpowerBase = Math.max(500, region.definition.manpower * 55);
    const waterBonus = terrain.includes('river') || geo.includes('water') ? 2 : 0;
    const frontierBonus = geo.includes('frontier') || geo.includes('corridor') || geo.includes('pass') ? 2 : 0;
    const legitimacyBonus = Math.min(3, region.definition.legitimacyMemory.length);
    const source = governanceFocusSource(region);

    switch (focusId) {
      case 'grain':
        return {
          id: focusId,
          label: '粮仓',
          specialization: '粮仓水利',
          actionLabel: '修渠垦田',
          description: '把劳力压到水利、仓储和田畴，换稳定粮入与前线续航。',
          source,
          costs: { food: 4, money: 10 },
          delta: {
            food: foodBase + waterBonus + 4,
            money: 0,
            army: 0,
            legitimacy: 0,
            integration: 1,
            contribution: 3,
            risk: waterBonus > 0 ? 0 : 1,
            logistics: { capacityBonus: 1, supplyRelief: 2 + waterBonus, interdictionRelief: 0, occupationBandwidthBonus: 1 }
          }
        };
      case 'tax':
        return {
          id: focusId,
          label: '财赋',
          specialization: '商税漕运',
          actionLabel: '开市核税',
          description: '把地区改成财税节点，收入更清楚，但豪强与民怨压力会上来。',
          source,
          costs: { food: 0, money: 2 },
          delta: {
            food: 0,
            money: taxBase + waterBonus + 6,
            army: 0,
            legitimacy: -1,
            integration: 0,
            contribution: 4,
            risk: 3,
            logistics: { capacityBonus: waterBonus > 0 ? 1 : 0, supplyRelief: 0, interdictionRelief: 0, occupationBandwidthBonus: 0 }
          }
        };
      case 'military':
        return {
          id: focusId,
          label: '军府',
          specialization: '兵源军府',
          actionLabel: '整籍征发',
          description: '把人口与乡兵组织进军府，快速补兵，但会压低地区贡献并推高民变。',
          source,
          costs: { food: 10, money: 4 },
          delta: {
            food: 0,
            money: 0,
            army: manpowerBase,
            legitimacy: -1,
            integration: 0,
            contribution: -2,
            risk: 4,
            logistics: { capacityBonus: 0, supplyRelief: -1, interdictionRelief: 1, occupationBandwidthBonus: 0 }
          }
        };
      case 'frontier':
        return {
          id: focusId,
          label: '边防',
          specialization: '边防屯戍',
          actionLabel: '筑塞置屯',
          description: '把地区当作前线支点经营，补给线更稳，财政和粮食会被守备吃掉。',
          source,
          costs: { food: 8, money: 8 },
          delta: {
            food: 0,
            money: 0,
            army: Math.round(manpowerBase * 0.35),
            legitimacy: 0,
            integration: 1,
            contribution: 1,
            risk: frontierBonus > 0 ? -2 : -1,
            logistics: { capacityBonus: 1 + frontierBonus, supplyRelief: 1, interdictionRelief: 2 + frontierBonus, occupationBandwidthBonus: 0 }
          }
        };
      case 'legitimacy':
        return {
          id: focusId,
          label: '礼制',
          specialization: '法统礼制',
          actionLabel: '修礼置学',
          description: '以学校、礼制和官僚秩序换长期承认，短期直接收益不高。',
          source,
          costs: { food: 0, money: 12 },
          delta: {
            food: 0,
            money: 0,
            army: 0,
            legitimacy: 3 + legitimacyBonus,
            integration: 3,
            contribution: 1,
            risk: -1,
            logistics: { capacityBonus: 0, supplyRelief: 0, interdictionRelief: 1, occupationBandwidthBonus: 1 }
          }
        };
      case 'relief':
      default:
        return {
          id: 'relief',
          label: '民生',
          specialization: '安抚民生',
          actionLabel: '赈济缓役',
          description: '把本回合资源让给地方恢复，风险下降，但国库和军需立刻承压。',
          source,
          costs: { food: 16, money: 4 },
          delta: {
            food: 0,
            money: 0,
            army: 0,
            legitimacy: 1,
            integration: 2,
            contribution: 1,
            risk: -6,
            logistics: { capacityBonus: 0, supplyRelief: 0, interdictionRelief: 0, occupationBandwidthBonus: 2 }
          }
        };
    }
  }

  private refreshRegionRecommendations(region: RegionViewModel): void {
    region.recommendedBuilding = chooseBuildingForSpecialization(this.dataset.buildings, region.specialization, region.definition.terrain);
    region.recommendedPolicy = choosePolicyForGovernanceFocus(this.dataset.policies, region.governanceFocus, region);
    region.sourceText = [
      region.history?.uiSummary,
      region.recommendedPolicy ? `${region.recommendedPolicy.name}：${region.recommendedPolicy.sourceReference}` : undefined,
      region.recommendedBuilding ? `${region.recommendedBuilding.name}：${region.recommendedBuilding.sourceReference}` : undefined
    ].filter(Boolean).join(' / ');
  }

  private createGovernanceProject(region: RegionViewModel, building: BuildingDefinition | undefined): GovernanceProject {
    const chosenBuilding = building ?? this.dataset.buildings[0];
    const effects = chosenBuilding?.effects ?? {};
    const logistics = governanceLogisticsDeltaFromBuilding(chosenBuilding);
    const totalTurns = Math.max(2, Math.min(5, Math.ceil((chosenBuilding?.cost ?? 25) / 15)));

    return {
      id: `governance_project_${this.governanceProjectCounter++}`,
      regionId: region.definition.id,
      regionName: region.definition.name,
      buildingName: chosenBuilding?.name ?? '基础营造',
      buildingCategory: chosenBuilding?.category ?? 'infrastructure',
      focusId: region.governanceFocus,
      laborHint: region.laborFocus,
      remainingTurns: totalTurns,
      totalTurns,
      foodYield: Math.max(0, Math.round(effect(effects.food, 0) / 2)),
      moneyYield: Math.max(0, Math.round((effect(effects.money, 0) + effect(effects.taxEfficiency, 0)) / 2)),
      contributionDelta: Math.max(1, Math.round((effect(effects.taxEfficiency, 0) + effect(effects.integrationSpeed, 0)) / 3)),
      integrationDelta: Math.max(1, Math.round(effect(effects.integrationSpeed, 0) / 2)),
      riskDelta: Math.min(0, Math.round(effect(effects.rebellionRisk, 0))),
      legitimacyDelta: Math.max(0, Math.round(effect(effects.legitimacy, 0))),
      logistics,
      source: chosenBuilding?.sourceReference ?? '据地区推荐建设抽象。'
    };
  }

  private advanceGovernanceTurn(): void {
    this.currentGovernanceTurn += 1;
    const region = this.selectedRegion;
    const plan = this.governanceLaborPlan(region.laborFocus, region);

    this.nationState.food = Math.max(0, this.nationState.food + plan.delta.food);
    this.nationState.money = Math.max(0, this.nationState.money + plan.delta.money);
    this.nationState.army = Math.max(0, this.nationState.army + plan.delta.army);
    this.nationState.legitimacy = clamp(this.nationState.legitimacy + plan.delta.legitimacy, 0, 100);
    region.integration = clamp(region.integration + plan.delta.integration, 0, 100);
    region.contribution = clamp(region.contribution + plan.delta.contribution, 0, 100);
    region.risk = clamp(region.risk + plan.delta.risk, 0, 100);
    region.legitimacy = clamp(region.legitimacy + plan.delta.legitimacy, 0, 100);

    const completedProjects: GovernanceProject[] = [];
    for (const project of this.governanceProjects) {
      if (project.regionId !== region.definition.id) continue;
      const laborBoost = project.laborHint === region.laborFocus ? 2 : 1;
      project.remainingTurns = Math.max(0, project.remainingTurns - laborBoost);
      if (project.remainingTurns === 0) completedProjects.push(project);
    }

    for (const project of completedProjects) {
      this.completeGovernanceProject(project);
      const index = this.governanceProjects.findIndex((candidate) => candidate.id === project.id);
      if (index >= 0) this.governanceProjects.splice(index, 1);
    }

    const completionText = completedProjects.length > 0
      ? `；完工 ${completedProjects.map((project) => project.buildingName).join('、')}`
      : '';
    this.enqueueGovernance(`第 ${this.currentGovernanceTurn} 旬：${region.definition.name}${plan.label}推进，${formatGovernanceLaborDelta(plan.delta)}${completionText}`);
  }

  private completeGovernanceProject(project: GovernanceProject): void {
    const region = this.dataset.regions.find((candidate) => candidate.definition.id === project.regionId);
    if (!region) return;

    this.nationState.food = Math.max(0, this.nationState.food + project.foodYield);
    this.nationState.money = Math.max(0, this.nationState.money + project.moneyYield);
    this.nationState.legitimacy = clamp(this.nationState.legitimacy + project.legitimacyDelta, 0, 100);
    region.contribution = clamp(region.contribution + project.contributionDelta, 0, 100);
    region.integration = clamp(region.integration + project.integrationDelta, 0, 100);
    region.risk = clamp(region.risk + project.riskDelta, 0, 100);
    region.legitimacy = clamp(region.legitimacy + project.legitimacyDelta, 0, 100);
    this.applyGovernanceLogisticsEffect(region, `${project.buildingName}完工`, project.logistics);
    this.operationLog.unshift(`${project.regionName}：${project.buildingName}完工，${formatGovernanceProjectYield(project)}`);
    trimTo(this.operationLog, 5);
  }

  private mergeUnitMix(candidate: ArmyViewModel, activeSoldiers: number): void {
    const total = activeSoldiers + candidate.soldiers;
    const merged: Record<string, number> = {};
    for (const unitId of new Set([...Object.keys(this.activeArmy.unitMix), ...Object.keys(candidate.unitMix)])) {
      merged[unitId] = Math.round(
        ((this.activeArmy.unitMix[unitId] ?? 0) * activeSoldiers + (candidate.unitMix[unitId] ?? 0) * candidate.soldiers) / total
      );
    }
    this.activeArmy.unitMix = normalizeMix(merged);
    const dominant = dominantUnit(this.activeArmy.unitMix, this.dataset.units);
    if (dominant) this.activeArmy.unit = dominant;
  }

  private currentGeneral(): GeneralDefinition | undefined {
    return this.generalForArmy(this.activeArmy);
  }

  private generalForArmy(army: ArmyViewModel): GeneralDefinition | undefined {
    return this.dataset.generals.find((general) => general.id === army.generalId);
  }

  private nextGeneral(): GeneralDefinition | undefined {
    if (this.dataset.generals.length === 0) return undefined;
    const currentIndex = Math.max(0, this.dataset.generals.findIndex((general) => general.id === this.activeArmy.generalId));
    return this.dataset.generals[(currentIndex + 1) % this.dataset.generals.length];
  }

  private applyGovernancePolicy(region: RegionViewModel): void {
    const policy = region.recommendedPolicy;
    this.spend('money', policy?.cost.money ?? 10);
    region.integration = clamp(region.integration + effect(policy?.effects.integrationSpeed, 5), 0, 100);
    region.legitimacy = clamp(region.legitimacy + effect(policy?.effects.legitimacy, 2), 0, 100);
    region.risk = clamp(region.risk + effect(policy?.effects.rebellionRisk, -3) + effect(policy?.risks.rebellionRisk, 0), 0, 100);
    region.contribution = clamp(region.contribution + 3, 0, 100);
    this.nationState.legitimacy = clamp(Math.round((this.nationState.legitimacy * 3 + region.legitimacy) / 4), 0, 100);
    const delta = governanceLogisticsDeltaFromPolicy(policy);
    this.applyGovernanceLogisticsEffect(region, policy?.name ?? '地方安抚', delta);
    this.enqueueGovernance(`${region.definition.name}：${policy?.name ?? '地方安抚'}已执行，整合 ${Math.round(region.integration)}%，${formatGovernanceLogisticsDelta(delta)}`);
  }

  private applyConstruction(region: RegionViewModel): void {
    const building = region.recommendedBuilding;
    this.spend('money', building?.cost ?? 20);
    const project = this.createGovernanceProject(region, building);
    this.governanceProjects.unshift(project);
    const setupDelta: GovernanceLogisticsDelta = {
      capacityBonus: project.logistics.capacityBonus > 0 ? 1 : 0,
      supplyRelief: project.logistics.supplyRelief > 0 ? 1 : 0,
      interdictionRelief: 0,
      occupationBandwidthBonus: 0
    };
    if (governanceLogisticsScore(setupDelta) > 0) {
      this.applyGovernanceLogisticsEffect(region, `${project.buildingName}施工便道`, setupDelta);
    }
    this.enqueueGovernance(`${region.definition.name}：${project.buildingName}开工，需 ${project.totalTurns} 旬，完工预期 ${formatGovernanceProjectYield(project)}`);
  }

  private applyReinforcement(region: RegionViewModel): void {
    const added = Math.max(800, region.definition.manpower * 120);
    this.nationState.army += added;
    this.spend('food', 18);
    region.risk = clamp(region.risk + 4, 0, 100);
    region.contribution = clamp(region.contribution - 2, 0, 100);
    this.enqueueGovernance(`${region.definition.name}：征发 ${formatNumber(added)}，民变风险升至 ${Math.round(region.risk)}%`);
  }

  private applyArmyOrder(order: string, formation: string, route: string, supplyDelta: number, moraleDelta: number, log: string): void {
    this.armyOrder = {
      stance: order,
      formation,
      route,
      last: order
    };
    this.activeArmy.supply = clamp(this.activeArmy.supply + supplyDelta, 0, 100);
    this.activeArmy.morale = clamp(this.activeArmy.morale + moraleDelta, 0, 100);
    this.enqueueLogistics(log);
  }

  private applyEmperorState(emperor: EmperorDefinition): void {
    const governanceLift = Math.round((effect(emperor.stats.administration, 70) - 70) / 12);
    const reformLift = Math.round((effect(emperor.stats.reform, 70) - 70) / 16);
    const militaryLift = Math.round((effect(emperor.stats.military, 70) - 70) / 10);
    this.nationState.legitimacy = clamp(this.nationState.legitimacy + governanceLift, 0, 100);
    this.activeArmy.morale = clamp(this.activeArmy.morale + militaryLift, 0, 100);
    this.selectedRegion.integration = clamp(this.selectedRegion.integration + reformLift, 0, 100);
    this.operationLog.unshift(`${emperor.title}${emperor.name}亲政：${emperor.uniqueMechanic.name}`);
    trimTo(this.operationLog, 5);
  }

  private enqueueGovernance(text: string): void {
    this.governanceQueue.unshift(text);
    this.operationLog.unshift(text);
    trimTo(this.governanceQueue, 4);
    trimTo(this.operationLog, 5);
  }

  private enqueueLogistics(text: string): void {
    this.logisticsQueue.unshift(text);
    this.operationLog.unshift(text);
    trimTo(this.logisticsQueue, 4);
    trimTo(this.operationLog, 5);
  }

  private promoteLogisticsQueue(): void {
    if (this.commandQueue.length > 1) {
      const command = this.commandQueue.pop();
      if (command) this.commandQueue.unshift(command);
    }
    if (this.logisticsQueue.length > 1) {
      const item = this.logisticsQueue.pop();
      if (item) this.logisticsQueue.unshift(item);
      this.operationLog.unshift(`${this.activeArmy.name}：后勤队列已上移`);
    } else {
      this.operationLog.unshift(`${this.activeArmy.name}：后勤队列无需上移`);
    }
    trimTo(this.logisticsQueue, 4);
    trimTo(this.operationLog, 5);
  }

  private cancelLogisticsQueue(): void {
    const removedCommand = this.commandQueue.shift();
    if (removedCommand) {
      this.releaseRouteCapacity(removedCommand);
      this.cancelCommandTransportConvoy(removedCommand);
    }
    const removed = this.logisticsQueue.shift();
    this.operationLog.unshift(removedCommand ? `${this.activeArmy.name}：已撤销 ${removedCommand.label} / ${removedCommand.targetName}` : removed ? `${this.activeArmy.name}：已撤销 ${removed}` : `${this.activeArmy.name}：暂无后勤可撤销`);
    trimTo(this.commandQueue, 6);
    trimTo(this.logisticsQueue, 4);
    trimTo(this.operationLog, 5);
  }

  private promoteTransportConvoy(): void {
    const convoy = this.selectedTransportConvoy() ?? this.activeTransportConvoys().find((candidate) => this.commandQueue.some((command) => command.id === candidate.commandId));
    if (!convoy) {
      this.operationLog.unshift('运输队：暂无可上移的在途车队。');
      trimTo(this.operationLog, 5);
      return;
    }

    convoy.priority += 1;
    convoy.orderIndex = this.transportConvoyCounter + convoy.priority;
    const command = this.commandQueue.find((candidate) => candidate.id === convoy.commandId);
    if (command) {
      command.convoyPriority = convoy.priority;
      command.convoyOrder = convoy.orderIndex;
      this.commandQueue.splice(this.commandQueue.indexOf(command), 1);
      this.commandQueue.unshift(command);
      this.syncTransportConvoy(command);
    }
    this.transportConvoys.splice(this.transportConvoys.indexOf(convoy), 1);
    this.transportConvoys.unshift(convoy);
    this.operationLog.unshift(`运输队：${convoy.id} 已上移，优先保障 ${convoy.routeLabel}`);
    trimTo(this.operationLog, 5);
  }

  private cancelTransportConvoy(): void {
    const convoy = this.selectedTransportConvoy() ?? this.activeTransportConvoys()[0];
    if (!convoy) {
      this.operationLog.unshift('运输队：暂无可取消的在途车队。');
      trimTo(this.operationLog, 5);
      return;
    }

    const command = this.commandQueue.find((candidate) => candidate.id === convoy.commandId);
    if (command) {
      this.releaseRouteCapacity(command);
      this.commandQueue.splice(this.commandQueue.indexOf(command), 1);
    }
    convoy.status = 'cancelled';
    convoy.supplyReserve = 0;
    this.logisticsQueue.unshift(`运输队：${convoy.id} 已取消，释放 ${convoy.routeLabel} 路线容量`);
    this.operationLog.unshift(`运输队：${convoy.id} 已取消，释放 ${convoy.routeLabel} 路线容量`);
    trimTo(this.logisticsQueue, 4);
    trimTo(this.operationLog, 5);
  }

  private cancelCommandTransportConvoy(command: WarCommand): void {
    if (!command.convoyId) return;
    const convoy = this.transportConvoys.find((candidate) => candidate.id === command.convoyId);
    if (!convoy || convoy.status === 'delivered') return;
    convoy.status = 'cancelled';
    convoy.supplyReserve = 0;
  }

  private promoteOccupationSupplyTask(): void {
    const task = this.selectedOccupationSupplyTask() ?? this.occupationSupplyTasks.find((candidate) => candidate.status !== 'delivered' && candidate.status !== 'cancelled');
    if (!task) {
      this.operationLog.unshift('安抚运输：暂无可上移的车队。');
      trimTo(this.operationLog, 5);
      return;
    }
    this.occupationSupplyTasks.splice(this.occupationSupplyTasks.indexOf(task), 1);
    task.priority += 1;
    task.orderIndex = 0;
    this.occupationSupplyTasks.unshift(task);
    this.operationLog.unshift(`安抚运输：${task.convoyId} 已上移，优先保障 ${task.regionName}`);
    trimTo(this.operationLog, 5);
  }

  private cancelOccupationSupplyTask(): void {
    const task = this.selectedOccupationSupplyTask() ?? this.occupationSupplyTasks.find((candidate) => candidate.status !== 'delivered' && candidate.status !== 'cancelled');
    if (!task) {
      this.operationLog.unshift('安抚运输：暂无可取消的车队。');
      trimTo(this.operationLog, 5);
      return;
    }
    task.status = 'cancelled';
    this.releaseOccupationSupplyRoute(task);
    this.operationLog.unshift(`安抚运输：${task.convoyId} 已取消，释放 ${task.routeLabel} 路线容量`);
    trimTo(this.operationLog, 5);
  }

  private releaseOccupationSupplyRoute(task: OccupationSupplyTask): void {
    if (!task.routeUsageClaimed) return;
    this.onRouteAssigned(task.fromRegionId, task.regionId, -1);
    task.routeUsageClaimed = false;
  }

  private spend(resource: 'food' | 'money', amount: number): void {
    this.nationState[resource] = Math.max(0, this.nationState[resource] - amount);
  }
}

function setText(id: string, text: string): void {
  const element = document.getElementById(id);
  if (element) element.textContent = text;
}

function metric(label: string, value: string | number): string {
  return `<div class="metric"><span>${escapeHtml(label)}</span><b>${escapeHtml(String(value))}</b></div>`;
}

function compactStat(label: string, value: number): string {
  return `<span>${escapeHtml(label)} <b>${Math.round(value)}</b></span>`;
}

function meter(label: string, value: number): string {
  const clamped = Math.max(0, Math.min(100, Math.round(value)));
  // Color coding: 0-33 warning (red), 34-66 neutral (yellow), 67-100 good (green)
  let colorClass = 'meter-neutral';
  if (clamped < 34) colorClass = 'meter-low';
  else if (clamped > 66) colorClass = 'meter-high';
  return `<span>${escapeHtml(label)}</span><div class="meter"><i class="${colorClass}" style="width:${clamped}%"></i></div><b>${clamped}%</b>`;
}

function queueLines(items: string[], emptyText: string): string {
  const source = items.length > 0 ? items : [emptyText];
  return `<div class="queue-lines">${source.map((item) => `<div>${escapeHtml(item)}</div>`).join('')}</div>`;
}

function saveSlotKey(slotId: SaveSlotId): string {
  return `${SAVE_SLOT_PREFIX}${slotId}`;
}

function saveSlotDefinition(slotId: SaveSlotId): SaveSlotDefinition {
  return SAVE_SLOTS.find((slot) => slot.id === slotId) ?? SAVE_SLOTS[0];
}

function saveSlotAction(action: 'save' | 'load' | 'delete', slotId: SaveSlotId): UiAction {
  const index = slotId.replace('slot_', '');
  return `${action}_slot_${index}` as UiAction;
}

function formatSaveTime(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '时间未知';
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hour = String(date.getHours()).padStart(2, '0');
  const minute = String(date.getMinutes()).padStart(2, '0');
  return `${month}-${day} ${hour}:${minute}`;
}

function governanceFocusAction(focusId: GovernanceFocusId): UiAction {
  return `governance_focus_${focusId}` as UiAction;
}

function formatGovernanceFocusDelta(delta: GovernanceFocusDelta): string {
  const parts = [
    delta.food !== 0 ? `粮 ${formatSigned(delta.food)}` : '',
    delta.money !== 0 ? `钱 ${formatSigned(delta.money)}` : '',
    delta.army !== 0 ? `兵 ${formatSigned(delta.army)}` : '',
    delta.contribution !== 0 ? `贡献 ${formatSigned(delta.contribution)}` : '',
    delta.integration !== 0 ? `整合 ${formatSigned(delta.integration)}` : '',
    delta.legitimacy !== 0 ? `法统 ${formatSigned(delta.legitimacy)}` : '',
    delta.risk !== 0 ? `民变 ${formatSigned(delta.risk)}` : '',
    governanceLogisticsScore(delta.logistics) > 0 ? formatGovernanceLogisticsDelta(delta.logistics) : ''
  ].filter(Boolean);
  return parts.length > 0 ? parts.join(' / ') : '维持现状';
}

function formatGovernanceLaborDelta(delta: GovernanceLaborDelta): string {
  const parts = [
    delta.food !== 0 ? `粮 ${formatSigned(delta.food)}` : '',
    delta.money !== 0 ? `钱 ${formatSigned(delta.money)}` : '',
    delta.army !== 0 ? `兵 ${formatSigned(delta.army)}` : '',
    delta.contribution !== 0 ? `贡献 ${formatSigned(delta.contribution)}` : '',
    delta.integration !== 0 ? `整合 ${formatSigned(delta.integration)}` : '',
    delta.legitimacy !== 0 ? `法统 ${formatSigned(delta.legitimacy)}` : '',
    delta.risk !== 0 ? `民变 ${formatSigned(delta.risk)}` : ''
  ].filter(Boolean);
  return parts.length > 0 ? parts.join(' / ') : '维持现状';
}

function formatSigned(value: number): string {
  return value > 0 ? `+${Math.round(value)}` : String(Math.round(value));
}

function governanceFocusSource(region: RegionViewModel): string {
  const parts = [
    region.history?.uiSummary,
    region.geography.label,
    region.definition.legitimacyMemory.slice(0, 2).join('、'),
    region.geography.resources.slice(0, 2).join('、')
  ].filter(Boolean);
  return parts.join(' / ') || '据地区地貌、人口、法统记忆与资源结构抽象。';
}

function governanceLaborAction(laborId: GovernanceLaborId): UiAction {
  return `governance_labor_${laborId}` as UiAction;
}

function describeGovernanceProject(project: GovernanceProject): string {
  return `${project.regionName} ${project.buildingName}，剩余 ${project.remainingTurns}/${project.totalTurns} 旬，${formatGovernanceProjectYield(project)}`;
}

function formatGovernanceProjectYield(project: GovernanceProject): string {
  const parts = [
    project.foodYield !== 0 ? `粮 ${formatSigned(project.foodYield)}` : '',
    project.moneyYield !== 0 ? `钱 ${formatSigned(project.moneyYield)}` : '',
    project.contributionDelta !== 0 ? `贡献 ${formatSigned(project.contributionDelta)}` : '',
    project.integrationDelta !== 0 ? `整合 ${formatSigned(project.integrationDelta)}` : '',
    project.legitimacyDelta !== 0 ? `法统 ${formatSigned(project.legitimacyDelta)}` : '',
    project.riskDelta !== 0 ? `民变 ${formatSigned(project.riskDelta)}` : ''
  ].filter(Boolean);
  return parts.length > 0 ? parts.join(' / ') : '暂无直接收益';
}

function chooseBuildingForSpecialization(buildings: BuildingDefinition[], specialization: string, terrain: string): BuildingDefinition | undefined {
  const category =
    specialization.includes('粮') ? 'agriculture' :
    specialization.includes('军') || specialization.includes('边防') ? 'military' :
    specialization.includes('商') || terrain.includes('river_delta') ? 'economy' :
    specialization.includes('礼') || specialization.includes('法统') || specialization.includes('文化') ? 'culture' :
    'defense';
  return buildings.find((building) => building.category === category) ?? buildings[0];
}

function choosePolicyForGovernanceFocus(
  policies: PolicyDefinition[],
  focusId: GovernanceFocusId,
  region: RegionViewModel
): PolicyDefinition | undefined {
  const preferredIds: Record<GovernanceFocusId, string[]> = {
    grain: ['land_survey', 'relief_grain'],
    tax: ['land_survey', 'standardization'],
    military: ['conscription', 'standardization'],
    frontier: ['standardization', 'local_compromise'],
    legitimacy: ['standardization', 'local_compromise'],
    relief: ['relief_grain', 'local_compromise']
  };
  if (region.risk >= 28 && focusId !== 'tax' && focusId !== 'military') {
    return policies.find((policy) => policy.id === 'relief_grain') ?? policies[0];
  }
  for (const id of preferredIds[focusId]) {
    const policy = policies.find((candidate) => candidate.id === id);
    if (policy) return policy;
  }
  return policies[0];
}

function commandRouteLabel(command: WarCommand): string {
  const waypoint = command.waypointName ? `→经${command.waypointName}` : '';
  return `${command.fromName}${waypoint}→${command.targetName}`;
}

function commandRouteLegSummary(command: WarCommand): string {
  if (command.routeLegs.length <= 1) return `路线 ${command.routeUsage}/${command.routeCapacity}`;
  return `分段 ${command.routeLegs.map((leg) => `${leg.fromName}→${leg.toName} ${leg.routeUsage}/${leg.routeCapacity}`).join('；')}`;
}

function describeWarCommand(command: WarCommand): string {
  const alert = command.alert ? `，${command.alert}` : '';
  const segment = command.kind === 'supply' ? `，分段 ${command.completedSegments}/${command.segmentCount}，已送 ${command.deliveredSupply}/${command.plannedSupplyReserve}` : '';
  return `${commandKindName(command.kind)}：${commandRouteLabel(command)}，尚余 ${command.remainingTurns}/${command.totalTurns} 回合，${commandRouteLegSummary(command)}，补给余量 ${command.supplyReserve}${segment}，截粮 ${command.interceptionRisk}%${alert}`;
}

function tacticBadgeRow(modifier?: TacticalModifier): string {
  if (!modifier) {
    return '<div class="tactic-badge-row"><span class="tactic-badge neutral">补给 --</span><span class="tactic-badge neutral">接敌 --</span><span class="tactic-badge neutral">截粮 --</span><span class="tactic-badge neutral">占领 --</span></div>';
  }
  return `
    <div class="tactic-badge-row" data-testid="tactic-badge-row">
      ${tacticBadge('补给', modifier.supplyDelta, modifier.supplyDelta <= 0)}
      ${tacticBadge('接敌', modifier.contactDelta, modifier.contactDelta >= 0)}
      ${tacticBadge('截粮', modifier.interceptionDelta, modifier.interceptionDelta <= 0)}
      ${tacticBadge('占领', modifier.occupationDelta, modifier.occupationDelta <= 0)}
    </div>
  `;
}

function tacticBadge(label: string, value: number, beneficial: boolean): string {
  const tone = value === 0 ? 'neutral' : beneficial ? 'good' : 'bad';
  return `<span class="tactic-badge ${tone}">${escapeHtml(label)} ${escapeHtml(signedNumber(value))}</span>`;
}

function formatWarCommandWithBadge(command: WarCommand): string {
  const kind = commandKindName(command.kind);
  const kindClass = command.kind === 'attack' ? 'badge-danger' : command.kind === 'supply' ? 'badge-bronze' : 'badge-jade';
  const turnsLeft = command.remainingTurns;
  const turnsTotal = command.totalTurns;
  const progress = Math.round((turnsTotal - turnsLeft) / turnsTotal * 100);
  const alert = command.alert ? `<span class="badge badge-danger">截粮</span>` : '';
  const supplySegments = command.kind === 'supply'
    ? `<span class="badge badge-neutral">分段 ${command.completedSegments}/${command.segmentCount}</span><span class="badge badge-jade">送达 ${command.deliveredSupply}/${command.plannedSupplyReserve}</span>`
    : '';
  return `
    <div class="command-badge-row">
      <span class="badge ${kindClass}">${kind}</span>
      <span>${escapeHtml(commandRouteLabel(command))}</span>
      <span class="badge badge-neutral">${turnsLeft}/${turnsTotal}回合</span>
      <span class="badge badge-neutral">路线 ${command.routeUsage}/${command.routeCapacity}</span>
      ${supplySegments}
      ${alert}
      <div class="command-progress"><i style="width:${progress}%"></i></div>
    </div>
  `;
}

function describeOccupationTask(task: OccupationAftercareTask): string {
  return `${task.regionName}：${occupationStageName(task.stage)}，尚余 ${task.remainingTurns} 回合，贡献上限 ${task.contributionCap}%，风险 ${task.riskPressure}%`;
}

function describeOccupationSupplyTask(task: OccupationSupplyTask): string {
  const stage = task.stage === 'military-govern' ? '军管' : task.stage === 'pacify' ? '安抚' : '编户';
  const status = occupationSupplyStatusName(task.status);
  return `${task.convoyId}：${task.regionName}${stage}${status}，${task.routeLabel}，尚需 ${task.supplyNeeded}`;
}

function describeTransportConvoy(convoy: TransportConvoy): string {
  const legs = convoy.routeLegs.length > 1 ? `，${convoy.routeLegs.map((leg) => `${leg.fromName}→${leg.toName} ${leg.routeUsage}/${leg.routeCapacity}`).join('；')}` : '';
  return `${convoy.id}：${convoy.routeLabel}，${transportConvoyStatusName(convoy.status)}，优先 ${convoy.priority}，送达 ${convoy.deliveredSupply}/${convoy.plannedSupplyReserve}，路线 ${convoy.routeUsage}/${convoy.routeCapacity}${legs}`;
}

function summarizeRouteAlternatives(alternatives: RouteAlternative[]): string {
  return alternatives
    .map((alternative) => `${alternative.id}:${alternative.currentUsage}/${alternative.capacity},补${alternative.supplyCost},截${alternative.interceptionRisk}`)
    .join(' | ');
}

function routeNetworkHasLeg(network: RouteNetworkDefinition, fromId: string, toId: string): boolean {
  for (let index = 0; index < network.nodes.length - 1; index += 1) {
    const from = network.nodes[index];
    const to = network.nodes[index + 1];
    if ((from === fromId && to === toId) || (from === toId && to === fromId)) return true;
  }
  return false;
}

function routeCapacityKey(fromId: string, toId: string): string {
  return [fromId, toId].sort().join('->');
}

function nextCounterFromIds(ids: string[], prefix: string): number {
  return ids.reduce((next, id) => {
    if (!id.startsWith(prefix)) return next;
    const value = Number.parseInt(id.slice(prefix.length), 10);
    return Number.isFinite(value) ? Math.max(next, value + 1) : next;
  }, 1);
}

function weightedSupplyFactor(legs: RouteLegEstimate[]): number {
  const totalDistance = legs.reduce((sum, leg) => sum + leg.distance, 0);
  if (totalDistance <= 0) return legs[0]?.profile.supplyFactor ?? 1;
  return legs.reduce((sum, leg) => sum + leg.profile.supplyFactor * (leg.distance / totalDistance), 0);
}

function mostRestrictiveLeg(legs: RouteLegEstimate[]): RouteLegEstimate {
  return [...legs].sort((a, b) => a.capacity - b.capacity || b.profile.interceptionModifier - a.profile.interceptionModifier)[0] ?? legs[0];
}

function routeTerrainReason(legs: RouteLegEstimate[]): string {
  const bottleneck = mostRestrictiveLeg(legs);
  if (legs.length === 1) return bottleneck.profile.terrainReason;
  const names = legs.map((leg) => `${leg.from.definition.name}-${leg.target.definition.name}`).join('、');
  const network = routeNetworkLabel(legs);
  const networkText = network ? `；经过${network}` : '';
  return `${names}${networkText}；瓶颈段按${bottleneck.profile.bottleneckLabel}计，${bottleneck.profile.terrainReason}`;
}

function routeNetworkLabel(legs: RouteLegEstimate[]): string {
  return [...new Set(legs.map((leg) => leg.profile.networkLabel).filter((label): label is string => Boolean(label)))].join('、');
}

function transportConvoyStatusName(status: TransportConvoyStatus): string {
  const names: Record<TransportConvoyStatus, string> = {
    queued: '排队',
    moving: '在途',
    delivered: '已送达',
    cancelled: '已取消'
  };
  return names[status];
}

function transportConvoyStatusRank(status: TransportConvoyStatus): number {
  const ranks: Record<TransportConvoyStatus, number> = {
    moving: 0,
    queued: 1,
    delivered: 2,
    cancelled: 3
  };
  return ranks[status];
}

function convoyProgress(convoy: TransportConvoy): number {
  if (convoy.status === 'delivered') return 1;
  if (convoy.status === 'cancelled') return 0;
  const segmentProgress = convoy.segmentCount <= 0 ? 0 : convoy.completedSegments / convoy.segmentCount;
  return clamp(segmentProgress || (convoy.status === 'moving' ? 0.35 : 0.12), 0.08, 0.96);
}

function convoyMapProgress(convoy: TransportConvoy): number {
  return clamp(convoyProgress(convoy) + Math.min(0.2, convoy.orderIndex * 0.045), 0.1, 0.92);
}

function occupationSupplyStatusName(status: OccupationSupplyTask['status']): string {
  const names: Record<OccupationSupplyTask['status'], string> = {
    pending: '待运输',
    dispatched: '已派遣',
    'in-transit': '运输中',
    delivered: '运输已送达',
    cancelled: '已取消'
  };
  return names[status];
}

function occupationSupplyProgress(task: OccupationSupplyTask): number {
  if (task.status === 'delivered') return 1;
  if (task.status === 'cancelled') return 0;
  if (task.status === 'in-transit') return 0.58;
  if (task.status === 'dispatched') return 0.32;
  return 0.14;
}

function occupationSupplyMapProgress(task: OccupationSupplyTask): number {
  return clamp(occupationSupplyProgress(task) + Math.min(0.18, task.orderIndex * 0.035), 0.1, 0.92);
}

function logisticsObjectRank(kind: LogisticsMapObject['kind']): number {
  return kind === 'route-blockade' ? -1 : kind === 'transport-convoy' ? 0 : kind === 'occupation-supply' ? 1 : 2;
}

function routeBlockadeStatusName(status: RouteBlockadeStatus): string {
  const names: Record<RouteBlockadeStatus, string> = {
    'enemy-blockade': '敌方封锁',
    guarded: '己方守备',
    cleared: '已清除'
  };
  return names[status];
}

function describeRouteBlockade(blockade: RouteBlockade): string {
  return `${blockade.id}：${blockade.chokePointLabel}，${routeBlockadeStatusName(blockade.status)}，封锁 ${blockade.strength}，守备 ${blockade.guardStrength}，${blockade.lastAction}`;
}

function describeEnemyInterdictionOrder(order: EnemyInterdictionOrder): string {
  const counter = order.lastCountermeasure ? `，反制 ${order.lastCountermeasure}` : '';
  const choke = order.chokePointLabel ? `，盯防 ${order.chokePointLabel}` : '';
  return `敌方截粮命令：${order.routeLabel}${choke}，${enemyInterdictionStageName(order.stage)}，尚余 ${order.remainingTurns}/${order.totalTurns} 回合，风险 ${order.risk}%，预计损耗 ${order.supplyDamage}${counter}`;
}

function occupationStageName(stage: OccupationAftercareStage): string {
  const names: Record<OccupationAftercareStage, string> = {
    'military-govern': '军管',
    pacify: '安抚',
    register: '编户'
  };
  return names[stage];
}

function enemyInterdictionStageName(stage: EnemyInterdictionStage): string {
  const names: Record<EnemyInterdictionStage, string> = {
    planning: '筹划',
    moving: '机动',
    striking: '袭扰',
    resolved: '已解除'
  };
  return names[stage];
}

function enemyInterdictionStageShortName(stage: EnemyInterdictionStage): string {
  const names: Record<EnemyInterdictionStage, string> = {
    planning: '筹',
    moving: '机',
    striking: '袭',
    resolved: '解'
  };
  return names[stage];
}

function routePressureDetailHtml(routeShort: string, rows: Array<{ tone: 'enemy' | 'friendly' | 'forecast'; label: string; title: string; body: string }>): string {
  return `
    <div class="route-pressure-card" data-testid="route-pressure-card">
      <div class="route-pressure-card-title">位置详情：${escapeHtml(routeShort)}</div>
      <div class="route-pressure-card-rows">
        ${rows
          .map((row) => `
            <div class="route-pressure-row ${row.tone}" data-route-pressure-row="${row.tone}">
              <span>${escapeHtml(row.label)}</span>
              <b>${escapeHtml(row.title)}</b>
              <em>${escapeHtml(row.body)}</em>
            </div>
          `)
          .join('')}
      </div>
    </div>
  `;
}

function enemyInterdictionPositionName(stage: EnemyInterdictionStage): { label: string; progress: number } {
  const positions: Record<EnemyInterdictionStage, { label: string; progress: number }> = {
    planning: { label: '中后段伏击口', progress: 56 },
    moving: { label: '中段向目标推进', progress: 68 },
    striking: { label: '接敌前沿险段', progress: 84 },
    resolved: { label: '已解除', progress: 96 }
  };
  return positions[stage];
}

function friendlyCountermeasurePositionName(countermeasure: string | undefined, stage: EnemyInterdictionStage): { label: string; progress: number } {
  if (countermeasure === '护粮队') return { label: '补给车队外侧护送线', progress: stage === 'striking' ? 70 : 46 };
  if (countermeasure === '改道') return { label: '前段改道岔口', progress: 32 };
  if (countermeasure === '反斥候') return { label: '中段侦察压制点', progress: 52 };
  if (countermeasure === '诱敌') return { label: '中前段弃车诱敌点', progress: 44 };
  return { label: '未部署反制点', progress: 0 };
}

function friendlyCountermeasureEffect(countermeasure: string | undefined): string {
  if (countermeasure === '护粮队') return '外侧护送车队，压低风险和损耗';
  if (countermeasure === '改道') return '绕开高危段，牺牲速度换补给安全';
  if (countermeasure === '反斥候') return '清查伏点，削弱敌方截粮判断';
  if (countermeasure === '诱敌') return '弃车设伏，诱使截粮队提前暴露';
  return '尚未下达反制';
}

function enemyInterdictionStageByRemaining(remainingTurns: number): EnemyInterdictionStage {
  if (remainingTurns >= 3) return 'planning';
  if (remainingTurns === 2) return 'moving';
  return 'striking';
}

function commandKindName(kind: WarCommandKind): string {
  const names: Record<WarCommandKind, string> = {
    deploy: '军府',
    supply: '运输',
    scout: '侦察',
    fortify: '兵站',
    attack: '战役'
  };
  return names[kind];
}

function commandTurns(kind: WarCommandKind, forecast: RouteForecast): number {
  if (kind === 'attack') return Math.max(2, Math.round(forecast.turns + 1));
  if (kind === 'supply') return Math.max(2, Math.round(forecast.turns * 0.7 + 1));
  if (kind === 'fortify') return 2;
  return Math.max(1, Math.round(forecast.turns * 0.35));
}

function commandSegmentCount(kind: WarCommandKind, totalTurns: number): number {
  if (kind !== 'supply') return 1;
  return Math.max(2, Math.min(4, totalTurns));
}

function commandSupplyReserve(kind: WarCommandKind, forecast: RouteForecast): number {
  if (kind === 'supply') return Math.max(18, forecast.supplyCost + 8);
  if (kind === 'attack') return Math.max(8, Math.round(forecast.supplyCost * 0.45));
  if (kind === 'fortify') return Math.max(10, Math.round(forecast.supplyCost * 0.35));
  return Math.max(6, Math.round(forecast.supplyCost * 0.25));
}

function unitMixText(mix: Record<string, number>, units: UnitDefinition[]): string {
  return Object.entries(mix)
    .filter(([, value]) => value > 0)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 3)
    .map(([unitId, value]) => `${unitName(unitId, units)}${Math.round(value)}%`)
    .join(' / ');
}

function unitMixBars(mix: Record<string, number>, units: UnitDefinition[]): string {
  return Object.entries(mix)
    .filter(([, value]) => value > 0)
    .sort((a, b) => b[1] - a[1])
    .map(([unitId, value]) => `
      <div class="unit-mix-row">
        <span>${escapeHtml(unitName(unitId, units))}</span>
        <div class="unit-mix-meter"><i style="width:${Math.round(value)}%"></i></div>
        <b>${Math.round(value)}%</b>
      </div>
    `)
    .join('');
}

function unitName(unitId: string, units: UnitDefinition[]): string {
  return units.find((unit) => unit.id === unitId)?.name ?? unitId;
}

function dominantUnit(mix: Record<string, number>, units: UnitDefinition[]): UnitDefinition | undefined {
  const [unitId] = Object.entries(mix).sort((a, b) => b[1] - a[1])[0] ?? [];
  return unitId ? units.find((unit) => unit.id === unitId) : undefined;
}

function normalizeMix(mix: Record<string, number>): Record<string, number> {
  const total = Object.values(mix).reduce((sum, value) => sum + Math.max(0, value), 0);
  if (total <= 0) return { infantry: 100 };
  const normalized: Record<string, number> = {};
  let assigned = 0;
  const entries = Object.entries(mix).filter(([, value]) => value > 0);
  for (const [index, [unitId, value]] of entries.entries()) {
    const ratio = index === entries.length - 1 ? 100 - assigned : Math.round(value / total * 100);
    normalized[unitId] = ratio;
    assigned += ratio;
  }
  return normalized;
}

function formatGeneral(general: GeneralDefinition): string {
  return `${general.title}${general.name}`;
}

function signedNumber(value: number): string {
  return `${value > 0 ? '+' : ''}${Math.round(value)}`;
}

function outlinerItem(group: string, text: string, regionId: string): string {
  return `<button class="outliner-item" type="button" data-region-id="${escapeHtml(regionId)}"><span>${escapeHtml(group)}</span><b>${escapeHtml(text)}</b></button>`;
}

function formatEffects(effects: Record<string, number>): string {
  const labels: Record<string, string> = {
    food: '粮',
    money: '钱',
    legitimacy: '法统',
    rebellionRisk: '民变',
    integrationSpeed: '整合',
    taxEfficiency: '税效',
    manpowerToArmy: '兵力',
    localPower: '地方势力',
    armyMorale: '军心',
    mobility: '机动'
  };
  return Object.entries(effects)
    .map(([key, value]) => `${escapeHtml(labels[key] ?? key)}${value > 0 ? '+' : ''}${value}`)
    .join(' / ');
}

function zeroGovernanceLogisticsDelta(): GovernanceLogisticsDelta {
  return { capacityBonus: 0, supplyRelief: 0, interdictionRelief: 0, occupationBandwidthBonus: 0 };
}

function governanceLogisticsScore(delta: GovernanceLogisticsDelta): number {
  return delta.capacityBonus * 5 + delta.supplyRelief + delta.interdictionRelief + delta.occupationBandwidthBonus;
}

function governanceLogisticsDeltaFromPolicy(policy: PolicyDefinition | undefined): GovernanceLogisticsDelta {
  if (!policy) return zeroGovernanceLogisticsDelta();
  const delta = zeroGovernanceLogisticsDelta();
  const tags = [policy.category, ...policy.mechanicTags].join(' ');
  const effects = policy.effects;
  const risks = policy.risks;

  if (matches(tags, ['infrastructure', 'frontier', 'military_farm', 'reconstruction', 'registration'])) {
    delta.capacityBonus += 1;
  }
  if (matches(tags, ['centralization', 'bureaucracy', 'audit', 'law'])) {
    delta.supplyRelief += 1;
    delta.interdictionRelief += 1;
  }
  if (matches(tags, ['relief', 'mandate', 'governance', 'multiethnic'])) {
    delta.interdictionRelief += 2;
    delta.occupationBandwidthBonus += 1;
  }
  if (matches(tags, ['military', 'frontier', 'cavalry', 'control'])) {
    delta.interdictionRelief += 2;
  }

  delta.supplyRelief += Math.max(0, Math.round((effect(effects.integrationSpeed, 0) + effect(effects.taxEfficiency, 0) + effect(effects.taxBase, 0)) / 8));
  delta.interdictionRelief += Math.max(0, Math.round((-effect(effects.rebellionRisk, 0) + effect(effects.legitimacy, 0) + effect(effects.armyMorale, 0) - effect(risks.rebellionRisk, 0)) / 4));
  delta.occupationBandwidthBonus += Math.max(0, Math.round((effect(effects.integrationSpeed, 0) + effect(effects.taxBase, 0) - effect(risks.localPower, 0)) / 7));

  return normalizeGovernanceLogisticsDelta(delta);
}

function governanceLogisticsDeltaFromBuilding(building: BuildingDefinition | undefined): GovernanceLogisticsDelta {
  if (!building) return zeroGovernanceLogisticsDelta();
  const delta = zeroGovernanceLogisticsDelta();
  const effects = building.effects;

  if (building.category === 'infrastructure') {
    delta.capacityBonus += 2;
    delta.supplyRelief += 3;
    delta.interdictionRelief += 4;
    delta.occupationBandwidthBonus += 4;
  } else if (building.category === 'agriculture') {
    delta.capacityBonus += 1;
    delta.supplyRelief += 3;
    delta.occupationBandwidthBonus += 4;
  } else if (building.category === 'economy') {
    delta.capacityBonus += 1;
    delta.supplyRelief += 2;
    delta.occupationBandwidthBonus += 2;
  } else if (building.category === 'military' || building.category === 'defense') {
    delta.capacityBonus += 1;
    delta.interdictionRelief += 4;
    delta.occupationBandwidthBonus += 1;
  } else if (building.category === 'culture') {
    delta.interdictionRelief += 1;
  }

  delta.supplyRelief += Math.max(0, Math.round(effect(effects.food, 0) / 9));
  delta.supplyRelief += Math.max(0, Math.round(effect(effects.mobility, 0) / 3));
  delta.interdictionRelief += Math.max(0, Math.round((effect(effects.armyMorale, 0) - effect(effects.rebellionRisk, 0)) / 2));
  delta.occupationBandwidthBonus += Math.max(0, Math.round((effect(effects.integrationSpeed, 0) + effect(effects.taxEfficiency, 0)) / 3));

  return normalizeGovernanceLogisticsDelta(delta);
}

function normalizeGovernanceLogisticsDelta(delta: GovernanceLogisticsDelta): GovernanceLogisticsDelta {
  return {
    capacityBonus: clamp(Math.round(delta.capacityBonus), 0, 3),
    supplyRelief: clamp(Math.round(delta.supplyRelief), 0, 12),
    interdictionRelief: clamp(Math.round(delta.interdictionRelief), 0, 18),
    occupationBandwidthBonus: clamp(Math.round(delta.occupationBandwidthBonus), 0, 12)
  };
}

function formatGovernanceLogisticsDelta(delta: GovernanceLogisticsDelta): string {
  const normalized = normalizeGovernanceLogisticsDelta(delta);
  const parts = [
    normalized.capacityBonus > 0 ? `容量 +${normalized.capacityBonus}` : '',
    normalized.supplyRelief > 0 ? `补给 -${normalized.supplyRelief}` : '',
    normalized.interdictionRelief > 0 ? `截粮 -${normalized.interdictionRelief}` : '',
    normalized.occupationBandwidthBonus > 0 ? `安抚带宽 +${normalized.occupationBandwidthBonus}` : ''
  ].filter(Boolean);
  return parts.length > 0 ? parts.join(' / ') : '无新增';
}

function formatNumber(value: number): string {
  if (value >= 10000) return `${Math.round(value / 10000)}万`;
  return String(value);
}

function terrainName(terrain: string): string {
  const names: Record<string, string> = {
    plain: '平原',
    hill: '丘陵',
    mountain: '山地',
    basin: '盆地',
    river_delta: '水网',
    river_plain: '河谷',
    subtropical: '岭南',
    plateau: '高原'
  };
  return names[terrain] ?? terrain;
}

function openRouteProfile(reason: string): RouteTerrainProfile {
  return {
    roadClass: 'open-road',
    bottleneckLabel: '平原官道',
    baseCapacity: 3,
    supplyFactor: 0.95,
    interceptionModifier: -2,
    terrainReason: reason
  };
}

function statusRank(status: OccupationSupplyTask['status']): number {
  const ranks: Record<OccupationSupplyTask['status'], number> = {
    'in-transit': 0,
    pending: 1,
    dispatched: 2,
    delivered: 3,
    cancelled: 4
  };
  return ranks[status];
}

function climateName(climate?: string): string {
  if (!climate) return '气候待补';
  const names: Record<string, string> = {
    temperate_loess: '温带黄土',
    northwest_dry_highland: '西北旱地',
    temperate_river_plain: '温带河谷',
    mountain_basin: '山地盆地',
    river_delta: '水网三角洲',
    subtropical_humid: '湿热岭南',
    plateau_cold: '高原寒地'
  };
  return names[climate] ?? climate.replaceAll('_', ' ');
}

function formatTags(tags: string[]): string {
  return tags
    .filter(Boolean)
    .map((tag) => `<span class="tag-chip">${escapeHtml(tag)}</span>`)
    .join('');
}

function ownerName(owner: RegionViewModel['owner']): string {
  if (owner === 'player') return '己方';
  if (owner === 'rival') return '敌对';
  return '边地';
}

function effect(value: number | undefined, fallback: number): number {
  return typeof value === 'number' ? value : fallback;
}

function matches(value: string, needles: string[]): boolean {
  const normalized = value.toLowerCase();
  return needles.some((needle) => normalized.includes(needle.toLowerCase()));
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}

function trimTo<T>(items: T[], max: number): void {
  items.splice(max);
}

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

// ============================================
// Logistics Dispatcher System - Core Methods
// ============================================

/** Station Management - Build and maintain logistics stations */
function buildLogisticsStation(
  stations: LogisticsStation[],
  regionId: string,
  regionName: string,
  counter: number
): LogisticsStation {
  const existing = stations.find(s => s.regionId === regionId);
  if (existing) {
    existing.isActive = true;
    return existing;
  }
  const station: LogisticsStation = {
    id: `station_${counter++}`,
    regionId,
    regionName,
    supplyBonus: 8,
    moraleBonus: 3,
    riskReduction: 2,
    isActive: true
  };
  stations.push(station);
  return station;
}

/** Route Capacity - Calculate and track route capacity constraints */
function updateRouteCapacity(
  capacities: Map<string, RouteCapacityConstraint>,
  fromRegion: string,
  toRegion: string,
  currentUsage: number,
  maxArmies = 3,
  profile: RouteTerrainProfile = openRouteProfile('按普通驿道处理')
): RouteCapacityConstraint {
  const key = routeCapacityKey(fromRegion, toRegion);
  const existing = capacities.get(key);
  const usage = currentUsage;

  let congestion: RouteCapacityConstraint['congestionLevel'] = 'low';
  if (usage >= maxArmies) congestion = 'critical';
  else if (usage >= maxArmies * 0.66) congestion = 'high';
  else if (usage >= maxArmies * 0.33) congestion = 'medium';

  const constraint: RouteCapacityConstraint = {
    routeId: key,
    fromRegion,
    toRegion,
    maxArmies,
    currentUsage: usage,
    congestionLevel: congestion,
    roadClass: profile.roadClass,
    bottleneckLabel: profile.bottleneckLabel,
    terrainReason: profile.terrainReason,
    networkId: profile.networkId,
    networkLabel: profile.networkLabel
  };
  capacities.set(key, constraint);
  return constraint;
}

/** Interdiction Priority - Smart selection of interdiction targets */
function calculateInterdictionPriority(
  orders: EnemyInterdictionOrder[]
): InterdictionPriority[] {
  return orders
    .filter(o => !o.resolved)
    .map(order => {
      const riskScore = order.risk * 0.4 + order.supplyDamage * 6;
      let recommendation: InterdictionPriority['recommendedCountermeasure'] = 'escort';
      let reasoning = order.chokePointLabel ? `护住${order.chokePointLabel}` : '基础护粮队反制';

      if (order.risk >= 70) {
        recommendation = 'decoy';
        reasoning = order.chokePointLabel ? `高风险：围绕${order.chokePointLabel}诱敌` : '高风险：诱敌分散注意力';
      } else if (order.risk >= 50) {
        recommendation = 'counter-scout';
        reasoning = order.chokePointLabel ? `中高风险：反斥候清理${order.chokePointLabel}` : '中高风险：反斥候侦察削弱';
      } else if (order.supplyDamage >= 5) {
        recommendation = 'reroute';
        reasoning = order.chokePointLabel ? `高损耗：绕开${order.chokePointLabel}` : '高损耗：改道规避';
      }

      return {
        orderId: order.id,
        targetRegion: order.targetRegionId,
        targetName: order.targetName,
        riskLevel: order.risk,
        supplyDamage: order.supplyDamage,
        priority: Math.round(riskScore),
        recommendedCountermeasure: recommendation,
        reasoning
      };
    })
    .sort((a, b) => b.priority - a.priority);
}

/** Occupation Supply Automation - Auto-dispatch supplies for occupied regions */
function createOccupationSupplyTask(
  tasks: OccupationSupplyTask[],
  fromRegionId: string,
  regionId: string,
  regionName: string,
  fromRegionName: string,
  stage: OccupationSupplyTask['stage'],
  contributionCap: number,
  turn: number,
  counter: number,
  bandwidthUsed: number
): OccupationSupplyTask {
  const supplyNeeded = Math.round(contributionCap * 0.5 + 10);
  const priority = stage === 'military-govern' ? 3 : stage === 'pacify' ? 2 : 1;

  const task: OccupationSupplyTask = {
    taskId: `occ_supply_${counter}`,
    convoyId: `安抚车队-${counter}`,
    fromRegionId,
    regionId,
    regionName,
    routeLabel: `${fromRegionName}→${regionName}`,
    stage,
    supplyNeeded,
    priority,
    orderIndex: counter,
    bandwidthUsed,
    routeUsageClaimed: true,
    autoDispatchTurn: turn + 1,
    status: 'pending'
  };
  tasks.unshift(task);
  return task;
}

function autoDispatchSupplies(
  tasks: OccupationSupplyTask[],
  activeArmies: ArmyViewModel[],
  currentTurn: number
): string[] {
  const dispatched: string[] = [];

  for (const task of tasks) {
    if (task.status !== 'pending') continue;
    if (task.autoDispatchTurn > currentTurn) continue;

    const suitableArmy = activeArmies.find(army =>
      army.faction === 'player' &&
      army.supply > 70 &&
      army.targetRegionId === task.regionId
    );

    if (suitableArmy) {
      task.status = 'dispatched';
      dispatched.push(`${task.regionName}安抚运输已自动派遣`);
    }
  }

  return dispatched;
}
