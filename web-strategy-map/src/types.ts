export type GameMode = 'governance' | 'war';
export type GovernanceFocusId = 'grain' | 'tax' | 'military' | 'frontier' | 'legitimacy' | 'relief';
export type GovernanceLaborId = 'balanced' | 'grain' | 'tax' | 'military' | 'stability';
export type UiAction =
  | 'governance_policy'
  | 'governance_build'
  | 'governance_reinforce'
  | 'governance_relief'
  | 'governance_registry'
  | 'governance_focus_grain'
  | 'governance_focus_tax'
  | 'governance_focus_military'
  | 'governance_focus_frontier'
  | 'governance_focus_legitimacy'
  | 'governance_focus_relief'
  | 'governance_labor_balanced'
  | 'governance_labor_grain'
  | 'governance_labor_tax'
  | 'governance_labor_military'
  | 'governance_labor_stability'
  | 'governance_advance_turn'
  | 'war_deploy'
  | 'war_supply'
  | 'war_scout'
  | 'war_fortify'
  | 'war_attack'
  | 'war_advance_turn'
  | 'war_counter_escort'
  | 'war_counter_reroute'
  | 'war_counter_scout'
  | 'war_counter_decoy'
  | 'route_blockade_guard'
  | 'route_blockade_clear'
  | 'occupation_aftercare'
  | 'army_order_balanced'
  | 'army_order_forced_march'
  | 'army_order_defensive'
  | 'army_order_flank'
  | 'army_order_reserve'
  | 'route_pick_target'
  | 'route_pick_waypoint'
  | 'route_waypoint_clear'
  | 'route_queue_promote'
  | 'route_queue_cancel'
  | 'transport_convoy_promote'
  | 'transport_convoy_cancel'
  | 'occupation_supply_promote'
  | 'occupation_supply_cancel'
  | 'army_split'
  | 'army_merge'
  | 'army_general_next'
  | 'army_mix_balanced'
  | 'army_mix_cavalry'
  | 'army_mix_crossbow'
  | 'army_mix_siege'
  | 'save_slot_1'
  | 'save_slot_2'
  | 'save_slot_3'
  | 'load_slot_1'
  | 'load_slot_2'
  | 'load_slot_3'
  | 'delete_slot_1'
  | 'delete_slot_2'
  | 'delete_slot_3';

export interface JsonCollection<T> {
  schemaVersion: number;
  items: T[];
}

export interface MapPoint {
  x: number;
  y: number;
}

export interface RegionShape {
  id: string;
  regionId: string;
  center: MapPoint;
  labelOffset?: MapPoint;
  renderOrder?: number;
  boundary: MapPoint[];
}

export interface RegionDefinition {
  id: string;
  name: string;
  terrain: string;
  population: number;
  foodOutput: number;
  taxOutput: number;
  manpower: number;
  landStructure: Record<string, number>;
  legitimacyMemory: string[];
  regionSpecialization?: string;
  supplyNode?: boolean;
  gameplaySourceReference?: string;
  localPower: number;
  rebellionRisk: number;
  neighbors: string[];
  eraProfile?: Record<string, string>;
}

export interface HistoricalLayerDefinition {
  id: string;
  regionId: string;
  climateZone: string;
  geographyTags: string[];
  customTags: string[];
  weaponTraditions: string[];
  strategicResources: string[];
  yieldModifiers?: Record<string, number>;
  uiSummary?: string;
}

export interface BuildingDefinition {
  id: string;
  name: string;
  category: string;
  cost: number;
  effects: Record<string, number>;
  sourceReference: string;
}

export interface PolicyDefinition {
  id: string;
  name: string;
  category: string;
  cost: Record<string, number>;
  effects: Record<string, number>;
  risks: Record<string, number>;
  sourceReference: string;
  mechanicTags: string[];
}

export interface UnitDefinition {
  id: string;
  name: string;
  category: string;
  stats: Record<string, number>;
  upkeep: Record<string, number>;
}

export type RouteRoadClass = 'open-road' | 'river-road' | 'hill-road' | 'pass-bottleneck' | 'frontier-track' | 'water-network';

export interface RouteNetworkBlockadeDefinition {
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
}

export interface RouteNetworkDefinition {
  id: string;
  label: string;
  nodes: string[];
  roadClass: RouteRoadClass;
  baseCapacity: number;
  supplyFactor: number;
  interceptionModifier: number;
  reason: string;
  blockade: RouteNetworkBlockadeDefinition;
  sourceReference: string;
}

export interface GeneralDefinition {
  id: string;
  portraitAssetPath: string;
  name: string;
  title: string;
  era: string;
  military: number;
  loyalty: number;
  specialAbility: string;
  specialAbilityName: string;
  specialAbilityDesc: string;
  terrainBonus: Record<string, number>;
  unitBonus: Record<string, number>;
  sourceReference: string;
}

export interface PortraitDefinition {
  id: string;
  emperorId: string;
  assetPath: string;
  sourceStatus: string;
  version: string;
  visualIdentity?: {
    silhouette?: string;
    costume?: string;
    props?: string[];
    palette?: string[];
    backgroundMotif?: string;
    expression?: string;
  };
  prompt?: string;
  uiCropHints?: {
    headCenterX?: number;
    headCenterY?: number;
    safeScale?: number;
  };
}

export interface EmperorDefinition {
  id: string;
  name: string;
  title: string;
  versionScope: string[];
  civilization: string;
  mapScope: string;
  era: string;
  legitimacyTypes: string[];
  globalMechanicTag: string;
  stats: Record<string, number>;
  score: {
    virtue: number;
    wisdom: number;
    physique: number;
    aesthetics: number;
    diligence: number;
    ambition: number;
    dignity: number;
    tolerance: number;
    selfControl: number;
    personnelManagement: number;
    nationalPower: number;
    popularSupport: number;
  };
  uniqueMechanic: {
    id: string;
    name: string;
    description: string;
  };
  diplomacySkills: Array<{
    id: string;
    name: string;
    moneyCost: number;
    talentCost: number;
    cooldownTurns: number;
  }>;
  historicalBurdens: string[];
  preferredPolicies: string[];
  aiPersonality: {
    expansion: number;
    governance: number;
    riskTolerance: number;
  };
  sourceReference: string;
}

export interface SceneMusicCue {
  scene: string;
  musicCueId: string;
  fileName: string;
  mood: string;
  bpm: number;
  tags: string[];
  description: string;
}

export interface EmperorThemeCue {
  emperorId: string;
  musicCueId: string;
  fileName: string;
  mood: string;
  bpm: number;
  tags: string[];
  historicalContext: string;
}

export interface ChronicleEventMusicCue {
  eventId: string;
  musicCueId: string;
  fileName: string;
  category?: string;
  mood: string;
  bpm: number;
  tags: string[];
  historicalContext?: string;
}

export interface ChronicleTurnWindow {
  startTurn: number;
  endTurn: number;
}

export interface ChronicleChoiceDefinition {
  id: string;
  label: string;
  effects?: Record<string, number>;
  risks?: Record<string, number>;
  followUpTags?: string[];
}

export interface ChronicleEventDefinition {
  id: string;
  name: string;
  eventType: string;
  eraScope?: string[];
  turnWindow?: ChronicleTurnWindow;
  regionScopeTags?: string[];
  requiredTechs?: string[];
  weatherTags?: string[];
  astronomyTags?: string[];
  triggerWeight: number;
  choices?: ChronicleChoiceDefinition[];
  cooldownTurns?: number;
  uiSummary: string;
}

export interface NarrationSegment {
  segmentId: string;
  text: string;
  trigger: string;
  priority: number;
}

export interface EmperorVoiceDefinition {
  emperorId: string;
  emperorName: string;
  voiceProfile: string;
  personality: string;
  lines: Record<string, string>;
}

export interface NarrationScript {
  schemaVersion: number;
  description: string;
  tutorial: {
    title: string;
    segments: NarrationSegment[];
  };
  emperor_voices: EmperorVoiceDefinition[];
}

export interface MapRenderMetadata {
  imageSize: { width: number; height: number };
  shapeCenter: MapPoint;
  pixelsPerShapeUnit: number;
  spritePixelsPerUnit: number;
}

export interface GeographyProfile {
  kind: string;
  label: string;
  description: string;
  sourceTags: string[];
  resources: string[];
}

export interface RegionViewModel {
  definition: RegionDefinition;
  shape: RegionShape;
  history?: HistoricalLayerDefinition;
  geography: GeographyProfile;
  owner: 'player' | 'rival' | 'frontier';
  controlStage: 'controlled' | 'newly-held' | 'military-govern' | 'pacify' | 'register';
  integration: number;
  contribution: number;
  risk: number;
  legitimacy: number;
  specialization: string;
  governanceFocus: GovernanceFocusId;
  laborFocus: GovernanceLaborId;
  recommendedBuilding?: BuildingDefinition;
  recommendedPolicy?: PolicyDefinition;
  sourceText: string;
}

export interface ArmyViewModel {
  id: string;
  name: string;
  faction: 'player' | 'rival';
  fromRegionId: string;
  targetRegionId: string;
  waypointRegionId?: string;
  soldiers: number;
  supply: number;
  morale: number;
  general: string;
  generalId?: string;
  unit: UnitDefinition;
  unitMix: Record<string, number>;
}

export interface RouteForecast {
  army: ArmyViewModel;
  from: RegionViewModel;
  target: RegionViewModel;
  waypoint?: RegionViewModel;
  supplyCost: number;
  turns: number;
  contactChance: number;
  occupationCost: number;
  interceptionRisk: number;
  routeCapacity?: number;
  routeUsage?: number;
  roadClass?: RouteRoadClass;
  bottleneckLabel?: string;
  terrainReason?: string;
  summary: string;
}

export interface LogisticsMapObject {
  id: string;
  kind: 'transport-convoy' | 'occupation-supply' | 'logistics-station' | 'route-blockade';
  regionId: string;
  fromRegionId?: string;
  targetRegionId?: string;
  label: string;
  routeLabel: string;
  status: string;
  progress: number;
  priority: number;
  details: string;
}
