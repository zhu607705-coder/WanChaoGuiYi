using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class DiplomacyAI : MonoBehaviour
    {
        private DiplomacySystem cachedDiplomacySystem;

        private DiplomacySystem GetDiplomacySystem()
        {
            if (cachedDiplomacySystem == null)
            {
                cachedDiplomacySystem = FindObjectOfType<DiplomacySystem>();
            }
            return cachedDiplomacySystem;
        }

        public void ExecuteAIDiplomacy(GameContext context, FactionState faction)
        {
            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            if (emperor == null) return;

            DiplomacySystem diplomacySystem = GetDiplomacySystem();
            if (diplomacySystem == null) return;

            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState other = context.State.factions[i];
                if (other.id == faction.id) continue;

                DiplomaticRelation relation = diplomacySystem.FindRelation(context, faction.id, other.id);
                if (relation == null) continue;

                if (relation.status == DiplomacyStatus.AtWar)
                {
                    EvaluateWarContinuation(context, faction, other, relation, emperor, diplomacySystem);
                }
                else
                {
                    EvaluatePeacefulAction(context, faction, other, relation, emperor, diplomacySystem);
                }
            }
        }

        private void EvaluateWarContinuation(GameContext context, FactionState faction, FactionState enemy,
            DiplomaticRelation relation, EmperorDefinition emperor, DiplomacySystem diplomacySystem)
        {
            int myStrength = EstimateMilitaryStrength(context, faction);
            int enemyStrength = EstimateMilitaryStrength(context, enemy);

            if (myStrength < enemyStrength * 0.6f)
            {
                if (emperor.aiPersonality.riskTolerance < 50 && faction.money > 50)
                {
                    diplomacySystem.MakePeace(context, faction.id, enemy.id);
                }
            }
        }

        private void EvaluatePeacefulAction(GameContext context, FactionState faction, FactionState other,
            DiplomaticRelation relation, EmperorDefinition emperor, DiplomacySystem diplomacySystem)
        {
            float score = CalculateDiplomacyScore(context, faction, other, relation, emperor);

            if (score > 60f)
            {
                if (relation.status == DiplomacyStatus.Neutral && relation.opinion > 20)
                {
                    if (Random.value < 0.3f)
                    {
                        ProposeTreaty(context, diplomacySystem, faction, other, DiplomacyStatus.NonAggression);
                    }
                }
                else if (relation.status == DiplomacyStatus.NonAggression && relation.opinion > 40)
                {
                    if (Random.value < 0.2f)
                    {
                        ProposeTreaty(context, diplomacySystem, faction, other, DiplomacyStatus.Alliance);
                    }
                }
            }
            else if (score < -40f && relation.status != DiplomacyStatus.AtWar)
            {
                if (emperor.aiPersonality.riskTolerance > 70 && Random.value < 0.05f)
                {
                    DiplomacyProposal trapProposal = new DiplomacyProposal
                    {
                        fromFactionId = faction.id,
                        toFactionId = other.id,
                        proposedStatus = DiplomacyStatus.Alliance,
                        isTrap = true
                    };
                    diplomacySystem.ProposeTreaty(context, trapProposal);
                }
                else
                {
                    TryDeclareWar(context, diplomacySystem, faction, other, emperor);
                }
            }
            else if (score < -20f)
            {
                TryDeclareWar(context, diplomacySystem, faction, other, emperor);
            }
        }

        private float CalculateDiplomacyScore(GameContext context, FactionState self, FactionState other,
            DiplomaticRelation relation, EmperorDefinition emperor)
        {
            float score = 0f;
            score += relation.opinion * 0.3f;
            score -= relation.grudge * 0.5f;

            int myStrength = EstimateMilitaryStrength(context, self);
            int otherStrength = EstimateMilitaryStrength(context, other);
            float strengthRatio = otherStrength > 0 ? (float)myStrength / otherStrength : 2f;
            score += (strengthRatio - 1f) * 30f;

            float economyRatio = other.money > 0 ? (float)self.money / other.money : 2f;
            score += (economyRatio - 1f) * 10f;

            if (AreNeighbors(context, self, other))
            {
                score -= 10f;
            }

            score -= emperor.aiPersonality.expansion * 0.15f;
            score += emperor.aiPersonality.governance * 0.1f;

            if (emperor.aiPersonality.riskTolerance > 60)
            {
                score -= 5f;
            }

            if (self.legitimacy < NumericTuning.AiDiplomacyThreshold)
            {
                score += 15f;
            }

            if (other.legitimacy < NumericTuning.AiDiplomacyThreshold)
            {
                score -= 15f;
            }

            if (self.regionIds.Count < other.regionIds.Count)
            {
                score += 10f;
            }

            return score;
        }

        private void ProposeTreaty(GameContext context, DiplomacySystem diplomacySystem,
            FactionState proposer, FactionState target, DiplomacyStatus status)
        {
            DiplomacyProposal proposal = new DiplomacyProposal
            {
                fromFactionId = proposer.id,
                toFactionId = target.id,
                proposedStatus = status,
                moneyOffer = 0,
                foodOffer = 0,
                isTrap = false
            };
            diplomacySystem.ProposeTreaty(context, proposal);
        }

        private void TryDeclareWar(GameContext context, DiplomacySystem diplomacySystem,
            FactionState faction, FactionState other, EmperorDefinition emperor)
        {
            int myStrength = EstimateMilitaryStrength(context, faction);
            int otherStrength = EstimateMilitaryStrength(context, other);

            if (myStrength > otherStrength * NumericTuning.AiExpansionThreshold / 100)
            {
                if (emperor.aiPersonality.expansion > 60 && Random.value < 0.15f)
                {
                    diplomacySystem.DeclareWar(context, faction.id, other.id);
                }
            }
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

        private bool AreNeighbors(GameContext context, FactionState a, FactionState b)
        {
            for (int i = 0; i < a.regionIds.Count; i++)
            {
                RegionDefinition regionDef = context.Data.GetRegion(a.regionIds[i]);
                if (regionDef == null || regionDef.neighbors == null) continue;
                for (int j = 0; j < regionDef.neighbors.Length; j++)
                {
                    RegionState neighbor = context.State.FindRegion(regionDef.neighbors[j]);
                    if (neighbor != null && neighbor.ownerFactionId == b.id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
