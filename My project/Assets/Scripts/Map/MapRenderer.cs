using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class MapRenderer : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private float ownerColorAlpha = 0.72f;
        [SerializeField] private Color fallbackOwnerColor = new Color(0.72f, 0.72f, 0.72f, 0.72f);

        private readonly Dictionary<string, RegionController> controllers = new Dictionary<string, RegionController>();
        private MapLensMode currentLens = MapLensMode.Governance;
        private bool hideMeshSurfacesInGovernanceLens;
        private bool subscribed;

        private static readonly Dictionary<string, Color> FactionColors = new Dictionary<string, Color>
        {
            { "faction_qin_shi_huang", new Color(0.08f, 0.07f, 0.06f, 1f) },
            { "faction_liu_bang", new Color(0.62f, 0.22f, 0.12f, 1f) },
            { "faction_han_wu_di", new Color(0.78f, 0.08f, 0.08f, 1f) },
            { "faction_cao_cao", new Color(0.30f, 0.30f, 0.34f, 1f) },
            { "faction_li_shi_min", new Color(0.95f, 0.66f, 0.18f, 1f) },
            { "faction_zhao_kuang_yin", new Color(0.55f, 0.17f, 0.15f, 1f) },
            { "faction_zhu_yuan_zhang", new Color(0.70f, 0.05f, 0.04f, 1f) },
            { "faction_kang_xi", new Color(0.95f, 0.78f, 0.22f, 1f) }
        };

        public void Bind(GameManager manager)
        {
            UnsubscribeEvents();
            gameManager = manager;
            SubscribeEvents();
            Refresh();
        }

        private void Start()
        {
            ResolveGameManager();
            SubscribeEvents();
            Refresh();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void RegisterRegion(RegionController controller)
        {
            if (controller == null || string.IsNullOrEmpty(controller.RegionId)) return;
            controllers[controller.RegionId] = controller;
        }

        public void ClearRegions()
        {
            controllers.Clear();
        }

        public void Refresh()
        {
            if (gameManager == null || gameManager.State == null) return;

            foreach (RegionState region in gameManager.State.regions)
            {
                RegionController controller;
                if (!controllers.TryGetValue(region.id, out controller)) continue;
                if (controller == null) continue;
                ApplyRegionColor(controller, region);
            }
        }

        public void SetLens(MapLensMode lens)
        {
            currentLens = lens;
            Refresh();
        }

        public void SetHideMeshSurfacesInGovernanceLens(bool hide)
        {
            hideMeshSurfacesInGovernanceLens = hide;
            Refresh();
        }

        public void RefreshRegion(string regionId)
        {
            if (gameManager == null || gameManager.State == null) return;

            RegionState region = gameManager.State.FindRegion(regionId);
            if (region == null) return;

            RegionController controller;
            if (!controllers.TryGetValue(region.id, out controller)) return;
            if (controller == null) return;
            ApplyRegionColor(controller, region);
        }

        private void ApplyRegionColor(RegionController controller, RegionState region)
        {
            if (region == null)
            {
                ApplyOwnerColor(controller, null);
                return;
            }

            Color color = currentLens == MapLensMode.Governance ? ResolveOwnerColor(region.ownerFactionId) : ResolveLensColor(region);
            color.a = currentLens == MapLensMode.Governance ? ownerColorAlpha : 0.48f;
            ApplyColor(controller, color, currentLens != MapLensMode.Governance);
        }

        private void ApplyOwnerColor(RegionController controller, string ownerFactionId)
        {
            if (controller == null) return;

            Color color = ResolveOwnerColor(ownerFactionId);
            color.a = ownerColorAlpha;
            ApplyColor(controller, color, false);
        }

        private void ApplyColor(RegionController controller, Color color, bool forceVisible)
        {
            if (controller == null) return;
            SpriteRenderer spriteRenderer = controller.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
                return;
            }

            MeshRenderer meshRenderer = controller.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.enabled = forceVisible || !hideMeshSurfacesInGovernanceLens;
                meshRenderer.material.color = color;
            }
        }

        private Color ResolveLensColor(RegionState region)
        {
            RegionDefinition definition = gameManager != null && gameManager.Data != null ? gameManager.Data.GetRegion(region.id) : null;
            int score = StrategyMapRulebook.CalculateLensScore(currentLens, definition, region);
            float t = Mathf.Clamp01(score / 100f);
            switch (currentLens)
            {
                case MapLensMode.Risk:
                    return Color.Lerp(new Color(0.20f, 0.55f, 0.28f, 1f), new Color(0.85f, 0.12f, 0.08f, 1f), t);
                case MapLensMode.Economy:
                    return Color.Lerp(new Color(0.15f, 0.32f, 0.62f, 1f), new Color(0.90f, 0.72f, 0.18f, 1f), t);
                case MapLensMode.Legitimacy:
                    return Color.Lerp(new Color(0.52f, 0.18f, 0.18f, 1f), new Color(0.20f, 0.58f, 0.72f, 1f), t);
                case MapLensMode.War:
                    return region.supplyNode ? new Color(0.95f, 0.65f, 0.18f, 1f) : new Color(0.45f, 0.18f, 0.12f, 1f);
                case MapLensMode.Terrain:
                    return Color.Lerp(new Color(0.18f, 0.43f, 0.24f, 1f), new Color(0.52f, 0.48f, 0.32f, 1f), t);
                default:
                    return Color.Lerp(new Color(0.25f, 0.40f, 0.58f, 1f), new Color(0.88f, 0.55f, 0.18f, 1f), t);
            }
        }

        private Color ResolveOwnerColor(string ownerFactionId)
        {
            if (string.IsNullOrEmpty(ownerFactionId)) return fallbackOwnerColor;

            Color color;
            if (FactionColors.TryGetValue(ownerFactionId, out color))
            {
                return color;
            }

            int hash = DeterministicHash(ownerFactionId);
            float hue = Mathf.Abs(hash % 360) / 360f;
            return Color.HSVToRGB(hue, 0.45f, 0.85f);
        }

        private void ResolveGameManager()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        private void SubscribeEvents()
        {
            if (subscribed || gameManager == null || gameManager.Events == null) return;

            gameManager.Events.Subscribe(GameEventType.GameStarted, OnMapStateChanged);
            gameManager.Events.Subscribe(GameEventType.RegionOwnerChanged, OnRegionOwnerChanged);
            gameManager.Events.Subscribe(GameEventType.RegionOccupied, OnRegionStateChanged);
            gameManager.Events.Subscribe(GameEventType.GovernanceImpactApplied, OnRegionStateChanged);
            gameManager.Events.Subscribe(GameEventType.TurnEnded, OnMapStateChanged);
            gameManager.Events.Subscribe(GameEventType.PolicyApplied, OnMapStateChanged);
            gameManager.Events.Subscribe(GameEventType.WeatherChanged, OnMapStateChanged);
            gameManager.Events.Subscribe(GameEventType.TechResearched, OnMapStateChanged);
            gameManager.Events.Subscribe(GameEventType.EspionageOperationCompleted, OnMapStateChanged);
            subscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!subscribed || gameManager == null || gameManager.Events == null) return;

            gameManager.Events.Unsubscribe(GameEventType.GameStarted, OnMapStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.RegionOwnerChanged, OnRegionOwnerChanged);
            gameManager.Events.Unsubscribe(GameEventType.RegionOccupied, OnRegionStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.GovernanceImpactApplied, OnRegionStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.TurnEnded, OnMapStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.PolicyApplied, OnMapStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.WeatherChanged, OnMapStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.TechResearched, OnMapStateChanged);
            gameManager.Events.Unsubscribe(GameEventType.EspionageOperationCompleted, OnMapStateChanged);
            subscribed = false;
        }

        private void OnMapStateChanged(GameEvent gameEvent)
        {
            Refresh();
        }

        private void OnRegionOwnerChanged(GameEvent gameEvent)
        {
            RegionOwnerChangedPayload payload = gameEvent.Payload as RegionOwnerChangedPayload;
            if (payload == null)
            {
                Refresh();
                return;
            }

            RefreshRegion(payload.regionId);
        }

        private void OnRegionStateChanged(GameEvent gameEvent)
        {
            GovernanceImpactPayload governancePayload = gameEvent.Payload as GovernanceImpactPayload;
            if (governancePayload != null)
            {
                RefreshRegion(governancePayload.regionId);
                return;
            }

            RegionOccupiedPayload occupiedPayload = gameEvent.Payload as RegionOccupiedPayload;
            if (occupiedPayload != null)
            {
                RefreshRegion(occupiedPayload.regionId);
                return;
            }

            if (gameEvent != null && !string.IsNullOrEmpty(gameEvent.EntityId))
            {
                RefreshRegion(gameEvent.EntityId);
                return;
            }

            Refresh();
        }

        private static int DeterministicHash(string text)
        {
            int hash = 5381;
            for (int i = 0; i < text.Length; i++)
            {
                hash = ((hash << 5) + hash) + text[i];
            }

            return hash;
        }
    }
}
