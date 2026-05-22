# Web-UI 层代码审查报告

> 审查范围：`web-strategy-map/src` 下所有 TypeScript 源文件
> 审查目标：类型与 C# 数据模型一致性、WebGL 渲染管线性能、数据加载边界 case、UI 状态同步风险、错误处理覆盖

---

## 1. `src/main.ts` — 初始化流程

### 代码质量

**初始化顺序合理**：异步加载数据集 → 创建音频 → 创建场景 → 创建 UI → 启动渲染循环，依赖关系清晰。

**错误处理存在**：bootstrap 顶层有 `catch`，会输出 HTML 错误提示并转义消息。

**导出 API 完整**：`window.__WANCHAO_APP__` 暴露了所有关键操作（setMode、selectRegion、getDebugState、import/export），覆盖了游戏状态持久化的核心路径。

**跨层回调绑定干净**：`setMode` 内部正确同步 scene + ui + labelManager，`applyWarSelection` 正确处理了 waypoint/target 模式切换。

### 潜在问题

1. **`loadStrategyDataset` 失败时 UI 未定义**：如果 `dataset` 加载抛出异常，页面会显示 fatal-error，但 canvas 可能已存在于 DOM 中。`bootstrap()` 本身正确，但无法从数据加载失败中恢复游戏。

2. **`syncSceneAfterStateImport` 依赖时序**：第 392 行调用 `syncSceneAfterStateImport()` 时没有传参，内部用 `selectedRegionId` 时会取 undefined 再 fallback 到 activeArmyTargetId——这是预期行为，但缺少 `selectedRegionId` 参数导致 fallback 链较长。如果 `ui.exportGameState()` 返回的结构中 `selectedRegionId` 为空，可能触发不预期的区域选中。

3. **`scene.start()` 先于 `setMode('governance')` 执行**：场景在 start() 后立即 setMode 到 governance，但 labelManager 和 ui 的初始化在 start() 之后。此时 `scene.getMode()` 已经是 governance，但 labelManager 还未处理初始渲染。实际执行顺序是 scene.start() → labelManager.start() → setMode('governance') → selectRegion(scene.getSelectedRegion())，labelManager 在 start() 后已经以 governance mode 运行，但第一次 update 发生在 setMode 之前。

4. **`getDebugState` 错误处理返回降级状态**：catch 块返回了一个全零的降级状态，这可能导致调试工具误判为"正常但无数据"而非"发生了错误"。虽然 console.error 有日志，但返回值的语义是"游戏正在运行但渲染层异常"，实际是初始化错误。

### 风险等级

- 中等：数据加载异常后游戏无法恢复，用户看到 fatal-error 后需要刷新页面

### 建议

- 在 `loadStrategyDataset` 失败时增加重试逻辑（最多 3 次）或显示更完整的诊断信息（哪个 JSON 文件失败）
- 考虑在 `__WANCHAO_APP__` 初始化前添加版本/兼容性检查

---

## 2. `src/scene.ts` — Three.js 场景管理

### 代码质量

**渲染管线结构清晰**：Group 分层（terrain/building/occupation/enemy/friendly/logistics/route），每种对象类型有独立 Group，setMode 时通过 visibility 控制而非重建场景，性能策略合理。

**Shader 优化意识**：`antialias: true` 但 `pixelRatio` 限制了 2 倍采样；shadow map 仅 2048×2048，场景用 fog 替代距离裁切。

**内存管理**：dispose() 方法完整，包含了 timer、renderer 的清理，避免 WebGL 上下文泄漏。

**几何体复用**：buildRoute 中复用 TubeGeometry 的分段数（40/18）固定，避免每帧重新分配。Enemy threat marker 使用 routeCurve 插值而非固定位置，动画平滑。

### 潜在问题

**1. 每帧 draw call 可能超出目标**

构造函数一次性创建了所有地区的 mesh 和边界线：
- `buildRegions()` 创建了 `regions.length * 2` 个 mesh（region body + border line）
- `buildTerrainFeatures()` 创建了每个地区至少 3-8 个地形特征 mesh（山脉群、水渠、走廊等）
- `buildBuildingMarkers()` 创建了每个地区的建筑标记

**预估 draw call 规模**：假设 40 个地区，每地区平均 8 个 mesh，加上路线系统（2 个 tube + 3 个 convoy + 2 个 raid + 2 个 handle），总计约 400+ mesh。

即使使用 instanced geometry 优化（Three.js r160+ 支持），当前实现仍在每个 mesh 上调用 `renderer.render`。

**2. 纹理加载错误回调已补**

`buildMapTexture()` 当前先创建 fallback 材质，再异步加载底图：
```typescript
new TextureLoader().load(mapTextureUrl, onLoad, undefined, onError);
```
如果纹理加载失败（404、网络超时），当前会保留纯色 fallback 地图平面并输出可诊断 warning；成功加载后再替换 `material.map`。该项已由 `scene-texture-loader.test.ts` 静态回归测试锁定。

**3. 场景启动时所有 mesh 可见**

`setMode()` 中：
```typescript
this.terrainFeatureGroup.visible = true;
```
治理模式下所有地形特征都是可见的，但对于山区/走廊等复杂地形，8-10 个 mesh 的合并开销在治理模式下并不必要。存在按需简化（reducing detail on governance mode）的空间。

**4. `animateWarPressure` 每帧遍历所有 threat/countermeasure marker**

第 1671-1705 行：在每帧 animation loop 中对所有 enemy threat 和 countermeasure marker 进行位置插值计算。如果后期增加到 20+ 个威胁标记（对应多军出兵场景），每帧 CPU 开销会线性增长。

**5. `routeGroup.visible` 状态管理缺陷**

`setMode('war')` 时设置 `routeGroup.visible = true`，但 `rebuildRoute()` 会保留之前的 visible 状态：
```typescript
private rebuildRoute(): void {
    const wasVisible = this.routeGroup.visible;
    this.buildRoute(...);
    this.routeGroup.visible = this.mode === 'war' || wasVisible; // 逻辑正确
}
```
但在 line 1276，`buildRoute` 结束时硬编码了 `routeGroup.visible = false`，这覆盖了 rebuildRoute 的预期行为，导致路线在 governance 模式下切换到 war 时需要额外的 rebuildRoute 调用才能显示。

### 风险等级

- **高**：Draw call 超出预算会导致低端设备卡顿（30fps 以下）
- **已回收**：纹理加载失败静默问题已补 `onError` 与 fallback 材质
- **中**：动画系统 O(n) 遍历限制后期扩展

### 建议

- 使用 `InstancedMesh` 合并地区几何体（按 terrain 类型分组）
- 保持 `scene-texture-loader.test.ts` 作为纹理加载错误回调回归门
- 考虑实现视锥体剔除（frustum culling）以跳过屏幕外 mesh 的渲染
- `animateWarPressure` 应限制最大处理对象数量，超过后跳过旧对象

### 2026-05-21 5分钟找缺口/修补回收

- 找缺口轮确认：旧代码使用 `TextureLoader.load` 单参数形式，缺少 `onError` 与 fallback 材质。
- 修补轮已回收：`buildMapTexture()` 先创建纯色 fallback 材质，纹理成功后设置 `material.map`；失败时保留 fallback 并输出 warning。
- 新增 `tests/unit/scene-texture-loader.test.ts`，已先观察红灯：`TextureLoader.load should pass url, onLoad, onProgress, onError`，再修复到绿灯。
- 当前验证通过：`scene-texture-loader.test.ts` `1` test 通过；`npm --prefix web-strategy-map run build` 通过，含 `sync:data`、`check:data-source`、`tsc --noEmit`、Vite build；定向 Playwright smoke `loads map shell, emperor audio, governance, and camera selection` `1` test 通过。

---

## 3. `src/data.ts` — 数据加载与 JSON 契约

### 代码质量

**错误处理完整**：`loadJson` 捕获网络错误、HTTP 错误、JSON 解析错误，所有异常包装为 `StrategyDatasetLoadError` 并携带文件名和原因。

**验证逻辑严密**：
- `validateRegionDefinitions`：检查 id 唯一性、双向邻接关系
- `validateRegionShapeCoverage`：确保每个 region 都有 shape

**空数据防护**：`loadCollection` 检查 `items` 数组存在性，`aggregateNationMetric` 对非有限值做 sanitize。

### 潜在问题

**1. C# → TypeScript 类型不一致（已回收）**

从 `domain-core/src/Data/DataModels.cs` 与 `types.ts` 对比：

| C# 字段 | TS 是否存在 | 影响 |
|---------|------------|------|
| `RegionDefinition.regionSpecialization` | 已存在 | 作为 C#/TS 字段漂移回归门 |
| `RegionDefinition.supplyNode` | 已存在 | 作为 C#/TS 字段漂移回归门 |
| `RegionDefinition.gameplaySourceReference` | 已存在 | 作为 C#/TS 字段漂移回归门 |
| `EmperorStats.diplomacy` | 已存在 | `EmperorStats` 已具体化 |
| `EmperorStats.successionControl` | 已存在 | `EmperorStats` 已具体化 |
| `UnitDefinition.cost` | 已存在 | `CostSet` 已接入 |
| `UnitDefinition.upkeep` | 已存在 | `CostSet` 已接入 |
| `BuildingDefinition.requiresTech` | 已存在 | 建筑科技依赖字段已保留 |
| `ChronicleEventDefinition.category` | 已存在 | 事件分类字段已保留 |
| `ChronicleEventDefinition.trigger` | 已存在 | 事件触发器结构已保留 |

**已存在但对齐的字段**：`id`、`name`、`population`、`foodOutput`、`taxOutput`、`manpower`、`landStructure`、`legitimacyMemory`、`neighbors` 等核心字段已对齐。

**当前状态**：`data-contract-alignment.test.ts` 与 `data-contract-emperor-alignment.test.ts` 已作为字段漂移回归门，当前定向通过。

**2. `RegionViewModel.owner` 计算依赖硬编码列表**

第 122-123 行：
```typescript
const playerCore = new Set(['guanzhong', 'chang_an', 'xianyang', 'yongzhou', 'longxi', 'hexi', 'liangzhou']);
const rivalCore = new Set(['hanzhong', 'bashu', 'chengdu', 'luoyang', 'hedong', 'zhongyuan']);
```
硬编码列表会导致新地区加入时需要同步修改此处，且与 regions.json 中的 owner 字段（如果有）不同步。

**3. `loadStrategyDataset` 并行加载 16 个文件**

`Promise.all([...])` 同时发起 16 个 fetch。如果其中任何一个失败，整个 Promise 会 reject，但这意味着其他 15 个已成功的请求会被浪费（浏览器已接收但 TS 层无法使用）。

建议：使用 `Promise.allSettled` 或分批加载（8+8），优先加载核心文件（regions、metadata），后加载次要文件（audio、chronicle）。

**4. `buildRouteForecast` 复用导致循环依赖风险**

`data.ts` 中 `buildRouteForecast`（第 518-542 行）创建了一个静态的 RouteForecast，但 `army.unit` 来自 `unitsData.items[0]`，如果 units 数组为空会取 undefined。

**5. 历史数据关联不完整**

第 128 行：`if (!shape) continue;` 如果一个 region 定义存在但缺少 shape，整个 region 被静默跳过。用户看到"地图地区数量少于 JSON 中定义的数量"，但没有明确错误提示。

### 风险等级

- **已回收**：原 C# → TS 关键字段缺失风险已通过类型补齐和字段漂移测试回收
- **中等**：并行加载失败后的降级策略缺失

### 建议

- 保持 `data-contract-alignment.test.ts` 与 `data-contract-emperor-alignment.test.ts` 作为字段漂移回归门
- 将 `playerCore`/`rivalCore` 的硬编码改为从配置读取或从 region 定义中的 faction 字段推导
- 使用 `Promise.allSettled` 包装加载，并在 UI 层显示加载进度

---

## 4. `src/ui.ts` — UI 事件处理与状态同步

### 代码质量

**状态导出/导入双向对称**：`exportGameState` 和 `importGameState` 成对实现，schemaVersion 检查避免了不兼容存档的崩溃性读取。

**操作日志防溢出**：`trimTo(this.operationLog, 5)` 防止日志无限增长。

**事件委托统一**：所有按钮事件通过 `[data-action]` 等 data 属性委托到单个 `document.addEventListener('click')`，避免重复监听。

**存档完整性验证**：预览存档时会检查 schemaVersion 和 state 结构。

### 潜在问题

**1. `importGameState` 后 `mode` 被设置两次**

第 1082 行设置一次，第 1116 行又设置一次。虽然最终值相同（都是 restoredMode），但这暴露了状态机的不清晰：第 1082 行是立即设置，第 1116 行是在 `importWarLogisticsState` 之后设置（war logistics 会额外修改 state）。

**2. `onStateMutated` 回调链过长**

当 UI 状态变化时，触发 `onStateMutated` → main.ts 调用 `scene.syncArmyMarkers()` + `scene.refreshActiveRoute()` + `labelManager.update()`。如果用户在短时间内快速操作（如连续点击多个按钮），这些操作会串行执行，导致掉帧。

建议：加入 debounce（100ms）来合并状态变化后的重渲染。

**3. `activeArmy.unit` 在存档恢复时可能失效**

`restoreArmyRuntimeState`（第 1153-1186 行）中：
```typescript
const unit = this.dataset.units.find((candidate) => candidate.id === saved.unitId) ?? this.dataset.units[0];
```
如果 `saved.unitId` 指向一个已删除的单位，回退到 `units[0]`。这在大多数情况下是合理的（默认返回步兵），但如果 `units[0]` 本身被删除（units 数组为空），会抛异常。

**4. `getLogisticsMapObjects` 每次调用都重新构造对象数组**

第 1484-1550 行：每次调用都从 convoy、task、blockade、station 重新构建完整的 `LogisticsMapObject[]`。在高频调用场景（如 labelManager 每帧更新 + scene 选中等），会产生不必要的 GC 压力。

**5. `selectRegion` 的 CSS dataset 操作与 scene 状态可能不同步**

`main.ts` 第 154 行：`document.documentElement.dataset.selectedRegion = region.definition.id` 是全局状态。但 `ui.ts` 中 `setSelectedRegion` 不写 dataset，仅修改内存中的 `selectedRegion`。如果 UI 通过 CSS 选择器监听 `[data-selected-region="xxx"]` 但 scene 的 selected region 已被其他逻辑改变，两者可能短暂不同步。

**6. `parseGameSaveEnvelope` 验证过于宽泛**

第 1318 行：`if (!envelope.state || typeof envelope.state !== 'object')` 只检查了 `state` 是对象，但 `state` 内部可能缺少必要字段（如 `mode`、`regions`）。导入后可能在读取这些字段时产生 undefined 错误。

### 风险等级

- **中等**：类型缺失导致存档恢复不完整（unitId fallback 逻辑）
- **中等**：高频操作触发回调链导致 UI 卡顿
- **低**：dataset 与内存状态可能短暂不一致

### 建议

- 对 `onStateMutated` 引入 debounce/throttle（200ms）
- 考虑在 `getLogisticsMapObjects` 前检查增量变化，仅在 convoy/task/blockade 变化时重建
- 完善存档导入的字段级验证，而非仅检查 schemaVersion

---

## 5. `src/types.ts` — 类型定义与数据契约

### 代码质量

**TypeScript 类型覆盖**：所有 JSON 表字段都有对应的 TypeScript 接口，结构与 JSON schema 一致。

**枚举类型定义清晰**：`GameMode`、`GovernanceFocusId`、`GovernanceLaborId` 等使用了联合字符串字面量类型，比 enum 更适合 tree-shaking。

**RouteForecast 接口完整**：包含路线预测的所有字段（supplyCost、turns、contactChance、occupationCost、interceptionRisk）。

### 潜在问题

**1. `RegionDefinition` 关键字段已回收**

`regionSpecialization`、`supplyNode`、`gameplaySourceReference`、`landStructure` 已在 TS 类型中保留；后续重点是继续用字段漂移测试防回退。

**2. `EmperorDefinition.stats` 已具体化**

当前 TS：
```typescript
stats: EmperorStats;
```
并包含：
```csharp
military, administration, reform, charisma, diplomacy, successionControl
```

**3. `EmperorDefinition.score` 已具体化**

TS 已保留 C# 对应的 12 个 score 字段，包括 `virtue`、`wisdom`、`physique`、`aesthetics`、`diligence`、`ambition`、`dignity`、`tolerance`、`selfControl`、`personnelManagement`、`nationalPower`、`popularSupport`。

**4. `UnitDefinition` cost/upkeep 已接入**

当前 TS：
```typescript
cost?: CostSet;
upkeep: CostSet;
stats: UnitStats;
```

**5. `BuildingDefinition.requiresTech` 已接入**

`requiresTech?: string` 已保留，继续作为数据契约漂移检查的一部分。

**6. `ChronicleEventDefinition` 已补齐核心触发/分类字段**

`category?: string` 与 `trigger?: ChronicleTriggerDefinition` 已接入；后续如新增 seasonal/yield/affinity 子结构，应继续先补 `docs/data-contract.md` 和类型漂移测试。

**7. `PolicyDefinition.effects` 和 `risks` 已具体化**

当前使用 `EffectSet` / `RiskSet`，并保留字符串索引签名作为扩展 fallback。

### 风险等级

- **已回收**：原类型不完整风险已通过 TS 类型补齐和数据契约测试回收
- **低**：后续新增字段仍需依赖字段漂移测试防止回归

### 建议

- 保持 `CostSet`、`EffectSet`、`RiskSet`、`EmperorStats`、`UnitStats` 等具体接口作为默认写法
- 新字段先进入 `docs/data-contract.md` 和 `web-strategy-map/src/types.ts`，再进入数据表
- 继续运行 `tests/unit/data-contract-alignment.test.ts` 与 `tests/unit/data-contract-emperor-alignment.test.ts` 防止 C#/TS 漂移

---

## 6. `src/labels.ts` — 标签和文本资源

### 代码质量

**标签布局算法健壮**：使用优先级排序 + 碰撞检测 + 预算控制，支持 force-visible 标签（landform）和预算动态调整。

**DOM 操作优化**：只在标签存在性/文本内容变化时才操作 DOM，避免不必要的重排。

**模式切换响应正确**：`setMode` 触发 `update(true)` 强制重算，标签显示/隐藏正确响应 governance/war 模式切换。

### 潜在问题

**1. `layoutLabels` 每帧执行**

`update(false)` 在 `start()` 中的 tick loop 每帧调用。`layoutLabels` 包含：
- `resolveBudget` 计算
- `projectToScreen` 对每个 anchor 调用 Three.js 投影
- DOM `getBoundingClientRect` 查询
- 碰撞检测 O(n²) 遍历

对于 40 个地区的场景，每帧 ~800 次 DOM 查询 + 1600 次投影计算。在普通浏览器中仍可接受，但笔记本/移动端可能感受到延迟。

**2. 标签预算基于屏幕尺寸和相机距离**

```typescript
if (distance > 20) return this.mode === 'war' ? 14 : 12;
```
预算切换是离散的（14/24/38），在边界距离时可能造成标签数量突变（突然显示/隐藏大量标签），视觉上不够平滑。

**3. `resolveBudget` 依赖于 `window.innerWidth/innerHeight`**

如果浏览器窗口大小变化但 canvas 尺寸未同步更新（resize 事件未触发），标签预算会基于错误的窗口尺寸计算。

**4. 标签文本内容硬编码**

第 446 行：`text: region.definition.name` 等文本来自 JSON 定义，没有国际化支持或文本过长截断。如果地区名称过长（如"京兆府"），标签可能会溢出容器。

### 风险等级

- **低**：性能可接受，但标注了优化空间
- **低**：标签切换不平滑，预算边界可能造成视觉跳跃

### 建议

- 将 `layoutLabels` 的执行频率降低到 10-15 FPS（通过 timestamp 限制），标签位置不需要 60fps 更新
- 对预算计算使用平滑过渡（lerp），避免离散切换
- 添加文本截断逻辑（最多 6 字符）+ 悬停 tooltip 显示完整名称

---

## 7. `src/audio.ts` — 音频加载和播放

### 代码质量

**资源延迟加载**：`playMusic` 在首次切换场景时才创建 Audio element，避免启动时长时间阻塞。

**错误隔离**：`tryPlay` 中 error 事件不会让玩法崩溃；Promise reject 和 media error event 都会写入 `lastError`，并由 HUD 的 `#audio-error` 呈现。

**cue 映射完整**：`musicByScene`、`emperorThemeById`、`chronicleEventById` 映射覆盖所有场景切换和事件触发。

### 潜在问题

**1. Audio 旧元素释放已回收，仍缺复用池**

第 134-150 行：每次切歌仍会创建 `new Audio(source)`。旧元素已通过 `pause()`、`src = ''` 和 `load()` 释放媒体资源，不再是未释放旧元素问题；剩余风险是频繁切换时仍依赖 GC，尚未实现 Audio 元素复用池。

**2. `playMusic` 音频切换无 crossfade**

第 145 行 `await this.tryPlay(next)` 后立即 pause previous，新音乐开始时没有音量渐变，听觉上会有突兀感。

**3. 音频错误和加载进度已显示**

第 178-187 行：`element.play()` reject 会写入 `lastError`，media error event 会按 `HTMLMediaElement.error.code` 写入可读分类，`renderAudioHud()` 会把它显示到 `#audio-error`。当前已有 unit 覆盖 media error event 和 `MEDIA_ERR_SRC_NOT_SUPPORTED` 分类，Playwright E2E 覆盖浏览器 autoplay 拒绝时的 HUD 呈现；后续第 44-48 轮已补齐 media loading stage、HUD 可读进度、`loadingProgress` 50% 正常路径和 0/100 clamp 边界。

**4. 音频路径已走统一资源 helper**

第 135、157、169 行：音乐、旁白和帝皇语音路径已通过 `gameDataAssetUrl('audio/...')` 生成，避免与图片/地图资产的 URL 归一化规则分叉。

**5. 音频加载进度指示已回收**

用户点击"启用音频"后，音频 HUD 现在会显示真实 media loading 阶段和可读缓冲百分比；`audio-not-enabled.test.ts` 已覆盖 `loadstart`/`canplay`、50% 进度、0/100 clamp 和无效 duration 的 `null` 回落。仍需关注的是 Audio 元素复用和切歌 crossfade，而不是加载进度缺失。

### 风险等级

- **低**：音频播放失败不影响核心游戏玩法，且 autoplay reject 与 media error 已显示到 HUD
- **低**：旧 Audio 元素释放已回收；频繁切换仍缺复用池，crossfade 也未实现

### 建议

- 使用 `this.musicElement` 池化（最多保留 3 个 Audio 元素，循环复用）
- 实现 500ms 音量的 crossfade
- 保留 HUD `lastError`、`loadingStage` 和 `loadingProgress` 回归门

### 2026-05-21 5分钟找缺口复核

- 当前 `audio.ts` 仍在 `playMusic()`、`playNarration()`、`playVoice()` 中直接拼接 `/game-data/audio/...`。
- 当前项目已有 `gameDataAssetUrl(assetPath)`，并有 `game-data-asset-url.test.ts` 覆盖路径穿越和反斜杠归一化；UI 图片资产已通过该 helper 生成 URL。
- 缺口判断：当前不是播放失败；风险在于音频路径绕过统一资源 URL 边界，未来调整 `assetRoot`、部署子路径或安全归一化规则时，音频链路会与图片/地图资产行为分叉。
- 定向验证通过：`audio-not-enabled.test.ts` 与 `game-data-asset-url.test.ts` 共 `2` files / `7` tests。
- 下一轮修补建议：先新增音频 URL 静态/unit 回归，要求 `audio.ts` 通过 `gameDataAssetUrl('audio/...')` 构造 `new Audio()` source，再小步替换三处硬编码路径。

### 2026-05-21 5分钟修补回收：音频诊断

- 音频 URL 边界已回收：`audio.ts` 统一通过 `gameDataAssetUrl('audio/...')` 生成音乐、旁白和帝皇语音 source。
- media error event 诊断已回收：`tryPlay()` 的 `error` listener 会写入 `Audio failed to load: <source>`，不抛异常、不恢复 console 噪声。
- 定向验证通过：`audio-not-enabled.test.ts` 与 `game-data-asset-url.test.ts` 共 `2` files / `9` tests；`typecheck`、`build` 与 autoplay HUD Playwright E2E 通过。

### 2026-05-21 5分钟找缺口复核：旧音频迟到错误

- 当前 `tryPlay()` 的 media `error` handler 只按 `element.src` 写入 `lastError`，没有判断该 `element` 是否仍是当前音乐、旁白或语音元素。
- `playMusic()` 会在新音乐播放后清空旧 music element 的 `src`，但旧 error 事件如果迟到仍可能写入空 source 错误；`playNarration()` 和 `playVoice()` 暂停旧元素后没有清空旧 `src`，迟到 error 更容易把当前 HUD 覆盖成旧资源错误。
- 当前 unit 只覆盖单个 audio element 的 media error，不覆盖“旧元素被新元素替换后旧 error 迟到”的场景。
- 下一轮可用低风险修补：让 `tryPlay()` 接收当前性判断，或在 error handler 中只在 `this.musicElement === element` / `this.narrationElement === element` / `this.voiceElement === element` 时写入 `lastError`。

### 2026-05-21 5分钟修补回收：旧音频迟到错误

- 旧音频迟到 error 保护已回收：`tryPlay()` 现在接收当前性判断，只有仍被当前音乐、旁白或语音槽位持有的 element 才能写入 media `lastError`。
- 新增 unit 覆盖旧 music element 被新 music element 替换后，旧 element 的迟到 error 不会覆盖当前 `lastError`。
- 定向验证通过：`audio-not-enabled.test.ts` 与 `game-data-asset-url.test.ts` 共 `2` files / `10` tests；`typecheck`、`build` 与 autoplay HUD Playwright E2E 均通过。

### 2026-05-21 5分钟找缺口复核：旁白/语音旧元素资源释放

- 当前 `playMusic()` 在新音乐播放成功后会 `pause()` 旧 music element 并清空 `src`，但 `playNarration()` 与 `playVoice()` 只 `pause()` 旧 element，没有清空旧 `src`。
- 现有 unit 只覆盖旧 music element 迟到 error 不覆盖 `lastError`，没有覆盖旧 narration / voice element 的资源释放或迟到 error 场景。
- 缺口判断：当前不影响核心玩法，也已有当前性 guard 防止旧 error 覆盖 HUD；剩余风险是频繁触发旁白/语音时旧媒体 source 保留到 GC，内存与网络资源释放不可观测。
- 下一轮可低风险修补：提取 `releaseAudioElement(element)` 小 helper，对 music/narration/voice 旧元素统一执行 `pause()` 与 `src = ''`；补 unit 验证替换 narration/voice 时旧元素 source 被清空，且 `lastError` 不被旧 element 迟到 error 覆盖。

### 2026-05-21 5分钟修补回收：旁白/语音旧元素资源释放

- 旁白/语音旧元素资源释放已回收：`audio.ts` 新增 `releaseAudioElement()`，音乐、旁白和帝皇语音替换旧 element 时统一执行 `pause()` 与 `src = ''`。
- 新增 unit 覆盖旧 narration / voice element 被替换后 source 被清空，并触发旧 element 迟到 error 确认不会覆盖当前 `lastError`。
- 验证通过：`audio-not-enabled.test.ts` `1` file / `8` tests；`typecheck`；`build`，其中 build 包含 `sync:data`、`check:data-source`、`tsc --noEmit` 与 Vite build。

### 2026-05-21 5分钟找缺口复核：旧媒体复位未调用 load

- 当前 `releaseAudioElement()` 已统一 `pause()` 并清空 `src`，但未调用 `HTMLMediaElement.load()` 触发浏览器资源选择复位。
- 缺口判断：当前不影响播放链路，也不影响 `lastError` 当前性 guard；剩余风险是旧媒体元素在浏览器内部仍可能保留部分加载状态，资源释放语义没有被 unit 锁定。
- 下一轮可低风险修补：在 `releaseAudioElement()` 清空 `src` 后调用 `element.load()`；补 unit mock `load`，断言旧 narration / voice element 被替换时 `pause()`、`src=''`、`load()` 均发生。

### 2026-05-21 5分钟修补回收：旧媒体复位 load

- 旧媒体复位已回收：`releaseAudioElement()` 在 `pause()` 与 `src = ''` 后调用 `load()`，显式触发浏览器媒体资源选择复位。
- Audio unit mock 已补 `load: vi.fn()`，旧 narration / voice element 替换测试同步断言 `pause()`、`src=''` 与 `load()`。
- 验证通过：`audio-not-enabled.test.ts` `1` file / `8` tests；`typecheck`；`build`，其中 build 包含 `sync:data`、`check:data-source`、`tsc --noEmit` 与 Vite build。

### 2026-05-21 5分钟找缺口复核：启用音频并发点击

- `bindAudioHud()` 中 `#audio-enable` 点击后直接执行 `void audio.enable().then(() => renderAudioHud(audio))`，没有立即写入“启动中”状态，也没有禁用按钮或 pending guard。
- `StrategyAudio.enable()` 一开始就把 `enabled` 设为 `true`，但 HUD 只有 promise 完成后才刷新；如果音频加载/播放耗时，用户仍看到“点击启用音频”，容易重复点击。
- 重复点击会并发启动多条 `enable()` / `setMode()` / `playNarration()` 链路，虽然当前性 guard 能避免旧 error 覆盖，但仍会产生多余 Audio element 与多次播放尝试。
- 下一轮可低风险修补：在 `bindAudioHud()` 内加本地 `audioEnablePending` guard，点击后立即禁用按钮并显示“音频启动中”，promise settle 后再解锁和刷新 HUD。

### 2026-05-21 5分钟修补回收：启用音频 pending

- 启用音频 pending 保护已回收：`bindAudioHud()` 现在在点击后立即显示“音频启动中”，禁用 `#audio-enable`，并用本地 guard 防止重复启动。
- promise settle 后会解锁按钮并刷新 HUD；若音频已启用，后续点击直接返回。
- 新增 Playwright 覆盖 pending 状态、按钮 disabled 和重复 DOM click 不触发第二次 `play()`。
- 定向验证通过：音频 HUD Playwright `2` tests、`typecheck`、`build`。

### 2026-05-21 5分钟找缺口复核：音频动作按钮 pending

- `#audio-enable` 已有 pending guard，但 `data-audio-action="mode|emperor|event"` 三个动作按钮仍直接执行 `audio.setMode()`、`audio.playEmperorTheme()` 或 `audio.playEventCue()`。
- 当启用音频的首个 `audio.enable()` 尚未 settle 时，`StrategyAudio.enabled` 已经是 `true`，用户点击动作按钮会启动额外播放链路。
- 当前 Playwright 只覆盖 `#audio-enable` 重复点击不触发第二次 `play()`，没有覆盖 pending 期间点击动作按钮不应触发额外 `play()`。
- 下一轮可低风险修补：复用 `audioEnablePending`，在 pending 时禁用 `.audio-action` 或直接忽略动作按钮点击；补 Playwright 验证 pending 期间动作按钮点击不会增加 `play()` 次数。

### 2026-05-21 5分钟修补回收：音频动作按钮 pending

- 音频动作按钮 pending 保护已回收：`bindAudioHud()` 现在统一维护音频启动 pending 状态，启动期间同时禁用 `#audio-enable` 与 `[data-audio-action]` 按钮。
- 动作按钮 click handler 也会在 pending 期间直接返回，防止程序化 click 绕过 disabled 状态后额外触发 `play()`。
- 扩展 Playwright 覆盖 pending 期间帝王主题按钮 disabled，以及 DOM `dispatchEvent('click')` 不会让 `__AUDIO_PLAY_CALLS__` 从 `1` 增加到 `2`。
- 定向验证通过：音频 HUD Playwright `2` tests、`typecheck`、`build`。

### 2026-05-21 5分钟找缺口复核：同一音乐 cue 失败后不可重试

- `playMusic()` 在发现 `this.musicElement?.dataset.source === source` 时直接返回，不区分该元素是否已成功播放。
- 当前 unit 覆盖了“播放失败后切换到另一个 cue 成功清空 `lastError`”，但没有覆盖“同一个 cue 首次 `play()` reject 后再次请求应重试”的路径。
- 缺口判断：不影响核心玩法，且错误已显示到 HUD；风险在于浏览器短暂拒绝或资源瞬时失败后，用户再次触发当前模式音乐时不会重新调用 `play()`，只能靠切换到其他 cue 间接恢复。
- 下一轮可低风险修补：为音乐播放记录成功状态，或在 `tryPlay()` 失败后释放当前 music element，使同源 cue 的下一次请求可以重新创建/播放；补 unit 断言同一 cue 失败后再次 `setMode()` 会触发第二次 `play()`。

### 2026-05-21 5分钟修补回收：同一音乐 cue 失败后重试

- 同一音乐 cue 重试缺口已回收：`tryPlay()` 现在返回播放尝试是否成功，`playMusic()` 在当前 music element 播放失败后释放并清空槽位。
- 新增 unit 覆盖首次 `play()` reject 后再次 `setMode('governance')` 会触发第二次 `play()`，并在成功后清空 `lastError`。
- 定向验证通过：`audio-not-enabled.test.ts` `1` file / `9` tests；`typecheck`；`build`，其中 build 包含 `sync:data`、`check:data-source`、`tsc --noEmit` 与 Vite build。

### 2026-05-21 5分钟找缺口复核：media error 细分原因

- `tryPlay()` 的 media `error` listener 当前只写入 `Audio failed to load: <source>`，没有读取 `HTMLMediaElement.error?.code` 或 `message`。
- 当前 unit `surfaces media loading errors in debug state` 只触发通用 `error` 事件，没有模拟 `MEDIA_ERR_NETWORK`、`MEDIA_ERR_DECODE` 或 `MEDIA_ERR_SRC_NOT_SUPPORTED`。
- 缺口判断：不影响核心玩法，也已有 HUD 呈现；风险在于文件缺失、网络失败、解码失败和格式不支持都会显示同一类文案，后续定位音频资源问题时诊断粒度不足。
- 下一轮可低风险修补：补 unit mock `element.error = { code: 4, message: '...' }`，要求 `lastError` 带出可读分类；实现一个小型 `describeMediaError()` helper，保持未知 code 仍回落到通用文案。

### 2026-05-21 5分钟修补回收：media error 细分原因

- media error code 分类已回收：`tryPlay()` 的 `error` listener 通过 `describeMediaLoadError()` 读取 `HTMLMediaElement.error.code` 与 `message`，可区分 aborted、network、decode 和 source not supported。
- 未知 code 或无 `element.error` 时仍回落到旧文案 `Audio failed to load: <source>`，避免扩大 HUD 行为面。
- 新增 unit 覆盖 `code: 4` 与 `message: 'unsupported codec'`，确认 `lastError` 输出 `Audio failed to load (source not supported: unsupported codec): <source>`。
- 验证通过：`audio-not-enabled.test.ts` `1` file / `10` tests；`typecheck`；`build`，其中 build 包含 `sync:data`、`check:data-source`、`tsc --noEmit` 与 Vite build。

### 2026-05-21 5分钟找缺口复核：音频动作播放 pending

- `bindAudioHud()` 目前只在 `audio.enable()` 启动期间用 `audioEnablePending` 禁用 `#audio-enable` 和 `[data-audio-action]`。
- 音频已经启用后，动作按钮 click handler 只检查 `audioEnablePending`，随后直接执行 `audio.playEmperorTheme()`、`audio.playEventCue()` 或 `audio.setMode()`；同一个动作 promise 未 settle 时，重复 click 仍会启动额外播放链路。
- 当前性 guard 与 `releaseAudioElement()` 能降低 HUD 污染和旧资源残留，但无法阻止用户快速点击时创建多余 Audio element 与多次 `play()` 尝试。
- 缺口判断：不影响核心玩法，属于低风险 UX/资源治理问题；下一轮可低风险修补为动作按钮增加独立 `audioActionPending` guard，播放任务 settle 前禁用 `[data-audio-action]` 并显示“音频切换中”，补 Playwright 或 unit 断言重复 DOM click 不增加 `play()` 次数。

### 2026-05-21 5分钟修补回收：音频动作播放 pending

- 音频动作 pending 保护已回收：`bindAudioHud()` 现在维护独立 `audioActionPending`，音频动作 promise settle 前禁用 `[data-audio-action]`。
- 动作启动后 HUD 立即显示“音频切换中”，promise settle 后恢复按钮并重新渲染真实 audio debug 状态。
- 新增 Playwright 覆盖音频已启用后事件音频动作挂起时，按钮 disabled，程序化重复 click 不会让 `__AUDIO_ACTION_PLAY_CALLS__` 从 `1` 增加到 `2`。
- 验证通过：音频动作 pending Playwright `1` test；`typecheck`；`build`，其中 build 包含 `sync:data`、`check:data-source`、`tsc --noEmit` 与 Vite build。

### 2026-05-21 5分钟找缺口复核：音频加载状态/进度

- `StrategyAudioDebugState` 当前只暴露启用、模式、当前 cue、catalog 计数和 `lastError`，没有 `loading`、`loadingSource`、`buffered` 或可读加载阶段。
- `tryPlay()` 只监听 media `error` 并等待 `element.play()`，未监听 `loadstart`、`loadedmetadata`、`canplay`、`canplaythrough` 或 `progress`，也没有读取 `duration`/`buffered`。
- `bindAudioHud()` 的“音频启动中”和“音频切换中”只表示操作 promise pending；promise settle 后 `renderAudioHud()` 又回到“音频已启用”，不能区分正在加载、已可播放、网络慢或缓冲不足。
- 现有 Playwright 覆盖 autoplay reject、启用 pending 和动作 pending，但未覆盖 media 加载事件到 HUD/debug state 的映射。
- 缺口判断：不影响核心玩法，属于低风险 UX/诊断问题；下一轮可低风险补强为 `StrategyAudio` 暴露最近一次加载阶段，并让 HUD 在音频任务 pending 时显示真实 media loading 文案，补 unit 或 Playwright 模拟 `loadstart`/`canplay` 事件。

### 2026-05-21 5分钟修补回收：音频加载状态/进度

- 音频加载状态缺口已回收：`AudioDebugState` 新增 `loadingStage`、`loadingSource`、`loadingProgress` 和 `loadingMessage`，覆盖 idle/loading/metadata/buffering/canplay/ready/playing/error。
- `tryPlay()` 现在监听 `loadstart`、`loadedmetadata`、`progress`、`canplay` 和 `canplaythrough`，并在播放 promise settle 后阻止迟到加载事件继续覆盖 HUD。
- `bindAudioHud()` 已注册 audio debug state listener，HUD 状态优先显示真实 media loading 文案；pending guard、autoplay reject 和动作防重复逻辑保持可用。
- 新增 unit 覆盖 `loadstart`/`canplay` 到 debug state 的转换；新增 Playwright 覆盖浏览器实际 media loading 阶段能显示到 `#audio-status` 并暴露到 debug state。
- 验证通过：`audio-not-enabled.test.ts` `1` file / `11` tests；audio Playwright grep `5` tests；`typecheck`；`build`，其中 build 包含 `sync:data`、`check:data-source`、`tsc --noEmit` 与 Vite build。

### 2026-05-21 5分钟找缺口复核：音频加载百分比回归

- `AudioDebugState` 已暴露 `loadingProgress`，`audioLoadingMessage()` 也会在 progress 非空时输出 `音频加载中 50%` 或 `音频缓冲中 50%`。
- 当前 unit 只覆盖 `loadstart` 与 `canplay` 的阶段文案，Playwright 只断言 loading stage 已进入 HUD/debug state；没有模拟 `duration` 与 `buffered.end()`，也没有断言百分比四舍五入、0-100 clamp 或 HUD 百分比文案。
- 缺口判断：不影响核心玩法，属于低风险测试覆盖缺口；风险在于未来改动 `readBufferedProgress()` 或浏览器 mock 结构时，加载百分比坏掉但现有 audio tests 仍全绿。
- 下一轮可低风险修补：在 `audio-not-enabled.test.ts` 增加一个 mock `duration=10`、`buffered.end(...)=5` 的 `progress` event 用例，断言 `loadingProgress=50` 且 `loadingMessage='音频缓冲中 50%'`；必要时再补 100% clamp case。

### 2026-05-21 5分钟修补回收：音频加载百分比回归

- 音频加载百分比回归缺口已回收：`audio-not-enabled.test.ts` 新增 `reports buffered progress percentage in debug state`。
- 新用例 mock `duration=10`、`buffered.end(...)=5` 并触发 `progress` event，断言 `loadingStage='buffering'`、`loadingProgress=50`、`loadingMessage='音频缓冲中 50%'`。
- 新用例首跑即通过，说明第 44 轮生产实现已满足百分比计算；本轮未改 `audio.ts` 或 `main.ts`。
- 验证通过：新增 grep `1` test；完整 `audio-not-enabled.test.ts` `1` file / `12` tests；`typecheck`。

### 2026-05-21 5分钟找缺口复核：音频进度 clamp 边界

- `readBufferedProgress()` 已用 `Math.max(0, Math.min(100, ...))` 把百分比限制在 `0..100`，但当前 unit 只覆盖 `duration=10`、`bufferedEnd=5` 的 50% 正常路径。
- 当前未覆盖 `bufferedEnd > duration` 被夹到 100%、`bufferedEnd < 0` 被夹到 0%、`duration <= 0` 或 `buffered.length=0` 返回 `null` 的边界。
- 缺口判断：不影响核心玩法，属于低风险测试覆盖缺口；风险在于未来重写 `readBufferedProgress()` 时，进度百分比可能溢出 HUD 或失去空状态保护而不被现有测试发现。
- 下一轮可低风险修补：在 `audio-not-enabled.test.ts` 增加 clamp 边界用例，复用现有 media mock，断言 150% 输入输出 100%、负值输入输出 0%、无效 duration 输出 `null` 与无百分比文案。

### 2026-05-21 5分钟修补回收：音频进度 clamp 边界

- 音频进度 clamp 边界缺口已回收：`audio-not-enabled.test.ts` 新增 `clamps buffered progress percentage boundaries in debug state`。
- 新用例覆盖 `duration=10` / `bufferedEnd=15` 输出 100%、`duration=10` / `bufferedEnd=-1` 输出 0%、`duration=0` 输出 `null` 且文案回落为 `音频缓冲中`。
- 新用例首跑即通过，说明第 44 轮生产实现已满足 clamp/null 行为；本轮未改 `audio.ts` 或 `main.ts`。
- 验证通过：新增 grep `1` test；完整 `audio-not-enabled.test.ts` `1` file / `13` tests；`typecheck`。

### 2026-05-21 5分钟找缺口复核：数据加载失败 E2E

- `bootstrap().catch()` 已会向页面插入 `.fatal-error[role="alert"]`，并用 `escapeHtml()` 输出 `loadStrategyDataset()` 的失败信息。
- 当前 Web 层已有 unit 覆盖 `loadStrategyDataset()` 对 404、网络失败、空 JSON、缺少 `items`、重复 id、邻接/shape 不一致等情况抛出 `StrategyDatasetLoadError`。
- 缺口在浏览器集成层：`strategy-map.spec.ts` 没有覆盖任一数据 JSON 请求失败时页面必须显示 `fatal-error`，也没有断言失败信息包含具体文件名且不触发未处理 `pageerror`。
- 下一轮可低风险补强：新增 Playwright 用例拦截 `/game-data/data/regions.json` 返回 404，断言 `.fatal-error` 可见、文案包含 `regions.json`，并确认 `document.documentElement.dataset.appReady` 不为 `true`。先锁住现有降级行为，再决定是否做“重试加载”按钮。

### 2026-05-21 5分钟修补回收：数据加载失败 E2E

- 数据加载失败浏览器降级路径已补 E2E：新增 Playwright 用例拦截 `/game-data/data/regions.json` 返回 404。
- 用例断言 `.fatal-error[role="alert"]` 可见，文案包含 `加载失败`、`regions.json` 与 `HTTP 404`。
- 用例同步断言 `document.documentElement.dataset.appReady` 不会被置为 `true`，并确认无未处理 `pageerror`。
- 定向验证通过：数据失败降级 Playwright `1` test、`typecheck`、`build`。

### 2026-05-21 5分钟找缺口复核：Web UI 审查摘要过期

- 复核发现 `src/audio.ts` 章节的潜在问题、建议、测试文件审查剩余风险和总结风险矩阵仍把“缺加载进度”或“无音频加载进度指示”列为未回收项。
- 当前真实状态：第 44-48 轮已补 `loadingStage`、`loadingMessage`、`loadingProgress`、HUD 展示、50% 正常路径、0/100 clamp 与无效 duration 的 `null` 回落测试。
- 缺口判断：这是审查文档摘要漂移，不影响 runtime；风险在于后续修补轮继续按过期摘要重复投入 Web audio 进度问题，而忽略仍真实存在的 Audio 元素复用、crossfade、数据恢复和 scene draw call 风险。
- 下一轮可低风险修补：只更新本审查文档的 audio 初始段、测试剩余风险和总结风险矩阵，将“缺加载进度”改为“加载进度已回收，剩余为 Audio 元素复用/crossfade”，不改产品代码。

---

## 8. 测试文件审查

### 测试覆盖概览

| 测试文件 | 覆盖目标 | 状态 |
|---------|---------|------|
| `data-contract-alignment.test.ts` | C# ↔ TS 字段对齐 | 已通过 |
| `data-contract-emperor-alignment.test.ts` | 帝皇数据契约对齐 | 已通过 |
| `data-contract-emperor-alignment.test.ts:45` | TS 中 score 字段存在性 | 已通过 |
| `bundle-budget.test.ts` | Vite 打包体积预算 | 存在 |
| `performance-baseline.test.ts` | Playwright headless 性能基线 | 存在 |
| `headless-vs-ui-numerics.test.ts` | Headless vs UI 数值一致性 | 存在 |
| `game-data-asset-url.test.ts` | 资产 URL 规范化 | 存在 |

### 关键发现

**已回收的问题**：`data-contract-alignment.test.ts` 和 `data-contract-emperor-alignment.test.ts` 现在通过，覆盖 `gameplaySourceReference`、`regionSpecialization`、`supplyNode`、`versionScope`、`aiPersonality`、`diplomacySkills` 和 Emperor score 结构。

### 测试文件风险

- **测试套件存在且当前通过**：数据契约验证已经成为字段漂移回归门
- **已回收失败测试仍需保留**：后续 C#/TS 字段变更应继续先跑定向契约测试
- **缺少场景测试**：没有针对空数据、网络超时、JSON 结构异常的集成测试

### 2026-05-21 5分钟找缺口复核

- 当前 `data-contract-alignment.test.ts` 与 `data-contract-emperor-alignment.test.ts` 定向 unit 已通过：`2` files / `9` tests。
- 当前 `strategy-map.spec.ts` 已新增 autoplay reject E2E，验证 `HTMLMediaElement.play()` 拒绝后 `#audio-error` 呈现 `lastError`；定向 E2E 通过：`1` test。
- 原旧状态描述已回收：数据契约测试、`types.ts` 字段完整性和 audio HUD 错误呈现均已改为当前验证状态。
- 仍成立的剩余风险：Audio 元素复用、crossfade、scene.ts draw call 优化。

### 2026-05-21 5分钟修补回收

- 已将数据契约测试、types.ts 字段缺失和 audio lastError HUD 不显示三类过期描述改为当前验证状态。
- 当前定向验证：数据契约 unit `2` files / `9` tests 通过；autoplay reject E2E `1` test 通过。
- 本轮只改审查文档，不改 Web runtime、测试代码或数据。

### 2026-05-21 5分钟找缺口复核：Audio 元素累积摘要漂移

- 复核 `src/audio.ts` 与本审查记录后确认：旧音乐、旁白和帝皇语音元素的基础释放链路已在前序修补轮回收，当前 `releaseAudioElement()` 会执行 `pause()`、`src = ''` 和 `load()`。
- 当前总结风险矩阵仍写作“Audio 元素累积”，容易把已回收的旧元素释放问题与仍真实存在的“无复用池 / 无 crossfade”混在一起。
- 缺口判断：这是文档摘要漂移，不影响 runtime；风险在于下一轮继续按“元素累积”重复投入，而不是准确评估是否需要 Audio pool、复用策略或 crossfade。
- 下一轮可低风险修补：只更新总结风险矩阵和 audio 摘要，将“Audio 元素累积”改为“旧元素释放已回收；仍无复用池 / crossfade”，不改 Web runtime、测试代码或数据。

### 建议

- 保持数据契约测试作为必跑回归门
- 添加边界 case 测试：空 regions.json、缺失 shape 的 region、网络超时
- 考虑将 data contract 测试与 CI 绑定（failing test = blocking PR）

---

## 总结：风险矩阵

| 文件 | 问题 | 风险等级 | 紧急度 |
|------|------|----------|--------|
| types.ts | C# → TS 类型缺失已回收；后续需防字段漂移 | 低 | 中 |
| data.ts | 并行加载失败无降级策略；硬编码 playerCore/rivalCore | 中 | 中 |
| scene.ts | Draw call 超预算；纹理加载 fallback 已补并有回归测试 | **高** | 中 |
| ui.ts | 高频操作回调链过长；getLogisticsMapObjects 每次重建 | 中 | 中 |
| main.ts | 数据加载失败后游戏无法恢复 | 中 | 低 |
| labels.ts | 标签布局每帧执行；预算切换不平滑 | 低 | 低 |
| audio.ts | 旧元素释放已回收；仍无 Audio 复用池；加载进度已回收；crossfade 未实现；autoplay 与 media error code 已显示到 HUD | 低 | 低 |

---

## 优先行动项

1. **持续**：保持 `types.ts` 字段契约测试为回归门，新字段先补类型和数据契约
2. **持续**：保持 `scene-texture-loader.test.ts` 作为纹理错误回调回归门
3. **本周**：将 `onStateMutated` 回调链加入 debounce（200ms）
4. **本周**：将 playerCore/rivalCore 从硬编码改为从配置或 JSON 推导
5. **计划**：使用 InstancedMesh 优化地区 mesh 的 draw call，考虑 LOD 分级

---

*审查完成日期：2026-05-17 | 审查范围：web-strategy-map/src 所有 TypeScript 文件 + tests/unit/* | 关联 C# 模型：domain-core/src/Data/DataModels.cs*
