using System;
using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public enum GameEventType
    {
        GameStarted,
        TurnStarted,
        TurnEnded,
        RegionSelected,
        RegionOwnerChanged,
        PolicyApplied,
        BattleResolved,
        EventTriggered,
        SuccessionResolved,
        VictoryChecked,
        TechResearched,
        WeatherChanged,
        CelestialEventOccurred,
        CustomEventOccurred,
        EquipmentUnlocked,
        ArmyMoveStarted,
        ArmyArrived,
        ContactDetected,
        EngagementStarted,
        RegionOccupied,
        GovernanceImpactApplied,
        EmperorSkillUsed,
        DiplomacyWarDeclared,
        DiplomacyTreatyBroken,
        DiplomacyProposalAccepted,
        DiplomacyProposalRejected,
        FrontlinePrepared,
        FrontlineLogisticsAdvanced,
        FrontlineLogisticsCommanded,
        FrontlineLogisticsRaided,
        OccupationPacificationQueueAdvanced,
        EspionageOperationStarted,
        EspionageOperationCompleted,
        EspionageAgentCaught,
        ChronicleEventTriggered,
        EmperorSelected,
        SceneMusicRequested
    }

    public sealed class GameEvent
    {
        public GameEventType Type { get; private set; }
        public string EntityId { get; private set; }
        public object Payload { get; private set; }

        public GameEvent(GameEventType type, string entityId, object payload)
        {
            Type = type;
            EntityId = entityId;
            Payload = payload;
        }
    }

    public sealed class RegionOwnerChangedPayload
    {
        public string regionId;
        public string previousOwnerFactionId;
        public string newOwnerFactionId;
    }

    public sealed class MapArmyMovementPayload
    {
        public string armyId;
        public string ownerFactionId;
        public string fromRegionId;
        public string toRegionId;
        public string[] route;
        public string task;
    }

    public sealed class EngagementPayload
    {
        public string engagementId;
        public string regionId;
        public string[] attackerArmyIds;
        public string[] defenderArmyIds;
    }

    public sealed class RegionOccupiedPayload
    {
        public string regionId;
        public string previousOwnerFactionId;
        public string newOwnerFactionId;
        public string engagementId;
    }

    public sealed class GovernanceImpactPayload
    {
        public string regionId;
        public int integration;
        public int taxContributionPercent;
        public int foodContributionPercent;
        public int rebellionRisk;
        public int localPower;
        public int annexationPressure;
        public int legitimacyBefore;
        public int legitimacyAfter;
        public int legitimacyDelta;
        public int occupationReservedFoodTransferred;
        public int occupationReservedFoodAvailable;
    }

    public sealed class FrontlinePreparationPayload
    {
        public string armyId;
        public string targetRegionId;
        public int foodBefore;
        public int foodAfter;
        public int foodCost;
        public int supplyBefore;
        public int supplyAfter;
        public int reserveFoodBefore;
        public int reserveFoodAfter;
        public int readinessBefore;
        public int readinessAfter;
        public string recommendedStep;
    }

    public sealed class FrontlineLogisticsPayload
    {
        public string armyId;
        public string convoyId;
        public string targetRegionId;
        public string supplyNodeRegionId;
        public int foodSpent;
        public int foodDelivered;
        public int foodLost;
        public int supplyAfter;
        public int reserveFoodAfter;
        public int turnsRemaining;
        public int completedSegments;
        public int priority;
        public bool paused;
    }

    public sealed class FrontlineLogisticsCommandPayload
    {
        public string armyId;
        public string convoyId;
        public string targetRegionId;
        public string command;
        public int priority;
        public bool paused;
        public int turnsRemaining;
        public int foodPerTurn;
    }

    public sealed class FrontlineLogisticsRaidPayload
    {
        public string armyId;
        public string convoyId;
        public string raiderFactionId;
        public string targetRegionId;
        public string supplyNodeRegionId;
        public int foodLost;
        public int riskBefore;
        public int riskAfter;
        public int turnsRemaining;
        public int raidPressure;
    }

    public sealed class OccupationPacificationQueuePayload
    {
        public string regionId;
        public string actionId;
        public int queueStepBefore;
        public int queueStepAfter;
        public int turnsRemainingAfter;
        public int reservedFoodAfter;
        public ControlStage controlStageAfter;
    }

    public sealed class VictoryCheckedPayload
    {
        public bool isVictory;
        public bool isDefeat;
        public int turnsPlayed;
        public int score;
    }

    public sealed class ChronicleEventPayload
    {
        public string eventId;
        public string category;
        public string musicCueId;
    }

    public sealed class EmperorSelectedPayload
    {
        public string emperorId;
    }

    public sealed class SceneMusicPayload
    {
        public string sceneMusicCueId;
        public bool play;
    }

    public sealed class EventBus
    {
        private readonly Dictionary<GameEventType, List<Action<GameEvent>>> listeners = new Dictionary<GameEventType, List<Action<GameEvent>>>();

        public void Subscribe(GameEventType type, Action<GameEvent> listener)
        {
            if (listener == null) return;

            List<Action<GameEvent>> existing;
            if (!listeners.TryGetValue(type, out existing))
            {
                existing = new List<Action<GameEvent>>();
                listeners[type] = existing;
            }

            if (!existing.Contains(listener))
            {
                existing.Add(listener);
            }
        }

        public void Unsubscribe(GameEventType type, Action<GameEvent> listener)
        {
            if (listener == null) return;

            List<Action<GameEvent>> existing;
            if (!listeners.TryGetValue(type, out existing)) return;

            existing.Remove(listener);
            if (existing.Count == 0)
            {
                listeners.Remove(type);
            }
        }

        public void Publish(GameEvent gameEvent)
        {
            List<Action<GameEvent>> listenerList;
            if (listeners.TryGetValue(gameEvent.Type, out listenerList))
            {
                Action<GameEvent>[] snapshot = listenerList.ToArray();
                for (int i = 0; i < snapshot.Length; i++)
                {
                    Action<GameEvent> handler = snapshot[i];
                    if (handler == null) continue;

                    try
                    {
                        handler.Invoke(gameEvent);
                    }
                    catch
                    {
                        // Event listeners are observers; one faulty observer must not block the rest.
                    }
                }
            }
        }
    }
}
