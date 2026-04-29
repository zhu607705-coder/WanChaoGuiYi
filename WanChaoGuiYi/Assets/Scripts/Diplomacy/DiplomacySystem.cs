using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class DiplomacySystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context)
        {
            InitializeRelations(context);
        }

        public void OnTurnStart(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            DecayGrudges(context);
            ProcessTreatyExpiration(context);
        }

        public void OnTurnEnd(GameContext context) { }

        private void InitializeRelations(GameContext context)
        {
            List<FactionState> factions = context.State.factions;
            for (int i = 0; i < factions.Count; i++)
            {
                for (int j = i + 1; j < factions.Count; j++)
                {
                    DiplomaticRelation relation = new DiplomaticRelation
                    {
                        factionA = factions[i].id,
                        factionB = factions[j].id,
                        status = DiplomacyStatus.Neutral,
                        opinion = 0,
                        turnsRemaining = -1,
                        grudge = 0,
                        isPlayerInvolved = factions[i].id == context.State.playerFactionId ||
                                          factions[j].id == context.State.playerFactionId
                    };
                    context.State.diplomaticRelations.Add(relation);
                }
            }
        }

        public DiplomaticRelation FindRelation(GameContext context, string factionA, string factionB)
        {
            for (int i = 0; i < context.State.diplomaticRelations.Count; i++)
            {
                DiplomaticRelation r = context.State.diplomaticRelations[i];
                if ((r.factionA == factionA && r.factionB == factionB) ||
                    (r.factionA == factionB && r.factionB == factionA))
                {
                    return r;
                }
            }
            return null;
        }

        public bool AreAtWar(GameContext context, string factionA, string factionB)
        {
            DiplomaticRelation relation = FindRelation(context, factionA, factionB);
            return relation != null && relation.status == DiplomacyStatus.AtWar;
        }

        public bool AreAllied(GameContext context, string factionA, string factionB)
        {
            DiplomaticRelation relation = FindRelation(context, factionA, factionB);
            return relation != null && relation.status == DiplomacyStatus.Alliance;
        }

        public DiplomacyResult ProposeTreaty(GameContext context, DiplomacyProposal proposal)
        {
            FactionState fromFaction = context.State.FindFaction(proposal.fromFactionId);
            FactionState toFaction = context.State.FindFaction(proposal.toFactionId);
            if (fromFaction == null || toFaction == null)
            {
                return new DiplomacyResult { success = false, reason = "faction_not_found" };
            }

            DiplomaticRelation relation = FindRelation(context, proposal.fromFactionId, proposal.toFactionId);
            if (relation == null)
            {
                return new DiplomacyResult { success = false, reason = "no_relation" };
            }

            if (relation.status == DiplomacyStatus.AtWar && proposal.proposedStatus != DiplomacyStatus.Neutral)
            {
                return new DiplomacyResult { success = false, reason = "at_war" };
            }

            if (proposal.isTrap)
            {
                return ProcessTrapProposal(context, proposal, relation);
            }

            float acceptChance = CalculateAcceptChance(context, proposal, relation);
            float roll = Random.value;

            if (roll < acceptChance)
            {
                return AcceptProposal(context, proposal, relation);
            }
            else
            {
                return RejectProposal(context, proposal, relation);
            }
        }

        public DiplomacyResult DeclareWar(GameContext context, string attackerId, string defenderId)
        {
            FactionState attacker = context.State.FindFaction(attackerId);
            FactionState defender = context.State.FindFaction(defenderId);
            if (attacker == null || defender == null)
            {
                return new DiplomacyResult { success = false, reason = "faction_not_found" };
            }

            DiplomaticRelation relation = FindRelation(context, attackerId, defenderId);
            if (relation == null)
            {
                return new DiplomacyResult { success = false, reason = "no_relation" };
            }

            if (relation.status == DiplomacyStatus.AtWar)
            {
                return new DiplomacyResult { success = false, reason = "already_at_war" };
            }

            int grudgeIncrease = 0;
            if (relation.status == DiplomacyStatus.Alliance)
            {
                attacker.legitimacy = Mathf.Max(0, attacker.legitimacy - 15);
                grudgeIncrease = 30;
                context.State.AddLog("diplomacy", attacker.name + "背弃盟约，向" + defender.name + "宣战！合法性大幅下降。");
            }
            else if (relation.status == DiplomacyStatus.NonAggression)
            {
                attacker.legitimacy = Mathf.Max(0, attacker.legitimacy - 8);
                grudgeIncrease = 15;
                context.State.AddLog("diplomacy", attacker.name + "撕毁互不侵犯条约，向" + defender.name + "宣战！");
            }
            else
            {
                context.State.AddLog("diplomacy", attacker.name + "向" + defender.name + "宣战！");
            }

            relation.status = DiplomacyStatus.AtWar;
            relation.turnsRemaining = -1;
            relation.grudge += grudgeIncrease;

            context.Events.Publish(new GameEvent(GameEventType.DiplomacyWarDeclared, attackerId, relation));
            return new DiplomacyResult { success = true, reason = "war_declared" };
        }

        public DiplomacyResult KillEnvoy(GameContext context, string killerId, string victimId)
        {
            DiplomaticRelation relation = FindRelation(context, killerId, victimId);
            if (relation == null)
            {
                return new DiplomacyResult { success = false, reason = "no_relation" };
            }

            FactionState killer = context.State.FindFaction(killerId);
            if (killer == null) return new DiplomacyResult { success = false, reason = "faction_not_found" };

            relation.status = DiplomacyStatus.AtWar;
            relation.turnsRemaining = -1;
            relation.grudge += 50;
            relation.opinion = Mathf.Max(NumericTuning.MinOpinion, relation.opinion - 40);

            killer.legitimacy = Mathf.Max(0, killer.legitimacy - 12);

            for (int i = 0; i < context.State.diplomaticRelations.Count; i++)
            {
                DiplomaticRelation r = context.State.diplomaticRelations[i];
                if (r.factionA == killerId || r.factionB == killerId)
                {
                    r.opinion = Mathf.Max(NumericTuning.MinOpinion, r.opinion - 8);
                }
            }

            context.State.AddLog("diplomacy", killer.name + "斩杀来使！天下震惊，" + relation.GetOtherFaction(killerId) + "直接宣战！");
            context.Events.Publish(new GameEvent(GameEventType.DiplomacyWarDeclared, killerId, relation));
            return new DiplomacyResult { success = true, reason = "envoy_killed_war_declared" };
        }

        public DiplomacyResult MakePeace(GameContext context, string proposerId, string targetId)
        {
            DiplomaticRelation relation = FindRelation(context, proposerId, targetId);
            if (relation == null || relation.status != DiplomacyStatus.AtWar)
            {
                return new DiplomacyResult { success = false, reason = "not_at_war" };
            }

            FactionState proposer = context.State.FindFaction(proposerId);
            FactionState target = context.State.FindFaction(targetId);
            if (proposer == null || target == null)
            {
                return new DiplomacyResult { success = false, reason = "faction_not_found" };
            }

            int peaceCost = Mathf.Max(10, Mathf.RoundToInt(proposer.money * 0.15f));
            if (proposer.money < peaceCost)
            {
                return new DiplomacyResult { success = false, reason = "insufficient_funds" };
            }

            proposer.money -= peaceCost;
            target.money += peaceCost;

            relation.status = DiplomacyStatus.Neutral;
            relation.turnsRemaining = -1;
            relation.opinion = Mathf.Min(NumericTuning.MaxOpinion, relation.opinion + 10);

            context.State.AddLog("diplomacy", proposer.name + "向" + target.name + "求和，赔偿" + peaceCost + "金。");
            return new DiplomacyResult { success = true, reason = "peace_accepted", moneyCost = peaceCost };
        }

        private DiplomacyResult ProcessTrapProposal(GameContext context, DiplomacyProposal proposal, DiplomaticRelation relation)
        {
            FactionState trapper = context.State.FindFaction(proposal.fromFactionId);
            FactionState target = context.State.FindFaction(proposal.toFactionId);

            int detectionChance = NumericTuning.TrapDetectionBase;
            EmperorDefinition targetEmperor = context.Data.GetEmperor(target.emperorId);
            if (targetEmperor != null)
            {
                detectionChance += targetEmperor.score.wisdom / 5;
            }

            if (relation.opinion < -20) detectionChance += 15;

            if (Random.Range(0, 100) < detectionChance)
            {
                relation.status = DiplomacyStatus.AtWar;
                relation.turnsRemaining = -1;
                relation.grudge += 40;
                relation.opinion = Mathf.Max(NumericTuning.MinOpinion, relation.opinion - 50);
                trapper.legitimacy = Mathf.Max(0, trapper.legitimacy - 10);

                context.State.AddLog("diplomacy", trapper.name + "设下鸿门宴，被" + target.name + "识破！双方进入战争状态。");
                context.Events.Publish(new GameEvent(GameEventType.DiplomacyTreatyBroken, trapper.id, relation));
                return new DiplomacyResult { success = false, reason = "trap_detected" }
;            }
            else
            {
                target.legitimacy = Mathf.Max(0, target.legitimacy - 20);
                target.money = Mathf.Max(0, target.money - 50);
                relation.grudge += 60;
                relation.opinion = Mathf.Max(NumericTuning.MinOpinion, relation.opinion - 60);

                if (target.heir != null && Random.value < 0.4f)
                {
                    context.State.AddLog("diplomacy", trapper.name + "设鸿门宴成功！" + target.name + "继承人遇害。");
                    target.heir = null;
                    target.successionRisk = Mathf.Min(100, target.successionRisk + 25);
                }
                else
                {
                    context.State.AddLog("diplomacy", trapper.name + "设鸿门宴成功！" + target.name + "损失惨重。");
                }

                relation.status = DiplomacyStatus.AtWar;
                relation.turnsRemaining = -1;

                return new DiplomacyResult { success = true, reason = "trap_succeeded" };
            }
        }

        private float CalculateAcceptChance(GameContext context, DiplomacyProposal proposal, DiplomaticRelation relation)
        {
            FactionState fromFaction = context.State.FindFaction(proposal.fromFactionId);
            FactionState toFaction = context.State.FindFaction(proposal.toFactionId);
            NumericContext numericContext = fromFaction != null ? NumericModifierFactory.ForFaction(fromFaction) : new NumericContext();

            int baseAcceptance = NumericFormulas.CalculateDiplomacyAcceptance(fromFaction, toFaction, numericContext);
            float baseChance = baseAcceptance / 100f;

            baseChance += relation.opinion * 0.003f;
            baseChance -= relation.grudge * 0.002f;

            switch (proposal.proposedStatus)
            {
                case DiplomacyStatus.NonAggression:
                    baseChance += 0.15f;
                    break;
                case DiplomacyStatus.Alliance:
                    baseChance -= 0.1f;
                    break;
                case DiplomacyStatus.Vassal:
                    baseChance -= 0.3f;
                    break;
                case DiplomacyStatus.Tributary:
                    baseChance -= 0.15f;
                    break;
            }

            if (proposal.moneyOffer > 0 && toFaction != null && toFaction.money > 0)
            {
                baseChance += (float)proposal.moneyOffer / toFaction.money * 0.3f;
            }

            if (fromFaction != null)
            {
                EmperorDefinition emperor = context.Data.GetEmperor(fromFaction.emperorId);
                if (emperor != null)
                {
                    baseChance += emperor.stats.diplomacy * 0.002f;
                }
            }

            return Mathf.Clamp01(baseChance);
        }

        private DiplomacyResult AcceptProposal(GameContext context, DiplomacyProposal proposal, DiplomaticRelation relation)
        {
            FactionState fromFaction = context.State.FindFaction(proposal.fromFactionId);
            FactionState toFaction = context.State.FindFaction(proposal.toFactionId);

            if (proposal.moneyOffer > 0)
            {
                fromFaction.money -= proposal.moneyOffer;
                toFaction.money += proposal.moneyOffer;
            }
            if (proposal.foodOffer > 0)
            {
                fromFaction.food -= proposal.foodOffer;
                toFaction.food += proposal.foodOffer;
            }

            relation.status = proposal.proposedStatus;
            NumericContext numericContext = NumericModifierFactory.ForFaction(fromFaction);
            relation.turnsRemaining = GetTreatyDuration(proposal.proposedStatus, numericContext);
            relation.opinion = Mathf.Min(NumericTuning.MaxOpinion, relation.opinion + 10);

            string statusName = GetStatusName(proposal.proposedStatus);
            context.State.AddLog("diplomacy", fromFaction.name + "与" + toFaction.name + "签订" + statusName + "。");
            context.Events.Publish(new GameEvent(GameEventType.DiplomacyProposalAccepted, proposal.fromFactionId, proposal));
            return new DiplomacyResult { success = true, reason = "accepted" };
        }

        private DiplomacyResult RejectProposal(GameContext context, DiplomacyProposal proposal, DiplomaticRelation relation)
        {
            FactionState fromFaction = context.State.FindFaction(proposal.fromFactionId);
            FactionState toFaction = context.State.FindFaction(proposal.toFactionId);

            relation.opinion = Mathf.Max(NumericTuning.MinOpinion, relation.opinion - 5);

            string statusName = GetStatusName(proposal.proposedStatus);
            context.State.AddLog("diplomacy", toFaction.name + "拒绝了" + fromFaction.name + "的" + statusName + "提议。");
            context.Events.Publish(new GameEvent(GameEventType.DiplomacyProposalRejected, proposal.fromFactionId, proposal));
            return new DiplomacyResult { success = false, reason = "rejected" };
        }

        private void DecayGrudges(GameContext context)
        {
            for (int i = 0; i < context.State.diplomaticRelations.Count; i++)
            {
                DiplomaticRelation r = context.State.diplomaticRelations[i];
                if (r.grudge > 0)
                {
                    FactionState factionA = context.State.FindFaction(r.factionA);
                    NumericContext numericContext = factionA != null ? NumericModifierFactory.ForFaction(factionA) : new NumericContext();
                    int decay = NumericFormulas.CalculateGrudgeDecay(r, numericContext);
                    r.grudge = Mathf.Max(0, r.grudge - decay);
                }
            }
        }

        private void ProcessTreatyExpiration(GameContext context)
        {
            for (int i = 0; i < context.State.diplomaticRelations.Count; i++)
            {
                DiplomaticRelation r = context.State.diplomaticRelations[i];
                if (r.turnsRemaining > 0)
                {
                    r.turnsRemaining--;
                    if (r.turnsRemaining <= 0)
                    {
                        string expiredStatus = GetStatusName(r.status);
                        r.status = DiplomacyStatus.Neutral;
                        r.turnsRemaining = -1;
                        r.opinion = Mathf.Max(NumericTuning.MinOpinion, r.opinion - 5);

                        context.State.AddLog("diplomacy", r.factionA + "与" + r.factionB + "的" + expiredStatus + "条约到期。");
                        context.Events.Publish(new GameEvent(GameEventType.DiplomacyTreatyBroken, r.factionA, r));
                    }
                }
            }
        }

        private void ApplyEmperorDiplomacyPassive(GameContext context, FactionState faction, string actionType, DiplomaticRelation relation)
        {
            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            if (emperor == null) return;

            int diplomacyStat = emperor.stats.diplomacy;

            switch (actionType)
            {
                case "declare_war":
                    int grudgeReduction = Mathf.RoundToInt(diplomacyStat / 20f);
                    relation.grudge = Mathf.Max(0, relation.grudge - grudgeReduction);
                    break;
            }
        }

        private int GetTreatyDuration(DiplomacyStatus status, NumericContext numericContext)
        {
            string treatyType;
            switch (status)
            {
                case DiplomacyStatus.NonAggression: treatyType = "non_aggression"; break;
                case DiplomacyStatus.Alliance: treatyType = "alliance"; break;
                case DiplomacyStatus.Vassal: treatyType = "vassal"; break;
                case DiplomacyStatus.Tributary: treatyType = "tributary"; break;
                default: return -1;
            }
            return NumericFormulas.CalculateTreatyDuration(treatyType, numericContext);
        }

        private string GetStatusName(DiplomacyStatus status)
        {
            switch (status)
            {
                case DiplomacyStatus.Neutral: return "中立";
                case DiplomacyStatus.NonAggression: return "互不侵犯";
                case DiplomacyStatus.Alliance: return "同盟";
                case DiplomacyStatus.Vassal: return "称臣";
                case DiplomacyStatus.Tributary: return "纳贡";
                case DiplomacyStatus.AtWar: return "交战";
                default: return "未知";
            }
        }
    }

    public sealed class DiplomacyResult
    {
        public bool success;
        public string reason;
        public int moneyCost;
    }
}
