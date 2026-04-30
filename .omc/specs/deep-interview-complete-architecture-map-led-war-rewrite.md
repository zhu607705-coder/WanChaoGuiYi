# Deep Interview Spec: 完整架构与地图主导战争闭环重写

## Metadata
- Interview ID: 20260429-map-led-war-rewrite
- Rounds: 7
- Final Ambiguity Score: 15%
- Type: brownfield
- Generated: 2026-04-29
- Threshold: 20%
- Status: PASSED

## Clarity Breakdown
| Dimension | Score | Weight | Weighted |
|-----------|-------|--------|----------|
| Goal Clarity | 0.92 | 0.35 | 0.3225 |
| Constraint Clarity | 0.84 | 0.25 | 0.2100 |
| Success Criteria | 0.86 | 0.25 | 0.2150 |
| Context Clarity | 0.80 | 0.15 | 0.1200 |
| **Total Clarity** | | | **0.8675** |
| **Ambiguity** | | | **0.1325** |

## Goal
以“完整游戏架构”为目标，保留现有 JSON 数据资产，重写 C# 运行架构；第一落点是建立地图主导的战争闭环，使军队移动、接敌、战斗、占领、地图刷新、UI反馈和治理后果在同一大地图状态模型中连续发生。

## Constraints
- 不再以 MVP 收缩为目标；架构设计要面向完整游戏。
- 允许全量重写 C# 运行逻辑，而不是在 mimo 生成的现有代码上继续补丁式修复。
- 保留现有 JSON 数据资产作为新架构输入，包括 `regions.json`、`map_region_shapes.json`、`emperors.json`、`units.json`、`technologies.json` 等。
- 若现有 JSON 字段无法支撑地图主导战争闭环，应先更新 `docs/data-contract.md`，再迁移数据。
- 第一阶段必须优先解决地图、行军、接敌、战斗、占领和治理联动；其它系统可围绕该核心重建，但不得抢占核心闭环。
- Unity/C# 仍是技术路线，数据仍以 JSON 为主。

## Non-Goals
- 不继续围绕“快速 MVP Demo”做临时拼接。
- 不以当前战斗 session 系统为核心继续扩展。
- 不为了保留已有代码而牺牲地图主导战争闭环。
- 不先做复杂外交、多人联机或大型实时战术战场。

## Acceptance Criteria
- [ ] 启动新架构后，JSON 数据能被加载并验证通过。
- [ ] 大地图是唯一权威战争状态来源：军队位置、目标、路线、接敌、战斗状态、占领状态都能从地图状态查询。
- [ ] 单路闭环成立：从一个己方区域出兵，沿邻接关系行军，进入敌方区域后自动触发战斗，胜利后占领区域并刷新地图/UI/日志。
- [ ] 多军闭环成立：多个军队可同时在大地图移动，接敌、围攻、增援、撤退都在地图状态中连续发生。
- [ ] 战争结果联动治理：新占领地区产生整合度变化、税粮折损、民变风险、地方势力或兼并压力变化。
- [ ] 战斗不再是孤立 session；战斗输入来自地图状态，战斗结果回写地图状态。
- [ ] 每个关键状态变化都有事件或日志反馈，用户能理解下一步发生了什么。
- [ ] 运行 `python3 tools/validate_data.py` 通过。
- [ ] 核心 C# 编译通过，不存在重复类型、错误构造器、系统注册类型不匹配等基础编译问题。

## Assumptions Exposed & Resolved
| Assumption | Challenge | Resolution |
|------------|-----------|------------|
| “断电重构”是断点/存档能力 | 询问断电具体含义 | 用户澄清是“断点”打错，真实意图是 mimo 生成代码质量差，需要系统性重构 |
| 继续做 MVP | 用户明确反对 MVP | 目标改为完整架构导向，不以 MVP 收缩为主 |
| 在现有代码上修补即可 | 询问重构边界 | 用户选择先设计完整架构，然后全量重写 |
| 战斗可以是独立 session | 用代码证据挑战地图/战斗割裂 | 用户选择地图主导模型 |
| 全量重写意味着所有资产归零 | 询问保留边界 | 用户选择保留数据重写 |
| 第一阶段只做单路闭环即可 | 询问验收范围 | 用户选择全部验收：单路、多军、治理联动都要覆盖 |

## Technical Context
当前仓库是 Unity/C# brownfield 项目：

- 核心代码目录：`WanChaoGuiYi/Assets/Scripts/`
- 当前约 67 个 C# 脚本，模块包括 AI、Building、Celestial、Core、Culture、Data、Diplomacy、Economy、Emperor、Equipment、Map、Military、Politics、Talent、Tech、UI、Weather。
- 数据目录：`WanChaoGuiYi/Assets/Data/`
- 数据验证入口：`python3 tools/validate_data.py`
- 已知数据验证通过：emperors=13、regions=56、map_region_shapes=56、historical_layers=56、technologies=40、generals=12、buildings=12、chronicle_events=200。
- 现有地图与战争割裂证据：
  - `WanChaoGuiYi/Assets/Scripts/Military/ArmyMovementSystem.cs` 中 `MoveArmy` 主要只是改变 `army.regionId`。
  - `WanChaoGuiYi/Assets/Scripts/Military/BattleResolver.cs` 中 `Resolve` 独立计算战斗并直接 `context.ChangeRegionOwner(defender.regionId, attacker.ownerFactionId)`。
  - `WanChaoGuiYi/Assets/Scripts/Military/BattleSessionSystem.cs` 管理离散 `BattleSession`，不是地图权威状态。
  - `WanChaoGuiYi/Assets/Scripts/Map/MapGraph.cs` 已有邻接关系，可作为新地图主导架构的基础输入。

## Ontology (Key Entities)
| Entity | Type | Fields | Relationships |
|--------|------|--------|---------------|
| 完整架构 | core domain | Unity/C#, JSON数据, 系统边界 | 约束全量重写和模块分层 |
| 保数据重写 | constraint | 保留JSON, 重写C# | 使用现有数据资产作为新架构输入 |
| 地图主导状态模型 | core domain | 区域, 邻接, 军队位置, 路线, 战斗状态, 占领状态 | 地图是战争状态唯一权威来源 |
| 战争闭环 | core domain | 行军, 接敌, 战斗, 占领, 治理后果 | 由地图主导状态模型驱动 |
| 军队 | core domain | ownerFactionId, regionId, soldiers, unitId, morale | 在地图区域/路线中移动并参与战斗 |
| 区域 | core domain | id, ownerFactionId, neighbors, integration, rebellionRisk, localPower | 承载军队、战斗、占领和治理后果 |
| 战斗 | supporting | attacker, defender, terrain, participants, result | 从地图状态产生，结果回写地图 |
| 治理联动 | supporting | integration, tax/food output, rebellionRisk, localPower, annexationPressure | 由战争结果触发 |
| JSON 数据资产 | external/input | regions, map_region_shapes, emperors, units, technologies | 为新架构提供初始世界和规则数据 |
| UI反馈 | supporting | 地图刷新, 日志, 战报 | 展示地图主导战争闭环的结果 |

## Ontology Convergence
| Round | Entity Count | New | Changed | Stable | Stability Ratio |
|-------|-------------|-----|---------|--------|----------------|
| 1 | 5 | 5 | - | - | - |
| 2 | 6 | 2 | 1 | 3 | 60% |
| 3 | 8 | 2 | 1 | 4 | 67% |
| 4 | 7 | 1 | 1 | 5 | 83% |
| 5 | 7 | 1 | 1 | 5 | 86% |
| 6 | 8 | 1 | 1 | 6 | 88% |
| 7 | 10 | 2 | 1 | 7 | 80% |

## Interview Transcript
<details>
<summary>Full Q&A (7 rounds)</summary>

### Round 1
**Q:** 你说的“全部功能和断电重构”，这里的“断电”具体指什么？
**A:** 断电打错了是断点。
**Ambiguity:** 79%

### Round 2
**Q:** 你说的“断点重构”，最终希望验收哪一种“断点”？
**A:** 我的意思是现在大量代码因为是 mimo 写的所有存在着很多的问题要进行重构。
**Ambiguity:** 67%

### Round 3
**Q:** 这次重构应该采用哪种边界？
**A:** 以后不要什么 MVP，要做就直接做完整，重新设计，还有我要说的是就是现在应该是没有办法做到大地图里面进行无缝对接作战的吧。
**Ambiguity:** 54%

### Round 4
**Q:** 你说“要做就直接做完整”，更接近哪一种？
**A:** 先设计完整架构然后全量重写，然后在战争闭环。
**Ambiguity:** 39%

### Round 5
**Q:** 从零设计战争闭环时，哪个模型才是你真正想要的？
**A:** 地图主导。
**Ambiguity:** 31%

### Round 6
**Q:** 全量重写时，哪些现有东西必须保留？
**A:** 保数据重写。
**Ambiguity:** 22%

### Round 7
**Q:** 第一阶段重写完成后，你希望用哪种场景验收“地图主导战争闭环”真的成立？
**A:** 全部验收。
**Ambiguity:** 15%

</details>
