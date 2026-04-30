# RALPLAN Consensus Plan: 完整架构与地图主导战争闭环重写

## Metadata
- Source spec: `.omc/specs/deep-interview-complete-architecture-map-led-war-rewrite.md`
- Mode: consensus / deliberate
- Generated: 2026-04-29
- Status: APPROVED_FOR_EXECUTION
- Note: Planner/Architect/Critic subagents could not run because the current token has no access to their configured models. This plan applies the ralplan structure in-session and records the architectural critique directly.

## RALPLAN-DR Summary

### Principles
1. **地图是战争状态唯一权威来源**：军队位置、路线、接敌、战斗、占领、围攻、增援、撤退都必须可从地图状态查询。
2. **保数据，重写运行层**：现有 JSON 是资产，不是运行架构的约束；C# 代码允许全量重写。
3. **先闭环，再扩展**：第一阶段只围绕地图主导战争闭环建立完整骨架，其它系统围绕它接入。
4. **所有机制必须可被 UI 解释**：事件、日志、地图刷新、战报要告诉用户“发生了什么、为什么、下一步能做什么”。
5. **数据契约先行**：任何新字段先进入 `docs/data-contract.md`，再迁移 JSON，再写 C# 读取和验证。

### Decision Drivers
1. **用户体验目标**：玩家在大地图上直接理解行军、接敌、作战、占领，而不是跳进割裂 session。
2. **架构可持续性**：当前 mimo 代码补丁式修复成本高，必须收敛到清晰状态模型。
3. **验证可执行性**：重写必须能通过自动数据验证、核心闭环测试和 Unity 编译验证。

### Viable Options

#### Option A — 地图主导状态模型（Chosen）
- **做法**：建立 `WorldState / MapState / ArmyState / WarState / BattleState / OccupationState`，地图是所有战争状态入口；战斗由地图接敌事件生成，结果回写地图与治理。
- **优点**：最符合用户目标；未来围攻、增援、撤退、补给、治理联动都自然接入。
- **缺点**：初期重写范围大，需要先设计状态边界和迁移层。

#### Option B — 战争系统主导，地图只做显示
- **做法**：战争系统接管行军、战斗、占领；地图订阅战争事件刷新。
- **优点**：战争逻辑集中，短期容易实现复杂战斗。
- **缺点**：违背“地图主导”；未来玩家会感到地图只是皮肤，体验割裂风险高。
- **Invalidated because**：用户明确选择地图主导。

#### Option C — 战略地图 + 战役 session 双层同步
- **做法**：地图触发离散 battle session，战后同步回地图。
- **优点**：保留当前 BattleSession 思路，改造成本较低。
- **缺点**：本质仍是割裂 session，不满足“大地图无缝对接作战”。
- **Invalidated because**：当前问题正是 session 与地图割裂，继续沿用会固化问题。

## ADR

### Decision
采用 **地图主导状态模型** 重写 C# 运行架构；保留并迁移现有 JSON 数据资产。第一阶段交付地图主导战争闭环：单路闭环、多军闭环、治理联动、UI/日志反馈。

### Drivers
- 用户明确要求：不要 MVP，先设计完整架构，然后全量重写，战争闭环优先。
- 当前代码证据显示地图、行军、战斗、占领是割裂实现。
- 项目已有地区、邻接、单位、帝皇等 JSON 数据，适合作为新架构输入。

### Alternatives Considered
- 战争系统主导：逻辑集中但不符合体验目标。
- 双层 session 同步：成本低但延续当前问题。
- 继续补丁式修复：短期快，长期不可控。

### Why Chosen
地图主导模型最符合“在大地图里面无缝对接作战”的用户体验目标，也最适合后续扩展补给、围攻、治理、AI 和事件系统。

### Consequences
- 需要重写核心 C# 状态层与系统层。
- 旧 BattleSession/BattleResolver/ArmyMovementSystem 不应作为核心继续扩展，可作为参考后废弃。
- 数据契约会新增战争路线、军队任务、接敌规则、占领后果等字段。

## Proposed Architecture

### Target Directory Shape

```text
WanChaoGuiYi/Assets/Scripts/
  Core/
    GameBootstrap.cs
    GameLoop.cs
    GameContext.cs
    EventBus.cs
  Data/
    DataRepository.cs
    DataModels.cs
    DataValidators.cs
  World/
    WorldState.cs
    MapState.cs
    RegionRuntimeState.cs
    FactionRuntimeState.cs
  Map/
    MapGraph.cs
    MapCommandService.cs
    MapQueryService.cs
    MapRenderer.cs
  Military/
    ArmyRuntimeState.cs
    ArmyCommandService.cs
    ArmyMovementSystem.cs
    WarState.cs
    EngagementDetector.cs
    BattleSimulationSystem.cs
    OccupationSystem.cs
  Governance/
    GovernanceImpactSystem.cs
    IntegrationSystem.cs
    RiskSystem.cs
  UI/
    MainMapUI.cs
    WarLogPanel.cs
    BattleReportPanel.cs
```

命名可在执行阶段微调，但职责边界不能变：状态、命令、查询、模拟、表现分离。

### Core State Model

- `WorldState`
  - 当前回合、季节、全局日志、所有 factions、regions、armies、wars。
- `MapState`
  - 地区运行状态、邻接图、地图上的军队索引、区域占领状态。
- `ArmyRuntimeState`
  - `id`, `ownerFactionId`, `locationRegionId`, `targetRegionId`, `route`, `task`, `soldiers`, `morale`, `supply`, `engagementId`。
- `WarState`
  - 参战方、战区区域、正在接敌的军队、战斗阶段、围攻/增援/撤退状态。
- `BattleState`
  - 从地图接敌产生，不独立拥有世界事实；只保存战斗计算所需快照和结果。
- `OccupationState`
  - 控制权变化、整合度、税粮折损、民变、地方势力、兼并压力。

### System Flow

```text
Player/AI command
  → MapCommandService issue move/order
  → ArmyMovementSystem advances armies along MapGraph
  → EngagementDetector checks enemy contact / siege / reinforcement
  → BattleSimulationSystem resolves or advances battle
  → OccupationSystem applies control changes
  → GovernanceImpactSystem applies integration/risk/output effects
  → EventBus publishes map/war/governance events
  → UI refreshes map, logs, battle reports
```

## Implementation Phases

### Phase 0 — Freeze and Baseline
1. Record current working state in `project-development-report.md`.
2. Run `python3 tools/validate_data.py`.
3. Identify old systems to retire after replacement:
   - `Military/ArmyMovementSystem.cs`
   - `Military/BattleResolver.cs`
   - `Military/BattleSessionSystem.cs`
   - `Military/BattleSetupSystem.cs`
   - `Military/BattleExecutionSystem.cs`
   - `Military/BattleConfigSystem.cs`
   - `Military/BattleDisplaySystem.cs`
4. Do not delete until replacement compiles and acceptance scenarios pass.

### Phase 1 — Data Contract for Map-Led War
1. Update `docs/data-contract.md` before JSON/C# changes.
2. Add or document runtime fields needed for:
   - army route / task / movement points
   - region control and occupation pressure
   - engagement triggers
   - governance consequences after occupation
3. Extend `tools/validate_data.py` only when persistent JSON fields change.
4. Keep existing JSON IDs stable.

### Phase 2 — New Runtime State Layer
1. Create new `World` runtime state classes.
2. Build adapters from existing `DataRepository` definitions into runtime state.
3. Keep JSON definitions immutable; runtime state owns changing values.
4. Add query APIs:
   - find armies in region
   - find neighboring regions
   - find hostile armies
   - get route / reachable regions

### Phase 3 — Map Command and Army Movement
1. Implement map commands: move army, stop, retreat, reinforce, siege target.
2. Movement must use `MapGraph` adjacency.
3. Army state should track route and task, not only mutate `regionId`.
4. Emit events for movement started, arrived, blocked, contact detected.

### Phase 4 — Engagement and Battle Simulation
1. Implement `EngagementDetector` over map state.
2. Contact rules:
   - entering enemy-controlled region with enemy army triggers battle
   - multiple friendly/enemy armies in one region join same engagement
   - adjacent reinforcement can join if ordered or allowed by rule
3. Implement deterministic/semi-deterministic `BattleSimulationSystem` first.
4. Result must not directly mutate ownership without `OccupationSystem`.

### Phase 5 — Occupation and Governance Link
1. `OccupationSystem` changes control state after battle victory.
2. `GovernanceImpactSystem` applies:
   - integration decrease or initial low integration
   - tax/food contribution penalty
   - rebellion risk increase
   - local power / annexation pressure changes
3. Log every governance consequence.

### Phase 6 — UI and Feedback
1. Main map refreshes from map state, not ad hoc battle result state.
2. Add war log entries for:
   - movement
   - contact
   - battle result
   - occupation
   - governance effects
3. Battle report shows reasoned outcome: participants, terrain, soldiers, losses, control changes.

### Phase 7 — Retire Old Runtime Code
1. Once new path passes acceptance, remove old battle/session code or quarantine it under `Legacy/` for one commit only.
2. Ensure no duplicate public types.
3. Ensure `GameManager` only registers actual turn systems; services are created separately.

## Acceptance Scenarios

### Scenario A — Single Route Closure
- Given Qin owns Region A and enemy owns adjacent/route Region B.
- When player orders an army from A to B.
- Then army moves along `MapGraph` route.
- When it reaches B and enemy army is present, battle starts automatically.
- If attacker wins, B control changes through `OccupationSystem`.
- Map color, region panel, log and battle report update.

### Scenario B — Multi-Army Closure
- Given two friendly armies and two enemy armies operate across neighboring regions.
- When armies converge into the same contested region.
- Then one engagement aggregates participants.
- Reinforcement and retreat commands affect the same map-owned engagement.
- Battle result applies losses to all participants and leaves consistent army states.

### Scenario C — Governance Link
- Given a newly conquered region.
- Then integration is low or reduced.
- Tax/food output is penalized.
- Rebellion/local power/annexation pressure changes.
- UI/log explains the governance cost of expansion.

## Pre-mortem

### Failure Scenario 1 — 全量重写变成无边界重写
- **Risk**: 重写范围扩散到外交、科技、AI、UI 美化，战争闭环迟迟不可运行。
- **Mitigation**: Phase 1-6 只围绕地图主导战争闭环；其它系统只接最小接口。

### Failure Scenario 2 — 状态模型重复，地图不再是唯一事实来源
- **Risk**: `ArmyState.regionId`、`BattleSession`、`Region.owner` 多处各说各话。
- **Mitigation**: 所有战争写操作必须经 `MapCommandService / OccupationSystem`；battle 只产出 result，不直接改地图。

### Failure Scenario 3 — 保留 JSON 但契约不够，代码硬凑
- **Risk**: 为兼容旧字段写大量补丁，重写后仍不干净。
- **Mitigation**: 缺字段先改 `docs/data-contract.md` 和验证脚本，再迁移数据，最后写 C#。

## Test Plan

### Unit Tests / EditMode Preferred
- MapGraph route and neighbor queries.
- Army movement state transitions.
- Engagement detection for enemy contact, multi-army merge, reinforcement.
- Battle simulation deterministic outcome with fixed inputs.
- Occupation and governance impact calculations.

### Integration Tests
- Load JSON → build runtime WorldState → issue move command → trigger battle → occupation → governance update.
- Verify no stale state remains in old battle/session path.

### E2E / Manual Unity Validation
- In Unity scene, click/select army, issue move to enemy region, watch movement/contact/battle/occupation/log/map refresh.
- Multi-army convergence scenario.
- Newly occupied region panel shows integration/output/risk changes.

### Observability
- Structured logs/events for:
  - `ArmyMoveStarted`
  - `ArmyArrived`
  - `EngagementStarted`
  - `BattleResolved`
  - `RegionOccupied`
  - `GovernanceImpactApplied`
- Debug panel or log view should expose current army task and region engagement state.

## Verification Commands

```bash
python3 tools/validate_data.py
```

If Unity CLI is available, also run project compile or EditMode tests. If not available, record manual Unity compile requirement in `project-development-report.md`.

## Execution Guidance for Autopilot

1. Start by updating docs and data contract; do not write runtime code first.
2. Build the new runtime path alongside old code until acceptance scenarios pass.
3. Avoid mixing UI beautification into core architecture work.
4. Prefer small compilable slices:
   - data contract
   - runtime state
   - movement
   - engagement
   - battle result
   - occupation/governance
   - UI feedback
5. After each slice, run matching validation.

## Critic Verdict
APPROVE.

The plan is aligned with the user’s clarified goal, fairly rejects alternatives, contains risk mitigation, and defines testable acceptance scenarios. The main execution risk is scope explosion; the phase boundaries and acceptance scenarios are the control mechanism.
