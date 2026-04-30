# RALPLAN Consensus Plan: Map-led War Rewrite Next Development

## Source Spec

- Deep interview spec: `.omc/specs/deep-interview-refactor-progress-next-development.md`
- User-selected direction: **multi-iteration executable roadmap**, **CLI/headless first**, **Unity validation second**, then execution via **Ralplan → Autopilot**.

## Consensus Note

The consensus workflow attempted Architect review through available agent models, but the current API token returned `403` for `opus`, `sonnet`, and `haiku` model-backed agents. To avoid blocking the user on provider availability, this plan incorporates the required RALPLAN-DR structure, explicit architectural counterargument, critic-style quality gates, and concrete acceptance criteria directly in the plan. Execution should still run full verification and reviewer gates in autopilot/Ralph.

## Requirements Summary

The current refactor checkpoint has already delivered and committed:

- Unity-free Domain Core under `WanChaoGuiYi/Assets/Scripts/Domain/`.
- Unity adapters for map-led movement, engagement, battle simulation, occupation, governance impact, and economy.
- Headless CLI harness under `tools/headless_runner/WanChaoGuiYiHeadless/`.
- Validation commands:
  - `python3 tools/validate_data.py`
  - `python3 tools/validate_domain_core.py`
  - `tools/run_headless_simulation.sh`
- Recent fixes for duplicate contact logs and attacker/defender semantics.

Next development must turn the single-lane headless smoke test into a reliable regression suite, then extend gameplay semantics, then validate in Unity.

## RALPLAN-DR Summary

### Principles

1. **CLI before Unity**: every gameplay logic change should first be provable through headless Domain execution.
2. **Domain purity is non-negotiable**: `WanChaoGuiYi/Assets/Scripts/Domain/` must remain Unity-free.
3. **Logs are product surface**: war logs are part of player comprehension and must be semantically asserted, not eyeballed.
4. **Small deterministic scenarios beat broad simulation**: prefer three deterministic pass/fail scenarios over one long probabilistic run.
5. **Unity validates integration, not core truth**: Unity Console/PlayMode should confirm adapters and scene wiring after Domain behavior is already stable.

### Decision Drivers

1. **Regression confidence**: prevent repeated breakage in movement → contact → battle → occupation/governance/economy sequence.
2. **User comprehension**: avoid contradictory logs like “defender won” followed by attacker occupation.
3. **Execution control**: keep the next implementation scoped enough for autopilot/Ralph to complete with verification.

### Viable Options

#### Option A — Headless verification first, gameplay second, Unity third (chosen)

**Pros**

- Builds on current working CLI harness.
- Fast feedback loop without needing Unity open.
- Keeps Domain/adapter boundary clean.
- Makes later Unity issues easier to isolate as integration problems.

**Cons**

- Risks delaying discovery of Unity serialization / scene reference problems.
- CLI tests may miss UI/event subscription issues.

#### Option B — Unity integration first

**Pros**

- Finds Console and scene binding errors immediately.
- Gives designer-visible proof sooner.

**Cons**

- Slower iteration loop.
- Harder to distinguish core logic failures from Unity adapter or scene setup failures.
- Current user explicitly selected “先 CLI 后 Unity”, so this conflicts with the chosen validation boundary.

#### Option C — Gameplay expansion before verification expansion

**Pros**

- More visible feature progress.
- Faster path to multi-army behavior.

**Cons**

- Expands behavior on top of only one smoke test.
- Increases chance of replaying the same duplicate-log/semantic mismatch mistakes.

### Strongest Counterargument Against Chosen Option

The strongest counterargument is that the codebase is a Unity game, so delaying Unity Console/PlayMode can let adapter and serialization failures accumulate while the CLI gives a false sense of safety. A headless runner can prove Domain behavior but cannot prove MonoBehaviour lifecycle, scene object references, event listener wiring, inspector fields, or actual UI feedback.

### Synthesis Path

Keep CLI/headless as the hard gate for Iterations 1 and 2, but add an explicit “Unity readiness checklist” to every iteration: no Unity namespaces in Domain, adapters compile by static inclusion where possible, and `project-development-report.md` records that Unity validation remains pending. Iteration 3 then becomes a focused Unity integration pass rather than a surprise cleanup phase.

## ADR

### Decision

Adopt a three-iteration execution plan:

1. Expand headless verification.
2. Expand map-war gameplay semantics under headless coverage.
3. Validate Unity integration and harden SDK/build workflow.

### Drivers

- Current CLI harness already runs and passes a single-lane scenario.
- The most recent bugs were semantic/logging regressions, which are best caught by deterministic log assertions.
- The user selected a multi-round roadmap and “先 CLI 后 Unity”.

### Alternatives Considered

- Unity-first validation.
- Gameplay-first expansion.
- One large milestone covering CLI, gameplay, and Unity all at once.

### Why Chosen

The chosen approach maximizes reliable forward progress: first stabilize the automated feedback loop, then add behavior under that loop, then validate Unity integration after Domain semantics are less volatile.

### Consequences

- Unity validation remains a known pending risk until Iteration 3.
- Headless scenarios must be designed carefully to avoid becoming brittle text snapshots.
- The harness becomes a first-class development surface and needs SDK policy cleanup.

### Follow-ups

- Decide `.NET 8 LTS` vs `.NET 10 local` vs multi-targeting.
- Add `.gitignore` rules if headless `bin/` or `obj/` artifacts can reappear.
- Consider Unity Test Framework integration after PlayMode validation succeeds.

## Implementation Plan

## Iteration 1 — Headless Verification Expansion

### Goal

Convert the existing single-lane smoke test into a small deterministic scenario suite.

### Files to inspect/update

- `WanChaoGuiYi/Assets/Scripts/Domain/Core/HeadlessSimulationRunner.cs`
- `tools/headless_runner/WanChaoGuiYiHeadless/Program.cs`
- `tools/run_headless_simulation.sh`
- `tools/validate_domain_core.py`
- `project-development-report.md`
- `.omc/progress.txt`
- Optional new script: `tools/verify_headless_war.sh`

### Story HV-001 — Multiple deterministic headless scenarios

Acceptance criteria:

- [ ] Add at least three named scenarios:
  - attacker wins and occupies target region;
  - defender wins and attacker retreats/routs;
  - reinforcement or multi-army membership changes an engagement.
- [ ] Each scenario returns a structured result with `scenarioName`, `passed`, `failureReason`, `turnsExecuted`, and relevant logs.
- [ ] `tools/run_headless_simulation.sh` runs all scenarios by default.
- [ ] CLI exits non-zero if any scenario fails.

### Story HV-002 — War-log semantic assertions

Acceptance criteria:

- [ ] Assert no duplicate `发生接敌` for unchanged engagement membership in a scenario turn.
- [ ] Assert battle winner wording matches occupation/retreat outcome.
- [ ] Assert expected phase order: movement → arrival → contact → battle → occupation/retreat → economy.
- [ ] Failure output names the missing, duplicate, or contradictory log pattern.

### Story HV-003 — Unified CLI verification gate

Acceptance criteria:

- [ ] Add `tools/verify_headless_war.sh`.
- [ ] It runs:
  - `python3 tools/validate_data.py`
  - `python3 tools/validate_domain_core.py`
  - `tools/run_headless_simulation.sh`
- [ ] It exits non-zero on any failure.
- [ ] `project-development-report.md` documents the command as the standard CLI-first gate.

### Iteration 1 verification

Run:

```bash
python3 tools/validate_data.py
python3 tools/validate_domain_core.py
tools/run_headless_simulation.sh
tools/verify_headless_war.sh
```

Expected:

- All gates pass.
- CLI output clearly lists each scenario and pass/fail status.

## Iteration 2 — Gameplay Behavior Expansion

### Goal

Add richer map-war behavior only after deterministic headless scenarios exist.

### Files to inspect/update

- `WanChaoGuiYi/Assets/Scripts/Domain/Map/MapCommandService.cs`
- `WanChaoGuiYi/Assets/Scripts/Domain/Military/DomainArmyMovementSystem.cs`
- `WanChaoGuiYi/Assets/Scripts/Domain/Military/DomainEngagementDetector.cs`
- `WanChaoGuiYi/Assets/Scripts/Domain/Military/DomainMapWarResolutionSystem.cs`
- `WanChaoGuiYi/Assets/Scripts/Domain/Military/DomainBattleSimulationSystem.cs`
- `WanChaoGuiYi/Assets/Scripts/Domain/Military/DomainOccupationSystem.cs`
- `WanChaoGuiYi/Assets/Scripts/Domain/Governance/DomainGovernanceImpactSystem.cs`
- `WanChaoGuiYi/Assets/Scripts/Domain/Economy/DomainEconomySystem.cs`
- Unity adapters in `WanChaoGuiYi/Assets/Scripts/Military/`, `Map/`, `Governance/`, `Economy/` as needed.

### Story GW-001 — Reinforcement semantics

Acceptance criteria:

- [ ] `MapCommandService.ReinforceArmy()` logs reinforcement intent with source, target, and route.
- [ ] Arriving reinforcement joins an existing engagement on the correct side.
- [ ] Re-detection after reinforcement publishes/logs only the membership change, not a duplicate unchanged contact.
- [ ] Headless scenario proves reinforcement changes side power or membership.

### Story GW-002 — Retreat semantics

Acceptance criteria:

- [ ] `ArmyTask.Retreat` can move while `engagementId` is set.
- [ ] Retreat route is explicit and logged.
- [ ] Retreating army clears engagement state only through a deterministic rule.
- [ ] Retreat does not immediately recreate the same stale engagement.
- [ ] Headless scenario covers retreat outcome.

### Story GW-003 — Occupation/governance/economy consistency

Acceptance criteria:

- [ ] Occupation updates runtime region state and legacy faction region ownership consistently.
- [ ] Newly occupied regions receive reduced tax/food contribution through runtime state.
- [ ] `DomainEconomySystem` uses runtime contribution multipliers.
- [ ] Headless assertion verifies occupation/governance/economy log order.

### Iteration 2 verification

Run the unified gate:

```bash
tools/verify_headless_war.sh
```

Additional static checks:

```bash
python3 tools/validate_domain_core.py
```

Expected:

- All Iteration 1 scenarios still pass.
- New reinforcement/retreat/occupation scenarios pass.
- Domain remains Unity-free.

## Iteration 3 — Unity Validation and Engineering Hardening

### Goal

After CLI confidence is stable, prove Unity integration and stabilize SDK/build workflow.

### Files to inspect/update

- `WanChaoGuiYi/Assets/Scripts/Core/GameManager.cs`
- Unity adapter scripts under:
  - `WanChaoGuiYi/Assets/Scripts/Military/`
  - `WanChaoGuiYi/Assets/Scripts/Economy/`
  - `WanChaoGuiYi/Assets/Scripts/Governance/`
- `tools/headless_runner/WanChaoGuiYiHeadless/WanChaoGuiYiHeadless.csproj`
- `tools/run_headless_simulation.sh`
- `.gitignore` if build artifacts are not already ignored
- `project-development-report.md`

### Story UE-001 — Unity Console compile validation

Acceptance criteria:

- [ ] Open the project in target Unity LTS.
- [ ] Unity Console shows zero C# compile errors.
- [ ] Any broken references caused by moved scripts are fixed or documented.
- [ ] `project-development-report.md` records Unity version and compile result.

### Story UE-002 — PlayMode single-lane smoke test

Acceptance criteria:

- [ ] `GameManager.RunSingleLaneWarSmokeTest()` or equivalent is invokable in PlayMode.
- [ ] Unity runtime logs match CLI phase order.
- [ ] Advancing one additional turn does not recreate a stale engagement.
- [ ] Result is recorded in `project-development-report.md`.

### Story UE-003 — Headless SDK and artifact policy

Acceptance criteria:

- [ ] Decide and document `.NET 8 LTS`, `.NET 10 local`, or multi-targeting policy.
- [ ] `WanChaoGuiYiHeadless.csproj` reflects that policy.
- [ ] `tools/run_headless_simulation.sh` gives actionable SDK/runtime errors.
- [ ] `bin/` and `obj/` artifacts under `tools/headless_runner/` remain untracked.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Headless tests become text-snapshot brittle | Valid changes fail tests | Assert semantic phases and key contradictions, not exact full log dumps |
| CLI pass hides Unity adapter failures | Unity integration delayed | Track Unity validation explicitly as Iteration 3, not “done” after CLI |
| Scenario setup mutates production data assumptions | Tests become unrealistic | Build scenarios by modifying runtime state in runner, not persistent JSON, unless contract requires data change |
| Reinforcement/retreat logic creates stale engagement IDs | Repeated battles or stuck armies | Add assertions for no stale engagement recreation after next turn |
| SDK target mismatch blocks teammates | Harness unusable | Decide target policy and improve script diagnostics |

## Verification Matrix

| Layer | Command / Action | Required by |
|-------|------------------|-------------|
| JSON data | `python3 tools/validate_data.py` | Every iteration touching data or data contracts |
| Domain purity | `python3 tools/validate_domain_core.py` | Every Domain or adapter change |
| Headless scenarios | `tools/run_headless_simulation.sh` | Iterations 1-2 |
| Unified CLI gate | `tools/verify_headless_war.sh` | After Iteration 1 |
| Unity compile | Unity Console zero errors | Iteration 3 |
| Unity runtime | PlayMode smoke test | Iteration 3 |

## Execution Guidance for Autopilot

1. Start with Iteration 1 only; do not implement all three iterations in one batch.
2. Keep changes small and commit-worthy by layer:
   - runner/scenario model;
   - assertions;
   - unified verification script;
   - documentation/progress update.
3. Run verification after each story if practical.
4. Do not commit unless explicitly asked by the user during execution mode.
5. If Unity is unavailable in the environment, record it as a known limitation and complete CLI gates first.

## Changelog Applied During Consensus

- Added explicit counterargument that CLI can hide Unity adapter/scene failures.
- Added synthesis path: CLI as hard gate, Unity as focused integration pass.
- Split the roadmap into three iterations with acceptance criteria per story.
- Added a verification matrix and risk table.
- Added SDK/artifact policy as a concrete engineering story.
