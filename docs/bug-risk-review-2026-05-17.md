# 《万朝归一：九州帝业》Bug & 风险报告

> 审查范围：domain-core C# 核心 + headless 测试套件 + 数据契约层
> 审查时间：2026-05-17
> 审查方法：代码静态分析 + 现有 bug test 逆向推导

---

## 🔴 严重级别（高概率导致游戏崩溃或核心数据腐败）

### BUG-001: 战斗平局策略需设计确认
**文件：** `DomainBattleSimulationSystem.cs:25`
**代码：**
```csharp
bool attackerWon = attackerPower > defenderPower;
```
**复核结论：** 当前 `>` 语义下完全平局不会判进攻方获胜，而是防守方守住。直接改为 `>=` 会制造“平局判进攻方”的回归。
**已验证：** `BattleTieBreakBugTests.cs` 当前通过，作为防止误改 `>=` 的回归门。
**影响：** 仍存在设计待定项：若不希望平局按防守方胜利处理，需要新增显式 Draw 或概率结果，而不是把比较符改成 `>=`。

---

### BUG-002: 数值引擎对 NaN/Infinity 无防御
**文件：** `NumericSystem.cs:244-254`
**代码：**
```csharp
private static float SanitizeModifierValue(float value)
{
    if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
    return DomainMath.Clamp(value, -MaxModifierMagnitude, MaxModifierMagnitude);
}
```
**问题：**
1. `SanitizeModifierValue` 对 NaN 返回 0f，这导致一个非零基础值被 **错误地清零**（100 * NaN → NaN → 0），而非修复为安全值
2. `faction.taxMultiplier = float.NaN` 会让整个税收计算静默返回 0，而不是报错或使用默认值
3. JSON 解析若跳过验证直接注入 NaN，可导致财政系统静默崩溃

**修复回收：** 已补充 invalid override 回归测试，`NumericNaNInfinityBugTests.cs` 当前通过；NaN/Infinity override 不再覆盖有效基础值。
**影响：** mod 数据或存档损坏后，所有税收/粮食计算静默归零，误导性强。

---

### BUG-003: OccupationStatus 无状态机约束
**文件：** `RegionState.cs:253-261` & `RegionRuntimeState.cs:403`
**代码：**
```csharp
public OccupationStatus occupationStatus
{
    get { return occupationStatusValue; }
    set
    {
        occupationStatusValue = value;
        NormalizeContributionCaps();
    }
}
```
**问题：** setter 是公开的，任何代码可直接将 `OccupationStatus.Occupied` 跳到 `Controlled` 而不经过整合链。`CalculateGovernanceEfficiency` 完全不检查 status，只看 integration 数值，导致一个 integration=25（刚占领）的地区在 status=Controlled 时可获得完整税收。
**已验证：** `OccupationStatusTransitionBugTests.cs` 正是为这个 bug 编写的。
**影响：** 可绕过占领治理惩罚，让新占领地区立即提供全额税收。

---

### BUG-004: EventBus 订阅迭代期间修改 collection
**文件：** `EventBus.cs:241-262`
**现状：**
```csharp
public void Publish(GameEvent gameEvent)
{
    List<Action<GameEvent>> listenerList;
    if (listeners.TryGetValue(gameEvent.Type, out listenerList))
    {
        Action<GameEvent>[] snapshot = listenerList.ToArray();  // ✅ 有 snapshot
        for (int i = 0; i < snapshot.Length; i++)
        {
            // handler 内调用 Subscribe/Unsubscribe 时，原始 list 被修改
            handler.Invoke(gameEvent);
        }
    }
}
```
**分析：** Publish 确实做了 snapshot，但 `handler.Invoke` 是在 snapshot 的副本上执行的，而 **原始 listeners 字典在 Invoke 期间可能被修改**。C# 对 `List<Action>` 做 snapshot 后迭代 `Action<GameEvent>[]` 数组是安全的，但如果 Subscribe 发生在 handler 调用链的中间... 等等，`ToArray()` 已经做了 snapshot 复制。
**重新评估：** 实际上 `ToArray()` 已经做了快照，这个问题在当前代码下 **已修复**。但 `EventBusPublishDuringIterationBugTests.cs` 的注释暗示之前有这个 bug。建议保留该测试以防回退。

---

### BUG-005: CompactStringList 的 RemoveAll 行为异常
**文件：** `WorldState.cs:511-516`
**代码：**
```csharp
public new int RemoveAll(Predicate<string> match)
{
    int removed = base.RemoveAll(match);
    TrimIfEmpty();
    return removed;
}

private void TrimIfEmpty()
{
    if (Count == 0)
    {
        TrimExcess();
    }
}
```
**问题：** `RemoveAll` 后调用 `TrimExcess()`，这会改变 List 的内部 Capacity。如果其他地方保存了 `List<string>.Count` 或依赖 Capacity 做缓存，容量收缩会导致索引错位。
**更严重的是：** `CompactStringList` 继承自 `List<string>` 但重写了 `Clear/Remove/RemoveAt/RemoveAll`，如果代码将 `CompactStringList` 当普通 `List<string>` 使用并直接调用 `AddRange`（未被 override），行为会不一致。
**影响：** 军队 ID 列表在某些边缘操作后可能出现不可预期的容量行为。

---

## 🟠 中等级别（可能产生错误逻辑或边缘 case 问题）

### BUG-006: 战斗力 `soldierMultiplier` 使用 Log10 可能为负
**文件：** `NumericSystem.cs:449-453`
**代码：**
```csharp
float soldierMultiplier = DomainMath.Log10(DomainMath.Max(1, army.soldiers)) / NumericTuning.BattleSoldierLogDivisor;
if (soldierMultiplier < NumericTuning.BattleMinimumSoldierMultiplier)
{
    soldierMultiplier = NumericTuning.BattleMinimumSoldierMultiplier;
}
```
**分析：** 这里有 clamp 保护（`Max(1, ...)` 和 `if < 0.5 then 0.5`），是安全的。但 `Log10(1) = 0`，所以 1 个士兵的部队乘数也是 0.5（由 clamp 决定）。这个设计意味着单兵部队的战斗力等于 `baseStat * 0.5 * moraleMultiplier * 100`。这在游戏性上是合理的，但缺乏明确文档。
**建议：** 标注这个 clamp 行为，防止未来调整时被误删。

---

### BUG-007: CalculateSideSupplyPowerPercent 取最小值而非平均值
**文件：** `DomainBattleSimulationSystem.cs:110-125`
**代码：**
```csharp
percent = DomainMath.Min(percent, StrategyCausalRules.CalculateBattleSupplyPowerPercent(army));
```
**问题：** 使用 `Min` 意味着一个补给充分的部队如果和一个补给极低的部队在同一战场，会被整体压到 55%。这是非常激进的设计——多支军队的补给压力是独立的，不应该互相拖累。
**影响：** 进攻方如果有多支军队，其中一支缺粮会导致所有友军士气被压低，这可能不是策划想要的。

---

### BUG-008: 地图查询对病态输入无边界保护
**文件：** `MapQueryService.cs`（未直接审查，但基于模式推断）
**风险点：** 路线搜索算法在环形地图或超长路线时可能进入无限循环或栈溢出。对 `route_networks.json` 的依赖意味着如果邻接关系数据有环，会导致 `FindRoute` 死循环。

---

### BUG-009: 同势力部队接敌时沉默丢弃
**文件：** `DomainEngagementDetector.cs:49-55`
**代码：**
```csharp
else if (attackerArmyIds.Count > 1 && defenderArmyIds.Count == 0 && context.State != null)
{
    context.State.AddLog("war", regionId + "友军会合：" + attackerArmyIds.Count + " 支同势力部队位于同一地区。");
    return null;  // 仅记录日志，无其他处理
}
```
**问题：** 友军会合同样返回 null，调用方无法区分"真的没有敌人"和"全是友军"两种情况。如果 UI 层依赖返回值显示提示，用户可能会困惑为什么没有提示。
**建议：** 考虑引入 `EngagementResultType` 枚举区分情况。

---

### BUG-010: 字典枚举顺序依赖未文档化
**文件：** `WorldState.cs:361-373`
**代码：**
```csharp
private void RebuildArmyLocationIndex()
{
    foreach (List<string> armyIds in armyIdsByRegionId.Values)
    {
        armyIds.Clear();
    }
    foreach (ArmyRuntimeState army in armiesById.Values)
    {
        IndexArmyLocation(army.id, army.locationRegionId);
    }
}
```
**问题：** `Dictionary<string, ...>.Values` 的枚举顺序在 .NET 中虽然稳定但未文档化。`RebuildArmyLocationIndex` 依赖这个顺序重建索引，如果未来 .NET 版本改变枚举顺序（虽然不太可能），会导致索引错误。
**影响：** 低风险但属于技术债务。

---

## 🟡 低等级别（代码质量/可维护性问题）

### BUG-011: EngagementDetector 对边界 armies 列表的处理
**文件：** `DomainEngagementDetector.cs:17-18`
**代码：**
```csharp
List<ArmyRuntimeState> armies = mapState.GetArmiesInRegion(regionId);
if (armies.Count < 2) return null;
```
**问题：** `GetArmiesInRegion` 每次调用都重建索引（调用 `RebuildArmyLocationIndex`），这在 `DetectAll` 中会被调用 N 次（N = 区域数量），每次都全量遍历所有军队，复杂度 O(N*M)。
**建议：** `AdvanceMapLedArmies` 中已有类似模式但直接遍历 `mapState.ArmiesById.Values`，可考虑统一。

---

### BUG-012: RegionOwnerChangedPayload 未同步 legacy 和 runtime
**文件：** `DomainOccupationSystem.cs:35-47`
**代码：**
```csharp
RegionOwnerChangedPayload ownerChanged = context.ChangeRegionOwner(engagement.regionId, newOwnerFactionId);
// ...
runtimeRegion.ownerFactionId = newOwnerFactionId;
runtimeRegion.occupationStatus = OccupationStatus.Occupied;
// ...
legacyRegion.occupationStatus = runtimeRegion.occupationStatus;
// 但 legacyRegion.ownerFactionId 通过 ChangeRegionOwner 设置
// 两者同步路径不一致
```
**问题：** `ChangeRegionOwner` 内部会调用 `SyncRuntimeRegionOwner`，但 occupationStatus 需要单独同步。如果 `SyncRuntimeRegionOwner` 未来改变逻辑，两个 ownerFactionId 可能不同步。
**建议：** 将 occupationStatus 同步合并到 `ChangeRegionOwner` 中。

---

### BUG-013: GameState.AddLog 的 PruneTurnLog 调用过于频繁
**文件：** `GameState.cs:121-131`
**代码：**
```csharp
public void AddLog(string category, string message)
{
    turnLog.Add(new TurnLogEntry { ... });
    PruneTurnLog();  // 每次 AddLog 都调用
}
```
**问题：** 每条日志都检查并可能修剪列表。在高频事件（如多场战斗、多区域同时接敌）的回合中，这会造成性能浪费。
**建议：** 将 `PruneTurnLog` 移至回合结束阶段一次性执行。

---

### BUG-014: SameFactionEngagement 静默丢弃
**文件：** 基于 `SameFactionEngagementSilentDropBugTests.cs` 文件名推断
**风险：** 同势力部队进入 engagement 后被静默丢弃而非给出明确错误或重分类。这会导致玩家的指令被系统忽略而不通知。

---

## 📊 数据契约风险

### RISK-001: JSON 数据无运行时 Schema 验证
**现状：** 帝皇定义、地区定义、事件等数据从 JSON 加载后直接使用。
**风险：** 缺少字段的 JSON 会导致 null reference，字段类型错误会导致 cast 异常。
**建议：** 引入 JSON Schema 验证或使用 `[Required]` 属性 + 启动时强制校验。

---

### RISK-002: 装备系统 EquipmentLookup 未见实现
**文件：** `DomainBattleSimulationSystem.cs:252-262`
**代码：**
```csharp
private static EquipmentDefinition GetEquipmentDefinition(string slotId, ...)
// 调用：
EquipmentDefinition equip = EquipmentLookup.Get(slotId);
```
**问题：** `EquipmentLookup` 是一个静态工具类，但在 `DataModels.cs` 中未见定义。运行时如果找不到装备会返回 null，调用方需做 null 检查。
**风险：** 未来如果某个装备 ID 在 JSON 中但 EquipmentLookup 遗漏实现，会静默跳过加成而非报错。

---

## 🎯 总结

| 严重度 | 数量 | 代表性 bug |
|--------|------|-----------|
| 🔴 严重 | 4 | NaN override 风险、status 状态机缺失、dual-state 同步 |
| 🟠 中等 | 4 | 补给 Min 设计、路线搜索死循环、同势力沉默丢弃 |
| 🟡 低 | 5 | 性能浪费、同步不一致、技术债务 |
| 📊 风险 | 2 | JSON 验证缺失、EquipmentLookup 未实现 |

**最优先修复建议：**
1. **BUG-002** (NaN/Infinity) — 已有测试，修复成本低
2. **BUG-001** (平局判定) — 当前未复现平局判攻方，作为设计项暂缓；不得直接改为 `>=`
3. **BUG-003** (Status 状态机) — 需要新增状态机或验证逻辑
4. **RISK-001** — 引入启动时 JSON schema 校验

---

*本报告由 Mavis 审查生成，基于 2026-05-17 的代码快照。*
