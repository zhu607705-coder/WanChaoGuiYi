# MVP Closure Ledger

## 目的

本台账用于收口当前已经出现的 MVP 半成品，防止后续继续堆新系统但核心玩法仍不完整。

当前方向以 deep-interview 结论为准：

- 核心玩法是架空帝皇斗蛐蛐。
- 主模式是类似 NBA2K 的模拟加可下场混合模式。
- 玩家下场层级是战役指挥：调军、路线、补给、进攻、撤退，战斗仍自动结算。
- 所有已经进入当前 MVP 的系统都必须保留并完善到可玩、可解释、可验证状态。
- 本阶段不做战术战斗、不做全球地图、不新增绕开当前 MVP 收口的新系统。

## 状态定义

| 状态 | 含义 | 收口要求 |
| --- | --- | --- |
| 已可玩 | Web 中可操作或可观察，headless 或 Web 测试覆盖核心因果 | 保持回归门禁 |
| 部分可玩 | 有数据、UI 或 headless 证明，但玩家体验链不完整 | 补齐缺失 UI、因果或验收 |
| 数据已备 | JSON/契约已存在，但运行态接入不足 | 接入 Web/Domain 或明确最小玩法 |
| 测试占位 | 已识别缺口并有 TODO 或审查项 | 转成真实回归测试 |
| 同步风险 | C# 与 Web 都表达同一规则，存在漂移风险 | 加 parity 测试或共享契约 |
| 阻塞 | 当前证据不足以判定可玩 | 先做探测和最小复现 |

## 全量 MVP 系统台账

| 系统 | 当前证据 | 状态 | 必须保留并完善的收口目标 | 验证门 |
| --- | --- | --- | --- | --- |
| 56 区九州地图 | `regions.json`、`map_region_shapes.json`、Three.js 点击/染色、区域 shape 单测 | 已可玩 | 保持 56 区可点击、可染色、可聚焦；地图仍是斗蛐蛐主舞台 | `check:data-source`、region shape/neighbor 单测、Playwright 地图断言 |
| 帝皇与势力差异 | 13 位帝皇数据、preferred policies、AI personality、机制描述 | 部分可玩 | 帝皇机制必须实际影响扩张、治理、继承或财政压力，而不只停留在描述 | 帝皇机制数据对齐测试、至少 3 位帝皇差异化模拟验收 |
| 架空斗蛐蛐模拟 | AI 倾向字段、headless 场景、Web 回合推进 | 部分可玩 | 自动推演能产生可读的强弱变化、危机和反转；玩家能观战后选择接管 | 新增模拟观战场景和 outliner/log 验收 |
| 玩家接管和战役指挥 | Web 战争命令、路线、补给、截粮、撤退、战报、27 条 Playwright | 已可玩 | 下场接管必须稳定覆盖调军、路线、补给、进攻、撤退、占后处理 | headless war、Playwright 战役指挥流 |
| 自动战斗结算 | Domain battle simulation、tie-break、casualty、morale、supply tests | 已可玩 | 保持自动结算，不进入战术战斗；战报解释胜负、伤亡、补给影响 | xUnit battle/morale/supply tests |
| 扩张后的治理拖累 | occupation status、control stage、contribution caps、pacification queue | 已可玩 | 新占地不能立刻完整贡献；玩家必须处理占领治理成本 | headless occupation/control chain、Web 治理行动断言 |
| 王朝周期压力 | successionRisk、stableSuccessions、legitimacy、expansion succession pressure | 部分可玩 | 扩张、继承、财政、土地、军队必须连成强盛王朝过热到危机的压力链 | 新增 20-40 回合王朝周期 headless/Web 验收 |
| 财政、粮食、人口、兵力 | EconomySystem、DomainEconomySystem、Web nation aggregation tests | 已可玩 | 财政和粮食要参与扩张、军队、治理的真实取舍 | xUnit economy tests、Web aggregation/property tests |
| 土地兼并和民变 | landStructure、annexationPressure、rebellionRisk、relief/tax pressure scenarios | 部分可玩 | 土地和民变必须成为王朝周期压力的显性后果，而不是只做数值字段 | 治理压力 headless 场景、UI 最大风险断言 |
| 继承系统 | heir、successionRisk、stableSuccessions、victory condition data | 部分可玩 | 帝皇老去或继承不稳必须能触发王朝断裂风险和玩家介入窗口 | 继承危机场景、三代延续验收 |
| 法统和合法性 | legitimacy、legitimacyMemory、localAcceptance、policy/event effects | 部分可玩 | 合法性要解释扩张、继承、民变、地方接受度之间的因果 | headless legitimacy pressure test、UI reason text |
| 人才系统 | `talents.json` 仅 4 种，TalentDefinition 和 NumericStat.TalentGain 存在 | 数据已备 | 人才要能影响战争、财政、改革或地方治理，并带政治代价 | 人才获得/任命最小 Web 或 headless 流 |
| 政策和治理行动 | 41 项 policies、recommendedPolicy、applyGovernancePolicy | 已可玩 | 政策必须服务王朝压力调节，展示成本、风险、收益和来源 | data-source validation、Playwright 治理操作 |
| 科技/制度树 | 32 项 technologies、boost、unlocks、Numeric technology helpers | 数据已备 | 技术/制度不能只存在数据里，至少驱动政策/单位/事件解锁或研究进度 | tech unlock/reference tests、最小研究流 |
| 建筑系统 | buildings data、recommendedBuilding、governance project/building markers | 部分可玩 | 建筑应成为区域长期治理和物流取舍，不只是推荐文本 | building project Playwright、data reference tests |
| 编年事件 | 200 chronicle events、event choices、Unity-free Web turn loop test | 已可玩 | 事件要解释王朝周期压力，不只随机弹窗 | chronicle trigger/choice tests、UI result log |
| 天气、风俗、装备、天文、将领 | data contracts and JSON/data model support | 部分可玩 | 保留为当前 MVP 表达层，至少要有数据解释和一个可观察影响路径 | data validation、UI summary 或 headless effect smoke |
| 胜利条件 | `victory_conditions.json` 三种胜利、Numeric victory helpers | 数据已备 | 一局可从开局推进到胜利/失败，玩家能理解原因 | victory progress test、20-40 回合演示验收 |
| 存档/导入导出 | Web local slots、schemaVersion、import/export Playwright | 已可玩 | 存档必须覆盖治理、军队、物流、战报和关键王朝压力状态 | Playwright save/load、corrupt save tests |
| UI 决策清晰度 | outliner、risk summaries、governance panel、war reports | 部分可玩 | 每回合清楚显示最大风险、原因、可选行动、预计后果、实际变化 | Playwright viewport and decision-surface assertions |
| Domain/Web 因果同步 | headless report helpers、headless-vs-ui numerics tests | 同步风险 | 重复表达的因果规则必须有 parity 检查，防止 C# 与 TS 漂移 | parity unit tests、headless report schema tests |
| 内容生产管线 | data contract、validate_web_data_source.py、schemaVersion 约定 | 部分可玩 | 新增帝皇、政策、事件、地区机制时要有版本和差异审查 | schemaVersion gate、content diff checklist |
| 测试质量债 | `CoverageGap_TODO_Placeholders.cs`、audit-test-coverage 缺口表 | 测试占位 | TODO 覆盖项必须逐步转成真实测试，避免假绿 | xUnit TODO closure PRs、audit status update |

## 当前第一批收口顺序

1. 王朝周期压力链：扩张拖累、财政军队土地挤压、继承风险、民变/割据后果。
2. 模拟观战和玩家接管：自动推演可读，玩家能在战役指挥层接管并改变局势。
3. UI 决策清晰度：最大风险、原因、行动、代价、预计后果、实际变化。
4. Domain/Web parity：把玩家可见因果纳入同步门。
5. 内容和测试管线：schemaVersion、内容差异审查、TODO 测试转正。

## 下一步可执行任务

| 优先级 | 任务 | 目标文件 | 验证 |
| --- | --- | --- | --- |
| P0 | 建立 20-40 回合王朝周期验收场景设计 | `docs/mvp-closure-ledger.md`、`project-development-report.md`、headless tests | 文档检查，后续 xUnit |
| P0 | 盘点现有 Web 是否有“接管王朝/恢复模拟”入口 | `web-strategy-map/src/ui.ts`、Playwright | Playwright targeted grep/test |
| P1 | 把 `CoverageGap_TODO_Placeholders.cs` 中最高优先级 TODO 转成真实测试 | `tools/headless_runner/WanChaoGuiYiTests` | `dotnet test` targeted |
| P1 | 为王朝周期压力增加 UI 最大风险解释断言 | `web-strategy-map/tests/strategy-map.spec.ts` | Playwright targeted |
| P1 | 给数据管线增加内容扩展差异审查清单 | `tools/validate_web_data_source.py`、`docs/data-contract.md` | data-source validation |

## 完成判定

MVP 收口完成不是“所有审查文档无缺口”，而是：

1. 现有 MVP 系统都在本台账中有状态和验收门。
2. 没有当前 MVP 系统被砍掉、隐藏、降级成未来预留。
3. 玩家能模拟，也能接管王朝进行战役指挥。
4. 强盛王朝能自然进入过热、危机、崩盘或续命路径。
5. UI 能解释危机原因、可做行动和行动后变化。
6. `tools/run_all_checks.ps1` 或同等完整门禁通过。
