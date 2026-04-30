# Deep Interview Spec: 地图战争四场景最小全链路升级

## Metadata

- Interview ID: deep-interview-war-four-scenario-upgrade-2026-04-30
- Rounds: 5
- Final Ambiguity Score: 18%
- Type: brownfield
- Generated: 2026-04-30
- Threshold: 20%
- Status: PASSED

## Clarity Breakdown

| Dimension | Score | Weight | Weighted |
|-----------|-------|--------|----------|
| Goal Clarity | 0.88 | 0.35 | 0.308 |
| Constraint Clarity | 0.86 | 0.25 | 0.215 |
| Success Criteria Clarity | 0.82 | 0.25 | 0.205 |
| Context Clarity | 0.94 | 0.15 | 0.141 |
| **Total Clarity** | | | **0.869** |
| **Ambiguity** | | | **13.1%** |

## Goal

把当前地图战争 headless 四场景从“能通过”升级为“最小全链路可解释验收”：在不依赖本机 Unity 配置的前提下，证明玩家能从战争命令、行军、接敌、战斗、占领/防守、撤退/溃散、治理折损到经济结算，看见一条连续、可复现、可断言的战争结果链。

## Core Decision

本轮不优先进入 Unity 场景配置，因为用户将在另一台性能和存储更合适的电脑上安装 Unity。当前电脑要尽量完成 Unity 之前的全部准备：Domain 逻辑、headless 验收、结构化报告、迁移到 Unity 时可直接复用的事件/日志/状态断言。

## Constraints

- 不做实际 Unity 安装、场景搭建、Inspector 绑定或 PlayMode 手动验收。
- 不扩大到完整战争系统；聚焦当前已有 4 个 headless 场景。
- 不引入新美术、新 UI 面板或复杂战术战场。
- 保持 Domain Core Unity-free，继续通过 `tools/validate_domain_core.py`。
- 所有玩法提升必须能由命令行验证，避免“只能进 Unity 看现象”。
- 输出要能服务下一台电脑的 Unity 构建：日志语义、事件、状态字段、验收步骤要清楚。

## Non-Goals

- 不实现完整 AI 战争策略。
- 不实现多人、多战区或完整围攻系统。
- 不做 Unity Test Framework 集成，除非后续在 Unity 电脑上单独立项。
- 不重写全部战争架构；只围绕现有 map-led war runtime 加固。

## Target Scenarios

当前四场景是本次最小全链路升级对象：

1. `defender_holds_and_attacker_retreats`
   - 防守方获胜，进攻军撤退或溃散。
2. `attacker_wins_and_occupies`
   - 进攻方获胜，占领地区，触发治理折损并进入经济结算。
3. `reinforcement_joins_existing_engagement`
   - 增援抵达后加入既有接敌，改变 attacker membership 与战斗结果。
4. `active_retreat_leaves_engagement`
   - 已接敌军队主动撤退，脱离 engagement，战区恢复 controlled，不发生误结算。

## Acceptance Criteria

- [ ] `tools/verify_headless_war.sh` 通过，输出 `passed=True`、`scenarioCount=4`。
- [ ] 每个场景都有结构化 outcome summary，而不只是散落日志。
- [ ] 命令行输出双层报告：stdout 提供人眼可读的场景摘要，固定 JSON 文件提供机器可读详情。
- [ ] JSON 固定写入 `tools/headless_runner/latest-war-report.json`，每次运行覆盖，方便便携电脑和 Unity 电脑之间迁移查看。
- [ ] 成功场景默认输出摘要；失败场景展开阶段明细，包含失败阶段、before/after、断言结果和解释字段。
- [ ] 自然语言日志优先服务人读，不强迫机器解析；机器验证依赖 JSON 字段和断言结果。
- [ ] JSON 详情覆盖全部关键维度：阶段完整性、状态差值、解释字段，但成功路径保持摘要级，避免报告过重。
- [ ] 每个场景至少断言以下阶段中的相关项：命令发出、行军抵达、接敌创建/更新、战斗结算、占领/防守、撤退/溃散、治理影响、经济结算。
- [ ] `attacker_wins_and_occupies` 明确断言资源差值：战前资源 + runtime 折算收入 - upkeep = 战后资源。
- [ ] `reinforcement_joins_existing_engagement` 明确断言增援前后 membership 变化，并能解释为什么战斗结果变化。
- [ ] `active_retreat_leaves_engagement` 明确断言撤退后无残留 engagement，且 war resolution 不再触发战斗。
- [ ] 日志语义能被玩家理解：每条关键日志能回答“发生了什么、为什么、对我有什么影响”。
- [ ] 文档更新到 `project-development-report.md`，记录本次四场景升级的目标、改动、验证结果和 Unity 迁移下一步。

## Report Contract

### Stdout Human Summary

`tools/verify_headless_war.sh` 的 stdout 面向人读，默认只显示每个场景的一屏摘要，固定包含：

- `scenario`：场景名。
- `passed`：场景是否通过。
- `turnsExecuted`：执行回合数。
- `chain`：关键阶段结果，按 `command → movement → engagement → battle → outcome → governance → economy` 展示 `pass/skip/fail`。
- `result`：玩家能理解的结果句，例如“进攻军占领 region_x，治理贡献下降，下一次经济结算按折损后收入入账”。
- `keyDelta`：最关键的状态变化摘要，例如地区归属、军队位置/存亡、资源差值。
- `failureReason`：仅失败时显示。
- `jsonReport`：固定显示 `tools/headless_runner/latest-war-report.json`。

建议 stdout 形态：

```text
Headless war verification: passed=True scenarioCount=4
Report: tools/headless_runner/latest-war-report.json

[PASS] attacker_wins_and_occupies turns=2
  chain: command=pass movement=pass engagement=pass battle=pass outcome=pass governance=pass economy=pass
  result: army_player_1 captured region_enemy_1; contribution is reduced before economy settlement.
  keyDelta: owner faction_enemy -> faction_qin_shi_huang; money 190 + 46 - 5 = 231; food 280 + 72 - 8 = 344
```

失败时 stdout 展开失败阶段，但不倾倒完整 JSON：

```text
[FAIL] active_retreat_leaves_engagement turns=1
  chain: command=pass movement=pass engagement=pass retreat=fail battle=skip outcome=skip
  failureStage: retreat
  failureReason: Retreating army still has engagementId after movement.
  before: army_player_1 engagement=engagement_region_x task=Retreat location=region_x
  after: army_player_1 engagement=engagement_region_x task=Retreat location=region_y
  jsonReport: tools/headless_runner/latest-war-report.json
```

### JSON Machine Report

JSON 固定写入 `tools/headless_runner/latest-war-report.json`，每次覆盖。机器判断只依赖该文件，不解析自然语言日志。

顶层字段：

```json
{
  "runName": "headless_war_four_scenarios",
  "passed": true,
  "scenarioCount": 4,
  "passedCount": 4,
  "failedCount": 0,
  "generatedAt": "ISO-8601 timestamp",
  "scenarios": []
}
```

每个 scenario 固定字段：

```json
{
  "name": "attacker_wins_and_occupies",
  "passed": true,
  "turnsExecuted": 2,
  "summary": "进攻军占领目标地区，治理折损进入经济结算。",
  "failureStage": null,
  "failureReason": null,
  "phaseResults": [],
  "assertions": [],
  "keyDeltas": [],
  "explanations": [],
  "logs": []
}
```

### Phase Result Fields

`phaseResults[]` 用来证明最小全链路是否完整：

```json
{
  "phase": "battle",
  "status": "pass",
  "before": {
    "attackerSoldiers": 1000,
    "defenderSoldiers": 600,
    "attackerMorale": 80,
    "defenderMorale": 60
  },
  "after": {
    "attackerWon": true,
    "attackerSoldiers": 820,
    "defenderRemaining": false
  },
  "explanation": "进攻方兵力和士气优势导致自动结算胜利，随后触发占领。"
}
```

`status` 只允许：

- `pass`：该阶段发生并通过断言。
- `skip`：该场景不适用该阶段，并且原因明确。
- `fail`：该阶段发生但断言失败。

### Assertion Fields

`assertions[]` 是机器验收入口，字段固定：

```json
{
  "id": "economy.money_delta_matches_runtime_contribution",
  "phase": "economy",
  "passed": true,
  "expected": 231,
  "actual": 231,
  "message": "moneyAfter equals moneyBefore + effectiveTax - moneyUpkeep"
}
```

断言命名规则：

- `command.*`：命令是否发出、task/route/target 是否正确。
- `movement.*`：军队是否抵达目标、route 是否消费、legacy army 是否同步。
- `engagement.*`：接敌是否创建/更新、双方 membership 是否正确。
- `battle.*`：胜负、伤亡、士气、result 是否符合预期。
- `outcome.*`：占领、防守、撤退、溃散、残留 engagement 清理是否正确。
- `governance.*`：integration、contribution、rebellion/local power 等治理影响是否发生。
- `economy.*`：资源差值是否匹配 runtime contribution 与 upkeep。

### Key Delta Fields

`keyDeltas[]` 用来让 JSON 同时可解释：

```json
{
  "field": "region.ownerFactionId",
  "entityId": "region_enemy_1",
  "before": "faction_enemy",
  "after": "faction_qin_shi_huang",
  "impact": "玩家取得地区控制权，但贡献率先被治理系统折损。"
}
```

### Scenario-Specific Required Assertions

#### `defender_holds_and_attacker_retreats`

- `command.attack_route_created`
- `movement.attacker_arrived_at_defended_region`
- `engagement.created_with_attacker_and_defender`
- `battle.defender_won`
- `outcome.attacker_retreated_or_routed`
- `outcome.region_owner_unchanged`
- `outcome.no_resolved_engagement_left`

#### `attacker_wins_and_occupies`

- `command.attack_route_created`
- `movement.attacker_arrived_at_target_region`
- `engagement.created_with_attacker_and_defender`
- `battle.attacker_won`
- `outcome.region_owner_changed_to_attacker`
- `governance.occupation_reduced_contribution`
- `economy.money_delta_matches_runtime_contribution`
- `economy.food_delta_matches_runtime_contribution`

#### `reinforcement_joins_existing_engagement`

- `command.reinforcement_route_created`
- `movement.reinforcement_arrived_at_engagement_region`
- `engagement.membership_changed_after_reinforcement`
- `battle.result_changed_or_explained_by_membership`
- `outcome.no_stale_reinforcement_task`

#### `active_retreat_leaves_engagement`

- `command.retreat_route_created`
- `movement.retreating_army_left_region`
- `engagement.retreating_army_removed_from_membership`
- `outcome.no_residual_engagement_after_retreat`
- `battle.no_battle_triggered_after_retreat`
- `outcome.region_returns_to_controlled_when_uncontested`

### Log Standard

自然语言日志服务玩家理解，每条关键日志优先回答三件事：

1. 发生了什么：谁对谁做了什么。
2. 为什么发生：由命令、抵达、接敌、兵力士气、治理规则或经济规则触发。
3. 对玩家有什么影响：地区归属、军队状态、收入折损、资源变化或风险变化。

推荐句式：

```text
army_player_1抵达region_enemy_1并与army_enemy_1接敌：双方位于同一敌对地区，自动创建战斗。影响：该地区进入争夺状态，本回合将结算胜负。
```

日志不承担机器断言职责；如果日志和 JSON 断言冲突，以 JSON 断言为验收依据，并把日志视为需要修正的人读解释层。

## Assumptions Exposed & Resolved

| Assumption | Challenge | Resolution |
|------------|-----------|------------|
| 最薄弱环节是战争玩法深度 | 仓库证据显示 Unity 验证也薄弱，但用户当前电脑不适合 Unity | 先做 Unity 之前能完成的战争四场景最小全链路升级 |
| “玩法深度”可能意味着更多机制 | 追问核心体验后确认不是堆机制，而是可解释闭环 | 聚焦玩家能理解战争全过程 |
| 最小全链路可能只需单场景 | 用户选择“四场景升级” | 以当前 4 个 headless 场景为最小范围 |
| 验收只看日志即可 | 日志可能假阳性 | 必须加入结构化状态断言和资源差值断言 |

## Technical Context

Relevant evidence from repo:

- `project-development-report.md` 多次记录 Unity Console、PlayMode、场景内按钮/地图交互尚未验证。
- `tools/verify_headless_war.sh` 当前已能串联 data validation、domain boundary validation、headless runner。
- `HeadlessSimulationRunner` 已有四场景，但还可以进一步升级为结构化 outcome summary 与更强断言报告。
- `DomainArmyMovementSystem`、`DomainMapWarResolutionSystem`、`DomainEconomySystem` 是当前最关键的无 Unity 验证链路。
- `MapCommandService` 是玩家战争命令的稳定入口，应继续围绕它做可解释命令链。

## Ontology (Key Entities)

| Entity | Type | Fields | Relationships |
|--------|------|--------|---------------|
| War Scenario | core domain | scenarioName, passed, failureReason, turnsExecuted | contains command, movement, engagement, battle, outcome |
| Army | core domain | id, ownerFactionId, locationRegionId, task, soldiers, morale, engagementId | participates in engagement; moves through regions |
| Region | core domain | id, ownerFactionId, occupationStatus, integration, contributionPercent | can be contested, occupied, defended |
| Engagement | core domain | id, regionId, attackerArmyIds, defenderArmyIds, phase, result | resolves into battle result |
| Economy Settlement | supporting | money, food, tax, upkeep, contributionPercent | validates occupation impact |
| Governance Impact | supporting | integration, rebellionRisk, localPower, annexationPressure | follows occupation |
| Headless Report | supporting | scenario summaries, assertions, logs | proves the feature before Unity |

## Ontology Convergence

| Round | Entity Count | New | Changed | Stable | Stability Ratio |
|-------|-------------|-----|---------|--------|----------------|
| 1 | 3 | War Feature, Unity Validation, Headless Verification | - | - | - |
| 2 | 4 | Explainable Loop | 0 | 3 | 75% |
| 3 | 5 | Implementation Boundary | 0 | 4 | 80% |
| 4 | 6 | Minimum Full Chain | 0 | 5 | 83% |
| 5 | 7 | Four Scenario Upgrade | 0 | 6 | 86% |

## Interview Transcript

<details>
<summary>Full Q&A (5 rounds)</summary>

### Round 1
**Q:** 当前最薄弱处集中在 Unity 场景/PlayMode 验证未闭环，但也可能继续强化 headless 战争真实断言。你希望这次 deep interview 聚焦哪个具体功能？

**A:** 战争玩法深度。

### Round 2
**Q:** 你希望战争玩法深度首先提升哪一种核心体验？

**A:** 可解释闭环。

### Round 3
**Q:** 你希望这次功能提升的实现边界是什么？

**A:** 我准备在另一台电脑上下载 Unity 开始构建，因为这台电脑只是便携办公本，存储不够性能也不够；最好能够做到除了实际 Unity 配置之外其他都能在这台电脑上做好。

### Round 4
**Q:** 在不能做实际 Unity 配置的前提下，最关键的验收标准应该是哪一个？

**A:** 最小全链路。

### Round 5
**Q:** 你希望这次最小全链路覆盖哪组战况？

**A:** 四场景升级。

</details>
