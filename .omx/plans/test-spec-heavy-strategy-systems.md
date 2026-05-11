# Test Spec: Heavy Strategy Systems Borrowing Phase

## Metadata

- Date: 2026-05-05
- PRD: `.omx/plans/prd-heavy-strategy-systems.md`
- Scope: Approved verification contract for H0-H6 execution.
- Execution status: H0-H6 full closure hard gate completed on 2026-05-05; see `project-development-report.md`.

## Baseline Commands

Run after each implemented stage unless explicitly narrowed:

```powershell
dotnet build "My project\WanChaoGuiYi.Runtime.csproj"
dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"
python tools\validate_data.py
python tools\unity\preflight_without_unity.py
python tools\validate_domain_core.py
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"
```

Graphical VisualSmoke must only run in a Unity/Tuanjie environment with a graphics device. Temporary `unity-*.png` screenshots and visual project copies must be deleted after use.

## H0: Data Contract And Source Gate Expansion

Required proof:

- New fields are documented in `docs/data-contract.md`.
- New fields either exist in data or have deterministic defaults.
- `tools/validate_data.py` verifies required fields and required historical sources.
- Legacy/current 56 region data still parses.

Suggested checks:

- Data validation count remains `regions=56 map_region_shapes=56`.
- A negative validation case can detect missing required source/action forecast fields.

Commands:

```powershell
python tools\validate_data.py
python tools\unity\preflight_without_unity.py
dotnet build "My project\WanChaoGuiYi.Runtime.csproj"
```

## H1: Region Specialization And Forecasted Governance

Required proof:

- All 56 regions resolve to a specialization.
- Representative regions differ by terrain/history:
  - Guanzhong: capital / military / legitimacy leaning.
  - Jiangnan or Jiangdong: grain / tax / population leaning.
  - Hexi: border / supply / corridor leaning.
  - Bashu: grain / basin defense leaning.
  - Liaodong or Xiyu: frontier / military / border leaning.
- Governance forecast matches execution for pacify, tax pressure, conscription, and one building action.
- UI displays recommendation reason and source.

Suggested PlayMode assertions:

- `RegionPanel` contains specialization label.
- `GovernanceOverviewText` shows forecast.
- `GovernanceSourceText` shows why/source.
- Button state, recommended action, and forecast remain aligned.

Suggested headless assertions:

- Forecast object before action equals actual state delta after action.
- No action directly violates causality: extra tax cannot raise local acceptance without a relief/source-backed mechanism.

## H2: Occupation Control Chain

Required proof:

- Occupation creates `newly_attached` or equivalent initial control stage.
- New region contribution stays below normal until control progresses.
- Military governance suppresses rebellion but costs grain/money/supply and may reduce local acceptance or legitimacy.
- Pacification and household registration improve contribution and control stage.
- Battle report and region panel show control stage.

Suggested headless scenarios:

- `occupation_starts_newly_attached`
- `military_governance_suppresses_but_costs`
- `pacify_advances_control_stage`
- `register_households_restores_contribution`

Suggested PlayMode assertions:

- Occupied selected region shows control stage and next required action.
- Outliner or region panel marks new occupation as a priority.

## H3: Connected Campaigning, Supply Nodes, Vision, And Interception

Required proof:

- Dispatch from disconnected rear regions is rejected with visible reason.
- Supply node or prepared staging region allows valid campaign projection.
- Route preview shows supply loss, terrain/contact risk, and interception warning.
- Hidden/unscouted target does not reveal full hostile army details.
- Scouting changes visibility and route forecast.
- Interception route produces an encounter or risk result on turn advance.

Suggested headless scenarios:

- `disconnected_dispatch_rejected`
- `supply_node_extends_campaign_range`
- `unscouted_route_hides_enemy_detail`
- `scouting_reveals_route_risk`
- `hostile_adjacent_route_interception`

Suggested PlayMode assertions:

- War mode source/forecast text shows visibility and interception.
- War route label changes when scouted.
- Map overlay remains non-overlapping at 1600x900, 1280x720, 1024x576.

## H4: Food, Supply, Public Order, And Diplomacy Coupling

Required proof:

- Grain shortage lowers army supply recovery or route readiness.
- Grain shortage raises unrest/local power or lowers local acceptance.
- Relief consumes grain and improves rebellion/local acceptance.
- Supplying army consumes or reserves grain.
- Food aid changes diplomacy and records source.

Suggested headless scenarios:

- `grain_shortage_hurts_supply_and_order`
- `relief_consumes_grain_and_reduces_unrest`
- `army_supply_draws_from_grain`
- `food_aid_improves_relation_with_cost`

Suggested UI assertions:

- Court/outliner shows grain pressure.
- Region panel shows relief forecast.
- War forecast shows supply constrained by grain.

## H5: Map Lenses And Outliner

Required proof:

- Lens buttons exist and switch visible map state.
- Each lens uses existing 56 region surfaces, not substitute geometry.
- Outliner sections exist and are collapsible.
- Outliner entries click-select the corresponding region, army, or report.
- Small viewport still keeps core controls usable.

Suggested lenses:

- Governance.
- Risk.
- Economy.
- Legitimacy.
- War.
- Terrain.

Suggested PlayMode assertions:

- Switching each lens changes `MapLensMode` or equivalent state.
- At least one region surface visually changes or has a lens marker.
- Outliner click sets `SelectionContext.selectedRegionId` or selected army.
- 1024x576 fit/non-overlap assertions cover outliner, HUD, region panel, and attack/governance controls.

## H6: Integration Closure And Visual Evidence

Required screenshot contract additions:

- `unity-governance-forecast.png`
- `unity-occupation-chain.png`
- `unity-war-route-risk.png`
- `unity-map-lens-risk.png`
- `unity-outliner.png`

Required proof:

- Existing 7 VisualSmoke screenshots continue passing.
- New screenshots are non-empty, expected resolution, and show the intended state.
- Temporary screenshots are deleted after validation unless deliberately copied into `.outputs/user-preview/`.
- `project-development-report.md` records scope, verification, and remaining risks.

## Final Closure Gate

The heavy systems phase is complete only when:

- H0-H6 acceptance criteria are met and verified against fresh build, headless, PlayMode, and graphical VisualSmoke evidence.
- Full baseline commands pass.
- No temporary `unity-*.png` remains under `.outputs/visual`.
- No `.outputs/tuanjie/visual-project-copy` or `.outputs/tuanjie/visual-preview-copy` remains.
- Code review finds no blocking issues.
- The report identifies residual design risks and next playable focus.
