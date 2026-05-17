import type {
  ArmyViewModel,
  BuildingDefinition,
  ChronicleEventDefinition,
  ChronicleEventMusicCue,
  EmperorDefinition,
  EmperorThemeCue,
  GeneralDefinition,
  GeographyProfile,
  GovernanceFocusId,
  GovernanceLaborId,
  HistoricalLayerDefinition,
  JsonCollection,
  MapRenderMetadata,
  NarrationScript,
  PolicyDefinition,
  PortraitDefinition,
  RegionDefinition,
  RouteNetworkDefinition,
  RegionShape,
  RegionViewModel,
  SceneMusicCue,
  RouteForecast,
  UnitDefinition
} from './types';

const dataRoot = '/game-data/data';
const assetRoot = '/game-data';

export class StrategyDatasetLoadError extends Error {
  public readonly fileName: string;
  public readonly reason: string;
  public readonly originalError: unknown;

  constructor(fileName: string, reason: string, originalError?: unknown) {
    super(`Failed to load strategy ${describeDatasetFile(fileName)}: ${reason}`);
    this.name = 'StrategyDatasetLoadError';
    this.fileName = fileName;
    this.reason = reason;
    this.originalError = originalError;
  }
}

export interface StrategyDataset {
  metadata: MapRenderMetadata;
  regions: RegionViewModel[];
  regionById: Map<string, RegionViewModel>;
  emperors: EmperorDefinition[];
  portraits: PortraitDefinition[];
  portraitByEmperorId: Map<string, PortraitDefinition>;
  generals: GeneralDefinition[];
  units: UnitDefinition[];
  routeNetworks: RouteNetworkDefinition[];
  policies: PolicyDefinition[];
  buildings: BuildingDefinition[];
  chronicleEvents: ChronicleEventDefinition[];
  armies: ArmyViewModel[];
  route: RouteForecast;
  audio: {
    sceneMusic: SceneMusicCue[];
    emperorThemes: EmperorThemeCue[];
    chronicleEvents: ChronicleEventMusicCue[];
    narration: NarrationScript;
  };
  nation: {
    food: number;
    money: number;
    army: number;
    legitimacy: number;
  };
}

export async function loadStrategyDataset(): Promise<StrategyDataset> {
  const [
    regionsData,
    shapesData,
    historyData,
    emperorsData,
    portraitsData,
    generalsData,
    policiesData,
    buildingsData,
    chronicleEventsData,
    unitsData,
    routeNetworksData,
    metadata,
    sceneMusic,
    emperorThemes,
    chronicleMusic,
    narration
  ] = await Promise.all([
    loadCollection<RegionDefinition>('regions.json'),
    loadCollection<RegionShape>('map_region_shapes.json'),
    loadCollection<HistoricalLayerDefinition>('historical_layers.json'),
    loadCollection<EmperorDefinition>('emperors.json'),
    loadCollection<PortraitDefinition>('portraits.json'),
    loadCollection<GeneralDefinition>('generals.json'),
    loadCollection<PolicyDefinition>('policies.json'),
    loadCollection<BuildingDefinition>('buildings.json'),
    loadCollection<ChronicleEventDefinition>('chronicle_events.json'),
    loadCollection<UnitDefinition>('units.json'),
    loadCollection<RouteNetworkDefinition>('route_networks.json'),
    loadJson<MapRenderMetadata>('map_render_metadata.json'),
    loadJson<JsonCollection<SceneMusicCue>>('../audio/scene_music.json'),
    loadJson<JsonCollection<EmperorThemeCue>>('../audio/emperor_themes.json'),
    loadJson<JsonCollection<ChronicleEventMusicCue>>('../audio/chronicle_event_music.json'),
    loadJson<NarrationScript>('../audio/narration_script.json')
  ]);

  validateRegionDefinitions(regionsData.items);
  validateRegionShapeCoverage(regionsData.items, shapesData.items);

  const shapeByRegion = new Map(shapesData.items.map((shape) => [shape.regionId, shape]));
  const historyByRegion = new Map(historyData.items.map((history) => [history.regionId, history]));
  const policies = policiesData.items;
  const buildings = buildingsData.items;
  const chronicleEvents = chronicleEventsData.items;
  const portraits = portraitsData.items;
  const portraitByEmperorId = new Map(portraits.map((portrait) => [portrait.emperorId, portrait]));
  const generals = generalsData.items;

  const playerCore = new Set(['guanzhong', 'chang_an', 'xianyang', 'yongzhou', 'longxi', 'hexi', 'liangzhou']);
  const rivalCore = new Set(['hanzhong', 'bashu', 'chengdu', 'luoyang', 'hedong', 'zhongyuan']);

  const regions: RegionViewModel[] = [];
  for (const definition of regionsData.items) {
    const shape = shapeByRegion.get(definition.id);
    if (!shape) continue;
    const history = historyByRegion.get(definition.id);
    const owner = playerCore.has(definition.id) ? 'player' : rivalCore.has(definition.id) ? 'rival' : 'frontier';
    const risk = clamp(definition.rebellionRisk + Math.max(0, definition.localPower - 48) * 0.35, 0, 100);
    const integration = owner === 'player' ? 72 : owner === 'rival' ? 34 : 48;
    const contribution = owner === 'player' ? 78 : owner === 'rival' ? 32 : 52;
    const legitimacy = clamp(55 + (definition.legitimacyMemory?.length ?? 0) * 8 - risk * 0.18, 0, 100);
    const specialization = resolveSpecialization(definition, history);
    const governanceFocus = focusFromSpecialization(specialization);
    const laborFocus = laborFromGovernanceFocus(governanceFocus);
    const geography = resolveGeographyProfile(definition, history);
    const recommendedBuilding = chooseBuilding(buildings, specialization, definition.terrain);
    const recommendedPolicy = choosePolicy(policies, definition, history, owner);
    const sourceText = [
      history?.uiSummary,
      recommendedPolicy ? `${recommendedPolicy.name}：${recommendedPolicy.sourceReference}` : undefined,
      recommendedBuilding ? `${recommendedBuilding.name}：${recommendedBuilding.sourceReference}` : undefined
    ]
      .filter(Boolean)
      .join(' / ');

    const region: RegionViewModel = {
      definition,
      shape,
      geography,
      owner,
      controlStage: owner === 'player' ? 'controlled' : owner === 'rival' ? 'military-govern' : 'newly-held',
      integration,
      contribution,
      risk,
      legitimacy,
      specialization,
      governanceFocus,
      laborFocus,
      recommendedBuilding,
      recommendedPolicy,
      sourceText
    };
    if (history) region.history = history;
    regions.push(region);
  }

  const regionById = new Map(regions.map((region) => [region.definition.id, region]));
  const units = unitsData.items;
  const cavalry = units.find((unit) => unit.id.includes('cavalry')) ?? units[0];
  const infantry = units.find((unit) => unit.id === 'infantry') ?? units[0];

  const armies: ArmyViewModel[] = [
    {
      id: 'army_player_1',
      name: '关中前军',
      faction: 'player',
      fromRegionId: 'guanzhong',
      targetRegionId: 'hanzhong',
      soldiers: 18000,
      supply: 76,
      morale: 84,
      generalId: 'li_jing',
      general: formatGeneralName(generals, 'li_jing', '王翦旧制军府'),
      unit: cavalry,
      unitMix: { cavalry: 58, infantry: 26, crossbowmen: 16 }
    },
    {
      id: 'army_player_2',
      name: '河西骑军',
      faction: 'player',
      fromRegionId: 'hexi',
      targetRegionId: 'liangzhou',
      soldiers: 9200,
      supply: 68,
      morale: 79,
      generalId: 'wei_qing',
      general: formatGeneralName(generals, 'wei_qing', '河西都护'),
      unit: cavalry,
      unitMix: { cavalry: 72, infantry: 18, crossbowmen: 10 }
    },
    {
      id: 'army_rival_1',
      name: '汉中守军',
      faction: 'rival',
      fromRegionId: 'hanzhong',
      targetRegionId: 'guanzhong',
      soldiers: 12600,
      supply: 61,
      morale: 72,
      generalId: 'guan_yu',
      general: formatGeneralName(generals, 'guan_yu', '地方都尉'),
      unit: infantry,
      unitMix: { infantry: 68, crossbowmen: 20, siege_engineer: 12 }
    }
  ];

  const route = buildRouteForecast(armies[0], regionById);
  const nation = {
    food: aggregateNationFood(regions.map(regionToNationAggregationInput)),
    money: aggregateNationMoney(regions.map(regionToNationAggregationInput)),
    army: armies.filter((army) => army.faction === 'player').reduce((sum, army) => sum + army.soldiers, 0),
    legitimacy: Math.round(regions.filter((r) => r.owner === 'player').reduce((sum, r) => sum + r.legitimacy, 0) / Math.max(1, regions.filter((r) => r.owner === 'player').length))
  };

  return {
    metadata,
    regions,
    regionById,
    emperors: emperorsData.items,
    portraits,
    portraitByEmperorId,
    generals,
    units,
    routeNetworks: routeNetworksData.items,
    policies,
    buildings,
    chronicleEvents,
    armies,
    route,
    audio: {
      sceneMusic: sceneMusic.items,
      emperorThemes: emperorThemes.items,
      chronicleEvents: chronicleMusic.items,
      narration
    },
    nation
  };
}

export interface NationAggregationInput {
  owner: string;
  foodOutput: number;
  taxOutput?: number;
  contribution: number;
}

export function aggregateNationFood(regions: NationAggregationInput[]): number {
  return aggregateNationMetric(regions, (region) => region.foodOutput);
}

export function aggregateNationMoney(regions: NationAggregationInput[]): number {
  return aggregateNationMetric(regions, (region) => region.taxOutput ?? 0);
}

export function gameDataAssetUrl(assetPath: string): string {
  const normalizedPath = assetPath.replace(/\\/g, '/').replace(/^\/+/, '');
  const safeSegments = normalizedPath
    .split('/')
    .filter((segment) => segment.length > 0 && segment !== '.' && segment !== '..');
  if (safeSegments.length === 0) return `${assetRoot}/`;
  return `${assetRoot}/${safeSegments.map(encodeURIComponent).join('/')}`;
}

function resolveGeographyProfile(region: RegionDefinition, history?: HistoricalLayerDefinition): GeographyProfile {
  const sourceTags = [...(history?.geographyTags ?? [])];
  const resources = [...(history?.strategicResources ?? [])];
  const tags = [region.terrain, history?.climateZone, ...sourceTags, ...resources].filter(Boolean).join(' ');
  const description = history?.uiSummary ?? `${region.name}当前使用${region.terrain}地形和地区资源生成地图地貌。`;

  if (matches(tags, ['hexi_corridor', 'oasis', 'silk_road_start'])) {
    return profile('arid_corridor', '河西走廊 / 绿洲道', description, sourceTags, resources);
  }
  if (matches(tags, ['river_delta', 'wetland', 'lake_marsh', 'river_network'])) {
    return profile('water_network', '江河水网 / 湿地', description, sourceTags, resources);
  }
  if (matches(tags, ['mountain_pass', 'mountain_gate', 'fortified_pass', 'pass_network', 'upland_pass', 'river_gate'])) {
    return profile('mountain_pass', '山地关隘 / 盆地门户', description, sourceTags, resources);
  }
  if (matches(tags, ['fertile_basin', 'basin_farmland', 'mountain_ring'])) {
    return profile('basin_granary', '盆地粮仓', description, sourceTags, resources);
  }
  if (matches(tags, ['loess_plain', 'river_irrigation', 'capital_corridor'])) {
    return profile('loess_irrigation', '黄土平原 / 灌渠', description, sourceTags, resources);
  }
  if (matches(tags, ['horse_route', 'steppe_edge', 'pasture_edge', 'frontier_corridor', 'frontier_plain'])) {
    return profile('frontier_horse_route', '边地马道 / 草场', description, sourceTags, resources);
  }
  if (matches(tags, ['mountain_coast', 'subtropical_coast', 'harbor_pockets', 'harbors', 'coastal_river'])) {
    return profile('mountain_coast_harbor', '山海港湾', description, sourceTags, resources);
  }
  if (matches(tags, ['plateau', 'mountain_plateau'])) {
    return profile('plateau_frontier', '高原边地', description, sourceTags, resources);
  }
  if (matches(tags, ['river_bend', 'river_plain', 'grain_corridor', 'river_border'])) {
    return profile('river_grain_corridor', '河谷粮廊', description, sourceTags, resources);
  }
  if (matches(tags, ['forest', 'forest_frontier'])) {
    return profile('forest_frontier', '林地边境', description, sourceTags, resources);
  }
  if (matches(tags, ['mineral_zone', 'minerals'])) {
    return profile('mineral_mountain', '矿山高地', description, sourceTags, resources);
  }
  if (matches(tags, ['open_plain', 'population_core', 'grain_zone'])) {
    return profile('central_plain', '平原粮田 / 人口核心', description, sourceTags, resources);
  }

  return profile('balanced_landform', `${terrainName(region.terrain)}地貌`, description, sourceTags, resources);
}

function profile(
  kind: string,
  label: string,
  description: string,
  sourceTags: string[],
  resources: string[]
): GeographyProfile {
  return { kind, label, description, sourceTags, resources };
}

async function loadCollection<T>(fileName: string): Promise<JsonCollection<T>> {
  const collection = await loadJson<unknown>(fileName);
  if (!isJsonObject(collection) || !Array.isArray(collection.items)) {
    throw new StrategyDatasetLoadError(fileName, 'invalid collection schema: items must be an array');
  }

  return collection as JsonCollection<T>;
}

function validateRegionDefinitions(regions: RegionDefinition[]): void {
  const regionById = new Map<string, RegionDefinition>();
  for (const region of regions) {
    const id = typeof region?.id === 'string' ? region.id.trim() : '';
    if (!id) {
      throw new StrategyDatasetLoadError('regions.json', 'region item is missing id');
    }
    if (regionById.has(id)) {
      throw new StrategyDatasetLoadError('regions.json', `duplicate region id: ${id}`);
    }
    regionById.set(id, region);
  }

  for (const region of regions) {
    const neighbors = Array.isArray(region.neighbors) ? region.neighbors : [];
    for (const neighborId of neighbors) {
      if (!regionById.has(neighborId)) {
        throw new StrategyDatasetLoadError('regions.json', `region ${region.id} references unknown neighbor ${neighborId}`);
      }
      const neighbor = regionById.get(neighborId);
      const reverseNeighbors = Array.isArray(neighbor?.neighbors) ? neighbor.neighbors : [];
      if (!reverseNeighbors.includes(region.id)) {
        throw new StrategyDatasetLoadError('regions.json', `neighbor edge is not bidirectional: ${region.id} -> ${neighborId}`);
      }
    }
  }
}

function validateRegionShapeCoverage(regions: RegionDefinition[], shapes: RegionShape[]): void {
  const shapeRegionIds = new Set<string>();
  for (const shape of shapes) {
    const regionId = typeof shape?.regionId === 'string' ? shape.regionId.trim() : '';
    if (!regionId) {
      throw new StrategyDatasetLoadError('map_region_shapes.json', 'shape item is missing regionId');
    }
    if (shapeRegionIds.has(regionId)) {
      throw new StrategyDatasetLoadError('map_region_shapes.json', `duplicate shape regionId: ${regionId}`);
    }
    shapeRegionIds.add(regionId);
  }

  for (const region of regions) {
    if (!shapeRegionIds.has(region.id)) {
      throw new StrategyDatasetLoadError('map_region_shapes.json', `missing shape for region id: ${region.id}`);
    }
  }
}

function regionToNationAggregationInput(region: RegionViewModel): NationAggregationInput {
  return {
    owner: region.owner,
    foodOutput: region.definition.foodOutput,
    taxOutput: region.definition.taxOutput,
    contribution: region.contribution
  };
}

function aggregateNationMetric(regions: NationAggregationInput[], selectBaseValue: (region: NationAggregationInput) => number): number {
  const total = regions
    .filter((region) => region.owner === 'player')
    .reduce((sum, region) => {
      const baseValue = sanitizeNonNegativeFinite(selectBaseValue(region));
      const contribution = Math.min(100, sanitizeNonNegativeFinite(region.contribution));
      return sum + (baseValue * contribution) / 100;
    }, 0);

  return Math.max(0, Math.round(total));
}

function sanitizeNonNegativeFinite(value: number): number {
  if (!Number.isFinite(value)) return 0;
  return Math.max(0, value);
}

async function loadJson<T>(fileName: string): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${dataRoot}/${fileName}`);
  } catch (error) {
    throw new StrategyDatasetLoadError(fileName, 'network request failed', error);
  }

  if (!response.ok) {
    throw new StrategyDatasetLoadError(fileName, `HTTP ${response.status}`);
  }

  try {
    return await response.json() as T;
  } catch (error) {
    throw new StrategyDatasetLoadError(fileName, 'invalid JSON response', error);
  }
}

function describeDatasetFile(fileName: string): string {
  const category = fileName.includes('/audio/') || fileName.startsWith('../audio/')
    ? 'audio file'
    : 'data file';
  return `${category} ${fileName}`;
}

function isJsonObject(value: unknown): value is { items?: unknown } {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function resolveSpecialization(region: RegionDefinition, history?: HistoricalLayerDefinition): string {
  const tags = [
    region.terrain,
    ...(region.legitimacyMemory ?? []),
    ...(history?.geographyTags ?? []),
    ...(history?.customTags ?? []),
    ...(history?.strategicResources ?? []),
    ...(history?.weaponTraditions ?? [])
  ].join(' ');

  if (matches(tags, ['horse', 'frontier', 'cavalry', 'border', 'corridor'])) return '边防军镇';
  if (matches(tags, ['capital', 'ritual', 'mandate', 'qin_han', 'imperial'])) return '法统都畿';
  if (matches(tags, ['grain', 'river_irrigation', 'plain', 'basin'])) return '粮税核心';
  if (matches(tags, ['market', 'river_delta', 'harbor', 'trade'])) return '商贸水网';
  if (matches(tags, ['academy', 'culture', 'bureaucracy'])) return '文化官僚';
  return region.manpower > region.taxOutput ? '兵源腹地' : '均衡治理';
}

function focusFromSpecialization(specialization: string): GovernanceFocusId {
  if (specialization.includes('粮')) return 'grain';
  if (specialization.includes('商')) return 'tax';
  if (specialization.includes('军') || specialization.includes('兵源')) return 'military';
  if (specialization.includes('边防')) return 'frontier';
  if (specialization.includes('法统') || specialization.includes('文化')) return 'legitimacy';
  return 'relief';
}

function laborFromGovernanceFocus(focus: GovernanceFocusId): GovernanceLaborId {
  if (focus === 'grain') return 'grain';
  if (focus === 'tax') return 'tax';
  if (focus === 'military' || focus === 'frontier') return 'military';
  if (focus === 'legitimacy' || focus === 'relief') return 'stability';
  return 'balanced';
}

function chooseBuilding(buildings: BuildingDefinition[], specialization: string, terrain: string): BuildingDefinition | undefined {
  const category =
    specialization.includes('粮') ? 'agriculture' :
    specialization.includes('军') || specialization.includes('边防') ? 'military' :
    specialization.includes('商') || terrain.includes('river_delta') ? 'economy' :
    specialization.includes('文化') || specialization.includes('法统') ? 'culture' :
    'defense';

  return buildings.find((building) => building.category === category) ?? buildings[0];
}

function choosePolicy(
  policies: PolicyDefinition[],
  region: RegionDefinition,
  history: HistoricalLayerDefinition | undefined,
  owner: RegionViewModel['owner']
): PolicyDefinition | undefined {
  if (owner !== 'player') {
    return policies.find((policy) => policy.id === 'local_compromise') ?? policies[0];
  }

  if (region.rebellionRisk >= 18) {
    return policies.find((policy) => policy.id === 'relief_grain') ?? policies[0];
  }

  const tags = [...(history?.customTags ?? []), ...(region.legitimacyMemory ?? [])].join(' ');
  if (matches(tags, ['law', 'centralization', 'qin'])) {
    return policies.find((policy) => policy.id === 'standardization') ?? policies[0];
  }

  if (region.manpower > 55) {
    return policies.find((policy) => policy.id === 'conscription') ?? policies[0];
  }

  return policies.find((policy) => policy.id === 'land_survey') ?? policies[0];
}

function buildRouteForecast(army: ArmyViewModel, regionById: Map<string, RegionViewModel>): RouteForecast {
  const from = regionById.get(army.fromRegionId);
  const target = regionById.get(army.targetRegionId);
  if (!from || !target) {
    throw new Error('Route regions are missing.');
  }

  const distance = Math.hypot(from.shape.center.x - target.shape.center.x, from.shape.center.y - target.shape.center.y);
  const supplyCost = Math.round(16 + distance * 3 + Math.max(0, target.risk - 15) * 0.4);
  const contactChance = clamp(44 + target.risk * 0.6 + (target.owner === 'rival' ? 16 : 0), 0, 96);
  const occupationCost = Math.round(42 + target.definition.localPower * 0.8 + target.definition.population / 50000);
  const interceptionRisk = clamp(18 + (target.history?.weaponTraditions?.length ?? 0) * 7 + (target.definition.terrain.includes('mountain') ? 16 : 0), 0, 94);

  return {
    army,
    from,
    target,
    supplyCost,
    turns: Math.max(1, Math.round(distance / 2.2)),
    contactChance,
    occupationCost,
    interceptionRisk,
    summary: `从${from.definition.name}出军至${target.definition.name}，粮草消耗${supplyCost}，接敌${contactChance}%`
  };
}

function formatGeneralName(generals: GeneralDefinition[], generalId: string, fallback: string): string {
  const general = generals.find((candidate) => candidate.id === generalId);
  return general ? `${general.title}${general.name}` : fallback;
}

function matches(value: string, needles: string[]): boolean {
  const normalized = value.toLowerCase();
  return needles.some((needle) => normalized.includes(needle.toLowerCase()));
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

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}
