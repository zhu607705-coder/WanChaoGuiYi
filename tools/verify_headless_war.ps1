$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$DataDir = if ($args.Count -ge 1) { $args[0] } else { Join-Path $RootDir "My project/Assets/Data" }
$PlayerFactionId = if ($args.Count -ge 2) { $args[1] } else { "faction_qin_shi_huang" }

$PythonBin = $env:PYTHON_BIN
if (-not $PythonBin) {
    $pythonCommand = Get-Command python -ErrorAction SilentlyContinue
    if ($pythonCommand) {
        $PythonBin = $pythonCommand.Source
    } else {
        $python3Command = Get-Command python3 -ErrorAction SilentlyContinue
        if ($python3Command) {
            $PythonBin = $python3Command.Source
        } else {
            Write-Error "Python is required to run the headless verification gate."
        }
    }
}

& $PythonBin (Join-Path $RootDir "tools/validate_data.py") $DataDir
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $PythonBin (Join-Path $RootDir "tools/validate_domain_core.py")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& (Join-Path $RootDir "tools/run_headless_simulation.ps1") $DataDir $PlayerFactionId
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$ReportPath = Join-Path $RootDir "tools/headless_runner/latest-war-report.json"
$Report = Get-Content -Raw -Encoding UTF8 $ReportPath | ConvertFrom-Json

if ($Report.runName -ne "headless_war_causality_scenarios") { Write-Error "Unexpected runName in $ReportPath" }
if ($Report.passed -ne $true) { Write-Error "Headless war report did not pass" }
if ($Report.scenarioCount -lt 14) { Write-Error "Expected at least 14 scenarios in $ReportPath" }
if ($Report.failedCount -ne 0) { Write-Error "Expected all scenarios to pass in $ReportPath" }
if ($Report.passedCount -ne $Report.scenarioCount) { Write-Error "Expected passedCount to match scenarioCount in $ReportPath" }

$Required = @{
    defender_holds_and_attacker_retreats = @(
        "command.attack_route_created",
        "movement.attacker_arrived_at_defended_region",
        "engagement.created_with_attacker_and_defender",
        "battle.defender_won",
        "outcome.attacker_retreated_or_routed",
        "outcome.region_owner_unchanged",
        "outcome.no_resolved_engagement_left"
    )
    attacker_wins_and_occupies = @(
        "command.attack_route_created",
        "movement.attacker_arrived_at_target_region",
        "engagement.created_with_attacker_and_defender",
        "battle.attacker_won",
        "outcome.region_owner_changed_to_attacker",
        "governance.occupation_reduced_contribution",
        "causal.occupation_reduced_legitimacy",
        "economy.money_delta_matches_runtime_contribution",
        "economy.food_delta_matches_runtime_contribution"
    )
    reinforcement_joins_existing_engagement = @(
        "command.reinforcement_route_created",
        "movement.reinforcement_arrived_at_engagement_region",
        "engagement.membership_changed_after_reinforcement",
        "battle.result_changed_or_explained_by_membership",
        "outcome.no_stale_reinforcement_task"
    )
    active_retreat_leaves_engagement = @(
        "command.retreat_route_created",
        "movement.retreating_army_left_region",
        "engagement.retreating_army_removed_from_membership",
        "outcome.no_residual_engagement_after_retreat",
        "battle.no_battle_triggered_after_retreat",
        "outcome.region_returns_to_controlled_when_uncontested",
        "command.unengaged_retreat_rejected"
    )
    tax_pressure_raises_rebellion = @(
        "causal.tax_pressure_increases_tax",
        "causal.tax_pressure_raises_rebellion",
        "causal.tax_pressure_does_not_raise_legitimacy"
    )
    conscription_consumes_population_and_manpower = @(
        "causal.conscription_consumes_manpower",
        "causal.conscription_reduces_population",
        "causal.conscription_adds_soldiers",
        "causal.conscription_raises_rebellion"
    )
    war_consumes_grain_and_supply = @(
        "command.attack_route_created",
        "causal.war_supply_decreased",
        "economy.money_delta_matches_runtime_contribution",
        "economy.food_delta_matches_runtime_contribution",
        "causal.war_food_upkeep_applied"
    )
    border_control_costs_and_diplomatic_pressure = @(
        "causal.border_control_paid_costs",
        "causal.border_control_has_diplomatic_cost"
    )
    low_supply_reduces_battle_power = @(
        "causal.low_supply_reduces_battle_power",
        "causal.low_supply_can_change_result",
        "command.attack_route_created",
        "engagement.created_with_low_supply_attacker",
        "outcome.low_supply_attacker_lost",
        "battle.low_supply_modifier_reported"
    )
    region_specialization_and_governance_forecasts = @(
        "specialization.all_regions_resolve",
        "specialization.guanzhong_capital_or_legitimacy",
        "specialization.jiangdong_grain_tax_population",
        "specialization.hexi_border_corridor",
        "specialization.bashu_grain_basin_defense",
        "specialization.liaodong_frontier_military",
        "forecast.pacify_matches_applied_delta",
        "forecast.tax_pressure_matches_applied_delta",
        "forecast.conscription_matches_applied_delta",
        "forecast.building_cost_and_source_match_execution"
    )
    occupation_control_chain_progression = @(
        "occupation.starts_newly_attached",
        "military_governance.suppresses_but_costs",
        "pacify.advances_control_stage",
        "register_households.restores_contribution",
        "control_chain.progresses_with_costs"
    )
    connected_campaign_vision_and_interception = @(
        "campaign.disconnected_dispatch_rejected",
        "campaign.supply_node_extends_campaign_range",
        "campaign.unscouted_route_hides_enemy_detail",
        "campaign.scouting_reveals_route_risk",
        "campaign.hostile_adjacent_route_interception",
        "campaign.supply_visibility_interception"
    )
    relief_and_tax_pressure_causality = @(
        "food_causality.relief_forecast_matches_applied_delta",
        "food_causality.relief_and_tax_pressure"
    )
    food_supply_order_diplomacy_coupling = @(
        "food.grain_shortage_hurts_supply_and_order",
        "food.relief_consumes_grain_and_reduces_unrest",
        "food.army_supply_draws_from_grain",
        "food.food_aid_improves_relation_with_cost"
    )
}

$Scenarios = @{}
foreach ($Scenario in $Report.scenarios) {
    $Scenarios[$Scenario.name] = $Scenario
}

foreach ($ScenarioName in $Required.Keys) {
    if (-not $Scenarios.ContainsKey($ScenarioName)) { Write-Error "Missing scenario in report: $ScenarioName" }

    $Scenario = $Scenarios[$ScenarioName]
    $Assertions = @{}
    foreach ($Assertion in $Scenario.assertions) {
        if ($Assertion.passed -ne $true) { Write-Error "Failed assertion in ${ScenarioName}: $($Assertion.id)" }
        $Assertions[$Assertion.id] = $Assertion
    }

    foreach ($AssertionId in $Required[$ScenarioName]) {
        if (-not $Assertions.ContainsKey($AssertionId)) { Write-Error "Missing required assertion in ${ScenarioName}: $AssertionId" }
        if ($Assertions[$AssertionId].passed -ne $true) { Write-Error "Required assertion did not pass in ${ScenarioName}: $AssertionId" }
    }
}
