using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class AudioEventBridge : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AudioManifestBinder manifestBinder;
        [SerializeField] private ChronicleMusicDispatcher musicDispatcher;
        [SerializeField] private NarrationDispatcher narrationDispatcher;

        private EventBus events;
        private bool bound;

        public void Bind(EventBus eventBus, AudioManager manager)
        {
            if (bound)
            {
                Unbind();
            }

            events = eventBus;
            audioManager = manager;
            if (manifestBinder == null)
            {
                manifestBinder = GetComponent<AudioManifestBinder>();
            }

            if (manifestBinder == null && audioManager != null)
            {
                manifestBinder = audioManager.GetComponent<AudioManifestBinder>();
            }

            if (manifestBinder != null && audioManager != null)
            {
                manifestBinder.Bind(audioManager);
            }

            if (musicDispatcher == null)
            {
                musicDispatcher = GetComponent<ChronicleMusicDispatcher>();
            }

            if (narrationDispatcher == null)
            {
                narrationDispatcher = GetComponent<NarrationDispatcher>();
            }

            if (events == null || audioManager == null) return;

            events.Subscribe(GameEventType.RegionSelected, OnRegionSelected);
            events.Subscribe(GameEventType.PolicyApplied, OnPolicyApplied);
            events.Subscribe(GameEventType.GovernanceImpactApplied, OnGovernanceImpactApplied);
            events.Subscribe(GameEventType.ArmyMoveStarted, OnArmyMoveStarted);
            events.Subscribe(GameEventType.ContactDetected, OnContactDetected);
            events.Subscribe(GameEventType.EngagementStarted, OnContactDetected);
            events.Subscribe(GameEventType.BattleResolved, OnBattleResolved);
            events.Subscribe(GameEventType.RegionOccupied, OnRegionOccupied);
            events.Subscribe(GameEventType.EventTriggered, OnEventTriggered);
            events.Subscribe(GameEventType.FrontlinePrepared, OnFrontlinePrepared);
            events.Subscribe(GameEventType.FrontlineLogisticsAdvanced, OnFrontlineLogisticsAdvanced);
            events.Subscribe(GameEventType.FrontlineLogisticsCommanded, OnFrontlineLogisticsCommanded);
            events.Subscribe(GameEventType.FrontlineLogisticsRaided, OnFrontlineLogisticsRaided);
            events.Subscribe(GameEventType.OccupationPacificationQueueAdvanced, OnOccupationPacificationAdvanced);
            events.Subscribe(GameEventType.DiplomacyWarDeclared, OnWarDeclared);
            events.Subscribe(GameEventType.TurnStarted, OnTurnStarted);

            // Chronicle music integration
            events.Subscribe(GameEventType.ChronicleEventTriggered, OnChronicleEventTriggered);
            events.Subscribe(GameEventType.EmperorSelected, OnEmperorSelected);
            events.Subscribe(GameEventType.SceneMusicRequested, OnSceneMusicRequested);

            // Narration integration
            events.Subscribe(GameEventType.GameStarted, OnGameStarted);
            events.Subscribe(GameEventType.VictoryChecked, OnVictoryChecked);
            bound = true;
        }

        public void Bind(GameManager gameManager)
        {
            if (gameManager == null) return;
            AudioManager manager = audioManager != null ? audioManager : FindObjectOfType<AudioManager>();
            Bind(gameManager.Events, manager);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (!bound || events == null) return;

            events.Unsubscribe(GameEventType.RegionSelected, OnRegionSelected);
            events.Unsubscribe(GameEventType.PolicyApplied, OnPolicyApplied);
            events.Unsubscribe(GameEventType.GovernanceImpactApplied, OnGovernanceImpactApplied);
            events.Unsubscribe(GameEventType.ArmyMoveStarted, OnArmyMoveStarted);
            events.Unsubscribe(GameEventType.ContactDetected, OnContactDetected);
            events.Unsubscribe(GameEventType.EngagementStarted, OnContactDetected);
            events.Unsubscribe(GameEventType.BattleResolved, OnBattleResolved);
            events.Unsubscribe(GameEventType.RegionOccupied, OnRegionOccupied);
            events.Unsubscribe(GameEventType.EventTriggered, OnEventTriggered);
            events.Unsubscribe(GameEventType.FrontlinePrepared, OnFrontlinePrepared);
            events.Unsubscribe(GameEventType.FrontlineLogisticsAdvanced, OnFrontlineLogisticsAdvanced);
            events.Unsubscribe(GameEventType.FrontlineLogisticsCommanded, OnFrontlineLogisticsCommanded);
            events.Unsubscribe(GameEventType.FrontlineLogisticsRaided, OnFrontlineLogisticsRaided);
            events.Unsubscribe(GameEventType.OccupationPacificationQueueAdvanced, OnOccupationPacificationAdvanced);
            events.Unsubscribe(GameEventType.DiplomacyWarDeclared, OnWarDeclared);
            events.Unsubscribe(GameEventType.TurnStarted, OnTurnStarted);
            events.Unsubscribe(GameEventType.ChronicleEventTriggered, OnChronicleEventTriggered);
            events.Unsubscribe(GameEventType.EmperorSelected, OnEmperorSelected);
            events.Unsubscribe(GameEventType.SceneMusicRequested, OnSceneMusicRequested);
            events.Unsubscribe(GameEventType.GameStarted, OnGameStarted);
            events.Unsubscribe(GameEventType.VictoryChecked, OnVictoryChecked);
            bound = false;
            events = null;
        }

        private void OnRegionSelected(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            audioManager.PlayCue("ui_region_select", gameEvent.EntityId);
            audioManager.SetDynamicMusicState("Governance", gameEvent.EntityId, 0.2f);
        }

        private void OnPolicyApplied(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            audioManager.PlayCue("policy_apply", gameEvent.EntityId);
        }

        private void OnGovernanceImpactApplied(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            audioManager.PlayCue("governance_apply", gameEvent.EntityId);
        }

        private void OnArmyMoveStarted(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            audioManager.PlaySpatialCue("war_route", ResolveEventWorldPosition(gameEvent), ResolveRegionId(gameEvent));
            audioManager.SetDynamicMusicState("War", ResolveRegionId(gameEvent), 0.55f);
        }

        private void OnContactDetected(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            audioManager.PlaySpatialCue("war_contact", ResolveEventWorldPosition(gameEvent), ResolveRegionId(gameEvent));
            audioManager.SetDynamicMusicState("War", ResolveRegionId(gameEvent), 0.9f);
        }

        private void OnBattleResolved(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            audioManager.PlayCue("battle_report", ResolveRegionId(gameEvent));
            audioManager.SetDynamicMusicState("War", ResolveRegionId(gameEvent), 0.8f);
        }

        private void OnRegionOccupied(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            audioManager.PlaySpatialCue("war_occupied", ResolveEventWorldPosition(gameEvent), ResolveRegionId(gameEvent));
            audioManager.SetDynamicMusicState("War", ResolveRegionId(gameEvent), 0.75f);
        }

        private void OnEventTriggered(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            ChronicleEventPayload chronicle = gameEvent.Payload as ChronicleEventPayload;
            if (chronicle != null && TryPlayChronicleCue(gameEvent, chronicle))
            {
                return;
            }

            audioManager.PlayCue("event_popup", gameEvent.EntityId);
        }

        private void OnFrontlinePrepared(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            string regionId = ResolveRegionId(gameEvent);
            audioManager.PlaySpatialCue("logistics_prepare", ResolveEventWorldPosition(gameEvent), regionId);
            audioManager.SetDynamicMusicState("War", regionId, 0.58f);
        }

        private void OnFrontlineLogisticsAdvanced(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            string regionId = ResolveRegionId(gameEvent);
            FrontlineLogisticsPayload payload = gameEvent.Payload as FrontlineLogisticsPayload;
            float pressure = payload != null && payload.foodLost > 0 ? 0.76f : 0.62f;
            audioManager.PlaySpatialCue("logistics_convoy", ResolveEventWorldPosition(gameEvent), regionId);
            audioManager.SetDynamicMusicState("War", regionId, pressure);
        }

        private void OnFrontlineLogisticsCommanded(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            string regionId = ResolveRegionId(gameEvent);
            audioManager.PlayCue("logistics_command", regionId);
            audioManager.SetDynamicMusicState("War", regionId, 0.45f);
        }

        private void OnFrontlineLogisticsRaided(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            string regionId = ResolveRegionId(gameEvent);
            FrontlineLogisticsRaidPayload payload = gameEvent.Payload as FrontlineLogisticsRaidPayload;
            float pressure = payload != null ? Mathf.Clamp01(payload.raidPressure / 100f) : 0.88f;
            audioManager.PlaySpatialCue("logistics_raided", ResolveEventWorldPosition(gameEvent), regionId);
            audioManager.SetDynamicMusicState("War", regionId, Mathf.Max(0.82f, pressure));
        }

        private void OnOccupationPacificationAdvanced(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            string regionId = ResolveRegionId(gameEvent);
            audioManager.PlayCue("occupation_pacification", regionId);
            audioManager.SetDynamicMusicState("Governance", regionId, 0.3f);
        }

        private void OnWarDeclared(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            audioManager.PlayCue("war_declared", gameEvent.EntityId);
            audioManager.SetDynamicMusicState("War", gameEvent.EntityId, 0.95f);
        }

        private void OnTurnStarted(GameEvent gameEvent)
        {
            if (audioManager == null) return;
            audioManager.PlayCue("turn_tick", gameEvent != null ? gameEvent.EntityId : null);
            audioManager.SetDynamicMusicState("Governance", null, 0.15f);
        }

        private static string ResolveRegionId(GameEvent gameEvent)
        {
            if (gameEvent == null) return null;
            RegionOccupiedPayload occupied = gameEvent.Payload as RegionOccupiedPayload;
            if (occupied != null) return occupied.regionId;
            GovernanceImpactPayload governance = gameEvent.Payload as GovernanceImpactPayload;
            if (governance != null) return governance.regionId;
            MapArmyMovementPayload movement = gameEvent.Payload as MapArmyMovementPayload;
            if (movement != null) return !string.IsNullOrEmpty(movement.toRegionId) ? movement.toRegionId : movement.fromRegionId;
            EngagementPayload engagement = gameEvent.Payload as EngagementPayload;
            if (engagement != null) return engagement.regionId;
            FrontlinePreparationPayload frontline = gameEvent.Payload as FrontlinePreparationPayload;
            if (frontline != null) return frontline.targetRegionId;
            FrontlineLogisticsPayload logistics = gameEvent.Payload as FrontlineLogisticsPayload;
            if (logistics != null) return logistics.targetRegionId;
            FrontlineLogisticsCommandPayload command = gameEvent.Payload as FrontlineLogisticsCommandPayload;
            if (command != null) return command.targetRegionId;
            FrontlineLogisticsRaidPayload raid = gameEvent.Payload as FrontlineLogisticsRaidPayload;
            if (raid != null) return raid.targetRegionId;
            OccupationPacificationQueuePayload pacification = gameEvent.Payload as OccupationPacificationQueuePayload;
            if (pacification != null) return pacification.regionId;
            return gameEvent.EntityId;
        }

        private static Vector3 ResolveEventWorldPosition(GameEvent gameEvent)
        {
            string regionId = ResolveRegionId(gameEvent);
            if (string.IsNullOrEmpty(regionId)) return Vector3.zero;
            Vector3 realMapPosition;
            if (TryResolveRegionWorldAnchor(regionId, out realMapPosition))
            {
                return realMapPosition;
            }

            int hash = regionId.GetHashCode();
            float x = ((hash & 255) / 255f - 0.5f) * 6f;
            float y = (((hash >> 8) & 255) / 255f - 0.5f) * 4f;
            return new Vector3(x, y, -1f);
        }

        private static bool TryResolveRegionWorldAnchor(string regionId, out Vector3 position)
        {
            position = Vector3.zero;
            RegionController[] controllers = FindObjectsOfType<RegionController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                RegionController controller = controllers[i];
                if (controller == null || controller.RegionId != regionId) continue;

                MeshRenderer meshRenderer = controller.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    position = meshRenderer.bounds.center;
                    return true;
                }

                Collider collider = controller.GetComponent<Collider>();
                if (collider != null)
                {
                    position = collider.bounds.center;
                    return true;
                }

                position = controller.transform.position;
                return true;
            }

            return false;
        }

        private void OnChronicleEventTriggered(GameEvent gameEvent)
        {
            ChronicleEventPayload payload = gameEvent?.Payload as ChronicleEventPayload;
            if (payload == null) return;

            if (TryPlayChronicleCue(gameEvent, payload))
            {
                return;
            }

            string musicCueId = ResolveChronicleMusicCue(payload.eventId);
            if (!string.IsNullOrEmpty(musicCueId) && musicDispatcher != null)
            {
                musicDispatcher.PlayChronicleEvent(musicCueId);
            }
        }

        private void OnEmperorSelected(GameEvent gameEvent)
        {
            EmperorSelectedPayload payload = gameEvent?.Payload as EmperorSelectedPayload;
            if (payload == null) return;

            string musicCueId = manifestBinder != null ? manifestBinder.GetEmperorMusicCueId(payload.emperorId) : null;
            if (string.IsNullOrEmpty(musicCueId))
            {
                musicCueId = "emperor_" + payload.emperorId;
            }

            if (audioManager != null && audioManager.HasCue(musicCueId))
            {
                audioManager.PlayCue(musicCueId, payload.emperorId);
                return;
            }

            if (musicDispatcher != null)
            {
                musicDispatcher.PlayEmperorTheme(musicCueId);
            }
        }

        private void OnSceneMusicRequested(GameEvent gameEvent)
        {
            SceneMusicPayload payload = gameEvent?.Payload as SceneMusicPayload;
            if (payload == null) return;

            if (payload.play)
            {
                string sceneName = manifestBinder != null ? manifestBinder.GetSceneNameForCueId(payload.sceneMusicCueId) : null;
                string sceneKey = !string.IsNullOrEmpty(sceneName) ? sceneName : payload.sceneMusicCueId;
                if (audioManager != null)
                {
                    string snapshotName = ResolveSceneSnapshot(sceneKey);
                    audioManager.SetDynamicMusicState(snapshotName, null, snapshotName == "War" ? 0.85f : 0.35f);
                    audioManager.PlaySceneMusic(sceneKey);
                    return;
                }

                if (musicDispatcher != null)
                {
                    musicDispatcher.PlayScene(payload.sceneMusicCueId);
                }
            }
            else if (musicDispatcher != null)
            {
                musicDispatcher.StopScene();
            }
        }

        private bool TryPlayChronicleCue(GameEvent gameEvent, ChronicleEventPayload payload)
        {
            if (audioManager == null || payload == null) return false;

            string musicCueId = !string.IsNullOrEmpty(payload.musicCueId) ? payload.musicCueId : null;
            if (string.IsNullOrEmpty(musicCueId) && manifestBinder != null)
            {
                musicCueId = manifestBinder.GetChronicleMusicCueId(payload.eventId);
            }

            if (string.IsNullOrEmpty(musicCueId))
            {
                musicCueId = ResolveChronicleMusicCue(payload.eventId);
            }

            if (string.IsNullOrEmpty(musicCueId) || !audioManager.HasCue(musicCueId)) return false;

            string regionId = ResolveRegionId(gameEvent);
            audioManager.PlayCue(musicCueId, regionId);
            audioManager.SetDynamicMusicState(IsWarCategory(payload.category) ? "War" : "Event", regionId, IsWarCategory(payload.category) ? 0.78f : 0.45f);
            return true;
        }

        private static string ResolveChronicleMusicCue(string eventId)
        {
            return "event_" + eventId;
        }

        private static bool IsWarCategory(string category)
        {
            return !string.IsNullOrEmpty(category) && category.Equals("military", System.StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveSceneSnapshot(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return "Governance";
            if (sceneName.Equals("War", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("Campaign", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("scene_war", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("scene_campaign", System.StringComparison.OrdinalIgnoreCase))
            {
                return "War";
            }

            if (sceneName.Equals("PalaceConspiracy", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("Famine", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("Decline", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("Funeral", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("scene_palace_conspiracy", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("scene_famine", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("scene_decline", System.StringComparison.OrdinalIgnoreCase) ||
                sceneName.Equals("scene_funeral", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Event";
            }

            return "Governance";
        }

        private void OnGameStarted(GameEvent gameEvent)
        {
            narrationDispatcher?.PlayTutorialIntro();
        }

        private void OnVictoryChecked(GameEvent gameEvent)
        {
            if (gameEvent == null) return;
            VictoryCheckedPayload payload = gameEvent.Payload as VictoryCheckedPayload;
            if (payload == null) return;

            if (payload.isVictory)
            {
                narrationDispatcher?.PlayTutorialVictory();
            }
            else if (payload.isDefeat)
            {
                narrationDispatcher?.PlayTutorialDefeat();
            }
        }
    }
}
