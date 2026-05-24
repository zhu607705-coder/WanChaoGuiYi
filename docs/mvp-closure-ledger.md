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
| P0 | 补 `institutional_order` 的 Domain/headless 字段来源与进度 payload | `domain-core/src/Domain/Victory` 或 `domain-core/src/Domain/Governance`、`tools/headless_runner/WanChaoGuiYiTests` | 先证明 `completedReformIds`、`treasuryStability`、法统和兼并压力能形成可解释进度；暂不先做 Web/UI/存档达成 |
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
