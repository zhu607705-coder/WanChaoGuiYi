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
- 2026-04-30：地图主导战争闭环 Ralph Iteration 3 完成：新增 `GameManager.RunSingleLaneWarSmokeTest()` 作为 Unity 内单路闭环验收入口；`MapState.RemoveArmy()` 与 `MapWarResolutionSystem` 的败方撤退/溃散处理已落位，战后败军会撤往同势力相邻地区或从 runtime/legacy 军队列表移除，降低重复接敌风险；`ArmyMovementSystem` 允许 `ArmyTask.Retreat` 的接敌军队脱离并写日志，`MapCommandService` 为撤退和增援补充可解释日志；旧 `BattleResolver.Resolve()` 默认不再直接改地区归属，只有显式开启 `allowLegacyOwnershipChange` 时才保留 legacy 改归属路径。
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
- 下一步建议把这 4 个场景拆为更接近测试框架的独立断言输出，或进入 Unity 手动验收 `RunSingleLaneWarSmokeTest()` 与 UI 日志/地图颜色同步。

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
  - 运行 `RunSingleLaneWarSmokeTest()` 和 `NextTurn()`，断言出现 `战斗结束` 与 `收入 金钱`。
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
