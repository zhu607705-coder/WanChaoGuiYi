# PRD: Heavy Strategy Systems Borrowing Phase

## Metadata

- Date: 2026-05-05
- Trigger: User selected option C, heavy borrowing of successful strategy-game gameplay practices.
- Context snapshot: `.omx/context/heavy-strategy-systems-plan-20260505T0218Z.md`
- Test spec: `.omx/plans/test-spec-heavy-strategy-systems.md`
- Reference benchmark: `docs/strategy-ui-reference-benchmarks.md`
- Scope: Approved for direct H0-H6 execution by the user.
- Execution status: H0-H6 full closure hard gate completed on 2026-05-05; see `project-development-report.md`.

## Intent

Turn the current dual-loop strategy map from a readable prototype into a deeper playable strategy system. The project should borrow proven gameplay patterns from successful strategy games, but express them through the existing Chinese historical 56-region Jiuzhou map and the project's historical-source / causal-consistency rules.

Borrowed design logic:

- From Civilization VI: region specialization, predictable action effects, yield readability.
- From 率土之滨: connected campaigning, march-line pressure, occupation not equal to instant full control.
- From 三国志·战略版: route risk, visibility, interception, pre-battle judgment.
- From Total War: Three Kingdoms: food, army supply, order, and diplomacy as coupled systems.
- From CK3: control, popular acceptance, legitimacy memory, and long-tail local resistance.
- From Stellaris: an outliner that turns map complexity into a short actionable queue.

## Non-Goals

- Do not replace the 56-region map with hexes or square grids.
- Do not copy UI skins, icons, art, exact names, numbers, monetization structures, or proprietary layouts from reference games.
- Do not build global civilizations, multiplayer, all emperors, or real-time tactical battles in this phase.
- Do not make a full 3D terrain rebuild before the systemic loops are testable.
- Do not add external dependencies or imported assets without separate license screening.

## Existing Foundation

Existing systems already provide useful anchors:

- Regions have terrain, land structure, customs, legitimacy memory, neighbors, integration, rebellion risk, annexation pressure, local power, and tax/food contribution.
- Armies have route, task, morale, soldiers, supply, and engagement state.
- Selection already flows through `MapInteractionMode` and `SelectionContext`.
- War routes already show supply pressure; battle reports already include supply and occupation consequences.
- VisualSmoke and PlayMode tests already guard core map/UI workflows and cleanup.

This phase should extend those systems instead of replacing them.

## Stage H0: Data Contract And Source Gate Expansion

Goal: Define the new gameplay vocabulary before implementing mechanics.

New concepts:

- `regionSpecialization`: grain, military, tax, border, legitimacy, culture, capital.
- `controlStage`: controlled, newly_attached, military_governed, pacified, registered.
- `localAcceptance`: local recognition of rule; distinct from short-term rebellion risk.
- `visibilityState`: known, scouted, hidden.
- `interceptionRisk`: risk that a route can be challenged by hostile or frontier forces.
- `supplyNode`: whether a region can project supply / staging range.
- `actionForecast`: cost, immediate delta, delayed delta, risk delta, historical source.

Scope:

- Update `docs/data-contract.md`.
- Extend data models minimally with serializable fields and defaults.
- Add validation rules so required new fields are present or defaulted deterministically.
- Keep legacy saves/data loadable by using defaults where possible.

Acceptance:

- Data validation passes for all 56 regions.
- Missing required source references fail validation where the new action system needs them.
- PRD/test docs identify which fields are data-authored and which are derived.

## Stage H1: Region Specialization And Forecasted Governance

Goal: Make each region feel like a historically grounded strategic place, not just a resource bundle.

Borrowing target:

- Civilization VI-style readable specialization and expected yields.
- CK3-style local acceptance and legitimacy memory.
- Total War-style food/order coupling.

Scope:

- Create a deterministic specialization resolver using terrain, population, land structure, customs, legitimacy memory, and historical layer tags.
- Add governance action forecasts for pacify, build, tax pressure, conscription, and reform/registration.
- Show expected effects before action: money, grain, population, manpower, integration, local acceptance, rebellion, local power, legitimacy.
- Ensure action forecast and post-action state delta match in tests.
- Show recommended action from the same scoring function used by the UI.

Example:

- Guanzhong trends toward capital / military / legitimacy.
- Jiangnan trends toward grain / tax / population.
- Hexi trends toward border / supply / corridor.

Acceptance:

- At least 56 regions receive a non-empty specialization.
- At least 6 representative regions show plausible different recommendations.
- Forecasted pacify/build/tax/conscription deltas match post-click state changes.
- UI can explain why a recommendation was chosen.

## Stage H2: Occupation Control Chain

Goal: Make conquest create governance work instead of instant full profit.

Borrowing target:

- 率土 connected conquest pressure.
- CK3 control / local recognition.
- Total War post-war order costs.

Control chain:

1. `newly_attached`: nominal ownership, very low contribution, high local resistance.
2. `military_governed`: army or garrison suppresses revolt but costs food/money and legitimacy.
3. `pacified`: rebellion and local power lower, contribution still capped.
4. `registered`: taxation, households, corvee, and manpower normalized.
5. `controlled`: normal domestic governance.

Scope:

- Map existing `OccupationStatus.Occupied` into a richer control-stage model without breaking current war flow.
- Add actions that advance the chain: military govern, pacify, register households.
- Garrison / army presence can accelerate control but consumes supply/grain and can hurt local acceptance if overused.
- New control stage must be visible in region panel, outliner, battle report, and map lens.

Acceptance:

- Occupying a region does not produce full tax/grain/manpower immediately.
- Advancing the control chain changes contribution and risks in plausible directions.
- Heavy military governance suppresses rebellion but can reduce local acceptance or legitimacy.
- Headless scenario proves control chain progression and costs.

## Stage H3: Connected Campaigning, Supply Nodes, Vision, And Interception

Goal: Make war preparation a real decision before automatic battle resolution.

Borrowing target:

- 率土: connected front and march-line pressure.
- 三国志·战略版: route risk, vision, interception, positioning.
- Total War: food/supply as a strategic constraint.

Scope:

- Dispatch requires adjacency, controlled supply node, or explicitly prepared staging region.
- Route forecast includes distance, terrain, supply consumption, contact risk, interception risk, and battle supply modifier.
- Add deterministic first version of visibility:
  - Own/friendly adjacent regions are visible.
  - Scouted regions reveal hostile armies and route risk.
  - Hidden hostile armies produce unknown-risk warnings rather than full certainty.
- Add deterministic first version of interception:
  - Hostile army or frontier region adjacent to route can create interception risk.
  - Interception is not real-time; it resolves on turn advance or route step.
- Route preview should explain risk before dispatch.

Acceptance:

- Non-connected distant dispatch is rejected with reason.
- Scouting changes a target from unknown-risk to known-risk.
- A route through hostile-adjacent terrain shows interception warning.
- Headless scenarios cover safe route, low-supply route, hidden-risk route, and interception route.
- War map labels remain readable at required viewports.

## Stage H4: Food, Supply, Public Order, And Diplomacy Coupling

Goal: Make food a strategic system rather than a passive resource.

Borrowing target:

- Total War: food affects armies, cities/order, and diplomacy.
- Civilization-like forecasted consequences.

Scope:

- Army supply draws from grain and supply nodes.
- Grain shortage reduces army supply replenishment and raises unrest / local power.
- Grain surplus can be used for relief, frontier provisioning, or diplomacy.
- Border control, relief, and diplomatic aid must have visible historical source notes.
- UI shows food pressure in court, outliner, region panel, and war route forecasts.

Acceptance:

- Grain shortage produces a negative governance and army effect.
- Relief consumes grain and reduces rebellion or improves local acceptance.
- Supplying an army consumes or reserves grain.
- Diplomatic food aid changes relation and local cost.
- Severe causality rule holds: extracting more without cost cannot directly increase acceptance.

## Stage H5: Map Lenses And Outliner

Goal: Let the player understand the whole country without clicking every region.

Borrowing target:

- Civ/CK3 map lenses.
- Stellaris outliner.

Map lenses:

- Governance: best action and control stage.
- Risk: rebellion, local power, annexation, succession-adjacent instability.
- Economy: grain, tax, manpower, contribution.
- Legitimacy: local acceptance, legal memory, occupation penalty.
- War: armies, routes, supply nodes, contested regions.
- Terrain: terrain class, corridor/pass/river/frontier.

Outliner sections:

- Critical regions.
- New occupation chain.
- Marching armies.
- Low-supply armies.
- Pending policy/building/governance actions.
- Recent battle reports.
- Imminent events.

Acceptance:

- Each lens recolors or marks the existing 56 region surfaces.
- Outliner entries click-select the matching region/army/report.
- The player can identify this turn's top 3 problems without manually scanning all regions.
- 1024x576 viewport still keeps mode controls, outliner collapse, and attack/governance actions usable.

## Stage H6: Integration Closure And Visual Evidence

Goal: Prove the heavy systems are playable, explainable, and visually stable.

Scope:

- Update VisualSmoke scenarios to include at least one governance forecast, one occupation-chain region, one route-risk preview, one outliner state, and one map lens.
- Update `project-development-report.md`.
- Clean temporary screenshots and visual project copies.
- Run the full baseline verification.

Acceptance:

- Full baseline commands pass.
- Visual preview shows governance forecast and outliner without overlaps.
- No `.outputs/visual/unity-*.png` residue.
- No `.outputs/tuanjie/visual-project-copy` residue.
- No `.outputs/tuanjie/visual-preview-copy` residue.

## ADR

Decision: Implement heavy gameplay borrowing as staged extensions of the existing dual-loop system.

Drivers:

- The user chose depth over UI-only polish.
- Current code already has map, region state, occupation, army route, supply, and UI anchors.
- Staged delivery prevents a large untestable redesign.

Alternatives:

- UI-only polish: rejected by user because gameplay practice should also be borrowed.
- Full 3D Civ-like rebuild: deferred because it would not solve causal/system depth first.
- Directly clone reference-game mechanics: rejected because the project needs Chinese historical causality and its own map structure.

Consequences:

- More data fields and tests are required.
- Region and army state become richer; save compatibility needs defaults.
- UI must expose explanations, not only numbers.

## Recommended Execution Order

1. H0 data contract.
2. H1 region specialization and governance forecast.
3. H2 occupation control chain.
4. H3 supply / vision / interception.
5. H4 food-system coupling.
6. H5 map lenses and outliner.
7. H6 visual closure.

## Approval And Execution Status

- User approved direct H0-H6 execution.
- H0-H6 now use the full closure status rather than the earlier initial-closure wording.
- Current residual work belongs to post-closure optimization: governance UI componentization, stronger war pressure presentation, 2.5D/3D terrain expression, outliner prioritization, and map feel/performance tuning.
