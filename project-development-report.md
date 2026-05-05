# 《万朝归一：九州帝业》Project Development Report

更新日期：2026-04-30

## 当前状态

- 阶段：地图主导战争闭环重写启动，执行 `.omc/plans/complete-architecture-map-led-war-rewrite-consensus.md`。
- 当前目标：保留现有 JSON 数据资产，重写 C# 运行层，让行军、接敌、战斗、占领、地图刷新、UI反馈和治理后果都从同一地图状态模型连续发生。
- 技术方向：Unity + C#，2D 节点地图 + 区域面片，UGUI，JSON 数据表。
- 版本策略：国内版完整架构优先，全球版只保留数据结构接口。
- 旧状态说明：此前“MVP 内容扩展完成，准备进入 Unity 编译验证”的阶段记录保留为历史基线；从 2026-04-29 起，军事运行层不再继续围绕旧 BattleSession 架构扩展。

## 数据规模

| 数据表 | 数量 | 说明 |
|--------|------|------|
| 帝皇 | 13 | 8 位 MVP + 5 位扩展 |
| 区域 | 56 | 从 17 扩展，覆盖全国 |
| 科技 | 40 | 从 14 扩展，含建筑/军事/文化科技 |
| 事件 | 200 | 基于渤海小吏五本书的历史事件 |
| 将领 | 12 | 历史将领，有地形和兵种加成 |
| 建筑 | 12 | 文明 6 式区域建筑 |
| 政策 | 35 | 内政/军事/经济/文化政策 |
| 兵种 | 8 | 步兵/骑兵/弩兵/守军/舟师/攻城/火枪 |
| 历史层 | 56 | 每个区域的地理/风俗/兵器/资源数据 |
| 地图面片 | 56 | 区域边界点数据 |

## 系统模块

| 系统 | 状态 | 说明 |
|------|------|------|
| 核心框架 | ✅ | GameManager/TurnManager/GameState/EventBus |
| 地图系统 | ✅ | MapGraph/MapRenderer/MapSetup/CameraController |
| 区域面片 | ✅ | RegionMeshBuilder + map_region_shapes.json |
| 帝皇系统 | ✅ | EmperorMechanicSystem + 13 位帝皇机制 |
| 经济系统 | ✅ | EconomySystem/TaxSystem/PopulationSystem/LandSystem |
| 政治系统 | ✅ | LegitimacySystem/FactionSystem/ReformSystem/RebellionSystem |
| 军事系统 | ✅ | ArmyMovementSystem/BattleResolver/SiegeSystem |
| 科技系统 | ✅ | TechSystem + 40 项科技 |
| 天气系统 | ✅ | WeatherSystem + 6 种天气 |
| 风俗系统 | ✅ | CultureSystem + 6 种风俗 |
| 装备系统 | ✅ | EquipmentSystem + 10 种装备 |
| 天文系统 | ✅ | CelestialEventSystem + 5 种天文事件 |
| 将领系统 | ✅ | GeneralSystem + 12 位将领 |
| 建筑系统 | ✅ | BuildingSystem + 12 种建筑 |
| AI 系统 | ✅ | StrategicAI/PolicyAI/MilitaryAI |
| 事件系统 | ✅ | 200 个历史事件 + 27 个编年事件 |
| UI 系统 | ✅ | 历史文明主题 UI（UITheme/UISetup/各面板） |
| 存档系统 | ✅ | SaveManager JSON 存档 |

## 产品目标

10 到 12 周内完成一个可运行 Demo。Demo 需要证明以下闭环成立：

1. 帝皇机制会改变玩法路线。
2. 扩张会带来治理、财政、土地和继承压力。
3. 统一之后仍然存在王朝延续压力。

## MVP 范围

| 模块 | MVP 内容 |
| --- | --- |
| 地图 | 史域九州融合地图，目标 40 到 60 个区域 |
| 帝皇 | 秦始皇、刘邦、汉武帝、曹操、李世民、赵匡胤、朱元璋、康熙 |
| 核心资源 | 粮食、金钱、人口、兵力、合法性 |
| 核心风险 | 土地兼并、民变、财政崩溃、继承危机、地方割据 |
| 战争 | 大地图行军、自动战斗结算、战报 |
| 内政 | 税制、征兵、赈灾、清丈、招贤、改革 |
| 继承 | 皇帝年龄、继承人、死亡、继位、危机事件 |
| 胜利 | 统一九州、三代延续、制度胜利 |
| AI | 每个 AI 帝皇有不同扩张和治理倾向 |
| UI | 地图、帝皇面板、地区面板、朝廷面板、事件面板 |

## 12 周里程碑

| 周次 | 目标 | 验收标准 |
| --- | --- | --- |
| 1 | 设计定稿与工程搭建 | 能打开游戏，看到简陋地图，点击区域显示名称和基础数据 |
| 2 | 地图与区域系统 | 区域可显示人口、粮食、税收、归属、邻接，归属可变化 |
| 3 | 回合与经济系统 | 下一回合后，国家资源和地区资源会结算 |
| 4 | 帝皇系统与机制框架 | 至少 3 位帝皇机制产生不同开局效果 |
| 5 | 战争系统初版 | 可从相邻区域发起进攻，自动结算后归属可能变化 |
| 6 | 土地兼并与民变 | 高税和频繁征兵会推高兼并与叛乱风险 |
| 7 | 合法性与法统 | 新占领地区需要整合，不同帝皇整合方式不同 |
| 8 | 继承系统 | 皇帝可能死亡，继承危机会改变局势 |
| 9 | 人才系统 | 可获得并任命人才，人才影响战争、财政、改革或治理 |
| 10 | AI 初版 | AI 能扩张、治理、征兵和相互进攻 |
| 11 | 胜利条件与 UI 打磨 | 一局可从开局玩到胜利或失败，原因可理解 |
| 12 | 测试、平衡与演示 | 可演示 20 到 40 回合完整流程，至少 3 位帝皇差异明显 |

## 第一批执行任务

### Task 1: 项目规则与目录

**目标：** 建立项目长期约束，避免后续资产和数据散乱。

**文件：**
- 创建：`CLAUDE.md`
- 创建：`docs/mvp-design.md`
- 创建：`docs/data-contract.md`
- 创建：`docs/roadmap-12-weeks.md`

**验收：**
- 根目录有项目规则。
- 文档能解释 MVP 范围、数据结构和排期。

### Task 2: Unity 资产目录与数据种子

**目标：** 创建可被 Unity 工程直接使用的基础目录和 JSON 数据表。

**文件：**
- 创建：`WanChaoGuiYi/Assets/Data/emperors.json`
- 创建：`WanChaoGuiYi/Assets/Data/regions.json`
- 创建：`WanChaoGuiYi/Assets/Data/policies.json`
- 创建：`WanChaoGuiYi/Assets/Data/events.json`
- 创建：`WanChaoGuiYi/Assets/Data/talents.json`
- 创建：`WanChaoGuiYi/Assets/Data/units.json`
- 创建：`WanChaoGuiYi/Assets/Data/victory_conditions.json`

**验收：**
- JSON 全部可解析。
- 帝皇数据包含 8 位 MVP 帝皇。
- 地区数据包含土地结构、法统记忆和邻接关系。

### Task 3: Week 1 入口

**目标：** 后续进入 Unity 后先实现最小地图点击闭环。

**建议文件：**
- `WanChaoGuiYi/Assets/Scripts/Core/GameManager.cs`
- `WanChaoGuiYi/Assets/Scripts/Map/Region.cs`
- `WanChaoGuiYi/Assets/Scripts/Map/MapGraph.cs`
- `WanChaoGuiYi/Assets/Scripts/UI/RegionPanel.cs`

**验收：**
- 打开游戏后能看到简陋地图。
- 点击区域能显示区域名称、人口、粮食、税收、归属、邻接区域。

### Task 4: 代码大框架

**目标：** 建立 Unity 工程的稳定模块边界，让后续每周系统可以增量实现。

**已创建入口：**
- `GameManager`
- `TurnManager`
- `GameState`
- `GameContext`
- `EventBus`
- `DataRepository`
- Unity 项目壳：`Packages/manifest.json`、`ProjectSettings/ProjectVersion.txt`

**已创建系统：**
- 地图：`MapGraph`、`RegionController`、`MapRenderer`
- 帝皇：`EmperorMechanicSystem`、`SuccessionSystem`
- 经济：`EconomySystem`、`TaxSystem`、`PopulationSystem`、`LandSystem`
- 政治：`LegitimacySystem`、`FactionSystem`、`ReformSystem`、`RebellionSystem`
- 军事：`ArmyMovementSystem`、`BattleResolver`、`SiegeSystem`
- 人才：`TalentSystem`
- AI：`StrategicAI`、`PolicyAI`、`MilitaryAI`
- UI：地图、地区、帝皇、朝廷、事件、战报面板入口

**验收：**
- 代码文件已按模块落位。
- 数据表可解析。
- 帝皇偏好政策、区域邻接等关键引用检查通过。

### Task 5: Week 1 UI 与可玩性闭环

**目标：** 把 Debug.Log 占位全部替换为真实 UGUI 面板，实现地图自动布局和摄像机控制。

**已创建/更新：**
- `RegionPanel`：完整 UGUI 面板，显示地形、人口、粮食、税收、兵源、整合度、民变风险、土地兼并、地方势力、相邻区域、土地结构。
- `EmperorPanel`：完整 UGUI 面板，显示帝皇名称、称号、机制描述、六维属性、历史负担、合法性、继承风险、继承人信息。
- `CourtPanel`：完整 UGUI 面板，显示势力资源总览和最近 30 条回合日志。
- `EventPanel`：完整 UGUI 面板，支持事件选择和效果应用。
- `BattleReportPanel`：完整 UGUI 面板，显示战斗双方、战力和结果。
- `MainMapUI`：集成所有面板，监听 EventBus 事件，自动刷新 HUD（回合数、资源）。
- `MapSetup`：基于历史地理坐标自动摆放 17 个区域节点，支持 prefab 覆盖。
- `CameraController`：键盘/鼠标/边缘平移、滚轮缩放、边界限制。
- `VictorySystem`：每回合检查三种胜利条件（统一九州、三代延续、制度胜利）。
- 补全全部 8 位帝皇机制：秦始皇、刘邦、汉武帝、曹操、李世民、赵匡胤、朱元璋、康熙。

**验收：**
- 打开游戏后能看到 17 个区域节点按九州地理布局。
- 点击区域显示完整地区面板。
- 推进回合后 HUD 更新金钱、粮食、合法性、领地数。
- 点击朝廷按钮查看势力资源和回合日志。
- 点击帝皇按钮查看帝皇属性和机制。
- 8 位帝皇每回合产生不同被动效果。

## 当前风险

- Unity 版本尚未锁定，建议 Week 1 选择 LTS 版本。
- UI 已选定 UGUI（Text + Button + ScrollRect），如需更丰富交互可考虑 UI Toolkit 迁移。
- 目前地区数据只有 17 个，距离 40 到 60 个 MVP 区域还有缺口（Week 2 扩充）。
- 自动战斗公式、土地兼并公式和继承危机公式已有初版数值，需要进入 Unity 后平衡测试。
- UI 面板需要在 Unity 中实际创建 GameObject 并绑定 SerializeField 引用。

## 验证记录

- 2026-04-29：地图主导战争闭环重写 Phase 0 启动；当前工作区基线存在未提交变更：`GameManager.cs` 将 battle session 相关组件从回合系统改为服务初始化、`BattleSessionSystem.cs` 改用 `GameContext` 构造函数、`EquipmentSystem.cs` 移除重复 `EquipmentDefinition/EquipmentBonus` 类型；这些属于重写前编译修复基线。
- 2026-04-29：地图主导战争闭环 Phase 1 完成数据契约更新：新增 Map-Led War Runtime Contract，明确复用现有 JSON 字段、运行时状态、查询能力、写入规则和占领治理后果；本阶段未新增持久 JSON 字段，因此无需迁移数据表。
- 2026-04-29：地图主导战争闭环 Phase 2 首片完成：新增 `WorldState`、`MapState`、`RegionRuntimeState`、`ArmyRuntimeState`、`WarRuntimeState`、`EngagementRuntimeState`、`WorldStateFactory`、`MapQueryService`，并在 `GameManager.StartNewGame()` 中由现有 `GameState` 构建地图主导运行状态和查询服务。
- 2026-04-30：地图主导战争闭环 Ralph Iteration 3 完成：新增 Unity 内地图行军验收入口；`MapState.RemoveArmy()` 与 `MapWarResolutionSystem` 的败方撤退/溃散处理已落位，战后败军会撤往同势力相邻地区或从 runtime/legacy 军队列表移除，降低重复接敌风险；`ArmyMovementSystem` 允许 `ArmyTask.Retreat` 的接敌军队脱离并写日志，`MapCommandService` 为撤退和增援补充可解释日志；旧 `BattleResolver.Resolve()` 默认不再直接改地区归属，只有显式开启 `allowLegacyOwnershipChange` 时才保留 legacy 改归属路径。
- 2026-04-30：地图主导战争闭环 Ralph Iteration 2 完成：`GameStateFactory.CreateDefault()` 现在创建稳定的初始玩家军队和非玩家军队，并优先放在玩家首个区域与敌方相邻区域；新增 `MapWarResolutionSystem` 并在 `GameManager.CollectSystems()` 中按 `ArmyMovementSystem -> MapWarResolutionSystem -> SiegeSystem -> EconomySystem` 顺序执行，使接敌、战斗、占领和新占领税粮折损进入同一回合闭环；`EconomySystem` 在存在 `GameManager.World.Map` 时按 `RegionRuntimeState.taxContributionPercent/foodContributionPercent` 折算收入，找不到 runtime region 时保留 legacy 计算路径。
- 2026-04-30：`python3 tools/validate_data.py` 通过；静态检查确认 `MapWarResolutionSystem` 已注册、`EconomySystem` 读取 runtime 贡献倍率、`BattleSimulationSystem` 不调用 `ChangeRegionOwner`。当前环境仍未发现 Unity/C# 编译器，无法完成 Console 编译与 PlayMode 验收；进入 Unity 后需优先验证单路攻击闭环和经济日志数值。
- 2026-04-30：地图主导战争闭环 Ralph Iteration 1 完成路线行军、接敌、战斗、占领、治理反馈首片：新增 `MapCommandService`、`EngagementDetector`、`BattleSimulationSystem`、`OccupationSystem`、`GovernanceImpactSystem`，扩展战争事件 `ArmyMoveStarted`、`ArmyArrived`、`ContactDetected`、`EngagementStarted`、`RegionOccupied`、`GovernanceImpactApplied`。当前实现仍需 Unity 编译验证与场景内单路/多军闭环手动验收。
- 2026-04-30：`python3 tools/validate_data.py` 通过，结果：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- 2026-04-29：`python3 tools/validate_data.py` 通过，结果：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- 2026-04-28：`python3 -m json.tool` 验证 7 个 JSON 数据表全部可解析。
- 2026-04-28：临时数据引用检查发现 30 个帝皇偏好政策缺表，已补齐到 `policies.json`。
- 2026-04-28：`python3 tools/validate_data.py` 通过，结果：`emperors=8 regions=17 policies=35`；该脚本检查唯一 id、帝皇全球扩展字段、偏好政策引用、区域邻接存在性和区域邻接双向性。
- 2026-04-28：本机未发现 `dotnet`、`mcs`、`csc` 或 Unity 安装，暂无法在当前环境做 C# 编译验证；进入 Unity 后第一件事是跑 Console 编译检查。
- 2026-04-28：UI 面板全部从 Debug.Log 升级为 UGUI 组件，支持 Text、Button、ScrollRect。
- 2026-04-28：MapSetup 自动布局 17 个区域节点，坐标参照九州历史地理方位。
- 2026-04-28：8 位帝皇机制全部实现（秦始皇、刘邦、汉武帝、曹操、李世民、赵匡胤、朱元璋、康熙）。
- 2026-04-28：VictorySystem 实现三种胜利条件检查（统一九州、三代延续、制度胜利）。

### Task 6: 历史深度系统（借鉴文明 6）

**目标：** 为游戏注入历史纵深感，让玩家感受到技术进步、天气变化、风俗差异、装备升级和天文事件。

**新增系统：**
- `TechSystem`：14 项科技/制度树，自动研究，Boost 启发机制，解锁单位/政策/装备。
- `WeatherSystem`：6 种天气状态（正常、旱灾、水灾、丰年、寒潮、瘟疫），季节修正，灾害概率。
- `CultureSystem`：6 种风俗类型（尚武、崇文、重商、农耕、边塞、多元），从历史层数据自动推导，影响兵源质量/税收/民变。
- `EquipmentSystem`：10 种装备（青铜戈→铁戟→火铳），3 槽位（武器/护甲/特殊），科技解锁，战力百分比加成。
- `CelestialEventSystem`：5 种天文事件（日食、彗星、五星聚、月食、客星），影响合法性，司天监科技降低负面效果。

**数据基础：**
- `technologies.json`：14 项科技，含前置、Boost、解锁、效果。
- `historical_layers.json`：17 个区域的历史地理、风俗、兵器传统、战略资源、季节特征、灾害权重。
- `chronicle_events.json`：11 个编年事件（天气、天文、风俗、兵器、基建、科技）。

**与现有系统集成：**
- `GameState`：新增 weatherId、celestialEventId、researchPoints、completedTechIds、customs、customStability、equipment slots。
- `GameManager`：注册全部 5 个新系统到回合调度链。
- `BattleResolver`：读取装备加成计算战力。
- `GameStateFactory`：从历史层数据推导初始风俗。

**验收：**
- 每回合自动推进科技研究，完成后解锁装备和政策。
- 天气每回合变化，影响粮食产出和军事行动。
- 区域风俗自动推导，影响兵源质量和税收。
- 装备科技解锁后可装备军队，提升战力。
- 天文事件随机触发，影响合法性和朝局。

### Task 7: 动态版图颜色层

**目标：** 高精度地形图保持为底图，势力范围使用独立覆盖层随地区归属变化而变色。

**已创建/更新：**
- `GameEventType.RegionOwnerChanged`：地区归属变化事件。
- `RegionOwnerChangedPayload`：记录地区、旧归属、新归属。
- `GameState.ChangeRegionOwner`：统一更新 `RegionState.ownerFactionId` 和双方 `FactionState.regionIds`。
- `GameContext.ChangeRegionOwner`：归属变化的推荐入口，负责发事件和写日志。
- `MapRenderer`：订阅 `RegionOwnerChanged`，只刷新变化地区，并使用 8 位核心帝皇的固定势力色。
- `BattleResolver`：进攻方胜利后触发地区归属变化。
- `MapSetup`：自动查找 `MapRenderer`，保证自动生成节点后能刷新颜色。

**验收：**
- 版图颜色变化机制已完成代码链路：战斗胜利 -> 地区归属变更 -> 发布事件 -> 地图局部刷新。
- 当前环境缺 Unity/C# 编译器，尚未做 Unity Console 编译验证。
- 2026-04-28：`python3 tools/validate_data.py` 通过，结果：`emperors=13 portraits=8 regions=56 historical_layers=56 policies=35 units=8 technologies=40 chronicle_events=200`，并提示 5 位新增帝皇尚缺立绘。
- 2026-04-28：全部 `Assets/Data/*.json` 通过 `python3 -m json.tool` 解析验证。

### Task 8: 地图实际建模第一版

**目标：** 把地图从“原画底图 + 节点”升级为可点击、可染色、可精修的 2D 地区面片模型。

**已创建/更新：**
- `map_region_shapes.json`：56 个地区全部拥有中心点、标签偏移、渲染顺序和边界点。
- `MapRegionShapeTable`、`MapRegionShapeDefinition`、`MapPoint`：Unity 可读取的地图面片数据模型。
- `DataRepository`：读取并索引 `map_region_shapes.json`。
- `RegionMeshBuilder`：根据边界点生成 Unity `Mesh`，支持后续替换为更精细边界。
- `MapSetup`：使用地区面片生成 `MeshRenderer`、`MeshCollider`、`RegionController`；旧节点仅保留为显式调试 fallback。
- `MapRenderer`：同时支持 `SpriteRenderer` 和 `MeshRenderer` 染色。
- `docs/data-contract.md` 与 `docs/architecture.md`：补充地图建模规则。
- `DataRepository`：新增按 `regionId` 索引地图面片，避免运行时遍历和错把 shape id 当地区 id。
- `MapSetup`：默认把地区面片作为主地图唯一方案；旧节点 fallback 需要显式开启 `allowNodeFallback`，缺面片时记录错误。
- `MapSetup`：重建地图前清空 `MapRenderer` 注册表，避免残留旧 controller 影响归属刷新。
- `MapSetup`：标签改为中心锚点，并把标签排序层级放到面片之上。
- `tools/validate_data.py`：新增地图面片中心点、标签点、边界点、渲染层级和区域覆盖验证。

**验收：**
- 2026-04-29：`python3 tools/validate_data.py` 通过，结果：`emperors=13 portraits=8 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 chronicle_events=200`。
- 2026-04-29：全部 `Assets/Data/*.json` 通过 `python3 -m json.tool` 解析验证。
- 2026-04-29：确认 56 个 `regions.json` 地区均有可用面片；所有面片中心点和标签点均在边界内，可作为最小点击命中与标签定位假说。
- 2026-04-29：确认归属变化链路仍为 `BattleResolver -> GameContext.ChangeRegionOwner -> RegionOwnerChanged -> MapRenderer.RefreshRegion`。
- 2026-04-29：当前本机未发现 `dotnet`、`mcs`、`csc` 或 Unity 安装，仍需进入 Unity Console 做 C# 编译与实际点击验收。
- 当前 `map_region_shapes.json` 是 `playable_blockout_v1`，满足交互和染色，不等于最终高精度边界；下一步应在 Unity/矢量工具中按地形底图精修边界点。

### Task 9: 视觉资产第一批

**目标：** 补齐 MVP 数据可引用的基础视觉资产，让帝皇、兵种、科技、资源、文化和风险系统有可放入 UI 的 PNG。

**已创建/更新：**
- `Assets/Art/Portraits/`：新增 13 位帝皇立绘，覆盖 8 位 MVP 帝皇与 5 位扩展帝皇。
- `Assets/Art/Icons/Units/`：新增 8 个兵种图标。
- `Assets/Art/Icons/Systems/`：新增 16 个资源、文化和风险图标。
- `Assets/Art/Icons/Technologies/`：新增 40 个科技/制度图标。
- `Assets/Art/GeneratedSheets/`：保留 10 张生成母版，方便后续重新裁切或人工精修。
- `portraits.json`：补齐 13 位帝皇的本地立绘状态，其中 5 位扩展帝皇新增 `assetPath`、视觉身份和生成 prompt。

**验收：**
- 2026-04-29：`python3 tools/validate_data.py` 通过，结果：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- 2026-04-29：`portraits.json` 解析通过。
- 2026-04-29：本地文件计数通过：单张资产 77 个，其中立绘 13、兵种 8、系统 16、科技 40；生成母版 10 张。
- 2026-04-29：已抽查立绘母版和科技图标母版；当前资产属于 concept_v1，后续进入 Unity UI 后再按实际尺寸、裁切和辨识度精修。

## 2026-04-30 Headless Scenario Suite 记录

### 目标

把 headless 验收从单路 smoke test 扩展为小型确定性场景套件。用户价值是：后续每次改地图战争逻辑，都能先在命令行确认“防守胜利撤退、进攻胜利占领、增援加入接敌”三类核心语义没有断链，再进入 Unity 验证。

### 已完成

- `HeadlessSimulationRunner` 新增 `RunAllScenarios()` 与 `HeadlessSimulationSuiteResult`。
- `HeadlessSimulationResult` 新增 `scenarioName`，CLI 现在逐场景输出 `scenario`、`scenarioPassed`、`turnsExecuted` 和失败原因。
- 当前默认运行 3 个场景：
  - `defender_holds_and_attacker_retreats`：验证防守方获胜后进攻军撤退。
  - `attacker_wins_and_occupies`：验证进攻方获胜、地区归属变化、新占领治理折损、败军溃散和经济结算。
  - `reinforcement_joins_existing_engagement`：验证增援抵达后加入既有 engagement，接敌成员从 `1 对 1` 变为 `2 对 1`，且不会重复记录未变化的接敌。
- `Program.cs` 改为调用 `runner.RunAllScenarios(repository, playerFactionId)`，任一场景失败即返回非 0。
- 新增 `tools/verify_headless_war.sh`，统一执行：
  - `python3 tools/validate_data.py`
  - `python3 tools/validate_domain_core.py`
  - `tools/run_headless_simulation.sh`

### 验证

- `python3 tools/validate_domain_core.py && tools/run_headless_simulation.sh` 通过。
- `tools/run_headless_simulation.sh` 输出 `passed=True`、`scenarioCount=3`。
- 三个场景全部 `scenarioPassed=True`。
- 日志语义确认：
  - 防守胜利路径：`防守方获胜` 后 `army_player_1战败后撤退至guanzhong`。
  - 进攻占领路径：`进攻方获胜` 后 `longxi归属变更`、`longxi被faction_qin_shi_huang占领`、`longxi新占领`。
  - 增援路径：先 `longxi发生接敌：1 支部队对 1 支部队`，增援抵达后 `longxi发生接敌：2 支部队对 1 支部队`。

### 限制与下一步

- 当前仍是 headless/Domain 验证，Unity Console 和 PlayMode 尚未验证。
- 场景仍属于确定性 smoke suite，不是完整测试框架；下一步可以继续把断言拆得更细，或进入增援/撤退玩法语义扩展。

## 2026-04-30 Headless Scenario Suite Iteration 2 记录

### 目标

把地图战争 headless 验收从“日志存在”推进到“状态一致”。用户价值是：后续改战争、治理或经济数值时，CLI 不只告诉我们发生了战斗，还能确认归属、占领治理折损、撤退脱战和经济贡献倍率真的落到状态里。

### 已完成

- `HeadlessSimulationRunner.RunAllScenarios()` 默认场景数从 3 个扩展为 4 个：
  - 新增 `active_retreat_leaves_engagement`：验证已接敌军队可以发出 `RetreatArmy()`，在行军阶段脱离 engagement 并回到撤退目标区域。
- 强化 `reinforcement_joins_existing_engagement`：
  - 断言 `ReinforceArmy()` 设置 `ArmyTask.Reinforce`。
  - 断言 `targetRegionId` 和路线终点保留为目标接敌区域。
  - 断言增援意图日志包含“增援”和“加入当地接敌”。
  - 保留抵达后 attacker membership 从 `1` 增至 `2` 的断言。
- 强化 `attacker_wins_and_occupies`：
  - 断言 runtime region owner 改为进攻方。
  - 断言 runtime `occupationStatus = Occupied`。
  - 断言 `integration = 25`，`taxContributionPercent = 35`，`foodContributionPercent = 35`。
  - 断言 legacy `RegionState` 镜像 owner / integration / rebellionRisk / localPower / annexationPressure。
  - 断言旧 owner faction 不再持有该 region，新 owner faction 已持有该 region。
  - 断言占领后有效税粮贡献低于基础税粮贡献，确认 runtime 贡献倍率进入经济闭环。
- `DomainArmyMovementSystem` 在撤退军队移动后同步从 engagement attacker/defender 列表移除该军队；若任一方为空，则清理 engagement，并把 contested region 复原为 controlled，避免撤退后残留空 engagement 被战斗系统误结算。

### 验证

- `tools/verify_headless_war.sh` 通过。
- 输出：`passed=True`、`scenarioCount=4`。
- 四个场景全部 `scenarioPassed=True`：
  - `defender_holds_and_attacker_retreats`
  - `attacker_wins_and_occupies`
  - `reinforcement_joins_existing_engagement`
  - `active_retreat_leaves_engagement`

### 限制与下一步

- 仍属于 CLI/headless 验证；Unity Console、PlayMode 和场景内按钮/地图交互尚未验证。
- 下一步建议把这 4 个场景拆为更接近测试框架的独立断言输出，或进入 Unity 手动验收 `StartPlayerAttack(...)` 与 UI 日志/地图颜色同步。

## 2026-04-30 Headless CLI Harness 记录

### 目标

把上一步的 `HeadlessSimulationRunner` 接到真正的命令行入口上：从 `WanChaoGuiYi/Assets/Data/*.json` 加载数据，创建非 Unity 数据仓库，然后直接调用 `RunSingleLaneWar()`。这一步的用户价值是让战争闭环未来能变成一条稳定命令，而不是每次都进 Unity 手动复现。

### 已完成

- 新增 `tools/headless_runner/WanChaoGuiYiHeadless/NonUnityJsonDataRepository.cs`
  - 实现 `IDataRepository`。
  - 使用 `System.Text.Json` 解析 JSON，并启用 `IncludeFields`，适配当前 Unity 风格 public field 数据模型。
  - 加载 headless 模拟需要的数据表：帝皇、地区、历史层、政策、事件、人才、兵种、科技、胜利条件、将领、建筑。
  - 对缺表、空 items、缺 id、重复 id 做明确异常。
- 新增 `tools/headless_runner/WanChaoGuiYiHeadless/WanChaoGuiYiHeadless.csproj`
  - .NET 8 console project。
  - 显式链接 Domain/Core/World/Map/Military/Governance/Economy 所需源码。
  - 不链接 Unity adapter 文件，避免 `MonoBehaviour`、`TextAsset`、`UnityEngine` 进入 headless 工程。
- 新增 `tools/headless_runner/WanChaoGuiYiHeadless/HeadlessBattleTypes.cs`
  - 提供非 Unity 版本 `BattleResult` 与 `EquipmentLookup`，避免 CLI harness 为这两个类型依赖 `BattleResolver : MonoBehaviour`。
- 新增 `tools/headless_runner/WanChaoGuiYiHeadless/Program.cs`
  - 默认数据目录：`WanChaoGuiYi/Assets/Data`。
  - 默认玩家势力：`faction_qin_shi_huang`。
  - 调用 `NonUnityJsonDataRepository.Load()` 后执行 `HeadlessSimulationRunner.RunSingleLaneWar()`。
  - 输出 `passed`、`turnsExecuted` 与日志。
  - 成功返回 0，runner 失败返回 1，异常返回 2。
- 新增 `tools/run_headless_simulation.sh`
  - 用法：`tools/run_headless_simulation.sh [data-dir] [player-faction-id]`。
  - 缺少 `dotnet` 时返回 127，并提示安装 .NET SDK 8+。

### 验证

- `python3 tools/validate_data.py` 通过。
- `python3 tools/validate_domain_core.py` 通过。
- 2026-04-30 环境修复：通过 Homebrew 清华镜像安装 .NET SDK `10.0.107`。
- 真实运行 `tools/run_headless_simulation.sh` 通过，输出 `passed=True`、`turnsExecuted=1`。
- 本次 headless 日志确认闭环：初始军队部署 → 行军 → 抵达 → 接敌 → 战斗结束 → 占领 → 新占领治理折损 → 败军溃散 → 经济收入。
- 真实编译修复：`DataModels.cs` 补 `System.Collections.Generic`；`MapGraphData.GetNeighbors()` 返回类型改为 `IEnumerable<string>`；`WanChaoGuiYiHeadless.csproj` 目标框架改为 `net10.0` 以匹配当前 SDK。
- 静态验证确认 CLI 调用：`runner.RunSingleLaneWar(repository, playerFactionId)`。
- 静态验证确认 headless csproj 不包含 Unity adapter 文件：`DataRepository.cs`、`MapGraph.cs`、`BattleResolver.cs`、`EconomySystem.cs`、`ArmyMovementSystem.cs`。
- 静态验证确认 headless csproj 的 26 个 `<Compile Include=...>` 链接文件全部存在。
- 静态 grep 确认 `tools/headless_runner` 与 `WanChaoGuiYi/Assets/Scripts/Domain` 无 `UnityEngine`、`MonoBehaviour`、`SerializeField`、`GetComponent`、`gameObject`、`Mathf.`。
- 注意：当前验证使用 .NET SDK `10.0.107`；若团队/CI 固定 .NET 8 LTS，需要另装 .NET 8 SDK 或改回 `net8.0` 后重新验证。

### 限制与下一步

- 当前工作环境没有 `dotnet`、`csc`、`mcs`，因此 CLI harness 尚未真实编译和执行。
- 下一步在安装 .NET SDK 8+ 后运行：

```bash
tools/run_headless_simulation.sh
```

- 如果编译失败，优先修复 headless csproj 缺失类型或 Unity 泄漏。
- 如果编译通过但 `passed=false`，根据输出的 `failureReason` 和 turnLog 修复战争闭环实际逻辑。


### 目标

把“地图主导战争闭环”的验收再往前推进一步：不用依赖 Unity PlayMode 手动点按钮，而是在代码层提供一个可组合的 headless runner。它的用户价值是把战争闭环从“只能进 Unity 看现象”变成“未来可命令行 pass/fail”，后续每次改战争、经济、治理都能更快发现断链。

### 已完成

- 新增 `IDataRepository`，让 `GameContext`、`GameStateFactory`、`WorldStateFactory` 可以接收非 Unity 数据仓库。
- `DataRepository` 实现 `IDataRepository`，保留现有 Unity JSON 加载路径。
- 新增 `IMapGraphData`，把地图邻接查询抽成接口。
- `MapGraph` 和 `MapGraphData` 均实现 `IMapGraphData`：
  - Unity 运行时继续使用 `MapGraph`。
  - headless runner 使用 `MapGraphData` 从 `RegionDefinition.neighbors` 读取邻接。
- `MapQueryService` 改为只依赖 `IMapGraphData`，Domain 不再直接引用 Unity `MapGraph` 具体类型。
- 新增 `HeadlessSimulationRunner.RunSingleLaneWar(IDataRepository data, string playerFactionId)`：
  - `GameStateFactory.CreateDefault()` 创建新局和初始军队。
  - `WorldStateFactory.Create(...)` 构建 runtime world。
  - `MapCommandService.MoveArmy(...)` 发起 `army_player_1` 攻向 `army_enemy_1` 所在区域。
  - 连续执行 `DomainArmyMovementSystem -> DomainMapWarResolutionSystem -> DomainEconomySystem`。
  - 断言日志出现：`行军`、`抵达`、`接敌`、`战斗结束`、`占领/防守`、`新占领` 治理折损、`收入 金钱` 经济结算。
- `NumericSystem` 移除 `using UnityEngine` 和 `Mathf.*`，改用 `DomainMath.*`，减少 Domain 战斗/经济公式的 Unity 依赖。
- `DomainMath.RoundToInt()` 改用 `MidpointRounding.AwayFromZero`，尽量贴近 Unity `Mathf.RoundToInt` 中点取整行为。
- `tools/validate_domain_core.py` 增加对 Domain 直接引用 `MapGraph` 具体类型的检查。

### 验证

- `python3 tools/validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python3 tools/validate_domain_core.py` 通过。
- 静态 grep 确认 `WanChaoGuiYi/Assets/Scripts/Domain` 无 `using UnityEngine`、`MonoBehaviour`、`SerializeField`、`GetComponent`、`gameObject`、`Mathf.`、直接 `MapGraph` 依赖。

### 限制与下一步

- 当前环境仍没有 Unity/C# 编译器，也没有独立 C# test harness；因此 `HeadlessSimulationRunner` 已落位，但还不能在本机实际执行并产出 pass/fail。
- 下一步最直接路径：实现一个非 Unity JSON repository / CLI harness，从 `WanChaoGuiYi/Assets/Data/*.json` 读取数据，直接调用 `HeadlessSimulationRunner.RunSingleLaneWar()`，把结果输出为命令行 smoke test。


### 目标

解决当前环境没有 Unity/C# 编译器导致核心玩法难以自动验收的问题。长期方向是把玩法核心拆成不依赖 Unity 的 Domain Core，Unity 层只保留 `MonoBehaviour` adapter。这样后续可以发展 headless simulation：不用进入 Unity，也能验证行军、接敌、战斗、占领、治理和经济结算。

### 已完成

- 建立 `WanChaoGuiYi/Assets/Scripts/Domain/` 边界，包含 `Core`、`World`、`Map`、`Military`、`Governance`、`Economy`。
- 新增 `DomainMath`，Domain 系统使用 `System.Math` 包装，不直接依赖 Unity `Mathf`。
- 将核心状态和地图服务移入 Domain：`IGameSystem`、`WorldState`、`WorldStateFactory`、`MapQueryService`、`MapCommandService`。
- 抽离地图战争相关 Domain 系统：
  - `DomainEngagementDetector`
  - `DomainBattleSimulationSystem`
  - `DomainOccupationSystem`
  - `DomainGovernanceImpactSystem`
  - `DomainMapWarResolutionSystem`
- 抽离回合推进相关 Domain 系统：
  - `DomainArmyMovementSystem`
  - `DomainEconomySystem`
- 对应 Unity 文件已瘦身为 adapter：
  - `EngagementDetector`
  - `BattleSimulationSystem`
  - `OccupationSystem`
  - `GovernanceImpactSystem`
  - `MapWarResolutionSystem`
  - `ArmyMovementSystem`
  - `EconomySystem`
- 新增 `tools/validate_domain_core.py`，用于在无 Unity 编译器环境检查 Domain 边界。

### 验证

- `python3 tools/validate_data.py` 通过。
- `python3 tools/validate_domain_core.py` 通过。
- 静态 grep 确认 `WanChaoGuiYi/Assets/Scripts/Domain` 下无 `using UnityEngine`、`MonoBehaviour`、`SerializeField`、`GetComponent`、`gameObject`、`Mathf.`。

### 限制与下一步

- 当前仍未验证 Unity Console 编译、PlayMode 和场景绑定。Unity 中必须重新导入并检查移动过的脚本文件是否保留/重建了可接受的 asset 引用。
- 下一步建议补一个 headless simulation runner 或 Domain smoke test：从 `GameStateFactory.CreateDefault()` 创建新局，构建 `WorldState`，调用 `MapCommandService.MoveArmy()`，连续执行 `DomainArmyMovementSystem`、`DomainMapWarResolutionSystem`、`DomainEconomySystem`，断言出现接敌、战斗、占领、治理折损和经济日志。

## 2026-04-30 Headless 战争闭环修复

### 已完成

- 统一 headless 默认玩家势力 id 为 `faction_qin_shi_huang`。
- `GameStateFactory.CreateDefault()` 对未知玩家势力 id 直接抛出 `InvalidOperationException`，取消静默 fallback 到首个势力。
- `GameState.ChangeRegionOwner()` 只负责归属和 `FactionState.regionIds` 维护；占领后的整合度、民变、地方势力、兼并压力继续由 `DomainGovernanceImpactSystem` 处理。
- `DomainEngagementDetector` 只标记 `engagementId`，不再把防守军或增援军强行改为 `ArmyTask.Attack`。
- `DomainMapWarResolutionSystem` 将战后地区状态恢复拆成独立方法，并在战斗结束后统一清理参战军队的 `targetRegionId`、`route`、`task` 和 `engagementId`。
- `MapCommandService` 构造参数移除未使用的 `WorldState`。
- `HeadlessSimulationRunner` 在经济结算前后抓取势力 `money/food` 快照，并断言实际资源差值等于公式期望值。

### 验证

- `python3 tools/validate_data.py` 通过。
- `python3 tools/validate_domain_core.py` 通过。
- `tools/verify_headless_war.sh` 通过，4 个场景全部 `scenarioPassed=True`。
- `tools/run_headless_simulation.sh WanChaoGuiYi/Assets/Data faction_missing` 返回退出码 `2`，错误信息包含 `Unknown player faction id: faction_missing`，确认无声 fallback 已取消。

## 2026-04-30 Headless 战争闭环继续修复

### 已完成

- 新增 `DomainEngagementCleanup`，统一处理接敌成员移除、空边接敌清理、战后参战军队清理和 contested 地区恢复。
- `DomainArmyMovementSystem` 主动撤退路径复用统一接敌清理逻辑，减少移动系统内的战后状态职责。
- `DomainMapWarResolutionSystem` 战斗结算后改为调用统一清理 helper。
- `DomainEngagementDetector` 修正既有接敌的增援分边：新到达部队优先按既有双方阵营归类，避免敌方增援被 initiating army 误分到进攻方。
- `HeadlessSimulationRunner` 新增 `defender_reinforcement_joins_existing_engagement` 场景，并断言防守军在接敌和增援过程中保持 `ArmyTask.Idle`。
- `ArmyMovementSystem`、`MapWarResolutionSystem`、`EconomySystem` Unity adapter 增加 `WorldState` 绑定检查，避免二次 `StartNewGame()` 后继续使用旧 Domain 实例。
- `GameStateFactory.CreateDefault()` 在创建 factions 后立即校验玩家势力 id，进一步前置未知 id 报错。

### 验证

- `python3 tools/validate_data.py` 通过。
- `python3 tools/validate_domain_core.py` 通过。
- `tools/verify_headless_war.sh` 通过，5 个场景全部 `scenarioPassed=True`。
- `tools/run_headless_simulation.sh WanChaoGuiYi/Assets/Data faction_missing` 返回退出码 `2`，错误信息包含 `Unknown player faction id: faction_missing`。

## 2026-04-30 Unity PlayMode 验证入口补齐

### 已完成

- 新增 `com.unity.test-framework` 依赖，为 Unity Test Runner 提供 PlayMode 测试能力。
- 新增 `WanChaoGuiYi.PlayModeTests` asmdef。
- 新增 `GameManagerPlayModeSmokeTests`：
  - 在 PlayMode 中动态创建 `GameManager`。
  - 断言 `DataRepository`、`GameState`、`WorldState`、`MapQueryService`、`MapCommandService` 完成 bootstrap。
  - 通过 `StartPlayerAttack(enemyArmy.locationRegionId)` 和 `NextTurn()`，断言出现 `战斗结束` 与 `收入 金钱`。
  - 二次调用 `StartNewGame()`，断言 `WorldState` 已替换，并再次跑战争烟测，用于覆盖 Unity adapter 重绑逻辑。
- 新增 `tools/unity/run_playmode_tests.sh`，统一以 batchmode 运行 PlayMode tests，支持通过 `UNITY_BIN` 指定 Unity Editor。

### 验证

- `python3 -m json.tool` 验证 `manifest.json` 与 PlayMode asmdef JSON 可解析。
- `tools/unity/run_playmode_tests.sh` 已执行，当前本机返回 `127`：`Unity editor executable not found. Set UNITY_BIN=/path/to/Unity or install Unity 2022.3.0f1.`。
- `python3 tools/validate_data.py` 通过。
- `python3 tools/validate_domain_core.py` 通过。
- `tools/verify_headless_war.sh` 通过，5 个场景全部 `scenarioPassed=True`。

### 限制与下一步

- 当前机器未安装 Unity Editor，PlayMode 测试入口已补，但实际 PlayMode 结果仍未完成。
- 当前仓库没有 `.unity` 场景文件，因此还没有可验证的保存场景绑定；新增 PlayMode 测试验证的是运行时 bootstrap 与二次开局重绑，不等同于实际 Demo 场景序列化绑定。
- 下一步需要在装有 Unity 2022.3.0f1 的环境运行：`UNITY_BIN=/path/to/Unity tools/unity/run_playmode_tests.sh`。

## 2026-04-30 第二轮审查问题修复

### 已完成

- `DomainMapWarResolutionSystem` 不再结算本回合刚形成的接敌，让玩家/AI 在下一回合前拥有增援或主动撤退窗口。
- `HeadlessSimulationRunner` 的战争、增援、主动撤退场景改成两回合语义，和 `TurnManager` 的真实系统顺序一致。
- 新增 headless 场景：
  - `engaged_army_rejects_non_retreat_commands`
  - `same_faction_contact_does_not_create_engagement`
- `DomainEngagementDetector` 先确认存在敌对双方再注册 engagement，避免同势力多军同区留下无效接敌。
- `MapCommandService` 拒绝已接敌军队的普通移动、增援、围攻和停止命令，只允许撤退。
- `RegionState` 增加 `occupationStatus`、`taxContributionPercent`、`foodContributionPercent`，占领治理结果可通过 `GameState` 保存并重建到 `WorldState`。
- `HeadlessSimulationRunner` 增加占领状态重建断言，确认新占领地区在重建 `WorldState` 后仍保持 `Occupied` 和 35% 税粮贡献。
- `tools/run_headless_simulation.sh` 支持 `net8.0`/`net10.0` 双目标，按本机 runtime 自动选择。
- 新增 `tools/unity/preflight_without_unity.py`，在无 Unity Editor 机器上检查数据表、地图面片、asmdef、包依赖和 PlayMode 入口。
- 新增 `docs/unity-handoff-checklist.md`，记录另一台 Unity 机器的验收命令和期望。
- PlayMode smoke 断言更新为第一回合只接敌、第二回合结算战斗。

### 验证

- `python3 tools/validate_data.py` 通过。
- `python3 tools/validate_domain_core.py` 通过。
- `python3 tools/unity/preflight_without_unity.py` 通过。
- `tools/run_headless_simulation.sh WanChaoGuiYi/Assets/Data faction_qin_shi_huang` 通过，7 个场景全部 `scenarioPassed=True`。
- `git diff --check` 通过。

### 限制与下一步

- 当前机器仍未安装 Unity Editor，真实 PlayMode 和场景序列化绑定需在另一台 Unity 机器运行 `tools/unity/run_playmode_tests.sh`。

## 2026-04-30 地图战争四场景最小全链路升级

### 目标

- 将 headless 地图战争验收收束为 4 个核心场景：
  - `defender_holds_and_attacker_retreats`
  - `attacker_wins_and_occupies`
  - `reinforcement_joins_existing_engagement`
  - `active_retreat_leaves_engagement`
- 命令行证明战争命令、行军、接敌、战斗、占领/防守、撤退/溃散、治理折损和经济结算形成连续链路。
- 当前便携电脑完成 Unity Editor 之前的 Domain、headless、报告和迁移准备工作。

### 已完成

- `HeadlessSimulationRunner.RunAllScenarios()` 对外只运行 4 个验收场景，输出 `scenarioCount=4`。
- 新增结构化报告模型：
  - `HeadlessWarReport`
  - `HeadlessScenarioReport`
  - `HeadlessPhaseResult`
  - `HeadlessAssertionResult`
  - `HeadlessKeyDelta`
  - `HeadlessExplanation`
- `Program.cs` 现在输出人读摘要，并固定写入机器报告：`tools/headless_runner/latest-war-report.json`。
- `tools/verify_headless_war.sh` 在运行后校验 JSON：`runName=headless_war_four_scenarios`、`passed=true`、`scenarioCount=4`、`passedCount=4`、`failedCount=0`。
- 四场景 JSON 覆盖关键阶段：
  - `command`
  - `movement`
  - `engagement`
  - `battle`
  - `outcome`
  - `governance`
  - `economy`
- `attacker_wins_and_occupies` 明确断言：
  - `economy.money_delta_matches_runtime_contribution`
  - `economy.food_delta_matches_runtime_contribution`
- `reinforcement_joins_existing_engagement` 明确断言增援前后 attacker membership 变化，并记录增援前后进攻方战力。
- `active_retreat_leaves_engagement` 明确断言撤退后无残留 engagement，且后续 war resolution 不触发战斗。
- 战争关键日志保留原关键词，并补充原因和影响说明，方便玩家理解。

### 验证

- `tools/verify_headless_war.sh WanChaoGuiYi/Assets/Data faction_qin_shi_huang` 通过：
  - `passed=True`
  - `scenarioCount=4`
  - 四个场景全部 `[PASS]`
- `python3 tools/unity/preflight_without_unity.py` 通过。
- 额外脚本检查 4 个场景的必需 assertion 均存在且 `passed=true`。
- `git diff --check` 通过。

### Unity 迁移下一步

- 在另一台 Unity 机器运行：

```bash
tools/verify_headless_war.sh WanChaoGuiYi/Assets/Data faction_qin_shi_huang
tools/unity/run_playmode_tests.sh
```

- Unity 验证时优先对照 `tools/headless_runner/latest-war-report.json`，确认场景内 UI/日志呈现与 headless 断言一致。

## 2026-04-30 Review Findings 修复

### 已完成

- 修复 headless assertion 假阳性风险：
  - `HeadlessSimulationRunner` 在 `Pass()` 与 `FinalizeScenarioReport()` 中扫描失败 assertion。
  - 任一 assertion 失败会把 scenario 和 suite 标为失败，并记录失败阶段与原因。
  - `tools/verify_headless_war.sh` 现在检查所有 `assertions[].passed`，并逐场景校验必需 assertion 是否存在且通过。
- 修复未接敌军队可执行撤退的问题：
  - `MapCommandService.RetreatArmy()` 要求军队已有 `engagementId`。
  - `active_retreat_leaves_engagement` 新增 `command.unengaged_retreat_rejected` 断言。
- 清理 volatile 报告产物：
  - `tools/headless_runner/latest-war-report.json` 保持运行时固定生成路径，但从版本跟踪中移除并加入 `.gitignore`。
  - 避免每次验证因时间戳和随机 engagement id 让工作区变脏。

### 验证

- `tools/verify_headless_war.sh WanChaoGuiYi/Assets/Data faction_qin_shi_huang` 通过：
  - `passed=True`
  - `scenarioCount=4`
  - 四个场景全部 `[PASS]`
  - `command.unengaged_retreat_rejected` 存在且通过。
- `python3 tools/unity/preflight_without_unity.py` 通过。
- `git diff --check` 通过。

## 2026-04-30 Unity 交接清单加固与 Demo 骨架

### 已完成

- 更新 `docs/unity-handoff-checklist.md`：
  - 将 Unity 前置验收命令从单场景 `tools/run_headless_simulation.sh` 升级为四场景门禁 `tools/verify_headless_war.sh WanChaoGuiYi/Assets/Data faction_qin_shi_huang`。
  - 增加期望输出示例，明确 4 个 headless 场景都应 `[PASS]`。
  - 明确 `tools/headless_runner/latest-war-report.json` 是运行时生成产物，不应提交。
  - 增加失败定位规则：数据失败、Domain 泄漏、headless 场景失败、Unity 编译失败、PlayMode 失败分别如何处理。
  - 补充 Windows Git Bash 运行口径和 Unity Hub/winget 独立安装两类 `UNITY_BIN` 示例。
- 新增 `tools/verify_unity_handoff.sh`：
  - 一步执行 Unity 前置门禁：`tools/unity/preflight_without_unity.py` + `tools/verify_headless_war.sh`。
  - 通过后直接提示下一步在 Unity 机器运行 `tools/unity/run_playmode_tests.sh`。
- 更新 `tools/unity/preflight_without_unity.py`：
  - 将 `tools/verify_unity_handoff.sh`、`DemoSceneBootstrap` 和 `UISetup` 纳入 Unity 交接入口检查，避免 Demo 启动骨架缺失时仍误判通过。
- 更新 `tools/unity/run_playmode_tests.sh`：
  - 增加 Windows Unity Hub 默认安装路径自动探测，支持 Git Bash 下直接找到 `Unity.exe`。
  - 增加 winget 独立安装路径自动探测，覆盖 `C:/Program Files/Unity 2022.3.62f3/Editor/Unity.exe` 这类不经过 Unity Hub 的安装目录。
  - 默认测试结果和日志路径从固定 `/tmp` 改为 `${TMPDIR:-${TEMP:-/tmp}}`，避免 Windows Git Bash 没有 Unix `/tmp` 语义时落点不清。
- 新增 Demo 运行时骨架：
  - `DemoSceneBootstrap` 在空场景启动时自动创建 `GameManager`、`MapRenderer`、`MapSetup`、正交相机和正式 `UISetup`。
  - `UISetup` 生成 UGUI HUD、地区/帝皇/朝廷/事件/战报面板，并把 `MainMapUI` 显式绑定到 `GameManager`、面板和按钮。
  - `MainMapUI` 提供“进攻选区”入口，调用 `GameManager.StartPlayerAttack(selectedRegionId)`，不再依赖开发期单路战争脚手架。
- 新增 Windows PowerShell 原生入口：
  - `tools/run_headless_simulation.ps1`
  - `tools/verify_headless_war.ps1`
  - `tools/verify_unity_handoff.ps1`
  - `tools/unity/run_playmode_tests.ps1`
- 新增 Windows Command Prompt 包装入口：
  - `tools/run_headless_simulation.cmd`
  - `tools/verify_headless_war.cmd`
  - `tools/verify_unity_handoff.cmd`
  - `tools/unity/run_playmode_tests.cmd`
- 更新 `tools/unity/preflight_without_unity.py`，将 PowerShell 与 `.cmd` 包装入口纳入 Unity 交接入口检查。
- 项目规则 `CLAUDE.md` 与 `AGENTS.md` 的 JSON 引用验证命令改为 `python tools/validate_data.py` 或 `python3 tools/validate_data.py`，兼容 Windows 上 `python3` launcher 不可用但 `python` 可用的环境。
- 扩展 PlayMode smoke：
  - 新增 `DemoBootstrapCreatesRunnableMapSkeleton`，验证 Demo bootstrap 能创建地图、渲染器、相机、正式 UI，并生成至少 40 个区域面片控制器。

### 验证

- `tools/verify_unity_handoff.sh` 通过。
  - 本机复跑于 2026-05-01 通过：preflight、数据校验、Domain 边界检查和 4 个 headless 战争场景全部通过。
- `python3 tools/unity/preflight_without_unity.py` 通过。
- `tools/verify_headless_war.sh WanChaoGuiYi/Assets/Data faction_qin_shi_huang` 通过：
  - `passed=True`
  - `scenarioCount=4`
  - 四个场景全部 `[PASS]`。
- `tools/unity/run_playmode_tests.sh` 于 2026-05-01 复跑，Unity Editor 可自动定位到 winget 独立安装路径，但仍在授权初始化阶段退出，未生成 PlayMode results XML：
  - `Access token is unavailable; failed to update`
  - `No ULF license found.,Token not found in cache`
  - `No valid Unity Editor license found. Please activate your license.`
- Windows 适配复验于 2026-05-01 通过：
  - `tools/verify_unity_handoff.sh` 通过。
  - `bash -n tools/run_headless_simulation.sh tools/verify_headless_war.sh tools/verify_unity_handoff.sh tools/unity/run_playmode_tests.sh` 通过。
  - `python tools/validate_data.py` 通过，确认 Windows `python` 命令路径可用。
- PowerShell 入口复验于 2026-05-01 通过：
  - `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/verify_unity_handoff.ps1` 通过。
  - `python tools/unity/preflight_without_unity.py` 通过，确认新增 `.ps1` 入口已纳入 preflight。
- Command Prompt 包装入口复验于 2026-05-01 通过：
  - `cmd.exe //c "cd /d E:\\万朝归一\\万朝归一 && tools\\verify_unity_handoff.cmd"` 通过。
  - `python tools/unity/preflight_without_unity.py` 通过，确认新增 `.cmd` 入口已纳入 preflight。
- 2026-05-01 Unity 接口补齐复验通过：
  - `DemoRuntimeHud` 和 `GameManager.RunSingleLaneWarSmokeTest()` 已移除，运行时代码无残留引用。
  - `DemoSceneBootstrap` 改为通过 `Bind(...)` 显式连接 `MapRenderer`、`MapSetup` 和 `UISetup`，不再使用反射写私有字段。
  - `GameManager.StartPlayerAttack(targetRegionId)` 成为 Unity/UI 发起地图行军的正式入口。
  - PlayMode smoke 改为通过 `StartPlayerAttack(enemyArmy.locationRegionId)` 验证开局和 `StartNewGame()` 后的战争绑定。
  - `tools/unity/preflight_without_unity.py` 已改为检查 `UISetup.cs`，不再要求已删除的 Demo HUD。
  - `python tools/validate_data.py`、`python tools/validate_domain_core.py`、`python tools/unity/preflight_without_unity.py` 均通过。
  - 静态 grep 确认 `WanChaoGuiYi/Assets/**/*.cs` 中无 `DemoRuntimeHud`、`RunSingleLaneWarSmokeTest`、`SetPrivateField` 或 `System.Reflection` 残留。
- 2026-05-01 Unity 机制 UI 纵切接入完成：
  - `UISetup` 新增科技、天象、事件、政策胜利 HUD 入口，并运行时生成 `TechPanel`、`WeatherPanel`、`MechanismPanel`。
  - `TechPanel`、`WeatherPanel` 增加 `Bind(...)`，可由运行时 UGUI 生成流程显式绑定。
  - `MainMapUI` 接入科技、天气/天象、政策/胜利面板，并让 `EventTriggered` 真正弹出 `EventPanel`；事件 HUD 按钮已修正为打开事件面板而不是朝廷面板。
  - `MechanismPanel` 通过 `ReformSystem.ApplyPolicy(...)` 提供首个 Unity 政策操作入口，并通过 `VictorySystem.CheckVictory(...)` 显示胜利进度；`ApplyPolicy(...)` 现在会记录 `completedReformIds`，保证政策、胜利和事件触发条件共用同一状态。
  - `MechanismPanel` 继续接入轻量外交/谍报纵切：显示首个非玩家势力的外交关系、宿怨和条约回合，提供“外交行动”按钮在宣战/求和间切换，提供“刺探情报”按钮调用 `EspionageSystem.StartOperation(...)` 并显示玩家进行中的谍报行动。
  - `EmperorPanel` 接入帝皇主动技能展示与“发动首个技能”按钮，读取 `EmperorSkillSystem` 的可用性和冷却状态；`emperors.json` 为秦始皇补入 `书同文车同轨` 技能，Unity UI 可直接调用 `UseActiveSkill(...)`。
  - `EmperorPanel` 补齐继承处理 Unity 操作入口：`UISetup` 运行时生成 `ResolveSuccessionButton`，`MainMapUI` 将 `SuccessionSystem` 传入帝皇面板；当继承风险或朝局压力达到阈值时，按钮调用 `ResolveSuccession(...)` 重置继承风险并改变合法性/平稳继承状态。
  - `TechPanel` 补齐“选择首项研究”Unity 操作入口：`UISetup` 运行时生成 `SetResearchButton`，`MainMapUI` 将 `TechSystem` 传入面板，按钮通过 `TechSystem.SetResearch(...)` 设置当前研究、重置研究进度并写入日志。
  - `RegionPanel` 补齐地区安抚 Unity 操作入口：`UISetup` 运行时生成 `PacifyRegionButton`，玩家自有地区可消耗金钱和粮食提升整合、降低民变风险与地方势力，让内政治理代价从回合系统变为可点击操作。
  - `RegionPanel` 补齐地区建筑 Unity 操作入口：`UISetup` 运行时生成 `BuildRegionBuildingButton`，`MainMapUI` 只对玩家自有地区传入建造用势力，按钮调用 `BuildingSystem.Build(...)` 建造首个满足科技、金钱、槽位和重复性检查的建筑，并在地区面板显示已建建筑列表。
  - `CourtPanel` 补齐募兵 Unity 操作入口：`UISetup` 运行时生成 `RecruitArmyButton`，按钮从玩家首个地区消耗金钱、粮食和兵源，创建新的 `ArmyState` 并写入募兵日志，让兵源资源可以通过朝廷面板转化为可用军队。
  - `CourtPanel` 补齐军队装备 Unity 操作入口：`UISetup` 运行时生成 `EquipArmyButton`，`MainMapUI` 将 `EquipmentSystem` 传入朝廷面板，按钮为首个玩家军队装配首个可用装备，并在朝廷面板显示武器、护甲和特装槽位。
  - PlayMode smoke 新增地区安抚动作覆盖：验证 `PacifyRegionButton` 存在，点击后会提升地区整合、降低民变风险和地方势力，并记录“安抚”日志。
  - PlayMode smoke 新增地区建筑动作覆盖：验证 `BuildRegionBuildingButton` 存在，补足建筑前置科技后点击按钮会初始化地区建筑列表、增加建筑并记录“建造”日志。
  - PlayMode smoke 新增募兵动作覆盖：验证 `RecruitArmyButton` 存在，点击后会创建新的玩家军队、消耗地区兵源并记录“募兵”日志。
  - PlayMode smoke 新增军队装备动作覆盖：验证 `EquipArmyButton` 存在，补足装备前置科技后点击按钮会为玩家军队写入装备槽并记录“装备”日志。
  - PlayMode smoke 新增继承处理动作覆盖：验证 `ResolveSuccessionButton` 存在，制造高继承风险后点击按钮会通过 Unity UI 重置继承风险、改变王朝稳定状态并记录“继承”日志。
  - `python tools/validate_data.py`、`python tools/validate_domain_core.py`、`python tools/unity/preflight_without_unity.py` 均通过。
  - 当前真实 PlayMode 已不再报 license 未激活，但本机未找到 Unity/Tuanjie 编辑器可执行文件；运行 `tools/unity/run_playmode_tests.sh` 需要先设置正确 `UNITY_BIN`，输出仍统一写入 `.outputs/tuanjie`。
- `git diff --check` 通过。

### 限制与下一步

- 当前真实 PlayMode 阻塞点已从 license 变为编辑器路径：`UNITY_BIN="/c/Program Files/Unity 2022.3.62f3/Editor/Unity.exe" tools/unity/run_playmode_tests.sh` 返回 `Unity editor executable not found`，`C:/Program Files/Unity 2022.3.62f3/Editor/Unity.exe`、`C:/Program Files/Unity/Hub/Editor` 和 Program Files 下递归查找均未发现 `Unity.exe`；需要提供实际 Unity/Tuanjie Editor 路径后再运行 PlayMode。

## 2026-05-02 My project PlayMode 修复记录

### 目标

把验证对象从旧 `WanChaoGuiYi` 工程切回真实 Unity 工程 `My project`，补齐 PlayMode smoke 暴露的运行时接入缺口，直到 `My project` 本身真实通过团结编辑器测试。

### 已完成

- `GameManager.CollectSystems()` 注册 `BuildingSystem`，让地区建筑 UI 可取得建造系统，并让建筑系统进入回合系统集合。
- `GameStateFactory.CreateDefault()` 为新局设置默认 `currentWeatherId = "normal"`，避免开局未推进回合时天气状态为空。
- `WeatherSystem.GetCurrentEffect()` 对空 `GameContext`、空 `GameState` 或空天气 id 回退到正常天气，防止天气面板在初始状态崩溃。
- Review 后继续修复 Unity 工具链默认路径：
  - `tools/unity/run_playmode_tests.ps1` 和 `tools/unity/run_playmode_tests.sh` 现在会自动探测仓库上级目录的 `Editor/Tuanjie.exe`，不再必须依赖手动设置 `UNITY_BIN`。
  - `tools/verify_unity_handoff.ps1` 和 `tools/verify_unity_handoff.sh` 的提示文案同步说明本机 Tuanjie 路径。
  - `docs/unity-handoff-checklist.md` 将 headless 数据目录和 Unity 打开目录从旧 `WanChaoGuiYi` 修正为真实 `My project`。
  - `.gitignore` 补入 `My project/Library`、`Temp`、`Logs`、`UserSettings` 和 `.outputs/`，避免 Unity 生成产物污染工作区。

### 验证

- `tools\unity\run_playmode_tests.ps1 "E:\万朝归一\万朝归一\My project"` 通过。
  - 结果文件：`.outputs\tuanjie\wanchao-playmode-results.xml`
  - `testcasecount="4"`、`result="Passed"`、`passed="4"`、`failed="0"`
  - 覆盖：Demo 启动骨架、战争绑定与重开局、战争指令状态、机制 UI 操作纵切。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `tools\verify_unity_handoff.ps1` 通过，并确认 4 个 headless 战争场景全部 `[PASS]`。
- 清除当前进程 `UNITY_BIN` 后重跑 `tools\unity\run_playmode_tests.ps1 "E:\万朝归一\万朝归一\My project"` 通过，确认本机 `../Editor/Tuanjie.exe` 自动探测生效；结果仍为 `testcasecount="4"`、`result="Passed"`、`failed="0"`。
- `bash -n tools\unity\run_playmode_tests.sh` 与 `bash -n tools\verify_unity_handoff.sh` 通过。
- `git status --short -- '.outputs' 'My project/Library'` 不再显示 Unity 生成目录和测试输出目录，说明忽略规则已生效。

### 限制与下一步

- 本次确认的是自动 PlayMode smoke 和静态/数据/交接验证通过；仍建议在 Tuanjie 编辑器中手动打开 `My project`，检查地图、HUD、地区建造和天气面板的实际视觉表现。

## 2026-05-02 将领立绘缺口补齐

### 目标

基于当前 Tuanjie 建模缺口，补齐 `generals.json` 中 12 位将领的可导入立绘资产，并让数据验证能确认每位将领都有实际 PNG 资源。

### 已完成

- 新增 12 张将领立绘到 `My project/Assets/Art/Portraits/Generals/`：
  - `bai_qi.png`
  - `guan_yu.png`
  - `guo_ziyi.png`
  - `han_xin.png`
  - `huo_qubing.png`
  - `li_jing.png`
  - `qi_jiguang.png`
  - `wei_qing.png`
  - `xie_xuan.png`
  - `xu_da.png`
  - `yue_fei.png`
  - `zhuge_liang.png`
- 为 12 张 PNG 补齐 Unity/Tuanjie `.meta`，复用现有帝皇立绘 `TextureImporter` 配置并生成唯一 GUID。
- 清理已无对应目录的 `My project/Assets/Art/Portraits/Generals/_preview.meta`。
- `generals.json` 新增 `portraitAssetPath`，每位将领指向 `Assets/Art/Portraits/Generals/*.png`。
- `GeneralDefinition` 新增 `portraitAssetPath` 字段，供后续 UI 或战斗配置界面读取。
- `docs/data-contract.md` 新增 General 立绘路径契约。
- `tools/validate_data.py` 新增将领立绘路径格式与文件存在性校验。
- 视觉总览保留在 `.outputs/visual/general_portraits_contact_sheet.png`。

### 验证

- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- 资源清点通过：`PngCount=12`、`MetaCount=12`、`MissingMeta=`、`StaleMeta=`。

### 限制与下一步

- 本次完成资产生成、导入元数据和数据引用；当前将领相关 UI 还没有读取 `portraitAssetPath` 显示立绘。后续接入战斗配置或将领详情界面时，应直接复用该字段。

## 2026-05-02 gpt-image-2 将领立绘重生成

### 目标

按最新要求直接调用 `gpt-image-2` 重新生成将领立绘，并将当前将领数据引用切换到 Image 2 版本。

### 已完成

- 直连官方 Image API 尝试 `gpt-image-2` 时连接超时；OpenAI 兼容网关的 `/images/generations` 对 `gpt-image-2` 返回 `model_not_found`。
- 改用 Responses `image_generation` 工具，工具模型显式指定 `gpt-image-2`，并通过单张测试确认可产出 PNG。
- 在 `My project/Assets/Art/Portraits/Generals/` 生成 12 张 Image 2 版本立绘，文件名使用 `_image2.png` 后缀，不覆盖上一版：
  - `bai_qi_image2.png`
  - `guan_yu_image2.png`
  - `guo_ziyi_image2.png`
  - `han_xin_image2.png`
  - `huo_qubing_image2.png`
  - `li_jing_image2.png`
  - `qi_jiguang_image2.png`
  - `wei_qing_image2.png`
  - `xie_xuan_image2.png`
  - `xu_da_image2.png`
  - `yue_fei_image2.png`
  - `zhuge_liang_image2.png`
- 为 12 张 Image 2 PNG 补齐 Unity/Tuanjie `.meta`，复用现有立绘导入配置并生成唯一 GUID。
- `generals.json` 已切换到 `Assets/Art/Portraits/Generals/*_image2.png`。
- 新总览图保存到 `.outputs/visual/general_portraits_image2_contact_sheet.png`。

### 验证

- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- Image 2 资源清点通过：`Image2PngCount=12`、`Image2MetaCount=12`、`MissingPng=`、`MissingMeta=`。
- `git diff --check` 通过，仅有既有 CRLF 转换提示。

### 限制与下一步

- 本次完成 Image 2 资产与数据引用切换；旧版无后缀 PNG 仍保留在同目录作为备份。
- 当前将领 UI 仍未读取 `portraitAssetPath` 展示立绘，后续接战斗配置或将领详情界面时继续复用该字段。

## 2026-05-03 将领立绘在 Court UI 中展示

### 目标

让已接入 `portraitAssetPath` 的将领立绘真正显示在朝廷面板里，而不只是停留在数据表。

### 已完成

- `CourtPanel` 新增将领立绘网格，显示时会根据 `context.Data.Generals` 重建卡片列表。
- 每个将领卡读取 `portraitAssetPath`，从 `Assets/Art/Portraits/Generals/*_image2.png` 加载 `Sprite` 并显示。
- `UISetup` 为 `CourtPanel` 生成 `GeneralPortraitGridContent`，并把该容器绑定给 `CourtPanel`。
- `GameManagerPlayModeSmokeTests` 增加断言，确认朝廷面板生成 12 个将领卡、每个卡片都能加载到非空 `Sprite`。

### 验证

- `GameManagerPlayModeSmokeTests` 定向测试通过：`UnityUiExposesMechanismPanelsAndActions`，`testcasecount=1`、`passed=1`、`failed=0`。
- `GameManagerPlayModeSmokeTests` 整个测试类通过：`testcasecount=4`、`passed=4`、`failed=0`。
- `python tools\validate_data.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `git diff --check` 通过，仅有既有 CRLF 转换提示。

### 限制与下一步

- 全量未过滤 PlayMode 仍会被既有 `VisualSmokeCaptureTests` 的 headless 渲染崩溃打断，属于仓库里已存在的独立问题，不是本次将领立绘展示逻辑引入的失败。
- 如果后续要把将领卡做成可点击详情面板，可以直接复用同一组 `portraitAssetPath` 与网格生成逻辑。

## 2026-05-03 VisualSmokeCaptureTests headless 渲染崩溃修复

### 目标

修复全量 PlayMode 在 `-batchmode -nographics` 下被既有 `VisualSmokeCaptureTests` 打断的问题，使 headless 验证可以稳定跑完玩法/UI smoke。

### 根因

- `VisualSmokeCaptureTests.Capture()` 在无图形设备的 headless 环境继续执行 `Camera.Render()`。
- Tuanjie/Unity 日志显示 `GfxDevice renderer is null`，崩溃栈落在 `UnityEngine.Camera:Render` 与原生渲染路径，进程收到 `SIGSEGV`。

### 已完成

- `VisualSmokeCaptureTests` 新增 `CanRenderScreenshots()` 前置门禁。
- 当 `SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null` 或图形设备名包含 `Null` 时，视觉截图测试以 `Assert.Ignore` 明确跳过。
- 有可用图形设备时仍保留原有 1600x900 截图流程，不改变视觉 smoke 的手动/有显卡验收路径。

### 验证

- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过。
- 最新 `.outputs/tuanjie/wanchao-playmode-results.xml`：`total=5`、`passed=4`、`failed=0`、`skipped=1`。
- `GameManagerPlayModeSmokeTests` 4 个用例全部通过。
- `VisualSmokeCaptureTests.CaptureMapHudRegionBuildAndWeatherPanels` 在 headless 下跳过，原因：`Visual smoke capture requires an active graphics device; skipped under headless or -nographics PlayMode.`
- 最新 `.outputs/tuanjie/wanchao-playmode.log` 未再出现 `Native Crash Reporting`、`SIGSEGV` 或 `Camera:Render` 崩溃栈。
- 不带 `-nographics` 定向运行 `VisualSmokeCaptureTests` 通过，日志确认使用 Direct3D 11 / NVIDIA GeForce RTX 5070 Ti Laptop GPU；结果文件 `.outputs/tuanjie/visual-smoke-graphics-results.xml` 显示 `total=1`、`passed=1`、`failed=0`。
- 产出的 3 张临时截图已查看非空渲染结果，并按临时产物规则删除；复查 `.outputs/visual` 下无 `unity-*.png` 残留。

### 限制与下一步

- headless 环境不会生成 `.outputs/visual/unity-*.png` 截图；截图验收仍需在有图形设备的 Unity/Tuanjie 运行中执行，并在检查后删除临时截图。

## 2026-05-03 Play Mode 地图与实体层纠偏

### 目标

修复进入 Play Mode 后运行时 Demo 把全部帝皇立绘和临时六边形区域层直接堆到画面上的问题，让运行时画面服从真实地图资产和实际游戏状态。

### 根因

- `DemoSceneBootstrap` 在 `RuntimeInitializeOnLoad` 路径中强制创建运行时 Demo，对已有编辑器预览层没有隔离。
- `DemoEntityVisualSpawner` 默认调用 `SpawnEmperorPortraits()`，把所有势力帝皇立绘按横条铺在地图上；这不是玩家操作或游戏状态触发的实体。
- `MapSetup` 只生成 `map_region_shapes.json` 的 `playable_blockout_v1` 六边形区域面片，没有加载 `Assets/Art/Map/jiuzhou_generated_map.png` 作为主地图底图。

### 已完成

- `DemoSceneBootstrap` 进入 Play Mode 时禁用 `EditorVisibleEntityPreview`，避免编辑器预览层和运行时层叠加。
- `DemoSceneBootstrap` 相机参数对齐编辑器预览视角，减少旧蓝底节点图视角造成的留白。
- `MapSetup` 运行时加载 `Assets/Art/Map/jiuzhou_generated_map.png`，创建 `Generated_Jiuzhou_Map_Background_Runtime` 作为主地图底图。
- `MapSetup` 将区域面片收束为 `RegionInteractionLayer`，在整图底图存在时关闭区域面片 `MeshRenderer`，只保留 `MeshCollider + RegionController` 作为透明交互层。
- `DemoEntityVisualSpawner` 默认不再铺全体帝皇立绘；只保留基于 `World.Map.ArmiesById` 的军队图标，属于当前游戏状态中的实际实体。
- `GameManagerPlayModeSmokeTests.DemoBootstrapCreatesRunnableMapSkeleton` 新增断言：运行时必须创建整图底图、不应自动生成 `Emperor_qin_shi_huang` 世界立绘、区域面片应作为隐藏交互层存在。

### 验证

- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=5`、`passed=4`、`failed=0`、`skipped=1`。
- 不带 `-nographics` 定向运行 `VisualSmokeCaptureTests` 通过：`total=1`、`passed=1`、`failed=0`；日志确认使用 Direct3D 11 / NVIDIA GeForce RTX 5070 Ti Laptop GPU。
- 视觉截图复查确认：顶部全帝皇立绘条已消失，运行时使用整张 `jiuzhou_generated_map.png`，区域层不再额外显示为一套覆盖地图的彩色六边形。
- 本轮生成的 3 张临时 `unity-*.png` 截图已删除；复查 `.outputs/visual` 无 `unity-*.png` 残留。

### 限制与下一步

- 当前军队图标仍使用原型单位图标直接贴在区域中心；后续应按“选中军队、行军、接敌、战报”等实际调用路径逐步显示/强调，而不是继续扩大默认常驻实体层。

## 2026-05-03 地图美术源图与区域边界精修

### 目标

回应 Play Mode 地图反馈，把修复从“运行时隐藏六边形面片”推进到地图源图和区域边界数据本身，避免底图、点击区和游戏状态各用一套互相脱节的表达。

### 根因

- `Assets/Art/Map/jiuzhou_generated_map.png` 本身仍是旧的六边形节点图，隐藏 `MeshRenderer` 后画面仍会看到六边形地点标识。
- `map_region_shapes.json` 的 `precision` 仍是 `playable_blockout_v1`，多数区域为统一六点 blockout，不适合作为后续地图交互和视觉表达的真实来源。
- `MapSetup` 的运行时投影参数没有记录底图生成所用的形状坐标中心，后续替换底图时容易再次错位。

### 已完成

- 新增 `tools/art/render_jiuzhou_map.py`，从 `map_region_shapes.json` 的稳定区域中心生成非均匀区域轮廓，并用同一份形状数据重绘 2048x1536 的 `jiuzhou_generated_map.png`。
- `map_region_shapes.json` 从 `playable_blockout_v1` 更新为 `map_aligned_region_v1`，56 个区域的边界点数已分化为 4 到 11 点，不再是全量六边形小格子。
- 新增 `map_render_metadata.json`，记录底图尺寸、形状坐标中心、每形状单位像素数和 Sprite PPU；`MapSetup` 读取该元数据计算 `RegionInteractionLayer` 投影原点与缩放，避免 Python/C# 双份投影常数漂移。
- `tools/validate_data.py` 增加回归门禁：拒绝旧 `playable_blockout_v1` 精度和全量六点 blockout 区域。
- `GameManagerPlayModeSmokeTests.DemoBootstrapCreatesRunnableMapSkeleton` 增加投影命中验收：对 `xiyu`、`liaodong`、`bashu`、`jiangdong` 的区域中心做射线命中，确认能命中对应隐藏 `MeshCollider` 并发布正确 `RegionSelected`。
- `VisualSmokeCaptureTests` 增加运行时视觉契约断言：必须使用 2048x1536 整图底图、不得出现旧 `Emperor_qin_shi_huang` 默认立绘条、不得渲染可见 `RegionSurface_*` debug 面片。
- Ralph 本地替代流程补齐 `.omx/context/map-source-boundary-refinement-20260502T234611Z.md`、`.omx/plans/prd-map-source-boundary-refinement.md` 和 `.omx/plans/test-spec-map-source-boundary-refinement.md`。

### 验证

- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- 区域边界统计通过：`precision=map_aligned_region_v1`，点数分布为 `{4: 8, 5: 19, 6: 19, 7: 4, 8: 3, 9: 2, 11: 1}`，面积范围 `0.86` 到 `6.747`。
- 地图源图复查通过：`jiuzhou_generated_map.png` 保持 `2048x1536`，已由非均匀区域轮廓重绘。
- `map_render_metadata.json` 校验通过：`precision` 与 `map_region_shapes.json` 一致，源图路径为 `Assets/Art/Map/jiuzhou_generated_map.png`，PNG 头部尺寸为 `2048x1536`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=5`、`passed=4`、`failed=0`、`skipped=1`；跳过项仍是 headless 下的 `VisualSmokeCaptureTests`，其余 PlayMode 包含新增区域投影命中断言。
- 不带 `-nographics` 定向运行 `VisualSmokeCaptureTests` 通过：`total=1`、`passed=1`、`failed=0`；日志确认 Direct3D 11 / NVIDIA GeForce RTX 5070 Ti Laptop GPU，并覆盖新增运行时视觉契约断言。
- 本轮产生的 `unity-map-hud.png`、`unity-region-building-panel.png`、`unity-weather-panel.png` 已检查并删除；复查 `.outputs/visual` 无 `unity-*.png` 残留。

### 限制与下一步

- 当前边界是基于既有 56 个玩法区域中心生成的地图对齐轮廓，不是历史 GIS 级边界；若进入美术锁定阶段，应由真实地形/河流/山脉矢量稿继续人工修边。
- 运行时军队图标仍偏原型化，但只来自 `World.Map.ArmiesById` 的实际游戏状态；后续应按选中、行军、接敌、战报等调用路径做状态化显示。

## 2026-05-03 军队实体层状态化显示

### 目标

继续修复 Play Mode 进入后实体层“默认堆上来”的问题，把军队图标从常驻展示改成由真实战争调用路径触发，避免运行时画面在玩家没有下令、没有接敌时铺满原型实体。

### 根因

- `DemoEntityVisualSpawner` 虽然已不再默认铺全体帝皇立绘，但仍遍历 `World.Map.ArmiesById` 并生成全部军队图标。
- `DemoEntityVisualSpawner.NeedsRefresh()` 只比较军队数量、回合和季节；`ArmyMoveStarted`、`ArmyArrived`、`EngagementStarted` 等真实游戏事件不会立刻触发画面刷新。

### 已完成

- `DemoEntityVisualSpawner` 新增默认规则：空闲军队不显示；只有行军、撤退、增援、围攻、接敌、目标路线或争夺地区中的军队才显示。
- `DemoEntityVisualSpawner` 订阅 `GameStarted`、`ArmyMoveStarted`、`ArmyArrived`、`ContactDetected`、`EngagementStarted`、`BattleResolved`、`RegionOccupied`，由实际状态变化驱动刷新。
- 刷新判定从“数量/回合/季节”扩展为实体视觉签名，覆盖军队位置、目标、任务、接敌、单位、兵力等字段。
- `GameManagerPlayModeSmokeTests.DemoBootstrapCreatesRunnableMapSkeleton` 新增断言：默认 Play Mode 不生成 `Army_army_player_1` 或 `Army_army_enemy_1`；发起玩家攻击后只显示行军玩家军队；接敌后敌军才显示。
- `VisualSmokeCaptureTests` 增加视觉契约断言：默认地图 HUD 截图不得出现空闲玩家军队或空闲敌军图标。

### 验证

- `python tools\validate_data.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `git diff --check -- "My project/Assets/Scripts/Demo/DemoEntityVisualSpawner.cs" "My project/Assets/Tests/PlayMode/GameManagerPlayModeSmokeTests.cs" "My project/Assets/Tests/PlayMode/VisualSmokeCaptureTests.cs"` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=5`、`passed=4`、`failed=0`、`skipped=1`；跳过项仍是 headless 下的 `VisualSmokeCaptureTests`。
- 原项目已有用户/Hub 打开的 Tuanjie 实例占用，直接图形 VisualSmoke 不能复用同一路径；本轮改用 `.outputs/tuanjie/visual-project-copy` 隔离工程副本运行。
- 隔离工程首次打开时重建 `Library` 触发一次 URP 导入期 `Cannot create required material because shader is null` 日志，重跑后通过：`.outputs/tuanjie/visual-smoke-graphics-copy-results.xml` 显示 `total=1`、`passed=1`、`failed=0`；日志确认 Direct3D 11 / NVIDIA GeForce RTX 5070 Ti Laptop GPU。
- 图形截图核验通过：`unity-map-hud.png`、`unity-region-building-panel.png`、`unity-weather-panel.png` 均为 `1600x900` 且采样颜色数分别为 `1969`、`2217`、`2100`，非空渲染。
- 临时 `unity-*.png` 截图已删除；复查 `.outputs/visual` 无 `unity-*.png` 残留。临时 `.outputs/tuanjie/visual-project-copy` 工程副本也已删除。

### 限制与下一步

- 军队图标仍使用现有单位原型图标；当前修复只收紧出现规则和刷新触发，后续可继续做选中军队、路线预览、接敌高亮和战报回放的分层视觉表现。
- `MapSetup` 仍通过 `Application.dataPath` 读取地图 PNG 和元数据；正式构建阶段建议改成 Unity 资产引用或 Resources/Addressables 约定。

## 2026-05-04 地图底图 Resources 构建路径补齐

### 目标

把地图底图和投影元数据从仅编辑器可用的 `Application.dataPath` 直读，推进到可被 Unity/Tuanjie 构建打包的 `Resources` 资源路径，同时保留文件直读作为开发兜底。

### 根因

- `MapSetup` 直接读取 `Assets/Art/Map/jiuzhou_generated_map.png` 和 `Assets/Data/map_render_metadata.json`，在编辑器 PlayMode 可用，但正式构建后 `Assets` 源目录不再是稳定运行时输入。
- 首轮新增 `Resources` 镜像时使用了非 Unity 标准 32-hex 的 `.meta` GUID，隔离工程导入时 Tuanjie 会忽略对应资源，导致 `Resources.Load` 无法命中。

### 已完成

- 新增 `Assets/Resources/Map/jiuzhou_generated_map.png`，作为地图底图的构建可打包镜像。
- 新增 `Assets/Resources/Data/map_render_metadata.json`，作为地图投影元数据的构建可打包镜像。
- `MapSetup` 优先通过 `Resources.Load` 加载地图图片和元数据；只有 Resources 不可用时才回退到 `Application.dataPath` 文件读取。
- `MapSetup` 暴露本轮验证用的加载来源状态，PlayMode 会断言运行时确实命中 Resources 路径。
- `tools/art/render_jiuzhou_map.py` 在重绘地图时同步刷新 Resources 镜像，避免主资产和构建资产漂移。
- `tools/validate_data.py` 增加门禁：校验 Resources 镜像存在、内容与主资产一致，并校验本轮新增地图相关 `.meta` 使用 Unity/Tuanjie 可解析的 32-hex GUID。
- `tools/unity/preflight_without_unity.py` 增加 Resources 地图底图和元数据存在性检查。

### 验证

- `python tools\validate_data.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `git diff --check` 针对本轮修改文件通过，仅提示既有 LF/CRLF 转换警告。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 更新时间为 `2026/5/4 8:12:12`，显示 `total=5`、`passed=4`、`failed=0`、`skipped=1`；跳过项仍是 headless 下的 `VisualSmokeCaptureTests`。
- 图形 VisualSmoke 在隔离工程副本 `.outputs/tuanjie/visual-project-copy` 中重跑通过：`.outputs/tuanjie/visual-smoke-graphics-copy-results.xml` 显示 `total=1`、`passed=1`、`failed=0`、`skipped=0`；日志确认 Direct3D 11 / NVIDIA GeForce RTX 5070 Ti Laptop GPU。
- 本轮图形截图核验通过：`unity-map-hud.png`、`unity-region-building-panel.png`、`unity-weather-panel.png` 均为 `1600x900`，采样颜色数分别为 `1984`、`2272`、`2125`，非空渲染。
- 临时 `unity-*.png` 截图已删除；复查 `.outputs/visual` 无 `unity-*.png` 残留。临时 `.outputs/tuanjie/visual-project-copy` 工程副本也已删除。

### 限制与下一步

- 图形隔离工程首次导入时仍可能出现一次 URP `Cannot create required material because shader is null` 测试日志拦截；同一副本重跑后通过，属于首次重建 `Library` 的导入期噪声。
- 只修复了地图 Resources 链路相关 `.meta`。本轮只读扫描发现 `My project/Assets` 下仍有约 `270` 个非 32-hex GUID `.meta`，图形日志中明确出现将领立绘 `.meta` 被 Tuanjie 忽略；后续应单独做“Unity meta GUID 规范化”任务，先评估场景/Prefab 引用风险，再机械修复并验证导入日志。

## 2026-05-04 Unity meta GUID 全 Assets 规范化

### 目标

修复 Tuanjie 导入阶段因非 32-hex `.meta` GUID 忽略资源的问题，覆盖将领立绘、美术图集、数据、脚本、测试和场景元数据，避免后续 PlayMode 或图形 VisualSmoke 被资产导入缺失打断。

### 根因

- `My project/Assets` 下存在 270 个非 Unity/Tuanjie 标准 32-hex GUID 的 `.meta` 文件。
- Tuanjie 导入日志会对这类文件报出 `does not have a valid GUID and its corresponding Asset file will be ignored`，将领立绘已经明确命中该问题。
- 这些 invalid GUID 经全工程文本扫描未发现外部序列化引用；命中仅来自各自 `.meta` 本身，因此可以安全替换为新的唯一 32-hex GUID。

### 已完成

- 对 `My project/Assets/**/*.meta` 中 270 个 invalid GUID 做机械规范化，生成稳定、唯一、32 位小写十六进制 GUID。
- 覆盖范围包括 `Assets/Art`、`Assets/Data`、`Assets/Resources`、`Assets/Scripts`、`Assets/Tests`、`Assets/Editor`、`Assets/Scenes` 以及根级资产目录 `.meta`。
- `tools/validate_data.py` 的 Unity `.meta` 门禁从地图 Resources 相关文件扩展为全 `Assets` 扫描，校验 GUID 格式和重复 GUID。
- 修复前先做外部引用扫描，确认 270 个旧 invalid GUID 均无场景、Prefab、材质、asmdef 或数据表外部引用。

### 验证

- 全 `Assets` `.meta` 复查通过：`meta_total=284`、`invalid=0`、`duplicates=0`。
- `python tools\validate_data.py` 通过，并覆盖新增全 Assets `.meta` 门禁。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `git diff --check -- "tools/validate_data.py"` 通过，仅提示既有 LF/CRLF 转换警告。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=5`、`passed=4`、`failed=0`、`skipped=1`；跳过项仍是 headless 下的 `VisualSmokeCaptureTests`。
- headless PlayMode 日志未再出现 `does not have a valid GUID`、`will be ignored`、`Native Crash`、`SIGSEGV` 或 `Camera:Render` 崩溃栈。
- 图形 VisualSmoke 使用隔离副本 `.outputs/tuanjie/visual-project-copy` 重跑通过：`.outputs/tuanjie/visual-smoke-graphics-copy-results.xml` 显示 `total=1`、`passed=1`、`failed=0`、`skipped=0`；日志确认 Direct3D 11 / NVIDIA GeForce RTX 5070 Ti Laptop GPU。
- 图形截图核验通过：`unity-map-hud.png`、`unity-region-building-panel.png`、`unity-weather-panel.png` 均为 `1600x900`，采样颜色数分别为 `1989`、`2258`、`2131`，非空渲染。
- 临时 `unity-*.png` 截图已删除；复查 `.outputs/visual` 无 `unity-*.png` 残留。临时 `.outputs/tuanjie/visual-project-copy` 工程副本已删除。

### 限制与下一步

- 图形隔离工程首次重建 `Library` 时仍可能被 URP `Cannot create required material because shader is null` 导入期日志拦截；同一副本第二次运行已通过，本轮未把该噪声改成测试豁免。
- 当前只规范化 `My project/Assets` 下项目资产 `.meta`；未触碰 `Packages` 缓存或 Unity 生成的 `Library`。

## 2026-05-04 图形 VisualSmoke 自动化入口补齐

### 目标

把图形 VisualSmoke 从手动流程固化为可重复运行的验证入口：自动创建隔离工程副本、运行有图形设备的 PlayMode 截图测试、处理首次导入期 URP 噪声、核验截图非空，并在结束后删除临时截图和工程副本。

### 根因

- 图形 VisualSmoke 不能在 headless `-nographics` 下运行，但手动用主工程运行容易受已打开的 Tuanjie/Hub 实例影响。
- 隔离副本首次重建 `Library` 时常被 URP `Cannot create required material because shader is null` 导入期日志拦截；同一副本重跑后通过。
- 之前每轮都要手动复制工程、重跑、检查 `unity-*.png`、删除截图和 `.outputs/tuanjie/visual-project-copy`，容易遗漏清理或证据记录。

### 已完成

- 新增 `tools/unity/run_visual_smoke_tests.ps1`。
  - 默认复制 `My project` 到 `.outputs/tuanjie/visual-project-copy`，排除 `Library`、`Temp`、`Obj`、`Logs`。
  - 不带 `-nographics` 运行 `WanChaoGuiYi.Tests.VisualSmokeCaptureTests`。
  - 若首轮被已知 URP material 导入日志拦截，自动在同一副本上重跑一次。
  - 检查 `unity-map-hud.png`、`unity-region-building-panel.png`、`unity-weather-panel.png` 存在、非空、尺寸为 `1600x900`，并做采样颜色数防空白检查。
  - `finally` 清理 `.outputs/visual/unity-*.png` 和 `.outputs/tuanjie/visual-project-copy`，保留 XML/log 证据。
- 新增 `tools/unity/run_visual_smoke_tests.cmd`，方便 Windows Command Prompt 调用。
- `tools/unity/preflight_without_unity.py` 纳入新 VisualSmoke 入口文件存在性检查。
- `docs/unity-handoff-checklist.md` 增加图形 VisualSmoke 截图验证说明。

### 验证

- PowerShell 解析检查通过：`tools/unity/run_visual_smoke_tests.ps1` 无语法错误。
- `python tools\unity\preflight_without_unity.py` 通过，确认新入口纳入静态门禁。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 通过。
  - 第一次尝试识别到已知 URP 首次导入日志并自动重跑。
  - 第二次尝试通过：`.outputs/tuanjie/visual-smoke-graphics-copy-results.xml` 显示 `total=1`、`passed=1`、`failed=0`、`skipped=0`。
  - 截图核验通过：三张截图均为 `1600x900`，采样颜色数分别为 `367`、`445`、`384`。
  - 脚本结束后复查：`.outputs/visual` 无 `unity-*.png`，`.outputs/tuanjie/visual-project-copy` 不存在。
- `python tools\validate_data.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `git diff --check -- "tools/unity/run_visual_smoke_tests.ps1" "tools/unity/run_visual_smoke_tests.cmd" "tools/unity/preflight_without_unity.py" "docs/unity-handoff-checklist.md"` 通过，仅提示既有 LF/CRLF 转换警告。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=5`、`passed=4`、`failed=0`、`skipped=1`；跳过项仍是 headless 下的 `VisualSmokeCaptureTests`。

### 限制与下一步

- 新脚本默认面向 Windows PowerShell/Tuanjie 图形环境；Git Bash/macOS/Linux 图形入口尚未补齐。
- Tuanjie 授权客户端仍会输出若干 licensing 诊断日志，但当前测试结果不受影响。

## 2026-05-04 编辑器外 2.5D 九州地图预览管线

### 目标

回应文明 6 / 率土之滨实现方式讨论，先在不依赖 Tuanjie 编辑器的路径上做出可验证的 3D/2.5D 地图效果原型。该原型必须复用现有地图数据，而不是重新生成脱离实际玩法的六边形地点标识。

### 根因

- 当前 Unity 运行时已经能使用整张九州地图和区域交互层，但 3D/立体方向仍缺少一个快速迭代的编辑器外预览面。
- 每次依赖 Tuanjie 图形运行会受导入、授权、已打开工程和截图清理影响，不适合做早期视觉方案快速试错。
- 项目已有 `regions.json`、`map_region_shapes.json`、`neighbors` 和地形/人口/地方势力/民变风险字段，足够先生成数据驱动的 2.5D 沙盘预览。

### 已完成

- 新增 Ralph 上下文快照 `.omx/context/3d-map-preview-20260504T091512Z.md`，固定本轮目标、约束、证据和触点。
- 新增 `tools/art/render_jiuzhou_isometric_preview.py`。
  - 读取 `My project/Assets/Data/regions.json` 和 `map_region_shapes.json`。
  - 使用真实区域边界绘制 56 个挤出面片。
  - 地形颜色来自 `regions.json terrain`。
  - 立体高度由 terrain、population、localPower、rebellionRisk 共同驱动。
  - 橙色边线标记较高民变风险。
  - 示例行军线通过 `regions.json neighbors` 做图搜索生成，避免脱离实际邻接数据。
  - 默认输出 `.outputs/visual/jiuzhou_isometric_preview.png`。
- `tools/unity/preflight_without_unity.py` 纳入该预览脚本存在性检查，避免后续工具入口丢失。

### 验证

- `python tools\art\render_jiuzhou_isometric_preview.py` 通过。
  - 输出：`.outputs/visual/jiuzhou_isometric_preview.png`
  - `regions=56`
  - `width=1920`
  - `height=1280`
  - `heightRange.min=22.81`
  - `heightRange.max=79.53`
  - 示例路线：`guanzhong -> zhongyuan`、`bashu -> jiangdong`、`liaodong -> guanzhong`
- 预览图核验通过：`1920x1280`、`bytes=149334`、采样颜色数 `4855`，确认非空渲染。
- `python -m py_compile tools\art\render_jiuzhou_isometric_preview.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_data.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `git diff --check -- ".omx/context/3d-map-preview-20260504T091512Z.md" "tools/art/render_jiuzhou_isometric_preview.py" "tools/unity/preflight_without_unity.py"` 通过，仅提示既有 LF/CRLF 转换警告。

### 限制与下一步

- 当前是 Pillow 生成的静态 2.5D 预览，不是 Unity runtime Mesh，也不是可旋转 Three.js 场景。
- 区域高度规则是视觉原型规则，尚未进入正式数据契约；后续如果要进入 Unity，应先新增 `map_terrain_visuals.json` 或等价字段契约。
- 下一步可把同一套区域边界和高度规则导出为 Unity Mesh / OBJ / GLB，或做 Three.js 交互预览，以便比较文明 6 式地形块和率土式行军沙盘的表现差异。

## 2026-05-04 Ralplan 战略地图双闭环规划收口

### 目标

把 `.omx/specs/deep-interview-strategy-map-longrun-plan.md` 转换为可执行的 Phase One 规划，明确“治理地图闭环 + 战争地图闭环”的阶段边界、验收路径和 Ralph/Team 后续执行入口。规划坚持现有 56 区域省域图为权威结构，不替换为伪六边形地点标识。

### 已完成

- 新增 `.omx/plans/prd-strategy-map-dual-loop.md`，定义 Stage 0、Stage A 到 Stage F 的 PRD、ADR、非目标、agent roster、Ralph/Team staffing guidance。
- 新增 `.omx/plans/test-spec-strategy-map-dual-loop.md`，定义逐阶段验证矩阵、baseline 命令、7 张 VisualSmoke 截图合同、截图清理要求和最终闭环门禁。
- Ralplan 共识审查完成：
  - Architect 首轮 `ITERATE`，要求补齐 source gate、SelectionContext、headless war gate、VisualSmoke 合同和 fallback closure。
  - Critic 首轮 `ITERATE`，要求 Stage 0 前置、明确 7 张截图、硬化 fallback 证明、统一 Stage E headless 入口。
  - Architect 末轮 `APPROVE`。
  - Critic 末轮 `APPROVE`。

### 关键决策

- Stage 0 先补政策 `sourceReference` 最小闭环：`PolicyDefinition`、`policies.json`、`docs/data-contract.md`、`tools/validate_data.py` 和政策 UI 必须同步支持来源说明，Stage B 不得绕过。
- Stage A 必须落 `MapInteractionMode + SelectionContext`，地图点击、外交桥、战争命令都从同一选择上下文读取，不得继续默认操作“第一个其他势力”。
- Stage E 的唯一 headless 因果验收入口是 `tools\verify_headless_war.ps1`，后续可扩展其内部 runner，但不得另起未纳入 baseline 的旁路脚本。
- Stage F 的图形 VisualSmoke 必须扩展到 7 张临时截图，并继续在图形设备环境运行；验证后必须删除 `.outputs/visual/unity-*.png` 和隔离工程副本。
- closure 验收不得依赖 `allowNodeFallback`：PlayMode 需要证明 56 个 `RegionSurface_*` 存在、无 fallback node，且日志无 `Using legacy node fallback` 与 `Missing usable map region shape`。

### 验证

- 本次只做规划文件落盘，未修改 Unity 玩法代码，未运行 PlayMode 或 VisualSmoke。
- 已完成共识型只读审查，最终 Architect 与 Critic 均批准。
- 后续实现阶段必须按 `.omx/plans/test-spec-strategy-map-dual-loop.md` 运行验证，并在每个阶段回写本报告。

### 后续入口

```text
$ralph .omx/plans/prd-strategy-map-dual-loop.md .omx/plans/test-spec-strategy-map-dual-loop.md
```

或在需要并行时：

```text
$team .omx/plans/prd-strategy-map-dual-loop.md .omx/plans/test-spec-strategy-map-dual-loop.md
```

## 2026-05-04 Strategy Map Dual-Loop Stage 0 执行

### 目标

执行 `.omx/plans/prd-strategy-map-dual-loop.md` 的强制 Stage 0：先补齐政策 `sourceReference` 最小闭环，再允许后续治理 UI 依赖来源说明。

### 已完成

- `PolicyDefinition` 新增 `sourceReference` 字段。
- `policies.json` 35 条政策均补齐 `sourceReference`，来源引用正史、政书或典籍。
- `docs/data-contract.md` 的 Policy 契约补充 `sourceReference` 必填要求，并说明治理/政策 UI 必须展示来源摘要。
- `docs/data-contract.md` 新增 Source Reference Contract，覆盖后续 policy、diplomacy、border、war、governance 机制的来源字段、示例和 UI 展示规则。
- `tools/validate_data.py` 新增政策来源门禁，缺少 `sourceReference` 会失败。
- `MechanismPanel` 的可执行政策列表显示政策来源。
- `GameManagerPlayModeSmokeTests.UnityUiExposesMechanismPanelsAndActions` 增加 UI smoke：确认 `standardization` 政策有来源，并且机制面板文本包含该来源。
- 新增 Ralph 执行上下文 `.omx/context/strategy-map-dual-loop-execution-20260504T121458Z.md` 和进度状态 `.omx/state/strategy-map-dual-loop/ralph-progress.json`。

### 验证

- `python tools\validate_data.py` 通过，输出 `policies=35`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `git diff --check` 针对 Stage 0 修改文件通过，仅提示既有 LF/CRLF 转换。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过；`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=5`、`passed=4`、`failed=0`、`skipped=1`。跳过项仍是 headless 下的 `VisualSmokeCaptureTests`。
- 本阶段未运行图形 VisualSmoke，也未生成新的 `unity-*.png`；复查 `.outputs/visual` 无 `unity-*.png` 残留。

### 限制与下一步

- 首轮 Ralph 架构复核给出 `ITERATE`：实现、数据、校验、UI 和 PlayMode 验证基本成立，但数据契约缺少 diplomacy/border/war/governance 来源字段约定；现已补齐并等待复核。
- 下一阶段应进入 Stage A：`MapInteractionMode + SelectionContext`、56 个 `RegionSurface_*` closure 门禁、以及不依赖 `allowNodeFallback` 的 PlayMode 证明。

- 第二轮 Ralph 架构复核已返回 `APPROVE`，Stage 0 正式收口，可进入 Stage A。复核再次确认 `Source Reference Contract` 已覆盖 `policy`、`diplomacy`、`border`、`war`、`governance`，且 `python tools\validate_data.py` 与 `python tools\unity\preflight_without_unity.py` 均通过。

## 2026-05-04 Strategy Map Dual-Loop Stage A 执行

### 目标

执行 Stage A 地图交互基础：保留现有 56 区域真实地图为唯一交互权威，新增显性的 `MapInteractionMode + SelectionContext`，并证明普通区域点击不会绕过外交桥直接发起攻击。

### 已完成

- 新增 `MapInteractionMode` 与 `SelectionContext`，包含 `selectedRegionId`、`selectedArmyId`、`ownerFactionId`、`targetFactionId`、friendly/neighbor/hostile、`modeEntryReason`、available actions 和 disabled reasons。
- `MainMapUI` 默认进入 `Governance`，区域点击先构建 `SelectionContext`，友方区域保持治理模式，邻接非友方区域进入外交桥模式；`AttackSelectedRegion` 只有在 `War` 且上下文允许 `dispatch_attack` 时才会调用 `StartPlayerAttack`。
- HUD 新增 `SelectionContextText`，以紧凑文本显示当前模式、区域、friendly/neighbor/hostile 反馈；进攻按钮默认禁用，避免普通点击被误解为攻击命令。
- `MapSetup` 新增只读诊断：`AllowNodeFallback`、`RegionSurfaceCount`、`LegacyNodeFallbackCount`、`MissingUsableShapeCount`、`MeshBuildFailureCount`。
- PlayMode smoke 新增 Stage A 断言：56 个 `RegionSurface_*`、无 legacy `Region_*` fallback node、无 fallback/missing-shape 日志、默认治理模式、选择上下文所有权与邻接判断、普通外区点击不改变军队任务或目标。

### 验证

- `python tools\validate_data.py` 通过，输出 `regions=56 map_region_shapes=56 policies=35`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过，`scenarioCount=4`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 二次干净运行通过；`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=6`、`passed=5`、`failed=0`、`skipped=1`，跳过项仍是 headless 下的 `VisualSmokeCaptureTests`。
- 第二次 PlayMode 日志复查无 `Using legacy node fallback`、`Missing usable map region shape`、`error CS`、`Native Crash` 或 `SIGSEGV`。
- 本阶段未运行图形 VisualSmoke，也未生成 `unity-*.png`；复查 `.outputs/visual` 无 `unity-*.png` 残留。

### 限制与下一步

- Ralph 架构复核已返回 `APPROVE`，Stage A 正式收口，可进入 Stage B。
- `MechanismPanel` 的外交动作仍保留旧的 `FindFirstOtherFaction()` 路径，这是 Stage C 的明确修复目标，不作为 Stage A 阻断。

## 2026-05-04 Strategy Map Dual-Loop Stage B 执行

### 目标

执行 Stage B 治理主界面：默认治理视图下，友方区域详情必须清楚显示政治、民生、粮食、人口、合法性、风险、建筑、政策和历史来源说明，同时不遮断地图选择反馈。

### 已完成

- `RegionPanel` 新增 `GovernanceOverviewText`，友方区域展示紧凑治理总览。
- 治理总览覆盖 `Politics`、`Civic`、`Grain`、`Population`、`legitimacy`、`Risk`、`Building`、`Policy` 和 `Source:`。
- 建筑来源从 `BuildingDefinition.sourceReference` 读取；政策来源优先显示 `standardization.sourceReference`，再退到首个可执行政策。
- `UISetup` 将 `RegionPanel` 调整为更宽的治理面板，使用双列基础指标和固定高度总览文本，保留原有安抚、建造、关闭按钮。
- PlayMode smoke 扩展 `UnityUiExposesMechanismPanelsAndActions`：断言治理总览包含全部 Stage B 组名、政策来源、`Source:`、建筑/政策入口，并检查总览文本 `preferredHeight` 不超过容器高度；同时断言 `SelectionContextText` 仍可见。

### 验证

- `python tools\validate_data.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `git diff --check` 针对 Stage B 修改文件通过，仅提示既有 LF/CRLF 转换。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过；`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=6`、`passed=5`、`failed=0`、`skipped=1`，跳过项仍是 headless 下的 `VisualSmokeCaptureTests`。
- PlayMode 日志复查无 `Using legacy node fallback`、`Missing usable map region shape`、`error CS`、`Native Crash`、`SIGSEGV`、`Assertion` 或 `Exception`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过，`scenarioCount=4`。
- 本阶段未运行图形 VisualSmoke，也未生成 `unity-*.png`；复查 `.outputs/visual` 无 `unity-*.png` 残留。

### 限制与下一步

- Stage B 已进入 Ralph 架构复核；复核通过后才能标记收口进入 Stage C。
- `MechanismPanel` 的外交目标仍沿用旧路径，继续作为 Stage C 前置债务处理。

## 2026-05-04 Strategy Map Dual-Loop Stage B 修复轮

### 目标

补齐 Ralph Architect 对 Stage B 提出的两个缺口：治理面板来源说明必须覆盖 policy、building、governance 三类证据；小视口下必须有直接断言证明治理总览不溢出、不遮挡底部操作按钮和 HUD 选择反馈。

### 已完成

- `RegionPanel` 的治理总览继续覆盖 `Politics`、`Civic`、`Grain`、`Population`、`Risk`、`Building`、`Policy`，并追加 `Governance Source:` 与 `Building Source:`。
- `UISetup` 修正运行时 Text 的 `RectTransform.pivot`，使左上锚定文本的几何边界与视觉位置一致；区域面板底部按钮上移，避免小视口下压出面板边界。
- `GameManagerPlayModeSmokeTests.GovernancePanelKeepsSourcesAndSelectionReadableAtSmallViewport` 增加小视口验证：`RegionPanel`、`SelectionContextText`、`GovernanceOverviewText` 和底部按钮均在边界内；`GovernanceOverviewText` 不遮挡 `PacifyRegionButton`、`BuildRegionBuildingButton`、`CloseButton`；`RegionPanel` 不遮挡 HUD 选择反馈。
- `tools/unity/run_playmode_tests.ps1` 修复验证假阳性：运行前删除旧 XML/log，通过 `Start-Process -Wait` 同步等待 Tuanjie 退出，并正确处理 `My project` 这类带空格项目路径。

### 验证

- `git diff --check -- "My project/Assets/Scripts/UI/RegionPanel.cs" "My project/Assets/Scripts/UI/UISetup.cs" "My project/Assets/Tests/PlayMode/GameManagerPlayModeSmokeTests.cs" "tools/unity/run_playmode_tests.ps1"` 通过。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56 policies=35 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=4`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过；`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=7`、`passed=6`、`failed=0`、`skipped=1`，`GovernancePanelKeepsSourcesAndSelectionReadableAtSmallViewport` 为 `Passed`。
- headless PlayMode 中 `VisualSmokeCaptureTests` 仍按预期跳过，XML skip message 为 `Visual smoke capture requires an active graphics device; skipped under headless or -nographics PlayMode.`。
- 本轮未运行图形 VisualSmoke；复查 `.outputs/visual` 时无 `unity-*.png` 残留。

### 限制与下一步

- Stage B 修复轮等待 Ralph Architect 再复核；通过后进入 Stage C。
- Stage C 明确处理 `MechanismPanel` 仍使用 `FindFirstOtherFaction()` 作为玩家外交目标的问题，改为从 `SelectionContext.selectedRegionId/targetFactionId` 读取邻国目标。

## 2026-05-04 Strategy Map Dual-Loop Stage C 执行

### 目标

执行 Stage C 邻国外交桥：相邻非友方地区选择后先进入外交/边境桥，所有外交、边境和战争模式入口都必须读取 `SelectionContext.selectedRegionId/targetFactionId`，不能再用 `FindFirstOtherFaction()` 作为地图选择动作的目标。

### 已完成

- `MainMapUI.OpenMechanismPanel()` 将当前 `SelectionContext` 传入 `MechanismPanel`，并传入显式进入战争模式的回调。
- `MainMapUI.SetInteractionMode(...)` 现在会按当前选区重建 `SelectionContext`；点击机制面板的 `EnterWarModeButton` 后，同一目标会从 `Diplomacy` 上下文重建为 `War` 上下文，并启用 `dispatch_attack`。
- `MechanismPanel` 新增 `BorderControlButton` 与 `EnterWarModeButton`，保留旧 `Show(...)` 兼容路径。
- `MechanismPanel` 在存在地图选择上下文时优先用 `SelectionContext.targetFactionId/ownerFactionId` 解析目标；无地图选择时才退回旧的 `FindFirstOtherFaction()`，以保持通用机制面板可用。
- 外交桥详情显示选中地区、目标势力、外交状态、外交来源、边境来源、边境成本和“必须显式进入战争模式后才能派遣”的提示。
- `BorderControlButton` 对相邻目标消耗 `money=20`、`food=10`，并调整选中目标外交关系的好感/宿怨；该动作记录 `Border Source:`。
- 新增 PlayMode 用例 `MechanismPanelUsesSelectionContextForDiplomacyBridgeAndWarMode`：选择一个相邻但不属于 first-other 势力的目标，验证边境管控、宣战和进入战争模式都作用于选中目标；legacy first-other 关系保持未宣战。

### 验证

- `git diff --check -- "My project/Assets/Scripts/UI/UISetup.cs" "My project/Assets/Scripts/UI/MechanismPanel.cs" "My project/Assets/Scripts/UI/MainMapUI.cs" "My project/Assets/Tests/PlayMode/GameManagerPlayModeSmokeTests.cs"` 通过。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56 policies=35 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=4`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过；`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=8`、`passed=7`、`failed=0`、`skipped=1`，新增 `MechanismPanelUsesSelectionContextForDiplomacyBridgeAndWarMode` 为 `Passed`。
- PlayMode 日志复查无 `Using legacy node fallback`、`Missing usable map region shape`、`error CS`、`Native Crash`、`SIGSEGV`、`Assertion` 或 `Exception`。
- headless PlayMode 中 `VisualSmokeCaptureTests` 仍按预期跳过；本轮未运行图形 VisualSmoke，复查 `.outputs/visual` 无 `unity-*.png` 残留。

### 限制与下一步

- Stage C 等待 Ralph Architect 复核；通过后进入 Stage D 战争模式作战层。
- 当前边境管控仍是 C# 内轻量动作，来源与成本已在 UI 中显式保留；Stage E 可将该动作迁移为更完整的数据化机制并纳入因果门禁。

## 2026-05-04 Strategy Map Dual-Loop Stage C 修复轮

### 目标

补齐 Ralph Architect 对 Stage C 首轮提出的剩余风险：存在地图选区上下文时，外交桥不得在选中目标无效、己方或不可用时回落到 `FindFirstOtherFaction()`；非相邻敌对地区也不得通过手动切换战争模式绕过邻接门禁。

### 已完成

- `MechanismPanel.ResolveDiplomacyTarget()` 收紧 legacy fallback 边界：只有没有地图选区上下文时才允许使用 `FindFirstOtherFaction()`；只要存在 `SelectionContext.selectedRegionId`，目标势力无效或等于玩家势力就返回 `null`。
- `GameManagerPlayModeSmokeTests.DistantHostileSelectionDisablesDispatchWithReason` 增加非相邻敌对选区验证：默认进入 `Diplomacy`，不暴露 `dispatch_attack` 与 `enter_war_mode`，`AttackButton` 保持禁用，并显示 `dispatch_requires_adjacent_target` 原因。
- 同一用例继续验证手动切换到 `War` 后仍保留非相邻目标上下文，仍不允许派遣攻击，避免 UI 模式切换绕过地图真实邻接规则。

### 验证

- `git diff --check -- "My project/Assets/Scripts/UI/UISetup.cs" "My project/Assets/Scripts/UI/MechanismPanel.cs" "My project/Assets/Scripts/UI/MainMapUI.cs" "My project/Assets/Tests/PlayMode/GameManagerPlayModeSmokeTests.cs" "project-development-report.md" ".omx/state/strategy-map-dual-loop/ralph-progress.json"` 通过，仅提示报告文件既有 LF/CRLF 转换。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56 policies=35 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=4`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过；`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=9`、`passed=8`、`failed=0`、`skipped=1`。
- 新增 `DistantHostileSelectionDisablesDispatchWithReason` 为 `Passed`，既有 `MechanismPanelUsesSelectionContextForDiplomacyBridgeAndWarMode` 仍为 `Passed`。
- PlayMode 日志复查无 `error CS`、`Native Crash`、`SIGSEGV`、`Using legacy node fallback`、`Missing usable map region shape`、`Exception` 或 `Assertion`。
- headless PlayMode 中 `VisualSmokeCaptureTests` 仍按预期跳过，XML skip message 为 `Visual smoke capture requires an active graphics device; skipped under headless or -nographics PlayMode.`。
- 本轮未运行图形 VisualSmoke；复查 `.outputs/visual` 时无 `unity-*.png` 残留。

### 限制与下一步

- Stage C 修复轮进入完整验证与 Ralph Architect 二次复核；通过后标记 Stage C 收口并进入 Stage D。
- 图形截图仍只在有图形设备的 Unity/Tuanjie 环境运行；headless 验证仅确认跳过逻辑与非截图闭环。

## 2026-05-04 Strategy Map Dual-Loop Stage D 执行

### 目标

执行 Stage D 战争模式作战层：战争模式必须继续沿用 `SelectionContext`，并在真实 56 区域地图上显示可操作军队、将领/兵力信息、行军线、目标高亮、接敌标记、占领标记和扩展战报。

### 已完成

- `MainMapUI` 在 `War` 模式下为相邻目标解析 `selectedArmyId`，只有存在可操作玩家空闲军队和可达路线时才暴露 `dispatch_attack`；否则保留可见禁用原因。
- `DemoEntityVisualSpawner` 扩展战争作战覆盖层：
  - `WarRoute_<armyId>` 显示行军路线。
  - `WarTargetHighlight_<regionId>` 显示目标高亮。
  - `WarContactMarker_<engagementId>` 显示接敌状态。
  - `WarOccupationMarker_<regionId>` 显示争夺/占领状态。
  - `ArmyInfo_<armyId>` 显示主将、兵力和任务，不显示无关 idle 军队。
- `BattleResult` 增加战场地区和攻防双方兵力前后字段；Unity 与 headless runner 的 `BattleResult` 定义已同步。
- `BattleReportPanel` 扩展显示兵力变化、占领结果和治理影响；`MainMapUI` 订阅 `RegionOccupied` 与 `GovernanceImpactApplied` 后追加战报。
- 新增 PlayMode 用例 `WarModeOperationalLayerShowsRouteContactOccupationAndBattleReport`：从邻国选区进入机制面板，再显式进入战争模式，验证 `SelectionContext.selectedArmyId/selectedRegionId`、行军线、目标高亮、军队信息、接敌标记、占领标记、战报兵力变化、占领结果和治理影响。

### 验证

- `git diff --check -- "My project/Assets/Scripts/Demo/DemoEntityVisualSpawner.cs" "My project/Assets/Scripts/UI/BattleReportPanel.cs" "My project/Assets/Scripts/UI/MainMapUI.cs" "My project/Assets/Scripts/UI/UISetup.cs" "My project/Assets/Scripts/Domain/Military/DomainBattleSimulationSystem.cs" "My project/Assets/Scripts/Military/BattleResolver.cs" "My project/Assets/Tests/PlayMode/GameManagerPlayModeSmokeTests.cs" ".omx/state/strategy-map-dual-loop/ralph-progress.json"` 通过。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56 policies=35 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 首轮暴露 headless `BattleResult` 字段同步缺口；补齐 `tools/headless_runner/WanChaoGuiYiHeadless/HeadlessBattleTypes.cs` 后重跑通过：`passed=True scenarioCount=4`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过；`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=10`、`passed=9`、`failed=0`、`skipped=1`，新增 `WarModeOperationalLayerShowsRouteContactOccupationAndBattleReport` 为 `Passed`。
- PlayMode 日志复查无 `error CS`、`Native Crash`、`SIGSEGV`、`Using legacy node fallback`、`Missing usable map region shape`、`Exception` 或 `Assertion`；仅有既有授权/LiveLink 诊断噪声。
- headless PlayMode 中 `VisualSmokeCaptureTests` 仍按预期跳过，XML skip message 为 `Visual smoke capture requires an active graphics device; skipped under headless or -nographics PlayMode.`。
- 本阶段未运行图形 VisualSmoke；复查 `.outputs/visual` 时无 `unity-*.png` 残留。

### 限制与下一步

- Ralph Architect 复核返回 `APPROVE`，确认 Stage D 满足 PRD/Test Spec 验收，可正式收口。
- 当前 Stage D 为作战层最小闭环：路线、高亮和标记已接入真实运行时状态，但还不是完整 3D 地形或复杂军队编组 UI。
- 下一步进入 Stage E 因果一致性和来源门禁，继续以 `tools\verify_headless_war.ps1` 作为唯一 headless 因果验收入口。

## 2026-05-04 Strategy Map Dual-Loop Stage E 修复与收口

### 目标

修复 Stage E 因果一致性、历史来源门禁、headless 战争门禁与 UI 状态回归问题。重点是避免反现实因果：急征税压不得提升民心或合法性，征兵必须消耗人口与兵源，行军必须消耗补给，占领必须带来合法性与治理代价，边境管控不得伪装成无成本收益。

### 已完成

- `StrategyCausalRules` 统一承载急征税压、征兵、行军补给、占领合法性、边境管控成本与外交代价规则，并接入 headless runner、治理影响系统、行军系统和机制面板。
- `tools\validate_data.py` 支持自定义 DataDir，并把 `sourceReference` 门禁扩展到 emperor、policy、technology、general、building；同时拦截 `TODO`、`missing`、`placeholder`、`pending source`、`fill later`、`tbd` 等占位来源。
- `tools\verify_headless_war.ps1` 与 `tools\verify_headless_war.sh` 对齐为 `headless_war_causality_scenarios` 八场景合同，并强制校验所有必需 assertion。
- `MainMapUI` 在回合结束、占领、治理影响、机制事件和成功发兵后重建当前 `SelectionContext`，避免占领后继续沿用旧敌对目标、旧 `selectedArmyId` 或旧 AttackButton 状态。
- `GameManager.StartPlayerAttack(string armyId, string targetRegionId)` 校验选中军队归属、空闲状态和路线；UI 发兵路径使用 `SelectionContext.selectedArmyId`，多军队下不会悄悄改用第一支军队。
- `BattleReportPanel` 和 `RegionPanel` 增加负反馈说明：新占领降低合法性、降低税粮贡献、推高民变和地方势力，治理恢复前不能稳定贡献。
- `MechanismPanel` 将边境管控说明修正为成本型外交施压，不再宣称不存在的正向控制收益。
- 修复 `My project/Assets/Scripts/Domain/Core/StrategyCausalRules.cs.meta` 的非法 GUID，恢复 Unity 32-hex meta 合同。
- PlayMode 增加/强化回归：占领后选区重建为治理模式、清除 stale dispatch army、禁用旧攻击按钮；治理小视口先制造低贡献/高风险状态再断言负反馈与来源说明。
- 创建 15 分钟 heartbeat 自动化“万朝 Stage 缺口修复循环”，后续周期按“先找缺口、再修复、再验证、再更新报告”的顺序推进。

### 验证

- `git diff --check` 通过；仅提示既有 LF/CRLF 转换警告。
- `python tools\validate_data.py` 通过，输出 `emperors=13 portraits=13 regions=56 map_region_shapes=56 policies=35 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过，`passed=True scenarioCount=8 failedCount=0`。
- `bash tools/verify_headless_war.sh "E:/万朝归一/万朝归一/My project/Assets/Data" faction_qin_shi_huang` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过，`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=10 passed=9 failed=0 skipped=1`。
- PlayMode 跳过项仍为 headless/nographics 下的 `VisualSmokeCaptureTests`，skip message 为 `Visual smoke capture requires an active graphics device; skipped under headless or -nographics PlayMode.`。
- PlayMode 日志复查未发现 `error CS`、`Native Crash`、`SIGSEGV`、`Using legacy node fallback`、`Missing usable map region shape`、`Unhandled Exception` 或 `Assertion failed`。
- 本轮未运行图形 VisualSmoke；复查 `.outputs/visual` 中 `unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 不存在。

### 限制与下一步

- Ralph Architect 最终只读复核返回 `APPROVE`，确认 Stage E 满足 PRD/Test Spec 验收，可正式收口并进入 Stage F。
- Stage F 应扩展有图形设备环境下的 VisualSmoke 截图合同；截图仍必须运行后清理，不得把临时 `unity-*.png` 留在仓库输出目录。
- 后续优化批次按优先级推进：Stage E 收口、Stage F 视觉截图契约、治理 UI 二次重构、战争压力感、性能与小视口、再考虑 2.5D/3D 地形表达。

## 2026-05-05 Strategy Map Dual-Loop Stage F 收口

### 目标

完成 Stage F 视觉打磨与验证闭环：把 VisualSmoke 从 3 张截图扩展为 7 张截图合同，并确保图形截图只在有图形设备的 Unity/Tuanjie 环境运行，验证后自动删除临时截图与隔离工程副本。

### 已完成

- `VisualSmokeCaptureTests` 扩展到 7 个运行态截图场景：
  - `unity-map-hud.png`
  - `unity-governance-default.png`
  - `unity-region-building-panel.png`
  - `unity-weather-panel.png`
  - `unity-diplomacy-bridge.png`
  - `unity-war-route.png`
  - `unity-battle-report.png`
- VisualSmoke 场景继续使用 `DemoSceneBootstrap` 的真实 56 区域地图、真实 UI、真实 `SelectionContext`、真实发兵路径与战报事件，不创建六边形替代地图。
- `tools\unity\run_visual_smoke_tests.ps1` 的截图校验列表同步扩展到 7 张，并继续检查 `1600x900`、非空、采样颜色足够丰富。
- `docs\unity-handoff-checklist.md` 更新为八场景 headless war 合同和七截图 VisualSmoke 合同。
- Stage F 验证后脚本删除 `.outputs/visual/unity-*.png`，并删除 `.outputs/tuanjie/visual-project-copy`。

### 验证

- `git diff --check -- "My project/Assets/Tests/PlayMode/VisualSmokeCaptureTests.cs" "tools/unity/run_visual_smoke_tests.ps1" "docs/unity-handoff-checklist.md" ".omx/state/strategy-map-dual-loop/ralph-progress.json"` 通过，仅提示既有 LF/CRLF 转换。
- `python tools\validate_data.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过，`passed=True scenarioCount=8 failedCount=0`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过；headless PlayMode 中 `VisualSmokeCaptureTests` 仍按预期跳过，避免 `-nographics` 原生渲染崩溃。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 通过；第一次遇到已知 URP first-import material log 后自动重试，第二次通过。
- 图形 VisualSmoke 截图校验结果：
  - `unity-map-hud.png`：1600x900，174478 bytes，396 sampled colors。
  - `unity-region-building-panel.png`：1600x900，244394 bytes，542 sampled colors。
  - `unity-weather-panel.png`：1600x900，182669 bytes，432 sampled colors。
  - `unity-governance-default.png`：1600x900，246664 bytes，529 sampled colors。
  - `unity-diplomacy-bridge.png`：1600x900，313532 bytes，750 sampled colors。
  - `unity-war-route.png`：1600x900，332457 bytes，819 sampled colors。
  - `unity-battle-report.png`：1600x900，299393 bytes，675 sampled colors。
- 复查 `.outputs/visual` 下 `unity-*.png` 数量为 0。
- 复查 `.outputs/tuanjie/visual-project-copy` 不存在。

### 限制与下一步

- Strategy Map Dual-Loop Stage 0 到 Stage F 已完整收口；当前证明的是 2D/UGUI/运行态视觉闭环，不等于完整 3D 地形。
- 后续优化应从体验缺口继续推进：治理主界面二次重构、战争压力感、地图滑动/缩放手感、图标文字避让、性能与小视口稳定性，再评估 2.5D/3D 地形导出。
- 若后续继续运行 VisualSmoke，仍必须使用图形设备环境，并保持截图和隔离工程副本清理合同。
## 2026-05-05 Post-Stage Optimization Pass: Map Input And Runtime Art Safety

### 目标

继续执行 Stage F 之后的体验缺口修复轮：先通过只读规划与代码 review 找缺口，再优先处理地图滑动/缩放手感、真实 UI 输入可用性、地图点击与 UI 遮挡、以及战争/帝皇运行时素材在构建版中的可加载性。

### 已完成

- 启动并关闭两个只读后台智能体：
  - 后续优化规划智能体给出三条小闭环优先级：治理主界面可读性、战争压力感、地图手感与图标避让。
  - 代码 review 智能体指出 4 个风险：运行时缺少 `EventSystem`、战争实体素材依赖 `Application.dataPath`、区域点击缺少 UI 遮挡保护、相机 bounds 未启用。
- `CameraController` 增加运行时地图边界能力：
  - 自动从 `Generated_Jiuzhou_Map_Background_Runtime` 的 `SpriteRenderer.bounds` 识别整张九州地图边界。
  - `CenterOnRegion`、拖动和平移后的 clamp 统一走边界约束。
  - 缩放 clamp 改为正交视口感知：视口小于地图时限制中心点，视口大于地图时稳定居中。
  - 增加 `ConfigureBounds`、`ConfigureZoomLimits`、`SetZoom`、`ClampToBounds`、`WorldBounds` 等测试入口。
- `UISetup` 在构建运行时自动保证 `EventSystem + BaseInputModule` 存在，避免真实 UGUI 按钮只在测试 `onClick.Invoke()` 中可用。
- `RegionController.OnMouseDown()` 增加 `EventSystem.current.IsPointerOverGameObject()` 保护，避免点击 HUD/面板时把底层地图区域一起选中。
- `DemoEntityVisualSpawner.LoadSprite()` 改为 `Resources.Load` 优先，找不到时才回退 `Application.dataPath`，降低构建版战争覆盖层/军队图标/帝皇头像消失风险。
- 新增 `Assets/Resources/Art/Icons/Units/` 下 8 个单位图标资源副本，以及 `Assets/Resources/Art/Portraits/` 下 13 个帝皇头像资源副本；Unity 已在 PlayMode 导入时生成对应 `.meta`。
- `GameManagerPlayModeSmokeTests` 新增/强化断言：
  - `CameraControllerClampsZoomAndCenterToConfiguredBounds` 验证相机缩放、居中、视口大于边界时稳定居中。
  - `DemoBootstrapCreatesRunnableMapSkeleton` 验证运行时 `EventSystem`、输入模块、单位图标 Resources、帝皇头像 Resources 都存在。

### 验证

- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 首次在沙箱内因 dotnet 首次运行写入用户目录被拒绝失败；常规权限重跑通过：`passed=True scenarioCount=8`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 沙箱内启动 Tuanjie 10 分钟未产生日志或 XML，按 GUI/编辑器权限问题处理；常规权限重跑通过。
- PlayMode XML：`total=11 passed=10 failed=0 skipped=1`。唯一跳过仍为 headless/nographics 下的 `VisualSmokeCaptureTests`，符合截图门禁。
- PlayMode log 复查未发现 `error CS`、`Native Crash`、`SIGSEGV`、`Using legacy node fallback`、`Missing usable map region shape`、`Unhandled Exception`、`Assertion failed` 或 `Test run failed`。
- 本轮未运行图形 VisualSmoke；复查 `.outputs/visual` 中 `unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 不存在。

### 限制与下一步

- 本轮解决了 review 中 3 个立即风险：相机 bounds、运行时 UGUI 输入、build-safe 战争/头像素材；`Application.dataPath` 仍保留为编辑器兜底路径。
- 真实指针驱动的 UI 点击集成测试仍可继续加强，目前回归证明的是运行时事件系统存在与地图点击入口已有 UI 遮挡保护。
- 下一轮建议进入治理主界面二次重构或战争压力感最小闭环：先把“当前最危险项/最有效动作/来源摘要”或“战前补给消耗/占领合法性代价/税粮折损预警”放到玩家决策前。

## 2026-05-05 Post-Stage Governance UI Mode Pass

### 目标

基于 E 盘现有真实 56 区地图与当前 UI，修复“治理/战争模式不够显性、右侧治理栏不能最小化、敌区仍像治理面板、治理信息像调试表”的体验缺口。保持现有整张九州地图与区域边界数据，不创建六边形替代地图。

### 已完成

- 启动并关闭两个只读后台智能体：
  - 后续优化规划智能体输出 1 到 2 周优先级：治理决策前置、模式条、战前压力、缩放分级、标签避让、小视口、2.5D 最小表达。
  - 代码 review 智能体指出 5 个风险，本轮直接修复其中 3 个高优先级风险：敌区右栏模式错误、右栏不可折叠、治理总览过密。
- `UISetup` 增加顶部显式模式入口：
  - `GovernanceModeButton`
  - `WarModeButton`
  - `ModeStateText`
- `UISetup` 将右侧地区栏从 420x680 缩为 360x640，并新增：
  - `CollapseRegionPanelButton`
  - `CollapsedRegionTab`
  - `CollapsedRegionTabButton`
  - `RegionPanelModeText`
- `MainMapUI` 接入顶部治理/战争模式按钮，并在 HUD 与右侧栏同步显示 `治理模式 / 外交过渡 / 战争模式`。
- `RegionPanel` 增加折叠状态：收起后保留窄条，显示当前模式与当前区域；展开后恢复完整右栏。
- `RegionPanel` 按 `SelectionContext.mode` 切换摘要：
  - 友方地区显示治理摘要：本回合摘要、最大风险、治理压力、最优行动、可执行政务、史料来源。
  - 敌方邻区先显示外交过渡摘要，并禁用安抚/建造。
  - 显式进入战争模式后显示战前压力摘要，并继续禁用治理动作。
- `GameManagerPlayModeSmokeTests` 增加/强化回归：
  - `HostileSelectionUsesModeAwareSidebarAndDisablesGovernanceActions` 覆盖敌区外交/战争摘要与治理动作禁用。
  - 机制 UI smoke 覆盖右侧栏收起/展开、折叠条保留模式与区域。
  - 小视口 smoke 覆盖模式按钮、模式文本、折叠条和收起按钮不越界。
- 修复 `Assets/Resources/Art*` 下 25 个非法 Unity meta GUID，统一为 32 位十六进制，恢复数据校验门禁。

### 验证

- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=8`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=12 passed=11 failed=0 skipped=1`。
- PlayMode 唯一跳过仍为 headless/nographics 下的 `VisualSmokeCaptureTests`，skip message 为 `Visual smoke capture requires an active graphics device; skipped under headless or -nographics PlayMode.`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第一次遇到已知 URP first-import material log 后自动重试，第二次通过。
- 图形 VisualSmoke 本轮截图统计：
  - `unity-map-hud.png`：1600x900，178903 bytes，412 sampled colors。
  - `unity-region-building-panel.png`：1600x900，252334 bytes，608 sampled colors。
  - `unity-weather-panel.png`：1600x900，180124 bytes，445 sampled colors。
  - `unity-governance-default.png`：1600x900，254603 bytes，624 sampled colors。
  - `unity-diplomacy-bridge.png`：1600x900，305859 bytes，789 sampled colors。
  - `unity-war-route.png`：1600x900，319019 bytes，876 sampled colors。
  - `unity-battle-report.png`：1600x900，308934 bytes，742 sampled colors。
- 额外手动图形预览重试通过，并复制治理界面预览到 `.outputs/user-preview/governance-ui-preview.png`，SHA256 为 `AFF94098B84435C2EC2BBCFCF2BD278185A8DDA2E138E2C09252C20DCE446200`。
- 复查 `.outputs/visual` 下 `unity-*.png` 数量为 0。
- 复查 `.outputs/tuanjie/visual-project-copy` 不存在。

### 限制与下一步

- 右侧栏仍是 UGUI 固定坐标布局；本轮先收口模式显性与折叠交互，下一轮应继续减少固定坐标债务。
- 治理默认摘要已经从调试表收缩为决策摘要，但史料与来源仍在同一文本块中；后续可拆成可展开的“来源/因果”层。
- 下一轮优先方向：战前补给/占领代价预告、标签避让与缩放分级、小视口 1280x720/1024x576 操作验收、56 区全量 center-hit 校验。

## 2026-05-05 Post-Stage War Forecast / Label Density / Narrow Viewport Pass

### 目标

推进战前补给/占领代价预告、标签避让、缩放分级，以及 1280x720 / 1024x576 小视口操作验收。继续保留现有 56 区域真实九州地图结构，不创建六边形替代地图。

### 已完成

- `RegionPanel` 的战争模式摘要接入战前预告：显示选中军队、路线段数、首回合补给消耗、全程补给上限、接敌/自动结算提示。
- `StrategyCausalRules` 统一占领代价常量：合法性 -2、整合 25%、税粮贡献 35%、民变/地方势力/土地兼并增量。
- 补给不再只是 UI 文案：`DomainBattleSimulationSystem` 在自动战斗力计算中应用低补给惩罚；0 补给战力按 55% 结算，低于 20 补给按 75% 结算。
- `HeadlessSimulationRunner` 新增 `low_supply_reduces_battle_power` 场景，证明低补给可把原本有利的进攻战力从 974 压到 536 并改变战果。
- `MainMapUI` 与 `GameManager.StartPlayerAttack` 收紧发兵规则：战争模式只能使用位于目标相邻、且由玩家控制地区内的空闲前线军队，避免后方任意军队绕过前线压力。
- `CameraController` 暴露 `MapZoomBand`：Detail / Operation / Overview，并在缩放变化时刷新分级。
- `DemoEntityVisualSpawner` 增加标签密度控制：按缩放层级隐藏低优先级标签，并按屏幕像素距离做碰撞避让；修复 `TextMesh.enabled` 编译错误，改为只切换 `MeshRenderer.enabled`。
- `UISetup` 将 HUD 改为双行紧凑布局，保证 headless 画布和 1280x720 / 1024x576 小视口下核心控件仍在画布内且有可用点击尺寸。
- `GameManagerPlayModeSmokeTests` 增强小视口验收：覆盖 1280x720、1024x576，断言展开右栏、折叠右栏、模式按钮、战争按钮、进攻按钮、下一回合按钮、模式文本、选择反馈均不越界。
- 地图命中 smoke 改为从真实 mesh 三角形重心寻找投射点，并在直接触发区域点击前清理残留 EventSystem，避免测试顺序污染。

### 验证

- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 与 Runtime 并行运行时曾因同一 DLL 文件锁失败；该问题是并行构建竞争，Unity PlayMode 编译与 Runtime 单独构建均已通过。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=9`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=14 passed=13 failed=0 skipped=1`。
- 唯一 skipped 仍为 headless / nographics 下的 `VisualSmokeCaptureTests`，符合视觉截图门禁。
- 本轮未运行图形 VisualSmoke；复查 `.outputs/visual` 中 `unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 不存在。

### 限制与下一步

- 本轮完成的是规则和操作验收，不代表战争视觉压力感已经充分。下一轮可继续加强行军线、接敌高亮、战报反馈层级。
- 标签避让当前按缩放层级和屏幕距离即时计算，现有标签数量可接受；后续如果标签数量上升，应增加状态签名缓存。
- 小视口已经有自动验收，但 HUD 仍是 UGUI 坐标布局，后续可继续向布局组件/自适应约束迁移。

## 2026-05-05 Post-Stage War Pressure Visual / Battle Report Closure Pass

### 目标

继续按“先找缺口、再修复、再验证”推进 post-stage 优化。本轮优先修复 code review 暴露的战争压力表达与验收缺口：战前补给压力需要显性进入地图与战报；非占领战报不能停留在“待结算”；56 区真实地图命中不能只抽测 4 区；1280x720 / 1024x576 需要覆盖战争态和战报态。

### 已完成

- 新增 Ralph 上下文快照：`.omx/context/post-stage-war-pressure-20260505T004133Z.md`。
- `StrategyCausalRules` 暴露 `CalculateBattleSupplyPowerPercentForSupply(...)`，让 UI 可直接复用真实低补给战力修正规则。
- `DomainBattleSimulationSystem` 将补给战力修正写入 `BattleResult`：进攻/防守双方最低补给与补给战力百分比会随战斗结果发布；headless 低补给场景增加对应断言。
- `BattleReportPanel` 新增“补给修正”行，并把无占领路径默认收口为“本次战斗未改变地区归属 / 无新增占领治理影响”，不再留下“待结算”占位。
- `MainMapUI` 的战前预告新增接敌补给、战力修正百分比与粗粒度风险等级，避免低补给因果只存在于 headless 场景。
- `DemoEntityVisualSpawner` 的行军线按预计接敌补给变色和变粗，并新增 `WarRoutePressureLabel_*` 地图标签；目标标签优先级高于路线压力标签。
- `DemoEntityVisualSpawner` 的标签避让从中心点距离升级为基于 `MeshRenderer.bounds` 的屏幕矩形碰撞，减少长文本相互压住的风险。
- `GameManagerPlayModeSmokeTests` 将真实地图命中从 `xiyu/liaodong/bashu/jiangdong` 4 区抽测改为遍历全部 56 个 `MapRegionShapeDefinition.regionId`。
- 新增 PlayMode 回归：
  - `BattleReportSettlesNoOccupationPathWithoutPendingPlaceholders`
  - `WarDispatchAndBattleReportStayUsableAtNarrowViewports`
  - 既有战争闭环 smoke 增强路线压力、补给修正与战报字段断言。

### 验证

- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过。
- `git diff --check` 针对本轮触碰文件通过，仅有既有 LF/CRLF 提示。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=9`；`low_supply_reduces_battle_power` 继续证明进攻战力 `974 -> 536`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=16 passed=15 failed=0 skipped=1`。
- 唯一 skipped 仍为 headless / nographics 下的 `VisualSmokeCaptureTests`，skip message 为图形设备门禁，符合要求。
- 本轮未运行图形 VisualSmoke；复查 `.outputs/visual` 中 `unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 不存在。

### 限制与下一步

- 本轮已把战争压力从规则层推到地图/战报/验收层，但视觉仍是 2D overlay，不是完整 2.5D/3D 地形。
- 标签避让已改为矩形碰撞，但区域名标签仍可作为下一轮单独治理对象，特别是全图 overview 下的区域名密度。
- 下一轮建议进入治理主界面决策可读性：把“最大风险 / 最优行动 / 预计变化 / 来源因果”从同一大文本拆成更可扫读的决策层。

## 2026-05-05 Post-Stage Governance Decision Readability / Sidebar Refresh Pass

### 目标

继续按“先找缺口、再修复、再验证”推进治理主界面。重点把右侧治理栏从信息堆叠推进到可决策：先看到最大风险、推荐行动、预计效果、按钮可用状态与因果/来源依据；同时修复 code review 指出的侧栏状态过期与按钮前置条件缺口。

### 已完成

- `RegionPanel` 治理摘要重排为决策首屏，新增并前置 `Decision`、`Recommended`、`Expected`、`Action State`、`Causal`。
- `RegionPanel` 将安抚成本、整合提升、民变下降、地方势力下降提取为统一常量；按钮可用状态和点击执行共用同一组前置条件。
- 安抚按钮现在会在钱粮不足时提前禁用；建造按钮现在会在无可建建筑、资源不足或槽位不足时提前禁用。
- `MainMapUI.UpdateRegionPanelMode()` 增加当前选中侧栏重刷：回合事件、占领事件、治理冲击、政策/外交等机制事件刷新时，会同步重刷归属、整合、民变、土地兼并、来源和摘要；但 `AdvanceTurn()` 隐藏后的侧栏不会被 turn-end refresh 重新打开。
- `GameManagerPlayModeSmokeTests` 增加 `RegionSidebarRefreshesStateChangesAndButtonPrerequisites`，覆盖按钮前置条件、占领/治理冲击后侧栏刷新，以及下一回合关闭侧栏后不被刷新重新打开。
- 保留真实 56 区九州地图和现有生成地图/区域边界；本轮未引入假六边形地点替代。
- 启动并收口两个只读后台智能体：后续规划智能体给出治理决策层优先级；代码 review 智能体指出侧栏刷新与按钮状态缺口，本轮已修复并补回归。

### 验证

- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过。期间一次并行构建遇到 Runtime DLL 文件锁，顺序重跑通过，确认为构建竞争。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过。
- `git diff --check -- "My project/Assets/Scripts/UI/RegionPanel.cs" "My project/Assets/Scripts/UI/MainMapUI.cs" "My project/Assets/Tests/PlayMode/GameManagerPlayModeSmokeTests.cs"` 通过。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=9`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=17 passed=16 failed=0 skipped=1`；唯一 skipped 仍为 headless/nographics 下的 `VisualSmokeCaptureTests` 图形设备门禁。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形 VisualSmoke 通过；第一次遇到已知 URP first-import material log 后自动重试，第二次通过。
- 图形 VisualSmoke 截图统计：`unity-map-hud.png` 421 sampled colors；`unity-region-building-panel.png` 835；`unity-weather-panel.png` 445；`unity-governance-default.png` 866；`unity-diplomacy-bridge.png` 971；`unity-war-route.png` 1057；`unity-battle-report.png` 815。
- 额外生成当前治理界面预览：`.outputs/user-preview/governance-ui-preview.png`，SHA256 `ADA57EED774515A73119E02AAD68A4180B416BBFFAC07F236DD7E6A59CD1E0E8`。
- 清理复查：`.outputs/visual` 中 `unity-*.png` 数量为 0；`.outputs/user-preview` 中临时 `unity-*.png` 数量为 0；`.outputs/tuanjie/visual-project-copy` 不存在；`.outputs/tuanjie/visual-preview-copy` 已删除。

### 限制与下一步

- 当前右侧治理栏仍是固定坐标 UGUI，决策层已前置，但来源/因果仍在同一文本块中；下一轮建议把“决策摘要”和“来源/因果”拆为更明确的视觉层级。
- 当前预览图证明治理决策文本已经进入真实运行 UI，但整体仍偏信息密集；下一轮可继续压缩基础属性区，把决策层做成更像《文明6》的分组面板或可展开详情。
- 后续优先方向：治理动作预告与执行后状态做数值一致性断言、战后新占领区默认治理包、区域名标签在全图 overview 下进一步避让、地图滑动/缩放手感继续打磨。

## 2026-05-05 Post-Stage Governance Source Split / Visual Preview Pass

### 目标

继续治理主界面二次重构：把右侧栏首屏决策摘要与因果/历史来源拆成两个稳定文本层，避免治理栏重新退回一整块调试日志；同时保留现有 56 区真实九州地图、战争/治理模式刷新、小视口验收和图形截图清理纪律。

### 已完成

- `RegionPanel` 新增 `GovernanceSourceText` 绑定，并在 `Show()` 与 `SetMode()` 后统一刷新 `GovernanceOverviewText` / `GovernanceSourceText`。
- `GovernanceOverviewText` 现在只保留首屏决策摘要：`Governance`、`Decision`、`Recommended`、`Expected`、`Action State`、`Politics/Civic`、`Grain/Population`、`Building/Policy`。
- `GovernanceSourceText` 承接 `Causal`、`Negative`、`Governance Source`、`Occupation Source`、`Building Source`、`Policy Source`；战争模式也会显示 `War Source` 与 `Occupation Source`。
- `UISetup.BuildRegionPanel()` 将治理摘要高度压缩为 154，并新增 100 高度的来源详情区，字体降为 10、使用次级文字色。
- `GameManagerPlayModeSmokeTests` 改为分别断言摘要层与来源层，并补充 `GovernanceSourceText` 在 1280x720 / 1024x576 小视口下不越界、不覆盖按钮、不与摘要重叠。
- 后台后续计划智能体已完成并关闭；代码 review 智能体已启动，等待只读审查结果。

### 验证

- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过；仅有既有 Unity 序列化字段 warning。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=9`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=17 passed=16 failed=0 skipped=1`；唯一 skipped 仍为 headless/nographics 下的 `VisualSmokeCaptureTests`。
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形 VisualSmoke 通过；首轮遇到已知 URP first-import material log 后自动重试，第二轮通过。
- 图形 VisualSmoke 截图统计：`unity-map-hud.png` 421 sampled colors，`unity-region-building-panel.png` 903，`unity-weather-panel.png` 445，`unity-governance-default.png` 852，`unity-diplomacy-bridge.png` 1044，`unity-war-route.png` 1140，`unity-battle-report.png` 842。
- 当前治理预览已更新：`.outputs/user-preview/governance-ui-preview.png`，SHA256 `93F4594D7058A4D1A07B24216F1E04A22FAC16ECC3FA9E4A585B5F47DB62481B`。
- 清理复查：`.outputs/visual/unity-*.png` 数量 0；`.outputs/user-preview/unity-*.png` 数量 0；`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 视觉判定

```json
{
  "score": 91,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "治理右栏仍是高密度 UGUI 文本面板，尚未达到完整商业策略游戏的组件化质感",
    "基础属性区仍占据较多首屏空间，后续可继续把决策层做成更强的分组面板"
  ],
  "suggestions": [
    "下一轮将治理动作预告与执行后数值一致性做成专门断言",
    "继续压缩基础属性区，把风险、推荐行动和预计变化放到更高视觉权重"
  ],
  "reasoning": "本轮已把决策摘要和来源/因果拆开，用户能先看到可执行选择，再向下追溯史料与因果。布局在图形 VisualSmoke 与小视口 PlayMode 验收中均通过。"
}
```

### 限制与下一步

- 本轮完成的是治理栏信息层级拆分，不改变 56 区地图、战争规则或历史数据表。
- 右侧栏仍是固定坐标 UGUI；下一轮可继续收敛基础属性区与 HUD 自适应。
- 后续优先方向：治理动作预告与执行结果数值一致性、历史来源门禁统一化、全图 overview 下区域名避让与战争压力层级。

### Code Review Closeout

- 只读代码 review 智能体已完成并关闭；审查范围为 `RegionPanel.cs`、`UISetup.cs`、`GameManagerPlayModeSmokeTests.cs`，结论为 `no findings / APPROVE`。

## 2026-05-05 Strategy UI Market Reference Benchmark Pass

### 目标

继续补齐“市面成功策略游戏 UI 怎么抄”的产品基准：提炼可借鉴的信息架构、地图模式、治理决策面板、战争压力层和全局 outliner，而不是复制受版权保护的素材、商标、图标或像素级布局。

### 已完成

- 新增基准文档：`docs/strategy-ui-reference-benchmarks.md`。
- 参考对象覆盖：
  - 《Civilization VI》：地图 lens、城市/区域 banner、可读经营信息。
  - 《Crusader Kings III》：政治/文化/信仰/地形等 map mode 信息架构。
  - 《Stellaris》：顶栏资源与右侧 outliner 的全局状态管理。
  - 《Total War: Three Kingdoms》：战役层内政、外交、征服和中国题材包装。
  - 《率土之滨》：大地图、行军线、接敌、战报、占领压力。
  - 《三国志·战略版》：行军、视野、拦截、路线风险。
- 为本项目落地为 5 个 UI 方向：
  - 治理首屏决策条。
  - 风险/经济/合法性卡片。
  - 56 区真实地图 lens bar。
  - 战争路线压力层。
  - 可折叠全局 outliner。

### 验证

- 文档已写入项目 `docs/`，与当前治理/战争双闭环和 56 区真实地图约束一致。
- 本轮为设计基准文档，无代码执行变更；无需运行 Unity 测试。

### 下一步

- 优先把 `docs/strategy-ui-reference-benchmarks.md` 中的第 1 项落地为 PlayMode 可验收：治理动作预告与执行结果数值一致性。

## 2026-05-05 Heavy Strategy Systems Planning Pass

### 目标

用户选择 C：重度借鉴成功策略游戏玩法实践。目标不是继续只改 UI，而是制定下一阶段系统计划，把地区专精、治理预告、新占领控制链、连地出征、补给、视野、拦截、粮食-军队-秩序-外交联动、地图 lens 和 outliner 系统化。

### 已完成

- 新增上下文快照：`.omx/context/heavy-strategy-systems-plan-20260505T0218Z.md`。
- 新增 PRD：`.omx/plans/prd-heavy-strategy-systems.md`。
- 新增测试规格：`.omx/plans/test-spec-heavy-strategy-systems.md`。
- 计划阶段切分为：
  - H0：数据契约与来源门禁扩展。
  - H1：地区专精与治理动作预告。
  - H2：新占领控制链。
  - H3：连地出征、补给节点、视野与拦截。
  - H4：粮食、补给、秩序与外交联动。
  - H5：地图 lens 与 outliner。
  - H6：集成闭环与视觉证据。

### 依据

- 现有代码已具备承接基础：56 区真实地图、`RegionState` 整合度/民变/地方势力/税粮贡献/占领状态、`ArmyRuntimeState` 补给/路线/任务/接敌、`SelectionContext`、`MapZoomBand`、战争路线补给压力、战报与新占领治理折损。
- 新计划不推翻 Stage 0-F，而是在已完成双闭环上继续扩展。

### 验证

- 本轮为计划文档变更，无 gameplay 代码执行变更；未运行 Unity。
- 已检查计划文件存在并写入 `.omx/plans/`。

### 待用户判断

- 是否按 H0-H6 阶段顺序推进。
- 第一轮执行是否只做 H0+H1，还是 H0-H2 一起做。
- H3 的视野/拦截第一版是否采用确定性规则，后续再加概率。

## 2026-05-05 Heavy Strategy Systems H0-H6 Execution / Verification Pass

### 目标

用户已认可 H0-H6 阶段顺序，并要求不分批等待、直接全部完成。本轮按“先找缺口、再修复、再验证”推进重策略系统完全闭环收口：保留现有 56 区真实九州地图，补齐治理/战争双闭环里最容易漂移的规则、UI、地图 lens、outliner、小视口与视觉证据门禁。

### 已完成

- 修复 `RegionPanel` 治理因果漂移：`Recommended` 与 `Expected` 使用 `StrategyMapRulebook.BuildRecommendedGovernanceForecast()`，新占领区首步明确推荐 `军管`，不再被高民变/低整合硬编码短路成 `安抚`。
- 保留稳定地区的显式安抚入口：右侧按钮优先执行规则推荐动作；当规则建议 `Hold` 时仍可作为手动安抚动作，避免旧治理入口被误删。
- 修复新占领控制链 UI/执行一致性：新增 PlayMode 断言 `OccupiedGovernancePanelRecommendsMilitaryGovernBeforePacify`，验证 `NewlyAttached -> MilitaryGoverned`、钱粮与合法性成本、民变压制均真实发生。
- 修复右侧 outliner 小视口重叠：地区详情展开时，`StrategyOutlinerPanel` 自动隐藏，`StrategyOutlinerCollapsed` 移到地区栏左侧，不与 `RegionPanel` 重叠；地区栏未展开时 outliner 可继续展开。
- `UISetup` 清理运行时生成的 `StrategyLensBar`、`StrategyOutlinerPanel`、`StrategyOutlinerCollapsed`，避免多轮 PlayMode/场景重建时拿到旧 UI 对象。
- `RegionMeshBuilder` 已拒绝自交/零面积/重复边界点，耳切失败返回空三角而不是静默 fan fallback；新增 `RegionMeshBuilderRejectsInvalidPolygonsInsteadOfUsingFanFallback`。
- 扩展 headless 硬门禁：`tools/verify_headless_war.ps1` 与 `.sh` 现在要求至少 13 个场景，并纳入 H1-H4 场景：
  - `region_specialization_and_governance_forecasts`
  - `occupation_control_chain_progression`
  - `connected_campaign_vision_and_interception`
  - `relief_and_tax_pressure_causality`
  - 同时补入 `low_supply_reduces_battle_power`
- 新增 H5 PlayMode 断言：`StrategyOutlinerEntriesSelectRegionsAndAvoidExpandedSidebar` 覆盖 outliner 条目点击选区、自动折叠和 1024x576 不重叠。
- 更新 `.omx/plans/prd-heavy-strategy-systems.md` 与 `.omx/plans/test-spec-heavy-strategy-systems.md` 的执行状态：H0-H6 已获用户批准并完成完全闭环硬门禁。

### H0-H6 对应收口

- H0：数据仍保持 `regions=56 map_region_shapes=56`，新增字段由现有数据/默认派生承接，数据和 source gate 验证通过。
- H1：所有 56 区 specialization 解析进入 headless 门禁，治理 forecast 与实际 pacify delta 对齐。
- H2：新占领链条进入 headless 与 PlayMode 双重门禁，UI 不再跳过军管阶段。
- H3：连地/补给节点/视野/拦截 forecast 进入 headless 门禁。
- H4：赈济与急征税压因果进入 headless 门禁，避免“加税但民心上涨”等反现实漂移。
- H5：地图 lens 与 outliner 的可点击、折叠、小视口避让进入 PlayMode 门禁。
- H6：PlayMode 与图形 VisualSmoke 都已通过，截图临时文件和临时工程副本已清理。

### 验证

- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过。期间并行构建曾因同一 Runtime DLL 文件锁失败，改为串行重跑后通过，判定为构建竞争而非代码错误。
- `python tools\validate_data.py "My project\Assets\Data"` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=13`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=20 passed=19 failed=0 skipped=1`；唯一 skipped 为 headless/nographics 下的 `VisualSmokeCaptureTests` 图形设备门禁。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过。
- VisualSmoke 图形截图契约共 12 张，均为 `1600x900`，采样色数满足非空画面检查：
  - `unity-map-hud.png`
  - `unity-region-building-panel.png`
  - `unity-weather-panel.png`
  - `unity-governance-default.png`
  - `unity-governance-forecast.png`
  - `unity-occupation-chain.png`
  - `unity-war-route-risk.png`
  - `unity-map-lens-risk.png`
  - `unity-outliner.png`
  - `unity-diplomacy-bridge.png`
  - `unity-war-route.png`
  - `unity-battle-report.png`
- 清理复查：`.outputs/visual/unity-*.png` 数量为 0；`.outputs/tuanjie/visual-project-copy` 不存在。

### 限制与下一步

- H0-H6 已完成可运行完全闭环与门禁；这代表当前 MVP 验收闭合，但不代表玩法深度已经达到最终商业策略游戏水平；下一轮应继续把治理主界面做成更清晰的可决策组件，而不是继续堆文本。
- 地图仍是 2D/2.5D 表达，3D 地形可行性尚未进入真正渲染实现；后续可以在不替换 56 区边界的前提下做地形高度、阴影、路径层级和选区高亮。
- outliner 已能折叠和点击，但条目分组/优先级仍偏 MVP；后续可继续深化“高风险地区、行军军队、待处理政策、新占领地区、战报”的分组和筛选。

## 2026-05-05 Heavy Strategy Systems H0-H6 Full Closure Hard Gate

### 目标

回应“不是初版闭环而是完全闭环”的验收要求：本轮不再把 H0-H6 标记为初版闭合，而是补齐 PlayMode 测试隔离、重新运行全量规则/视觉门禁，并把证据写回报告与 `.omx` 状态。

### 已修复

- 修复 `GameManagerPlayModeSmokeTests.AllStrategyLensesSwitchStateAndReuseRegionSurfaces` 对 Demo bootstrap 运行时对象的隔离问题。
- 根因：`DemoSceneBootstrap` 会创建独立的 `GameManager`、`MapRoot`、`MapRenderer`、`EntityVisuals`、`GameCanvas`、`UISetup`、`Main Camera`、`EventSystem`。旧 lens 测试只销毁 `RegionController` 子对象，留下空壳 `MapSetup`，后续 `DemoBootstrapCreatesRunnableMapSkeleton` 复用空壳时得到 0 个真实区域面片。
- 新增统一测试清理函数 `DestroyDemoBootstrapRuntimeObjects()`，让 Demo bootstrap 相关测试回收整套运行时对象，避免后续测试复用失效对象。
- `DemoBootstrapCreatesRunnableMapSkeleton` 的日志订阅现在通过 `try/finally` 解除，并在 finally 中清理 Demo bootstrap 运行时对象，避免失败路径继续污染后续用例。
- 保留现有 56 区真实地图结构，没有替换为六边形、方格或临时假区域。

### 完全闭环证据

- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过；仅保留既有 Unity 序列化字段 warning，0 error。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过；0 error。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`，覆盖 H1 代表区专精、H2 占领控制链、H3 连地/补给/视野/拦截、H4 粮食/补给/秩序/外交联动。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`.outputs/tuanjie/wanchao-playmode-results.xml` 显示 `total=21 passed=20 failed=0 skipped=1`。唯一 skipped 为图形设备门禁下的 `VisualSmokeCaptureTests`，符合 headless PlayMode 预期。
- 原失败用例 `DemoBootstrapCreatesRunnableMapSkeleton` 已通过；新增 lens 用例 `AllStrategyLensesSwitchStateAndReuseRegionSurfaces` 也通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 在图形 Tuanjie 环境通过；第 1 次遇到已知 URP first-import material log 后自动重试，第 2 次通过。
- 图形 VisualSmoke 校验 12 张 `1600x900` 截图非空并具备足够采样颜色：
  - `unity-map-hud.png`
  - `unity-region-building-panel.png`
  - `unity-weather-panel.png`
  - `unity-governance-default.png`
  - `unity-governance-forecast.png`
  - `unity-occupation-chain.png`
  - `unity-war-route-risk.png`
  - `unity-map-lens-risk.png`
  - `unity-outliner.png`
  - `unity-diplomacy-bridge.png`
  - `unity-war-route.png`
  - `unity-battle-report.png`
- 清理复核：`.outputs/visual/unity-*.png` 数量为 0；`.outputs/tuanjie/visual-project-copy` 不存在；`.outputs/tuanjie/visual-preview-copy` 不存在。

### 状态结论

- Heavy Strategy Systems H0-H6 现在标记为 `completed-full-closure`，不再使用“initial closure”作为完成口径。
- 完全闭环含义：当前 MVP 范围内的数据门禁、因果门禁、56 区真实地图门禁、治理/战争双闭环、PlayMode 小视口与 lens/outliner 门禁、图形 VisualSmoke 与截图清理门禁均有本轮新验证证据。
- 剩余风险不属于本阶段未闭合项，而是下一阶段继续优化项：治理主界面组件化、战争压力层更强表达、2.5D/3D 地形表现、outliner 分组优先级、地图缩放/拖动手感与性能剖析。

## 2026-05-05 Review Findings Fix Pass

### 目标

修复 PR review 指出的 6 个缺口：默认开局绕过占领/侦察闭环、事件面板无操作出口、地图 lens 不随治理状态刷新、PlayMode runner 不校验 XML 语义、Unity 交接文档与当前门禁脱节、Unity Test Runner 临时场景被提交。

### 已修复

- 默认开局不再按 `index % factions.Count` 撒点。`GameStateFactory` 现在按历史启发的连续区块分配 56 区，玩家秦系核心位于关中/陇西/长安/咸阳/雍州，并保持各势力领土连通。
- 初始 visibility 改为玩家自有区与邻接敌区 `Known`，非邻接敌区 `Hidden`；`ScoutIntel` 成功后只把指定地区标为 `Scouted`，并同步 runtime region。
- `MechanismPanel` 的真实 UI “刺探情报”入口会把当前选中敌区作为 `targetEntityId` 传入；没有选区时也只兜底侦察一个目标区，避免一次性揭示整势力。
- `EventPanel` 增加 3 个选择按钮和关闭按钮，按钮绑定 `ChooseOption(index)` 与 `Hide()`，事件弹出后不再阻断玩家。
- `MapRenderer` 订阅 `GovernanceImpactApplied`、`RegionOccupied`、`TurnEnded`、`PolicyApplied`、`WeatherChanged`、`TechResearched`、`EspionageOperationCompleted`，治理/战争/侦察变化后 lens 会刷新。
- `tools/unity/run_playmode_tests.ps1` 与 `.sh` 解析 PlayMode XML 的 `total/passed/failed/skipped`，要求 `failed=0`、`total>0`、`passed>0`，且只允许唯一 skip 来自 `VisualSmokeCaptureTests`。
- `docs/unity-handoff-checklist.md` 同步为 headless `scenarioCount=14` 与 12 张 VisualSmoke 截图清单。
- 删除 `My project/Assets/InitTestScene*.unity(.meta)` 临时场景，并在 `.gitignore` 增加忽略规则。

### 新增/更新验证

- 新增 PlayMode `DefaultStartUsesContiguousOwnersAndFogOfWar`：验证开局连续领土、邻接敌区 Known、远方敌区 Hidden、侦察后仅目标区 Scouted。
- 扩展 PlayMode `MechanismPanelUsesSelectionContextForDiplomacyBridgeAndWarMode`：验证真实 `EspionageActionButton` 路径携带选区、只侦察目标区，不揭示整势力。
- 扩展 UI smoke：验证事件面板选择按钮可处理事件、关闭按钮可退出。

### 验证结果

- `git diff --check` 通过。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `dotnet build "tools\headless_runner\WanChaoGuiYiHeadless\WanChaoGuiYiHeadless.csproj"` 通过，0 warning / 0 error。
- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过，0 error；保留既有 Unity 序列化字段 warning。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 warning / 0 error。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；12 张截图均为 `1600x900` 非空渲染。
- 清理复核：`.outputs/visual/unity-*.png` 数量 0；`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### Review 复核

- 只读 review 智能体第一轮指出 UI 侦察入口仍会整势力揭示、事件关闭按钮文案异常；本轮已补 UI 入口目标区传递、底层单区兜底与 PlayMode 真实按钮断言，并修正关闭按钮文案。
- 最终只读 review 智能体复核结论为 `APPROVE`，审查范围覆盖 `MechanismPanel.cs`、`EspionageSystem.cs`、`GameManagerPlayModeSmokeTests.cs`、`UISetup.cs`，0 findings。

## 2026-05-05 Outliner Grouping Optimization Pass

### 目标

继续在 H0-H6 完全闭环之后寻找可验证缺口。本轮选择 outliner：它已经能折叠、点击和避让右侧栏，但仍只是按优先级排出的 `Top actions`，没有把“高风险地区、行军军队、新占治理、最新战报”显性分组，玩家扫读压力不够。

### 已修复

- `StrategyOutlinerEntry` 增加 `groupId`、`groupLabel`、`groupPriority`，让 outliner 不只按单条优先级排序，也能先按军政压力类型分组。
- `StrategyMapRulebook.BuildOutliner()` 将高民变/高地方势力与低民心归入“高风险地区”，占领链归入“新占治理”，行军/低补给归入“行军军队”，最近 war/rebellion/event/diplomacy 日志中能匹配地区的记录归入“最新战报”。
- `MainMapUI` 将 outliner 标题改为“军政待办”，摘要区显示当前 lens、总条目数和前 4 个分组计数；按钮标签带分组名前缀，降低玩家看见一串内部 category 的调试感。
- PlayMode `StrategyOutlinerEntriesSelectRegionsAndAvoidExpandedSidebar` 扩展为同时制造高风险区、新占治理区、低补给行军军队和地区战报，断言四类分组摘要出现、最高优先级条目带“高风险地区”，并继续验证 1024x576 点击后右栏折叠避让。

### 验证结果

- `git diff --check` 通过。
- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过；仅保留既有 Unity 序列化字段 warning。并行验证时曾出现 Runtime obj 文件锁 warning，串行重跑 PlayModeTests 后无 warning / 0 error。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 串行通过，0 warning / 0 error。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 仍是 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- 未运行 VisualSmoke；本轮只改 outliner UI 文本/按钮分组和 PlayMode 验收，不生成截图。清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。
