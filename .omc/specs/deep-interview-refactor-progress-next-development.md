# Deep Interview Spec: 后续开发与当前重构进度执行型 PRD

## Metadata

- Interview ID: deep-interview-refactor-progress-next-development-2026-04-30
- Rounds: 4
- Final Ambiguity Score: 14%
- Type: brownfield
- Generated: 2026-04-30
- Threshold: 20%
- Status: PASSED

## Clarity Breakdown

| Dimension | Score | Weight | Weighted |
|-----------|-------|--------|----------|
| Goal Clarity | 0.90 | 0.35 | 0.315 |
| Constraint Clarity | 0.84 | 0.25 | 0.210 |
| Success Criteria Clarity | 0.86 | 0.25 | 0.215 |
| Context Clarity | 0.88 | 0.15 | 0.132 |
| **Total Clarity** | | | **0.872** |
| **Ambiguity** | | | **12.8%** |

## Goal

Produce an execution-ready, multi-iteration PRD for the map-led war rewrite after the current refactor checkpoint. The PRD must both summarize the current refactor state and define the next development route in executable stages: first strengthen CLI/headless verification, then expand map-war gameplay behaviors, then move into Unity Console/PlayMode validation and project engineering hardening.

## Current Refactor Progress

### Completed and committed

Recent commit stack:

1. `f827925 docs: 记录地图主导战争契约`
   - Updated `docs/data-contract.md` and `project-development-report.md` with map-led war runtime contract and verification notes.
2. `184c0df feat: 抽离地图战争 Domain Core`
   - Added Unity-free Domain Core under `WanChaoGuiYi/Assets/Scripts/Domain/`.
   - Key symbols: `WorldState`, `MapState`, `MapCommandService`, `MapQueryService`, `DomainArmyMovementSystem`, `DomainEngagementDetector`, `DomainBattleSimulationSystem`, `DomainMapWarResolutionSystem`, `DomainOccupationSystem`, `DomainGovernanceImpactSystem`, `DomainEconomySystem`, `HeadlessSimulationRunner`.
3. `257c506 feat: 串联地图主导战争闭环`
   - Connected Unity adapters and runtime wiring.
   - Key files: `GameManager.cs`, `GameStateFactory.cs`, `ArmyMovementSystem.cs`, `MapWarResolutionSystem.cs`, `BattleSimulationSystem.cs`, `OccupationSystem.cs`, `GovernanceImpactSystem.cs`, `EconomySystem.cs`, `BattleResolver.cs`.
4. `bf7041b test: 添加地图战争 headless 验收入口`
   - Added CLI harness under `tools/headless_runner/WanChaoGuiYiHeadless/`.
   - Added `tools/run_headless_simulation.sh` and `tools/validate_domain_core.py`.
5. `e96ee11 chore: 更新地图战争闭环 OMC 进度`
   - Updated `.omc/prd.json`, `.omc/progress.txt`, consensus plan, and prior deep-interview spec.

### Verified at this checkpoint

Commands already run successfully:

```bash
python3 tools/validate_data.py
python3 tools/validate_domain_core.py
tools/run_headless_simulation.sh
```

Known latest headless result:

- `passed=True`
- `turnsExecuted=1`
- Single-lane war loop produces: initial army deployment → movement → arrival → contact → battle resolution → loser retreat/rout or occupation path → economy settlement.

### Recently fixed review issues

1. Duplicate contact log:
   - Symptom: `longxi发生接敌` appeared twice because movement and war resolution both ran detection.
   - Fix: `DomainEngagementDetector` now publishes contact events/logs only when engagement membership is new.
2. Attacker/defender semantic mismatch:
   - Symptom: log said defender won while Qin occupied `longxi`.
   - Fix: movement arrival passes initiating army id to `DetectRegion(..., army.id)`, and attacker seed is based on initiating/moving army rather than first army in region list.

## Constraints

- The next work must be multi-iteration, not a single oversized implementation batch.
- Verification order is **CLI/headless first, Unity second**.
- The current headless single-lane pass is not enough to claim the full war rewrite is finished.
- Unity Console/PlayMode validation remains required before product-level completion, but is not the first hard gate for the next iteration.
- Keep Domain Core Unity-free. Domain files must not introduce `UnityEngine`, `MonoBehaviour`, `SerializeField`, `GetComponent`, `gameObject`, or `Mathf.`.
- Do not reintroduce direct ownership mutation through legacy `BattleResolver` as the default path. Region ownership changes should flow through map-led occupation logic.
- Avoid committing generated `bin/` or `obj/` artifacts from the headless runner.
- Preserve existing JSON assets; do not add persistent JSON fields unless `docs/data-contract.md` is updated first.

## Non-Goals

- Do not redesign the whole game architecture again.
- Do not add multiplayer, diplomacy expansion, real-time tactics, or global map scope.
- Do not treat Unity visual polish as part of the immediate next CLI-first iteration.
- Do not replace JSON persistence with SQLite in this phase.
- Do not make the headless harness depend on Unity assemblies.

## Execution PRD: Multi-Round Roadmap

### Iteration 1 — Headless Verification Expansion

Objective: Turn the current single-lane smoke test into a small but credible regression suite for the map-led war loop.

#### User Story HV-001: Multiple deterministic headless scenarios

As a developer, I want multiple deterministic headless scenarios so that war-loop regressions are caught before opening Unity.

Acceptance criteria:

- [ ] `HeadlessSimulationRunner` or adjacent runner supports at least three scenarios:
  - attacker wins and occupies target region;
  - defender wins and attacker retreats/routs;
  - reinforcement or multiple armies join an existing engagement.
- [ ] CLI output identifies scenario name, pass/fail status, and failure reason.
- [ ] Existing `tools/run_headless_simulation.sh` can run all scenarios by default.
- [ ] All scenarios pass with current data.

#### User Story HV-002: Log semantic assertions

As a designer, I want the smoke tests to assert readable war-log semantics so that contradictions are caught automatically.

Acceptance criteria:

- [ ] Tests assert no duplicate `发生接敌` log for unchanged engagement membership in a single turn.
- [ ] Tests assert battle winner wording matches subsequent occupation/retreat outcome.
- [ ] Tests assert each scenario includes expected log phases in order.
- [ ] Failure output includes missing or contradictory log phrases.

#### User Story HV-003: Domain boundary regression gate

As a maintainer, I want a single command to verify data, Domain purity, and headless scenarios so that future changes have a reliable pre-Unity gate.

Acceptance criteria:

- [ ] Add or update a script such as `tools/verify_headless_war.sh`.
- [ ] The script runs:
  - `python3 tools/validate_data.py`
  - `python3 tools/validate_domain_core.py`
  - `tools/run_headless_simulation.sh`
- [ ] The script exits non-zero if any gate fails.
- [ ] Documentation/progress notes mention the command as the standard CLI-first gate.

### Iteration 2 — Gameplay Behavior Expansion

Objective: Move from one adjacent attack to more realistic map-war behaviors while keeping headless verification first.

#### User Story GW-001: Reinforcement semantics

As a player, I want reinforcing armies to join an existing battle when they arrive so that multi-army conflict feels coherent.

Acceptance criteria:

- [ ] `MapCommandService.ReinforceArmy()` logs intent clearly.
- [ ] Arriving reinforcement merges into existing engagement without duplicate contact spam.
- [ ] Headless scenario proves reinforcement changes side membership and battle power.
- [ ] Engagement payload includes updated attacker/defender army ids.

#### User Story GW-002: Retreat semantics

As a player, I want retreating armies to disengage predictably rather than teleport or remain stuck in combat.

Acceptance criteria:

- [ ] `ArmyTask.Retreat` is allowed to move even when `engagementId` is set.
- [ ] Retreat route and log explain where the army goes.
- [ ] Retreating army does not immediately trigger a new engagement in the same region.
- [ ] Headless scenario covers retreat before or after battle resolution.

#### User Story GW-003: Occupation and governance feedback consistency

As a player, I want new occupied regions to contribute reduced tax/food until integrated so that expansion has a visible governance cost.

Acceptance criteria:

- [ ] Occupation updates runtime region owner/status and legacy faction region lists consistently.
- [ ] Governance impact applies tax/food contribution penalties to newly occupied region.
- [ ] `DomainEconomySystem` reads runtime contribution multipliers for income.
- [ ] Headless log shows occupation/governance/economy sequence in order.

### Iteration 3 — Unity Validation and Engineering Hardening

Objective: After CLI confidence is stable, validate Unity integration and harden the workflow.

#### User Story UE-001: Unity Console compile validation

As a developer, I want the refactor to compile in Unity so that Domain extraction did not break scene/runtime code.

Acceptance criteria:

- [ ] Open project in target Unity LTS.
- [ ] Unity Console has zero C# compile errors.
- [ ] Any moved scripts/adapters retain valid references or have clear migration notes.
- [ ] `project-development-report.md` records Unity version and result.

#### User Story UE-002: PlayMode single-lane manual smoke test

As a designer, I want to trigger the war smoke test in Unity so that the CLI behavior is visible in the actual game runtime.

Acceptance criteria:

- [ ] `GameManager.RunSingleLaneWarSmokeTest()` or equivalent can be invoked in PlayMode.
- [ ] Log sequence matches CLI expectations: movement → arrival → contact → battle → occupation/retreat → economy.
- [ ] Advancing an additional turn does not recreate the same stale engagement.
- [ ] Result is recorded in `project-development-report.md`.

#### User Story UE-003: Headless SDK target policy

As a maintainer, I want the headless runner target framework to be deliberate so that teammates and CI can run it reliably.

Acceptance criteria:

- [ ] Decide whether project standard is `.NET 8 LTS`, `.NET 10 local`, or multi-targeting.
- [ ] Update `WanChaoGuiYiHeadless.csproj` accordingly.
- [ ] `tools/run_headless_simulation.sh` prints an actionable message if required SDK/runtime is missing.
- [ ] Build artifacts remain untracked.

## Assumptions Exposed & Resolved

| Assumption | Challenge | Resolution |
|------------|-----------|------------|
| The next output should be a report | User selected “执行型 PRD” | Produce an execution-ready PRD, not just a progress summary |
| Next step could be validation, gameplay, or engineering | User selected “阶段组合” | Use staged roadmap: verification → gameplay → engineering |
| Unity validation might be the immediate hard gate | User selected “先 CLI 后 Unity” | CLI/headless is the next hard gate; Unity follows after CLI confidence |
| PRD might cover one small iteration | User selected “多轮路线” | Write a multi-iteration roadmap with executable stories |

## Technical Context

Relevant repo evidence:

- `WanChaoGuiYi/Assets/Scripts/Domain/World/WorldState.cs`
  - Defines `MapState`, `ArmyRuntimeState`, `EngagementRuntimeState`, `ArmyTask`, `EngagementPhase`, `OccupationStatus`.
- `WanChaoGuiYi/Assets/Scripts/Domain/Military/DomainEngagementDetector.cs`
  - Detects hostile armies in a region, assigns attacker/defender membership, publishes `ContactDetected` and `EngagementStarted`.
- `WanChaoGuiYi/Assets/Scripts/Domain/Military/DomainArmyMovementSystem.cs`
  - Advances map-led armies and now passes initiating army id into engagement detection.
- `WanChaoGuiYi/Assets/Scripts/Domain/Military/DomainMapWarResolutionSystem.cs`
  - Runs detection, resolves formed engagements, applies occupation, loser retreat/rout, and clears resolved engagement state.
- `WanChaoGuiYi/Assets/Scripts/Domain/Economy/DomainEconomySystem.cs`
  - Handles economy settlement using runtime contribution multipliers.
- `tools/headless_runner/WanChaoGuiYiHeadless/Program.cs`
  - CLI entry that loads JSON data and runs headless simulation.
- `tools/validate_domain_core.py`
  - Static boundary check for Domain purity and adapter delegation.

## Ontology (Key Entities)

| Entity | Type | Fields | Relationships |
|--------|------|--------|---------------|
| Execution PRD | core deliverable | staged stories, acceptance criteria, verification commands | summarizes refactor progress and drives future development |
| Refactor Progress | status artifact | commit stack, completed systems, known verification results | informs Execution PRD scope |
| Future Development | roadmap | verification, gameplay expansion, Unity validation | decomposed into phased roadmap |
| Map-led War Loop | core domain | movement, contact, battle, occupation, governance, economy | verified by headless harness and later Unity |
| Headless Harness | verification system | CLI runner, non-Unity repository, scenario output | tests Domain Core without Unity |
| Phased Roadmap | planning structure | iteration 1, iteration 2, iteration 3 | organizes Execution PRD stories |
| Validation Boundary | constraint | CLI-first, Unity-second | determines done criteria per iteration |

## Ontology Convergence

| Round | Entity Count | New | Changed | Stable | Stability Ratio |
|-------|--------------|-----|---------|--------|-----------------|
| 1 | 5 | 5 | - | - | N/A |
| 2 | 6 | 1 | 0 | 5 | 83% |
| 3 | 7 | 1 | 0 | 6 | 86% |
| 4 | 7 | 0 | 0 | 7 | 100% |

## Interview Transcript

<details>
<summary>Full Q&A (4 rounds)</summary>

### Round 1

**Q:** 你这次最想得到的最终产物是什么？

Options included: 重构体检报告、后续开发路线图、两者合并、执行型 PRD.

**A:** 执行型 PRD

**Ambiguity:** 47%

### Round 2

**Q:** 这个执行型 PRD 的下一轮执行重点应该是什么？

Options included: 验证优先、玩法优先、工程优先、阶段组合.

**A:** 阶段组合

**Ambiguity:** 32%

### Round 3

**Q:** 你希望下一阶段的验收边界是什么？

Options included: Headless 为准、Unity 为准、先 CLI 后 Unity、暂不碰 Unity.

**A:** 先 CLI 后 Unity

**Ambiguity:** 20%

### Round 4

**Q:** 这个执行型 PRD 应该覆盖多长的执行周期？

Options included: 单轮 Ralph、多轮路线、演示里程碑、只做 backlog.

**A:** 多轮路线

**Ambiguity:** 12.8%

</details>
