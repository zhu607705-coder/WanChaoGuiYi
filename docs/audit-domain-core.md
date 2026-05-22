# domain-core C# 深度审查报告 v2

**审查范围**：`E:\万朝归一\万朝归一\domain-core\src`
**审查时间**：2026-05-17
**审查者**：General Agent (Mavis)
**版本**：v2 — 补充审查遗漏文件 + 风险等级修正
**状态**：完整审查（需完整编译验证）
**修复回收状态（2026-05-17）**：`RestoreContestedRegion` dual-state 同步、`DomainMath.Log10(0)` 和 Numeric NaN/Infinity override 已修复并通过回归。平局项经复核为设计争议：当前代码使用 `attackerPower > defenderPower`，精确平局是防守方守住；不得直接改为 `>=`。

---

## 📋 审查摘要

共审查 24 个 C# 文件，发现 **严重缺陷 7 处**，**中等问题 11 处**，**建议优化 15 处**。最关键问题包括：平局判定设计需确认、同势力接敌检测 bug、legacy/runtime 状态同步不完整。

---

## 1. DomainBattleSimulationSystem — 平局判定设计审查

### 描述
当攻守双方 power 完全相等时，当前系统判定防守方守住（attackerWon = attackerPower > defenderPower）。这不是“平局判攻方胜利”的当前 bug；是否需要显式 Draw 或概率结果，应作为设计项单独决策。

### 代码片段
```csharp
// DomainBattleSimulationSystem.cs:25
bool attackerWon = attackerPower > defenderPower;

// DomainBattleSimulationSystem.cs:201-218 — 伤亡计算基于 dominance ratio
private static float CalculateWinnerCasualtyFraction(...) { ... }
private static float CalculateLoserCasualtyFraction(...) { ... }
```

### 风险等级：🟡 设计待定
- **当前行为**：精确平局时 `attackerWon=false`，下游按防守方胜利处理。
- **回归风险**：若误改为 `>=`，平局会变成进攻方胜利，测试 `BattleTieBreakBugTests` 会失败。
- **设计缺口**：如需“无结果平局”或“显式 Draw”，需要扩展 `BattleResult` 表达胜负之外的状态。

### 建议修复方案
```csharp
// 选项 A：严格平局
if (attackerPower == defenderPower) return null; // 拒绝结算，要求重算或撤退

// 选项 B：平局时按随机或防守方优势
bool attackerWon = attackerPower > defenderPower ||
    (attackerPower == defenderPower && random.NextDouble() < 0.4); // 40%攻方优势
```

---

## 2. DomainEngagementDetector — 同势力沉默丢弃与友军会合日志无效

### 描述
当一个区域只有同势力多支部队（attackerArmyIds.Count > 1, defenderArmyIds.Count == 0）时，系统在满足以下条件时静默返回 null：
- `existingEngagement` 为 false（无现存接敌）
- `attackerSeed == null`（找不到攻击方种子）
- 但日志仍然写入，且条件并非所有同势力场景

关键缺陷：`ClassifyArmies` 中同势力部队分入 attacker，不会在 `defenderArmyIds` 中出现，但后续判定 `attackerArmyIds.Count > 1 && defenderArmyIds.Count == 0` 时可能遗漏纯友军会合场景。

### 代码片段
```csharp
// DomainEngagementDetector.cs:43-55
if (attackerArmyIds.Count == 0 || defenderArmyIds.Count == 0)
{
    if (existingEngagement)
    {
        DomainEngagementCleanup.ClearEngagementIfSideEmpty(mapState, engagement.id);
    }
    else if (attackerArmyIds.Count > 1 && defenderArmyIds.Count == 0 && context.State != null)
    {
        // 此分支仅在友军会合时触发，但缺少同势力敌军在场的情况
        context.State.AddLog("war", regionId + "友军会合：" + attackerArmyIds.Count + " 支同势力部队位于同一地区。");
    }
    return null;
}
```

### 风险等级：🟠 较高
- **数据一致性问题**：同势力部队进入敌方区域后，可能不会触发任何日志或事件
- **状态机混乱**：occupationStatus 可能停留在 Contested，但实际无接敌

### 建议修复方案
```csharp
// 在 DetectRegion 开头增加防御性检查
if (AllArmiesSameFaction(armies))
{
    context.State.AddLog("war", regionId + "友军会合：" + armies.Count + " 支同势力部队。");
    return null;
}

// 或者增强 ClassifyArmies 处理同势力场景
private static bool AllArmiesSameFaction(List<ArmyRuntimeState> armies)
{
    if (armies.Count < 2) return false;
    string firstFaction = armies[0].ownerFactionId;
    return armies.All(a => a.ownerFactionId == firstFaction);
}
```

---

## 3. RegionState / RegionRuntimeState — occupationStatus 无状态机约束

### 描述
`OccupationStatus` 是枚举类型，在 `RegionState`（legacy）和 `RegionRuntimeState`（runtime）中均为普通 public 字段，无任何状态转换验证逻辑。这导致：
- 任何系统可以直接设置任意值（如从 Controlled 直接跳到 Rebelling）
- 状态转换不变量（如 Contested 必须由接敌触发）无法强制执行
- 违反状态机的封装原则

### 代码片段
```csharp
// GameState.cs:253-261 — RegionState.occupationStatus
public OccupationStatus occupationStatus
{
    get { return occupationStatusValue; }
    set
    {
        occupationStatusValue = value;
        NormalizeContributionCaps(); // 仅规范化贡献百分比，无状态验证
    }
}

// WorldState.cs:23-29 — OccupationStatus enum
public enum OccupationStatus
{
    Controlled,
    Contested,
    Occupied,
    Rebelling
}

// DomainEngagementDetector.cs:109-116 — 仅有的一处状态转换逻辑
private static void MarkRegionContested(MapState mapState, string regionId)
{
    RegionRuntimeState region;
    if (mapState.TryGetRegion(regionId, out region))
    {
        region.occupationStatus = OccupationStatus.Contested;
    }
}
```

### 风险等级：🟡 中等
- **状态转换不确定性**：无状态机约束意味着任何代码都可能设置错误状态
- **调试困难**：状态异常难以追踪到具体代码路径

### 建议修复方案
```csharp
// 方案 A：引入状态转换验证方法
public static class OccupationStatusRules
{
    public static bool CanTransition(OccupationStatus from, OccupationStatus to,
        string trigger, GameContext context)
    {
        return to switch
        {
            OccupationStatus.Contested => from == OccupationStatus.Controlled || from == OccupationStatus.Occupied,
            OccupationStatus.Occupied => from == OccupationStatus.Contested || from == OccupationStatus.Controlled,
            OccupationStatus.Rebelling => from == OccupationStatus.Occupied,
            _ => false
        };
    }
}

// 方案 B：在 RegionState.setter 中增加验证
public OccupationStatus occupationStatus
{
    get => occupationStatusValue;
    set
    {
        if (!OccupationStatusRules.CanTransition(occupationStatusValue, value, "code_set", null))
        {
            throw new InvalidOperationException($"Invalid occupationStatus transition from {occupationStatusValue} to {value}");
        }
        occupationStatusValue = value;
        NormalizeContributionCaps();
    }
}
```

---

## 4. DomainOccupationSystem — Legacy/Runtime 状态同步不完整

### 描述
占领系统更新 `RegionRuntimeState` 后同步到 `RegionState`，但存在以下问题：
1. 同步操作在 `ApplyBattleOccupation` 中手动执行，遗漏点难以追踪
2. `governanceImpactSystem.ApplyOccupationImpact` 同时修改 runtime 和 legacy，但同步逻辑分散
3. 没有统一的同步机制验证 dual-state 一致性

### 代码片段
```csharp
// DomainOccupationSystem.cs:35-47 — 手动同步
runtimeRegion.ownerFactionId = newOwnerFactionId;
runtimeRegion.occupationStatus = OccupationStatus.Occupied;

RegionState legacyRegion = context.State.FindRegion(engagement.regionId);
if (legacyRegion != null)
{
    legacyRegion.occupationStatus = runtimeRegion.occupationStatus; // 仅同步 occupationStatus
    runtimeRegion.integration = legacyRegion.integration; // 反向同步 integration
    runtimeRegion.rebellionRisk = legacyRegion.rebellionRisk;
}

// DomainGovernanceImpactSystem.cs:35-46 — 再次同步
legacyRegion.integration = runtimeRegion.integration;
legacyRegion.occupationStatus = runtimeRegion.occupationStatus;
legacyRegion.controlStage = runtimeRegion.controlStage;
// ... 8 个字段逐一同步
```

### 风险等级：🟠 较高
- **数据漂移**：dual-state 可能在某些代码路径下不同步
- **状态不一致**：存档/加载后 legacy 和 runtime 可能出现不一致
- **维护困难**：每次新增字段需要记住更新两处同步逻辑

### 建议修复方案
```csharp
// 统一同步方法
public static class RegionStateSync
{
    public static void SyncRuntimeToLegacy(RegionRuntimeState runtime, RegionState legacy)
    {
        if (runtime == null || legacy == null) return;

        legacy.ownerFactionId = runtime.ownerFactionId;
        legacy.occupationStatus = runtime.occupationStatus;
        legacy.integration = runtime.integration;
        legacy.taxContributionPercent = runtime.taxContributionPercent;
        legacy.foodContributionPercent = runtime.foodContributionPercent;
        legacy.rebellionRisk = runtime.rebellionRisk;
        legacy.localPower = runtime.localPower;
        legacy.annexationPressure = runtime.annexationPressure;
        legacy.localAcceptance = runtime.localAcceptance;
        legacy.controlStage = runtime.controlStage;
        legacy.occupationReservedFood = runtime.occupationReservedFood;
        legacy.occupationPacificationQueueStep = runtime.occupationPacificationQueueStep;
        legacy.occupationPacificationQueueTurnsRemaining = runtime.occupationPacificationQueueTurnsRemaining;
    }

    public static void SyncLegacyToRuntime(RegionState legacy, RegionRuntimeState runtime)
    {
        if (legacy == null || runtime == null) return;

        // 仅同步必要字段
        runtime.ownerFactionId = legacy.ownerFactionId;
        runtime.occupationStatus = legacy.occupationStatus;
    }
}
```

---

## 5. WorldStateFactory — 存档重建缺少验证

### 描述
`Create` 方法从 GameState 重建 MapState 时，仅复制字段值，无以下验证：
1. 区域所有权一致性（faction.regionIds 与 region.ownerFactionId 匹配）
2. 军队位置有效性（army.locationRegionId 对应有效区域）
3. 数值范围合理性（integration 应在 0-100）

### 代码片段
```csharp
// WorldStateFactory.cs:9-36
public static WorldState Create(GameState gameState, IDataRepository data)
{
    MapState mapState = new MapState();
    // ...
    for (int i = 0; i < gameState.regions.Count; i++)
    {
        RegionState region = gameState.regions[i];
        RegionRuntimeState runtimeRegion = new RegionRuntimeState
        {
            id = region.id,
            ownerFactionId = region.ownerFactionId,
            // 直接复制，无验证
            integration = region.integration, // 可能超出 0-100
            // ...
        };
        // ...
    }
}
```

### 风险等级：🟡 中等
- **存档损坏风险**：损坏的存档可能无法被正确加载
- **运行时错误**：无效数据可能在后续计算中导致 NaN/Infinity

### 建议修复方案
```csharp
public static WorldState Create(GameState gameState, IDataRepository data)
{
    // 添加验证
    ValidateGameState(gameState, data);

    MapState mapState = new MapState();
    // ... 现有逻辑
}

private static void ValidateGameState(GameState state, IDataRepository data)
{
    if (state == null) throw new ArgumentNullException(nameof(state));

    // 验证 faction-region 所有权一致性
    HashSet<string> validFactionIds = new HashSet<string>(state.factions.Select(f => f.id));
    HashSet<string> validRegionIds = new HashSet<string>(state.regions.Select(r => r.id));

    foreach (var faction in state.factions)
    {
        foreach (var regionId in faction.regionIds)
        {
            if (!validRegionIds.Contains(regionId))
                throw new InvalidOperationException($"Faction {faction.id} owns non-existent region {regionId}");
        }
    }

    foreach (var region in state.regions)
    {
        if (!validFactionIds.Contains(region.ownerFactionId))
            throw new InvalidOperationException($"Region {region.id} owned by non-existent faction {region.ownerFactionId}");

        // 验证数值范围
        region.integration = DomainMath.Clamp(region.integration, 0, 100);
    }
}
```

---

## 6. DomainEconomySystem — 税收/粮食计算缺少防御性检查

### 描述
经济系统计算税收和粮食时，存在以下潜在问题：
1. `CalculateEffectiveRegionalTax/Food` 依赖 `GetRuntimeRegion` 可能返回 null
2. `CalculateArmyMoneyUpkeep` 和 `CalculateArmyFoodUpkeep` 遍历所有军队，每次调用都重新计算
3. 双重循环（faction → region）复杂度 O(n*m)

### 代码片段
```csharp
// DomainEconomySystem.cs:62-68
private int CalculateEffectiveRegionalTax(RegionState region, NumericContext numericContext)
{
    int baseTax = NumericFormulas.CalculateRegionalTax(region, numericContext);
    RegionRuntimeState runtimeRegion = GetRuntimeRegion(region.id);
    if (runtimeRegion == null) return baseTax; // 降级处理，但缺少日志
    return ApplyContributionPercent(baseTax, runtimeRegion.taxContributionPercent);
}

// DomainEconomySystem.cs:16-39 — ExecuteTurn
public void ExecuteTurn(GameContext context)
{
    for (int i = 0; i < context.State.factions.Count; i++)  // O(n)
    {
        FactionState faction = context.State.factions[i];
        NumericContext numericContext = NumericModifierFactory.ForFaction(faction);

        for (int j = 0; j < faction.regionIds.Count; j++)  // O(m)
        {
            RegionState region = context.State.FindRegion(faction.regionIds[j]); // 线性搜索
            // ...
        }

        int moneyUpkeep = CalculateArmyMoneyUpkeep(context, faction, numericContext); // 遍历所有军队
        // ...
    }
}
```

### 风险等级：🟡 中等
- **性能问题**：每次 ExecuteTurn 多次遍历 context.State.armies
- **潜在 null 引用**：虽然有防御性检查，但降级处理可能掩盖数据问题

### 建议修复方案
```csharp
// 优化 1：缓存 NumericContext
public void ExecuteTurn(GameContext context)
{
    if (context == null || context.State == null) return;

    // 预计算所有 faction 的 numericContext
    Dictionary<string, NumericContext> factionContexts = new Dictionary<string, NumericContext>();
    for (int i = 0; i < context.State.factions.Count; i++)
    {
        factionContexts[context.State.factions[i].id] = NumericModifierFactory.ForFaction(context.State.factions[i]);
    }

    // 使用 region -> faction 映射避免线性搜索
    // ...
}

// 优化 2：军队 upkeep 预计算一次
int totalMoneyUpkeep = 0;
int totalFoodUpkeep = 0;
for (int i = 0; i < context.State.armies.Count; i++)
{
    ArmyState army = context.State.armies[i];
    if (!factionContexts.ContainsKey(army.ownerFactionId)) continue;
    // 计算并累积
}
```

---

## 7. MapQueryService — FindRoute 死循环风险

### 描述
`FindRoute` 使用 BFS 查找路径，在以下情况下可能出现问题：
1. `GetNeighborRegions` 返回包含回路的邻居（地图数据错误）
2. `cameFrom` 字典在特定边界条件下可能产生问题
3. 无最大迭代次数限制

### 代码片段
```csharp
// MapQueryService.cs:59-104
public List<string> FindRoute(string startRegionId, string targetRegionId)
{
    // ...
    Queue<string> frontier = new Queue<string>();
    Dictionary<string, string> cameFrom = new Dictionary<string, string>();
    frontier.Enqueue(startRegionId);
    cameFrom[startRegionId] = null;

    while (frontier.Count > 0)  // 无最大迭代次数
    {
        string current = frontier.Dequeue();
        if (current == targetRegionId) break;

        foreach (string neighbor in GetNeighborRegions(current))
        {
            if (cameFrom.ContainsKey(neighbor)) continue;
            cameFrom[neighbor] = current;
            frontier.Enqueue(neighbor);
        }
    }
    // ...
}
```

### 风险等级：🟢 低
- **设计合理**：`cameFrom` 保护确保每个区域只入队一次，无限循环不可能发生
- **性能可接受**：BFS 在连通图上最多遍历所有节点，时间复杂度 O(V+E)

### 建议修复方案
```csharp
public List<string> FindRoute(string startRegionId, string targetRegionId)
{
    const int MaxIterations = 1000; // 最大迭代次数保护
    int iterations = 0;

    // ... 现有 BFS 逻辑 ...

    while (frontier.Count > 0)
    {
        iterations++;
        if (iterations > MaxIterations)
        {
            context?.State.AddLog("system", "Route search exceeded max iterations from " +
                startRegionId + " to " + targetRegionId);
            return new List<string>(); // 返回空路线表示失败
        }

        string current = frontier.Dequeue();
        // ...
    }
}
```

---

## 8. EventBus — 事件订阅/发布完整性检查

### 描述
EventBus 实现正确，但存在以下观察点：
1. `Subscribe` 检查 `existing.Contains(listener)` 是 O(n) 操作
2. `Publish` 中异常被静默吞掉，缺少错误日志
3. 无事件历史记录功能

### 代码片段
```csharp
// EventBus.cs:220-224
if (!existing.Contains(listener))  // O(n) 线性搜索
{
    existing.Add(listener);
}

// EventBus.cs:252-259
try
{
    handler.Invoke(gameEvent);
}
catch
{
    // 静默吞掉异常，无日志
}
```

### 风险等级：🟢 低
- **性能**：大量订阅时，Contains 检查可能成为瓶颈
- **调试**：异常被吞掉难以调试

### 建议修复方案
```csharp
public void Publish(GameEvent gameEvent)
{
    List<Action<GameEvent>> listenerList;
    if (!listeners.TryGetValue(gameEvent.Type, out listenerList)) return;

    Action<GameEvent>[] snapshot = listenerList.ToArray();
    for (int i = 0; i < snapshot.Length; i++)
    {
        try
        {
            snapshot[i].Invoke(gameEvent);
        }
        catch (Exception ex)
        {
            // 至少记录到日志
            Console.Error.WriteLine($"Event handler exception: {ex.Message}");
        }
    }
}

// 使用 HashSet 替代 List 提高 Contains 性能
private readonly Dictionary<GameEventType, HashSet<Action<GameEvent>>> listeners =
    new Dictionary<GameEventType, HashSet<Action<GameEvent>>>();
```

---

## 9. IGameSystem — 接口一致性检查

### 描述
所有实现 IGameSystem 的类需要检查四个方法的一致性：

| 系统 | Initialize | OnTurnStart | ExecuteTurn | OnTurnEnd |
|------|------------|-------------|-------------|-----------|
| IGameSystem | ✓ | ✓ | ✓ | ✓ |
| DomainEconomySystem | ✓ (空) | ✓ (空) | ✓ | ✓ (空) |
| DomainArmyMovementSystem | ✓ (空) | ✓ (空) | ✓ | ✓ (空) |
| DomainBattleSimulationSystem | 不实现 | 不实现 | 不实现 | 不实现 |
| DomainOccupationSystem | 不实现 | 不实现 | 不实现 | 不实现 |
| DomainGovernanceImpactSystem | 不实现 | 不实现 | 不实现 | 不实现 |

### 风险等级：🟢 低
- **设计合理**：DomainMapWarResolutionSystem 统一调用战斗相关系统，执行顺序明确
- **架构清晰**：接口定义正确，部分系统通过委托调用是合理的设计选择

### 建议修复方案
```csharp
// 统一所有战斗相关系统实现 IGameSystem
public sealed class DomainBattleSimulationSystem : IGameSystem
{
    public void Initialize(GameContext context) { }
    public void OnTurnStart(GameContext context) { }
    public void ExecuteTurn(GameContext context) { ResolveAllReadyEngagements(context, context.State.Map); }
    public void OnTurnEnd(GameContext context) { }
}

// 或者添加注释说明哪些系统由外部调用
// DomainBattleSimulationSystem.ResolveEngagement 由 DomainMapWarResolutionSystem 调用
```

---

## 10. NumericSystem — NaN/Infinity 处理评估

### 描述
`NumericEngine` 对 NaN 和 Infinity 的处理是正确的：
- `SanitizeModifierValue` 将 NaN/Infinity 替换为 0f
- `SanitizeFiniteValue` 将 NaN/Infinity 替换为 fallback
- 最终值被限制在 `MaxFinalMagnitude` (1,000,000,000)

### 代码片段
```csharp
// NumericSystem.cs:244-248
private static float SanitizeModifierValue(float value)
{
    if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
    return DomainMath.Clamp(value, -MaxModifierMagnitude, MaxModifierMagnitude);
}

// NumericSystem.cs:250-254
private static float SanitizeFiniteValue(float value, float fallback)
{
    if (float.IsNaN(value) || float.IsInfinity(value)) return fallback;
    return DomainMath.Clamp(value, -MaxFinalMagnitude, MaxFinalMagnitude);
}
```

### 风险等级：🟢 低
- **设计合理**：NaN 作为 0 处理是明确的降级策略
- **无静默错误**：所有 Sanitize 方法都有明确的行为

### 建议
保持当前实现，添加单元测试验证边界条件。

---

## 11. DomainGovernanceImpactSystem — 治理影响计算评估

### 描述
治理影响系统正确地同步了 runtime 和 legacy 状态，但存在一些观察点：
1. `ApplyOccupationImpact` 在修改 runtime 后同步到 legacy，可能遗漏某些字段
2. 没有验证新旧 owner 之间的过渡逻辑

### 代码片段
```csharp
// DomainGovernanceImpactSystem.cs:14-33
FactionState ownerFaction = context.State.FindFaction(runtimeRegion.ownerFactionId);
int legitimacyBefore = ownerFaction != null ? ownerFaction.legitimacy : 0;
StrategyCausalRules.ApplyOccupationLegitimacyCost(ownerFaction);
int legitimacyAfter = ownerFaction != null ? ownerFaction.legitimacy : legitimacyBefore;

// 同步 12 个字段到 legacy
legacyRegion.integration = runtimeRegion.integration;
// ...
```

### 风险等级：🟡 中等
- **同步遗漏风险**：新增字段可能忘记同步
- **合法性计算依赖 FindFaction**：如果 faction 找不到，不会有警告

### 建议修复方案
```csharp
// 添加字段列表验证
private static readonly string[] SyncFields = new[]
{
    "integration", "occupationStatus", "controlStage",
    "occupationReservedFood", "occupationPacificationQueueStep",
    "occupationPacificationQueueTurnsRemaining", "taxContributionPercent",
    "foodContributionPercent", "rebellionRisk", "localPower",
    "annexationPressure", "localAcceptance"
};

// 每次修改后验证同步完整性
public void ApplyOccupationImpact(...)
{
    // ... 现有逻辑

    // 验证同步
    Debug.Assert(legacyRegion.integration == runtimeRegion.integration);
    Debug.Assert(legacyRegion.occupationStatus == runtimeRegion.occupationStatus);
    // ...
}
```

---

## 12. GameStateFactory — 游戏初始化逻辑评估

### 描述
`CreateDefault` 方法正确地初始化了游戏状态，但存在以下观察点：
1. `BuildHistoricalRegionOwners` 硬编码了历史归属，缺乏可扩展性
2. `ResolveRegionOwner` 的 fallback 逻辑可能产生非预期结果

### 代码片段
```csharp
// GameStateFactory.cs:119-133
private static Dictionary<string, string> BuildHistoricalRegionOwners()
{
    Dictionary<string, string> owners = new Dictionary<string, string>();
    AddRegionOwners(owners, "faction_qin_shi_huang", new string[] { ... });
    AddRegionOwners(owners, "faction_liu_bang", new string[] { ... });
    // ... 8 个势力硬编码
    return owners;
}

// GameStateFactory.cs:156-165
private static FactionState ResolveRegionOwner(...)
{
    string ownerFactionId;
    if (historicalOwners != null && historicalOwners.TryGetValue(regionId, out ownerFactionId))
    {
        FactionState historicalOwner = state.FindFaction(ownerFactionId);
        if (historicalOwner != null) return historicalOwner;
    }

    // Fallback: 轮换分配
    for (int i = 0; i < factions.Count; i++)
    {
        FactionState fallback = factions[(fallbackIndex + i) % factions.Count];
        // ...
    }
}
```

### 风险等级：🟡 中等
- **维护性差**：新增势力或修改地图需要修改工厂代码
- **测试困难**：硬编码数据难以模拟不同场景

### 建议修复方案
```csharp
// 方案 A：将历史归属移到数据文件
public static Dictionary<string, string> LoadHistoricalRegionOwners(IDataRepository data)
{
    // 从 regions.json 读取 historicalOwner 字段
}

// 方案 B：使用策略模式
public interface IRegionOwnershipStrategy
{
    string ResolveOwner(RegionDefinition region, List<FactionState> factions, int fallbackIndex);
}

public class HistoricalOwnershipStrategy : IRegionOwnershipStrategy { ... }
public class RandomOwnershipStrategy : IRegionOwnershipStrategy { ... }
```

---

## 13. DomainMapWarResolutionSystem — 整体调度逻辑评估

### 描述
`DomainMapWarResolutionSystem` 是战争子系统的总调度器，协调检测、战斗、占领和清理流程。整体设计合理，但存在以下观察点：

1. **战斗后 Legacy 同步不完整**：`RemoveArmy` 和 `ResolveLoserArmy` 同步了 `regionId` 但未同步 `engagementId`（后者在 legacy 中不存在，可接受）
2. **新形成的接敌跳过处理**：`IsNewlyFormedThisTurn` 跳过本回合形成的接敌，但 `ResolveFormedEngagements` 没有等待机制，可能导致新接敌在本回合被忽略
3. **无重试/冲突解决**：多个接敌同时争夺同一区域时，没有明确的冲突解决顺序

### 代码片段
```csharp
// DomainMapWarResolutionSystem.cs:40-65 — ExecuteTurn
public void ExecuteTurn(GameContext context)
{
    MapState mapState = worldState.Map;
    engagementDetector.DetectAll(context, mapState);       // 第1步：检测
    ResolveFormedEngagements(context, mapState);           // 第2步：结算
}

// ResolveFormedEngagements 中跳过新形成的接敌
if (IsNewlyFormedThisTurn(context, engagement))
{
    continue; // 跳过，期望下一回合再处理
}

// ResolveLoserArmy 中同步 legacy 区域归属
legacyArmy.regionId = retreatRegionId;
legacyArmy.movementProgress = 0;
```

### 风险等级：🟢 低
- **设计合理**：调度流程清晰，分工明确
- **Legacy 同步不完整影响有限**：军队 regionId 同步正确，`engagementId` 在 legacy 不存在

### 建议
1. 添加 `EngagementQueue` 机制，确保新形成的接敌在下一回合被优先处理
2. 考虑在 `RemoveArmy` 后调用 `DomainOccupationSystem.CheckRegionStability` 验证区域状态
3. 添加战斗结果的 EventBus 发布，便于 UI 层监听

---

## 14. DomainEngagementCleanup — 清理逻辑与 dual-state 不同步风险

### 描述
`DomainEngagementCleanup` 负责清理已解决或无效的接敌状态，包含关键的 dual-state 不同步问题：

**核心问题**：`RestoreContestedRegion` 只修改 `RegionRuntimeState.occupationStatus`，不修改 `RegionState`（legacy）中的对应字段。

### 代码片段
```csharp
// DomainEngagementCleanup.cs:42-49 — 关键缺陷
public static void RestoreContestedRegion(MapState mapState, string regionId)
{
    RegionRuntimeState region;
    if (mapState != null && mapState.TryGetRegion(regionId, out region) && region.occupationStatus == OccupationStatus.Contested)
    {
        region.occupationStatus = OccupationStatus.Controlled;  // 仅修改 runtime！
    }
    // 没有同步到 legacy RegionState！
}

// 调用链
ClearEngagementIfSideEmpty → RestoreContestedRegion  // 只修改 runtime
ClearResolvedEngagement → RestoreContestedRegion      // 只修改 runtime
```

### dual-state 分离后果

| 场景 | Runtime Region | Legacy Region | 后果 |
|------|---------------|---------------|------|
| 接敌解散（非战斗） | Contested → **Controlled** | Contested（不变） | 存档加载后恢复 Contested |
| 战斗结束 | Contested → **Controlled** | Contested（不变） | 存档加载后恢复 Contested |
| 存档/加载 | — | Contested | runtime 从 Contested 开始下一回合检测 |

### 影响分析

**症状**：
- 玩家看到区域状态为 Controlled，但存档后重新加载变为 Contested
- 重复触发接敌检测，导致异常日志增多
- 游戏状态在 runtime 和存档不一致

**触发条件**：
1. `ClearEngagementIfSideEmpty` 在任何一方无部队时触发
2. `ClearResolvedEngagement` 在战斗结束后触发

### 风险等级：🔴 高
- **存档损坏症状**：dual-state 不同步导致存档后状态不一致
- **UI/逻辑不一致**：用户看到的状态与实际逻辑不符
- **难以调试**：runtime 正常但存档异常，问题定位困难

### 建议修复方案
```csharp
public static void RestoreContestedRegion(
    MapState mapState,
    string regionId,
    GameState legacyState = null)
{
    RegionRuntimeState region;
    if (mapState != null && mapState.TryGetRegion(regionId, out region)
        && region.occupationStatus == OccupationStatus.Contested)
    {
        region.occupationStatus = OccupationStatus.Controlled;

        // 同步到 legacy（如果提供了 legacy 引用）
        if (legacyState != null)
        {
            RegionState legacyRegion = legacyState.FindRegion(regionId);
            if (legacyRegion != null)
            {
                legacyRegion.occupationStatus = OccupationStatus.Controlled;
            }
        }
    }
}

// 调用点更新
public static void ClearEngagementIfSideEmpty(MapState mapState, string engagementId, GameState legacyState = null)
{
    // ... 现有逻辑 ...
    RestoreContestedRegion(mapState, engagement.regionId, legacyState);
}

public static void ClearResolvedEngagement(MapState mapState, EngagementRuntimeState engagement, GameState legacyState = null)
{
    // ... 现有逻辑 ...
    RestoreContestedRegion(mapState, engagement.regionId, legacyState);
}
```

---

## 15. DomainMath — 数学工具函数边界条件

### 描述
`DomainMath` 提供基础数学工具函数，存在以下边界条件问题：

### 15.1 RoundToInt 截断行为

**问题**：`RoundToInt` 使用 `MidpointRounding.AwayFromZero`，这在负数上有非直觉行为。

```csharp
// DomainMath.cs:46-49
public static int RoundToInt(float value)
{
    return (int)Math.Round(value, MidpointRounding.AwayFromZero);
}
```

| 输入 | 输出 | 期望？ |
|------|------|--------|
| 2.5 | 3 | ✓ |
| 3.5 | 4 | ✓ |
| -2.5 | -3 | ? |
| -3.5 | -4 | ? |

**风险等级：🟢 低**
- **设计选择**：`AwayFromZero` 是合理的统计取整方式
- **使用场景**：在游戏经济系统中，正负数值都可能出现，开发者需注意

### 15.2 Log10 对 0-1 输入的处理

**问题**：`Log10` 对 0 到 1 之间的值返回负数，对 0 返回负无穷，对负数返回 NaN。

```csharp
// DomainMath.cs:56-59
public static float Log10(float value)
{
    return (float)Math.Log10(value);
}
```

| 输入 | 输出 | 可用？ |
|------|------|--------|
| 1.0 | 0 | ✓ |
| 10.0 | 1 | ✓ |
| 0.5 | -0.301 | ✓（但在某些场景可能非预期）|
| 0.0 | `-Infinity` | ⚠️ 可能导致后续计算 NaN |
| -1.0 | `NaN` | ⚠️ 无防御性检查 |

**使用场景分析**：
- `DomainBattleSimulationSystem` 可能使用 `Log10` 计算军队数量比例
- 如果 `attackerPower = 0`，`Log10(0) = -Infinity` 可能导致后续 `Math.Pow` 溢出

### 风险等级：🟡 中等
- **潜在 NaN 传播**：Log10(0) 产生的 -Infinity 可能在后续计算中传播
- **缺乏输入验证**：没有对 0 和负数进行特殊处理

### 建议修复方案
```csharp
public static float Log10(float value)
{
    if (value <= 0f) return float.NegativeInfinity; // 或 float.MinValue 表示不可计算
    return (float)Math.Log10(value);
}

// 或者更严格的版本
public static float SafeLog10(float value, float fallback = 0f)
{
    if (value <= 0f) return fallback;
    float result = (float)Math.Log10(value);
    if (float.IsNaN(result) || float.IsInfinity(result)) return fallback;
    return result;
}
```

---

## 📊 总结 v2

### 严重缺陷（需立即修复）
1. **DomainBattleSimulationSystem 平局判定设计项**（🟡）— 当前平局为防守方守住；不得直接改为 `>=`，如需 Draw 需扩展结果模型
2. **DomainEngagementCleanup 不同步风险**（🟡→🔴新增高，已修复）— RestoreContestedRegion 现在同步 runtime 与 legacy occupationStatus
3. **DomainEngagementDetector 同势力检测遗漏** — 可能导致状态不一致

### 中等问题（建议近期修复）
4. **RegionState occupationStatus 无状态机** — 缺乏转换验证
5. **DomainOccupationSystem 同步不完整** — dual-state 可能漂移
6. **WorldStateFactory 存档验证缺失** — 损坏存档无法检测
7. **DomainEconomySystem 性能问题** — 多次遍历军队列表
8. **DomainMath.Log10 边界条件**（🟢→🟡新增中等，已修复）— Log10(0) 不再产生 -Infinity

### 低风险项（已确认或设计合理）
- **MapQueryService.FindRoute**（🟡→🟢降为低）— visited 保护确保无无限循环
- **IGameSystem 接口一致性**（🟡→🟢降为低）— 设计合理，部分系统通过委托调用是合理选择
- **DomainMapWarResolutionSystem**（🟢新增）— 整体调度逻辑设计合理
- **DomainMath.RoundToInt**（🟢新增）— AwayFromZero 取整是合理的设计选择
- **EventBus** — 使用 HashSet 可优化但当前实现可接受
- **NumericSystem** — NaN 处理设计合理

---

## 🔧 附录：审查文件清单 v2

| 文件 | 路径 | 风险等级 |
|------|------|--------|
| NumericSystem.cs | Core/ | 🟢 低 |
| DomainBattleSimulationSystem.cs | Domain/Military/ | 🔴 高（平局+invalid result） |
| DomainEngagementDetector.cs | Domain/Military/ | 🔴 严重 |
| DomainEngagementCleanup.cs | Domain/Military/ | 🔴 高（不同步）— 新增 |
| DomainOccupationSystem.cs | Domain/Military/ | 🟡 中等 |
| DomainGovernanceImpactSystem.cs | Domain/Governance/ | 🟡 中等 |
| DomainEconomySystem.cs | Domain/Economy/ | 🟡 中等 |
| WorldStateFactory.cs | Domain/World/ | 🟡 中等 |
| GameStateFactory.cs | Core/ | 🟡 中等 |
| EventBus.cs | Core/ | 🟢 低 |
| GameState.cs | Core/ | 🟡 中等 |
| WorldState.cs | Domain/World/ | 🟡 中等 |
| MapQueryService.cs | Domain/Map/ | 🟢 低 |
| IGameSystem.cs | Domain/Core/ | 🟢 低 |
| DomainMapWarResolutionSystem.cs | Domain/Military/ | 🟢 低 — 新增审查 |
| DomainMath.cs | Domain/Core/ | 🟡 中等（Log10）— 新增审查 |
| DomainArmyMovementSystem.cs | Domain/Military/ | 🟢 低 |
| StrategyCausalRules.cs | Domain/Core/ | 🟢 低 |
| GameContext.cs | Core/ | 🟢 低 |
| DataModels.cs | Data/ | 🟢 低 |
