using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class DemoEntityVisualSpawner : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private float portraitY = 5.2f;
        [SerializeField] private float armyIconYOffset = 0.42f;
        [SerializeField] private float portraitScale = 1.15f;
        [SerializeField] private float unitIconScale = 0.7f;
        [SerializeField] private bool showFactionPortraitStrip;
        [SerializeField] private bool showIdleArmies;
        [SerializeField] private bool showWarOperationalOverlays = true;
        [SerializeField] private float detailLabelGapPixels = 26f;
        [SerializeField] private float operationLabelGapPixels = 40f;
        [SerializeField] private float overviewLabelGapPixels = 56f;

        private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, Vector2> fallbackPositions = new Dictionary<string, Vector2>();

        private Material overlayMaterial;
        private int lastTurn = -1;
        private string lastSeason;
        private string lastVisualSignature;
        private bool forceRefresh = true;
        private bool subscribed;

        public int LastVisibleLabelCount { get; private set; }
        public int LastHiddenLabelCount { get; private set; }
        public int LastHiddenByZoomCount { get; private set; }
        public int LastHiddenByBudgetCount { get; private set; }
        public int LastHiddenByOverlapCount { get; private set; }
        public int LastLabelCandidateCount { get; private set; }
        public int LastActivePulseCount { get; private set; }
        public int LastInactivePulseCount { get; private set; }
        public int LastPulseBudget { get; private set; }
        public int LastWarOverlayObjectCount { get; private set; }
        public int LastWarRouteObjectCount { get; private set; }
        public int LastWarContactObjectCount { get; private set; }
        public int LastWarOccupationObjectCount { get; private set; }
        public int LastWarTargetObjectCount { get; private set; }
        public int LastWarLabelObjectCount { get; private set; }

        private void Start()
        {
            ResolveGameManager();
            SubscribeEvents();
            SpawnIfReady();
        }

        private void LateUpdate()
        {
            ResolveGameManager();
            SubscribeEvents();
            SpawnIfReady();
            ApplyLabelDensityForCurrentZoom();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void Bind(GameManager manager)
        {
            if (gameManager != manager)
            {
                UnsubscribeEvents();
            }

            gameManager = manager;
            lastTurn = -1;
            lastSeason = null;
            lastVisualSignature = null;
            forceRefresh = true;
            SubscribeEvents();
        }

        private void SpawnIfReady()
        {
            if (gameManager == null || gameManager.State == null || gameManager.Data == null || !gameManager.Data.IsLoaded) return;
            if (!NeedsRefresh()) return;

            ClearChildren();
            BuildFallbackPositions();
            if (showFactionPortraitStrip)
            {
                SpawnEmperorPortraits();
            }
            SpawnArmies();
            if (showWarOperationalOverlays)
            {
                SpawnWarOperationalOverlays();
            }
            CaptureRefreshState();
            forceRefresh = false;
            ApplyLabelDensityForCurrentZoom();
            RefreshOverlayObjectStats();
        }

        private void SpawnEmperorPortraits()
        {
            List<FactionState> factions = gameManager.State.factions;
            if (factions == null || factions.Count == 0) return;

            float startX = -(factions.Count - 1) * 0.95f * 0.5f;
            for (int i = 0; i < factions.Count; i++)
            {
                FactionState faction = factions[i];
                if (faction == null || string.IsNullOrEmpty(faction.emperorId)) continue;

                Sprite sprite = LoadSprite("Art/Portraits/" + faction.emperorId + ".png", 256f);
                if (sprite == null) continue;

                GameObject portrait = CreateSpriteObject("Emperor_" + faction.emperorId, sprite, new Vector3(startX + i * 0.95f, portraitY, -0.2f), 30, portraitScale);
                portrait.transform.SetParent(transform, false);
                CreateLabel(portrait.transform, ResolveEmperorLabel(faction), new Vector3(0f, -0.58f, -0.02f), 31, 0.11f);
            }
        }

        private void SpawnArmies()
        {
            if (gameManager.World == null || gameManager.World.Map == null) return;

            Dictionary<string, int> regionArmyCounts = new Dictionary<string, int>();
            foreach (ArmyRuntimeState army in gameManager.World.Map.ArmiesById.Values)
            {
                if (army == null || string.IsNullOrEmpty(army.locationRegionId)) continue;
                if (!ShouldShowArmy(army)) continue;

                Vector2 position;
                if (!TryResolveRegionPosition(army.locationRegionId, out position)) continue;

                int index;
                regionArmyCounts.TryGetValue(army.locationRegionId, out index);
                regionArmyCounts[army.locationRegionId] = index + 1;

                string unitId = string.IsNullOrEmpty(army.unitId) ? "infantry" : army.unitId;
                Sprite sprite = LoadSprite("Art/Icons/Units/" + unitId + ".png", 128f);
                if (sprite == null) continue;

                Vector3 iconPosition = new Vector3(position.x + index * 0.32f, position.y + armyIconYOffset, -0.35f);
                GameObject icon = CreateSpriteObject("Army_" + army.id, sprite, iconPosition, 45, unitIconScale);
                icon.transform.SetParent(transform, false);
                TintArmy(icon, army.ownerFactionId);
                CreateLabel(icon.transform, "ArmyInfo_" + army.id, ResolveArmyInfoLabel(army), new Vector3(0f, -0.44f, -0.02f), 46, 0.075f);
            }
        }

        private void SpawnWarOperationalOverlays()
        {
            if (gameManager.World == null || gameManager.World.Map == null) return;

            HashSet<string> highlightedTargetRegions = new HashSet<string>();
            foreach (ArmyRuntimeState army in gameManager.World.Map.ArmiesById.Values)
            {
                if (army == null || !ShouldShowArmy(army)) continue;

                if (army.route != null && army.route.Count >= 2)
                {
                    SpawnArmyRoute(army);
                }

                if (!string.IsNullOrEmpty(army.targetRegionId) && highlightedTargetRegions.Add(army.targetRegionId))
                {
                    SpawnTargetHighlight(army.targetRegionId);
                }
            }

            foreach (EngagementRuntimeState engagement in gameManager.World.Map.EngagementsById.Values)
            {
                SpawnEngagementMarker(engagement);
            }

            foreach (RegionRuntimeState region in gameManager.World.Map.RegionsById.Values)
            {
                if (region == null || region.occupationStatus == OccupationStatus.Controlled) continue;
                SpawnOccupationMarker(region);
            }
        }

        private void SpawnArmyRoute(ArmyRuntimeState army)
        {
            List<Vector3> routePoints = new List<Vector3>();
            for (int i = 0; i < army.route.Count; i++)
            {
                Vector2 point;
                if (TryResolveRegionPosition(army.route[i], out point))
                {
                    routePoints.Add(new Vector3(point.x, point.y, -0.42f));
                }
            }

            if (routePoints.Count < 2) return;

            Color color = ResolveRoutePressureColor(army);
            CreateRouteUnderlay(army.id, routePoints, color);

            GameObject routeObject = new GameObject("WarRoute_" + army.id);
            routeObject.transform.SetParent(transform, false);

            float routeWidth = ResolveRoutePressureWidth(army);
            CreateRouteLine(routeObject, routePoints, routeWidth, 38, color);
            CreateRouteContactNode(army.id, routePoints[routePoints.Count - 1], color);

            CreateLabel(routeObject.transform, "WarRoutePressureLabel_" + army.id, FormatRoutePressureLabel(army), ResolveRouteLabelPosition(routePoints), 47, 0.085f, color);
        }

        private void CreateRouteUnderlay(string armyId, List<Vector3> routePoints, Color color)
        {
            if (routePoints == null || routePoints.Count < 2) return;

            GameObject underlayObject = new GameObject("WarRouteUnderlay_" + armyId);
            underlayObject.transform.SetParent(transform, false);
            Color underlayColor = new Color(0.03f, 0.02f, 0.015f, Mathf.Clamp01(color.a * 0.74f));
            CreateRouteLine(underlayObject, routePoints, 0.22f, 37, underlayColor);
        }

        private void CreateRouteLine(GameObject routeObject, List<Vector3> routePoints, float width, int sortingOrder, Color color)
        {
            if (routeObject == null || routePoints == null || routePoints.Count < 2) return;

            LineRenderer line = routeObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = routePoints.Count;
            line.startWidth = width;
            line.endWidth = width;
            line.numCapVertices = 6;
            line.numCornerVertices = 3;
            line.sortingOrder = sortingOrder;
            line.material = ResolveOverlayMaterial();
            line.startColor = color;
            line.endColor = color;

            for (int i = 0; i < routePoints.Count; i++)
            {
                line.SetPosition(i, routePoints[i]);
            }
        }

        private void CreateRouteContactNode(string armyId, Vector3 routeEnd, Color color)
        {
            GameObject nodeObject = new GameObject("WarRouteContactNode_" + armyId);
            nodeObject.transform.SetParent(transform, false);
            nodeObject.transform.localPosition = routeEnd + new Vector3(0f, 0f, -0.01f);

            LineRenderer ring = nodeObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = 18;
            ring.startWidth = 0.055f;
            ring.endWidth = 0.055f;
            ring.numCapVertices = 4;
            ring.sortingOrder = 40;
            ring.material = ResolveOverlayMaterial();
            ring.startColor = new Color(color.r, color.g, color.b, 0.96f);
            ring.endColor = ring.startColor;

            const float radius = 0.24f;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = Mathf.PI * 2f * i / ring.positionCount;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }

            ConfigurePressurePulse(nodeObject, ring, 0.055f, 0.16f, 0.28f, 5.2f, 0f);
        }

        private void SpawnTargetHighlight(string regionId)
        {
            Vector2 position;
            if (!TryResolveRegionPosition(regionId, out position)) return;

            GameObject highlightObject = new GameObject("WarTargetHighlight_" + regionId);
            highlightObject.transform.SetParent(transform, false);
            highlightObject.transform.localPosition = new Vector3(position.x, position.y, -0.41f);

            LineRenderer line = highlightObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 24;
            line.startWidth = 0.07f;
            line.endWidth = 0.07f;
            line.numCapVertices = 4;
            line.sortingOrder = 39;
            line.material = ResolveOverlayMaterial();
            line.startColor = new Color(1f, 0.86f, 0.2f, 0.95f);
            line.endColor = line.startColor;

            const float radius = 0.42f;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = Mathf.PI * 2f * i / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }

            ConfigurePressurePulse(highlightObject, line, 0.07f, 0.08f, 0.18f, 3.6f, 1.3f);
            CreateLabel(highlightObject.transform, "WarTargetLabel_" + regionId, "目标", new Vector3(0f, 0.5f, -0.02f), 47, 0.075f, new Color(1f, 0.9f, 0.45f, 1f));
        }

        private Color ResolveRoutePressureColor(ArmyRuntimeState army)
        {
            int projectedSupply = CalculateProjectedSupplyAfterRoute(army);
            int powerPercent = StrategyCausalRules.CalculateBattleSupplyPowerPercentForSupply(projectedSupply);
            if (powerPercent <= StrategyCausalRules.DepletedSupplyBattlePowerPercent)
            {
                return new Color(1f, 0.22f, 0.16f, 0.96f);
            }

            if (powerPercent < 100)
            {
                return new Color(1f, 0.66f, 0.18f, 0.96f);
            }

            return army != null && gameManager != null && gameManager.State != null && army.ownerFactionId == gameManager.State.playerFactionId
                ? new Color(0.2f, 0.95f, 0.45f, 0.95f)
                : new Color(1f, 0.45f, 0.25f, 0.9f);
        }

        private static float ResolveRoutePressureWidth(ArmyRuntimeState army)
        {
            int projectedSupply = CalculateProjectedSupplyAfterRoute(army);
            int powerPercent = StrategyCausalRules.CalculateBattleSupplyPowerPercentForSupply(projectedSupply);
            if (powerPercent <= StrategyCausalRules.DepletedSupplyBattlePowerPercent) return 0.16f;
            if (powerPercent < 100) return 0.12f;
            return 0.08f;
        }

        private static string FormatRoutePressureLabel(ArmyRuntimeState army)
        {
            if (army == null) return "补给压力 未知";

            int projectedSupply = CalculateProjectedSupplyAfterRoute(army);
            int powerPercent = StrategyCausalRules.CalculateBattleSupplyPowerPercentForSupply(projectedSupply);
            return "补给压力 " + ResolveRoutePressureGrade(powerPercent) + " " + powerPercent + "% | " + army.supply + "→" + projectedSupply;
        }

        private static string ResolveRoutePressureGrade(int powerPercent)
        {
            if (powerPercent <= StrategyCausalRules.DepletedSupplyBattlePowerPercent) return "高";
            if (powerPercent < 100) return "中";
            return "低";
        }

        private static int CalculateProjectedSupplyAfterRoute(ArmyRuntimeState army)
        {
            if (army == null) return 0;

            int routeSteps = army.route != null ? Mathf.Max(0, army.route.Count - 1) : 0;
            return Mathf.Max(0, army.supply - routeSteps * StrategyCausalRules.WarMovementSupplyCost);
        }

        private static Vector3 ResolveRouteLabelPosition(List<Vector3> routePoints)
        {
            if (routePoints == null || routePoints.Count == 0) return Vector3.zero;
            if (routePoints.Count == 1) return routePoints[0] + new Vector3(0f, 0.34f, -0.02f);

            int index = Mathf.Clamp(routePoints.Count / 2, 1, routePoints.Count - 1);
            Vector3 midpoint = (routePoints[index - 1] + routePoints[index]) * 0.5f;
            return midpoint + new Vector3(0f, 0.34f, -0.02f);
        }

        private void SpawnEngagementMarker(EngagementRuntimeState engagement)
        {
            if (engagement == null || string.IsNullOrEmpty(engagement.regionId)) return;

            Vector2 position;
            if (!TryResolveRegionPosition(engagement.regionId, out position)) return;

            GameObject marker = new GameObject("WarContactMarker_" + engagement.id);
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = new Vector3(position.x, position.y + 0.72f, -0.43f);
            LineRenderer ring = marker.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = 22;
            ring.startWidth = 0.05f;
            ring.endWidth = 0.05f;
            ring.numCapVertices = 4;
            ring.sortingOrder = 48;
            ring.material = ResolveOverlayMaterial();
            ring.startColor = new Color(1f, 0.24f, 0.18f, 0.92f);
            ring.endColor = ring.startColor;

            const float radius = 0.3f;
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = Mathf.PI * 2f * i / ring.positionCount;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }

            ConfigurePressurePulse(marker, ring, 0.05f, 0.18f, 0.34f, 5.8f, 0.7f);
            CreateLabel(marker.transform, "WarContactLabel_" + engagement.id, "接敌 " + engagement.attackerArmyIds.Count + ":" + engagement.defenderArmyIds.Count, Vector3.zero, 49, 0.095f, new Color(1f, 0.36f, 0.25f, 1f));
        }

        private void SpawnOccupationMarker(RegionRuntimeState region)
        {
            Vector2 position;
            if (!TryResolveRegionPosition(region.id, out position)) return;

            GameObject marker = new GameObject("WarOccupationMarker_" + region.id);
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = new Vector3(position.x, position.y - 0.62f, -0.43f);
            CreateLabel(marker.transform, "WarOccupationLabel_" + region.id, FormatOccupationStatus(region.occupationStatus), Vector3.zero, 48, 0.085f, new Color(1f, 0.78f, 0.35f, 1f));
        }

        private bool ShouldShowArmy(ArmyRuntimeState army)
        {
            if (army == null) return false;
            if (showIdleArmies) return true;
            if (army.task != ArmyTask.Idle) return true;
            if (!string.IsNullOrEmpty(army.targetRegionId)) return true;
            if (!string.IsNullOrEmpty(army.engagementId)) return true;
            return IsContestedRegion(army.locationRegionId);
        }

        private bool IsContestedRegion(string regionId)
        {
            if (gameManager == null || gameManager.World == null || gameManager.World.Map == null || string.IsNullOrEmpty(regionId))
            {
                return false;
            }

            EngagementRuntimeState engagement;
            if (gameManager.World.Map.TryGetEngagementInRegion(regionId, out engagement) && engagement != null)
            {
                return true;
            }

            RegionRuntimeState region;
            return gameManager.World.Map.TryGetRegion(regionId, out region) && region != null && region.occupationStatus == OccupationStatus.Contested;
        }

        private bool TryResolveRegionPosition(string regionId, out Vector2 position)
        {
            MapRegionShapeDefinition shape;
            if (gameManager.Data.TryGetMapRegionShapeByRegionId(regionId, out shape) && shape != null && shape.center != null)
            {
                Vector3 projected = ResolveProjectedRegionPoint(shape.center);
                position = new Vector2(projected.x, projected.y);
                return true;
            }

            return fallbackPositions.TryGetValue(regionId, out position);
        }

        private Vector3 ResolveProjectedRegionPoint(MapPoint point)
        {
            Vector3 localShapePoint = new Vector3(point.x, point.y, 0f);
            GameObject regionLayer = GameObject.Find("RegionInteractionLayer");
            if (regionLayer == null)
            {
                return localShapePoint;
            }

            Vector3 worldPoint = regionLayer.transform.TransformPoint(localShapePoint);
            return transform.InverseTransformPoint(worldPoint);
        }

        private GameObject CreateSpriteObject(string objectName, Sprite sprite, Vector3 localPosition, int sortingOrder, float scale)
        {
            GameObject gameObject = new GameObject(objectName);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localScale = Vector3.one * scale;

            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.color = Color.white;
            return gameObject;
        }

        private void CreateLabel(Transform parent, string label, Vector3 localPosition, int sortingOrder, float size)
        {
            CreateLabel(parent, "Label_" + label, label, localPosition, sortingOrder, size, Color.white);
        }

        private void CreateLabel(Transform parent, string objectName, string label, Vector3 localPosition, int sortingOrder, float size)
        {
            CreateLabel(parent, objectName, label, localPosition, sortingOrder, size, Color.white);
        }

        private void CreateLabel(Transform parent, string objectName, string label, Vector3 localPosition, int sortingOrder, float size, Color color)
        {
            GameObject labelObject = new GameObject(objectName);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;

            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = label;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = size;
            textMesh.fontSize = 24;
            textMesh.color = color;

            MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }
        }

        public void ApplyLabelDensityForCurrentZoom()
        {
            LastVisibleLabelCount = 0;
            LastHiddenLabelCount = 0;
            LastHiddenByZoomCount = 0;
            LastHiddenByBudgetCount = 0;
            LastHiddenByOverlapCount = 0;
            LastLabelCandidateCount = 0;

            Camera camera = Camera.main;
            if (camera == null) return;

            MapZoomBand zoomBand = ResolveZoomBand(camera);
            float gapPixels = ResolveLabelGapPixels(zoomBand);
            int visibleBudget = ResolveVisibleLabelBudget(zoomBand);
            TextMesh[] labels = GetComponentsInChildren<TextMesh>(true);
            List<LabelCandidate> candidates = new List<LabelCandidate>();
            if (labels != null)
            {
                for (int i = 0; i < labels.Length; i++)
                {
                    TextMesh label = labels[i];
                    if (label == null) continue;

                    MeshRenderer renderer = label.GetComponent<MeshRenderer>();
                    if (renderer == null) continue;

                    int priority = ResolveLabelPriority(label.gameObject.name);
                    bool allowedByZoom = IsLabelAllowedByZoom(priority, zoomBand);
                    candidates.Add(new LabelCandidate
                    {
                        textMesh = label,
                        renderer = renderer,
                        priority = priority,
                        screenPosition = camera.WorldToScreenPoint(label.transform.position),
                        screenRect = ResolveLabelScreenRect(camera, renderer, gapPixels),
                        allowedByZoom = allowedByZoom
                    });
                }
            }

            LastLabelCandidateCount = candidates.Count;
            candidates.Sort((a, b) =>
            {
                int priority = a.priority.CompareTo(b.priority);
                if (priority != 0) return priority;
                return string.CompareOrdinal(a.textMesh.gameObject.name, b.textMesh.gameObject.name);
            });

            List<Rect> occupied = new List<Rect>();
            for (int i = 0; i < candidates.Count; i++)
            {
                LabelCandidate candidate = candidates[i];
                bool blockedByZoom = !candidate.allowedByZoom;
                bool blockedByBudget = !blockedByZoom && LastVisibleLabelCount >= visibleBudget;
                bool blockedByOverlap = !blockedByZoom && !blockedByBudget && OverlapsExistingLabel(candidate, occupied);
                bool visible = !blockedByZoom && !blockedByBudget && !blockedByOverlap;
                candidate.renderer.enabled = visible;

                if (visible)
                {
                    occupied.Add(candidate.screenRect);
                    LastVisibleLabelCount++;
                }
                else
                {
                    if (blockedByZoom) LastHiddenByZoomCount++;
                    else if (blockedByBudget) LastHiddenByBudgetCount++;
                    else if (blockedByOverlap) LastHiddenByOverlapCount++;
                    LastHiddenLabelCount++;
                }
            }

            ApplyPulseBudgetForCurrentZoom(zoomBand);
            RefreshOverlayObjectStats();
        }

        public void RefreshOverlayObjectStats()
        {
            LastWarOverlayObjectCount = 0;
            LastWarRouteObjectCount = 0;
            LastWarContactObjectCount = 0;
            LastWarOccupationObjectCount = 0;
            LastWarTargetObjectCount = 0;
            LastWarLabelObjectCount = 0;
            CountOverlayStats(transform);
        }

        public string FormatOverlayStats()
        {
            return "overlay=" + LastWarOverlayObjectCount +
                   " routes=" + LastWarRouteObjectCount +
                   " contacts=" + LastWarContactObjectCount +
                   " occupations=" + LastWarOccupationObjectCount +
                   " targets=" + LastWarTargetObjectCount +
                   " labels=" + LastWarLabelObjectCount +
                   " visibleLabels=" + LastVisibleLabelCount +
                   " hiddenLabels=" + LastHiddenLabelCount +
                   " hiddenZoom=" + LastHiddenByZoomCount +
                   " hiddenBudget=" + LastHiddenByBudgetCount +
                   " hiddenOverlap=" + LastHiddenByOverlapCount +
                   " pulses=" + LastActivePulseCount + "/" + (LastActivePulseCount + LastInactivePulseCount) +
                   " pulseBudget=" + LastPulseBudget;
        }

        private void CountOverlayStats(Transform root)
        {
            if (root == null) return;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child == null) continue;

                string objectName = child.gameObject.name;
                bool counted = false;
                if (objectName.StartsWith("WarRoute_") ||
                    objectName.StartsWith("WarRouteUnderlay_") ||
                    objectName.StartsWith("WarRouteContactNode_"))
                {
                    LastWarRouteObjectCount++;
                    counted = true;
                }
                else if (objectName.StartsWith("WarContactMarker_"))
                {
                    LastWarContactObjectCount++;
                    counted = true;
                }
                else if (objectName.StartsWith("WarOccupationMarker_"))
                {
                    LastWarOccupationObjectCount++;
                    counted = true;
                }
                else if (objectName.StartsWith("WarTargetHighlight_"))
                {
                    LastWarTargetObjectCount++;
                    counted = true;
                }

                if (objectName.StartsWith("ArmyInfo_") ||
                    objectName.StartsWith("WarRoutePressureLabel_") ||
                    objectName.StartsWith("WarTargetLabel_") ||
                    objectName.StartsWith("WarContactLabel_") ||
                    objectName.StartsWith("WarOccupationLabel_"))
                {
                    LastWarLabelObjectCount++;
                    counted = true;
                }

                if (counted)
                {
                    LastWarOverlayObjectCount++;
                }

                CountOverlayStats(child);
            }
        }

        private void ApplyPulseBudgetForCurrentZoom(MapZoomBand zoomBand)
        {
            WarPressurePulse[] pulses = GetComponentsInChildren<WarPressurePulse>(true);
            LastActivePulseCount = 0;
            LastInactivePulseCount = 0;
            LastPulseBudget = ResolvePulseBudget(zoomBand);
            if (pulses == null || pulses.Length == 0) return;

            List<WarPressurePulse> ordered = new List<WarPressurePulse>(pulses);
            ordered.Sort((a, b) =>
            {
                int priority = ResolvePulsePriority(a != null ? a.gameObject.name : null).CompareTo(ResolvePulsePriority(b != null ? b.gameObject.name : null));
                if (priority != 0) return priority;
                return string.CompareOrdinal(a != null ? a.gameObject.name : string.Empty, b != null ? b.gameObject.name : string.Empty);
            });

            for (int i = 0; i < ordered.Count; i++)
            {
                WarPressurePulse pulse = ordered[i];
                if (pulse == null) continue;

                bool active = i < LastPulseBudget;
                pulse.SetPulseActive(active);
                if (active) LastActivePulseCount++;
                else LastInactivePulseCount++;
            }
        }

        private static Rect ResolveLabelScreenRect(Camera camera, MeshRenderer renderer, float paddingPixels)
        {
            if (camera == null || renderer == null)
            {
                return Rect.zero;
            }

            Bounds bounds = renderer.bounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, max.y, min.z)
            };

            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 screen = camera.WorldToScreenPoint(corners[i]);
                xMin = Mathf.Min(xMin, screen.x);
                xMax = Mathf.Max(xMax, screen.x);
                yMin = Mathf.Min(yMin, screen.y);
                yMax = Mathf.Max(yMax, screen.y);
            }

            float padding = Mathf.Max(0f, paddingPixels * 0.5f);
            return Rect.MinMaxRect(xMin - padding, yMin - padding, xMax + padding, yMax + padding);
        }

        private static bool OverlapsExistingLabel(LabelCandidate candidate, List<Rect> occupied)
        {
            for (int i = 0; i < occupied.Count; i++)
            {
                if (candidate.screenRect.Overlaps(occupied[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static MapZoomBand ResolveZoomBand(Camera camera)
        {
            CameraController controller = camera != null ? camera.GetComponent<CameraController>() : null;
            if (controller != null) return controller.CurrentZoomBand;

            float zoom = camera != null ? camera.orthographicSize : 6f;
            if (zoom <= 6f) return MapZoomBand.Detail;
            if (zoom <= 10f) return MapZoomBand.Operation;
            return MapZoomBand.Overview;
        }

        private float ResolveLabelGapPixels(MapZoomBand zoomBand)
        {
            float viewportScale = 1f;
            if (Screen.width > 0)
            {
                if (Screen.width <= 1024) viewportScale = 1.34f;
                else if (Screen.width <= 1280) viewportScale = 1.16f;
            }

            switch (zoomBand)
            {
                case MapZoomBand.Detail: return detailLabelGapPixels * viewportScale;
                case MapZoomBand.Overview: return overviewLabelGapPixels * viewportScale;
                default: return operationLabelGapPixels * viewportScale;
            }
        }

        private static int ResolveVisibleLabelBudget(MapZoomBand zoomBand)
        {
            bool smallViewport = Screen.width > 0 && Screen.width <= 1024;
            switch (zoomBand)
            {
                case MapZoomBand.Detail:
                    return smallViewport ? 18 : 24;
                case MapZoomBand.Operation:
                    return smallViewport ? 10 : 15;
                case MapZoomBand.Overview:
                    return smallViewport ? 5 : 7;
                default:
                    return smallViewport ? 10 : 15;
            }
        }

        private static int ResolvePulseBudget(MapZoomBand zoomBand)
        {
            bool tinyViewport = Screen.width > 0 && Screen.width <= 1024;
            bool smallViewport = Screen.width > 0 && Screen.width <= 1280;
            switch (zoomBand)
            {
                case MapZoomBand.Detail:
                    return tinyViewport ? 6 : (smallViewport ? 8 : 12);
                case MapZoomBand.Operation:
                    return tinyViewport ? 5 : (smallViewport ? 6 : 8);
                case MapZoomBand.Overview:
                    return tinyViewport ? 3 : (smallViewport ? 4 : 5);
                default:
                    return tinyViewport ? 5 : (smallViewport ? 6 : 8);
            }
        }

        private static bool IsLabelAllowedByZoom(int priority, MapZoomBand zoomBand)
        {
            switch (zoomBand)
            {
                case MapZoomBand.Detail:
                    return true;
                case MapZoomBand.Operation:
                    return priority <= 3;
                case MapZoomBand.Overview:
                    return priority <= 1;
                default:
                    return true;
            }
        }

        private static int ResolveLabelPriority(string objectName)
        {
            if (objectName.Contains("WarContactLabel")) return 0;
            if (objectName.Contains("WarTargetLabel")) return 0;
            if (objectName.Contains("WarRoutePressureLabel")) return 1;
            if (objectName.Contains("ArmyInfo")) return 2;
            if (objectName.Contains("WarOccupationLabel")) return 3;
            return 4;
        }

        private static int ResolvePulsePriority(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return 5;
            if (objectName.Contains("WarContactMarker")) return 0;
            if (objectName.Contains("WarRouteContactNode")) return 1;
            if (objectName.Contains("WarTargetHighlight")) return 2;
            return 4;
        }

        private Sprite LoadSprite(string assetRelativePath, float pixelsPerUnit)
        {
            Sprite cached;
            if (spriteCache.TryGetValue(assetRelativePath, out cached)) return cached;

            string resourcePath = Path.ChangeExtension(assetRelativePath, null).Replace('\\', '/');
            Sprite resourceSprite = Resources.Load<Sprite>(resourcePath);
            if (resourceSprite != null)
            {
                spriteCache[assetRelativePath] = resourceSprite;
                return resourceSprite;
            }

            Texture2D resourceTexture = Resources.Load<Texture2D>(resourcePath);
            if (resourceTexture != null)
            {
                Sprite spriteFromResource = CreateSprite(resourceTexture, resourceTexture.name, pixelsPerUnit);
                spriteCache[assetRelativePath] = spriteFromResource;
                return spriteFromResource;
            }

            string path = Path.Combine(Application.dataPath, assetRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) return null;

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.name = Path.GetFileNameWithoutExtension(path);
            if (!texture.LoadImage(bytes)) return null;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Sprite sprite = CreateSprite(texture, texture.name, pixelsPerUnit);
            spriteCache[assetRelativePath] = sprite;
            return sprite;
        }

        private static Sprite CreateSprite(Texture2D texture, string spriteName, float pixelsPerUnit)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.name = spriteName;
            return sprite;
        }

        private string ResolveEmperorLabel(FactionState faction)
        {
            EmperorDefinition emperor;
            if (gameManager.Data.Emperors.TryGetValue(faction.emperorId, out emperor) && emperor != null)
            {
                return emperor.name;
            }

            return faction.name;
        }

        private string ResolveArmyInfoLabel(ArmyRuntimeState army)
        {
            if (army == null) return "主将：未知 | 兵力：0";

            return "主将：" + ResolveGeneralLabel(army) + " | 兵力：" + army.soldiers + " | " + FormatArmyTask(army);
        }

        private string ResolveGeneralLabel(ArmyRuntimeState army)
        {
            FactionState owner = gameManager.State != null ? gameManager.State.FindFaction(army.ownerFactionId) : null;
            if (owner != null && owner.talentIds != null)
            {
                for (int i = 0; i < owner.talentIds.Count; i++)
                {
                    GeneralDefinition general;
                    if (gameManager.Data.Generals.TryGetValue(owner.talentIds[i], out general) && general != null)
                    {
                        return general.name;
                    }
                }
            }

            if (owner != null)
            {
                EmperorDefinition emperor;
                if (gameManager.Data.Emperors.TryGetValue(owner.emperorId, out emperor) && emperor != null)
                {
                    return emperor.name + "亲征";
                }

                return owner.name;
            }

            return "未任命";
        }

        private static string FormatArmyTask(ArmyRuntimeState army)
        {
            if (army == null) return "待命";
            if (!string.IsNullOrEmpty(army.engagementId)) return "接敌";
            if (army.task == ArmyTask.Idle) return "待命";
            if (!string.IsNullOrEmpty(army.targetRegionId)) return army.task + "→" + army.targetRegionId;
            return army.task.ToString();
        }

        private static string FormatOccupationStatus(OccupationStatus status)
        {
            switch (status)
            {
                case OccupationStatus.Contested: return "争夺";
                case OccupationStatus.Occupied: return "占领";
                case OccupationStatus.Rebelling: return "民变";
                default: return status.ToString();
            }
        }

        private Material ResolveOverlayMaterial()
        {
            if (overlayMaterial != null) return overlayMaterial;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            }

            overlayMaterial = shader != null ? new Material(shader) : null;
            return overlayMaterial;
        }

        private static void ConfigurePressurePulse(GameObject target, LineRenderer line, float baseWidth, float scaleAmount, float alphaAmount, float speed, float phase)
        {
            if (target == null || line == null) return;

            WarPressurePulse pulse = target.AddComponent<WarPressurePulse>();
            pulse.Configure(line, baseWidth, target.transform.localScale, scaleAmount, alphaAmount, speed, phase);
        }

        private void TintArmy(GameObject icon, string ownerFactionId)
        {
            SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();
            if (renderer == null || gameManager.State == null) return;

            renderer.color = ownerFactionId == gameManager.State.playerFactionId
                ? new Color(0.9f, 1f, 0.9f, 1f)
                : new Color(1f, 0.82f, 0.72f, 1f);
        }

        private void ResolveGameManager()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    forceRefresh = true;
                }
            }
        }

        private bool NeedsRefresh()
        {
            string visualSignature = BuildVisualSignature();
            string season = gameManager.State.season.ToString();
            return forceRefresh || lastVisualSignature != visualSignature || lastTurn != gameManager.State.turn || lastSeason != season;
        }

        private void CaptureRefreshState()
        {
            lastVisualSignature = BuildVisualSignature();
            lastTurn = gameManager.State.turn;
            lastSeason = gameManager.State.season.ToString();
        }

        private string BuildVisualSignature()
        {
            if (gameManager == null || gameManager.World == null || gameManager.World.Map == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(showFactionPortraitStrip ? "portraits:1|" : "portraits:0|");
            builder.Append(showIdleArmies ? "idle:1|" : "idle:0|");
            builder.Append(showWarOperationalOverlays ? "warOverlay:1|" : "warOverlay:0|");

            foreach (ArmyRuntimeState army in gameManager.World.Map.ArmiesById.Values)
            {
                if (!ShouldShowArmy(army)) continue;

                builder.Append(army.id).Append(':')
                    .Append(army.ownerFactionId).Append(':')
                    .Append(army.locationRegionId).Append(':')
                    .Append(army.targetRegionId).Append(':')
                    .Append(army.task).Append(':')
                    .Append(army.engagementId).Append(':')
                    .Append(army.unitId).Append(':')
                    .Append(army.soldiers).Append(':')
                    .Append(army.supply).Append(':')
                    .Append(army.route != null ? string.Join(">", army.route.ToArray()) : string.Empty).Append('|');
            }

            foreach (EngagementRuntimeState engagement in gameManager.World.Map.EngagementsById.Values)
            {
                if (engagement == null) continue;
                builder.Append("engagement:").Append(engagement.id).Append(':')
                    .Append(engagement.regionId).Append(':')
                    .Append(engagement.attackerArmyIds.Count).Append(':')
                    .Append(engagement.defenderArmyIds.Count).Append(':')
                    .Append(engagement.phase).Append('|');
            }

            foreach (RegionRuntimeState region in gameManager.World.Map.RegionsById.Values)
            {
                if (region == null || region.occupationStatus == OccupationStatus.Controlled) continue;
                builder.Append("occupation:").Append(region.id).Append(':').Append(region.occupationStatus).Append('|');
            }

            return builder.ToString();
        }

        private void SubscribeEvents()
        {
            if (subscribed || gameManager == null || gameManager.Events == null) return;

            gameManager.Events.Subscribe(GameEventType.GameStarted, OnEntityVisualStateChanged);
            gameManager.Events.Subscribe(GameEventType.ArmyMoveStarted, OnEntityVisualStateChanged);
            gameManager.Events.Subscribe(GameEventType.ArmyArrived, OnEntityVisualStateChanged);
            gameManager.Events.Subscribe(GameEventType.ContactDetected, OnEntityVisualStateChanged);
            gameManager.Events.Subscribe(GameEventType.EngagementStarted, OnEntityVisualStateChanged);
            gameManager.Events.Subscribe(GameEventType.BattleResolved, OnEntityVisualStateChanged);
            gameManager.Events.Subscribe(GameEventType.RegionOccupied, OnEntityVisualStateChanged);
            subscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!subscribed || gameManager == null || gameManager.Events == null) return;

            gameManager.Events.Unsubscribe(GameEventType.GameStarted, OnEntityVisualStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.ArmyMoveStarted, OnEntityVisualStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.ArmyArrived, OnEntityVisualStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.ContactDetected, OnEntityVisualStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.EngagementStarted, OnEntityVisualStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.BattleResolved, OnEntityVisualStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.RegionOccupied, OnEntityVisualStateChanged);
            subscribed = false;
        }

        private void OnEntityVisualStateChanged(GameEvent gameEvent)
        {
            forceRefresh = true;
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private void BuildFallbackPositions()
        {
            fallbackPositions.Clear();
            fallbackPositions["guanzhong"] = new Vector2(-5f, 2.5f);
            fallbackPositions["longxi"] = new Vector2(-10f, 5f);
            fallbackPositions["hedong"] = new Vector2(0f, 5f);
            fallbackPositions["hanzhong"] = new Vector2(-7.5f, 0f);
            fallbackPositions["bashu"] = new Vector2(-10f, -2.5f);
            fallbackPositions["jingxiang"] = new Vector2(-2.5f, -2.5f);
            fallbackPositions["zhongyuan"] = new Vector2(2.5f, 2.5f);
            fallbackPositions["qilu"] = new Vector2(7.5f, 5f);
            fallbackPositions["youyan"] = new Vector2(5f, 10f);
            fallbackPositions["bingzhou"] = new Vector2(-2.5f, 8.75f);
            fallbackPositions["liangzhou"] = new Vector2(-12.5f, 7.5f);
            fallbackPositions["huainan"] = new Vector2(5f, 0f);
            fallbackPositions["jiangdong"] = new Vector2(7.5f, -2.5f);
            fallbackPositions["minyue"] = new Vector2(10f, -5f);
            fallbackPositions["lingnan"] = new Vector2(2.5f, -7.5f);
            fallbackPositions["yun_gui"] = new Vector2(-5f, -7.5f);
            fallbackPositions["liaodong"] = new Vector2(10f, 10f);
        }

        private sealed class LabelCandidate
        {
            public TextMesh textMesh;
            public MeshRenderer renderer;
            public int priority;
            public Vector2 screenPosition;
            public Rect screenRect;
            public bool allowedByZoom;
        }
    }
}
