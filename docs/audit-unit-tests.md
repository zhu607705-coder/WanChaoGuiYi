# Unit Tests Audit — web-strategy-map/tests/unit

审查时间：2026-05-17
审查范围：19 个测试文件 + 2 个测试 helper

---

## 一、总体评价

测试套件覆盖维度丰富，涵盖：bundle 预算、headless 报告契约、data-contract 对齐、性能基线、数据加载鲁棒性、property-based 测试、Playwright 配置静态分析。整体质量较高，文档注释详细，断言逻辑清晰。

**主要风险：**
1. `data-contract-alignment` 和 `data-contract-emperor-alignment` 的历史失败原因已定位并回收——TS 已补全字段，测试注释已更新为回归门说明
2. `game-data-asset-url.test.ts` 的断言与实现行为已重新复核，当前测试作为路径穿越与反斜杠输入的回归门保留
3. `headless-vs-ui-numerics` 依赖真实 headless 报告；报告缺失时已显式 skip，但无法替代定期刷新 headless 报告
4. bundle 测试阈值来源已集中到 `bundle-budget-helpers.ts`，仍保留 600kB soft、500kB advisory、400kB renderer 的分层预算语义
5. 文档一致性缺口已回收：`audio-not-enabled.test.ts`、`game-data-asset-url.test.ts`、`data-asset-url.test.ts` 的 section 级审查结论已与正文和定向测试结果对齐

---

## 二、逐文件审查

### 2.1 `data-contract-alignment.test.ts`

**目的：** 检测 C# `DataModels.cs` 和 TS `types.ts` 之间字段漂移

**审查结论：通过**（轻微调整建议）

**断言逻辑：**
- `inBoth()` 用字符串包含检查，简洁有效
- `presentInCsButNotTs()` 正确识别单向漂移
- 关键字段表覆盖 population、foodOutput、taxOutput、manpower、localPower、rebellionRisk、neighbors、landStructure、legitimacyMemory、terrain — 覆盖充分

**已知失败原因：**
- `gameplaySourceReference`、`regionSpecialization`、`supplyNode` 三项在 C# 而非 TS — **但实际检查 `types.ts:172-189`（RegionDefinition），三项均已存在**
- 结论：**测试当前会通过，无需修改。原先描述当前失败的注释已回收。**

**风险：**
- 字符串包含检查可能被注释中的文本触发（假阳性），但当前场景不严重
- 没有检查 TS 在 C# 不在的情况（单向漂移另一方向）

**覆盖缺口：** `eraProfile` 的对齐验证已补充，当前作为 C#/TS 字段漂移回归门。

---

### 2.2 `data-contract-emperor-alignment.test.ts`

**目的：** 检测 EmperorDefinition 字段对齐

**审查结论：已通过（注释已更新为回归门说明）**

**断言逻辑：**
- `cs.includes('versionScope')` — C# 确认存在 ✓
- `ts.includes('versionScope')` — TS 确认存在 ✓（types.ts:305）
- `cs.includes('aiPersonality')` — C# 确认存在 ✓（DataModels.cs:32）
- `ts.includes('aiPersonality')` — TS 确认存在 ✓（types.ts:340-344）
- `cs.includes('diplomacySkills')` — C# 确认存在 ✓（DataModels.cs:29）
- `ts.includes('diplomacySkills')` — TS 确认存在 ✓（types.ts:331-337）
- `cs.includes('public sealed class EmperorScore')` — ✓
- `empBlock.includes('score')` — TS 有 score 字段（types.ts:312-325）✓

**结论：** 所有检查均通过。原先描述 TS 缺字段的过期注释已移除。

---

### 2.3 `headless-vs-ui-numerics.test.ts`

**目的：** 验证 headless 报告 keyDelta 为原始类型

**审查结论：缺失语义已回收**

**断言逻辑：**
- `existsSync(reportPath)` 不存在时统一为 `it.skip('no report yet')`
- `every keyDelta has primitive before/after values` — 逻辑正确，递归覆盖所有场景
- `every scenario has at least one numeric keyDelta` — 防止场景无数字
- fixture 级坏报告：空数组、空 scenarios、非数组 keyDeltas 会抛出明确错误

**问题：**
- 报告不存在时已改为显式 skip，不再 soft-pass 或直接 return
- 文件存在但为空数组或格式错误的情况已由 fixture 测试覆盖

**建议：** 后续如需进一步简化，可合并三份 headless 报告测试的读取逻辑。

---

### 2.4 `headless-report-numeric-fields.test.ts`

**目的：** 验证 keyDelta 数值比例和命名一致性

**审查结论：良好**

**断言逻辑：**
- 50% 数值比例阈值合理（`expect.soft ratio > 0.5`）
- numeric-sounding 字段名检查覆盖 money/food/soldiers/contribution/percent/count/integration/risk/legitimacy — 覆盖合理
- offenders 切片前 3 个输出，防止溢出

**风险：**
- 正则 `/money|food|soldiers|contribution|percent|count|integration|risk|legitimacy/i` 可能匹配嵌套字段（如 `nested.money`），假阳性低
- `expect.soft` 在 CI 中不会阻止通过，需配合 CI 配置

---

### 2.5 `headless-keydelta-numeric-coverage.test.ts`

**目的：** 深度验证特定场景的数值范围

**审查结论：严格且有效**

**断言逻辑：**
- `every numeric field has finite non-negative` — 使用 `Number.isFinite` + `v < 0` 双检查，覆盖 NaN/Infinity
- `low_supply_reduces_battle_power` 场景：dropRatio > 0.3（30% 降幅），足够严格
- `attacker_wins_and_occupies` 场景验证 money/food 存在

**亮点：**
- 场景不存在时用 `expect.soft(true).toBe(true)` 而非 skip，保留 CI 可见性
- 使用 `if (!sc)` 的软跳过而非硬 skip，避免测试被完全忽略

**风险：** 场景名硬编码（`low_supply_reduces_battle_power`、`attacker_wins_and_occupies`），如果 headless 报告重命名场景则测试失败

---

### 2.6 `performance-baseline.test.ts`

**目的：** 性能基线守卫

**审查结论：基线值合理，逻辑正确**

**基线值：**
| 指标 | 基线 | 合理性 |
|------|------|--------|
| `aggregateNationFood` on 100 regions | < 5ms | 宽松，100 区域应 < 1ms |
| 1000 sequential `aggregateNationMoney` | < 100ms | 合理（0.1ms/调用） |
| `loadStrategyDataset` with stub fetch | < 1000ms | 宽松，mock fetch 应 < 100ms |

**问题：**
- 第三个测试原先允许 `loadStrategyDataset()` 抛错后继续计时，不能证明真实成功路径性能；现已改为最小有效 stub dataset，并断言 route 成功构造。
- 性能测试在 CI 环境可能不稳定（VM 性能差异），但注释已说明"generous for CI"

**覆盖缺口：** `loadStrategyDataset` 成功路径性能已回收；真实 56 区域 fixture smoke 已回收。

**2026-05-20 5分钟找缺口复核：**
- 定向性能 baseline unit 通过：`1` file / `3` tests。
- 当前 `performance-baseline.test.ts` 注释称 `loadStrategyDataset on a 56-region in-memory fetch`，但实际 stub dataset 只构造 `4` 个 regions 和 `4` 个 shapes。
- 真实权威数据规模：`regions=56`、`map_region_shapes=56`、`historical_layers=56`、`emperors=13`、`chronicle_events=200`、`route_networks=6`，音频 JSON 位于 `game-data-source/audio`。
- 缺口判断：当前不是性能失败；下一轮修补可新增真实 fixture fetch helper，从 `game-data-source/data` 与 `game-data-source/audio` 读取 JSON，验证完整 `loadStrategyDataset()` 在真实 56 区域数据下仍小于宽松阈值并返回 56 个 regions。

**2026-05-20 5分钟修补回收：**
- `performance-baseline.test.ts` 新增真实 fixture fetch helper，按 `/game-data/data/*` 与 `/game-data/audio/*` 路径读取 `game-data-source` 下的权威 JSON。
- 新增真实数据 smoke：断言 `loadStrategyDataset()` 返回 `56` 个 regions、`56` 个 regionById、`200` 个 chronicle events，并加载 scene music 与 narration tutorial。
- 定向回归通过：`1` file / `4` tests；Web typecheck 通过；完整 Web unit 通过：`19` files / `56` tests。

---

### 2.7 `bundle-budget.test.ts`

**目的：** 通用 bundle 预算

**审查结论：阈值来源已整理，职责与其他 bundle 测试保持分层**

**断言逻辑：**
- 600kB JS chunk 阈值（`expect.soft largest < 600_000`）
- 50kB CSS 阈值
- 至少 2 个 JS chunks（`expect` 非 soft）
- 总 JS raw size < 1.1MB，防止 code-splitting 掩盖整体 payload 增长

**问题：**
- 已新增 `bundle-budget-helpers.ts`，集中维护 `BUNDLE_SIZE_BUDGETS` 和 dist asset 读取逻辑
- 600kB、500kB、400kB 仍保留为不同层级预算：总体 soft guard、Vite advisory hard guard、renderer 子 chunk hard guard
- 后续如调整预算，应优先改共享常量而不是单独改测试文件

**覆盖缺口：** 总 bundle size 缺口已回收；阈值来源统一已回收。

---

### 2.8 `bundle-budget-three-chunk.test.ts`

**目的：** 检测 three.js chunk 是否 > 500kB

**审查结论：与 2.7 分层互补，阈值来源已统一**

**数据：**
- 当前 `dist/assets/three-core-Ch6YQs_N.js` 即 three chunk
- 阈值：500kB（非 soft，CI 失败）
- `index-Cjrbs_Dw.js` 单独测试 < 250kB

**问题：** 500kB advisory 阈值已从共享常量读取；与 600kB soft budget 的差异现在是显式分层，而非散落常量。

---

### 2.9 `bundle-budget-three-renderer.test.ts`

**目的：** 检测 three-renderer chunk 是否 > 400kB

**审查结论：良好，与 2.7/2.8 互补**

**亮点：**
- 文件名前缀匹配 `three-renderer`（`three-renderer-QZDlbOjq.js` 实际存在）
- 非 soft 断言，CI 会失败
- 要求 three.js 分成至少 3 个 chunk（core + renderer + controls）— 有效防止退化

---

### 2.10 `playwright-time-budget.test.ts`

**目的：** 分析 Playwright spec 的超时和 poll 密度

**审查结论：已合并为唯一 Playwright 静态预算测试**

**断言逻辑：**
- 解析 `test.setTimeout(...)` 字面量，正确
- 解析 `test.setTimeout(playwrightTimeout(...))` 包装，正确
- poll 计数：按 `test(...)` 块分割后统计 `expect.poll(` 与 `expectDebug(` 合计
- 同时检查单测内最高 poll-like 调用数与全 spec 总调用数

**问题：**
- poll 计数依赖 `text.split(/test\(['"`]/)` — 如果 test 标题含特殊字符可能解析错误
- `playwright-poll-density.test.ts` 已合并删除，避免同一 spec 被两个 unit 文件重复解析

---

### 2.11 `playwright-poll-density.test.ts`（已合并删除）

**目的：** 控制 poll 密度

**审查结论：已合并进 `playwright-time-budget.test.ts`**

**断言逻辑：**
- 原有 `expect.poll(` + `expectDebug(` 合计口径保留
- 原有阈值保留：30/单 test、200/全 spec
- 删除重复文件后，Playwright 静态预算集中在一个测试文件中

**与 2.10 关系：** 重复解析和重复 poll 密度守卫已回收。

---

### 2.12 `region-shape-coverage.test.ts`

**目的：** 验证 region 无 shape 时 `loadStrategyDataset` 抛出

**审查结论：良好**

**断言逻辑：**
- stub: regions.json 有 `'a'` 但 map_region_shapes.json items=[]（空）
- `expect(caught).toBeInstanceOf(StrategyDatasetLoadError)` — 严格检查
- orphan shape 指向不存在 region 时必须抛出明确错误
- 实现检查：`data.ts validateRegionShapeCoverage` 已覆盖缺失 shape、重复 shape regionId、shape 指向未知 region

**覆盖缺口：** 反向覆盖缺口已回收；后续重点是 shape 几何结构有效性。

---

### 2.13 `region-neighbor-bidirectional.test.ts`

**目的：** 验证单向邻居关系时抛出

**审查结论：良好**

**断言逻辑：**
- `a.neighbors=['b']`，但 `b.neighbors=[]` — 经典单向边
- `expect(caught).toBeInstanceOf(StrategyDatasetLoadError)`
- `a.neighbors=['a']` 自环邻居必须抛出明确错误
- 实现检查：`data.ts` 已覆盖未知邻居、单向邻居和自环邻居

**覆盖缺口：** 自环邻居缺口已回收；后续重点是跨真实数据的拓扑一致性统计。

---

### 2.14 `nation-aggregation-property.test.ts`

**目的：** fast-check PBT 验证聚合逻辑

**审查结论：高质量**

**断言逻辑：**
- 非负性：`aggregateFood >= 0` — 正确
- 零值：`all rival` → 0 — 正确
- 确定性：`aggregateFood(regions) === aggregateFood([...regions])` — 正确
- 病理：`negative contribution → result >= 0` — 与当前实现对齐
- 真实 `regions.json`：按 Web 初始 player core 和 78% contribution 手算 food/money，必须与 `aggregateNationFood` / `aggregateNationMoney` 一致

**关键发现：**
- 旧注释描述的是已修复 bug；当前测试文件已改为回归门说明。
- **测试逻辑与实现对齐**。`data.ts:404` 会把负数 contribution clamp 到 0，测试会通过。
- 真实 JSON 聚合一致性缺口已回收。

---

### 2.15 `nation-aggregation-pathological-input.test.ts`

**目的：** 边界值 PBT（NaN/Infinity/极大数）

**审查结论：高质量**

**断言逻辑：**
- `Number.isFinite(result)` — 防 NaN/Infinity
- `result >= 0` — 防负数
- `Number.isInteger(result)` — 防浮点

**关键发现：**
- `foodOutput` 生成器包含 `fc.float({ noNaN: false })` — **会生成 NaN 值**
- `data.ts:411-414 sanitizeNonNegativeFinite 对 NaN/Infinity 返回 0，所以 test 正确通过
- 覆盖完整：Integer、Float、Infinity、NaN 全部覆盖

---

### 2.16 `dataset-duplicate-id.test.ts`

**目的：** 检测重复 id 时抛出

**审查结论：良好**

**断言逻辑：**
- 两个 `id='guanzhong'` 区域
- `expect(caught).toBeInstanceOf(StrategyDatasetLoadError)` — 严格
- 实现检查：`data.ts:343-353` validateRegionDefinitions 有 duplicate check

**覆盖缺口：** 没有测试多个不同文件中的 id 冲突（如 regions.json 和 emperors.json 中有相同 id）— 不属于同一 collection，实际不冲突

---

### 2.17 `dataset-error-shape.test.ts`

**目的：** 检测 schema 错误（items 缺失、空 JSON）

**审查结论：良好**

**断言逻辑：**
- `{}` 无 items 字段 → StrategyDatasetLoadError — **实现正确**（`loadCollection` 检查 items 存在）
- `''` 空字符串 → StrategyDatasetLoadError — **实现正确**（JSON.parse 失败被 catch）

**风险：** 空 JSON `{}` 场景下，测试期望 `StrategyDatasetLoadError` — 实现中 `loadJson` 不会在此失败（items 缺失由 `loadCollection` 检测），逻辑正确。

---

### 2.18 `audio-not-enabled.test.ts`

**目的：** 验证 StrategyAudio 禁用状态行为

**审查结论：已确认，覆盖已回收**

**断言逻辑：**
- `audio.setMode('war')` 然后检查 `debug.mode === 'war'` — 验证 pre-enable 状态可写
- `getDebugState().enabled === false` — 验证初始状态
- `enable()` 后继续检查 pre-enable 选择的 `mode` 保留
- 模拟一次播放失败后再成功播放，验证 `lastError` 可恢复清空

**代码确认：**
- `audio.ts:24` `mode: GameMode = 'governance'` — 默认 governance
- `audio.ts:64` `setMode()` 直接写 `this.mode = mode`，不检查 enabled — 测试断言正确

**覆盖缺口：** post-enable mode 保留、错误恢复、真实浏览器 autoplay 拒绝后 HUD `#audio-error` 呈现断言均已回收。

**2026-05-20 5分钟找缺口复核：**
- 定向回归通过：`npm --prefix web-strategy-map run test:unit -- --run tests/unit/audio-not-enabled.test.ts`，结果 `1` file / `4` tests。
- 当前 unit 覆盖 `StrategyAudio` 内部 `lastError` 写入、恢复清空和 pre-enable mode 保留。
- 当前 Playwright E2E 覆盖正常启用音频、帝皇 cue 和战争 narration 状态。
- 缺口确认：未模拟真实浏览器 `HTMLMediaElement.play()` 被 autoplay policy 拒绝后，`renderAudioHud()` 是否把 `state.lastError` 写入 `#audio-error`。
- 下一轮修补可限定在 Playwright 静态/E2E 测试层，先补 browser-side mock 拒绝播放与 HUD 文本断言；不需要改 runtime，除非测试暴露真实问题。

**2026-05-20 5分钟修补回收：**
- `strategy-map.spec.ts` 新增 `surfaces autoplay playback failures in the audio HUD`。
- 测试在 `openApp()` 前通过 `page.addInitScript()` mock `HTMLMediaElement.prototype.play()` reject，模拟浏览器 autoplay policy 拒绝。
- 断言 `#audio-status` 仍进入“音频已启用”，`#audio-error` 呈现 `autoplay blocked by test`，并同步检查 debug state 的 `audio.lastError`。
- 定向 E2E 通过：`npm --prefix web-strategy-map run test:ui -- --grep "surfaces autoplay playback failures in the audio HUD" --reporter=line --workers=1`，结果 `1` test passed；该命令同时通过 `sync:data` 与 `check:data-source`。
- Playwright 静态预算 unit 通过：`1` file / `3` tests；Web typecheck 通过。

---

### 2.19 `game-data-asset-url.test.ts`

**目的：** 检测路径穿越和反斜杠问题

**审查结论：已对齐，作为路径安全回归门保留**

**断言逻辑：**
- `'../../etc/passwd'` → `expect(result).not.toMatch(/%2E%2E|\.\./i)` — 检查输出不含 `..`
- `'art\\Portraits\\evil.png'` → `expect(result).not.toMatch(/%5C|\\/)` — 检查无反斜杠
- `''` → `expect(result).toBe('/game-data/')` — 边界行为

**关键发现：**
- `data.ts:268-275` 的实现逻辑：过滤掉 `segment !== '..'` 和 `segment !== '.'`，**路径穿越字符已被过滤**，输出不含 `..`
- **测试断言与实现对齐**（测试通过）
- 旧路径穿越注释已改为历史失败说明，当前实现已过滤 `..`。

---

### 2.20 `data-asset-url.test.ts`

**目的：** 检测 loadStrategyDataset 错误封装

**审查结论：已对齐，作为 domain error 回归门保留**

**断言逻辑：**
- 404 → `expect(error.name).not.toBe('Error')` — 检查错误非 raw Error
- 网络错误 → `expect(error.name).not.toBe('TypeError')` — 检查错误非 raw TypeError

**关键发现：**
- 实现中 `StrategyDatasetLoadError` **继承自 `Error`**，`error.name === 'StrategyDatasetLoadError'` — 测试断言正确
- `data.ts:425` 抛出 `new StrategyDatasetLoadError(fileName, \`HTTP ${response.status}\`)` — 错误类型正确
- **测试通过**，旧 raw Error 注释已改为当前 domain error 回归门说明

---

### 2.21 文档一致性复核回收

**2026-05-20 5分钟找缺口复核：**
- 定向 unit 通过：`npm --prefix web-strategy-map run test:unit -- --run tests/unit/audio-not-enabled.test.ts tests/unit/game-data-asset-url.test.ts tests/unit/data-asset-url.test.ts`，结果 `3` files / `9` tests。
- 仍需清理的文档标签：
  - `audio-not-enabled.test.ts` section 标题仍写“需要代码确认”，但代码确认和后续 E2E 已完成。
  - `game-data-asset-url.test.ts` section 标题仍写“有断言语义问题”，但正文已确认测试与实现对齐。
  - `data-asset-url.test.ts` section 标题仍写“断言语义不匹配”，但正文已确认 domain error 包装断言正确。
- 缺口判断：这是审查文档一致性问题，不是产品 bug；下一轮可只改上述三个 section 的审查结论标签和总结风险文字。

**2026-05-21 5分钟修补回收：**
- 三个 section 级审查结论已改为当前状态：`audio-not-enabled.test.ts` 为“已确认，覆盖已回收”，`game-data-asset-url.test.ts` 为“已对齐，作为路径安全回归门保留”，`data-asset-url.test.ts` 为“已对齐，作为 domain error 回归门保留”。
- “主要风险”同步更新：headless 报告缺失语义改为显式 skip 后的 freshness 风险；文档一致性缺口标记为已回收。
- 本轮只改审查文档，不改测试代码和 runtime。

---

## 三、覆盖缺口汇总

| 模块 | 测试文件 | 缺口 |
|------|---------|------|
| 数据加载 | `region-shape-coverage` | 已回收：shape 存在但 region 不存在 |
| 数据加载 | `region-neighbor-bidirectional` | 已回收：自环邻居（a.neighbors=['a']）由显式错误覆盖 |
| 数据加载 | `loadStrategyDataset` | 已回收：成功路径性能（所有文件有效返回） |
| 聚合 | `nation-aggregation-property` | 已回收：聚合结果与真实 JSON 数据的一致性 |
| 音频 | `audio-not-enabled` | 已回收：enable() 后的 mode 保留、错误恢复 |
| 音频 | `strategy-map.spec.ts` / HUD | 已回收：真实浏览器 autoplay 拒绝后 `#audio-error` 呈现 |
| bundle | 3 个 bundle 测试 | 已回收：总 bundle size（含所有 JS）的合理性 |
| headless | 3 个 headless 报告测试 | 已回收：报告文件为空数组、格式错误的处理 |

---

## 四、重复模式与可优化点

### 重复 1：bundle 测试阈值冲突（已回收）
- `bundle-budget.test.ts` → 600kB（soft）
- `bundle-budget-three-chunk.test.ts` → 500kB（non-soft）
- `bundle-budget-three-renderer.test.ts` → 400kB（non-soft）

**回收结果：** 已新增 `bundle-budget-helpers.ts`，三个测试共享 `BUNDLE_SIZE_BUDGETS` 和 dist asset loader。保留分工：`bundle-budget` 测总体，`bundle-budget-three-chunk` 测 chunk advisory，`bundle-budget-three-renderer` 测 renderer 子 chunk。

### 重复 2：Playwright 静态分析（已回收）
- `playwright-time-budget.test.ts` 计数 `expect.poll`
- `playwright-poll-density.test.ts` 计数 `expect.poll + expectDebug`

**回收结果：** 已删除 `playwright-poll-density.test.ts`，并把 combined poll 密度与总量预算合并进 `playwright-time-budget.test.ts`。保留 timeout、单测 poll-like 密度、全 spec poll-like 总量三类断言。

**2026-05-19 5分钟找缺口复核：**
- 两个 Playwright 静态分析 unit 定向通过：`2` files / `4` tests。
- 当前真实计数：`test.setTimeout` 最大值 `75_000ms`，低于 `90_000ms` 上限；`expect.poll=4`、`expectDebug=113`，合计 `117`，低于 `200` 总量上限。
- 单测内最高密度为 `exports and imports full game state across governance, army, logistics, and UI...`：`expectDebug=20`、`expect.poll=0`、合计 `20`，低于 `30` 上限。
- 缺口确认：`playwright-time-budget.test.ts` 的第二个断言只统计 raw `expect.poll`，对当前主流 `expectDebug` 密度已经弱化；`playwright-poll-density.test.ts` 才是更完整的守卫。下一轮修补可合并为共享 parser 或单一 `playwright-static-budget.test.ts`。

**2026-05-20 5分钟修补回收：**
- `playwright-poll-density.test.ts` 已合并删除。
- `playwright-time-budget.test.ts` 现保留三项守卫：timeout ≤ 90s、单 test `expectDebug+expect.poll` ≤ 30、全 spec `expectDebug+expect.poll` < 200。
- 定向回归通过：`1` file / `3` tests；Web typecheck 通过；完整 Web unit 通过：`19` files / `55` tests。

### 重复 3：headless 报告依赖同一文件（已回收）
- `headless-vs-ui-numerics.test.ts`
- `headless-report-numeric-fields.test.ts`
- `headless-keydelta-numeric-coverage.test.ts`

都读 `latest-war-report.json`，分别检查不同角度。

**回收结果：** 已新增 `headless-report-helpers.ts`，集中 report path、显式 skip、schema parser 和 typed report 结构。三份测试保留不同断言职责，但不再各自维护路径和 parser。

**2026-05-20 5分钟找缺口复核：**
- 三个 headless 报告 unit 定向通过：`3` files / `8` tests。
- 当前真实报告存在且健康：`passed=true`、`scenarioCount=16`、`scenarios=16`、`keyDeltas=34`、numeric-like 字段 `21` 个、非数字异常 `0` 个。
- 重复点确认：三份测试都各自构造 `latest-war-report.json` 路径；`headless-vs-ui-numerics.test.ts` 内部还重复读取两次，另外两份测试各自 `JSON.parse(readFileSync(...))`。
- 缺口判断：当前不是功能 bug，测试通过；下一轮可抽取共享 `headless-report-helpers.ts`，统一路径、显式 skip、schema parser 和 typed report 结构。

**2026-05-20 5分钟修补回收：**
- 已新增 `headless-report-helpers.ts`，三份 headless 报告测试复用 `describeHeadlessReport()` 与 `parseHeadlessReport()`。
- `headless-vs-ui-numerics.test.ts` 的异常 fixture 保留，直接验证共享 parser。
- 初次定向回归暴露遗留本地 parser 未删除导致的 `isRecord is not defined`，已移除后通过。
- 定向回归通过：`3` files / `8` tests；Web typecheck 通过；完整 Web unit 通过：`19` files / `55` tests。

**2026-05-19 5分钟找缺口复核：**
- 当前真实报告存在：`scenarioCount=16`、`scenarios=16`、`passed=true`。
- 三个 headless 报告测试定向运行通过：`3` files / `9` tests。
- 仍确认一个低风险但真实的测试可信度缺口：`headless-report-numeric-fields.test.ts` 在报告缺失时直接 `return`，`headless-vs-ui-numerics.test.ts` 用 soft-pass，`headless-keydelta-numeric-coverage.test.ts` 用 `it.skip`，三者缺失语义不一致。下一轮修补可统一为显式 skip 或共享 report loader。

**2026-05-19 5分钟修补回收：**
- 三份 headless 报告测试的真实报告缺失语义已统一为 `it.skip('no report yet')`。
- 定向回归通过：`3` files / `8` tests；减少的 1 个测试是原先无意义 soft-pass。

---

## 五、风险等级

| 等级 | 文件 | 原因 |
|------|------|------|
| **低** | `headless-vs-ui-numerics.test.ts` | 空数组/格式错误、真实文件缺失语义、共享 parser 均已补测/统一 |
| **中** | `bundle-budget.test.ts` | 阈值来源已集中；600kB soft budget 仍不会单独阻断 CI，需依赖 500kB hard guard 配合 |
| **低** | `performance-baseline.test.ts` | stub 成功路径和真实 56 区域 fixture smoke 均已覆盖 |
| **低** | `audio-not-enabled.test.ts` / `strategy-map.spec.ts` | post-enable mode 保留、lastError 恢复和浏览器 autoplay 拒绝后的 HUD 错误呈现均已补测 |
| **低** | `nation-aggregation-property.test.ts` | 真实 regions.json 聚合一致性已补测 |
| **低** | `data-contract-alignment.test.ts` | 当前失败态注释已过期，已更新为回归门说明 |
| **低** | `data-contract-emperor-alignment.test.ts` | TS 缺字段注释已过期，已更新为回归门说明 |

---

## 六、总结

19 个测试文件和 2 个测试 helper 整体质量高，断言逻辑正确，实现与测试对齐度好。**主要问题不是 bug，而是注释过期和少量重复模式**。

最需关注：
1. 历史过程段仍较长，但 section 级过期结论标签已回收
2. 真实浏览器 autoplay 拒绝后的音频错误 HUD 呈现已补自动化覆盖
3. 后续可继续清理审查报告中的历史过程段，避免误读为当前风险

覆盖率方面：数据加载层（duplicate id、missing id、missing shape、不对称邻居、schema 错误）覆盖完整；聚合逻辑有 PBT；headless 报告契约有覆盖但依赖外部文件；性能基线合理；Playwright 静态分析有效。
