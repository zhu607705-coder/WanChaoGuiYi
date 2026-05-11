# Strategy UI Reference Benchmarks

Date: 2026-05-05

This document records reusable UI patterns from successful strategy games for `万朝归一：九州帝业`. The goal is to borrow interaction logic, information hierarchy, and decision rhythm, not copyrighted layouts, art assets, logos, exact icons, or proprietary UI skins.

## Reference Principles

- Borrow patterns, not pixels.
- Keep the existing 56-region Jiuzhou map as the authoritative strategic structure.
- Every UI pattern must serve one of two loops: governance decision or war operation.
- Every displayed action must expose cause, cost, risk, expected result, and historical source.
- Small viewports must stay usable before visual polish counts as done.

## Case Matrix

| Source game | Proven pattern | What to adapt | What not to copy |
| --- | --- | --- | --- |
| Civilization VI | Map lenses and city/region banners make complex map state readable without opening full panels. | Add explicit map lens buttons for governance, war pressure, legitimacy, population, grain, risk, and terrain. Use small region badges for the highest-priority value only. | Do not copy hex tile UI, Civ icons, exact banner composition, or terminology. |
| Crusader Kings III | Map modes separate political, title, culture, faith, terrain, development, and government readings. | Convert our current mode button idea into map-mode clusters: governance, legal order, local power, terrain, war, economy. The selected lens should recolor existing 56 region surfaces. | Do not copy CK3 dynasty/title presentation or medieval European framing. |
| Stellaris | Top bar plus right outliner keeps global resources, time, alerts, fleets, planets, and claims always reachable. | Add a collapsible right/left "state outliner" for armies, active risks, pending policies, building queues, and battle reports. | Do not copy sci-fi iconography or galaxy layout. |
| Total War: Three Kingdoms | Campaign layer combines empire-building, statecraft, conquest, hero governance, and Chinese-inspired UI art. Diplomacy uses deal categories, numerical trade value, and quick-deal scanning. | For neighbor clicks, show quick diplomacy categories: appease, border control, spy, threaten, declare war. Add acceptability/cost preview before action. | Do not copy CA's art direction, UI frame silhouettes, portraits, or exact diplomacy text. |
| 率土之滨 | Large strategic sandbox emphasizes land ownership, march lines, supply/morale, alliance war reports, and wide strategic zoom. | War mode should prioritize front-line pressure: march line, ETA, supply state, contact risk, occupation cost, report feed. Overview zoom should reduce labels to ownership/risk/army clusters. | Do not copy NetEase map art, season UI, monetization UI, or exact line/icon style. |
| 三国志·战略版 | Open marching plus vision creates ambush, interception, and formation planning. | Add explicit visible/invisible army states, scouting radius, interception warning, and route-through-risk preview. | Do not copy its grid visuals or military command labels directly. |

## Governance UI Adaptation

The governance panel should become a Civilization-like decision surface:

1. Region title row: name, owner, status, current mode.
2. Decision strip: best action, expected gain/loss, action availability.
3. Risk cards: rebellion, annexation, local power, legitimacy, population pressure.
4. Economy strip: grain, tax, manpower, population, contribution rate.
5. Source drawer: causal explanation and historical references.

Acceptance:

- The player can answer "what should I do this turn and why" in the first visible block.
- Source/cause remains visible but visually secondary.
- Action preview must match post-click state deltas in PlayMode tests.

## War UI Adaptation

The war layer should become a 率土 / 三国志战略版-style pressure map:

1. Selected target: region highlight, owner, terrain, defenses.
2. Selected army: general, soldiers, morale, supply.
3. Route preview: path, ETA, supply loss, contact risk, predicted battle modifier.
4. Occupation preview: legitimacy cost, integration floor, tax/grain contribution penalty, rebellion/local power rise.
5. Report feedback: contact, battle, occupation, retreat, governance aftershock.

Acceptance:

- Before dispatch, the player sees whether the route is worth taking.
- After battle, the report explains both combat result and governance consequences.
- Labels and route pressure remain readable at 1600x900, 1280x720, and 1024x576.

## Map Mode Backlog

Recommended mode/lens buttons:

- Governance: stability, best action, policy/building availability.
- Risk: rebellion, annexation, local power, succession pressure.
- Economy: grain, tax, population, manpower contribution.
- Legitimacy: legal memory, occupation penalty, dynasty recognition.
- War: armies, routes, contact zones, target highlights.
- Terrain: pass, river, plain, basin, mountain, frontier.

## Next Implementation Order

1. Governance action forecast/result consistency.
2. Map lens bar using the existing 56 region surfaces.
3. War route pressure overlay refinement.
4. Collapsible outliner for armies, risks, policies, and reports.
5. Historical source gate unification across governance, diplomacy, and war.

## Source Notes

- Civilization VI lens and city-banner patterns are used as interaction inspiration only.
- Crusader Kings III map-mode taxonomy is used as map information architecture inspiration only.
- Stellaris top bar/outliner is used as global-state management inspiration only.
- Total War: Three Kingdoms diplomacy and Chinese campaign presentation are used as strategy-layer inspiration only.
- 率土之滨 / 三国志·战略版 war-map references are used as macro-war readability inspiration only.
