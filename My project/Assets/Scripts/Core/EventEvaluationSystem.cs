using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class EventEvaluationSystem : MonoBehaviour, IGameSystem
    {
        private readonly Dictionary<string, int> eventCooldowns = new Dictionary<string, int>();

        public void Initialize(GameContext context) { }

        public void OnTurnStart(GameContext context)
        {
            List<string> keys = new List<string>(eventCooldowns.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                eventCooldowns[keys[i]]--;
                if (eventCooldowns[keys[i]] <= 0)
                {
                    eventCooldowns.Remove(keys[i]);
                }
            }
        }

        public void ExecuteTurn(GameContext context)
        {
            int triggered = 0;
            List<EventDefinition> eligible = new List<EventDefinition>();

            foreach (var kvp in context.Data.Events)
            {
                EventDefinition evt = kvp.Value;
                if (evt == null || evt.trigger == null) continue;

                int cd;
                if (eventCooldowns.TryGetValue(evt.id, out cd) && cd > 0) continue;

                for (int i = 0; i < context.State.factions.Count; i++)
                {
                    FactionState faction = context.State.factions[i];
                    if (EvaluateTrigger(context, faction, evt.trigger))
                    {
                        eligible.Add(evt);
                        break;
                    }
                }
            }

            Shuffle(eligible);
            NumericContext numericContext = context.State.factions.Count > 0
                ? NumericModifierFactory.ForFaction(context.State.factions[0])
                : new NumericContext();
            float triggerChance = NumericFormulas.CalculateEventTriggerChance(numericContext);

            for (int i = 0; i < eligible.Count && triggered < NumericTuning.MaxEventsPerTurn; i++)
            {
                if (Random.value < triggerChance)
                {
                    TriggerEvent(context, eligible[i], numericContext);
                    triggered++;
                }
            }
        }

        public void OnTurnEnd(GameContext context) { }

        private bool EvaluateTrigger(GameContext context, FactionState faction, EventTrigger trigger)
        {
            if (!string.IsNullOrEmpty(trigger.emperorId) && trigger.emperorId != faction.emperorId)
            {
                return false;
            }

            if (trigger.minTurn > 0 && context.State.turn < trigger.minTurn)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(trigger.era))
            {
                string currentEra = InferEra(context.State.turn);
                if (trigger.era != currentEra) return false;
            }

            if (trigger.minArmyStrength > 0)
            {
                int armyStrength = EstimateMilitaryStrength(context, faction);
                if (armyStrength < trigger.minArmyStrength) return false;
            }

            if (trigger.maxArmyStrength > 0)
            {
                int armyStrength = EstimateMilitaryStrength(context, faction);
                if (armyStrength > trigger.maxArmyStrength) return false;
            }

            if (trigger.minSuccessionRisk > 0 && faction.successionRisk < trigger.minSuccessionRisk)
            {
                return false;
            }

            if (trigger.minCourtFactionPressure > 0 && faction.courtFactionPressure < trigger.minCourtFactionPressure)
            {
                return false;
            }

            if (trigger.minRebellionRisk > 0)
            {
                int maxRebellion = 0;
                for (int i = 0; i < faction.regionIds.Count; i++)
                {
                    RegionState region = context.State.FindRegion(faction.regionIds[i]);
                    if (region != null && region.rebellionRisk > maxRebellion)
                    {
                        maxRebellion = region.rebellionRisk;
                    }
                }
                if (maxRebellion < trigger.minRebellionRisk) return false;
            }

            if (trigger.minPopularDissatisfaction > 0)
            {
                if (faction.legitimacy > 100 - trigger.minPopularDissatisfaction) return false;
            }

            if (trigger.minLocalPower > 0)
            {
                int maxLocalPower = 0;
                for (int i = 0; i < faction.regionIds.Count; i++)
                {
                    RegionState region = context.State.FindRegion(faction.regionIds[i]);
                    if (region != null && region.localPower > maxLocalPower)
                    {
                        maxLocalPower = region.localPower;
                    }
                }
                if (maxLocalPower < trigger.minLocalPower) return false;
            }

            if (trigger.minFrontierThreat > 0)
            {
                int neighborCount = 0;
                for (int i = 0; i < faction.regionIds.Count; i++)
                {
                    RegionDefinition regionDef = context.Data.GetRegion(faction.regionIds[i]);
                    if (regionDef != null && regionDef.neighbors != null)
                    {
                        for (int j = 0; j < regionDef.neighbors.Length; j++)
                        {
                            RegionState neighbor = context.State.FindRegion(regionDef.neighbors[j]);
                            if (neighbor != null && neighbor.ownerFactionId != faction.id)
                            {
                                neighborCount++;
                            }
                        }
                    }
                }
                if (neighborCount < trigger.minFrontierThreat) return false;
            }

            if (!string.IsNullOrEmpty(trigger.policyUsed))
            {
                if (!faction.completedReformIds.Contains(trigger.policyUsed)) return false;
            }

            if (!string.IsNullOrEmpty(trigger.terrainTag))
            {
                bool hasTerrain = false;
                for (int i = 0; i < faction.regionIds.Count; i++)
                {
                    RegionDefinition regionDef = context.Data.GetRegion(faction.regionIds[i]);
                    if (regionDef != null && regionDef.terrain == trigger.terrainTag)
                    {
                        hasTerrain = true;
                        break;
                    }
                }
                if (!hasTerrain) return false;
            }

            return true;
        }

        private void TriggerEvent(GameContext context, EventDefinition evt, NumericContext numericContext)
        {
            int cooldown = NumericFormulas.CalculateEventCooldown(numericContext);
            eventCooldowns[evt.id] = Mathf.Max(evt.cooldownTurns, cooldown);

            context.State.AddLog("event", "【" + evt.name + "】" + "发生了！");
            context.Events.Publish(new GameEvent(GameEventType.EventTriggered, evt.id, evt));
        }

        private int EstimateMilitaryStrength(GameContext context, FactionState faction)
        {
            int total = 0;
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                if (context.State.armies[i].ownerFactionId == faction.id)
                {
                    total += context.State.armies[i].soldiers;
                }
            }
            return total;
        }

        private string InferEra(int turn)
        {
            if (turn < 20) return "classical";
            if (turn < 60) return "medieval";
            return "early_modern";
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
