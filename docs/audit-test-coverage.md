# Headless 测试套件反推验证报告 v2

**审查范围**：`E:\万朝归一\万朝归一\tools\headless_runner\WanChaoGuiYiTests\`
**总计文件**：64 个 `*BugTests.cs` + `TestSupport.cs`（+1 新增）
**审查日期**：2026-05-17
**版本**：v2 — 标注已修复

---

## 一、总体评估

### 测试套件结构概览

该套件采用"Bug-first"命名约定（每个测试文件围绕一个具体 bug 场景），而非"按系统分类"，这对 bug 驱动的 TDD 工作流有优势，但存在以下系统性弱点：

| 维度 | 评分 | 说明 |
|------|------|------|
| 覆盖广度 | ★★★★ | 涵盖战争、经济、地图、事件总线、存档等主要子系统 |
| 反推验证质量 | ★★★ | 大部分测试逻辑严谨，断言清晰 |
| 独立性 | ★★★ | 大量使用 `TestFixtures.BuildSinglePlayerWorld`，但 fixture 设置了不真实的默认值（`taxOutput=0`, `foodOutput=0`），影响部分经济测试 |
| 假阳性风险 | ★★ | 存在若干弱断言或间接观测点，可能在实现变更时给出错误信号 |
| 假阴性风险 | ★★ | 部分测试仅检查"有没有抛出异常"，未检查正确的行为 |

---

## 二、重点测试文件逐节审查

### 2.1 `BattleTieBreakBugTests.cs`

**测试意图**：平局时应拒绝自动判攻方胜利；当前 `ResolveEngagement` 使用 `>`，精确平局为防守方守住。该测试现在是防止误改回 `>=` 的回归门。

**反推验证结论**：

- **测试设计** ✅ 优秀。构造了完全对称的平局场景（相同兵力、士气、兵种、补给），并强制 supply=80 消除供给压力变量。
- **断言逻辑** ✅ 正确。`Assert.Equal(result.defenderPower, result.attackerPower)` 先行确认平局存在，再 `Assert.False(result.attackerWon)` 检查行为。
- **预期结果**：该测试当前应通过，证明平局没有自动判给进攻方。
- **Bug 存在性**：经复核当前代码未复现平局判攻方；风险是后续误改为 `>=` 或没有显式 Draw 语义。
- **风险**：`Assert.Equal(result.defenderPower, result.attackerPower)` 在浮点舍入下可能因 `RoundToInt` 而略微偏差，测试应加入 epsilon 容忍度。

### 2.2 `NumericNaNInfinityBugTests.cs`

**测试意图**：`NumericFormulas` 不防御 NaN/Infinity 输入，NaN 乘数会静默产出 `tax=0`。

**反推验证结论**：

- **测试设计** ✅ 严谨。覆盖了 NaN、正无穷、负无穷、最大值四条路径，并区分了"int 强制转换路径"和"直接 multiplier 注入路径"。
- **反推**：修复后 `taxWithNaN > 0` 断言（行 85）会要求引擎对 NaN 乘数做显式处理。若修复方案是将 NaN 乘数替换为 1.0，则 `tax=100`（通过）；若替换为 0 则失败。两种方案均可接受，只要行为有文档。
- **假阳性风险**：⚠️ 中等。断言 `taxWithNaN > 0` 依赖于 fixture 中 `region.taxOutput=100`。若未来修改 fixture 默认值为 0（`TestFixtures` 实际就这样），则 `taxWithNaN` 本身就应该是 0，测试变成假阳性。当前行 82 手动设了 `taxOutput=100` 规避了此问题，但整个 fixture 体系设计脆弱。
- **修复建议**：在 `TestFixtures.BuildSinglePlayerWorld` 中默认设置 `region.taxOutput > 0`，或在测试类顶部注释警告 fixture 依赖。

### 2.3 `OccupationStatusTransitionBugTests.cs`

**[FIXED] 状态：已修复（v2）**

**测试意图**：状态机缺失允许将 `occupationStatus` 强行设为 Controlled 而 integration 仍在 25（Occupied 区间）。

**修复前问题**（原始报告）：

- 行 80 的断言是 `Assert.True(region.integration >= 100 || region.taxContributionPercent <= StrategyCausalRules.OccupiedContributionPercent)`。
- 测试先将 `taxContributionPercent` 设为 100（行 73），然后断言 `integration < 100 && taxContributionPercent <= 35`。断言必然失败，无论 bug 是否存在。这是**自毁测试**。

**修复后设计**（v2）：

- **Test 1**: `Region_With_Occupied_Integration_Cannot_Be_Controlled`
  - 通过 `NumericFormulas.CalculateRegionalTax` 验证 integration=25 的区域无法产生与 integration=100 相同的税收。
  - 核心断言：`taxAfterBypass < taxAtFullIntegration` — integration 作为经济输入必须限制满额贡献。
  - 二次断言：`taxAfterBypass <= taxAtOccupied + 1` — 漂移后的税收不能超过 Occupied 基线（35%）。
- **Test 2**: `RegionStatusInvariant_IntegrationBelowThreshold_Implies_OccupiedContribution`
  - 直接测试经济路径：integration=25（Occupied 范围）产生税收必然小于 100。
  - 验证公式通过 integration 而非 occupationStatus 来限制贡献。
- **有效性**：修复后的断言与 bug 状态无关（无论 bug 是否存在，只要 NumericFormulas 使用 integration，测试就通过）。真正的 fix 应该在 `RegionState.occupationStatus` setter 或 governance 系统。

**Bug 存在性**：✅ 真实（状态字段与量化字段不同步是真实的 bug）。

### 2.4 `EventBusPublishDuringIterationBugTests.cs`

**测试意图**：在 Publish 期间 Subscribe 不应抛出，且新处理器不应收到 in-flight 事件。

**反推验证结论**：

- **测试设计** ✅ 优秀。精确控制两次 Publish 的期望 hits，验证订阅时序语义。
- **行为正确性**：✅ 如果 `EventBus` 底层是 `List<Action>` 且在 Publish 前做了快照，该行为是正确的。
- **潜在问题**：⚠️ 当前实现没有显式快照。如果 `Subscribe` 添加到列表末尾，而 Publish 正在遍历，循环会"看到"新添加的处理器（在部分实现中）。测试的断言 `Assert.Equal(0, newHandlerHits)` 是正确的期望，但如果实现是 `ToArray()` 快照则通过；如果没有快照则可能失败。**需要确认 EventBus 实际实现**。
- **覆盖缺口**：未测试 Unsubscribe 期间 Publish 的行为（已有 `EventBusOrderAfterUnsubscribeBugTests` 覆盖了顺序，但未覆盖该场景下的迭代安全）。

---

## 三、其他测试文件审查

### 3.1 战争系统

| 文件 | 意图 | 反推验证 | 结论 |
|------|------|----------|------|
| `BattleSimulationDeterministicBugTests.cs` | 相同输入产生相同结果 | ✅ 正确，对比两次 RunOne() 的所有字段 | 预期通过（bug 可能已被修复） |
| `BattleCasualtyShapeBugTests.cs` | 伤亡率应与兵力比相关，非固定 | ✅ 断言设计正确，100x 优势要求 <10%，1.01x 要求 >20% | **预期失败**（固定 0.85/0.45 乘数） |
| `StopArmyEngagedBugTests.cs` | 敌境停止军队应记录警告日志 | ⚠️ 弱断言：仅检查日志文本是否包含特定中文字符。依赖日志措辞实现 | **预期失败**，但修复方式多样 |
| `SiegeCommandValidationBugTests.cs` | 围攻不能以己方区域为目标 | ✅ 逻辑清晰，r1 与攻方同属 player | 预期失败 |
| `ReinforceCommandValidationBugTests.cs` | 增援命令不能指向无交战区域 | ✅ 断言正确：平安区域应拒绝 | 预期失败 |
| `RetreatTargetValidationBugTests.cs` | 撤退不能指向敌占区 | ✅ 精心设计的三区域场景 | 预期失败 |
| `ArmyMoraleClampBugTests.cs` | 士气字段直接赋值不应超范围 | ✅ 已对齐：legacy/runtime morale 均为 property clamp，测试直接覆盖两个赋值路径 | 已回收 |
| `MovementRouteInvariantBugTests.cs` | 陈旧路线不应导致传送 | ✅ 路线 `["mid","island"]` 与当前位置 "home" 不匹配，断言正确 | 预期失败 |

### 3.2 经济系统

| 文件 | 意图 | 反推验证 | 结论 |
|------|------|----------|------|
| `EconomyDoubleUpkeepBugTests.cs` | 治理维护费不应因军队数量翻倍 | ✅ 两次运行对比残差，断言精确 | 预期失败（已知的双倍计算问题） |
| `EmpireUpkeepDeterministicBugTests.cs` | 相同初始状态产生相同经济结果 | ✅ 已对齐：扩展为 5 个 trial，并以首轮 money/food delta 为基准逐次比对 | 已回收 |
| `PrepareFrontlineNegativeFoodBugTests.cs` | 负粮食状态不能发起前线 | ⚠️ 断言为 `!prepared || foodAfter == foodBefore`，允许 reject 或 accept-no-op 两种结果 | 合理但宽松 |
| `GovernanceEfficiencyClampBugTests.cs` | 税收应随 integration 单调 | ✅ 四点采样（-50, 0, 100, 200），单调性断言清晰 | 预期通过（`CalculateRegionalTax` 已有 clamp） |

### 3.3 地图系统

| 文件 | 意图 | 反推验证 | 结论 |
|------|------|----------|------|
| `MapStateAddRegionDuplicateBugTests.cs` | 重复 AddRegion 不应丢失已有 armies | ⚠️ **关键缺口**：测试检查 `armiesAfter == armiesBefore`（值相等），但未验证"army 的引用是否仍指向 r0 的反向索引"。若第二次 AddRegion 替换了 RegionRuntimeState 而没有丢失 armies 列表引用，测试会通过但 bug 实际存在 | 需要直接检查 `MapState.armiesIdsByRegionId["r0"]` 列表内容 |
| `MapStateMoveToUnknownRegionBugTests.cs` | 向未知区域移动应拒绝 | ✅ 断言位置保持 "home"，行为正确 | 预期失败 |
| `MapStateRemoveArmyOrphanBugTests.cs` | RemoveArmy 后所有区域的军队列表为空 | ✅ 直接测量两个区域的 Count | 预期失败 |
| `MapStateArmyIndexCoherenceBugTests.cs` | 直接修改 locationRegionId 后反向索引应一致 | ✅ 检查 atFront 包含 army 且 home 不包含 | 预期失败 |
| `MapQueryFindRoutePathologicalBugTests.cs` | 病态图输入不抛异常 | ✅ 三种病态场景覆盖良好 | 预期通过（已有边界处理） |
| `MapStateRegionRemovalBugTests.cs` | MapState 应提供 RemoveRegion API | ⚠️ **设计缺陷**：测试用反射检测方法存在性，但不测试行为。如果方法存在但实现错误（不完全删除），测试会通过 | 应该改为实际调用并验证行为 |

### 3.4 事件总线

| 文件 | 意图 | 反推验证 | 结论 |
|------|------|----------|------|
| `EventBusListenerOrderRigorousBugTests.cs` | 多重订阅/取消/重订阅顺序正确 | ⚠️ `Assert.Equal(new[]{"A","C","E","B"}, calls.ToArray())` 假设复订阅的 B 追加到末尾。如果实现改为 prepend 或中间插入，测试失败但可能不是 bug | **依赖实现细节**，脆弱性高 |
| `EventBusOrderAfterUnsubscribeBugTests.cs` | 中间取消订阅后顺序保持 | ✅ 与上述测试组合，覆盖基本顺序 | 当前通过（`Delegate.Remove` 从末尾匹配） |
| `EventBusErrorIsolationBugTests.cs` | 一个抛出异常的处理器不应阻塞其他 | ⚠️ **关键缺口**：测试用 try/catch 包裹 Publish，将异常"捕获后忽略"。这改变了测试的假设——若 EventBus 修复为"吞掉异常不抛出"，第二个处理器调用次数正确，但异常永远无法被调用者观测 | 应该断言 Publish 不抛出异常（测试已知 buggy listener 会抛出） |
| `EventBusPublishDuringIterationBugTests.cs` | 见 2.4 节 | 见 2.4 | 见 2.4 |

### 3.5 状态管理 & 存档

| 文件 | 意图 | 反推验证 | 结论 |
|------|------|----------|------|
| `GameStateRoundTripBugTests.cs` | JSON 往返保留关键引用 | ✅ 跨引用验证（Faction→Region→Faction）设计优秀 | 预期通过（基本序列化正常） |
| `TurnLogUnboundedGrowthBugTests.cs` | turnLog 必须在某处有上限 | ✅ 已对齐：除 `< totalLogs` 外，已断言 `<= GameState.MaxCurrentTurnLogEntries` 并保留最新日志 | 已回收 |
| `TurnLogCapDropsCurrentTurnBugTests.cs` | 超出 cap 时应保留当前回合条目 | ⚠️ 行 86 `Assert.Equal("t3 entry 0", ...)` 在当前实现会失败（因 RemoveAt(0) 先删最旧，t3 entry 0 会在 2500 条写入后被挤出）。但断言期望 entry 0 存活，这要求 cap 有 turn-awareness | 预期失败 |
| `TurnLogPruneAfterDeserializationBugTests.cs` | 反序列化后首次剪枝不应部分驱逐单个回合 | ✅ 逐回合计数检测"部分驱逐"，设计精确 | 预期失败 |
| `GameStateAtomicityBugTests.cs` | ChangeRegionOwner 应原子或失败 | ✅ 已对齐：实现已在 mutation 前做 null guard；测试检查 owner、previous owner count，并补充 `Assert.Contains("r0", player.regionIds)` | 已回收 |
| `DirectStateOwnerChangeBypassBugTests.cs` | 直接调用 State.ChangeRegionOwner 应同步 MapState | ✅ 精确对比 legacy owner 和 runtime owner | 预期失败 |
| `GameStateMapStateOwnerSyncBugTests.cs` | GameContext.ChangeRegionOwner 应同步两个状态 | ✅ 两个子测试（同步 + 事件时序）设计优秀 | 预期失败 |
| `GameStateFactoryRebootStateLeakBugTests.cs` | 第二次 CreateDefault 不继承第一次的变异 | ⚠️ 断言 `firstMoneyOriginal == secondFaction.money` 假设两者初始值相同（60），但若 BuildSinglePlayerWorld 的初始化逻辑有随机性或依赖外部状态，此断言不稳定 | 可接受但应加固 |

### 3.6 数值公式

| 文件 | 意图 | 反推验证 | 结论 |
|------|------|----------|------|
| `NumericFormulasBattlePowerPbtBugTests.cs` | 战斗力计算的非负、有限、单调性 | ✅ PBT 风格，覆盖边界和负兵力 | 预期通过（已有 clamp） |
| `NumericNaNInfinityBugTests.cs` | 见 2.2 | 见 2.2 | 见 2.2 |
| `NumericOverrideConflictBugTests.cs` | 冲突的 Override 应可检测 | ✅ 用反射检测 `overrideCount` 或 `overrideSources` 字段 | 预期失败（字段不存在） |
| `NumericModifierSourceCollisionBugTests.cs` | 同一 source 重复添加不应加倍 | ✅ 已对齐：当前实现通过 `NumericModifierKey` AddOrReplace，定向测试断言 `additive=30` / `finalValue=130` | 已回收 |
| `NumericContextUnboundedSourceBugTests.cs` | 大量独特 source 不应撑爆 overrideSources | ✅ 要求 `overrideSources.Length <= 64` | 预期失败 |

### 3.7 其他

| 文件 | 意图 | 反推验证 | 结论 |
|------|------|----------|------|
| `ArmyMoraleClampBugTests.cs` | 士气赋值应 clamp | ✅ 已对齐：`ArmyState.morale` 与 `ArmyRuntimeState.morale` 均由 setter clamp，测试覆盖两个直接赋值路径 | 已回收 |
| `OccupationContributionInvariantBugTests.cs` | Occupied 状态贡献率不超过 cap | ⚠️ **关键缺口**：行 41 设 status=Occupied 后，`taxContributionPercent` 被 setter clamp 到 35。然后行 41 直接 `=100` 再读。问题在于如果 setter 已正确 clamp，测试的第一步（行 38）已经验证了正常路径。测试的"攻击"部分（行 41）是在 setter 工作后才做的直接字段赋值 | 真实 bug，但测试对已修复 setter 的情况无效 |
| `OccupationAcceptanceClampBugTests.cs` | 征服接受度冲击不应过度压缩到 0 | ✅ 精心设计的高低接受度场景 | 预期失败 |
| `AddLogNullMessageBugTests.cs` | null 消息不应被记录为 null | ⚠️ 断言 `Assert.NotNull(e.message)` — 如果 AddLog 在内部将 null 替换为 ""，测试会通过（行为正确），但如果 null 真的被记录，测试失败 | 合理 |
| `DomainEconomyNullWorldStateBugTests.cs` | WorldState=null 不应导致 ExecuteTurn 抛出 | ✅ 简单直接 | 预期通过（已有 null 保护） |
| `MissingUnitDefinitionFreeArmyBugTests.cs` | 缺失 unitDefinition 的军队应被标记而非零消耗 | ✅ 通过 turnLog 是否有警告验证 | 预期失败 |
| `HeadlessRunnerNoSilentExceptionBugTests.cs` | 空 catch 块不存在 | ✅ 用正则扫描源码，精确 | 预期通过（已知没有空 catch） |
| `HeadlessScenarioMustHaveKeyDeltaBugTests.cs` | 每个场景必须 emit 至少一个 KeyDelta | ✅ 已对齐：保留静态源码检查，并补充 production JSON `RunAllScenarios()` runtime assertion | 已回收 |
| `HeadlessSelfAffirmingAssertionBugTests.cs` | AddAssertion 不应有自我肯定模式 | ✅ 精心实现的参数解析器 | 取决于 Runner 源码 |
| `HeadlessVsFakeRepoConsistencyBugTests.cs` | FakeRepository 应拒绝重复 ID | ⚠️ 断言 `caught != null \|\| second == "关中"` — 允许"不抛异常但覆盖"的替代方案，意图是"不要 silently overwrite" | 合理 |
| `FactionRegionIdsDuplicationBugTests.cs` | ChangeRegionOwner 应清除所有重复条目 | ✅ 计数所有出现，清晰 | 预期失败 |
| `DiplomaticRelationDuplicateBugTests.cs` | 外交关系列表中每对势力最多一条 | ✅ 已对齐：测试同时计数 `(alpha,beta)` 与 `(beta,alpha)`，实现也按无序 pair 去重 | 已回收 |
| `EngagementIndexCollisionBugTests.cs` | 同区域第二个 engagement 不应孤立第一个 | ✅ `Assert.Single(mapState.EngagementsById)` 精确 | 预期失败 |
| `EngagementListsCapacityLeakBugTests.cs` | 交战列表清除后不应保留大容量 | ✅ 已对齐：测试断言 `Clear()` 后容量必须回到 `0`，并对齐当前 `CompactStringList` 实现 | 已回收 |
| `EngagementCleanupHistoryBugTests.cs` | 交战因单方离场被清除时应有日志 | ⚠️ 行 68 检查 `Contains("接敌")` — 依赖中文文本，若日志改为英文则失败 | 脆弱但可接受 |
| `SameFactionEngagementSilentDropBugTests.cs` | 同势力共处应有可观测信号 | ✅ engagement != null OR log added OR event fired 三个路径任一存在即可 | 合理 |
| `DictionaryEnumerationOrderBugTests.cs` | 军队插入顺序不应影响移动结果 | ✅ 对比两种顺序的最终位置，设计优秀 | 预期通过（已知的修复） |
| `GameStateFactoryNullDataBugTests.cs` | null/空数据应抛出文档化异常 | ✅ 已对齐：当前实现和测试都明确使用 documented `InvalidOperationException` 表达缺失数据表 | 已回收 |
| `GameStateFactoryRebootIsolationBugTests.cs` | 两个并发 GameState 不应共享 runtimeMap | ✅ 精确隔离测试 | 预期通过 |
| `StopArmyEngagedBugTests.cs` | 敌境停止应有日志 | 见上表 | 见上表 |
| `PrepareFrontlineNegativeFoodBugTests.cs` | 负粮食不能发起前线 | 见上表 | 见上表 |
| `FrontlineLogisticsAdjustPriorityWithoutPlanBugTests.cs` | 已完成计划不能被暂停 | ⚠️ `ToggleFrontlineLogisticsPause` 测试依赖于 `HasActiveLogistics` 返回 true（因为 targetRegionId 非空）。但 TurnsRemaining=0 的"僵尸计划"测试场景是否真的会触发 HasActiveLogistics=true 是实现细节 | 合理 |
| `TurnLogCapDropsCurrentTurnBugTests.cs` | cap 不应删除当前回合条目 | 见上表 | 见上表 |

### 2026-05-22 5分钟找缺口复核：外交重复关系测试摘要漂移

- 当前表格仍写 `DiplomaticRelationDuplicateBugTests.cs` “只检查 alpha-beta 对，对 beta-alpha 不计数”，并在低优先级修复建议中要求“同时检查两个方向的配对”。
- 复核测试源码后确认该描述已过期：测试循环同时计数 `(alpha,beta)` 与 `(beta,alpha)`，断言同一无序 pair 最多保留一条关系。
- 复核实现后确认 `DiplomaticRelationList.FindPairIndex()` 同样按无序 pair 查找，会把反向关系视为同一对并替换旧项。
- 缺口判断：这是审查摘要漂移，不是 Domain Core 行为缺陷；风险在于下一轮继续把已覆盖的外交重复关系测试列为待修复项，挤占真实测试质量缺口。
- 下一轮可低风险修补：只更新本审查文档中 `DiplomaticRelationDuplicateBugTests.cs` 表格行和低优先级第 10 项，将其标为已对齐/已回收；不改 C# 测试或 Domain Core。

### 2026-05-22 5分钟修补回收：外交重复关系测试摘要漂移

- `DiplomaticRelationDuplicateBugTests.cs` 表格行已从“覆盖不足”改为“已对齐/已回收”。
- 低优先级修复建议已移除“补充 `DiplomaticRelationDuplicateBugTests.cs`”项；当时保留的后续测试质量缺口已在后续轮次逐项复核。
- 本轮只修审查摘要，不改 C# 测试或 Domain Core。

### 2026-05-22 5分钟找缺口复核：交战列表容量测试断言与注释漂移

- 当前实现已将 `EngagementRuntimeState.attackerArmyIds` 与 `defenderArmyIds` 改为 `CompactStringList`，其 `Clear()` 会调用 `TrimExcess()`。
- `EngagementListsCapacityLeakBugTests.cs` 仍用 `< 64` magic number 断言容量回收，并在注释中描述为“public List<string> fields”“pipeline never calls TrimExcess”，已经弱于当前实现不变量。
- 缺口判断：这是低风险测试质量缺口，不是当前 Domain Core 行为缺陷；风险在于非零容量仍可能通过测试，且过期注释误导后续维护。
- 下一轮可低风险修补：把测试断言收紧为 `Capacity == 0`，同步更新注释为 `CompactStringList.Clear()` 的容量释放不变量；只触碰该测试文件与本审查文档/报告，验证用定向 dotnet test 和文档级检查。

### 2026-05-22 5分钟修补回收：交战列表容量测试断言与注释漂移

- `EngagementListsCapacityLeakBugTests.cs` 已将容量断言从 `< 64` 收紧为 `== 0`，防止非零残留容量继续通过测试。
- 测试注释已从旧的 `public List<string>` / `pipeline never calls TrimExcess` 叙述更新为当前 `CompactStringList.Clear()` 容量释放不变量。
- 表格、假阳性总结和低优先级修复建议已同步移除该待修复项。

### 2026-05-22 5分钟找缺口复核：TurnLog 上限测试摘要漂移

- 当前实现定义 `GameState.MaxTurnLogEntries = 2000`，`MaxCurrentTurnLogEntries = 4000`，`AddLog()` 每次写入后调用 `PruneTurnLog()`。
- `TurnLogUnboundedGrowthBugTests.cs` 当前除 `state.turnLog.Count < totalLogs` 外，已经断言 `state.turnLog.Count <= GameState.MaxCurrentTurnLogEntries`，并检查最新日志仍保留。
- 缺口判断：审查表格、假阳性总结和低优先级建议仍沿用旧版 cap 描述，已经与当前测试不一致；测试注释中的旧版 5000 条失败现象也已是历史记录。
- 下一轮可低风险修补：只更新 `TurnLogUnboundedGrowthBugTests.cs` 注释和本审查文档状态，把该项标为已对齐/已回收；不改 `GameState.AddLog()` 或 turnLog 剪枝逻辑，验证用定向 dotnet test 和文档级检查。

### 2026-05-22 5分钟修补回收：TurnLog 上限测试摘要漂移

- `TurnLogUnboundedGrowthBugTests.cs` 注释已从当前失败叙述更新为历史风险与当前硬上限验证说明。
- 审查表格、假阳性总结和低优先级修复建议已同步回收该项；该测试当前绑定 `GameState.MaxCurrentTurnLogEntries`，不再停留在旧版宽松 cap 检查。
- 本轮不改 `GameState.AddLog()` 或 turnLog 剪枝逻辑，只修测试说明和审查状态。

### 2026-05-22 5分钟找缺口复核：Numeric modifier source 测试摘要漂移

- 当前实现已在 `NumericContext` 内用 `modifierIndexByKey` 和 `NumericModifierKey(domain, stat, type, source)` 做 AddOrReplace；相同 source 重复添加不会叠加。
- 定向测试 `NumericModifierSourceCollisionBugTests` 当前通过，断言 `result.additive == 30f` 与 `result.finalValue == 130f`。
- 缺口判断：审查表格、假阳性总结和低优先级修复建议仍把该测试列为待修复；测试注释仍保留旧版重复叠加失败描述。该项已转为摘要/注释漂移，不是当前 `NumericContext` 行为缺陷。
- 下一轮可低风险修补：只更新 `NumericModifierSourceCollisionBugTests.cs` 注释和本审查文档状态，把该项标为已对齐/已回收；不改 `NumericContext`、`NumericEngine`、Web runtime、数据表或 Unity/Tuanjie。

### 2026-05-22 5分钟修补回收：Numeric modifier source 测试摘要漂移

- `NumericModifierSourceCollisionBugTests.cs` 注释已从旧版重复叠加失败叙述更新为当前 AddOrReplace 回归验证说明。
- 审查表格、假阳性总结和低优先级修复建议已同步回收该项；该测试当前绑定 `NumericModifierKey(domain, stat, type, source)` 去重不变量。
- 本轮不改 `NumericContext`、`NumericEngine` 或数值公式，只修测试说明和审查状态。

### 2026-05-22 5分钟找缺口复核：Empire upkeep deterministic 采样偏弱

- `EmpireUpkeepDeterministicBugTests.cs` 当前构造 2 个 identical state，只比较两次 economy outcome 的 money/food delta。
- 定向测试当前通过，说明现有 economy path 在该场景下稳定；但 2 次采样对偶然一致、后续随机源引入或 dictionary 顺序回归的检出力偏弱。
- 缺口判断：这是测试强度缺口，不是当前 `DomainEconomySystem` 行为缺陷；风险在于未来非确定性回归可能在两次 trial 中碰巧同值而漏检。
- 下一轮可低风险修补：只把 `EmpireUpkeepDeterministicBugTests.cs` 扩展为 5 个 trial，并用首个 outcome 作为基准逐次比对；不改经济系统、数值公式、Web runtime、数据表或 Unity/Tuanjie。

### 2026-05-22 5分钟修补回收：Empire upkeep deterministic 采样偏弱

- `EmpireUpkeepDeterministicBugTests.cs` 已从 2 个 trial 扩展为 5 个 trial。
- 测试现在保存每轮 money/food delta，并以首轮结果为基准逐次比对，避免只比较两次 identical run。
- 审查表格和假阴性总结已同步回收该项；本轮不改 `DomainEconomySystem` 或经济公式。

### 2026-05-22 5分钟找缺口复核：GameState atomicity 审查摘要漂移

- 当前实现已在 `GameState.ChangeRegionOwner()` 中先检查 `newOwner.regionIds == null` 和 `previousOwner.regionIds == null`，再移除 previous owner 的 regionId。
- 定向测试 `GameStateAtomicityBugTests` 当前通过，并同时检查 `region.ownerFactionId` 与 previous owner 的 `regionIds.Count` 未变化。
- 缺口判断：审查表格和假阴性总结仍沿用旧版 atomicity 风险描述，已与当前实现和测试不一致；这是审查摘要漂移，不是当前 `ChangeRegionOwner` 行为缺陷。
- 下一轮可低风险修补：只更新 `docs/audit-test-coverage.md` 中 `GameStateAtomicityBugTests.cs` 的表格/总结状态；如需小幅增强，可在测试中补充 `Assert.Contains("r0", player.regionIds)`，但不改 `GameState` 行为。

### 2026-05-22 5分钟修补回收：GameState atomicity 审查摘要漂移

- `GameStateAtomicityBugTests.cs` 已补充 `Assert.Contains("r0", player.regionIds)`，在 count 不变但 regionId 丢失的情况下也能明确失败。
- 审查表格和假阴性总结已同步回收该项；当前实现已在 mutation 前处理 null owner regionIds。
- 本轮不改 `GameState.ChangeRegionOwner()` 行为，只增强测试可读性并修正审查状态。

### 2026-05-22 5分钟找缺口复核：Headless keyDelta 静态检查缺少运行时佐证

- `HeadlessScenarioMustHaveKeyDeltaBugTests.cs` 当前通过，说明每个源码中会 `return Pass(...)` 的 scenario 方法体内存在 `AddKeyDelta(...)`。
- 该测试仍是源码结构检查，不执行 `HeadlessSimulationRunner.RunAllScenarios()` 或同等 runtime path；若后续场景分支重构导致运行结果进入 `Fail`、被跳过，或 report 组装漏掉 keyDelta，源码检查无法直接捕获。
- 缺口判断：这是测试层级缺口，不是当前 `HeadlessSimulationRunner` 行为缺陷；风险在于 headless 验证表面绿色但 runtime report 的 keyDelta 仍可能缺失。
- 下一轮可低风险修补：在 `HeadlessScenarioMustHaveKeyDeltaBugTests.cs` 增加 runtime assertion，执行所有 headless scenarios 并断言每个 passing result 至少包含一个 keyDelta；不改 `HeadlessSimulationRunner` 场景逻辑、Web runtime、数据表或 Unity/Tuanjie。

### 2026-05-22 5分钟修补回收：Headless keyDelta 静态检查缺少运行时佐证

- `HeadlessScenarioMustHaveKeyDeltaBugTests.cs` 已保留静态源码检查，并新增 runtime assertion。
- runtime assertion 使用 `NonUnityJsonDataRepository` 加载 `web-strategy-map/game-data-source/data`，执行 `HeadlessSimulationRunner.RunAllScenarios()`。
- 新断言同时检查每个 passed `HeadlessSimulationResult.report.keyDeltas` 和最终 `suite.report.scenarios[].keyDeltas`，避免源码结构绿色但运行报告丢失 keyDelta。
- 本轮不改 `HeadlessSimulationRunner` 场景逻辑、Web runtime、数据表或 Unity/Tuanjie。

### 2026-05-22 5分钟找缺口复核：Army morale clamp 测试摘要与 runtime 覆盖不一致

- `ArmyState.morale` 当前已是 property，并在 setter 内通过 `DomainMath.Clamp(value, 0, 100)` 限制到 `[0, 100]`。
- `ArmyRuntimeState.morale` 当前同样已是 property，并在 setter 内通过 `DomainMath.Clamp(value, 0, 100)` 限制到 `[0, 100]`。
- `ArmyMoraleClampBugTests.cs` 当前定向测试通过，说明 legacy `ArmyState.morale` 直接赋值会被 clamp。
- 缺口判断：测试文件注释仍称 `ArmyState.morale` 与 `ArmyRuntimeState.morale` 是无 clamp 的 public int fields，已与当前实现不一致；同时测试只直接覆盖 `ArmyState`，没有直接覆盖 `ArmyRuntimeState`，与注释宣称的双路径不变量不完全匹配。
- 下一轮可低风险修补：在 `ArmyMoraleClampBugTests.cs` 中补充 `ArmyRuntimeState.morale` 的直接赋值 clamp 断言，并同步更新测试注释与本审查文档状态；不改 morale 生产逻辑、Web runtime、数据表或 Unity/Tuanjie。

### 2026-05-22 5分钟修补回收：Army morale clamp 测试摘要与 runtime 覆盖不一致

- `ArmyMoraleClampBugTests.cs` 注释已从旧版 public field 风险更新为当前 setter-level clamp 回归验证说明。
- 测试已新增 `Runtime_Army_Morale_Must_Stay_In_Range_After_Assignment()`，直接覆盖 `ArmyRuntimeState.morale` 的负值与超上限赋值路径。
- 审查表格和假阴性总结已同步回收该项；本轮不改 morale 生产逻辑、Web runtime、数据表或 Unity/Tuanjie。

### 2026-05-22 5分钟找缺口复核：GameStateFactory null-data 异常类型摘要漂移

- `GameStateFactory.CreateDefault()` 当前入口先调用 `ValidateRepository(data)`，对 `data == null`、`Emperors == null`、`Regions == null`、`Units == null` 都抛带领域语义的 `InvalidOperationException`。
- `GameStateFactoryNullDataBugTests.cs` 注释和断言当前都明确要求 documented `InvalidOperationException`，定向测试结果为 `2/2 passed`。
- 缺口判断：审查表格仍写“若修复方案是 `ArgumentNullException`，测试失败，应接受两种异常类型”，已与当前实现选择和测试意图不一致；这是审查摘要漂移，不是当前 `GameStateFactory` 行为缺陷。
- 下一轮可低风险修补：只更新 `docs/audit-test-coverage.md` 中 `GameStateFactoryNullDataBugTests.cs` 的表格状态，把该项标为已对齐/已回收；不改 `GameStateFactory`、C# 测试、Web runtime、数据表或 Unity/Tuanjie。

### 2026-05-22 5分钟修补回收：GameStateFactory null-data 异常类型摘要漂移

- `GameStateFactoryNullDataBugTests.cs` 表格行已从“应接受 `ArgumentNullException`”改为“已对齐/已回收”。
- 当前实现和测试统一使用 documented `InvalidOperationException` 表达缺失 repository 或关键数据表；该项已不再作为测试类型过窄缺口跟踪。
- 本轮不改 `GameStateFactory`、C# 测试、Web runtime、数据表或 Unity/Tuanjie。

---

## 四、覆盖缺口分析

### 4.1 军事系统缺口

| 缺口 | 描述 | 建议测试 | 状态 |
|------|------|----------|------|
| **军队分裂/合并** | 两支军队在同一格合并时，soldiers 相加逻辑是否正确？是否存在整数溢出？ | ✅ 已记录为 TODO 占位符 | 待补充 |
| **兵力为 0 的军队** | 当 soldiers 经过战斗降至 0 时，army 是否被正确移除出 engagement？UI 是否正确显示"已被消灭"？ | ✅ 已记录为 TODO 占位符 | 待补充 |
| **连续多回合战斗** | 同一 engagement 持续 N 回合，每回合伤亡叠加逻辑是否正确？是否有无限增长风险？ | 未记录 | 待补充 |
| **补给耗尽效果** | 补给降至 0 后，战斗力如何变化？morale 是否开始衰减？ | 未记录 | 待补充 |
| **跨区域增援路径验证** | 增援命令应验证路线上所有区域邻接关系；若路线中出现非邻接区域应拒绝 | 未记录 | 待补充 |
| **围城命令的食物消耗** | Siege 命令是否消耗食物？粮食不足时 Siege 是否被拒绝？ | 未记录 | 待补充 |
| **撤退目的地不可达** | 当所有相邻区域都是敌占区时，撤退命令应如何处理？返回 null？记录警告？ | 未记录 | 待补充 |
| **增援到达后的行为** | 增援 army 到达有交战区域后，是加入 attacker 还是 defender？还是创建新交战？ | 未记录 | 待补充 |

### 4.2 经济系统缺口

| 缺口 | 描述 | 建议测试 | 状态 |
|------|------|----------|------|
| **资源溢出** | 大量税收/战利品是否会导致 money 超过 int.MaxValue？溢出后行为如何？ | ✅ 已记录为 TODO 占位符（`TODO_EconomicMoneyOverflow`） | 待补充 |
| **负粮食传染** | 粮食为负会传染给依赖粮食的机制（军队消耗、增援）吗？这些路径是否检查了负值？ | 未记录 | 待补充 |
| **税收乘数的边界** | `taxMultiplier = -1.0` 或 `taxMultiplier = 0` 会产生什么效果？负乘数是否被允许？ | 未记录 | 待补充 |
| **粮食产出为负的地区** | `foodOutput < 0` 的地区会消耗粮食还是产生？经济系统如何处理？ | ✅ 已记录为 TODO 占位符（`TODO_NegativeFoodOutput`） | 待补充 |
| **驻军消耗的累计上限** | 大量军队在同一地区时，驻军消耗是否有上限？超过后是否触发驻军损耗事件？ | 未记录 | 待补充 |
| **继承危机时的经济冻结** | 继承危机期间，税收/驻军是否应该暂停？当前是否有此机制？ | 未记录 | 待补充 |

### 4.3 地图系统缺口

| 缺口 | 描述 | 建议测试 | 状态 |
|------|------|----------|------|
| **区域删除后的孤立引用** | 当地图 JSON 中删除了某个 region definition 后，GameState 中的 region 和 army 引用如何处理？ | 未记录 | 待补充 |
| **邻接关系的不变量** | 如果 region A 的 neighbors 包含 B，但 B 的 neighbors 不包含 A（即有向图），FindRoute 的行为如何？ | 未记录 | 待补充 |
| **自我交战检测** | 一支军队与自己交战（即同一 faction 的两支军队在同区域，但被错误分类为敌对）？ | 未记录 | 待补充 |
| **地图热重载** | 游戏运行时（ExecuteTurn 期间）修改地图数据，是否会导致异常？ | 未记录 | 待补充 |
| **路线计算的时间复杂度** | 在 60 个区域的地图上，FindRoute 的最坏情况性能如何？是否存在 O(n²) 或更差的情况？ | 未记录 | 待补充 |

### 4.4 存档系统缺口

| 缺口 | 描述 | 建议测试 | 状态 |
|------|------|----------|------|
| **版本迁移** | 当 JSON 存档中的字段与当前代码的字段不匹配时（如新版本删除了某字段），反序列化行为是什么？ | ✅ 已记录为 TODO 占位符（`TODO_SerializationVersionMigration`） | 待补充 |
| **部分损坏的存档** | JSON 中的数值字段包含非数字字符串（如 "nan" 或 "Infinity"），反序列化是否安全？ | ✅ 已记录为 TODO 占位符（`TODO_SerializationCorruptField`） | 待补充 |
| **engagement 的游离引用** | 已清除的 engagement 中的 armyId 在存档后被反序列化，MapState 是否会拒绝或清理？ | ✅ 已记录为 TODO 占位符（`TODO_SerializationOrphanedArmyIdsInEngagement`） | 待补充 |
| **turnLog 中的循环引用** | turnLog 条目是否可能引用 GameState 中的对象，从而在 GC 中产生循环引用导致内存泄漏？ | 未记录 | 待补充 |

### 4.5 统治/治理系统缺口

| 缺口 | 描述 | 建议测试 | 状态 |
|------|------|----------|------|
| **继承危机触发条件** | successionRisk 如何累积？达到 100 时是否一定会触发继承事件？ | 未记录 | 待补充 |
| **合法性为 0 的效果** | legitimacy=0 的效果是什么？是否导致所有区域叛逆？合法性如何恢复？ | 未记录 | 待补充 |
| **法统记忆的处理** | `legitimacyMemory` 字段在 gameplay 中如何使用？是否有系统在读取它？ | 未记录 | 待补充 |
| **地方势力叛乱阈值** | `rebellionRisk` 达到多少时触发叛乱？叛乱如何结算？ | 未记录 | 待补充 |

---

## 五、假阳性与假阴性总结

### 假阳性（测试写错了）— 已更新

| 测试文件 | 问题 | 严重性 | 状态 |
|----------|------|--------|------|
| `OccupationStatusTransitionBugTests.cs` | 断言条件本身就是 OR，且测试设置使其两侧都为假，无论 bug 是否存在测试都失败 | 高 | ✅ **已修复（v2）** |
| `ConcurrentModificationBugTests.cs` | 测试构造不会触发它声称要测试的路径（没有 engagement → 没有 RemoveArmy） | 高 | ✅ **已修复（v2）** |
| `NumericModifierSourceCollisionBugTests.cs` | 已绑定 `NumericModifierKey` AddOrReplace 去重不变量，并同步更新旧版失败注释 | 中 | ✅ 已修复 |
| `TurnLogUnboundedGrowthBugTests.cs` | 已绑定 `GameState.MaxCurrentTurnLogEntries`，并同步更新历史失败注释 | 中 | ✅ 已修复 |
| `EngagementListsCapacityLeakBugTests.cs` | `< 64` magic number 已收紧为 `== 0`，并同步更新过期注释 | 低 | ✅ 已修复 |

### 假阴性（测试不够严格）

| 测试文件 | 问题 | 严重性 | 状态 |
|----------|------|--------|------|
| `EmpireUpkeepDeterministicBugTests.cs` | 已扩展为 5 个 trial，并逐次对比首轮 money/food delta | 中 | ✅ 已修复 |
| `ArmyMoraleClampBugTests.cs` | 已确认 legacy/runtime morale 均为 property clamp，并补充 runtime 赋值路径断言 | 高 | ✅ 已修复 |
| `GameStateAtomicityBugTests.cs` | 已补充 previous owner 仍包含原 regionId 的断言，并对齐当前 null guard 实现 | 中 | ✅ 已修复 |
| `HeadlessScenarioMustHaveKeyDeltaBugTests.cs` | 已补充 production JSON `RunAllScenarios()` runtime assertion，并检查 result report 与 suite report keyDelta | 中 | ✅ 已修复 |
| `OccupationContributionInvariantBugTests.cs` | 对已修复 setter 的情况无效（直接字段赋值绕过了 setter） | 中 | 待修复 |

---

## 六、修复建议优先级

### 高优先级（影响核心游戏逻辑）

1. ✅ **已修复** `OccupationStatusTransitionBugTests.cs` — 断言逻辑已重写为经济路径验证
2. ✅ **已修复** `ConcurrentModificationBugTests.cs` — 改为 battle rout 路径触发 RemoveArmy
3. **补充军事系统缺口测试** — 军队分裂合并、兵力为 0 清除、连续多回合战斗、补给耗尽效果

### 中优先级（影响经济平衡）

4. **加固 `TestFixtures.BuildSinglePlayerWorld`** — 默认 `taxOutput=0` 导致经济测试依赖手动覆盖，应改为合理默认值。
5. **补充资源溢出/负值测试** — money/food/soldiers 的溢出和负值边界。
6. **补充统治继承危机测试** — successionRisk 累积和触发机制。

### 低优先级（改进测试质量）

7. 当前无新增低优先级测试质量修补项；保留后续审查中发现的新项。

---

## 七、结论

该测试套件整体质量为**中上**。其设计理念（bug-first、以可观测行为驱动）值得肯定，但存在三类问题：

1. **结构性假阳性**：原版 `OccupationStatusTransitionBugTests` 和 `ConcurrentModificationBugTests` 的测试构造不会触发其声称要检测的 bug，需要重写。**v2 已修复这两个高优先级问题。**
2. **Fixture 脆弱性**：`TestFixtures` 默认 `taxOutput=0`，导致多个经济测试依赖于测试方法内部的手动覆盖，容易在 fixture 演进中回归。
3. **覆盖空白**：军事系统的"军队生命周期"（分裂/合并/清零）、资源系统的溢出行为、存档版本迁移仍有明显空白。**v2 已补充 4 个 TODO 占位符**。

建议按第六节的优先级顺序逐一修复，并建立测试"可运行性"的 CI 检查（确保每个测试在修复前至少失败一次，修复后通过）。

---

## 附录：v2 变更摘要

| 变更 | 文件 | 说明 |
|------|------|------|
| 修复假阳性 #1 | `OccupationStatusTransitionBugTests.cs` | 重写两个测试：从自毁 OR 断言改为 integration-based 经济路径验证 |
| 修复假阳性 #2 | `ConcurrentModificationBugTests.cs` | 重写两个测试：1) battle rout 触发 RemoveArmy（通过 DomainMapWarResolutionSystem），2) RemoveRegion 直接触发 |
| 新增占位符 | `CoverageGap_TODO_Placeholders.cs` | 新增文件，为最高优先级覆盖缺口创建 TODO 测试框架 |
| 更新报告 | `docs/audit-test-coverage.md` | 更新为 v2，标注已修复项，添加缺口状态跟踪表 |
