import { PCFShadowMap, MOUSE, SRGBColorSpace } from 'three/src/constants.js';
import { PerspectiveCamera } from 'three/src/cameras/PerspectiveCamera.js';
import { BufferGeometry } from 'three/src/core/BufferGeometry.js';
import { Raycaster } from 'three/src/core/Raycaster.js';
import { Timer } from 'three/src/core/Timer.js';
import { Shape } from 'three/src/extras/core/Shape.js';
import { CatmullRomCurve3 } from 'three/src/extras/curves/CatmullRomCurve3.js';
import { BoxGeometry } from 'three/src/geometries/BoxGeometry.js';
import { ConeGeometry } from 'three/src/geometries/ConeGeometry.js';
import { CylinderGeometry } from 'three/src/geometries/CylinderGeometry.js';
import { ExtrudeGeometry } from 'three/src/geometries/ExtrudeGeometry.js';
import { PlaneGeometry } from 'three/src/geometries/PlaneGeometry.js';
import { SphereGeometry } from 'three/src/geometries/SphereGeometry.js';
import { TorusGeometry } from 'three/src/geometries/TorusGeometry.js';
import { TubeGeometry } from 'three/src/geometries/TubeGeometry.js';
import { TextureLoader } from 'three/src/loaders/TextureLoader.js';
import { DirectionalLight } from 'three/src/lights/DirectionalLight.js';
import { HemisphereLight } from 'three/src/lights/HemisphereLight.js';
import { Color } from 'three/src/math/Color.js';
import { Vector2 } from 'three/src/math/Vector2.js';
import { Vector3 } from 'three/src/math/Vector3.js';
import { LineBasicMaterial } from 'three/src/materials/LineBasicMaterial.js';
import { MeshBasicMaterial } from 'three/src/materials/MeshBasicMaterial.js';
import { MeshStandardMaterial } from 'three/src/materials/MeshStandardMaterial.js';
import { Group } from 'three/src/objects/Group.js';
import { Line } from 'three/src/objects/Line.js';
import { Mesh } from 'three/src/objects/Mesh.js';
import { WebGLRenderer } from 'three/src/renderers/WebGLRenderer.js';
import { Fog } from 'three/src/scenes/Fog.js';
import { Scene } from 'three/src/scenes/Scene.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import type { StrategyDataset } from './data';
import type { ArmyViewModel, GameMode, LogisticsMapObject, RegionViewModel, RouteForecast } from './types';

type EnemyInterdictionStage = 'planning' | 'moving' | 'striking' | 'resolved';

export interface StrategySceneEvents {
  onSelectRegion: (region: RegionViewModel) => void;
  onHoverRegion: (region: RegionViewModel | null) => void;
  onRouteEdited?: (region: RegionViewModel, army: ArmyViewModel) => void;
  onSelectLogisticsObject?: (objectId: string) => void;
  onSelectEnemyInterdiction?: (orderId: string) => void;
}

export interface LabelAnchor {
  id: string;
  kind: 'region' | 'army' | 'building' | 'landform';
  text: string;
  priority: number;
  position: Vector3;
}

interface EnemyInterdictionTarget {
  id: string;
  armyId: string;
  regionId: string;
  stage: EnemyInterdictionStage;
  risk: number;
  remainingTurns: number;
  lastCountermeasure?: string;
}

export class StrategyScene {
  private readonly renderer: WebGLRenderer;
  private readonly scene = new Scene();
  private readonly camera: PerspectiveCamera;
  private readonly controls: OrbitControls;
  private readonly raycaster = new Raycaster();
  private readonly pointer = new Vector2();
  private readonly regionMeshes = new Map<string, Mesh>();
  private readonly armyMarkers = new Map<string, Group>();
  private readonly terrainFeatureGroup = new Group();
  private readonly buildingFeatureGroup = new Group();
  private readonly occupationBadgeGroup = new Group();
  private readonly enemyThreatGroup = new Group();
  private readonly friendlyCountermeasureGroup = new Group();
  private readonly logisticsObjectGroup = new Group();
  private readonly routeGroup = new Group();
  private readonly hoverRing = new Mesh();
  private readonly selectedRing = new Mesh();
  private routeRaidSegmentCount = 0;
  private routeConvoyMarkerCount = 0;
  private routeDragHandleCount = 0;
  private occupationBadgeCount = 0;
  private enemyThreatMarkerCount = 0;
  private enemyThreatMovingCount = 0;
  private enemyThreatDampenedCount = 0;
  private friendlyCountermeasureMarkerCount = 0;
  private friendlyCountermeasureMovingCount = 0;
  private logisticsMapObjectCount = 0;
  private selectedLogisticsObjectId = '';
  private selectedEnemyInterdictionId = '';
  private hoveredRegion: RegionViewModel | null = null;
  private selectedRegion: RegionViewModel;
  private activeArmy: ArmyViewModel;
  private draggingRouteNode: 'target' | 'waypoint' | null = null;
  private hoveredDragHandle: 'target' | 'waypoint' | null = null;
  private suppressNextClick = false;
  private mode: GameMode = 'governance';
  private animationFrame = 0;
  private readonly timer = new Timer();

  constructor(
    private readonly canvas: HTMLCanvasElement,
    private readonly dataset: StrategyDataset,
    private readonly events: StrategySceneEvents
  ) {
    this.selectedRegion = dataset.regions[0];
    this.activeArmy = dataset.route.army;
    this.renderer = new WebGLRenderer({ canvas, antialias: true, alpha: false });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.shadowMap.enabled = true;
    this.renderer.shadowMap.type = PCFShadowMap;
    this.renderer.outputColorSpace = SRGBColorSpace;
    this.timer.connect(document);

    this.camera = new PerspectiveCamera(38, 1, 0.1, 200);
    this.camera.position.set(-3.5, 19, 16);
    this.camera.lookAt(-3.8, 0, -0.2);

    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    this.controls.enableRotate = false;
    this.controls.enableDamping = true;
    this.controls.dampingFactor = 0.1;
    this.controls.screenSpacePanning = true;
    this.controls.minDistance = 7;
    this.controls.maxDistance = 28;
    this.controls.mouseButtons = {
      LEFT: MOUSE.PAN,
      MIDDLE: MOUSE.DOLLY,
      RIGHT: MOUSE.PAN
    };
    this.controls.target.set(-3.8, 0, -0.2);

    this.scene.background = new Color(0x10171a);
    this.scene.fog = new Fog(0x10171a, 24, 56);
    this.terrainFeatureGroup.name = 'TerrainFeatureLayer';
    this.buildingFeatureGroup.name = 'BuildingMarkerLayer';
    this.occupationBadgeGroup.name = 'OccupationStageBadgeLayer';
    this.enemyThreatGroup.name = 'EnemyInterdictionThreatLayer';
    this.friendlyCountermeasureGroup.name = 'FriendlyCountermeasureLayer';
    this.logisticsObjectGroup.name = 'LogisticsMapObjectLayer';
    this.scene.add(this.terrainFeatureGroup, this.buildingFeatureGroup, this.occupationBadgeGroup, this.enemyThreatGroup, this.friendlyCountermeasureGroup, this.logisticsObjectGroup);
    this.scene.add(this.routeGroup);
    this.buildLights();
    this.buildMapTexture();
    this.buildRegions();
    this.buildTerrainFeatures();
    this.buildBuildingMarkers();
    this.buildArmyMarkers(dataset.armies);
    this.buildRoute(this.createRouteForecast(this.activeArmy));
    this.buildSelectionRings();
    this.setMode('governance');
    this.selectRegion(this.selectedRegion.definition.id);

    canvas.addEventListener('pointermove', this.handlePointerMove);
    canvas.addEventListener('pointerdown', this.handlePointerDown);
    canvas.addEventListener('pointerup', this.handlePointerUp);
    canvas.addEventListener('pointercancel', this.handlePointerUp);
    canvas.addEventListener('click', this.handleClick);
    window.addEventListener('resize', this.resize);
    this.resize();
  }

  dispose(): void {
    cancelAnimationFrame(this.animationFrame);
    this.canvas.removeEventListener('pointermove', this.handlePointerMove);
    this.canvas.removeEventListener('pointerdown', this.handlePointerDown);
    this.canvas.removeEventListener('pointerup', this.handlePointerUp);
    this.canvas.removeEventListener('pointercancel', this.handlePointerUp);
    this.canvas.removeEventListener('click', this.handleClick);
    window.removeEventListener('resize', this.resize);
    this.timer.disconnect();
    this.timer.dispose();
    this.renderer.dispose();
  }

  start(): void {
    const animate = (timestamp?: number) => {
      this.animationFrame = requestAnimationFrame(animate);
      this.timer.update(timestamp);
      this.controls.update();
      this.animateWarPressure();
      this.animateDragHandles();
      this.renderer.render(this.scene, this.camera);
    };
    animate();
  }

  setMode(mode: GameMode): void {
    this.mode = mode;
    for (const region of this.dataset.regions) {
      const mesh = this.regionMeshes.get(region.definition.id);
      const material = mesh?.material as MeshStandardMaterial | undefined;
      if (!material) continue;
      material.color.setHex(this.resolveRegionColor(region));
      material.emissive.setHex(region.definition.id === this.selectedRegion.definition.id ? 0x2c2410 : 0x000000);
      material.opacity = mode === 'war' && region.owner === 'frontier' ? 0.58 : 0.78;
    }

    this.terrainFeatureGroup.visible = true;
    this.buildingFeatureGroup.traverse((object) => {
      const mesh = object as Mesh;
      const material = mesh.material as MeshStandardMaterial | undefined;
      if (material?.opacity !== undefined) {
        material.opacity = mode === 'war' ? 0.46 : 0.96;
      }
    });

    for (const marker of this.armyMarkers.values()) {
      marker.visible = mode === 'war';
    }
    this.routeGroup.visible = mode === 'war';
    this.syncOccupationBadges();
    this.occupationBadgeGroup.visible = true;
    this.enemyThreatGroup.visible = mode === 'war' && this.enemyThreatMarkerCount > 0;
    this.friendlyCountermeasureGroup.visible = mode === 'war' && this.friendlyCountermeasureMarkerCount > 0;
    this.logisticsObjectGroup.visible = mode === 'war' && this.logisticsMapObjectCount > 0;
  }

  selectRegion(regionId: string): RegionViewModel | null {
    const region = this.dataset.regionById.get(regionId);
    if (!region) return null;
    this.selectedRegion = region;
    this.setMode(this.mode);
    this.placeRing(this.selectedRing, region, 0.08);
    this.events.onSelectRegion(region);
    return region;
  }

  getSelectedRegion(): RegionViewModel {
    return this.selectedRegion;
  }

  getMode(): GameMode {
    return this.mode;
  }

  setActiveArmy(armyId: string): ArmyViewModel | null {
    const army = this.dataset.armies.find((candidate) => candidate.id === armyId);
    if (!army) return null;
    this.activeArmy = army;
    this.syncArmyMarkers();
    this.refreshArmyMarkers();
    this.rebuildRoute();
    return army;
  }

  syncArmyMarkers(): void {
    const existingIds = new Set(this.dataset.armies.map((army) => army.id));
    for (const [armyId, marker] of this.armyMarkers) {
      if (existingIds.has(armyId)) continue;
      this.scene.remove(marker);
      this.armyMarkers.delete(armyId);
    }

    for (const army of this.dataset.armies) {
      if (!this.armyMarkers.has(army.id)) {
        this.buildArmyMarker(army);
      }
    }
    if (!existingIds.has(this.activeArmy.id)) {
      this.activeArmy = this.dataset.armies.find((army) => army.faction === 'player') ?? this.dataset.armies[0];
    }
    this.refreshArmyMarkers();
  }

  retargetActiveArmy(regionId: string): RouteForecast | null {
    const target = this.dataset.regionById.get(regionId);
    if (!target) return null;
    this.activeArmy.targetRegionId = regionId;
    this.rebuildRoute();
    return this.createRouteForecast(this.activeArmy);
  }

  setActiveArmyWaypoint(regionId: string | null): RouteForecast {
    if (regionId && this.dataset.regionById.has(regionId) && regionId !== this.activeArmy.fromRegionId && regionId !== this.activeArmy.targetRegionId) {
      this.activeArmy.waypointRegionId = regionId;
    } else {
      this.activeArmy.waypointRegionId = undefined;
    }
    this.rebuildRoute();
    return this.createRouteForecast(this.activeArmy);
  }

  refreshActiveRoute(): void {
    this.rebuildRoute();
  }

  setEnemyInterdictionTargets(targets: EnemyInterdictionTarget[]): void {
    this.enemyThreatGroup.clear();
    this.friendlyCountermeasureGroup.clear();
    this.enemyThreatMarkerCount = 0;
    this.enemyThreatMovingCount = 0;
    this.enemyThreatDampenedCount = 0;
    this.friendlyCountermeasureMarkerCount = 0;
    this.friendlyCountermeasureMovingCount = 0;
    const validIds = new Set(targets.map((target) => target.id));
    if (this.selectedEnemyInterdictionId && !validIds.has(this.selectedEnemyInterdictionId)) {
      this.selectedEnemyInterdictionId = '';
    }
    for (const target of targets) {
      const region = this.dataset.regionById.get(target.regionId);
      if (!region) continue;
      const marker = this.createEnemyThreatMarker(region, target);
      this.enemyThreatGroup.add(marker);
      this.enemyThreatMarkerCount += 1;
      if (marker.userData.routeCurve) this.enemyThreatMovingCount += 1;
      if (target.lastCountermeasure) this.enemyThreatDampenedCount += 1;
      if (target.lastCountermeasure) {
        const friendlyMarker = this.createFriendlyCountermeasureMarker(region, target);
        this.friendlyCountermeasureGroup.add(friendlyMarker);
        this.friendlyCountermeasureMarkerCount += 1;
        if (friendlyMarker.userData.routeCurve) this.friendlyCountermeasureMovingCount += 1;
      }
    }
    this.enemyThreatGroup.visible = this.mode === 'war' && this.enemyThreatMarkerCount > 0;
    this.friendlyCountermeasureGroup.visible = this.mode === 'war' && this.friendlyCountermeasureMarkerCount > 0;
  }

  selectEnemyInterdictionTarget(orderId: string): void {
    this.selectedEnemyInterdictionId = orderId;
    this.refreshEnemyThreatSelection();
  }

  setLogisticsMapObjects(objects: LogisticsMapObject[]): void {
    this.logisticsObjectGroup.clear();
    this.logisticsMapObjectCount = 0;
    const validIds = new Set(objects.map((object) => object.id));
    if (this.selectedLogisticsObjectId && !validIds.has(this.selectedLogisticsObjectId)) {
      this.selectedLogisticsObjectId = '';
    }

    for (const object of objects) {
      const marker = this.createLogisticsMapObjectMarker(object);
      if (!marker) continue;
      this.logisticsObjectGroup.add(marker);
      this.logisticsMapObjectCount += 1;
    }
    this.logisticsObjectGroup.visible = this.mode === 'war' && this.logisticsMapObjectCount > 0;
  }

  selectLogisticsMapObject(objectId: string): void {
    this.selectedLogisticsObjectId = objectId;
    this.refreshLogisticsObjectSelection();
  }

  getActiveArmy(): ArmyViewModel {
    return this.activeArmy;
  }

  getCameraDistance(): number {
    return this.camera.position.distanceTo(this.controls.target);
  }

  getDebugState(): {
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
  } {
    return {
      mode: this.mode,
      selectedRegionId: this.selectedRegion.definition.id,
      regionMeshCount: this.regionMeshes.size,
      armyMarkerCount: this.armyMarkers.size,
      terrainFeatureCount: this.terrainFeatureGroup.children.length,
      buildingMarkerCount: this.buildingFeatureGroup.children.length,
      routeVisible: this.routeGroup.visible,
      cameraDistance: Number(this.getCameraDistance().toFixed(3)),
      cameraX: Number(this.camera.position.x.toFixed(3)),
      cameraZ: Number(this.camera.position.z.toFixed(3)),
      targetX: Number(this.controls.target.x.toFixed(3)),
      targetZ: Number(this.controls.target.z.toFixed(3)),
      drawCalls: this.renderer.info.render.calls,
      routeRaidSegmentCount: this.routeRaidSegmentCount,
      routeConvoyMarkerCount: this.routeConvoyMarkerCount,
      routeDragHandleCount: this.routeDragHandleCount,
      occupationBadgeCount: this.occupationBadgeCount,
      enemyThreatMarkerCount: this.enemyThreatMarkerCount,
      enemyThreatMovingCount: this.enemyThreatMovingCount,
      enemyThreatDampenedCount: this.enemyThreatDampenedCount,
      friendlyCountermeasureMarkerCount: this.friendlyCountermeasureMarkerCount,
      friendlyCountermeasureMovingCount: this.friendlyCountermeasureMovingCount,
      logisticsMapObjectCount: this.logisticsMapObjectCount,
      selectedLogisticsObjectId: this.selectedLogisticsObjectId,
      selectedEnemyInterdictionId: this.selectedEnemyInterdictionId,
      activeArmyId: this.activeArmy.id,
      activeArmyTargetId: this.activeArmy.targetRegionId,
      activeArmyWaypointId: this.activeArmy.waypointRegionId ?? ''
    };
  }

  getLabelAnchors(): LabelAnchor[] {
    const anchors: LabelAnchor[] = [];
    for (const region of this.dataset.regions) {
      const offset = region.shape.labelOffset ?? { x: 0, y: 0 };
      anchors.push({
        id: region.definition.id,
        kind: 'region',
        text: region.definition.name,
        priority: this.regionLabelPriority(region),
        position: new Vector3(region.shape.center.x + offset.x, this.regionHeight(region) + 0.24, -region.shape.center.y - offset.y)
      });
    }

    anchors.push({
      id: `landform:${this.selectedRegion.definition.id}`,
      kind: 'landform',
      text: this.selectedRegion.geography.label,
      priority: 320,
      position: this.featureBasePosition(this.selectedRegion, -2.0, 0.44).add(new Vector3(0, 1.02, 0))
    });

    if (this.mode === 'war') {
      for (const army of this.dataset.armies) {
        const region = this.dataset.regionById.get(army.fromRegionId);
        if (!region) continue;
        anchors.push({
          id: army.id,
          kind: 'army',
          text: `${army.name} ${Math.round(army.supply)}%`,
          priority: 200,
          position: new Vector3(region.shape.center.x, 1.3, -region.shape.center.y)
        });
      }
    } else {
      for (const region of this.dataset.regions) {
        if (region.owner !== 'player' && region.definition.id !== this.selectedRegion.definition.id) continue;
        anchors.push({
          id: `building:${region.definition.id}`,
          kind: 'building',
          text: region.recommendedBuilding?.name ?? '建设',
          priority: region.definition.id === this.selectedRegion.definition.id ? 260 : 112,
          position: this.featureBasePosition(region, 0.54, -0.4).add(new Vector3(0, 0.72, 0))
        });
      }
    }

    return anchors;
  }

  projectToScreen(position: Vector3): { x: number; y: number; visible: boolean } {
    const projected = position.clone().project(this.camera);
    const rect = this.renderer.domElement.getBoundingClientRect();
    return {
      x: (projected.x * 0.5 + 0.5) * rect.width + rect.left,
      y: (-projected.y * 0.5 + 0.5) * rect.height + rect.top,
      visible: projected.z > -1 && projected.z < 1
    };
  }

  getRegionScreenPoint(regionId: string): { x: number; y: number; visible: boolean } | null {
    const region = this.dataset.regionById.get(regionId);
    if (!region) return null;
    return this.projectToScreen(
      new Vector3(region.shape.center.x, this.regionHeight(region) + 0.1, -region.shape.center.y)
    );
  }

  getLogisticsObjectScreenPoint(objectId: string): { x: number; y: number; visible: boolean } | null {
    const marker = this.logisticsObjectGroup.children.find((child) => child.userData.logisticsObjectId === objectId);
    if (!marker) return null;
    return this.projectToScreen(marker.getWorldPosition(new Vector3()));
  }

  getEnemyInterdictionScreenPoint(orderId: string): { x: number; y: number; visible: boolean } | null {
    const marker = this.enemyThreatGroup.children.find((child) => child.userData.enemyInterdictionId === orderId);
    if (!marker) return null;
    return this.projectToScreen(marker.getWorldPosition(new Vector3()));
  }

  private buildLights(): void {
    const hemisphere = new HemisphereLight(0xd8f3ff, 0x1b2017, 1.8);
    this.scene.add(hemisphere);

    const sun = new DirectionalLight(0xffe0a1, 3.1);
    sun.position.set(-8, 18, 12);
    sun.castShadow = true;
    sun.shadow.mapSize.set(2048, 2048);
    this.scene.add(sun);
  }

  private buildMapTexture(): void {
    const { imageSize, pixelsPerShapeUnit, shapeCenter } = this.dataset.metadata;
    const width = imageSize.width / pixelsPerShapeUnit;
    const height = imageSize.height / pixelsPerShapeUnit;
    const texture = new TextureLoader().load('/game-data/map/jiuzhou_generated_map.png');
    texture.colorSpace = SRGBColorSpace;
    texture.anisotropy = 8;

    const geometry = new PlaneGeometry(width, height);
    geometry.rotateX(-Math.PI / 2);
    const material = new MeshStandardMaterial({
      map: texture,
      color: 0xffffff,
      roughness: 0.88,
      metalness: 0,
      transparent: true,
      opacity: 0.88
    });
    const plane = new Mesh(geometry, material);
    plane.name = 'Generated_Jiuzhou_Map_Texture';
    plane.position.set(shapeCenter.x, -0.08, -shapeCenter.y);
    plane.receiveShadow = true;
    this.scene.add(plane);
  }

  private buildRegions(): void {
    for (const region of this.dataset.regions) {
      const shape = new Shape(region.shape.boundary.map((point) => new Vector2(point.x, point.y)));
      const geometry = new ExtrudeGeometry(shape, {
        depth: this.regionHeight(region),
        bevelEnabled: false
      });
      geometry.rotateX(-Math.PI / 2);
      geometry.computeVertexNormals();

      const material = new MeshStandardMaterial({
        color: this.resolveRegionColor(region),
        roughness: 0.74,
        metalness: 0.03,
        transparent: true,
        opacity: 0.78
      });

      const mesh = new Mesh(geometry, material);
      mesh.name = `RegionMesh_${region.definition.id}`;
      mesh.userData.regionId = region.definition.id;
      mesh.castShadow = true;
      mesh.receiveShadow = true;
      this.regionMeshes.set(region.definition.id, mesh);
      this.scene.add(mesh);

      const lineGeometry = new BufferGeometry().setFromPoints(
        [...region.shape.boundary, region.shape.boundary[0]].map((point) => new Vector3(point.x, this.regionHeight(region) + 0.018, -point.y))
      );
      const line = new Line(
        lineGeometry,
        new LineBasicMaterial({ color: region.owner === 'player' ? 0xe5bd63 : 0x5e6f74, transparent: true, opacity: 0.68 })
      );
      line.name = `RegionBorder_${region.definition.id}`;
      this.scene.add(line);
    }
  }

  private buildArmyMarkers(armies: ArmyViewModel[]): void {
    for (const army of armies) {
      this.buildArmyMarker(army);
    }
    this.refreshArmyMarkers();
  }

  private buildArmyMarker(army: ArmyViewModel): void {
    const region = this.dataset.regionById.get(army.fromRegionId);
    if (!region) return;

    const group = new Group();
    group.name = `ArmyMarker_${army.id}`;
    group.userData.armyId = army.id;
    group.position.set(region.shape.center.x, 1.05, -region.shape.center.y);

    const color = army.faction === 'player' ? 0xe7c25c : 0xd55243;
    const base = new Mesh(
      new CylinderGeometry(0.24, 0.34, 0.16, 6),
      new MeshStandardMaterial({ color, emissive: color, emissiveIntensity: 0.18, roughness: 0.55 })
    );
    const banner = new Mesh(
      new BoxGeometry(0.12, 0.72, 0.04),
      new MeshStandardMaterial({ color: 0x1b2224, roughness: 0.65 })
    );
    banner.position.set(0, 0.42, 0);
    const flag = new Mesh(
      new BoxGeometry(0.44, 0.2, 0.035),
      new MeshStandardMaterial({ color, roughness: 0.5 })
    );
    flag.position.set(0.22, 0.74, 0);
    group.add(base, banner, flag);
    group.visible = this.mode === 'war';
    this.armyMarkers.set(army.id, group);
    this.scene.add(group);
  }

  private refreshArmyMarkers(): void {
    for (const [armyId, marker] of this.armyMarkers) {
      const active = armyId === this.activeArmy.id;
      marker.scale.setScalar(active ? 1.26 : 1);
      marker.traverse((object) => {
        const mesh = object as Mesh;
        const material = mesh.material as MeshStandardMaterial | undefined;
        if (!material || material.emissiveIntensity === undefined) return;
        material.emissiveIntensity = active ? 0.38 : 0.16;
      });
    }
  }

  private buildTerrainFeatures(): void {
    for (const region of this.dataset.regions) {
      const group = new Group();
      group.name = `TerrainFeature_${region.definition.id}`;
      const base = this.featureBasePosition(region, -0.28, 0.14);
      const height = this.regionHeight(region) + 0.08;
      const kind = region.geography.kind;
      group.userData.geographyKind = kind;
      group.userData.geographyLabel = region.geography.label;

      if (kind === 'arid_corridor') {
        this.addCorridorFeature(group, base, height, 0xb9934d);
        this.addResourcePip(group, base.clone().add(new Vector3(0.2, 0.02, -0.12)), 0x8fb86b);
      } else if (kind === 'water_network') {
        this.addDeltaFeature(group, base, height);
      } else if (kind === 'mountain_pass') {
        this.addMountainCluster(group, region, base, height);
        this.addPassGate(group, base.clone().add(new Vector3(0.28, 0.02, -0.14)), height);
      } else if (kind === 'basin_granary') {
        this.addMountainCluster(group, region, base.clone().add(new Vector3(-0.14, 0, 0.08)), height);
        this.addFieldStrips(group, region, base.clone().add(new Vector3(0.28, 0, -0.2)), height);
      } else if (kind === 'loess_irrigation') {
        this.addFieldStrips(group, region, base, height);
        this.addWaterFeature(group, region, base.clone().add(new Vector3(0.16, 0, -0.2)), height);
      } else if (kind === 'frontier_horse_route') {
        this.addCorridorFeature(group, base, height, 0x9f8052);
        this.addResourcePip(group, base.clone().add(new Vector3(0.28, 0.02, -0.16)), 0x9f8052);
      } else if (kind === 'mountain_coast_harbor') {
        this.addWaterFeature(group, region, base, height);
        this.addHarborPier(group, base.clone().add(new Vector3(0.24, 0.02, -0.16)), height);
      } else if (kind === 'plateau_frontier' || kind === 'mineral_mountain') {
        this.addMountainCluster(group, region, base, height);
        this.addResourcePip(group, base.clone().add(new Vector3(-0.22, 0.02, 0.18)), 0xb5b9a4);
      } else if (kind === 'river_grain_corridor') {
        this.addWaterFeature(group, region, base, height);
        this.addFieldStrips(group, region, base.clone().add(new Vector3(0.24, 0, -0.18)), height);
      } else if (kind === 'forest_frontier') {
        this.addForestFeature(group, base, height);
      } else if (kind === 'central_plain') {
        this.addFieldStrips(group, region, base, height);
      } else {
        this.addLowlandMarker(group, base, height);
      }

      if (region.geography.resources.some((resource) => matches(resource, ['horse', 'pasture']))) {
        this.addResourcePip(group, base.clone().add(new Vector3(0.26, 0.02, -0.16)), 0x9f8052);
      }
      if (region.geography.resources.some((resource) => matches(resource, ['iron', 'bronze', 'salt', 'minerals']))) {
        this.addResourcePip(group, base.clone().add(new Vector3(-0.22, 0.02, 0.18)), 0xb5b9a4);
      }

      this.terrainFeatureGroup.add(group);
    }
  }

  private buildBuildingMarkers(): void {
    for (const region of this.dataset.regions) {
      const building = region.recommendedBuilding;
      const group = new Group();
      group.name = `BuildingMarker_${region.definition.id}_${building?.id ?? 'planned'}`;
      group.position.copy(this.featureBasePosition(region, 0.28, -0.22));
      group.userData.regionId = region.definition.id;
      group.userData.buildingName = building?.name ?? '建设预案';

      const category = building?.category ?? 'infrastructure';
      if (category === 'agriculture') {
        this.addGranary(group);
      } else if (category === 'defense') {
        this.addFortification(group);
      } else if (category === 'military') {
        this.addMilitaryYard(group);
      } else if (category === 'economy') {
        this.addMarket(group);
      } else if (category === 'culture') {
        this.addCivicHall(group);
      } else {
        this.addCourierPost(group);
      }

      group.scale.setScalar(region.owner === 'player' ? 1.22 : 0.92);
      this.buildingFeatureGroup.add(group);
    }
  }

  private addMountainCluster(group: Group, region: RegionViewModel, base: Vector3, height: number): void {
    const peakMaterial = new MeshStandardMaterial({ color: 0x8d8a75, roughness: 0.86, metalness: 0.02 });
    const capMaterial = new MeshStandardMaterial({ color: 0xd9d2b4, roughness: 0.82, metalness: 0.02 });
    const radius = region.definition.terrain.includes('mountain') ? 0.18 : 0.13;
    const offsets = [
      new Vector3(0, 0, 0),
      new Vector3(0.18, -0.01, 0.1),
      new Vector3(-0.17, -0.01, 0.08)
    ];
    for (const [index, offset] of offsets.entries()) {
      const peak = new Mesh(
        new ConeGeometry(radius * (index === 0 ? 1.25 : 0.9), 0.58 - index * 0.08, 5),
        peakMaterial
      );
      peak.position.copy(base).add(offset).setY(height + 0.22 - index * 0.02);
      peak.castShadow = true;
      const cap = new Mesh(new ConeGeometry(radius * 0.42, 0.14, 5), capMaterial);
      cap.position.copy(peak.position).add(new Vector3(0, 0.3 - index * 0.04, 0));
      cap.castShadow = true;
      group.add(peak, cap);
    }
  }

  private addWaterFeature(group: Group, region: RegionViewModel, base: Vector3, height: number): void {
    const waterMaterial = new MeshStandardMaterial({
      color: region.definition.terrain.includes('river_delta') ? 0x61aeb0 : 0x4c8c9d,
      emissive: 0x12383a,
      emissiveIntensity: 0.18,
      roughness: 0.42,
      metalness: 0.04,
      transparent: true,
      opacity: 0.78
    });
    for (let i = 0; i < 3; i += 1) {
      const strip = new Mesh(new BoxGeometry(0.46, 0.026, 0.055), waterMaterial);
      strip.position.copy(base).add(new Vector3((i - 1) * 0.16, height + 0.02, (i - 1) * 0.11));
      strip.rotation.y = -0.62 + i * 0.08;
      group.add(strip);
    }
  }

  private addDeltaFeature(group: Group, base: Vector3, height: number): void {
    const waterMaterial = new MeshStandardMaterial({
      color: 0x62b6b4,
      emissive: 0x12383a,
      emissiveIntensity: 0.18,
      roughness: 0.38,
      transparent: true,
      opacity: 0.82
    });
    for (let i = 0; i < 5; i += 1) {
      const strip = new Mesh(new BoxGeometry(0.42 - i * 0.025, 0.025, 0.045), waterMaterial);
      strip.position.copy(base).add(new Vector3((i - 2) * 0.12, height + 0.02, Math.abs(i - 2) * 0.07));
      strip.rotation.y = -0.82 + i * 0.38;
      group.add(strip);
    }
  }

  private addCorridorFeature(group: Group, base: Vector3, height: number, color: number): void {
    const roadMaterial = new MeshStandardMaterial({
      color,
      roughness: 0.78,
      metalness: 0,
      transparent: true,
      opacity: 0.86
    });
    for (let i = 0; i < 4; i += 1) {
      const segment = new Mesh(new BoxGeometry(0.22, 0.024, 0.052), roadMaterial);
      segment.position.copy(base).add(new Vector3(-0.32 + i * 0.22, height + 0.025, -0.04 + i * 0.035));
      segment.rotation.y = 0.22;
      group.add(segment);
    }
  }

  private addPassGate(group: Group, base: Vector3, height: number): void {
    const material = markerMaterial(0x8e7a56);
    const gate = new Mesh(new BoxGeometry(0.28, 0.16, 0.09), material);
    gate.position.copy(base).setY(height + 0.11);
    const postA = new Mesh(new BoxGeometry(0.07, 0.24, 0.08), material);
    postA.position.copy(base).add(new Vector3(-0.15, 0, 0)).setY(height + 0.15);
    const postB = postA.clone();
    postB.position.x = base.x + 0.15;
    group.add(gate, postA, postB);
  }

  private addHarborPier(group: Group, base: Vector3, height: number): void {
    const dock = new Mesh(new BoxGeometry(0.32, 0.035, 0.08), markerMaterial(0x9d7548));
    dock.position.copy(base).setY(height + 0.05);
    dock.rotation.y = -0.28;
    const boat = new Mesh(new BoxGeometry(0.18, 0.045, 0.07), markerMaterial(0x5f7f83));
    boat.position.copy(base).add(new Vector3(0.12, 0.03, -0.12)).setY(height + 0.07);
    boat.rotation.y = -0.28;
    group.add(dock, boat);
  }

  private addForestFeature(group: Group, base: Vector3, height: number): void {
    const trunkMaterial = markerMaterial(0x5f4d31);
    const leafMaterial = markerMaterial(0x5f8a55);
    for (let i = 0; i < 3; i += 1) {
      const x = (i - 1) * 0.16;
      const trunk = new Mesh(new CylinderGeometry(0.025, 0.035, 0.14, 6), trunkMaterial);
      trunk.position.copy(base).add(new Vector3(x, 0, i % 2 === 0 ? 0.04 : -0.05)).setY(height + 0.1);
      const crown = new Mesh(new ConeGeometry(0.11, 0.22, 7), leafMaterial);
      crown.position.copy(trunk.position).add(new Vector3(0, 0.17, 0));
      group.add(trunk, crown);
    }
  }

  private addFieldStrips(group: Group, region: RegionViewModel, base: Vector3, height: number): void {
    const fieldMaterial = new MeshStandardMaterial({
      color: region.definition.foodOutput >= 90 ? 0x9eaa56 : 0x7d8e4d,
      roughness: 0.9,
      metalness: 0,
      transparent: true,
      opacity: 0.84
    });
    for (let i = 0; i < 4; i += 1) {
      const strip = new Mesh(new BoxGeometry(0.38, 0.028, 0.052), fieldMaterial);
      strip.position.copy(base).add(new Vector3(-0.21 + i * 0.14, height + 0.015, 0.04 * (i % 2)));
      strip.rotation.y = 0.18;
      group.add(strip);
    }
  }

  private addLowlandMarker(group: Group, base: Vector3, height: number): void {
    const marker = new Mesh(
      new CylinderGeometry(0.14, 0.18, 0.05, 8),
      new MeshStandardMaterial({ color: 0x7b8d69, roughness: 0.82, metalness: 0.02 })
    );
    marker.position.copy(base).setY(height + 0.04);
    group.add(marker);
  }

  private addResourcePip(group: Group, position: Vector3, color: number): void {
    const pip = new Mesh(
      new SphereGeometry(0.065, 8, 6),
      new MeshStandardMaterial({ color, emissive: color, emissiveIntensity: 0.12, roughness: 0.5 })
    );
    pip.position.copy(position);
    group.add(pip);
  }

  private addGranary(group: Group): void {
    const body = new Mesh(
      new CylinderGeometry(0.13, 0.16, 0.26, 8),
      markerMaterial(0xb08b4f)
    );
    body.position.y = 0.17;
    const roof = new Mesh(new ConeGeometry(0.18, 0.16, 8), markerMaterial(0x6c5435));
    roof.position.y = 0.39;
    group.add(body, roof);
  }

  private addFortification(group: Group): void {
    const material = markerMaterial(0x8f8370);
    const wall = new Mesh(new BoxGeometry(0.42, 0.18, 0.13), material);
    wall.position.y = 0.14;
    const towerA = new Mesh(new BoxGeometry(0.12, 0.3, 0.15), material);
    towerA.position.set(-0.18, 0.2, 0);
    const towerB = towerA.clone();
    towerB.position.x = 0.18;
    group.add(wall, towerA, towerB);
  }

  private addMilitaryYard(group: Group): void {
    const base = new Mesh(new CylinderGeometry(0.16, 0.2, 0.08, 6), markerMaterial(0x735b3c));
    base.position.y = 0.08;
    const banner = new Mesh(new BoxGeometry(0.08, 0.42, 0.045), markerMaterial(0x2e3635));
    banner.position.y = 0.31;
    const flag = new Mesh(new BoxGeometry(0.28, 0.11, 0.04), markerMaterial(0xc46c43));
    flag.position.set(0.13, 0.48, 0);
    group.add(base, banner, flag);
  }

  private addMarket(group: Group): void {
    const stall = new Mesh(new BoxGeometry(0.34, 0.14, 0.22), markerMaterial(0x9d6e4c));
    stall.position.y = 0.11;
    const canopy = new Mesh(new ConeGeometry(0.25, 0.14, 4), markerMaterial(0x4f8b8f));
    canopy.position.y = 0.28;
    canopy.rotation.y = Math.PI / 4;
    group.add(stall, canopy);
  }

  private addCivicHall(group: Group): void {
    const plinth = new Mesh(new BoxGeometry(0.38, 0.09, 0.25), markerMaterial(0x7c6fa5));
    plinth.position.y = 0.08;
    const hall = new Mesh(new BoxGeometry(0.28, 0.18, 0.18), markerMaterial(0xb9a76d));
    hall.position.y = 0.2;
    const roof = new Mesh(new ConeGeometry(0.23, 0.11, 4), markerMaterial(0x5b4d72));
    roof.position.y = 0.36;
    roof.rotation.y = Math.PI / 4;
    group.add(plinth, hall, roof);
  }

  private addCourierPost(group: Group): void {
    const post = new Mesh(new BoxGeometry(0.11, 0.36, 0.09), markerMaterial(0xa78554));
    post.position.y = 0.24;
    const cap = new Mesh(new BoxGeometry(0.3, 0.07, 0.16), markerMaterial(0x6b7654));
    cap.position.y = 0.45;
    group.add(post, cap);
  }

  private syncOccupationBadges(): void {
    this.occupationBadgeGroup.clear();
    this.occupationBadgeCount = 0;
    for (const region of this.dataset.regions) {
      if (!shouldShowOccupationBadge(region.controlStage)) continue;
      const badge = this.createOccupationBadge(region);
      this.occupationBadgeGroup.add(badge);
      this.occupationBadgeCount += 1;
    }
  }

  private createOccupationBadge(region: RegionViewModel): Group {
    const group = new Group();
    group.name = `OccupationStageBadge_${region.definition.id}_${region.controlStage}`;
    group.userData.regionId = region.definition.id;
    group.userData.controlStage = region.controlStage;
    group.position.set(region.shape.center.x + 0.48, this.regionHeight(region) + 0.68, -region.shape.center.y + 0.28);

    const config = occupationBadgeConfig(region.controlStage);
    const plateMaterial = new MeshBasicMaterial({ color: config.color, transparent: true, opacity: 0.86 });
    const darkMaterial = new MeshBasicMaterial({ color: 0x161c1d, transparent: true, opacity: 0.92 });

    const stem = new Mesh(new CylinderGeometry(0.025, 0.025, 0.5, 8), plateMaterial);
    stem.position.y = -0.22;
    const plate = new Mesh(new CylinderGeometry(0.23, 0.23, 0.055, 6), plateMaterial);
    plate.rotation.y = Math.PI / 6;
    const center = new Mesh(new CylinderGeometry(0.12, 0.12, 0.065, 6), darkMaterial);
    center.position.y = 0.012;
    center.rotation.y = Math.PI / 6;

    group.add(stem, plate, center);
    for (let index = 0; index < config.pips; index += 1) {
      const pip = new Mesh(new BoxGeometry(0.045, 0.11, 0.045), plateMaterial);
      pip.position.set(-0.07 + index * 0.07, 0.09, 0.16);
      group.add(pip);
    }
    return group;
  }

  private createEnemyThreatMarker(region: RegionViewModel, target: EnemyInterdictionTarget): Group {
    const group = new Group();
    group.name = `EnemyInterdictionThreat_${target.id}_${target.stage}`;
    group.userData.enemyInterdictionId = target.id;
    group.userData.regionId = region.definition.id;
    group.userData.stage = target.stage;
    group.userData.lastCountermeasure = target.lastCountermeasure ?? '';

    const color = enemyThreatColor(target.stage);
    const counterDampen = target.lastCountermeasure ? 0.22 : 0;
    const opacity = clamp(0.66 + target.risk / 220 - counterDampen, 0.48, 0.98);
    const threatMaterial = new MeshBasicMaterial({ color, transparent: true, opacity });
    const darkMaterial = new MeshBasicMaterial({ color: 0x111416, transparent: true, opacity: target.lastCountermeasure ? 0.68 : 0.92 });

    const routeCurve = this.createEnemyThreatRouteCurve(target);
    const baseProgress = enemyThreatRouteProgress(target.stage, target.remainingTurns, Boolean(target.lastCountermeasure));
    if (routeCurve) {
      const routePoint = routeCurve.getPoint(baseProgress);
      group.position.copy(routePoint).add(new Vector3(0, 0.3, 0));
      group.userData.routeCurve = routeCurve;
      group.userData.baseProgress = baseProgress;
      group.userData.speed = target.lastCountermeasure ? 0.018 : 0.036;
      group.userData.sway = target.lastCountermeasure ? 0.012 : 0.026;
    } else {
      group.position.set(region.shape.center.x - 0.52, this.regionHeight(region) + 0.82, -region.shape.center.y - 0.34);
    }

    const halo = new Mesh(new TorusGeometry(0.28, 0.028, 8, 32), threatMaterial);
    halo.rotation.x = -Math.PI / 2;
    const stem = new Mesh(new CylinderGeometry(0.025, 0.025, 0.54, 8), darkMaterial);
    stem.position.y = -0.12;
    const blade = new Mesh(new ConeGeometry(0.2, 0.42, 3), threatMaterial);
    blade.rotation.x = Math.PI / 2;
    blade.rotation.z = target.stage === 'striking' ? Math.PI / 6 : 0;
    blade.position.y = 0.22;
    const pulse = new Mesh(new CylinderGeometry(0.11, 0.11, 0.045, 12), threatMaterial);
    pulse.position.y = 0.02;
    pulse.scale.setScalar((target.stage === 'striking' ? 1.18 : target.stage === 'moving' ? 1.05 : 0.94) * (target.lastCountermeasure ? 0.82 : 1));

    const routeNeedle = new Mesh(
      new BoxGeometry(target.lastCountermeasure ? 0.34 : 0.46, 0.045, 0.075),
      new MeshBasicMaterial({ color, transparent: true, opacity: clamp(opacity + 0.08, 0.5, 0.98) })
    );
    routeNeedle.position.set(0, 0.06, -0.28);

    const selectedRing = new Mesh(
      new TorusGeometry(0.42, 0.034, 8, 36),
      new MeshBasicMaterial({
        color: 0xffd36b,
        transparent: true,
        opacity: target.id === this.selectedEnemyInterdictionId ? 0.92 : 0
      })
    );
    selectedRing.name = 'EnemyThreatSelectionRing';
    selectedRing.rotation.x = -Math.PI / 2;
    selectedRing.position.y = -0.04;

    group.add(halo, stem, blade, pulse, routeNeedle, selectedRing);
    group.traverse((child) => {
      child.userData.enemyInterdictionId = target.id;
    });
    group.scale.setScalar(target.id === this.selectedEnemyInterdictionId ? 1.22 : 1);
    return group;
  }

  private refreshEnemyThreatSelection(): void {
    for (const marker of this.enemyThreatGroup.children) {
      const selected = marker.userData.enemyInterdictionId === this.selectedEnemyInterdictionId;
      marker.scale.setScalar(selected ? 1.22 : 1);
      const ring = marker.getObjectByName('EnemyThreatSelectionRing') as Mesh | undefined;
      if (ring?.material instanceof MeshBasicMaterial) {
        ring.material.opacity = selected ? 0.92 : 0;
      }
    }
  }

  private createFriendlyCountermeasureMarker(region: RegionViewModel, target: EnemyInterdictionTarget): Group {
    const group = new Group();
    group.name = `FriendlyCountermeasure_${target.id}`;
    group.userData.regionId = region.definition.id;
    group.userData.kind = target.lastCountermeasure ?? '';

    const color = countermeasureColor(target.lastCountermeasure);
    const material = new MeshBasicMaterial({ color, transparent: true, opacity: 0.92 });
    const muted = new MeshBasicMaterial({ color: 0x132323, transparent: true, opacity: 0.82 });
    const routeCurve = this.createEnemyThreatRouteCurve(target);
    const baseProgress = friendlyCountermeasureProgress(target.stage, target.remainingTurns, target.lastCountermeasure);
    if (routeCurve) {
      const routePoint = routeCurve.getPoint(baseProgress);
      group.position.copy(routePoint).add(new Vector3(0, 0.52, 0));
      group.userData.routeCurve = routeCurve;
      group.userData.baseProgress = baseProgress;
      group.userData.speed = target.lastCountermeasure === '改道' ? 0.014 : 0.026;
      group.userData.sway = target.lastCountermeasure === '反斥候' ? 0.018 : 0.012;
    } else {
      group.position.set(region.shape.center.x - 0.86, this.regionHeight(region) + 0.88, -region.shape.center.y + 0.18);
    }

    const escortRing = new Mesh(new TorusGeometry(0.26, 0.028, 8, 32), material);
    escortRing.rotation.x = -Math.PI / 2;
    const body = new Mesh(new BoxGeometry(0.34, 0.12, 0.22), material);
    body.position.y = 0.09;
    const cap = new Mesh(new BoxGeometry(0.2, 0.1, 0.16), muted);
    cap.position.y = 0.21;
    const pennant = new Mesh(new ConeGeometry(0.13, 0.28, 3), material);
    pennant.rotation.x = Math.PI / 2;
    pennant.position.set(0.27, 0.26, 0);

    if (target.lastCountermeasure === '反斥候') {
      const scoutLens = new Mesh(new TorusGeometry(0.14, 0.022, 8, 24), material);
      scoutLens.position.set(-0.24, 0.18, 0);
      scoutLens.rotation.y = Math.PI / 2;
      group.add(scoutLens);
    } else if (target.lastCountermeasure === '诱敌') {
      const bait = new Mesh(new CylinderGeometry(0.1, 0.12, 0.14, 8), material);
      bait.position.set(-0.24, 0.12, 0);
      group.add(bait);
    } else if (target.lastCountermeasure === '改道') {
      const turn = new Mesh(new TorusGeometry(0.16, 0.02, 8, 22, Math.PI * 1.45), material);
      turn.position.set(-0.24, 0.16, 0);
      turn.rotation.x = -Math.PI / 2;
      group.add(turn);
    }

    group.add(escortRing, body, cap, pennant);
    return group;
  }

  private createEnemyThreatRouteCurve(target: EnemyInterdictionTarget): CatmullRomCurve3 | null {
    const army = this.dataset.armies.find((candidate) => candidate.id === target.armyId) ?? this.activeArmy;
    const from = this.dataset.regionById.get(army.fromRegionId);
    const to = this.dataset.regionById.get(target.regionId);
    if (!from || !to) return null;
    const fromPoint = new Vector3(from.shape.center.x, this.regionHeight(from) + 0.92, -from.shape.center.y);
    const toPoint = new Vector3(to.shape.center.x, this.regionHeight(to) + 0.92, -to.shape.center.y);
    const waypointRegion = army.waypointRegionId ? this.dataset.regionById.get(army.waypointRegionId) : undefined;
    if (waypointRegion && waypointRegion.definition.id !== from.definition.id && waypointRegion.definition.id !== to.definition.id) {
      const waypoint = new Vector3(waypointRegion.shape.center.x, this.regionHeight(waypointRegion) + 1.02, -waypointRegion.shape.center.y);
      return new CatmullRomCurve3([
        fromPoint,
        fromPoint.clone().lerp(waypoint, 0.52).add(new Vector3(0, 0.52, 0)),
        waypoint,
        waypoint.clone().lerp(toPoint, 0.54).add(new Vector3(0, 0.5, 0)),
        toPoint
      ]);
    }

    return new CatmullRomCurve3([
      fromPoint,
      fromPoint.clone().lerp(toPoint, 0.5).add(new Vector3(0, 0.72, 0)),
      toPoint
    ]);
  }

  private createLogisticsMapObjectMarker(object: LogisticsMapObject): Group | null {
    const region = this.dataset.regionById.get(object.regionId);
    if (!region) return null;

    const group = new Group();
    group.name = `LogisticsMapObject_${object.id}`;
    group.userData.logisticsObjectId = object.id;
    group.userData.kind = object.kind;
    group.userData.baseScale = object.kind === 'logistics-station' ? 1.05 : object.kind === 'route-blockade' ? 1.12 : 1;
    group.position.copy(this.logisticsObjectPosition(object, region));

    const selected = object.id === this.selectedLogisticsObjectId;
    const color = logisticsObjectColor(object.kind);
    const material = new MeshStandardMaterial({
      color,
      emissive: color,
      emissiveIntensity: selected ? 0.44 : 0.18,
      roughness: 0.52,
      metalness: 0.04,
      transparent: true,
      opacity: object.status.includes('取消') ? 0.38 : 0.92
    });

    if (object.kind === 'logistics-station') {
      const base = new Mesh(new CylinderGeometry(0.22, 0.28, 0.16, 8), material);
      const tower = new Mesh(new BoxGeometry(0.18, 0.38, 0.18), material);
      tower.position.y = 0.26;
      const cap = new Mesh(new ConeGeometry(0.22, 0.2, 6), material);
      cap.position.y = 0.58;
      group.add(base, tower, cap);
    } else if (object.kind === 'route-blockade') {
      const base = new Mesh(new CylinderGeometry(0.3, 0.34, 0.08, 8), material);
      const postA = new Mesh(new BoxGeometry(0.09, 0.34, 0.09), material);
      postA.position.set(-0.18, 0.2, 0);
      const postB = postA.clone();
      postB.position.x = 0.18;
      const bar = new Mesh(new BoxGeometry(0.48, 0.08, 0.12), material);
      bar.position.y = 0.34;
      bar.rotation.z = -0.24;
      const ring = new Mesh(
        new TorusGeometry(0.36, 0.03, 8, 30),
        new MeshBasicMaterial({ color, transparent: true, opacity: selected ? 0.96 : 0.68 })
      );
      ring.rotation.x = -Math.PI / 2;
      group.add(base, postA, postB, bar, ring);
    } else {
      const cart = new Mesh(new BoxGeometry(0.42, 0.14, 0.24), material);
      const load = new Mesh(new BoxGeometry(0.22, 0.14, 0.18), material);
      load.position.y = 0.15;
      const ring = new Mesh(
        new TorusGeometry(object.kind === 'transport-convoy' ? 0.28 : 0.32, 0.025, 8, 30),
        new MeshBasicMaterial({ color, transparent: true, opacity: selected ? 0.94 : 0.62 })
      );
      ring.rotation.x = -Math.PI / 2;
      const progress = new Mesh(
        new CylinderGeometry(0.035, 0.035, 0.5 * clamp(object.progress, 0.08, 1), 8),
        new MeshBasicMaterial({ color: 0xfff0b0, transparent: true, opacity: 0.82 })
      );
      progress.position.set(0.34, 0.22, 0);
      group.add(ring, cart, load, progress);
    }

    const selectionRing = new Mesh(
      new TorusGeometry(0.42, 0.032, 8, 36),
      new MeshBasicMaterial({ color: 0xfff0a8, transparent: true, opacity: selected ? 0.88 : 0.0 })
    );
    selectionRing.name = 'LogisticsSelectionRing';
    selectionRing.rotation.x = -Math.PI / 2;
    selectionRing.position.y = -0.02;
    group.add(selectionRing);

    group.traverse((child) => {
      child.userData.logisticsObjectId = object.id;
    });
    group.scale.setScalar(selected ? 1.24 : Number(group.userData.baseScale));
    return group;
  }

  private logisticsObjectPosition(object: LogisticsMapObject, region: RegionViewModel): Vector3 {
    if (object.kind !== 'logistics-station' && object.fromRegionId && object.targetRegionId) {
      const curve = this.createLogisticsRouteCurve(object.fromRegionId, object.targetRegionId);
      if (curve) {
        const point = curve.getPoint(clamp(object.progress, 0.12, 0.92));
        return point.add(new Vector3(0, object.kind === 'transport-convoy' ? 0.32 : object.kind === 'route-blockade' ? 0.52 : 0.42, 0));
      }
    }

    const offset = object.kind === 'occupation-supply' ? new Vector3(-0.42, 0.78, 0.34) : new Vector3(0.42, 0.68, -0.34);
    return new Vector3(region.shape.center.x, this.regionHeight(region), -region.shape.center.y).add(offset);
  }

  private createLogisticsRouteCurve(fromRegionId: string, targetRegionId: string): CatmullRomCurve3 | null {
    const from = this.dataset.regionById.get(fromRegionId);
    const to = this.dataset.regionById.get(targetRegionId);
    if (!from || !to) return null;
    const fromPoint = new Vector3(from.shape.center.x, this.regionHeight(from) + 0.88, -from.shape.center.y);
    const toPoint = new Vector3(to.shape.center.x, this.regionHeight(to) + 0.88, -to.shape.center.y);
    return new CatmullRomCurve3([
      fromPoint,
      fromPoint.clone().lerp(toPoint, 0.5).add(new Vector3(0, 0.58, 0)),
      toPoint
    ]);
  }

  private refreshLogisticsObjectSelection(): void {
    for (const marker of this.logisticsObjectGroup.children) {
      const selected = marker.userData.logisticsObjectId === this.selectedLogisticsObjectId;
      const baseScale = Number(marker.userData.baseScale ?? 1);
      marker.scale.setScalar(selected ? 1.24 : baseScale);
      const ring = marker.getObjectByName('LogisticsSelectionRing') as Mesh | undefined;
      if (ring?.material instanceof MeshBasicMaterial) {
        ring.material.opacity = selected ? 0.88 : 0;
      }
      marker.traverse((child) => {
        if (child instanceof Mesh && child.material instanceof MeshStandardMaterial) {
          child.material.emissiveIntensity = selected ? 0.44 : 0.18;
        }
      });
    }
  }

  private featureBasePosition(region: RegionViewModel, offsetX: number, offsetZ: number): Vector3 {
    return new Vector3(
      region.shape.center.x + offsetX,
      this.regionHeight(region) + 0.04,
      -region.shape.center.y + offsetZ
    );
  }

  private buildRoute(route: RouteForecast): void {
    this.routeGroup.clear();
    this.routeRaidSegmentCount = 0;
    this.routeConvoyMarkerCount = 0;
    this.routeDragHandleCount = 0;
    const from = new Vector3(route.from.shape.center.x, 0.72, -route.from.shape.center.y);
    const to = new Vector3(route.target.shape.center.x, 0.72, -route.target.shape.center.y);
    const waypointRegion = route.army.waypointRegionId ? this.dataset.regionById.get(route.army.waypointRegionId) : undefined;
    const waypoint = waypointRegion ? new Vector3(waypointRegion.shape.center.x, 0.88, -waypointRegion.shape.center.y) : undefined;
    const mid = from.clone().lerp(to, 0.5).add(new Vector3(0, 0.85, 0));
    const points = waypoint
      ? [from, from.clone().lerp(waypoint, 0.5).add(new Vector3(0, 0.7, 0)), waypoint, waypoint.clone().lerp(to, 0.5).add(new Vector3(0, 0.7, 0)), to]
      : [from, mid, to];
    const curve = new CatmullRomCurve3(points);
    const pressure = route.supplyCost > 34 ? 0.13 : 0.09;
    const underlay = new Mesh(
      new TubeGeometry(curve, 40, pressure + 0.07, 10, false),
      new MeshBasicMaterial({ color: 0x551f1c, transparent: true, opacity: 0.42 })
    );
    underlay.name = 'WarRouteUnderlay';
    const line = new Mesh(
      new TubeGeometry(curve, 40, pressure, 10, false),
      new MeshBasicMaterial({ color: 0xd96b46, transparent: true, opacity: 0.94 })
    );
    line.name = 'WarRouteLine';
    const contact = new Mesh(
      new TorusGeometry(0.42, 0.045, 10, 42),
      new MeshBasicMaterial({ color: 0xf1c15f, transparent: true, opacity: 0.94 })
    );
    contact.name = 'WarRouteContactNode';
    contact.position.copy(to).add(new Vector3(0, 0.12, 0));
    contact.rotation.x = -Math.PI / 2;

    this.routeGroup.add(underlay, line, contact);
    this.addRoutePressureLayer(curve, route);
    this.addRouteDragHandle(to, 'target');
    if (waypoint) {
      const waypointNode = new Mesh(
        new TorusGeometry(0.32, 0.035, 10, 36),
        new MeshBasicMaterial({ color: 0x7ad7c9, transparent: true, opacity: 0.92 })
      );
      waypointNode.name = 'WarRouteWaypointNode';
      waypointNode.position.copy(waypoint).add(new Vector3(0, 0.08, 0));
      waypointNode.rotation.x = -Math.PI / 2;
      this.routeGroup.add(waypointNode);
      this.addRouteDragHandle(waypoint, 'waypoint');
    }
    this.routeGroup.visible = false;
  }

  private addRoutePressureLayer(curve: CatmullRomCurve3, route: RouteForecast): void {
    const convoyCount = route.supplyCost >= 34 || route.interceptionRisk >= 40 ? 3 : 2;
    for (let index = 0; index < convoyCount; index += 1) {
      const t = (index + 1) / (convoyCount + 1);
      this.addConvoyMarker(curve.getPoint(t), curve.getTangent(t), index);
    }

    const start = route.interceptionRisk >= 42 ? 0.56 : 0.68;
    const raidPoints = [start, start + 0.1, start + 0.2, 0.96].map((t) => curve.getPoint(Math.min(0.98, t)));
    const raidCurve = new CatmullRomCurve3(raidPoints);
    const raid = new Mesh(
      new TubeGeometry(raidCurve, 18, 0.08 + route.interceptionRisk / 900, 8, false),
      new MeshBasicMaterial({
        color: 0xff4e3f,
        transparent: true,
        opacity: clamp(0.52 + route.interceptionRisk / 130, 0.58, 0.96)
      })
    );
    raid.name = 'WarRouteRaidSegment';
    this.routeGroup.add(raid);
    this.routeRaidSegmentCount = 1;

    for (const t of [0.72, 0.86]) {
      this.addRaidWarningMarker(curve.getPoint(t), curve.getTangent(t));
    }
  }

  private addConvoyMarker(position: Vector3, tangent: Vector3, index: number): void {
    const group = new Group();
    group.name = `WarRouteConvoyMarker_${index}`;
    group.position.copy(position).add(new Vector3(0, 0.2, 0));
    group.rotation.y = Math.atan2(tangent.x, tangent.z);

    const cart = new Mesh(
      new BoxGeometry(0.44, 0.12, 0.24),
      new MeshBasicMaterial({ color: 0xf2c96a, transparent: true, opacity: 0.92 })
    );
    const load = new Mesh(
      new BoxGeometry(0.26, 0.12, 0.18),
      new MeshBasicMaterial({ color: 0xb89145, transparent: true, opacity: 0.96 })
    );
    load.position.y = 0.12;
    const halo = new Mesh(
      new TorusGeometry(0.24, 0.022, 8, 28),
      new MeshBasicMaterial({ color: 0xffdf7a, transparent: true, opacity: 0.88 })
    );
    halo.rotation.x = -Math.PI / 2;

    const wheelMaterial = new MeshBasicMaterial({ color: 0x2b2520, transparent: true, opacity: 0.88 });
    const wheelA = new Mesh(new CylinderGeometry(0.06, 0.06, 0.045, 8), wheelMaterial);
    wheelA.rotation.z = Math.PI / 2;
    wheelA.position.set(-0.14, -0.065, 0.13);
    const wheelB = wheelA.clone();
    wheelB.position.z = -0.13;
    group.add(halo, cart, load, wheelA, wheelB);
    this.routeGroup.add(group);
    this.routeConvoyMarkerCount += 1;
  }

  private addRaidWarningMarker(position: Vector3, tangent: Vector3): void {
    const group = new Group();
    group.name = 'WarRouteRaidWarning';
    group.position.copy(position).add(new Vector3(0, 0.28, 0));
    group.rotation.y = Math.atan2(tangent.x, tangent.z);

    const warning = new Mesh(
      new ConeGeometry(0.18, 0.34, 3),
      new MeshBasicMaterial({ color: 0xff5b49, transparent: true, opacity: 0.95 })
    );
    warning.rotation.x = Math.PI / 2;
    const base = new Mesh(
      new TorusGeometry(0.22, 0.026, 8, 24),
      new MeshBasicMaterial({ color: 0xff8a5e, transparent: true, opacity: 0.84 })
    );
    base.rotation.x = -Math.PI / 2;
    group.add(base, warning);
    this.routeGroup.add(group);
  }

  private addRouteDragHandle(position: Vector3, kind: 'target' | 'waypoint'): void {
    const color = kind === 'target' ? 0xffd36b : 0x7ad7c9;
    const handle = new Group();
    handle.name = `WarRouteDragHandle_${kind}`;
    handle.position.copy(position).add(new Vector3(0, 0.42, 0));

    const stem = new Mesh(
      new CylinderGeometry(0.035, 0.035, 0.34, 8),
      new MeshBasicMaterial({ color, transparent: true, opacity: 0.9 })
    );
    const cap = new Mesh(
      new SphereGeometry(0.115, 12, 8),
      new MeshBasicMaterial({ color, transparent: true, opacity: 0.96 })
    );
    cap.position.y = 0.24;
    handle.add(stem, cap);
    this.routeGroup.add(handle);
    this.routeDragHandleCount += 1;
  }

  private rebuildRoute(): void {
    const wasVisible = this.routeGroup.visible;
    this.buildRoute(this.createRouteForecast(this.activeArmy));
    this.routeGroup.visible = this.mode === 'war' || wasVisible;
  }

  private createRouteForecast(army: ArmyViewModel): RouteForecast {
    const from = this.dataset.regionById.get(army.fromRegionId);
    const target = this.dataset.regionById.get(army.targetRegionId);
    if (!from || !target) {
      throw new Error(`Route regions are missing for ${army.id}.`);
    }
    const waypoint = army.waypointRegionId ? this.dataset.regionById.get(army.waypointRegionId) : undefined;
    const directDistance = Math.hypot(from.shape.center.x - target.shape.center.x, from.shape.center.y - target.shape.center.y);
    const distance = waypoint
      ? Math.hypot(from.shape.center.x - waypoint.shape.center.x, from.shape.center.y - waypoint.shape.center.y)
        + Math.hypot(waypoint.shape.center.x - target.shape.center.x, waypoint.shape.center.y - target.shape.center.y)
      : directDistance;
    const supplyCost = Math.round(16 + distance * 3 + Math.max(0, target.risk - 15) * 0.4 + Math.max(0, 70 - army.supply) * 0.12);
    const contactChance = clamp(44 + target.risk * 0.6 + (target.owner === 'rival' ? 16 : 0) + Math.max(0, army.morale - 70) * 0.18, 0, 96);
    const occupationCost = Math.round(42 + target.definition.localPower * 0.8 + target.definition.population / 50000);
    const interceptionRisk = clamp(18 + (target.history?.weaponTraditions?.length ?? 0) * 7 + (target.definition.terrain.includes('mountain') ? 16 : 0) + Math.max(0, 72 - army.supply) * 0.25, 0, 94);
    return {
      army,
      from,
      target,
      supplyCost,
      turns: Math.max(1, Math.round(distance / 2.2)),
      contactChance,
      occupationCost,
      interceptionRisk,
      summary: `从${from.definition.name}出军至${target.definition.name}，粮草消耗${supplyCost}，接敌${Math.round(contactChance)}%`
    };
  }

  private buildSelectionRings(): void {
    this.selectedRing.geometry = new TorusGeometry(0.86, 0.035, 8, 64);
    this.selectedRing.material = new MeshBasicMaterial({ color: 0xf0cc68, transparent: true, opacity: 0.95 });
    this.selectedRing.rotation.x = -Math.PI / 2;
    this.selectedRing.name = 'SelectedRegionRing';
    this.scene.add(this.selectedRing);

    this.hoverRing.geometry = new TorusGeometry(0.72, 0.025, 8, 48);
    this.hoverRing.material = new MeshBasicMaterial({ color: 0x9bd5d6, transparent: true, opacity: 0.7 });
    this.hoverRing.rotation.x = -Math.PI / 2;
    this.hoverRing.visible = false;
    this.hoverRing.name = 'HoverRegionRing';
    this.scene.add(this.hoverRing);
  }

  private placeRing(ring: Mesh, region: RegionViewModel, yOffset: number): void {
    ring.position.set(region.shape.center.x, this.regionHeight(region) + yOffset, -region.shape.center.y);
    const maxSpan = Math.max(...region.shape.boundary.map((point) => Math.hypot(point.x - region.shape.center.x, point.y - region.shape.center.y)));
    ring.scale.setScalar(Math.max(0.72, maxSpan * 0.88));
    ring.visible = true;
  }

  private handlePointerMove = (event: PointerEvent): void => {
    if (this.draggingRouteNode) {
      this.handleRouteNodeDrag(event);
      return;
    }

    // Track hovered drag handle
    const hoveredHandle = this.pickRouteDragNode(event.clientX, event.clientY);
    const hoveredEnemyThreat = hoveredHandle ? null : this.pickEnemyInterdictionTarget(event.clientX, event.clientY);
    const hoveredLogisticsObject = hoveredHandle || hoveredEnemyThreat ? null : this.pickLogisticsMapObject(event.clientX, event.clientY);
    if (hoveredHandle !== this.hoveredDragHandle) {
      this.hoveredDragHandle = hoveredHandle;
    }
    this.canvas.style.cursor = hoveredHandle ? 'grab' : hoveredLogisticsObject || hoveredEnemyThreat ? 'pointer' : 'default';

    const region = this.pickRegion(event);
    if (region?.definition.id === this.hoveredRegion?.definition.id) return;

    this.hoveredRegion = region;
    if (region) {
      this.placeRing(this.hoverRing, region, 0.12);
    } else {
      this.hoverRing.visible = false;
    }
    this.events.onHoverRegion(region);
  };

  private handlePointerDown = (event: PointerEvent): void => {
    const dragNode = this.pickRouteDragNode(event.clientX, event.clientY);
    if (!dragNode) return;
    this.draggingRouteNode = dragNode;
    this.controls.enabled = false;
    this.canvas.setPointerCapture(event.pointerId);
    event.preventDefault();
  };

  private handlePointerUp = (event: PointerEvent): void => {
    if (!this.draggingRouteNode) return;
    this.draggingRouteNode = null;
    this.suppressNextClick = true;
    this.controls.enabled = true;
    if (this.canvas.hasPointerCapture(event.pointerId)) {
      this.canvas.releasePointerCapture(event.pointerId);
    }
    event.preventDefault();
  };

  private handleClick = (event: PointerEvent): void => {
    if (this.draggingRouteNode || this.suppressNextClick) {
      this.suppressNextClick = false;
      return;
    }
    const enemyCenterHit = this.pickEnemyThreatProjectedHit(event.clientX, event.clientY);
    const logisticsCenterHit = this.pickLogisticsObjectProjectedHit(event.clientX, event.clientY);
    if (enemyCenterHit || logisticsCenterHit) {
      if (enemyCenterHit && (!logisticsCenterHit || enemyCenterHit.distance <= logisticsCenterHit.distance)) {
        this.selectedEnemyInterdictionId = enemyCenterHit.id;
        this.refreshEnemyThreatSelection();
        this.events.onSelectEnemyInterdiction?.(enemyCenterHit.id);
      } else if (logisticsCenterHit) {
        this.selectedLogisticsObjectId = logisticsCenterHit.id;
        this.refreshLogisticsObjectSelection();
        this.events.onSelectLogisticsObject?.(logisticsCenterHit.id);
      }
      event.preventDefault();
      return;
    }
    const enemyInterdictionId = this.pickEnemyInterdictionTarget(event.clientX, event.clientY);
    if (enemyInterdictionId) {
      this.selectedEnemyInterdictionId = enemyInterdictionId;
      this.refreshEnemyThreatSelection();
      this.events.onSelectEnemyInterdiction?.(enemyInterdictionId);
      event.preventDefault();
      return;
    }
    const logisticsObjectId = this.pickLogisticsMapObject(event.clientX, event.clientY);
    if (logisticsObjectId) {
      this.selectedLogisticsObjectId = logisticsObjectId;
      this.refreshLogisticsObjectSelection();
      this.events.onSelectLogisticsObject?.(logisticsObjectId);
      event.preventDefault();
      return;
    }
    const region = this.pickRegion(event);
    if (region) {
      this.selectRegion(region.definition.id);
    }
  };

  private pickRegion(event: PointerEvent): RegionViewModel | null {
    const rect = this.renderer.domElement.getBoundingClientRect();
    const clientX = event.clientX;
    const clientY = event.clientY;
    const centerPick = this.pickRegionByProjectedCenter(clientX, clientY);
    if (centerPick) return centerPick;

    this.pointer.x = ((clientX - rect.left) / rect.width) * 2 - 1;
    this.pointer.y = -((clientY - rect.top) / rect.height) * 2 + 1;
    this.raycaster.setFromCamera(this.pointer, this.camera);
    const hits = this.raycaster.intersectObjects([...this.regionMeshes.values()], false);
    const regionId = hits[0]?.object.userData.regionId as string | undefined;
    return regionId ? this.dataset.regionById.get(regionId) ?? null : null;
  }

  private pickRouteDragNode(clientX: number, clientY: number): 'target' | 'waypoint' | null {
    if (this.mode !== 'war') return null;
    const nodes: Array<{ kind: 'target' | 'waypoint'; point: { x: number; y: number; visible: boolean } | null }> = [];
    if (this.activeArmy.waypointRegionId) {
      nodes.push({ kind: 'waypoint', point: this.getRegionScreenPoint(this.activeArmy.waypointRegionId) });
    }
    nodes.push({ kind: 'target', point: this.getRegionScreenPoint(this.activeArmy.targetRegionId) });

    let best: { kind: 'target' | 'waypoint'; distance: number } | null = null;
    for (const node of nodes) {
      if (!node.point?.visible) continue;
      const distance = Math.hypot(node.point.x - clientX, node.point.y - clientY);
      if (distance <= 34 && (!best || distance < best.distance)) {
        best = { kind: node.kind, distance };
      }
    }
    return best?.kind ?? null;
  }

  private pickLogisticsMapObject(clientX: number, clientY: number): string | null {
    if (this.mode !== 'war' || this.logisticsMapObjectCount <= 0) return null;
    const centerPick = this.pickLogisticsObjectByProjectedCenter(clientX, clientY);
    if (centerPick) return centerPick;

    const rect = this.renderer.domElement.getBoundingClientRect();
    this.pointer.x = ((clientX - rect.left) / rect.width) * 2 - 1;
    this.pointer.y = -((clientY - rect.top) / rect.height) * 2 + 1;
    this.raycaster.setFromCamera(this.pointer, this.camera);
    const hits = this.raycaster.intersectObjects(this.logisticsObjectGroup.children, true);
    const objectId = hits.find((hit) => typeof hit.object.userData.logisticsObjectId === 'string')?.object.userData.logisticsObjectId as string | undefined;
    return objectId ?? null;
  }

  private pickLogisticsObjectByProjectedCenter(clientX: number, clientY: number): string | null {
    return this.pickLogisticsObjectProjectedHit(clientX, clientY)?.id ?? null;
  }

  private pickLogisticsObjectProjectedHit(clientX: number, clientY: number): { id: string; distance: number } | null {
    let best: { id: string; distance: number } | null = null;
    for (const marker of this.logisticsObjectGroup.children) {
      const objectId = marker.userData.logisticsObjectId as string | undefined;
      if (!objectId) continue;
      const point = this.projectToScreen(marker.getWorldPosition(new Vector3()));
      if (!point.visible) continue;
      const distance = Math.hypot(point.x - clientX, point.y - clientY);
      if (distance <= 24 && (!best || distance < best.distance)) {
        best = { id: objectId, distance };
      }
    }
    return best;
  }

  private pickEnemyInterdictionTarget(clientX: number, clientY: number): string | null {
    if (this.mode !== 'war' || this.enemyThreatMarkerCount <= 0) return null;
    const centerPick = this.pickEnemyThreatByProjectedCenter(clientX, clientY);
    if (centerPick) return centerPick;

    const rect = this.renderer.domElement.getBoundingClientRect();
    this.pointer.x = ((clientX - rect.left) / rect.width) * 2 - 1;
    this.pointer.y = -((clientY - rect.top) / rect.height) * 2 + 1;
    this.raycaster.setFromCamera(this.pointer, this.camera);
    const hits = this.raycaster.intersectObjects(this.enemyThreatGroup.children, true);
    const orderId = hits.find((hit) => typeof hit.object.userData.enemyInterdictionId === 'string')?.object.userData.enemyInterdictionId as string | undefined;
    return orderId ?? null;
  }

  private pickEnemyThreatByProjectedCenter(clientX: number, clientY: number): string | null {
    return this.pickEnemyThreatProjectedHit(clientX, clientY)?.id ?? null;
  }

  private pickEnemyThreatProjectedHit(clientX: number, clientY: number): { id: string; distance: number } | null {
    let best: { id: string; distance: number } | null = null;
    for (const marker of this.enemyThreatGroup.children) {
      const orderId = marker.userData.enemyInterdictionId as string | undefined;
      if (!orderId) continue;
      const point = this.projectToScreen(marker.getWorldPosition(new Vector3()));
      if (!point.visible) continue;
      const distance = Math.hypot(point.x - clientX, point.y - clientY);
      if (distance <= 28 && (!best || distance < best.distance)) {
        best = { id: orderId, distance };
      }
    }
    return best;
  }

  private handleRouteNodeDrag(event: PointerEvent): void {
    const region = this.pickRegion(event);
    if (!region) return;

    if (this.draggingRouteNode === 'target' && region.definition.id !== this.activeArmy.fromRegionId) {
      this.retargetActiveArmy(region.definition.id);
      this.events.onRouteEdited?.(region, this.activeArmy);
    } else if (
      this.draggingRouteNode === 'waypoint' &&
      region.definition.id !== this.activeArmy.fromRegionId &&
      region.definition.id !== this.activeArmy.targetRegionId
    ) {
      this.setActiveArmyWaypoint(region.definition.id);
      this.events.onRouteEdited?.(region, this.activeArmy);
    }
    event.preventDefault();
  }

  private pickRegionByProjectedCenter(clientX: number, clientY: number): RegionViewModel | null {
    let best: { region: RegionViewModel; distance: number } | null = null;
    for (const region of this.dataset.regions) {
      const point = this.getRegionScreenPoint(region.definition.id);
      if (!point?.visible) continue;
      const distance = Math.hypot(point.x - clientX, point.y - clientY);
      if (distance <= 28 && (!best || distance < best.distance)) {
        best = { region, distance };
      }
    }
    return best?.region ?? null;
  }

  private resize = (): void => {
    const width = this.canvas.clientWidth || window.innerWidth;
    const height = this.canvas.clientHeight || window.innerHeight;
    this.camera.aspect = width / Math.max(1, height);
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(width, height, false);
  };

  private animateWarPressure(): void {
    const elapsed = this.timer.getElapsed();
    const contact = this.routeGroup.getObjectByName('WarRouteContactNode');
    if (contact) {
      const scale = 1 + Math.sin(elapsed * 3.4) * 0.1;
      contact.scale.setScalar(scale);
    }

    for (const threat of this.enemyThreatGroup.children) {
      const routeCurve = threat.userData.routeCurve as CatmullRomCurve3 | undefined;
      const baseProgress = Number(threat.userData.baseProgress ?? 0);
      const speed = Number(threat.userData.speed ?? 0);
      const sway = Number(threat.userData.sway ?? 0);
      if (!routeCurve) {
        threat.scale.setScalar(1 + Math.sin(elapsed * 3.1) * 0.04);
        continue;
      }

      const progress = clamp(baseProgress + Math.sin(elapsed * 1.9 + baseProgress * 7) * sway + (elapsed * speed) % 0.028, 0.08, 0.96);
      const point = routeCurve.getPoint(progress);
      const tangent = routeCurve.getTangent(progress);
      threat.position.copy(point).add(new Vector3(0, 0.3 + Math.sin(elapsed * 4.2 + baseProgress) * 0.04, 0));
      threat.rotation.y = Math.atan2(tangent.x, tangent.z);
      threat.scale.setScalar(1 + Math.sin(elapsed * 4.6 + baseProgress * 5) * 0.05);
    }

    for (const countermeasure of this.friendlyCountermeasureGroup.children) {
      const routeCurve = countermeasure.userData.routeCurve as CatmullRomCurve3 | undefined;
      const baseProgress = Number(countermeasure.userData.baseProgress ?? 0);
      const speed = Number(countermeasure.userData.speed ?? 0);
      const sway = Number(countermeasure.userData.sway ?? 0);
      if (!routeCurve) {
        countermeasure.scale.setScalar(1 + Math.sin(elapsed * 3.5) * 0.04);
        continue;
      }

      const progress = clamp(baseProgress + Math.sin(elapsed * 2.2 + baseProgress * 5) * sway + (elapsed * speed) % 0.024, 0.06, 0.88);
      const point = routeCurve.getPoint(progress);
      const tangent = routeCurve.getTangent(progress);
      countermeasure.position.copy(point).add(new Vector3(0, 0.52 + Math.sin(elapsed * 4.0 + baseProgress) * 0.035, 0));
      countermeasure.rotation.y = Math.atan2(tangent.x, tangent.z);
      countermeasure.scale.setScalar(1 + Math.sin(elapsed * 4.2 + baseProgress * 4) * 0.045);
    }
  }

  private animateDragHandles(): void {
    const elapsed = this.timer.getElapsed();

    // Animate drag handles based on hover/drag state
    for (const handle of this.routeGroup.children) {
      if (!handle.name.startsWith('WarRouteDragHandle_')) continue;

      const kind = handle.name.replace('WarRouteDragHandle_', '') as 'target' | 'waypoint';
      const isHovered = this.hoveredDragHandle === kind;
      const isDragging = this.draggingRouteNode === kind;

      // Scale: normal=1, hovered=1.15, dragging=1.3
      let targetScale = 1;
      if (isDragging) targetScale = 1.3;
      else if (isHovered) targetScale = 1.15;

      // Smooth lerp to target scale
      const currentScale = handle.scale.x;
      const newScale = currentScale + (targetScale - currentScale) * 0.15;
      handle.scale.setScalar(newScale);

      // Pulse opacity for hovered/dragging
      for (const child of handle.children) {
        if (child instanceof Mesh && child.material instanceof MeshBasicMaterial) {
          const baseOpacity = kind === 'target' ? 0.9 : 0.85;
          const pulse = isDragging ? Math.sin(elapsed * 8) * 0.15 : isHovered ? Math.sin(elapsed * 5) * 0.1 : 0;
          child.material.opacity = clamp(baseOpacity + pulse, 0.5, 1);
        }
      }
    }
  }

  private resolveRegionColor(region: RegionViewModel): number {
    if (this.mode === 'war') {
      if (region.owner === 'player') return 0x2d6b68;
      if (region.owner === 'rival') return 0x80362d;
      return 0x3a4749;
    }

    if (region.risk >= 36) return 0xa24c3d;
    if (region.specialization.includes('粮')) return 0x5d7f45;
    if (region.specialization.includes('军') || region.specialization.includes('边防')) return 0x6f5a3b;
    if (region.specialization.includes('商')) return 0x3d7282;
    if (region.specialization.includes('法统') || region.specialization.includes('文化')) return 0x6f6aa2;
    return 0x436861;
  }

  private regionHeight(region: RegionViewModel): number {
    const terrain = region.definition.terrain;
    if (terrain.includes('mountain')) return 0.42;
    if (terrain.includes('hill') || terrain.includes('plateau')) return 0.32;
    if (terrain.includes('river')) return 0.16;
    if (terrain.includes('subtropical')) return 0.22;
    return 0.24;
  }

  private regionLabelPriority(region: RegionViewModel): number {
    let priority = region.owner === 'player' ? 90 : region.owner === 'rival' ? 72 : 48;
    priority += Math.min(30, Math.round(region.definition.population / 80000));
    priority += region.definition.id === this.selectedRegion.definition.id ? 100 : 0;
    priority += region.risk >= 30 ? 24 : 0;
    return priority;
  }
}

function markerMaterial(color: number): MeshStandardMaterial {
  return new MeshStandardMaterial({
    color,
    roughness: 0.72,
    metalness: 0.04,
    transparent: true,
    opacity: 0.86
  });
}

function shouldShowOccupationBadge(stage: RegionViewModel['controlStage']): boolean {
  return stage === 'military-govern' || stage === 'pacify' || stage === 'register';
}

function occupationBadgeConfig(stage: RegionViewModel['controlStage']): { color: number; pips: number } {
  if (stage === 'military-govern') return { color: 0xff6a50, pips: 1 };
  if (stage === 'pacify') return { color: 0xf0bf63, pips: 2 };
  if (stage === 'register') return { color: 0x74d1bd, pips: 3 };
  return { color: 0x5d8f7d, pips: 0 };
}

function enemyThreatColor(stage: EnemyInterdictionStage): number {
  if (stage === 'resolved') return 0x666666;
  if (stage === 'striking') return 0xff4b3f;
  if (stage === 'moving') return 0xd26d3b;
  return 0x8d2f2d;
}

function enemyThreatRouteProgress(stage: EnemyInterdictionStage, remainingTurns: number, dampened: boolean): number {
  const delay = dampened ? -0.08 : 0;
  if (stage === 'resolved') return 0.95;
  if (stage === 'planning') return clamp(0.52 + Math.max(0, 3 - remainingTurns) * 0.04 + delay, 0.42, 0.68);
  if (stage === 'moving') return clamp(0.7 + delay, 0.56, 0.78);
  return clamp(0.86 + delay * 0.5, 0.72, 0.92);
}

function friendlyCountermeasureProgress(stage: EnemyInterdictionStage, remainingTurns: number, countermeasure?: string): number {
  const interceptBias = countermeasure === '反斥候' ? 0.06 : countermeasure === '诱敌' ? 0.1 : 0;
  if (stage === 'planning') return clamp(0.38 + Math.max(0, 3 - remainingTurns) * 0.035 + interceptBias, 0.28, 0.56);
  if (stage === 'moving') return clamp(0.54 + interceptBias, 0.42, 0.68);
  return clamp(0.66 + interceptBias * 0.5, 0.54, 0.78);
}

function countermeasureColor(countermeasure?: string): number {
  if (countermeasure === '反斥候') return 0x7ad7c9;
  if (countermeasure === '诱敌') return 0xf0bf63;
  if (countermeasure === '改道') return 0x93c887;
  return 0xffd36b;
}

function logisticsObjectColor(kind: LogisticsMapObject['kind']): number {
  if (kind === 'transport-convoy') return 0xf2c96a;
  if (kind === 'occupation-supply') return 0x7ad7c9;
  if (kind === 'route-blockade') return 0xb95045;
  return 0x93c887;
}

function matches(value: string, needles: string[]): boolean {
  const normalized = value.toLowerCase();
  return needles.some((needle) => normalized.includes(needle));
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}
