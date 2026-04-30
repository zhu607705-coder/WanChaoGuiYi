#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

python3 "$ROOT_DIR/tools/validate_data.py"
python3 "$ROOT_DIR/tools/validate_domain_core.py"
"$ROOT_DIR/tools/run_headless_simulation.sh" "$@"
python3 - "$ROOT_DIR/tools/headless_runner/latest-war-report.json" <<'PY'
import json
import sys

path = sys.argv[1]
with open(path, "r", encoding="utf-8") as handle:
    report = json.load(handle)

if report.get("runName") != "headless_war_four_scenarios":
    raise SystemExit("Unexpected runName in " + path)
if report.get("passed") is not True:
    raise SystemExit("Headless war report did not pass")
if report.get("scenarioCount") != 4:
    raise SystemExit("Expected scenarioCount=4 in " + path)
if report.get("passedCount") != 4 or report.get("failedCount") != 0:
    raise SystemExit("Expected all four scenarios to pass in " + path)

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
