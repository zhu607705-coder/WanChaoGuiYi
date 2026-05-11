#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DATA_DIR="${1:-"$ROOT_DIR/My project/Assets/Data"}"
PLAYER_FACTION_ID="${2:-faction_qin_shi_huang}"

PYTHON_BIN="${PYTHON_BIN:-}"
if [[ -z "$PYTHON_BIN" ]]; then
  if command -v python >/dev/null 2>&1; then
    PYTHON_BIN="python"
  elif command -v python3 >/dev/null 2>&1; then
    PYTHON_BIN="python3"
  else
    echo "Python is required to run the headless verification gate." >&2
    exit 127
  fi
fi

"$PYTHON_BIN" "$ROOT_DIR/tools/validate_data.py" "$DATA_DIR"
"$PYTHON_BIN" "$ROOT_DIR/tools/validate_domain_core.py"
"$ROOT_DIR/tools/run_headless_simulation.sh" "$DATA_DIR" "$PLAYER_FACTION_ID"
"$PYTHON_BIN" - "$ROOT_DIR/tools/headless_runner/latest-war-report.json" <<'PY'
import json
import sys

path = sys.argv[1]
with open(path, "r", encoding="utf-8") as handle:
    report = json.load(handle)

if report.get("runName") != "headless_war_causality_scenarios":
    raise SystemExit("Unexpected runName in " + path)
if report.get("passed") is not True:
    raise SystemExit("Headless war report did not pass")
if report.get("scenarioCount", 0) < 14:
    raise SystemExit("Expected at least 14 scenarios in " + path)
if report.get("failedCount") != 0:
    raise SystemExit("Expected all scenarios to pass in " + path)
if report.get("passedCount") != report.get("scenarioCount"):
    raise SystemExit("Expected passedCount to match scenarioCount in " + path)

required = {
    "defender_holds_and_attacker_retreats": [
        "command.attack_route_created",
        "movement.attacker_arrived_at_defended_region",
        "engagement.created_with_attacker_and_defender",
        "battle.defender_won",
        "outcome.attacker_retreated_or_routed",
        "outcome.region_owner_unchanged",
        "outcome.no_resolved_engagement_left",
    ],
    "attacker_wins_and_occupies": [
        "command.attack_route_created",
        "movement.attacker_arrived_at_target_region",
        "engagement.created_with_attacker_and_defender",
        "battle.attacker_won",
        "outcome.region_owner_changed_to_attacker",
        "governance.occupation_reduced_contribution",
        "causal.occupation_reduced_legitimacy",
        "economy.money_delta_matches_runtime_contribution",
        "economy.food_delta_matches_runtime_contribution",
    ],
    "reinforcement_joins_existing_engagement": [
        "command.reinforcement_route_created",
        "movement.reinforcement_arrived_at_engagement_region",
        "engagement.membership_changed_after_reinforcement",
        "battle.result_changed_or_explained_by_membership",
        "outcome.no_stale_reinforcement_task",
    ],
    "active_retreat_leaves_engagement": [
        "command.retreat_route_created",
        "movement.retreating_army_left_region",
        "engagement.retreating_army_removed_from_membership",
        "outcome.no_residual_engagement_after_retreat",
        "battle.no_battle_triggered_after_retreat",
        "outcome.region_returns_to_controlled_when_uncontested",
        "command.unengaged_retreat_rejected",
    ],
    "tax_pressure_raises_rebellion": [
        "causal.tax_pressure_increases_tax",
        "causal.tax_pressure_raises_rebellion",
        "causal.tax_pressure_does_not_raise_legitimacy",
    ],
    "conscription_consumes_population_and_manpower": [
        "causal.conscription_consumes_manpower",
        "causal.conscription_reduces_population",
        "causal.conscription_adds_soldiers",
        "causal.conscription_raises_rebellion",
    ],
    "war_consumes_grain_and_supply": [
        "command.attack_route_created",
        "causal.war_supply_decreased",
        "economy.money_delta_matches_runtime_contribution",
        "economy.food_delta_matches_runtime_contribution",
        "causal.war_food_upkeep_applied",
    ],
    "border_control_costs_and_diplomatic_pressure": [
        "causal.border_control_paid_costs",
        "causal.border_control_has_diplomatic_cost",
    ],
    "low_supply_reduces_battle_power": [
        "causal.low_supply_reduces_battle_power",
        "causal.low_supply_can_change_result",
        "command.attack_route_created",
        "engagement.created_with_low_supply_attacker",
        "outcome.low_supply_attacker_lost",
        "battle.low_supply_modifier_reported",
    ],
    "region_specialization_and_governance_forecasts": [
        "specialization.all_regions_resolve",
        "specialization.guanzhong_capital_or_legitimacy",
        "specialization.jiangdong_grain_tax_population",
        "specialization.hexi_border_corridor",
        "specialization.bashu_grain_basin_defense",
        "specialization.liaodong_frontier_military",
        "forecast.pacify_matches_applied_delta",
        "forecast.tax_pressure_matches_applied_delta",
        "forecast.conscription_matches_applied_delta",
        "forecast.building_cost_and_source_match_execution",
    ],
    "occupation_control_chain_progression": [
        "occupation.starts_newly_attached",
        "military_governance.suppresses_but_costs",
        "pacify.advances_control_stage",
        "register_households.restores_contribution",
        "control_chain.progresses_with_costs",
    ],
    "connected_campaign_vision_and_interception": [
        "campaign.disconnected_dispatch_rejected",
        "campaign.supply_node_extends_campaign_range",
        "campaign.unscouted_route_hides_enemy_detail",
        "campaign.scouting_reveals_route_risk",
        "campaign.hostile_adjacent_route_interception",
        "campaign.supply_visibility_interception",
    ],
    "relief_and_tax_pressure_causality": [
        "food_causality.relief_forecast_matches_applied_delta",
        "food_causality.relief_and_tax_pressure",
    ],
    "food_supply_order_diplomacy_coupling": [
        "food.grain_shortage_hurts_supply_and_order",
        "food.relief_consumes_grain_and_reduces_unrest",
        "food.army_supply_draws_from_grain",
        "food.food_aid_improves_relation_with_cost",
    ],
}

scenarios = {scenario.get("name"): scenario for scenario in report.get("scenarios", [])}
for name, assertion_ids in required.items():
    scenario = scenarios.get(name)
    if scenario is None:
        raise SystemExit("Missing scenario in report: " + name)

    assertions = {assertion.get("id"): assertion for assertion in scenario.get("assertions", [])}
    for assertion in scenario.get("assertions", []):
        if assertion.get("passed") is not True:
            raise SystemExit("Failed assertion in " + name + ": " + str(assertion.get("id")))

    for assertion_id in assertion_ids:
        assertion = assertions.get(assertion_id)
        if assertion is None:
            raise SystemExit("Missing required assertion in " + name + ": " + assertion_id)
        if assertion.get("passed") is not True:
            raise SystemExit("Required assertion did not pass in " + name + ": " + assertion_id)
PY
