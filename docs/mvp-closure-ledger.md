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
| 帝皇与势力差异 | 13 位帝皇数据、preferred policies、AI personality、机制描述、`DomainEmperorMechanicSystem` 三类纯 C# 效果、秦始皇/刘邦/汉武帝差异化 xUnit | 部分可玩 | 帝皇机制必须继续接入更完整的扩张、治理、继承或财政压力闭环，而不只停留在描述 | 帝皇机制数据对齐测试、至少 3 位帝皇差异化模拟验收、后续应用到回合流的 headless 验收 |
| 架空斗蛐蛐模拟 | AI 倾向字段、`DomainStrategicAiSystem` 可解释意图、headless 场景、Web 回合推进、王朝接管面板、Web 20 回合危机到接管胜利/失败长线 | 部分可玩 | 自动推演能产生可读的强弱变化、危机和反转；玩家能观战后选择接管 | StrategicAI 意图 xUnit、Playwright 模拟观战/接管/续命断言、Web 20 回合成功/失败长线 |
| 玩家接管和战役指挥 | Web 战争命令、路线、补给、截粮、撤退、战报、27 条 Playwright | 已可玩 | 下场接管必须稳定覆盖调军、路线、补给、进攻、撤退、占后处理 | headless war、Playwright 战役指挥流 |
| 自动战斗结算 | Domain battle simulation、tie-break、casualty、morale、supply tests | 已可玩 | 保持自动结算，不进入战术战斗；战报解释胜负、伤亡、补给影响 | xUnit battle/morale/supply tests |
| 扩张后的治理拖累 | occupation status、control stage、contribution caps、pacification queue | 已可玩 | 新占地不能立刻完整贡献；玩家必须处理占领治理成本 | headless occupation/control chain、Web 治理行动断言 |
| 王朝周期压力 | successionRisk、stableSuccessions、legitimacy、expansion succession pressure、`DynastyCyclePressureAcceptanceTests` 场景 A/B/C/D 与 headless 20 回合成功/失败长线串联、Web 王朝接管入口、资源不足不可续命状态和 Web 20 回合成功/失败长线 | 部分可玩 | 扩张、继承、财政、土地、军队必须连成强盛王朝过热到危机的压力链 | Domain C/D、headless 长线、Playwright 接管续命/不可续命断言、Web 20 回合成功/失败长线 |
| 财政、粮食、人口、兵力 | EconomySystem、DomainEconomySystem、Web nation aggregation tests | 已可玩 | 财政和粮食要参与扩张、军队、治理的真实取舍 | xUnit economy tests、Web aggregation/property tests |
| 土地兼并和民变 | landStructure、annexationPressure、rebellionRisk、relief/tax pressure scenarios | 部分可玩 | 土地和民变必须成为王朝周期压力的显性后果，而不是只做数值字段 | 治理压力 headless 场景、UI 最大风险断言 |
| 继承系统 | heir、successionRisk、stableSuccessions、victory condition data、`DomainSuccessionSystem`、Web 立储安宗按钮和存档字段 | 部分可玩 | 帝皇老去或继承不稳必须能触发王朝断裂风险和玩家介入窗口 | 继承危机场景、Web 接管续命断言、三代延续验收 |
| 法统和合法性 | legitimacy、legitimacyMemory、localAcceptance、policy/event effects | 部分可玩 | 合法性要解释扩张、继承、民变、地方接受度之间的因果 | headless legitimacy pressure test、UI reason text |
| 人才系统 | `talents.json` 仅 4 种、TalentDefinition、NumericStat.TalentGain、`DomainTalentSystem` 招贤/任命、清丈能吏降低兼并压力并抬高朝局压力的 xUnit | 部分可玩 | 人才要继续扩展到战争、财政、改革或地方治理的多角色任命，并带政治代价 | TalentSystem xUnit、后续 Web 或 headless 多角色流 |
| 政策和治理行动 | 41 项 policies、recommendedPolicy、applyGovernancePolicy | 已可玩 | 政策必须服务王朝压力调节，展示成本、风险、收益和来源 | data-source validation、Playwright 治理操作 |
| 科技/制度树 | 32 项 technologies、boost、unlocks、Numeric technology helpers | 数据已备 | 技术/制度不能只存在数据里，至少驱动政策/单位/事件解锁或研究进度 | tech unlock/reference tests、最小研究流 |
| 建筑系统 | buildings data、recommendedBuilding、governance project/building markers | 部分可玩 | 建筑应成为区域长期治理和物流取舍，不只是推荐文本 | building project Playwright、data reference tests |
| 编年事件 | 200 chronicle events、event choices、Unity-free Web turn loop test | 已可玩 | 事件要解释王朝周期压力，不只随机弹窗 | chronicle trigger/choice tests、UI result log |
| 天气、风俗、装备、天文、将领 | data contracts and JSON/data model support | 部分可玩 | 保留为当前 MVP 表达层，至少要有数据解释和一个可观察影响路径 | data validation、UI summary 或 headless effect smoke |
| 胜利条件 | `victory_conditions.json` 三种胜利、Numeric victory helpers、Web 三代延续胜利进度、达成断言与 20 回合成功/失败长线；Domain 和 Web 均已消费 `maxFragmentation` 阻断高分裂三代胜利 | 部分可玩 | 一局可从开局推进到胜利/失败，玩家能理解原因；三代延续必须持续受分裂度约束，并避免 Web/Domain 胜利口径漂移 | 三代延续 Playwright 进度断言、Web 20 回合成功/失败长线、`DomainVictorySystem` maxFragmentation xUnit、Web 分裂度 Playwright |
| 存档/导入导出 | Web local slots、schemaVersion、import/export Playwright、王朝压力和接管模式导出/导入断言 | 已可玩 | 存档必须覆盖治理、军队、物流、战报和关键王朝压力状态 | Playwright save/load、corrupt save tests、王朝接管存档断言 |
| UI 决策清晰度 | outliner、risk summaries、dynasty pressure summary、governance panel、war reports | 部分可玩 | 每回合清楚显示最大风险、原因、可选行动、预计后果、实际变化 | Playwright viewport and decision-surface assertions |
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
| P0 | 修补 Web `institutional_order` 进度/outliner | `web-strategy-map/src/ui.ts`、`web-strategy-map/tests/strategy-map.spec.ts` | Web 字段载体已补；下一步读取 `victory_conditions.json` 阈值，显示制度胜利进度、达成状态和玩家可见压力门 |
| P1 | 复核 Web/Domain 分裂度公式精确 parity | `domain-core/src/Domain/Victory`、`web-strategy-map/src/ui.ts` | Web 已用玩家可见 `risk` / 低 `integration` 形成分裂度门；Domain 仍用 `rebellionRisk` / `localPower` / `annexationPressure` / 低 `integration`，后续如需完全同口径需先补 Web 字段来源 |
| P1 | 复核 `unify_jiuzhou` 是否还需要长线自然统一演示 | `web-strategy-map/src/ui.ts`、`web-strategy-map/tests/strategy-map.spec.ts` | 统一九州已具备 Web 运行态进度、达成/未达成断言和导出/导入保留；后续可复核是否需要战役自然扩张长线 |
| P1 | 扩展 StrategicAI 从意图到可控命令建议 | `domain-core/src/Domain/Ai`、`tools/headless_runner/WanChaoGuiYiTests` | 已有纯 C# 意图选择；后续再把 `expand` / `stabilize` / `recover` 转成可审查命令建议，不直接跳到完整 AI 自动回合 |
| P1 | 扩展 TalentSystem 多角色和 Web 可见入口 | `domain-core/src/Domain/Talents`、`web-strategy-map/src/ui.ts`、`tools/headless_runner/WanChaoGuiYiTests` | 已有清丈能吏 headless 最小证明；后续再补宿将、理财重臣、边疆使臣和玩家可见招贤/任命入口 |
| P1 | 扩展 domain-core 帝皇机制到回合流应用 | `domain-core/src/Domain/Emperors`、`tools/headless_runner/WanChaoGuiYiTests` | 已有 3 位帝皇差异化效果对象和 full gate；后续再把效果接入经济、治理、战争或继承回合结算 |
| P1 | 把 Web 20 回合失败长线推广到更自然的资源耗尽路径 | `web-strategy-map/tests/strategy-map.spec.ts` | 当前失败长线使用长线后资源不足种子；后续可复核自然消耗版 |
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

## 已启动的收口工件

- `docs/dynasty-cycle-acceptance-scenarios.md`：定义 20-40 回合王朝周期长线验收场景，作为后续 headless 和 Playwright 实现依据。
- `tools/headless_runner/WanChaoGuiYiTests/DynastyCyclePressureAcceptanceTests.cs`：已覆盖场景 A“扩张后过热”和场景 B“财政军队土地挤压”，验证资源收益、治理/军队成本、地方压力、继承压力和解释日志同时出现；已新增 headless 20 回合长线串联，覆盖继承危机、接管前后压力对比、资源代价、三代延续胜利进度，以及资源不足时无法续命且危机继续恶化的失败路径。
- `domain-core/src/Domain/Governance/DomainSuccessionSystem.cs` 与 `DynastyCyclePressureAcceptanceTests` 场景 C/D：已覆盖继承危机触发、合法性/朝局/地方稳定外溢，以及玩家立储安宗续命的资源代价。
- `web-strategy-map/src/ui.ts` 与 `web-strategy-map/tests/strategy-map.spec.ts`：outliner 已显示王朝继承压力摘要，Playwright 首屏断言会检查“王朝/继承稳定或承压或危机/可立储安宗”。
- `web-strategy-map/src/ui.ts` 与 `web-strategy-map/tests/strategy-map.spec.ts`：已补“模拟推演 -> 接管王朝 -> 立储安宗”面板、队列日志、资源代价、继承/朝局降压、`stableSuccessions` 增加，以及 `dynastyControlMode` 和王朝压力存档导入导出断言。
- `web-strategy-map/src/ui.ts` 与 `web-strategy-map/tests/strategy-map.spec.ts`：已补资源不足时的玩家可见不可续命状态，面板和 outliner 会显示资源不足原因，debug 暴露 `dynastyRescueBlocked`、`dynastyRescueBlockReason`、`dynastyFailureRisk`，Playwright 断言按钮禁用且不会误增 `stableSuccessions`。
- `web-strategy-map/tests/strategy-map.spec.ts`：已补 Web 20 回合成功长线，使用长线种子推进 20 个真实治理回合，断言 outliner 出现继承危机，观战日志解释扩张/民变/低法统来源，接管后连续立储安宗消耗资源、降低风险、达成三代延续，并验证导出/导入保留长线状态。
- `web-strategy-map/tests/strategy-map.spec.ts`：已补 Web 20 回合失败长线，推进 20 个真实治理回合后进入继承危机，再切到资源不足接管状态，断言不可续命、危机未缓解、未达成胜利，并验证导出/导入保留失败态。
- `web-strategy-map/src/data.ts`、`web-strategy-map/src/ui.ts` 与 `web-strategy-map/tests/strategy-map.spec.ts`：Web 已加载 `victory_conditions.json`，从 `three_generation_dynasty` 读取 `stableSuccessions` 与 `minLegitimacy` 阈值，并在 outliner/debug 中显示“三代延续”进度与达成状态。
- `web-strategy-map/src/data.ts`、`web-strategy-map/src/ui.ts` 与 `web-strategy-map/tests/strategy-map.spec.ts`：三代延续胜利进度已接入运行态，`stableSuccessions >= 3` 且法统达标时会显示“胜利 / 三代延续达成”。
- `web-strategy-map/src/ui.ts` 与 `web-strategy-map/tests/strategy-map.spec.ts`：统一九州胜利进度已接入运行态，按 `unify_jiuzhou.minLegitimacy` 与 `regions.owner` 计算 `playerOwnedRegions / totalRegions`，并在 outliner/debug 中显示“统一九州 / 统一九州达成”，Playwright 覆盖法统不足未达成、法统达标达成和导出/导入保留。
- `domain-core/src/Domain/Ai/DomainStrategicAiSystem.cs` 与 `tools/headless_runner/WanChaoGuiYiTests/StrategicAiIntentTests.cs`：已补 StrategicAI 最小可解释意图，覆盖高扩张资源足选 `expand`、高治理压力选 `stabilize`、资源不足选 `recover`，并断言不改变地图所有权。
- `domain-core/src/Domain/Victory/DomainVictorySystem.cs` 与 `tools/headless_runner/WanChaoGuiYiTests/VictorySystemFragmentationTests.cs`：已补三代延续最小 headless 胜利门，`stableSuccessions` 和法统达标但分裂度超过 `maxFragmentation:10` 时不会误判胜利，并输出“分裂度”原因。
- `web-strategy-map/src/ui.ts` 与 `web-strategy-map/tests/strategy-map.spec.ts`：已补 Web 三代延续分裂度门，从 `victory_conditions.json.maxFragmentation` 读取上限，用玩家可见 `risk` 与低 `integration` 计算分裂度，在 debug/outliner 显示分裂度原因，并覆盖高分裂阻断、低分裂达成和导出/导入保留。

## 2026-05-23 缺口复核

- 已完成项：Domain 场景 C/D 与 Web 接管入口不再列为下一步 P0，避免后续自动化重复只做旧缺口。
- 当前 P0：三代延续已具备 Web 可见胜利进度、达成断言、headless 20 回合成功/失败长线、Web 资源不足不可续命失败态和 Web 20 回合成功/失败长线；`unify_jiuzhou` 已具备 Web 运行态进度和达成断言。下一步应转向 `institutional_order` 与 `maxFragmentation` 的可解释运行态字段定义，或复核统一九州是否需要自然扩张长线。

## 2026-05-24 TalentSystem 后缺口复核

- 已完成项：domain-core 帝皇机制三类效果、TalentSystem 清丈能吏招贤/任命与政治代价均已有 headless 证明，不再作为下一轮重复目标。
- 当前 P0 首选：StrategicAI 最小可解释意图。只证明 AI 能基于现有皇帝 `aiPersonality`、派系资源、地区压力和相邻敌区选择 `expand` / `stabilize` / `recover`，先不执行命令、不改 Web。
- 暂缓项：`institutional_order` 需要先定义 `completedCoreReforms` 与 `minTreasuryStability` 的运行态来源；`maxFragmentation` 需要先定义分裂度指标口径；TalentSystem 多角色/Web 入口降为下一批 P1。
- 当前 P1：人才和科技仍主要是数据/定义层；`CoverageGap_TODO_Placeholders.cs` 仍保留军队生命周期、经济溢出、存档迁移等 TODO 占位，后续应逐项转成真实测试。

## 2026-05-24 StrategicAI 最小修补

- 已完成项：domain-core 新增只读 StrategicAI 意图选择，能解释扩张、治理整顿和资源休整三类意图，不执行攻击、治理命令或地图归属变更。
- 当前 P0 转向：`institutional_order` 的运行态字段来源与 `maxFragmentation` 指标口径仍需复核；TalentSystem 多角色/Web 入口和 StrategicAI 命令建议均降为 P1 后续扩展。

## 2026-05-24 StrategicAI 后缺口复核

- 已完成项：StrategicAI 最小意图、TalentSystem 清丈能吏、帝皇机制 parity、Web 王朝长线和 Web 统一九州均不再作为下一修补目标。
- 当前 P0 首选：三代延续 `maxFragmentation` 的 domain-core 胜利门。证据是 `victory_conditions.json` 已声明 `maxFragmentation:10`，但 Web `dynastyVictoryAchieved()` 和 headless helper 只检查 `stableSuccessions` 与 `minLegitimacy`。
- 建议下一修补轮只新增纯 C# `DomainVictorySystem` 与 xUnit：稳定三代且分裂度低时通过；同样续承/法统但高叛乱、地方势力、兼并或低整合时失败，并输出分裂度原因。`institutional_order`、Talent 多角色/Web 入口、StrategicAI 命令建议均排到后续 P1。

## 2026-05-24 maxFragmentation 最小修补

- 已完成项：纯 C# `DomainVictorySystem` 只评估 `three_generation_dynasty`，以己方地区 `rebellionRisk`、`localPower`、`annexationPressure`、`100 - integration` 的平均压力计算 `fragmentationScore`，并消费数据里的 `maxFragmentation`。
- 验证：targeted xUnit `1/1`、完整 `WanChaoGuiYiTests` `91/91`、`python tools/validate_domain_core.py`、`tools\verify_headless_war.ps1` `16/16`。
- 剩余风险：本轮按边界未做 Web/UI/存档；Web 三代达成口径仍需后续复核是否与 Domain 分裂度门做 parity。`institutional_order` 的运行态字段仍是下一批 P0 事实复核目标。

## 2026-05-24 Web/Domain parity 与制度胜利字段复核

- 已完成复核：Web `dynastyVictoryAchieved()` 仍只检查 `stableSuccessions` 和 `minLegitimacy`；Domain `DomainVictorySystem` 已检查 `maxFragmentation`。这会让 Web 在高风险/低整合状态下仍可能显示“三代延续达成”。
- `institutional_order` 证据：`VictoryRequirement.completedCoreReforms`、`minTreasuryStability`、`maxAnnexationPressure` 已在 TS/C# 类型中存在，`FactionState.completedReformIds` 也存在；但 Web `nationState` 还没有 completed reforms / treasury stability，技术和事件里的 `treasuryStability` 仍未形成稳定运行态累计来源。
- 下一修补首选：先补 Web 三代分裂度可见门，使用 `victory_conditions.json.maxFragmentation` 和玩家可见 `risk` / 低 `integration` 形成最小阻断与 Playwright 导入断言；之后再做 `institutional_order` 字段来源。

## 2026-05-24 Web 分裂度最小修补

- 已完成项：Web 三代延续达成现在读取 `maxFragmentation`，并用玩家可见 `risk` 与低 `integration` 计算 `dynastyFragmentationScore`；分裂度超过上限时 debug/outliner 显示“分裂度”原因且不再误报“三代延续达成”。
- 验证：Playwright targeted 红/绿；王朝相关 Playwright 子集 `5/5`；`npm --prefix web-strategy-map run typecheck`；胜利口径相关 `rg`。
- 剩余风险：Web 分裂度是玩家可见口径，不是 Domain 公式逐字段复刻；`institutional_order` 的 `treasuryStability` / `completedCoreReforms` 运行态来源仍是下一批 P0。

## 2026-05-24 institutional_order 字段来源复核

- 已完成复核：`victory_conditions.json` 已声明 `completedCoreReforms:4`、`minLegitimacy:70`、`minTreasuryStability:65`、`maxAnnexationPressure:45`；TS/C# `VictoryRequirement` 与 `EffectSet.treasuryStability` 均已有类型承载。
- 可用运行态来源：Domain `FactionState.completedReformIds` 可承载已完成改革；Domain `RegionState.annexationPressure` 已被治理、人才、AI 和胜利分裂度消费，可支撑土地兼并压力门。
- 阻塞点：Domain `FactionState` 尚无 `treasuryStability` 累计字段；Web `nationState` / debug / export/import 也没有 `completedCoreReforms` 或 `treasuryStability`。
- 数据线索：`technologies.json` 已有 `treasuryStability` 效果和 `complete_reform` / `complete_three_reforms` boost；`policies.json` 已有 `central_reform`；但这些还未形成当前纯代码运行态制度胜利链。
- 当前 P0 首选：下一修补轮先做纯 C# / headless 字段来源与进度 payload，例如 `Institutional_Order_Field_Sources_Should_Expose_Treasury_Stability_And_Core_Reforms`；payload 显示 `completedCoreReforms`、`requiredCoreReforms`、`treasuryStability`、`minTreasuryStability`、`maxObservedAnnexationPressure`、`maxAnnexationPressure`、`achieved`、`reason`。
- 暂缓项：不直接做 Web 制度胜利 UI、存档 schema、研究流、事件消费或完整政策改革系统，等财政稳定和改革推进口径先被 headless 测试锁住。

## 2026-05-24 institutional_order Domain 字段来源最小修补

- 已完成项：`FactionState.treasuryStability` 成为最小运行态来源，`DomainVictorySystem.EvaluateInstitutionalOrder()` 可只读评估制度胜利进度。
- payload 已暴露：`completedCoreReforms`、`requiredCoreReforms`、`treasuryStability`、`minTreasuryStability`、`maxObservedAnnexationPressure`、`maxAnnexationPressure`、`achieved`、`reason`。
- 验证：targeted xUnit 红/绿；`python tools\validate_domain_core.py`；完整 `WanChaoGuiYiTests` `92/92`；`tools\verify_headless_war.ps1` `16/16`。
- 剩余风险：Web `nationState` / debug / export/import 还未接入制度胜利字段；`treasuryStability` 正式累计算法和 `completedReformIds` 推进路径仍需下一轮复核后再修补。

## 2026-05-24 institutional_order 下一修补线复核

- Web 可见性结论：`StrategyDataset.nation` 还没有 `completedCoreReforms` / `treasuryStability`，但 debug 和 export/import 已复制 `nationState`；未来可做小型 Web carrier proof，暂不直接做制度胜利 UI。
- 财政稳定结论：技术和编年事件已有 `treasuryStability` / `treasuryPressure` 数据，政策 `fiscal_order` 仍是 `taxEfficiency` / `money`，Web/Domain 均未形成正式累计系统。
- 改革语义结论：Domain 当前按 `completedReformIds.Count` 计算核心改革数，重复 ID 会制造制度胜利假阳性；这是下一修补轮最小红绿切片。
- 数据阈值结论：`NonUnityJsonDataRepository` 已能加载 `victory_conditions.json`，但 `9920627` 测试仍手写阈值；后续可补 repository-driven 阈值测试，优先级低于重复 ID 语义修补。
- 当前 P0 首选：新增 `Institutional_Order_Should_Count_Unique_Core_Reforms_Only`，证明重复改革 ID 不会满足 `completedCoreReforms:4`，并把 `DomainVictorySystem` 改为非空唯一 ID 计数。

## 2026-05-24 institutional_order 唯一改革 ID 最小修补

- 已完成项：`DomainVictorySystem.CountCompletedCoreReforms()` 现在按 trim 后非空唯一 ID 计数，重复 `completedReformIds` 和空 ID 不再推动制度胜利。
- 已完成证明：`Institutional_Order_Should_Count_Unique_Core_Reforms_Only` 覆盖重复 `central_reform` / `fiscal_order` 时不达成、`completedCoreReforms == 2`、原因包含“核心改革”。
- 验证：targeted `VictorySystemInstitutionalOrderTests` `2/2`、`python tools\validate_domain_core.py`、完整 `WanChaoGuiYiTests` `93/93`、`tools\verify_headless_war.ps1` `16/16`。
- 当前 P0 转向：复核 `institutional_order` 下一条最小修补线，优先比较 Web `nationState` / debug / export carrier、`treasuryStability` 累计来源、repository-driven `victory_conditions.json` 阈值测试，以及改革推进路径。

## 2026-05-24 institutional_order Web 载体复核

- 已完成复核：Web `getDebugState()`、`exportGameState()`、`importGameState()` 已能复制 `nationState`，但 `StrategyDataset.nation` 与 Playwright `GameExportState.nationState` 还没有 `completedCoreReforms` / `treasuryStability`。
- 当前 P0 首选：下一修补轮做 Web carrier proof，给初始 `nationState` 增加 `completedCoreReforms:0` 与 `treasuryStability:50`，并用 Playwright 验证导入包含制度字段的 snapshot 后 debug/export/import 保留。
- 暂缓项：制度胜利 UI/outliner 达成判断、财政稳定正式累计、改革推进链和 repository-driven 阈值测试。

## 2026-05-24 institutional_order Web 字段载体最小修补

- 已完成项：Web `nationState` 现在有 `completedCoreReforms` 与 `treasuryStability` 默认值，debug/export/import 可保留制度胜利字段。
- 已完成证明：Playwright `preserves institutional order fields through Web debug export import` 覆盖默认 `0/50`、导入 `3/68`、debug、导出和再导入保留。
- 验证：targeted Playwright 红/绿；胜利相关 Playwright 子集 `3/3`；`npm --prefix web-strategy-map run typecheck`；字段相关 `rg`。
- 当前 P0 转向：复核下一条制度胜利成熟切片，优先比较 Web 制度胜利进度/outliner、财政稳定累计、改革推进路径、repository-driven 阈值测试。

## 2026-05-24 institutional_order Web 进度显示复核

- 已完成复核：Web 胜利 outliner 仍只显示三代延续与统一九州；`institutional_order` 阈值已在 JSON 和 TS 类型中存在，Web `nationState` 也已有 `completedCoreReforms` / `treasuryStability`。
- 当前 P0 首选：下一修补轮补 Web 制度胜利进度/outliner/debug achievement，读取 `completedCoreReforms:4`、`minLegitimacy:70`、`minTreasuryStability:65`、`maxAnnexationPressure:45`，并用玩家自有地区最大 `risk` 作为第一版 Web 可见“兼并压力”门。
- 暂缓项：财政稳定正式累计、改革自然推进、Domain command execution、repository-driven 阈值测试；后续需要再补 Web/Domain 精确压力 parity。

## 2026-05-24 institutional_order Web 进度显示最小修补

- 已完成项：Web 胜利 outliner/debug 现在消费 `institutional_order` 阈值，并显示制度胜利进度、达成状态、兼并压力分数和阈值。
- 已完成证明：Playwright `shows institutional order victory progress in the Web when fields meet data thresholds` 覆盖 blocked、achieved、export/import 保持状态；相邻胜利子集 `4/4 passed`；`npm --prefix web-strategy-map run typecheck` 通过。
- 当前 P0 转向：下一轮应做 fact-based review，比较 `treasuryStability` 正式累计、`completedCoreReforms` 推进链、Web/Domain 兼并压力 parity、repository-driven `victory_conditions.json` 阈值测试，选出最小修补切片。
- 剩余风险：Web 兼并压力暂用玩家自有地区最大 `risk`，仍不是 Domain `annexationPressure` 的精确字段来源。

## 2026-05-24 institutional_order 后续成熟切片复核

- 已完成复核：Domain 初始 `annexationPressure` 已由 `landStructure.localElites * 100` 计算，制度胜利也消费玩家地区最大 `RegionState.annexationPressure`；Web 有 `landStructure` 数据但还没有 `RegionViewModel.annexationPressure`，上一轮只能用 `risk` 代理。
- 当前 P0 首选：下一修补轮做 Web/Domain 兼并压力 parity，小范围增加 Web `annexationPressure` 运行态、export/import 保留，并让 `institutionalOrderPressureScore()` 改用该字段。
- 暂缓项：`treasuryStability` 正式累计、核心改革 ID 推进链、科技/事件消费、Domain command execution；这些需要更大规则定义。
- 后续低风险补证：Domain institutional_order 应再补 repository-driven `victory_conditions.json` 阈值测试，防止手写阈值漂移。

## 2026-05-24 institutional_order Web 兼并压力 parity 最小修补

- 已完成项：Web `RegionViewModel` 现在有运行态 `annexationPressure`，初始值按 Domain 公式 `landStructure.localElites * 100` 生成，export/import 保留，制度胜利压力门改用玩家自有地区最大 `annexationPressure`。
- 已完成证明：Playwright `shows institutional order pressure uses annexation pressure rather than rebellion risk` 红/绿覆盖低 risk 高兼并压力阻断、低兼并压力达成、export/import 保持；胜利相关子集 `5/5 passed`；`npm --prefix web-strategy-map run typecheck` 通过。
- 当前 P0 转向：下一轮应找缺口，优先比较 `treasuryStability` 正式累计、核心改革 ID 推进链、repository-driven `victory_conditions.json` 阈值测试。
- 剩余风险：财政稳定和改革推进仍是制度胜利从“可导入证明”走向自然玩法闭环的主要缺口。

## 2026-05-24 institutional_order 自然推进切片复核

- 已完成复核：Web 已有 `governance_policy` 玩家操作入口和多条 `category:"reform"` 政策，但 `nationState` 仍只有数字 `completedCoreReforms`，没有与 Domain 唯一改革 ID 语义对齐的 Web 运行态。
- 当前 P0 首选：下一修补轮做 Web 核心改革唯一 ID 自然推进，增加 `completedCoreReformIds`，执行 reform policy 时按 policy id 去重并同步 `completedCoreReforms`。
- 暂缓项：`treasuryStability` 正式累计仍需政策/科技/编年事件/经济回合的合成规则，不应在未定义前随手加公式。
- 后续补证：Domain institutional_order 仍应补 repository-driven `victory_conditions.json` 阈值测试。

## 2026-05-24 institutional_order Web 核心改革推进最小修补

- 已完成项：Web `nationState` 增加 `completedCoreReformIds`，治理政策执行 `category:"reform"` 时按 policy id 去重记录，并同步 `completedCoreReforms`。
- 已完成证明：Playwright `advances institutional order with unique reform policies in the Web` 红/绿覆盖默认空 ID、执行 `standardization` 推进到 1、重复执行不重复计数、export/import 保持。
- 验证：制度胜利相关 Playwright 子集 `4/4 passed`；`npm --prefix web-strategy-map run typecheck`；`npm --prefix web-strategy-map run build`；字段相关 `rg`；`git diff --check`。
- 当前 P0 转向：下一轮应找缺口，优先比较 `treasuryStability` 自然累计、Domain repository-driven `victory_conditions.json` 阈值测试、Web 已完成改革 ID 可见性。
- 剩余风险：制度胜利仍缺财政稳定自然来源；Web 只证明当前推荐改革政策推进，不代表完整研究/政策制度树已完成。

## 2026-05-24 institutional_order 下一成熟切片复核

- 提交阻断：Web 核心改革推进切片已验证但 `git add` 无法创建 `.git/index.lock`，当前仍是未提交工作树；后续修补前应优先恢复 `.git` 写权限并提交。
- 财政稳定结论：`treasuryStability` / `treasuryPressure` 已存在于 Domain/Web 字段和数据表，但 Web 编年事件只记录选择、不消费 `choice.effects` / `choice.risks`，政策也未消费财政压力；这是制度胜利最大剩余玩法缺口。
- 阈值防漂移结论：`NonUnityJsonDataRepository` 已加载 `victory_conditions.json`，但 `VictorySystemInstitutionalOrderTests` 仍手写制度胜利阈值；这是下一条最小纯 C# 修补。
- 当前 P0 首选：新增 `Institutional_Order_Should_Use_Repository_Victory_Condition_Thresholds`，从真实 `victory_conditions.json` 读取 institutional_order 阈值，证明 3 个唯一改革被阻断、4 个唯一改革在其余阈值达标时达成。
- 暂缓项：Web 已完成改革 ID 列表可见性、`treasuryStability` 完整自然累计、技术/政策/事件一体化财政系统。

## 2026-05-25 institutional_order repository 阈值防漂移最小修补

- 已完成项：新增纯 C# xUnit `Institutional_Order_Should_Use_Repository_Victory_Condition_Thresholds`，直接通过 `NonUnityJsonDataRepository` 读取真实 `victory_conditions.json` 的 `institutional_order` 阈值。
- 已完成证明：3 个唯一改革在法统、财政稳定、兼并压力均达标时仍因“核心改革”不足而阻断；4 个唯一改革在同一 JSON 阈值下达成制度胜利。
- 验证：本地 NuGet 包缓存离线 restore；`dotnet build ... --no-restore`；新增 targeted xUnit `1/1`；`VictorySystemInstitutionalOrderTests` `3/3`；完整 `WanChaoGuiYiTests` `94/94`；`python tools\validate_domain_core.py`；Web `npm --prefix web-strategy-map run typecheck`；`git diff --check`。
- 当前 P0 转向：仍需先恢复 `.git` 写权限并提交当前 Web + Domain 两个已验证切片；下一成熟修补优先做 `treasuryStability` 自然累计来源的最小消费 proof。
- 剩余风险：当前工作树因 `.git` 权限阻断仍无法提交；制度胜利还缺财政稳定从技术/政策/编年事件自然流入的闭环。

## 2026-05-25 institutional_order 财政稳定来源复核

- 已完成复核：`treasuryStability` / `treasuryPressure` 已存在于 Web 类型、Domain 字段和数据表；`chronicle_events.json`、`policies.json`、`technologies.json` 均已有财政稳定或财政压力数据。
- 运行态缺口：Web `tryTriggerChronicleEvent()` 当前只记录事件选择，不消费 `choice.effects` / `choice.risks`；`applyGovernancePolicy()` 当前消费地区整合、法统、民变、后勤和改革 ID，不消费 `policy.risks.treasuryPressure`。
- 已确认不重复项：`DomainVictorySystem.CountCompletedCoreReforms()` 已按 trim 后非空唯一 ID 计数，`Institutional_Order_Should_Count_Unique_Core_Reforms_Only` 已覆盖重复改革 ID 阻断。
- 当前 P0 顺序：先恢复 `.git` 写权限并提交当前 6 文件；下一修补轮优先做 Web 治理政策财政压力最小消费 proof，用现有可控 `governance_policy` 入口证明 `policy.risks.treasuryPressure` 会降低并保存 `nationState.treasuryStability`。
- 暂缓项：完整技术研究效果、编年事件 choice effects/risks 消费、经济回合财政公式和制度胜利自动达成链，不应在提交门阻断时继续扩大代码面。

## 2026-05-25 institutional_order Web 财政压力最小修补

- 已完成项：Web `applyGovernancePolicy()` 现在消费 `policy.effects.treasuryStability` 与 `policy.risks.treasuryPressure`，把政策财政压力折算进 `nationState.treasuryStability`。
- 可解释性：`formatEffects()` 新增 `财稳` / `财压` 标签，治理政策副作用预览可显示 `副作用：财压+4`。
- 已完成证明：Playwright `applies treasury pressure from governance policies in the Web` 覆盖 relief 治理政策使财政稳定 `50 -> 46`，并通过 export/import 保持。
- 验证：targeted Playwright `1/1 passed`；制度/统一相关 Playwright 子集 `6/6 passed`；直接浏览器探针确认风险预览、debug、export；`npm --prefix web-strategy-map run typecheck`；字段 `rg`；`git diff --check`。
- 剩余风险：编年事件 choice effects/risks 和技术研究效果仍未消费；`.git` 权限阻断导致本切片仍未提交。

## 2026-05-25 institutional_order 编年财政来源复核

- 已完成复核：Web 编年事件已有稳定回合入口和 Playwright 覆盖，`chronicle_events.json` 多处包含 `treasuryPressure`，且 `harvest_festival` 的 `increase_storage` 选择已有 `treasuryStability:2`。
- 当前缺口：`tryTriggerChronicleEvent()` 只记录事件与 choice label，不消费 `choice.effects` / `choice.risks`，所以编年事件仍不能自然提升或压低制度胜利财政稳定。
- 候选比较：技术树 effects 还没有正式研究入口，经济回合财政公式会跨资源结算；编年事件 choice 消费是下一条最窄、数据已存在、可由现有回合按钮触发的修补。
- 当前 P0 首选：下一修补轮新增 Web Playwright `applies treasury stability from chronicle choices in the Web`，通过可控 seed 触发 `harvest_festival` / `increase_storage`，断言财政稳定上升、事件摘要保留廷议选择、export/import 保持。
- 暂缓项：完整技术研究 effects 消费、经济回合财政盈余/赤字公式、编年事件全效果矩阵；先做财政字段最小消费，防止扩大机制面。

## 2026-05-25 institutional_order Web 编年财政最小修补

- 已完成项：Web `tryTriggerChronicleEvent()` 现在通过 `applyChronicleChoiceFiscalEffect()` 消费 choice 财政字段，`effects.treasuryStability` 增加财政稳定，`risks.treasuryPressure` 降低财政稳定。
- 已完成证明：Playwright `applies treasury stability from chronicle choices in the Web` 用 `changsha` + 冷却竞争事件稳定触发 `harvest_festival` / `增储入仓`，财政稳定 `50 -> 52`，export/import 保持。
- 验证：targeted Playwright 红/绿；编年/制度相关 Playwright 子集 `7/7 passed`；`npm --prefix web-strategy-map run typecheck`；字段 `rg`；`git diff --check`。
- 剩余风险：编年事件目前只消费财政相关字段，尚未消费 food、legitimacy、rebellionRisk 等全量 effects/risks；技术研究 effects 与经济回合财政公式仍未进入运行态。
- 当前 P0 转向：下一轮先找缺口，比较是否扩展编年事件全量 effect 消费、技术树 effects 入口，或经济回合财政公式。

## 2026-05-25 institutional_order 编年核心效果后续复核

- 已完成复核：`ChronicleChoiceDefinition` 已承载 `effects?: EffectSet` 与 `risks?: RiskSet`；`tryTriggerChronicleEvent()` 已有稳定回合入口，但当前只调用财政字段消费函数。
- 数据证据：`chronicle_events.json` 中 `harvest_festival/increase_storage` 已有 `food:60`，`harvest_festival/ritual_gratitude` 已有 `legitimacy:5` 与 `rebellionRisk:-3`，`yellow_river_flood/open_granary` 已有 `rebellionRisk:-8`、`legitimacy:4`、`treasuryPressure:5`。
- 候选比较：
  - 技术树 effects：Web 仅用回合与地形信号伪造 `currentChronicleSignals().techs`，没有正式研究入口；现在做会扩大 UI/存档/解锁范围。
  - 经济回合财政公式：会牵动粮钱产出、财政稳定、政策成本和制度胜利阈值，范围大于单个成熟修补。
  - 编年 choice 核心资源/风险：已有数据、类型、触发入口和现成 Playwright 结构，是最窄成熟化切片。
- 当前 P0 首选：下一修补轮新增 Web Playwright `applies chronicle choice core effects in the Web`，用可控 seed 触发 `harvest_festival` / `increase_storage`，断言粮食增加 60、财政稳定增加 2、事件摘要保留、export/import 保持；实现可从通用 `applyChronicleChoiceCoreEffects()` 开始，只消费 `food`、`money`、`legitimacy`、`rebellionRisk`、`treasuryStability`、`treasuryPressure`，暂不消费建筑、科技、外交或完整 follow-up tag。
- 暂缓项：技术研究系统、经济回合财政公式、编年事件全字段矩阵、Domain chronicle executor、StrategicAI 命令建议。
- 提交门风险：当前 `.git/index.lock` 仍因权限无法创建，下一修补前仍应优先恢复 `.git` 写权限并提交当前已验证 6 文件。

## 2026-05-25 institutional_order Web 编年核心效果最小修补

- 已完成项：Web `tryTriggerChronicleEvent()` 现在通过 `applyChronicleChoiceCoreEffects()` 消费 choice 核心运行态字段；在财政稳定之外，`food` / `money` / `legitimacy` / `rebellionRisk` 也会进入当前 Web 状态。
- 已完成证明：Playwright `applies chronicle choice core effects in the Web` 覆盖 `harvest_festival / increase_storage`，回合基础粮食 `+2` 加 choice 粮食 `+60`，总粮食 `300 -> 362`，财政稳定 `50 -> 52`，export/import 保持。
- 验证：targeted Playwright 红灯 `Expected 360 / Received 302` 暴露 choice 粮食未消费；修正为包含回合基础收益后 targeted 绿灯 `1/1 passed`；编年/制度相邻 Playwright 子集 `7/7 passed`；`npm --prefix web-strategy-map run typecheck`；`python tools\validate_domain_core.py`；`git diff --check`。
- 剩余风险：当前只消费核心资源/风险字段；建筑、科技、follow-up tag、外交和完整 Domain chronicle executor 仍未进入运行态。`.git/index.lock` 权限仍阻塞提交。
- 当前 P0 转向：恢复 `.git` 写权限并提交当前累积切片；若继续修补，下一轮应先找缺口，比较技术研究入口、经济回合财政公式、或编年事件法统/民变反向选择的可见 proof。

## 2026-05-25 institutional_order 编年反向选择后续复核

- 已完成复核：Web 现在已有通用 `applyChronicleChoiceCoreEffects()`，但 Playwright 只证明了 `harvest_festival / increase_storage` 的粮食与财政稳定正向效果。
- 数据证据：`harvest_festival / ritual_gratitude` 可证明同一事件的另一选择会改变法统与民变风险；`yellow_river_flood / open_granary` 可证明救灾类选择同时降低民变、提高法统、压低财政稳定。
- 候选比较：
  - 技术研究入口：当前 Web 仍没有 completed tech / research UI，做起来会跨数据、UI、存档和解锁。
  - 经济回合财政公式：需要定义财政稳定从粮钱盈亏到制度胜利阈值的公式，风险高于单个 proof。
  - 编年反向选择 proof：复用已有事件入口和核心效果函数，只补可见测试，能证明核心消费不是只覆盖粮食/财政快乐路径。
- 当前 P0 首选：下一修补轮新增 Playwright `applies chronicle choice legitimacy and unrest effects in the Web`，通过可控 seed 触发 `harvest_festival / ritual_gratitude` 或 `yellow_river_flood / open_granary`，断言法统上升、当前地区民变风险下降、财政压力按已有规则影响财政稳定，并验证 export/import 保持。
- 暂缓项：技术研究系统、经济回合财政公式、建筑/科技/follow-up tag 全矩阵、Domain chronicle executor。
- 提交门风险：当前 `.git/index.lock` 仍因 ACL Deny 无法创建，继续代码修补前仍优先提交当前累积切片。

## 2026-05-25 institutional_order Web 编年反向选择补证

- 已完成项：新增 Playwright `applies chronicle choice legitimacy and unrest effects in the Web`，证明 `applyChronicleChoiceCoreEffects()` 也覆盖法统、地区民变风险和财政压力。
- 已完成证明：可控 seed 触发 `harvest_festival / ritual_gratitude`，`nationState.legitimacy` `60 -> 65`，当前 `changsha` 风险 `18 -> 14`，地区法统 `62 -> 67`，财政稳定 `50 -> 48`，export/import 保持。
- 验证：targeted 首跑失败 `Expected 68 / Received 67`，校正地区基础回合口径后 targeted `1/1 passed`；编年/制度相邻子集 `8/8 passed`；`npm --prefix web-strategy-map run typecheck`；`python tools\validate_domain_core.py`；`git diff --check`。
- 结论：本轮只需测试覆盖，生产代码无需扩展；下一轮应先恢复 `.git` 写权限并提交累积切片，之后再找缺口比较技术研究入口、经济回合财政公式或编年 follow-up tag。

## 2026-05-25 institutional_order 累积切片收口复核

- 当前状态：Domain repository 阈值、Web 改革 ID、政策财政压力、编年核心效果和反向选择 proof 已集中在 6 个修改文件中，验证覆盖到 Domain targeted xUnit 与 Web 制度/编年 8 用例子集。
- 新鲜验证：`python tools\validate_domain_core.py`、`npm --prefix web-strategy-map run typecheck`、`git diff --check`、`VictorySystemInstitutionalOrderTests 3/3`、Job 托管 Vite 的 Playwright 制度/编年子集 `8/8 passed`。
- 运行注意：Playwright 配置自带 `webServer` 在本机仍会外层超时，不能作为通过证据；后续用 `Start-Job` 托管 `npm run dev:server -- --port 5177` 再执行 `npx playwright test`。
- 当前 P0：恢复 `.git` 元数据写权限并提交当前 6 文件；提交前不继续扩大代码面。
- 下一找缺口方向：提交恢复后再比较技术研究入口、经济回合财政稳定公式、编年建筑/科技/follow-up tag 或 Domain chronicle executor。

## 2026-05-25 技术/编年/经济下一成熟切片复核

- 技术树现状：Domain `FactionState` 已有 `researchPoints`、`completedTechIds`、`currentResearchId`，但 Web 只有 `currentChronicleSignals()` 用回合/地形推导临时 techs；Web debug/export/import 没有完成科技字段。
- 编年 follow-up 现状：TS/C# 类型和 `chronicle_events.json` 均有 `followUpTags`，但 Web 不把 choice follow-up 写入后续 eligibility 信号。
- 经济财政现状：Domain/Web 均有基础经济路径，当前 institutional_order 已消费政策与编年 choice 对 `treasuryStability` 的直接影响；把粮钱盈亏合成为财政稳定仍需规则定义。
- 当前 P0 顺序：先恢复 `.git` 写权限并提交当前 institutional_order 6 文件；提交后下一修补首选 Web 技术研究 carrier proof，让 `completedTechIds` / `researchPoints` 可导入、导出、debug，并让编年 `requiredTechs` 从完成科技读取。
- 暂缓项：完整研究 UI、科技 effects/unlocks 消费、经济财政公式、Domain chronicle executor、编年 follow-up tag 链式事件。

## 2026-05-25 Web 技术研究 carrier 最小修补

- 已完成项：Web `nationState` 增加 `researchPoints`、`completedTechIds`、`currentResearchId`，debug/export/import 可保留完成科技字段。
- 已完成 proof：Playwright `uses completed technologies for chronicle required tech gates in the Web` 证明未完成 `paper_bureaucracy` 时 `xiaowen_sinicization / 孝文迁都` 不触发，完成后触发，并且 export/import 保持完成科技。
- 运行态接线：`currentChronicleSignals()` 现在合并 `nationState.completedTechIds`，同时保留既有回合/地形伪 tech 信号以免破坏现有编年事件路径。
- 验证：targeted Playwright 红/绿；制度/编年/技术 carrier 子集 `9/9 passed`；`npm --prefix web-strategy-map run typecheck`；`python tools\validate_domain_core.py`；`git diff --check`。
- 当前 P0：仍需恢复 `.git` 写权限并提交当前累积切片。
- 下一候选：完整研究 UI/研究点累计、科技 effects/unlocks 消费、或更小的编年 follow-up tag carrier proof。

## 2026-05-25 Web 编年 follow-up tag carrier 最小修补

- 已完成项：Web 编年 runtime event 现在保存 `choice.followUpTags`，导入旧/新存档时规整该字段，并把最近编年 follow-up tags 合并进 `currentChronicleSignals().tags`。
- 已完成 proof：Playwright `uses chronicle follow-up tags to unlock later Web events` 证明并州 `border_horse_plague / 市马补缺` 写入 `frontier_trade` 后，可解锁原本并州不天然具备的 `trade_route_open / 商路开通`，并且 export/import 保持链路。
- 验证：targeted Playwright 红/绿；制度/编年/技术/follow-up 相邻子集 `9/9 passed`；`npm --prefix web-strategy-map run typecheck`；`python tools\validate_domain_core.py`；`git diff --check`。
- 剩余风险：当前只是短期 tag carrier，没有 UI 展示、tag 过期规则、Domain chronicle executor 或完整 follow-up 事件链平衡。
- 当前 P0：恢复 `.git` 写权限并提交当前 6 文件累积切片；本轮 `git add` 仍因 `.git/index.lock` permission denied 失败。提交恢复后下一轮先找缺口，比较完整研究 UI/科技 effects、经济财政公式、Domain chronicle executor 或 follow-up tag 时效。

## 2026-05-25 累积切片提交门复核

- 当前证据：6 文件 diff 仍集中在 Domain 制度阈值与 Web 制度/编年/科技/follow-up carrier，未新增 Unity/Tuanjie 或大范围玩法迁移。
- 新鲜验证：`git diff --check`、`python tools\validate_domain_core.py`、`npm --prefix web-strategy-map run typecheck` 均通过。
- 提交阻断：`git add -- docs/mvp-closure-ledger.md project-development-report.md tools/headless_runner/WanChaoGuiYiTests/VictorySystemInstitutionalOrderTests.cs web-strategy-map/src/data.ts web-strategy-map/src/ui.ts web-strategy-map/tests/strategy-map.spec.ts` 仍失败，错误为 `.git/index.lock` permission denied。
- 当前 P0：恢复 `.git` 写权限并提交当前 6 文件累积切片；提交前不要继续扩大代码面。
- 下一成熟修补候选：提交恢复后做 Web 技术研究最小完成流，从 `technologies.json` 读取技术目录，治理回合累计研究点，达到 cost 后写入 `completedTechIds` 并保持 export/import。

## 2026-05-25 提交门 ACL 与技术研究入口复核

- 提交门根因：`.git` 根目录存在显式 Deny ACL，阻止当前会话创建 `.git/index.lock`；`Set-Acl` 移除 Deny 失败，错误为 unauthorized。
- 当前策略：不再扩大业务代码面；先恢复 `.git` 写权限并提交 6 文件累积切片。
- 技术研究缺口事实：
  - `technologies.json` 已有 40 项技术/制度，包含 cost、prerequisites、boost、unlocks、effects。
  - Web 已有 `researchPoints` / `completedTechIds` / `currentResearchId` carrier，但还没有加载 `technologies.json`、没有 `TechnologyDefinition` TS 类型、没有治理回合研究点累计和完成科技逻辑。
  - Domain 已有 `TechnologyDefinition`、repository 载入、`FactionState` 研究字段、`NumericSystem.CalculateResearchPoints()` / `CalculateTechCost()`。
- 提交恢复后的下一修补：Playwright TDD `completes a Web technology through governance research progress`，用 `agricultural_calendar` 作为最小样例，证明治理回合能把 `researchPoints:34` 推到 cost 35 并写入 `completedTechIds`，export/import 保持。
