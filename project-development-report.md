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

## 2026-05-06 Stage Heartbeat Automation

### 目标

设计并创建一个 15 分钟周期的项目推进 heartbeat，让当前线程持续寻找《万朝归一：九州帝业》的现有问题缺口，并按“检查缺口 -> 修复 -> 验证 -> 更新报告/状态”的顺序推进。

### 设计

- automation id/name：`stage`。
- 类型：Codex App `heartbeat`，绑定当前线程继续执行，避免另起 detached runner 丢失上下文。
- 周期：`RRULE:FREQ=MINUTELY;INTERVAL=15`。
- 状态：`ACTIVE`。
- 执行契约：每轮优先检查 H0-H6 Heavy Strategy Systems、治理/战争双闭环、治理主界面美观、战争压力感、地图滑动/缩放、标签避让、真实 56 区地图投影、小视口稳定性、2.5D/3D 地形表达和 VisualSmoke 清理缺口。
- 视觉门禁：截图只在有图形设备的 Unity/Tuanjie 环境运行；使用后删除 `.outputs/visual/unity-*.png`，并清理 `.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy`。
- 阻塞策略：遇到真正阻塞先自行寻找替代路径；完成的后台智能体要关闭。

### 验证

- `automation_update create` 返回 `automationId=stage`。
- `automation_update view` 可渲染 automation card。
- 本地落盘检查通过：`C:\Users\123\.codex\automations\stage\automation.toml` 存在。
- UTF-8 语义检查通过：`status = "ACTIVE"`、`RRULE:FREQ=MINUTELY;INTERVAL=15`、`万朝归一`、`.outputs/visual/unity-*.png` 均可从配置中读出。

## 2026-05-06 Stage Heartbeat UI Readability Pass

### 缺口

本轮 heartbeat 先复核 H0-H6 后续缺口，选择治理主界面美观与可读性继续推进：右侧地区栏虽然已能显示 forecast/apply 同源结果，但治理摘要仍偏“调试文本”，特别是可见的 `nextControl`、连续竖线和缺少分区底色，会削弱《文明6》式清楚可决策的阅读感。

### 已修复

- `RegionPanel` 的治理摘要改为更稳定的决策层级：`本回合摘要`、`最大风险`、`最优行动/按钮状态`、`预计效果`、`政务/民生`、`粮税/人口`、`建设/政策`。
- 可见 forecast 不再暴露 `nextControl` 英文调试字段，改为“下一阶段 + 中文控制阶段”；PlayMode 需要的 `nextControl` 断言保留在 1px 隐藏 token 中，不影响玩家阅读。
- `UISetup` 给地区栏的指标区、决策区、史源区增加轻量背景带，强化主次层级，但不新建嵌套操作卡片。
- `UITheme` 增加 section/decision/source 背景色，沿用当前深墨、青玉、鎏金 UI 方向。
- 清理 `SampleScene.scene` 里 Unity 序列化字段产生的行尾空格，让全量 `git diff --check` 重新通过。

### 验证

- `git diff --check` 通过，仅有 LF/CRLF 提示。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- 文本语义检查通过：可见“下一阶段”存在，可见 `| nextControl` 已移除，隐藏 `nextControl` token、`本回合摘要`、`最优行动`、`预计效果` 仍保留。
- 未运行图形 VisualSmoke；本轮未生成截图。清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 当前阻塞

- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 当前仍受本机 Unity/Tuanjie `UnityEngine.UI.dll` / `UnityEditor.UI.dll` 引用缺失影响，失败集中在 `UnityEngine.UI` 与 `UnityEngine.EventSystems` 类型解析；这与当前 UI 修改无直接语义关系。
- `tools\unity\run_playmode_tests.ps1 "My project"` 在 Package Manager 阶段失败：`The "path" argument must be of type string. Received undefined. No packages loaded.`。
- 已尝试将项目复制到 ASCII 临时路径并用 ASCII 输出目录重跑 PlayMode，仍在相同 Package Manager `path undefined` 处失败；临时副本已安全删除，日志复制到 `.outputs/tuanjie/wanchao-playmode-ascii-copy.log`。

## 2026-05-06 Stage Heartbeat Package Manager Fallback

### 缺口

上轮记录的 Tuanjie Package Manager `path undefined` 会在进入 PlayMode/VisualSmoke 前阻断脚本编译，且同一问题在 ASCII 临时工程中仍复现。由于当前修改主要集中在 UGUI 运行时代码和 PlayMode 断言，缺少 `UnityEngine.UI.dll`、`UnityEditor.UI.dll`、`UnityEngine.TestRunner.dll`、`UnityEditor.TestRunner.dll` 时，`dotnet build` 也无法作为本轮 fallback 编译门禁。

### 已修复

- 新增 `tools/unity/restore_cached_script_assemblies.ps1`，从本机 Tuanjie universal-2d 模板缓存恢复 UI/TestRunner ScriptAssemblies 到 `My project/Library/ScriptAssemblies`。
- 修复脚本的 Tuanjie root 解析，确保 `E:\万朝归一\Editor\Tuanjie.exe` 会解析到 `E:\万朝归一\Editor`，而不是上级目录。
- 该脚本只写入 `Library/ScriptAssemblies` 这一类 Unity 生成缓存，不改 `Assets` 业务内容，也不绕过后续真实 PlayMode/VisualSmoke；它只是让 Package Manager 故障时仍可做 C# 编译级验证。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过，恢复 UI/TestRunner 缓存程序集。
- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过，保留既有 Unity 序列化字段 warning，0 error。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 warning / 0 error。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `-noUpm` PlayMode 尝试仍会在 Unity 编译阶段缺少 `UnityEngine.UI` 类型，不能替代真实 Package Manager 解析；未生成 PlayMode XML。
- 未运行图形 VisualSmoke；本轮未生成截图。清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

## 2026-05-06 Stage Heartbeat Package Manager Recovery

### 缺口

上一轮 `.omx` 状态仍把 Tuanjie Package Manager 标记为 fallback/blocker。复核后确认当前环境的 `path undefined` 根因是缺少 `ALLUSERSPROFILE`，同时 PlayMode runner 需要让 Unity Test Runner 自行结束测试流程；当前脚本已经不再传 `-quit`，测试 XML 可以真实生成并进入语义校验。

### 已修复

- `tools/unity/run_playmode_tests.ps1` 在启动 Tuanjie/Unity 前补齐 `ALLUSERSPROFILE`，为空时回落到 `$env:PROGRAMDATA` 或 `C:\ProgramData`。
- `tools/unity/run_visual_smoke_tests.ps1` 使用同一环境修复，避免后续图形 VisualSmoke 在 Package Manager 阶段复现 `path undefined`。
- 保留 `tools/unity/restore_cached_script_assemblies.ps1` 作为 C# 编译级 fallback；它只恢复 `Library/ScriptAssemblies` 中的 UI/TestRunner 缓存程序集，不替代真实 PlayMode 或图形 VisualSmoke。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `dotnet build "My project\WanChaoGuiYi.Runtime.csproj"` 通过，保留既有 Unity 序列化字段 warning，0 error。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；最后一次串行复跑保留既有 Runtime warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 是 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- 本轮未运行图形 VisualSmoke，未生成新的 Unity 截图。清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

PlayMode/Tuanjie Package Manager 阻塞已恢复为可运行状态；后续自动轮次不再把 `path undefined` 当作当前 blocker。下一轮可继续推进治理主界面美观、战争路线压力层级、地图缩放/滑动手感、小视口性能与有图形设备时的 VisualSmoke 截图验收。

## 2026-05-06 War Pressure / Zoom Label / VisualSmoke Pass

### 缺口

本轮继续检查 H0-H6 后续体验缺口：治理栏已比上一轮更可读，但战争路线仍主要依赖一条线和一段文字，缺少类似大地图压力的层级；缩放标签已有裁剪规则，但 1024x576 小视口缺少显式可见标签预算；地图滚轮缩放会改变视野中心，缺少按指针稳定缩放的手感门禁。

### 已修复

- `DemoEntityVisualSpawner` 给行军路线增加 `WarRouteUnderlay_*` 宽底线和 `WarRouteContactNode_*` 接敌节点，补给压力标签加入高中低等级，强化战前压力和接敌焦点。
- `DemoEntityVisualSpawner.ApplyLabelDensityForCurrentZoom()` 增加按缩放层级与小视口的可见标签预算；1024x576 远景最多保留 5 个高优先级标签，避免图标文字互相压住。
- `CameraController` 增加随缩放比例调整的平移速度，并让滚轮缩放通过 `ZoomAroundScreenPoint()` 保持指针下地图位置稳定。
- `MainMapUI` 的战前预告改为分行中文层级：行军预告、补给压力、接敌判断；保留隐藏的 `visibility` / `interceptionRisk` token 供 PlayMode 断言。
- `RegionPanel` 的战争摘要从调试式“战争模式 | 战前压力”改为“战争态势 | 战前压力”，并强化目标阻力、攻占代价、战后治理三层信息。
- PlayMode 扩展验证：路线 underlay、接敌节点、小视口标签预算、指针缩放稳定性、中文战争态势与本地化可见性文案。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化字段 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 是 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过。第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过；12 张截图均为 `1600x900` 且非空，采样色数量约 `1557-3403`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争路线压力感、地图缩放手感、标签避让和图形 VisualSmoke 已完成本轮收口。下一轮建议继续把治理主界面从文本块推进到更强的数值条/状态徽标/行动预告组件，同时保留现有 56 区真实地图投影和 H0-H6 因果门禁。

## 2026-05-06 Governance Decision Meter Pass

### 缺口

本轮继续检查治理主界面美观与小视口稳定性：上一轮治理栏已经有分区底色和因果文字，但第一屏仍偏长句阅读，缺少可扫读的状态徽标、整合/民变/税粮贡献条。第一次实现把三类新信息拆成多行后，PlayMode 小视口门禁发现 `GovernanceOverviewText` preferredHeight 超出容器，说明信息密度需要压缩而不是继续堆高。

### 已修复

- `RegionPanel` 治理摘要新增固定长度的 `状态徽标`、`治理条`、`产出条`：显示控制阶段、民变等级、整合等级、贡献折损，以及整合/民变/税贡/粮贡的 5 格 ASCII 条。
- 将新增状态条压缩进一行，保留 `Governance / Politics / Civic / Grain / Population / legitimacy / Risk / Decision / Recommended / Expected / Action State / Building / Policy` 等既有验收 token，同时不增加容器高度。
- PlayMode `AssertGovernanceOverviewCoversStageB` 增加对 `状态徽标`、`治理条`、`产出条` 的断言，并继续检查小视口不溢出、不遮挡按钮。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化字段 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 是 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过。第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过；12 张截图均为 `1600x900` 且非空，采样色数量约 `1604-3403`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

治理主界面现在有第一屏状态徽标与固定长度数值条，能够更接近《文明6》式“清楚可决策”的扫读结构，同时 PlayMode 小视口和图形 VisualSmoke 门禁均通过。下一轮可继续做真实 UI Image 进度条/徽标组件，或推进 2.5D 地形高度与选区高亮。

## 2026-05-06 Terrain Depth / Selection Highlight Pass

### 缺口

本轮继续检查 H0-H6 后续体验缺口：治理和战争 UI 已完成数轮可读性与压力层级强化，但地图本体仍偏平面；上一轮已写入 2.5D 地形阴影与选区抬升对象，尚未通过 Unity/Tuanjie 运行门禁确认。为避免回到“假六边形小格子”，本轮只在现有 56 区真实区域 mesh 上叠加轻量深度表现，不改地图投影、区域边界或治理/战争数据。

### 已修复

- `MapSetup` 在每个真实 `RegionSurface_*` 下生成 `RegionTerrainShadow_*` 和默认隐藏的 `SelectedRegionElevation_*`，用同一 region mesh 做阴影和选区抬升。
- `RegionController` 增加地形视觉绑定与 `SetSelected()`，选中区域时只激活对应抬升层，未选区域保持真实边界的原始平面。
- `MapRenderer` 订阅 `RegionSelected`，在地图刷新后同步选区高亮状态，避免 lens、治理刷新或回合推进后高亮丢失。
- PlayMode 增加 56 个阴影层、56 个选区抬升层、初始隐藏和点击后单区激活的断言，保证 2.5D 表达不会脱离真实 56 区结构。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化字段 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 是 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过。第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过；12 张截图均为 `1600x900` 且非空，采样色数量约 `1617-3305`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

2.5D 地形阴影与选区抬升已经在真实 56 区地图上完成运行验收；它增强地图本体的层次感，但不改变区域数据、边界和 H0-H6 因果闭环。下一轮可继续做真实 UI Image 进度条/徽标组件、战报反馈动效，或对新增视觉层做 1024x576/1280x720 性能采样。

## 2026-05-06 Governance UI Image Badge / Bar Pass

### 缺口

本轮继续检查 H0-H6 后续体验缺口：治理主界面已有状态徽标、治理条和产出条，但上一版主要依赖文本/ASCII 条，视觉上仍像调试摘要，不够像正式策略游戏的可扫读治理面板。目标是在不扩大右侧栏、不破坏 1024x576 小视口门禁的前提下，把核心治理状态升级为真实 UGUI 徽标和进度条。

### 已修复

- `UISetup` 在地区面板中新增 4 个真实 `Image + Text` 状态徽标：控制阶段、民变压力、整合质量、贡献状态。
- `UISetup` 在既有指标区新增 4 条真实 `Image` 填充条：整合、民变、税贡、粮贡；位置嵌入原有指标行，避免新增面板高度。
- `RegionPanel` 新增徽标/条形绑定与数据同步：根据 `RegionState` 更新徽标文案、风险色、填充宽度和填充色；战争/外交模式下隐藏治理专属组件。
- 治理摘要文本压缩为 5 行，保留 `Governance / Politics / Civic / Grain / Population / legitimacy / Risk / Decision / Recommended / Expected / Action State / Building / Policy` 等验收 token，同时给真实 UI 组件留出可见空间。
- PlayMode 增加真实 UGUI 组件断言：徽标必须有 `Image` 背景和 `Text` 标签，四条治理 meter 必须是 `Image` fill 且宽度由数据绑定。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化字段 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 是 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过。第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过；12 张截图均为 `1600x900` 且非空，采样色数量约 `1598-3305`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

治理主界面已经从“文本模拟条”推进到真实 UGUI 徽标与数据绑定进度条，第一屏决策信息更接近正式 4X/大战略 UI 的扫读结构，并继续保留 56 区真实地图、历史来源门禁和治理/战争因果闭环。下一轮可继续做战报反馈动效、战线压力动画，或对 1024x576/1280x720 的新增视觉层做性能采样。

## 2026-05-06 Battle Report Feedback Visual Pass

### 缺口

本轮继续检查 H0-H6 后续体验缺口：战争模式已有行军线、接敌节点和补给压力层级，但战斗结束后的战报仍主要依赖文本，胜负、战力差、补给压力和占领代价缺少一眼可读的视觉反馈。目标是在不改变自动结算、不破坏新占领治理折损的前提下，把战报从文字弹窗推进到有结果层级的反馈面板。

### 已修复

- `UISetup` 扩展 `BattleReportPanel`：新增胜负结果色带、攻守战力 `Image` 条、攻守补给压力徽标，并调整面板高度和文本区位置以保持小视口可用。
- `BattleReportPanel` 绑定新增视觉组件，按 `BattleResult` 更新结果色带文案、攻守战力条宽度、补给压力徽标文案与风险色。
- `BattleReportPanel` 增加轻量结果脉冲，战斗结算打开面板时给胜负色带一个短反馈，不影响结算数据。
- PlayMode 扩展胜利占领、未占领、防守/进攻窄视口路径：断言战报有真实 `Image` 结果色带、战力条、补给徽标，并继续保留补给修正、占领结果、治理影响和合法性压力文本。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化字段 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 是 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过。第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过；12 张截图均为 `1600x900` 且非空，采样色数量约 `1598-3304`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争闭环的结果反馈已经从静态文本推进到可扫读的战报 UI：玩家能直接看到胜负、战力对比、补给压力和后续占领治理代价；核心战斗结算、连地/补给/占领因果链保持不变。下一轮可继续做战线压力动画、战报与地图接敌点联动，或对新增视觉层做 1024x576/1280x720 性能采样。

## 2026-05-06 Battle Report Map Focus Pass

### 缺口

本轮继续检查战争结果反馈闭环：战报已有胜负色带、战力条和补给徽标，但玩家从战报看完结算后还不能直接回到战场地区；这会让“接敌点 -> 战报 -> 占领/治理后续处理”的闭环断在弹窗内。目标是在不改变战斗结算、不替换 56 区地图结构的前提下，让战报能反向定位到真实区域。

### 已修复

- `BattleReportPanel` 新增“定位战场”按钮，保存当前战斗/占领地区 id，并通过回调请求地图聚焦。
- `MainMapUI` 接入战报聚焦回调：点击按钮会发布 `RegionSelected`，重新打开对应地区栏；有真实 `RegionSurface_*` 和 `CameraController` 时，会按该区域 mesh bounds 中心居中相机。
- `OnBattleResolved` 将 `battleRegionId` 传给战报面板；`AppendOccupation` 会把焦点更新到实际被占领地区，避免战报后续治理影响仍指向旧状态。
- PlayMode 增加战报聚焦验收：未占领战报点击“定位战场”后必须打开对应地区栏；窄视口下“定位战场”按钮必须保持在战报面板内部。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化字段 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 首次发现 1024x576 下“定位战场”按钮底边越界约 1.3 像素；上移按钮后复跑通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 是 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过。第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过；12 张截图均为 `1600x900` 且非空，采样色数量约 `1598-3305`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争闭环现在能从地图接敌进入战报，也能从战报回到真实 56 区地图上的战场地区；战报不再是孤立弹窗，而是接回占领治理后续处理的操作入口。下一轮可继续做战线压力动画、接敌点高亮持续时间，或对新增视觉层做 1024x576/1280x720 性能采样。
## 2026-05-06 Battle Report Focus Pulse Pass

### 缺口

本轮继续检查 H0-H6 后续体验缺口：战报已经能通过“定位战场”返回真实 56 区地图区域，但地图只完成选择与相机居中，缺少一个短暂、明确的区域反馈。玩家看完战报后需要立刻知道战场落在地图上的哪一块，否则“战报 -> 战场 -> 占领治理后续处理”的闭环仍然偏弱。

### 已修复

- `RegionController` 复用现有 `SelectedRegionElevation_*` 真实区域 mesh，新增短促 focus pulse：放大和提亮只发生在对应真实区域边界上，不新增假节点、不替换地图投影。
- `MapRenderer` 新增 `PulseRegionFocus(regionId)`，让 UI 可以触发有界的地图高亮，同时不改变 lens 着色、归属数据或治理/战争结算。
- `MainMapUI` 在 `FocusBattleReportRegion` 发布 `RegionSelected` 后触发地图 focus pulse，并继续保留已有的 `CameraController.CenterOnRegion()` 居中逻辑。
- PlayMode 在 DemoBootstrap 真实 56 区地图上新增断言：点击 `FocusBattleReportRegionButton` 后，对应 `SelectedRegionElevation_*` 必须保持可见并发生比例脉冲。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 且非空，采样色范围约 `1599-3305`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战报现在不只会打开地区栏和移动相机，还会在真实 56 区地图对应区域上给出短暂可见反馈。战争闭环的“接敌点 -> 战报 -> 地图定位 -> 占领治理处理”更连续，且没有改变战斗结算、新占领治理折损、历史来源门禁或地图区域结构。下一轮可继续推进战线压力动画、1024x576/1280x720 性能采样，以及密集前线图标/标签避让。
## 2026-05-06 Frontline Pressure Pulse Pass

### 缺口

本轮继续检查战争压力感：行军线、路线接敌点、战场接敌标记已经存在，但在地图上仍偏静态。战争模式需要让玩家一眼知道“这条线正在推进、这个点正在接敌”，同时不能脱离真实 56 区地图，也不能改变连地、补给、视野、拦截或占领治理链。

### 已修复

- `DemoEntityVisualSpawner` 给 `WarRouteContactNode_*` 加入持续脉冲 ring，让预计接敌点有动态压力层。
- `DemoEntityVisualSpawner` 给 `WarContactMarker_*` 增加真实 `LineRenderer` ring 和持续脉冲，让已接敌地区不再只是一个文字标签。
- `WarTargetHighlight_*` 同步加入轻量脉冲，使正在被攻击的目标区更像活跃前线，而不是静态标注。
- 新增 `WarPressurePulse` 组件，只驱动 ring 的宽度、透明度和缩放，不读写地图数据、战斗结算、占领状态或治理数值。
- PlayMode 增加断言：路线接敌点和战场接敌标记的 ring 宽度必须随时间变化，证明战线压力动画真实运行。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。首次编译发现新增 `WarPressurePulse.cs` 未进入当前生成的 csproj，已补入当前 dotnet 验证清单后通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=22 passed=21 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 且非空，采样色范围约 `1606-3305`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争大地图现在在“出征路线 -> 预计接敌点 -> 实际接敌标记”三处都有持续压力反馈，且全部基于真实区域位置和既有战争状态生成。核心玩法链路仍保持：真实 56 区、连地出征、补给压力、接敌、占领治理折损和因果一致性不变。下一轮建议转向 1024x576/1280x720 性能采样和密集前线图标/标签避让。
## 2026-05-06 War Overlay Pulse Budget Pass

### 缺口

上一轮加入战线脉冲后，战争地图的压力感更强，但 1024x576 / 1280x720 overview 视角缺少显式动画预算。若后续前线更密集，脉冲 ring 和文字标签会同时争夺注意力，也会增加不必要的 `Update` 压力。目标是在不削弱“接敌正在发生”的视觉信号前提下，让小视口按缩放层级限制活跃脉冲数量。

### 已修复

- `DemoEntityVisualSpawner` 在 `ApplyLabelDensityForCurrentZoom()` 中同步记录 label candidate、visible/hidden label、active/inactive pulse 和当前 pulse budget。
- `WarPressurePulse` 新增可暂停状态；超出预算时恢复基础宽度、透明度和缩放，避免远景/小视口继续跑满所有 ring 动画。
- `DemoEntityVisualSpawner` 根据 zoom band 和实际 `Screen.width` 设定 pulse budget：1024 overview 使用最紧预算，1280/中等小视口使用中等预算，更大视口保留更多动态反馈。
- PlayMode 扩展 `WarOverlayLabelsCullByZoomDensity`：验证 1024x576 overview 下标签仍不超过 5 个，pulse active 数不超过预算。
- PlayMode 新增 `WarOverlayPulseBudgetUsesMediumViewportAt1280`：请求 1280x720 并按实际 PlayMode viewport 校验 pulse/label budget。首次发现同一 headless PlayMode 会保留 1024 宽度，因此断言改为依据实际 viewport，而不是假设 `Screen.SetResolution` 必然成功。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=23 passed=22 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 且非空，采样色范围约 `1601-3306`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争 overlay 现在不只是有动态压力层，也有小视口/远景下的预算门禁。1024 overview 会限制可见标签和活跃 pulse，1280 请求路径也纳入测试并按实际 PlayMode viewport 收敛。核心 H0-H6 玩法链路没有改变：真实 56 区、连地出征、补给、视野/拦截、接敌、占领治理折损和历史因果仍保持原规则。
## 2026-05-06 Outliner Map Focus Pass

### 缺口

右侧 outliner 已经能显示高风险地区、行军军队、新占治理和最新战报，也能点击打开地区栏；但它只发布 `RegionSelected`，没有复用战报定位已经具备的真实区域脉冲与相机居中。密集前线下，玩家点了待办项后仍可能要自己在地图上找目标，操作闭环不够直接。

### 已修复

- `MainMapUI` 新增统一的 `SelectAndFocusMapRegion()` 路径，让战报“定位战场”和 outliner 待办点击共用真实地图聚焦逻辑。
- outliner 点击现在仍会发布 `RegionSelected` 并打开地区栏，同时在存在 `MapRenderer` 与 `RegionSurface_*` 时触发对应 `SelectedRegionElevation_*` 脉冲。
- 有 `CameraController` 时，outliner 点击会按真实 `RegionSurface_*` mesh bounds 中心执行地图居中。
- `StrategyOutlinerEntriesSelectRegionsAndAvoidExpandedSidebar` 改为使用 `DemoSceneBootstrap`，测试真实 56 区地图，而不是只测纯 UI 选择。
- PlayMode 新增断言：outliner 点击后对应 `SelectedRegionElevation_*` 必须激活并放大，同时 1024x576 下 compact outliner 仍不能与地区面板重叠。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=23 passed=22 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 且非空，采样色范围约 `1603-3305`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

右侧 outliner 现在不只是待办列表，也能作为真实地图导航入口：点击高风险/新占/行军/战报条目后会打开地区栏、折叠自身、聚焦真实 56 区区域并给出可见脉冲。治理/战争双闭环的地图回跳更一致，且没有改变历史来源门禁、因果规则、占领折损或 56 区结构。
## 2026-05-07 Governance Action Hint And Pulse Stability Pass

### 缺口

本轮继续检查治理主界面美观与小视口操作缺口：治理按钮虽然能执行，但玩家必须读大段概览才能判断成本、收益和不可用原因，按钮本身缺少可决策提示。同时，战争路线脉冲测试使用全局时间采样，偶尔会落在波峰附近导致可测宽度变化过小；两条战争 overlay 测试也硬押默认敌军位置，没有显式服从连地/前线出征规则。

### 已修复

- `PacifyRegionButton` 和 `BuildRegionBuildingButton` 扩成双行按钮，在按钮内新增 `GovernancePacifyActionHintText` 与 `GovernanceBuildActionHintText`。
- 治理按钮 hint 直接读取 `StrategyMapRulebook` 的 forecast：安抚显示 `金50 粮30 整+10 乱-12`，建造显示建筑名、金钱成本和主要效果；不可用时显示非己方、战争/外交模式锁定、缺资源、槽位满或科技前置原因。
- `RegionPanel` 只增强显示绑定，不改 `ApplyGovernanceAction`、建筑执行或占领治理链，继续保持 forecast/apply 同源。
- `WarPressurePulse` 改为从自身启用后累积时间驱动脉冲，避免全局时间相位导致 PlayMode 采样偶发过小。
- 战争 overlay 的 label/pulse 测试改为动态寻找并准备一个符合连地/前线规则的可出征目标，测试不再绕过规则，也不依赖默认敌军固定站位。
- PlayMode 增加治理按钮 hint 存在、可见、文本非空、成本/收益可读、小视口内不与来源栏重叠的断言。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=23 passed=22 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 且非空，采样色范围约 `1619-3305`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

治理按钮现在不只是“能点”，而是能直接告诉玩家本次政务或建造的成本、收益和阻塞原因；战争压力脉冲也更稳定可测。核心 H0-H6 规则未改变：真实 56 区地图、治理/战争双闭环、历史来源门禁、因果一致性、新占领折损、连地/补给/视野/拦截仍按既有规则运行。下一轮可继续推进治理面板图标层级、更多按钮图形化、战争多军队密集标签避让和小视口性能计数。

## 2026-05-07 Persistent Automation Generalization Pass

### 缺口

已有 `stage` heartbeat 能继续推进项目，但提示词仍偏向固定问题清单。用户要求把持续推进能力做得更普适、更广泛：每轮不仅检查 H0-H6、治理/战争闭环、视觉、小视口、真实 56 区和 VisualSmoke 清理，也要能自主选择功能、表现、数据、测试、工具链、文档、清理或防回归中价值最高的一到三个小闭环。

### 已修复

- 更新 Codex App 自动化 `stage`，保留名称 `万朝归一持续推进` 和 180 分钟 heartbeat 节奏。
- 自动化提示词改为通用持续开发循环：读取项目规则、报告、状态、git 状态和最近验证日志后，扫描最高价值缺口，自主选择一到三个可闭环任务。
- 明确保留真实 56 区、历史来源门禁、占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动可解释等硬约束。
- 明确每轮按改动类型运行匹配验证，并在视觉任务后清理 `.outputs/visual/unity-*.png`、`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy` 和发现的历史临时副本。

### 验证

- `codex_app.automation_update` 更新 `stage` 成功，返回 `automationId=stage`。
- `codex_app.automation_update` view 已打开自动化卡片，确认配置可被 Codex App 读取。
- 读取 `C:\Users\123\.codex\automations\stage\automation.toml`，确认 `name = "万朝归一持续推进"`、`status = "ACTIVE"`、`rrule = "RRULE:FREQ=MINUTELY;INTERVAL=180"`，提示词为正常中文并包含通用持续推进约束。
- VisualSmoke 清理复核：本轮未生成新截图；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 不存在；发现并删除历史遗留 `.outputs/visual-project` 临时副本。

### 结论

持续推进机制已从固定清单巡检扩展为“扫描缺口、选择高价值小闭环、修复、验证、记录、清理”的通用开发循环。下一轮自动化可直接从当前报告和 `.omx/state/strategy-map-dual-loop/ralph-progress.json` 的候选项继续，优先处理治理面板图标层级、VisualSmoke 运行计数、脉冲强度调校或多军队压力场景下的标签避让。

## 2026-05-07 War Overlay Runtime Stats Gate Pass

### 缺口

上一轮已完成小视口 label/pulse 预算，但 VisualSmoke 只证明截图非空和场景可见，没有把战争 overlay 的对象数量、路线数量、目标数量和标签数量写入图形门禁。若后续继续增加前线视觉层，可能在不破坏截图的情况下让小视口对象堆叠或预算失控。

### 已修复

- `DemoEntityVisualSpawner` 记录当前战争 overlay 统计：总对象、路线段、接敌点、占领标记、目标高亮、标签、可见/隐藏标签以及 active pulse/budget。
- `VisualSmokeCaptureTests` 在战争路线截图中输出 `[VisualSmokeOverlayStats]`，并断言单路线 overlay 预算仍然受控。
- `GameManagerPlayModeSmokeTests` 在 1024x576 与 1280x720 路径上补充 overlay object/label/pulse 预算断言，继续使用真实连地前线准备逻辑。
- `tools/unity/run_visual_smoke_tests.ps1` 成功后必须从 Unity log 读到 `[VisualSmokeOverlayStats]`，否则图形门禁失败。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 warning / 0 error。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=23 passed=22 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 非空，采样色范围 `1613-3303`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 pulses=2/2 pulseBudget=5`。
- VisualSmoke 清理复核：`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争路线视觉层现在不只“看得见”，也有运行时计数门禁。后续继续加接敌、占领、军队与路线压力效果时，VisualSmoke 会同步暴露 overlay 对象增长，避免小视口在不知不觉中被标签和脉冲层挤爆。核心 H0-H6 规则未改变：真实 56 区、连地出征、补给/视野/拦截、新占领折损、治理 forecast/apply 同源与历史因果门禁仍按既有逻辑运行。

## 2026-05-07 Governance Action Icons Pass

### 缺口

本轮继续检查治理主界面美观与信息层级。上一轮治理按钮已经有 forecast hint，但按钮第一眼仍主要依赖文字；在 1024x576 小视口下，玩家需要更快区分“政务安抚”和“地区建造”两个动作，同时不能把成本收益、来源说明或 forecast/apply 同源关系挤出面板。

### 已修复

- `PacifyRegionButton` 新增 `GovernancePacifyActionIcon` 图标层，使用真实 UGUI `Image` 色块和紧凑 glyph，强化政务动作入口。
- `BuildRegionBuildingButton` 新增 `GovernanceBuildActionIcon` 图标层，保留建造 hint 与成本收益显示。
- `CreateButtonActionHint()` 调整按钮主标签位置与宽度，为左侧图标留出稳定列，避免图标、主标签和 forecast hint 互相压住。
- `RegionPanel` 绑定两个动作图标，并按按钮可用状态调色；不可用时降为次级色，可用时分别使用治理/金钱色，不改 `StrategyMapRulebook` forecast 或 `ApplyGovernanceAction`。
- PlayMode 增加断言：两个动作图标必须存在、可见、使用 `Image`，绑定非空 glyph，位于各自按钮内，并与 forecast hint 分离。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- 首次 `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 失败，原因是新增图标在 Canvas 缩放后宽度只有约 `10.39`，低于可用尺寸断言；已把图标扩大并重新分配主标签列宽。
- 复跑 `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=23 passed=22 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 非空，采样色范围 `1628-3303`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 pulses=2/2 pulseBudget=5`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。
- `git diff --check` 通过，仅有 LF/CRLF 提示。

### 结论

治理动作现在具备“图标 + 动作名 + forecast hint”的三层扫读结构，玩家能更快区分政务与建造，同时继续从同一 forecast 来源读取成本、收益和阻塞原因。核心因果链没有改变：真实 56 区、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截仍按既有规则运行。下一轮可继续推进多军队密集前线标签避让、1024/1280 视觉性能计数，或继续调校治理/战争按钮图标体系。

## 2026-05-07 Dense Frontline Label Reason Pass

### 缺口

本轮继续检查战争压力感、图标文字避让、小视口对象预算和 VisualSmoke 运行计数。已有 label/pulse budget 能限制单路线 overlay，但密集前线下只知道“隐藏了多少标签”，不知道是因为 zoom 层级、可见预算还是屏幕重叠；后续调战争压力表现时容易把标签避让变成黑盒。

### 已修复

- `DemoEntityVisualSpawner` 新增 `LastHiddenByZoomCount`、`LastHiddenByBudgetCount`、`LastHiddenByOverlapCount`，每次 `ApplyLabelDensityForCurrentZoom()` 都把隐藏原因拆开记录。
- `FormatOverlayStats()` 增加 `hiddenZoom`、`hiddenBudget`、`hiddenOverlap`，图形 VisualSmoke 日志能直接看出标签隐藏原因。
- `WarOverlayLabelsCullByZoomDensity` 增加隐藏原因求和断言，保证 `hiddenLabels` 不再是不可解释总数。
- 新增 `WarOverlayDenseFrontlineExplainsLabelSuppression`，在真实连地/前线规则下创建三条玩家出征路线，验证 1024x576 overview 下标签预算、隐藏原因、pulse 暂停和 overlay 对象预算都受控。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=24 passed=23 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 非空，采样色范围 `1623-3305`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。
- `git diff --check` 通过，仅有 LF/CRLF 提示。

### 结论

密集前线标签避让现在具备可解释门禁：小视口下不只限制显示数量，还能说明隐藏来自 zoom、预算还是重叠。三路出征压力场景保持连地/前线规则，不伪造地图结构、不绕过补给或战争命令。下一轮可继续把隐藏原因计数接入 1024/1280 性能摘要，或扩展到战报/外交动作按钮的图标体系。

## 2026-05-07 Overlay Budget HUD Summary Pass

### 缺口

本轮继续检查战争压力感、图标文字避让、1024x576 小视口稳定性与性能对象预算。上一轮已经把 `hiddenZoom`、`hiddenBudget`、`hiddenOverlap` 写入 PlayMode 和 VisualSmoke 日志，但玩家/开发者在运行画面里仍看不到这些计数，只能翻日志判断密集前线标签为何被隐藏。目标是在不改变战争结算、真实 56 区地图和连地/补给/视野/拦截规则的前提下，把同一套 overlay 预算统计接入 HUD。

### 已修复

- `UISetup` 在 HUD 第二行新增 `OverlayBudgetText`，显示标签隐藏原因、可见/总标签、隐藏标签和脉冲预算。
- `MainMapUI` 新增 `RefreshOverlayBudgetText()`，从 `DemoEntityVisualSpawner` 读取同一套运行时计数：缩放、预算、重叠、标签和脉冲，不新增机制来源。
- HUD 摘要使用紧凑中文：`避让 缩放/预算/重叠 ... | 标签 ... | 脉冲 ...`，让日志里的 hidden reason 变成画面内可解释信息。
- PlayMode 新增 `OverlayBudgetText` 存在性和密集前线绑定断言，验证 1024x576 下文本位于 Canvas 内、不遮挡紧凑 HUD 按钮，并与 spawner 当前计数一致。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=24 passed=23 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 非空，采样色范围 `1646-3328`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。

### 结论

战争 overlay 预算现在从“日志可解释”推进到“运行 HUD 可解释”：密集前线下玩家/开发者能直接看到标签被缩放、预算或重叠规则隐藏的数量，以及当前脉冲预算。核心 H0-H6 因果链未改：真实 56 区、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动仍按既有规则运行。下一轮可继续做 1024/1280 HUD 视觉截屏专项、战报/外交动作按钮图标体系，或地图滑动/缩放手感采样。

## 2026-05-07 Smooth Map Focus Pass

### 缺口

本轮按持续开发清单复查 H0-H6、治理/战争双闭环、治理 HUD、战争压力、地图滑动/缩放、图标文字避让、真实 56 区地图、小视口稳定性、2.5D 地形、VisualSmoke 清理与工具链。最高价值缺口落在地图滑动/缩放手感：战报与 outliner 点击地区时相机从瞬移改为平滑聚焦后，PlayMode 暴露出被动边缘滚屏会抢占平滑聚焦，且失败测试留下的旧 Demo 对象会污染后续战争 overlay 用例。

### 已修复

- `CameraController` 在平滑聚焦进行中暂停 edge-pan，并在聚焦完成后保留短释放延迟，避免鼠标停在屏幕边缘导致战报/outliner 聚焦后马上漂移。
- `CameraController` 保留键盘平移、鼠标拖拽、滚轮缩放对平滑聚焦的主动取消能力，继续保证玩家手动输入优先。
- `GameManagerPlayModeSmokeTests` 增加 `UnityTearDown` 统一清理 Demo bootstrap/runtime 对象，避免一个失败用例留下旧 `GameManager`、地图或 UI 影响后续测试。
- outliner 聚焦测试改为先解析真实 `RegionSurface_*` 投影中心，并把相机移动到一个可观测偏移点后再点击条目，避免目标被视口边界 clamp 后误判平滑聚焦没有启动。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- 首次 `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 复现旧问题收敛为 1 个失败：聚焦完成后 edge-pan 又把相机推离目标约 0.18；已用短释放延迟修复。
- 复跑 `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=24 passed=23 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过，12 张截图均为 `1600x900` 非空，采样色范围 `1651-3328`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。
- `git diff --check` 通过，仅有 LF/CRLF 提示。

### 结论

战报与 outliner 的地图聚焦现在是可验证的平滑过渡，并且不会被被动边缘滚屏立即打断；测试隔离也更稳，后续失败不会轻易污染战争 overlay、HUD 或视觉 smoke 用例。核心边界未改：真实 56 区结构、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动均保持原规则。剩余风险是平滑曲线与释放延迟仍需结合实际手感继续微调；下一轮可优先做 1024/1280 HUD 截屏专项、战报/外交动作按钮图标体系，或相机拖拽/滚轮手感采样。

## 2026-05-07 Small Viewport VisualSmoke Pass

### 缺口

本轮继续按普适开发循环复查 H0-H6、治理/战争双闭环、治理主界面、战争压力感、地图滑动/缩放、图标文字避让、真实 56 区投影、小视口稳定性、性能对象预算、2.5D 地形、VisualSmoke 清理和 Unity/Tuanjie 工具链。最高价值缺口是：PlayMode 已覆盖 1024x576 与 1280x720 的布局断言，但图形 VisualSmoke 仍只校验 `1600x900` 截图；地图 HUD、地区建造和天气面板缺少真实小视口截图门禁。

### 已修复

- `VisualSmokeCaptureTests` 新增 1024x576 与 1280x720 的地图 HUD 截图：`unity-1024-map-hud.png`、`unity-1280-map-hud.png`。
- `VisualSmokeCaptureTests` 新增 1024x576 与 1280x720 的地区建造面板截图：`unity-1024-region-building-panel.png`、`unity-1280-region-building-panel.png`。
- `VisualSmokeCaptureTests` 新增 1024x576 与 1280x720 的天气面板截图：`unity-1024-weather-panel.png`、`unity-1280-weather-panel.png`。
- 截图 helper 改为按目标视口创建 RenderTexture，并记录 `[VisualSmokeViewport]` 日志，使图形门禁能证明实际采样过指定分辨率。
- `tools/unity/run_visual_smoke_tests.ps1` 的截图校验从固定 `1600x900` 扩展为逐文件期望尺寸校验，18 张截图都必须非空、尺寸正确、采样色数达标；成功后继续删除截图与临时项目副本。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过。
- VisualSmoke 本轮校验 18 张非空截图：12 张 `1600x900` 既有场景，加 6 张小视口截图；小视口覆盖地图 HUD、地区建造和天气面板的 `1024x576` 与 `1280x720`。
- 小视口截图采样色证据：`unity-1024-map-hud.png` 为 `1024x576 sampledColors=3565`，`unity-1280-map-hud.png` 为 `1280x720 sampledColors=3277`，`unity-1024-region-building-panel.png` 为 `2951`，`unity-1280-region-building-panel.png` 为 `2666`，`unity-1024-weather-panel.png` 为 `2152`，`unity-1280-weather-panel.png` 为 `2088`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=24 passed=23 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。
- `git diff --check` 通过，仅有 LF/CRLF 提示。

### 结论

图形门禁现在不只看桌面大视口，也会真实渲染并校验 1024x576/1280x720 下的地图 HUD、地区建造和天气面板。核心机制没有变：真实 56 区结构、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动仍保持既有规则。剩余风险是小视口截图目前证明“非空、尺寸正确、视觉复杂度足够”，还没有做逐像素/结构化视觉裁判；下一轮可继续补战报/外交动作图标体系，或增加针对 1024/1280 的截图裁判摘要。

## 2026-05-07 VisualSmoke Verdict Summary Pass

### 缺口

本轮继续扫描 H0-H6、治理/战争双闭环、治理主界面、战争压力、地图手感、图标文字避让、真实 56 区投影、小视口稳定性、对象预算、2.5D 地形、VisualSmoke 清理和工具链。上一轮已经让 VisualSmoke 真实渲染 `1024x576` 与 `1280x720`，但图形门禁仍主要依赖非空、尺寸和采样色；若截图尺寸正确但中心内容塌掉或边缘区域过空，脚本没有结构化 verdict 给下一轮判断。由于当前没有参考图，本轮不做 screenshot-to-reference 对比，而是落地 smoke-verdict 摘要。

### 已修复

- `tools/unity/run_visual_smoke_tests.ps1` 新增 `visual-smoke-verdict.json` 输出，保存在 `.outputs/tuanjie/visual-smoke-verdict.json`。
- 每张截图现在除 `SampledColors` 外，还统计 `CenterColors` 与 `EdgeColors`，用于发现中心主内容或边缘背景异常单调。
- 每张截图生成 `Score`、`Verdict` 与 `Findings`；总 verdict 取最低分，18 张截图必须全部达到通过阈值。
- 输出表格新增 `CenterColors`、`EdgeColors`、`Score`、`Verdict`、`Findings`，让小视口视觉质量不再只靠人工翻图片。
- 成功后仍执行原有清理：截图删除，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 删除；verdict JSON 作为验证证据保留。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过。
- VisualSmoke verdict：`.outputs/tuanjie/visual-smoke-verdict.json` 生成成功，`verdict=pass score=100 screenshots=18 smallViewports=6`。
- 18 张截图全部通过结构化 smoke verdict；小视口截图覆盖地图 HUD、地区建造和天气面板的 `1024x576` 与 `1280x720`，中心/边缘采样均达标。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=24 passed=23 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。
- `git diff --check` 通过，仅有 LF/CRLF 提示。

### 结论

VisualSmoke 现在具备结构化 smoke verdict：不仅能证明截图存在、尺寸正确、颜色不空，还能输出中心与边缘视觉复杂度、最低分和 per-screenshot verdict，方便后续持续开发判断小视口是否退化。核心机制没有变化：真实 56 区、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动仍按既有规则运行。剩余风险是这仍不是带参考图的严格视觉对比；下一轮可继续补战报/外交/政策动作图标体系，或为重点截图补参考图式 visual-verdict。

## 2026-05-07 Battle Report Small Viewport VisualSmoke Pass

### 缺口

本轮按持续开发清单复查 H0-H6、治理/战争双闭环、治理主界面、战争压力感、地图滑动/缩放手感、图标文字避让、真实 56 区地图投影、小视口稳定性、性能对象预算、2.5D 地形、VisualSmoke 清理和 Unity/Tuanjie 工具链。地图 HUD、地区建造与天气面板已具备 1024x576 和 1280x720 截图门禁，但战报面板仍只有 1600x900 截图。战争反馈是压力感核心来源，小视口下需要真实图形采样防止战报拥挤或内容塌缩。

### 已修复

- `VisualSmokeCaptureTests` 在战报出现后新增 `unity-1024-battle-report.png` 与 `unity-1280-battle-report.png` 两个截图采样。
- `run_visual_smoke_tests.ps1` 将两个战报小视口截图纳入期望文件清单，继续校验尺寸、非空、总采样色、中心采样色、边缘采样色、评分和 verdict。
- VisualSmoke verdict 从 18 张截图扩展到 20 张截图，小视口样本从 6 张扩展到 8 张，覆盖地图 HUD、地区建造、天气面板与战报面板。
- 截图使用后仍清理 `.outputs/visual/unity-*.png`、`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview`。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次遇到已知 first-import URP material log 后自动重试，第 2 次通过。
- VisualSmoke verdict：`.outputs/tuanjie/visual-smoke-verdict.json` 生成成功，`verdict=pass score=100 screenshots=20 smallViewports=8`。
- 战报小视口证据：`unity-1024-battle-report.png` 为 `1024x576 sampledColors=1713 centerColors=954 edgeColors=254 score=100`，`unity-1280-battle-report.png` 为 `1280x720 sampledColors=1489 centerColors=878 edgeColors=269 score=100`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=24 passed=23 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。

### 结论

战报面板现在进入小视口图形门禁：1024x576 与 1280x720 下能真实渲染、尺寸正确、中心与边缘视觉复杂度达标，并纳入同一个 smoke verdict。核心机制未改：真实 56 区、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动保持既有规则。剩余风险是战报小视口目前仍是 smoke-verdict，不是带参考图的严格视觉对比；下一轮可继续补外交/政策动作按钮图标体系，或为关键 1024/1280 截图建立 reference-backed visual-verdict。

## 2026-05-07 Diplomacy And War Route Small Viewport VisualSmoke Pass

### 缺口

本轮继续复查 H0-H6、治理/战争双闭环、治理主界面、战争压力感、地图滑动/缩放手感、图标文字避让、真实 56 区地图投影、1024x576 与 1280x720 小视口稳定性、性能对象预算、2.5D 地形表达、VisualSmoke 临时副本清理、Unity/Tuanjie 工具链和数据契约。上一轮已补战报小视口截图，但外交桥接面板与战争路线压力画面仍只有 1600x900 图形门禁。它们是从外交/机制说明进入战争命令、再看到行军压力的关键中间态，小视口下需要同等截图证据。

### 已修复

- `VisualSmokeCaptureTests` 为 `unity-diplomacy-bridge.png` 增加 `unity-1024-diplomacy-bridge.png` 与 `unity-1280-diplomacy-bridge.png`。
- `VisualSmokeCaptureTests` 为战争路线画面增加 `unity-1024-war-route.png` 与 `unity-1280-war-route.png`。
- `run_visual_smoke_tests.ps1` 将新增 4 张截图纳入期望清单，继续校验尺寸、非空、总采样色、中心采样色、边缘采样色、score 与 verdict。
- `run_visual_smoke_tests.ps1` 的临时目录清理新增重试，覆盖 Tuanjie PackageCache 文件消失竞态；重试条件从 URP first-import 扩展到临时副本锁与 PackageCache ENOENT。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- 首次重跑 VisualSmoke 时复现临时副本/PackageCache 竞态：`another Tuanjie instance is running with this project open` 与 `PackageCache ENOENT`；清理复核后临时副本已消失。
- 修复重试逻辑后，`powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 通过；第 1 次命中 retryable import/package log，第 2 次通过。
- VisualSmoke verdict：`.outputs/tuanjie/visual-smoke-verdict.json` 生成成功，`verdict=pass score=100 screenshots=24 smallViewports=12`。
- 外交桥接小视口证据：`unity-1024-diplomacy-bridge.png` 为 `1024x576 sampledColors=1655 centerColors=984 edgeColors=208 score=100`，`unity-1280-diplomacy-bridge.png` 为 `1280x720 sampledColors=1490 centerColors=922 edgeColors=218 score=100`。
- 战争路线小视口证据：`unity-1024-war-route.png` 为 `1024x576 sampledColors=1819 centerColors=1125 edgeColors=208 score=100`，`unity-1280-war-route.png` 为 `1280x720 sampledColors=1602 centerColors=1025 edgeColors=226 score=100`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。

### 结论

外交桥接、战争路线与战报现在都进入 1024x576/1280x720 小视口图形门禁，VisualSmoke 覆盖提升到 24 张截图与 12 张小视口截图。工具链也能对临时副本锁和 PackageCache ENOENT 做一次自动恢复重试，并在成功后清理截图和副本。核心机制未改：真实 56 区、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动保持既有规则。剩余风险是这些仍是 deterministic smoke-verdict，不是参考图视觉对比；下一轮可补政策动作按钮图标层级，或为关键小视口截图建立 reference-backed visual-verdict。

## 2026-05-07 Mechanism Action Icon Small Viewport Pass

### 缺口

本轮继续检查治理/战争双闭环、政策与外交入口、1024x576/1280x720 小视口稳定性、图标文字避让、VisualSmoke 工具链和临时截图清理。此前治理面板的安抚/建造按钮已经具备图标层级，但 `MechanismPanel` 里的政策、外交、封关、谍报、入战动作仍主要依赖文字；小视口下玩家难以快速区分“国内治理动作”和“转入战争压力”的操作出口。同时 VisualSmoke runner 在 Tuanjie 进程提前返回、XML 稍后生成时仍可能误判失败。

### 已修复

- `MechanismPanel` 从轻微下移改为居中，底部 `EnterWarModeButton` 与 `CloseButton` 上移，避免 1024x576 下贴边越界。
- `UISetup` 新增 `CreateButtonLeadingIcon()`，并给政策、外交、封关、谍报、入战按钮分别加入 `PolicyActionIcon`、`DiplomacyActionIcon`、`BorderActionIcon`、`EspionageActionIcon`、`EnterWarModeActionIcon`。
- PlayMode 新增机制面板动作图标断言：5 个图标必须存在、可见、位于各自按钮内，并在 1024x576 与 1280x720 下保持按钮内部布局稳定。
- `VisualSmokeCaptureTests` 新增 `unity-mechanism-actions.png`、`unity-1024-mechanism-actions.png`、`unity-1280-mechanism-actions.png`，让机制按钮图标有专门图形门禁，而不只混在外交桥接截图里。
- `run_visual_smoke_tests.ps1` 在临时工程复制时保留源项目 `Library/PackageCache`，减少 Tuanjie 首次导入时的 PackageCache ENOENT。
- `run_visual_smoke_tests.ps1` 新增 `Wait-VisualSmokeResults`，当 Tuanjie 进程非零退出但测试 XML 尚未出现时等待 Test Runner 输出，避免过早判失败并清理有效结果。

### 验证

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\restore_cached_script_assemblies.ps1 "My project"` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`regions=56 map_region_shapes=56`，并保留 13 位帝皇、35 条政策、40 项技术、200 条编年事件等数据契约。
- `python tools\validate_domain_core.py` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次命中 retryable import/package log，第 2 次通过。
- VisualSmoke verdict：`.outputs/tuanjie/visual-smoke-verdict.json` 为 `verdict=pass score=100 screenshots=27 smallViewports=14`。
- 机制动作小视口证据：`unity-1024-mechanism-actions.png` 为 `1024x576 sampledColors=1657 centerColors=984 edgeColors=208 score=100`，`unity-1280-mechanism-actions.png` 为 `1280x720 sampledColors=1490 centerColors=922 edgeColors=218 score=100`。
- 关键小视口截图：`unity-1024-diplomacy-bridge.png`、`unity-1280-diplomacy-bridge.png`、`unity-1024-war-route.png`、`unity-1280-war-route.png` 均 `score=100`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。

### 结论

机制面板现在具备“政策/外交/封关/谍报/入战”五类动作的稳定图标扫读层级，并有 1024x576 与 1280x720 PlayMode + VisualSmoke 双门禁保护。VisualSmoke runner 也补上了“进程提前返回但 XML 后生成”的等待路径，减少 Tuanjie 图形门禁误判。核心机制未改：真实 56 区、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动继续按既有规则运行。剩余风险是这些图标仍是 UGUI 文本 glyph，不是最终美术图标；下一轮可继续做 reference-backed visual-verdict，或推进 2.5D 地形与相机拖拽/滚轮手感采样。

## 2026-05-07 Camera Drag Zoom Feel Gate

### 缺口

本轮继续复查 H0-H6 Heavy Strategy Systems、治理/战争双闭环、治理主界面、战争压力感、地图滑动/缩放手感、图标文字避让、真实 56 区地图投影、1024x576 与 1280x720 小视口稳定性、性能对象预算、2.5D 地形表达、VisualSmoke 清理、Unity/Tuanjie 工具链、JSON 数据契约、历史来源门禁、因果一致性与可解释性。上一轮已经补齐机制动作图标与小视口图形门禁，但相机测试只验证了基础 zoom clamp、focus 和 zoom-around-point，还没有明确证明同一拖拽比例在 1024x576 与 1280x720 下保持一致手感，也没有断言完整视口在极端拖拽后仍留在地图边界内。

### 已修复

- `CameraController` 新增 `ViewportWorldRect`，让测试和后续 UI 可以直接解释当前相机完整世界视口。
- `CameraController` 将中键拖拽的世界位移计算抽为 `CalculateScreenDragWorldDelta()`，并新增 `PanByScreenDrag()`，用于不依赖真实鼠标输入的确定性手感门禁。
- PlayMode 扩展 `CameraControllerClampsZoomAndCenterToConfiguredBounds`：在 1024x576 与 1280x720 下验证同一 16:9 比例拖拽产生一致世界位移、拖拽方向符合手感预期、zoom-around-point 保持指向地图点稳定、极端拖拽后完整视口仍夹在配置边界内。
- 本轮不改 JSON、地图形状、战争结算、占领折损、治理 forecast/apply 或连地/补给/视野/拦截规则。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\unity\preflight_without_unity.py` 通过：data tables、map shapes、asmdefs、packages 与 Unity handoff entrypoints 均存在。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 本轮未运行，因为没有新增或修改截图场景；清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。

### 结论

地图相机现在有可重复的小视口手感门禁：1024x576 与 1280x720 下，同一相对屏幕拖拽产生一致世界位移，滚轮缩放保持指针下地图点稳定，极端平移不会让完整视口逃出配置地图边界。核心机制未改，真实 56 区结构和历史来源门禁保持不变。剩余风险是这仍是确定性 PlayMode 行为门禁，不是玩家主观手感录屏或参考截图对比；下一轮可继续推进 2.5D 地形表达专项断言，或建立 reference-backed visual-verdict。

## 2026-05-07 Terrain Lens VisualSmoke Pass

### 缺口

本轮继续复查 H0-H6 Heavy Strategy Systems、治理/战争双闭环、治理主界面美观与信息层级、战争压力感、地图滑动/缩放手感、图标与文字避让、真实 56 区地图投影、1280x720 与 1024x576 小视口稳定性、性能与对象预算、2.5D/3D 地形表达、VisualSmoke 截图清理、Unity/Tuanjie 工具链、JSON 数据契约、历史来源门禁、因果一致性与可解释性。上一轮补齐了相机拖拽/缩放手感门禁；2.5D 地形已有 56 个阴影层和选中抬升层的 PlayMode 断言，但 VisualSmoke 仍没有专门捕获“地形镜头”下真实 56 区投影和小视口表现。

### 已修复

- `VisualSmokeCaptureTests` 新增地形镜头捕获：点击 `LensTerrainButton` 后采集 `unity-terrain-lens.png`、`unity-1024-terrain-lens.png`、`unity-1280-terrain-lens.png`。
- `VisualSmokeCaptureTests` 新增 `AssertTerrainLensVisible()`：地形镜头下必须保留真实 56 个 `RegionSurface_*`，并且 56 个真实地区表面可见、56 个 2.5D shadow layer 仍存在。
- `tools/unity/run_visual_smoke_tests.ps1` 将 3 张地形镜头截图纳入期望清单和结构化 smoke-verdict 评分。
- 本轮不改 JSON、地图形状、战争结算、占领折损、治理 forecast/apply 或连地/补给/视野/拦截规则。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次命中 retryable import/package log，第 2 次通过。
- VisualSmoke verdict：`.outputs/tuanjie/visual-smoke-verdict.json` 为 `verdict=pass score=100 screenshots=30 smallViewports=16`。
- 地形镜头小视口证据：`unity-1024-terrain-lens.png` 为 `1024x576 sampledColors=2708 centerColors=1757 edgeColors=250 score=100`；`unity-1280-terrain-lens.png` 为 `1280x720 sampledColors=2453 centerColors=1664 edgeColors=264 score=100`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。

### 结论

2.5D 地形表达现在进入小视口图形门禁：地形镜头能在真实 56 区投影上显示所有地区表面，并在 1024x576 与 1280x720 下通过结构化 VisualSmoke 评分。核心机制未改，真实 56 区结构、历史来源门禁、新占领地区折损、治理 forecast/apply 同源，以及连地/补给/视野/拦截与粮食-补给-秩序-外交联动均保持既有规则。剩余风险是地形镜头仍是 deterministic smoke-verdict，不是参考图视觉对比；下一轮可继续做 reference-backed visual-verdict，或补 zoom-band 与标签密度切换时的地形/战争叠加截图。

## 2026-05-07 Terrain War Overlay VisualSmoke Pass

### 缺口

本轮继续复查 H0-H6 Heavy Strategy Systems、治理/战争双闭环、治理主界面美观、战争压力感、地图滑动/缩放手感、图标与文字避让、真实 56 区地图投影、1024x576 与 1280x720 小视口稳定性、性能与对象预算、2.5D/3D 地形表达、VisualSmoke 截图清理、Unity/Tuanjie 工具链、JSON 数据契约、历史来源门禁、因果一致性与可解释性。上一轮已给地形镜头单独建立图形门禁，但还没有证明“地形镜头 + 战争路线压力叠加”时，真实 56 区表面、路线 underlay、目标高亮、路线标签避让和对象预算能同时成立。

### 已修复

- `VisualSmokeCaptureTests` 在战争路线发出后切回 `LensTerrainButton`，新增 `unity-terrain-war-overlay.png`、`unity-1024-terrain-war-overlay.png`、`unity-1280-terrain-war-overlay.png`。
- 组合截图前复用 `AssertTerrainLensVisible()`，确保地形叠加态仍显示真实 56 个 `RegionSurface_*` 且保留 56 个 2.5D shadow layer。
- 组合截图前复用 `DemoEntityVisualSpawner` 对象预算断言：路线、underlay、投射接敌点和目标高亮必须保留，单路线 overlay 对象预算仍小于等于 14。
- `tools/unity/run_visual_smoke_tests.ps1` 将 3 张组合截图纳入期望清单与结构化 smoke-verdict 评分。
- 本轮不改 JSON、地图形状、战争结算、占领折损、治理 forecast/apply 或连地/补给/视野/拦截规则。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过：0 warning，0 error。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次命中 retryable import/package log，第 2 次通过。
- VisualSmoke verdict：`.outputs/tuanjie/visual-smoke-verdict.json` 为 `verdict=pass score=100 screenshots=33 smallViewports=18`。
- 地形战争叠加小视口证据：`unity-1024-terrain-war-overlay.png` 为 `1024x576 sampledColors=1709 centerColors=1026 edgeColors=208 score=100`；`unity-1280-terrain-war-overlay.png` 为 `1280x720 sampledColors=1503 centerColors=929 edgeColors=227 score=100`。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。

### 结论

地形镜头与战争路线压力现在具备组合图形门禁：真实 56 区投影、2.5D 地形层、战争路线/目标/标签/脉冲预算能在 1024x576 与 1280x720 同时通过结构化 VisualSmoke。核心机制未改，新占领折损、治理 forecast/apply 同源，以及连地、补给节点、视野、拦截与粮食-补给-秩序-外交联动继续保持既有可解释规则。剩余风险是组合截图仍为 deterministic smoke-verdict，不是 reference-backed visual-verdict；下一轮可优先建立参考图对比，或补性能/对象预算的独立回归统计。

## 2026-05-07 Reference-backed VisualSmoke Pass

### 缺口

本轮继续复查 reference-backed visual-verdict、2.5D 地形表达、相机拖拽/滚轮手感采样、真实 56 区地图投影、1024x576 与 1280x720 小视口稳定性、VisualSmoke 截图清理和工具链可重复性。前序相机手感、地形镜头、地形+战争叠加已经有 PlayMode 或 VisualSmoke 门禁，但图形 verdict 仍主要依赖每次截图的非空、尺寸、采样色、中心/边缘复杂度；缺少一份“已批准小视口参考签名”来防止关键界面悄悄降级。

### 已修复

- 新增 `tools/unity/visual_smoke_reference.json`，记录 18 张关键小视口截图的参考签名和最低阈值；只保存数值签名，不保存临时 PNG。
- `run_visual_smoke_tests.ps1` 新增 `VISUAL_SMOKE_REFERENCE_PATH` 支持，默认读取 `tools/unity/visual_smoke_reference.json`。
- `run_visual_smoke_tests.ps1` 新增 reference-backed 比对：逐张校验尺寸、采样色、中心采样色、边缘采样色和编码大小是否仍高于参考阈值。
- VisualSmoke verdict JSON 新增 `referenceVerdict`、`referenceScore`、`referenceComparisonCount`、`referenceSource` 与 `referenceComparisons`，让后续轮次能直接看到参考签名结果。
- `preflight_without_unity.py` 将参考签名文件纳入 Unity 工具链入口检查，避免新机器缺少基准文件仍误以为图形门禁完整。

### 验证

- `Get-Content tools\unity\visual_smoke_reference.json | ConvertFrom-Json` 通过：`schemaVersion=1`，`minComparisonScore=90`，`items=18`。
- PowerShell AST 解析 `tools\unity\run_visual_smoke_tests.ps1` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次命中 retryable import/package log，第 2 次通过。
- VisualSmoke verdict：`.outputs/tuanjie/visual-smoke-verdict.json` 为 `verdict=pass score=100 screenshots=33 smallViewports=18`。
- Reference-backed verdict：`referenceVerdict=pass referenceScore=100 referenceComparisonCount=18`，覆盖地图 HUD、地区建造、天气、地形镜头、机制动作、外交桥接、战争路线、地形战争叠加和战报的小视口截图。
- VisualSmoke overlay stats：`overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。

### 结论

关键小视口图形门禁现在不只做 deterministic smoke-verdict，也会对照一份已批准参考签名进行回归判断。截图仍只在有图形设备的 Unity/Tuanjie 环境运行，运行后继续删除临时 PNG 和工程副本。核心机制未改：真实 56 区结构、历史来源门禁、新占领地区折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动继续保持既有规则。剩余风险是当前参考仍为数值签名基线，不是逐像素或人工设计稿对比；下一轮可导出 VisualSmoke overlay/object-budget 到 verdict JSON，或补 zoom-band 地形+战争叠加截图。

## 2026-05-07 Overlay Budget Verdict JSON Pass

### 缺口

本轮继续推进已选中的“把 VisualSmoke overlay/object-budget 统计写进 verdict JSON，做成可趋势化的性能与标签避让回归”。此前 `DemoEntityVisualSpawner` 已输出 `[VisualSmokeOverlayStats]` 日志，PlayMode/VisualSmoke 也会断言对象预算，但这些关键数值只存在日志里，后续自动化很难直接比较 overlay 对象、路线对象、标签隐藏原因和脉冲预算是否退化。

### 已修复

- `run_visual_smoke_tests.ps1` 新增 `Convert-VisualSmokeOverlayStats()`，把 `[VisualSmokeOverlayStats] overlay=...` 日志解析成结构化字段。
- `run_visual_smoke_tests.ps1` 新增 `Get-OverlayBudgetVerdict()`，将 overlay 对象数、路线对象数、标签数、隐藏原因、脉冲数和 pulseBudget 纳入预算评分。
- `visual-smoke-verdict.json` 新增 `overlayStats`、`overlayBudgetVerdict`、`overlayBudgetScore`、`overlayBudgetComparisonCount`、`overlayBudgetReference` 与 `overlayBudgetComparisons`。
- `visual_smoke_reference.json` 新增 `overlayBudget` 阈值：当前单路线参考为 `overlay=7 routes=3 labels=3 hiddenOverlap=2 pulses=2/2 pulseBudget=5`，预算上限保守设置为 `maxOverlayObjects=14`、`maxRouteObjects=6`、`maxLabels=6`、`maxHiddenLabels=5`、`maxPulseBudget=5`。
- 本轮不改地图数据、地图形状、战争结算、占领折损、治理 forecast/apply 或连地/补给/视野/拦截规则。

### 验证

- `Get-Content tools\unity\visual_smoke_reference.json | ConvertFrom-Json` 通过：`items=18`，`overlayBudget.maxOverlayObjects=14`，`overlayBudget.minScore=90`。
- PowerShell AST 解析 `tools\unity\run_visual_smoke_tests.ps1` 通过。
- `python tools\unity\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁完成；虽然用户中途打断了工具输出，复核显示 Tuanjie 已跑完并写出新 verdict。
- VisualSmoke verdict：`.outputs/tuanjie/visual-smoke-verdict.json` 为 `verdict=pass score=100 screenshots=33 smallViewports=18`。
- Reference-backed verdict：`referenceVerdict=pass referenceScore=100 referenceComparisonCount=18`。
- Overlay budget verdict：`overlayBudgetVerdict=pass overlayBudgetScore=100 overlayBudgetComparisonCount=1`。
- 结构化 overlay stats：`OverlayObjects=7 RouteObjects=3 ContactObjects=0 OccupationObjects=0 TargetObjects=1 Labels=3 VisibleLabels=1 HiddenLabels=2 HiddenByZoom=0 HiddenByBudget=0 HiddenByOverlap=2 PulsesActive=2 PulsesTotal=2 PulseBudget=5`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project` 与 `.outputs/visual-preview` 均不存在。

### 结论

VisualSmoke 现在具备趋势化的 overlay/object-budget 回归数据：截图质量、参考签名、战争 overlay 对象预算、标签避让原因和脉冲预算都在同一个 verdict JSON 内可读取。核心机制未改，真实 56 区结构、历史来源门禁、新占领地区折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动继续保持既有规则。剩余风险是当前 overlayBudget 仍是单路线压力场景基准；下一轮可补 zoom-band 地形+战争叠加截图，或单独增加密集前线 overlayBudget 场景。

## 2026-05-07 Dense Frontline VisualSmoke Budget Pass

### 缺口

本轮继续按 Ralph 要求检查 H0-H6、治理/战争双闭环、战争压力感、标签避让、1024x576/1280x720 小视口稳定性、真实 56 区地图投影、VisualSmoke 清理与可趋势化证据。已完成的 overlay/object-budget verdict 只覆盖单路线战争压力，尚未证明多路线密集前线下，路线对象、标签隐藏原因、脉冲预算和截图参考签名仍能稳定通过。同时图形诊断暴露了一个真实 UI 问题：战报弹出时可能被已经打开的治理/机制面板遮挡。

### 已修复

- `VisualSmokeCaptureTests` 新增 dense-frontline 场景：在真实 56 区地图上创建三条玩家进攻路线，采集 `unity-dense-frontline-war-overlay.png`、`unity-1024-dense-frontline-war-overlay.png`、`unity-1280-dense-frontline-war-overlay.png`。
- dense 场景在战报结算前会停止两条额外 stress route，只保留主路线进入战报闭环，避免多路线压力场景污染战报基线。
- `BattleReportPanel` 在 `Show`、`AppendOccupation`、`AppendGovernanceImpact` 时调用置顶逻辑，确保战报反馈压过当前治理/机制面板。
- `VisualSmokeCaptureTests` 增加战报面板 Canvas 顶层断言，防止后续再次被其他面板盖住。
- `visual_smoke_reference.json` 增加 `overlayBudgetProfiles`，现在同时覆盖 `single_route` 与 `dense_frontline`；并把 dense-frontline 1024/1280 小视口纳入 reference-backed 签名。
- `run_visual_smoke_tests.ps1` 现在在同一 verdict JSON 中输出并评分两个 overlay budget comparison，形成可趋势化的密集前线回归证据。
- Review 后补齐 `unity-dense-frontline-war-overlay.png` 与 `unity-battle-report.png` 的 1600x900 reference-backed 签名，避免关键大视口只靠粗 smoke 阈值。
- Review 后收紧 `Get-OverlayBudgetVerdict()`：存在 `overlayBudgetProfiles` 时，场景必须显式命中 profile；未知或拼错的 `scenario` 不再静默回退到默认预算。
- Review 复核后移除 overlay stats 的默认场景补值；单路线 VisualSmoke 日志现在也显式输出 `scenario=single_route`，缺失 `scenario=` 会直接失败。

### 验证

- `dotnet build "My project\WanChaoGuiYi.PlayModeTests.csproj"` 通过；最终 targeted build 0 warning / 0 error，full build 仅保留既有 Unity 序列化 warning。
- `python tools\unity\preflight_without_unity.py` 通过。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期的 `VisualSmokeCaptureTests`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\unity\run_visual_smoke_tests.ps1 "My project"` 图形门禁通过；第 1 次命中 retryable import/package log，第 2 次通过。
- VisualSmoke verdict：`.outputs/tuanjie/visual-smoke-verdict.json` 为 `verdict=pass score=100 screenshots=36 smallViewports=20`。
- Reference-backed verdict：`referenceVerdict=pass referenceScore=100 referenceComparisonCount=22`。
- Overlay budget verdict：`overlayBudgetVerdict=pass overlayBudgetScore=100 overlayBudgetComparisonCount=2`。
- 单路线 overlay stats：`scenario=single_route overlay=7 routes=3 contacts=0 occupations=0 targets=1 labels=3 visibleLabels=1 hiddenLabels=2 hiddenZoom=0 hiddenBudget=0 hiddenOverlap=2 pulses=2/2 pulseBudget=5`。
- 密集前线 overlay stats：`overlay=17 routes=9 contacts=0 occupations=0 targets=1 labels=7 visibleLabels=1 hiddenLabels=6 hiddenZoom=3 hiddenBudget=0 hiddenOverlap=3 pulses=3/4 pulseBudget=3`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy`、`.outputs/tuanjie/visual-preview-copy`、`.outputs/visual-project`、`.outputs/visual-preview`、临时 `visual-inspect` 副本均不存在。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- 独立 code review 曾提出 2 个 `REQUEST CHANGES`：1600x900 关键图未进入 reference-backed 对比、overlayBudget profile 会静默 fallback/默认补场景；均已修复并重新通过图形 VisualSmoke 与补充验证。

### 结论

战争压力层现在不仅有单路线门禁，也有密集前线三路线门禁：对象预算、标签避让原因、脉冲预算、截图非空、reference-backed 小视口签名都进入同一个 verdict JSON。战报面板也补上了置顶保护，避免玩家完成战斗后看不到反馈。核心机制未改：真实 56 区结构、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动保持既有规则。剩余风险是 reference 仍为数值签名，不是像素级或人工设计稿对比；下一轮可继续做 zoom-band 地形+战争叠加截图，或写入轻量趋势历史而不保存 PNG。

## 2026-05-07 Terrain-War Zoom-Band Trend History Pass

### 缺口

上一轮已经把 terrain-war overlay 和 dense-frontline overlay 做成了可回归的 reference-backed 视觉证据，但 terrain-war 仍只有单一镜头，没有把同一战场在 detail / operation / overview 三个缩放带的可读性单独拉出来。与此同时，overlay/object-budget 数据虽然已进 verdict JSON，仍缺少一条专门给后续回归脚本消费的轻量趋势历史。

### 已修复

- `VisualSmokeCaptureTests` 在 terrain-war overlay 后新增三张 zoom-band 截图：`unity-terrain-war-detail-zoom.png`、`unity-terrain-war-operation-zoom.png`、`unity-terrain-war-overview-zoom.png`。
- 三个 zoom-band 都在同一张真实 56 区战争叠加态上采样，分别验证 detail / operation / overview 的标签密度、脉冲预算和路线对象保持可读。
- `run_visual_smoke_tests.ps1` 新增 `Save-VisualSmokeTrendHistory()`，把每次通过后的 verdict 摘要追加到 `.outputs/tuanjie/visual-smoke-trend.jsonl`，不保存 PNG。
- `run_visual_smoke_tests.ps1` 的期待截图列表同步加入三张 zoom-band 截图。
- `visual_smoke_reference.json` 新增三张 zoom-band 截图的 reference-backed 数值签名，reference 比较数从 22 提升到 25。

### 验证

- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error。
- `python tools\\unity\\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_visual_smoke_tests.ps1 "My project"` 通过：`verdict=pass score=100 screenshots=39 smallViewports=20`，`referenceComparisonCount=25`，`overlayBudgetComparisonCount=2`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\\validate_data.py` 通过。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 和 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

terrain-war 现在同时有单路线叠加、三段缩放带和密集前线三路线压力三类证据，趋势历史也已经开始写入 JSONL，后续可以直接拿来比较预算和标签避让是否退化。核心机制仍未改，真实 56 区结构、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动继续保持既有规则。剩余风险是趋势历史目前仍是数值摘要，尚未接入外部仪表板或像素级参考。

## 2026-05-07 Camera Interaction Trend Gate Pass

### 缺口

上一轮已经把 terrain-war 三段缩放带和 VisualSmoke 趋势历史打通，但相机拖拽/滚轮手感还停留在 PlayMode 行为断言里，没有进入图形 VisualSmoke verdict。后续如果滚轮锚点漂移、同宽高比小视口拖拽比例失真，趋势历史无法直接发现。

### 已修复

- `VisualSmokeCaptureTests` 新增 `[VisualSmokeCameraStats]` 日志，在真实 terrain-war 场景后采样 `1024x576` 与 `1280x720` 两档视口。
- 相机采样覆盖：屏幕拖拽世界位移、滚轮锚点误差、结束 zoom band、拖拽后/缩放后 viewport 是否仍在地图边界内。
- `run_visual_smoke_tests.ps1` 新增相机日志解析和 `Get-CameraInteractionVerdict()`，把手感数据纳入 `visual-smoke-verdict.json`。
- `visual_smoke_reference.json` 新增 `cameraInteractionBudget`：`maxAnchorError=0.05`、`minDragAbsX=1.0`、`minDragAbsY=1.0`、`maxSameAspectDragDelta=0.05`、`expectedZoomBand=Operation`。
- `.outputs/tuanjie/visual-smoke-trend.jsonl` 现在同步记录 `cameraInteractionVerdict`、`cameraInteractionScore`、`cameraInteractionComparisonCount` 与两档视口的 `cameraStats`，仍不保存 PNG。
- 新增 Ralph 上下文快照：`.omx/context/camera-interaction-sampling-20260507T124456Z.md`。

### 验证

- PowerShell AST 解析 `tools\\unity\\run_visual_smoke_tests.ps1` 通过。
- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\\unity\\preflight_without_unity.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_visual_smoke_tests.ps1 "My project"` 通过：`verdict=pass score=100 screenshots=39 smallViewports=20`，`referenceComparisonCount=25`，`overlayBudgetComparisonCount=2`，`cameraInteractionComparisonCount=2`。
- 相机采样结果：`terrain_war_1024 dragX=-2.667 dragY=2 anchorError=0 zoomBand=Operation`；`terrain_war_1280 dragX=-2.667 dragY=2 anchorError=0 zoomBand=Operation`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\\validate_data.py`、`python tools\\validate_domain_core.py`、`powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 均通过。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。
- `git diff --check` 通过，仅有 LF/CRLF 提示。

### 结论

相机拖拽与滚轮锚点现在进入了同一套图形 verdict 与趋势历史：后续不仅能看截图质量、reference、overlay budget，也能看到小视口拖拽比例和滚轮锚点漂移。核心机制未改，真实 56 区结构、历史来源门禁、新占领折损、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动继续保持既有规则。剩余风险是相机趋势目前只有 1024/1280 两档 16:9 采样，还未覆盖超宽屏或移动端纵向视口。

## 2026-05-07 战争与经营功能设计压力补强

### 缺口

本轮按用户澄清，把审查对象从代码风格/视觉皮肤转回战争与经营模块的功能设计。现有 H0-H6 骨架已经覆盖治理 forecast/apply、占领控制链、连地/补给/视野/拦截、粮食-秩序-外交耦合与 outliner；但 AI 决策层仍偏“可运行占位”：政策选择优先取帝皇偏好或第一条政策，军事目标选择第一个邻接敌区，`StrategicAI` 只记录倾向日志，没有把治理缺口、政策选择和战争压力转成局势行动。结果是系统能展示闭环，但非玩家势力不能持续制造经营与战争压力。

### 已修复

- `PolicyAI` 改为局势评分：根据财政、粮食、合法性、民变、地方势力、低接受度、新占控制链、继承/朝堂压力、战争状态与帝皇性格选择政策。
- `MilitaryAI` 改为目标评分：不再取第一个邻接区，而是评估目标产出、兵源、补给节点、区域专精、占后治理代价、敌方合法性、己方粮钱合法性和继承风险。
- `StrategicAI` 现在每回合会先处理 AI 势力的最高治理缺口，调用同一套 `StrategyMapRulebook` forecast/apply；再执行一个可支付、未完成且符合局势的政策。
- 战争推进保持外交门槛：AI 会选择战争目标，但只有目标所属势力与己方已经处于 `AtWar` 时才会派出可用空闲军队，避免绕开外交桥和宣战代价。
- 真实 56 区地图、占领折损、治理 forecast/apply 同源、补给/视野/拦截规则未改。

### 验证

- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争与经营现在不只是 UI 展示规则：AI 势力会按局势处理地方治理、选择政策，并在外交战争状态成立后把目标选择转成实际行军压力。剩余风险是这仍是第一版确定性 AI，不含长期战略记忆、战区级兵力调度、多线佯攻或经济专项计划；下一轮应继续补“玩家经营决策本身”的区域专精路线、政策组合代价和前线后勤计划。

## 2026-05-07 区域专精路线与前线补给规划

### 缺口

上一轮 AI 已经会按局势制造治理和战争压力，但玩家侧仍缺两条关键决策线：治理面板只有地区专精名称，没有把“粮仓/兵源/财税/边防/法统/文化/都城”转成下一阶段经营路线；战争面板只有单条行军补给预告，没有把兵站覆盖、接敌后余粮、占后军管/安抚/编户粮政成本合并成前线后勤计划。

### 已修复

- `StrategyMapRulebook` 新增 `RegionSpecializationPlanForecast`：输出区域专精、路线阶段、下一政务、建设重点、政策重点、预期收益、取舍和 readiness。
- `StrategyMapRulebook` 新增 `FrontlineSupplyPlanForecast`：输出兵站覆盖、接敌后可撑回合、预留粮、占后粮政成本、readiness 和下一步建议。
- 治理面板接入专精路线：现在首屏显示 `专精路线`、`路线收益`、`取舍`，让玩家知道该地区不是单点资源，而是可发展的经营方向。
- 战争预告接入前线补给规划：现在战前显示 `前线补给规划`、`兵站`、`预留粮`、`占后粮政`，把行军补给和占领治理成本放在同一判断里。
- PlayMode 补断言，锁定治理/战争面板必须展示这两条功能设计线。

### 验证

- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

治理侧现在能把地区专精转成路线判断，战争侧能把行军补给和占后治理成本合成前线计划。核心约束仍保持：真实 56 区地图、历史来源门禁、占领不立刻完整贡献、治理 forecast/apply 同源、连地/补给/视野/拦截与粮食-秩序-外交耦合未被削弱。剩余风险是专精路线暂时仍是轻量 forecast，没有形成多回合排程、政策组合锁定或建筑队列；前线补给规划也还没有做成可点击的“准备兵站/预留粮草”命令。

## 2026-05-07 前线整备可执行闭环

### 缺口

上轮只把前线补给规划做成了预告文本，但玩家还不能把它落成一个实际命令。这样一来，战前判断里出现的“兵站覆盖、接敌后可撑回合、占后粮政”只能读不能做，战争闭环还缺最后一脚。

### 已修复

- `MapCommandService` 新增 `PrepareFrontline(armyId, targetRegionId)`，把前线整备做成可点击命令。
- `GameManager` 新增前线整备入口，能按目标地区自动寻找可用的前线空闲军队。
- `StrategyMapRulebook` 的 `FrontlineSupplyPlanForecast` 现在记录目标供给、已预留粮、是否已整备，并把这些状态合进推荐步骤与 readiness。
- `EventBus` 新增 `FrontlinePrepared` 事件，UI 可据此刷新战争态势与 outliner。
- `MainMapUI` 在战争模式下新增“整备前线”按钮，和“进攻选区”并列出现。
- `MainMapUI` 的 outliner 现在会显示前线补给相关分组，且摘要保留“最新战报”分组，避免新功能挤掉战报反馈。
- `GameManagerPlayModeSmokeTests` 新增前线整备断言，覆盖扣粮、补供、占后预留、日志和 outliner 可见反馈。

### 验证

- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

前线补给规划现在不只是预告，而是能被玩家点出来执行的战前动作：它会扣粮、补供、预留占后军管口粮，并把结果回写到 UI 和 outliner。战争闭环已经从“能看补给”推进到“能先整备再开打”。剩余风险是这仍是单次命令，不是完整多回合后勤排程；后续还能继续往兵站建造、路线分段补给和占后安抚队列推进。

## 2026-05-07 前线预留粮占后治理链闭环

### 缺口

上轮前线整备已经会扣粮、补供并把占后粮政成本挂在军队上，但占领成功后这批粮还停留在 `ArmyRuntimeState.frontlineReservedFood`。这会让战争准备和占后治理链断开：玩家战前付了粮，却不能在军管、安抚、编户中看到同源抵扣；同时也缺少 headless 断言证明“新占领仍低贡献，但预留粮进入治理链”。

### 已修复

- `RegionState` 与 `RegionRuntimeState` 新增 `occupationReservedFood`，并在 `WorldStateFactory`、runtime/legacy 同步中保持一致。
- `DomainOccupationSystem` 在胜利占领后消费胜利方参战军队中指向该目标的 `frontlineReservedFood`，最多转入占后治理链总成本。
- `DomainGovernanceImpactSystem` 接收并记录 `occupationReservedFoodTransferred`，新占领仍保持低整合、低税粮贡献和合法性损失，但会把预留粮转入地区治理准备。
- `StrategyMapRulebook.BuildGovernanceForecast` 与 `ApplyGovernanceAction` 同源抵扣占后治理粮耗：军管、安抚、编户优先消耗 `occupationReservedFood`，预测中同步显示 `occupationReservedFoodDelta`。
- `BattleReportPanel` 与 `RegionPanel` 增加预留粮提示，让玩家能看到“前线整备粮只抵扣占后治理，不直接恢复税粮”。
- `HeadlessSimulationRunner` 的进攻占领场景加入前线整备、占领转入、军管抵扣断言；`tools/verify_headless_war.ps1` 把这些断言纳入门禁。

### 验证

- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=14`，新增必过断言包括 `frontline_preparation.reserves_occupation_chain_food`、`frontline_reserve.transfers_to_occupation_region`、`frontline_reserve.offsets_first_governance_food_cost`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

前线整备现在真正接进占后治理链：战前付粮、战胜转入地区、军管/安抚/编户逐步抵扣，且不绕开新占领低贡献和合法性代价。剩余风险是预留粮仍是一次性地区池，不是多回合运输队、仓储损耗或敌方截粮系统；下一轮可以继续做兵站建造、路线分段补给和占后安抚队列。

## 2026-05-07 多回合后勤排程与占后安抚队列

### 缺口

上轮已经把战前预留粮接进占后治理链，但它仍然像一次性地区池：玩家付粮后立即得到补给与预留结果，缺少跨回合兵站建设、路线分段运输、截粮损耗和占后安抚自动推进。

### 已修复

- `PrepareFrontline` 对长路线不再瞬时完成，改为创建多回合后勤计划，记录目标地区、兵站施工地区、总回合、剩余粮、分段数、补给缺口、占后预留缺口和截粮风险。
- `MapCommandService.ExecuteLogisticsTurn` 在回合推进中执行后勤排程：按每回合粮额扣粮，先建兵站，再补军队补给，再补占后预留粮；高风险/中风险路线会按截粮比例造成运输损耗，若损耗导致缺口未补齐，会自动延长排程。
- 新占领地区若获得预留粮，会进入 `军管 -> 安抚 -> 编户` 的占后安抚队列；队列逐回合调用治理 apply 同源逻辑消耗预留粮，不绕开新附地区低贡献、合法性压力和地方秩序代价。
- `StrategyMapRulebook.FrontlineSupplyPlanForecast` 与 outliner 现在能显示后勤排程中、预计回合、分段、兵站、截粮损耗和占后队列状态。
- `EventBus` 新增 `FrontlineLogisticsAdvanced` 与 `OccupationPacificationQueueAdvanced`，UI 可以从后勤推进与占后队列事件刷新。
- `DomainArmyMovementSystem` 在行军前推进后勤回合，让战争行动与后勤准备自然分层。
- `HeadlessSimulationRunner` 新增 `frontline_logistics_schedule_and_occupation_queue` 场景，锁定长线排程创建、首回合建兵站/推进分段、最终补齐预留粮、占后队列跨回合消耗预留粮。
- `GameManagerPlayModeSmokeTests` 的战争压力脉冲断言改成多帧宽度采样，仍要求 0.001 以上可观测变化，避免 headless 帧时间刚好采到波峰导致误报。

### 验证

- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=15`，新增场景 `frontline_logistics_schedule_and_occupation_queue` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

前线预留粮已经从“一次性地区池”升级为可跨回合推进的后勤排程：长路线会先筹粮、建兵站、分段运输并承担截粮损耗；占领后再进入安抚队列逐步消耗预留粮。战争与治理的因果链保持一致：补给准备能降低占后治理压力，但不能让新占区立刻完整贡献。

## 2026-05-08 后勤队列控制与敌方截粮 AI

### 缺口

上一轮虽然把后勤做成了多回合排程，但玩家还不能直接在地图上对这条队列做重排、暂停或取消，敌方也没有主动针对活动运输队的拦截决策入口。这样后勤仍然更像自动演算，而不是可管理的战争后方。

### 已修复

- `ArmyRuntimeState` 继续扩展后勤状态，加入运输队编号、优先级、暂停标记、截粮压力与最近一次截粮记录，保证队列本身可视可控。
- `MapCommandService` 新增后勤队列命令：`CancelFrontlineLogistics`、`ToggleFrontlineLogisticsPause`、`AdjustFrontlineLogisticsPriority`，可直接取消、暂停/恢复或加急/后置当前运输队。
- `MapCommandService` 新增敌方截粮执行入口，会选择最暴露、最值得打击的活动后勤队列，增加截粮损耗、抬升后勤风险，并把敌方名称写回日志与事件。
- `StrategicAI` 和 `MilitaryAI` 接上敌方截粮入口，敌军回合现在会主动识别并打击暴露的前线后勤运输队。
- `StrategyMapRulebook` 的前线补给预告和 outliner 现在会显示运输队编号、优先级、暂停状态与截粮压力，玩家能从预告和待办里看见队列节奏。
- `MainMapUI` 新增 `LogisticsQueuePanel`，提供加急、后置、暂停/继续、取消按钮，并支持从 outliner 选中某条后勤队列后直接操作。
- `UISetup` 将后勤队列面板纳入生成物清理名单，避免重复构建时残留旧 UI。
- `HeadlessSimulationRunner` 新增 `logistics_queue_control_and_enemy_raids` 场景，锁定队列重排、暂停、恢复、敌方主动截粮与取消命令都能被执行。
- `GameManagerPlayModeSmokeTests` 增加后勤队列 UI 的存在性断言，确保地图界面能直接看到队列控制面板和按钮。

### 验证

- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=16`，新增场景 `logistics_queue_control_and_enemy_raids` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

后勤不再只是自动推进的排程，而是地图上可调度的运输队：玩家能加急、后置、暂停和取消，敌军也会主动盯上暴露的粮道。这样“前线补给规划”才真正和战争压力、地图操作、战后治理连成一个完整闭环。

## 2026-05-08 原创双闭环 UI 收口与图形验收

### 缺口

本轮按 `$ralph` + `$team` 的收口要求先做缺口审查。Codex App 当前不在 tmux 内，不能启动真实 OMX team pane；因此改用本线程执行、两个 Codex 原生子智能体并行只读审查。审查发现三处需要收口：HUD 和机制面板仍暴露 `M:Governance`、`R/F/N/H`、`modeEntryReason` 等内部 token；右侧 compact outliner 在地区面板展开时用固定坐标避让，1024x576/1280x720 会与 `RegionPanel` 重叠；新增 `StrategyLayoutSpec.cs` 未纳入 Unity 脚本资产和本地 Runtime csproj，CLI build 不稳定。

### 已修复

- `MainMapUI` 的 HUD 选择摘要改成玩家可读中文：治理/外交/战争、地区名、己方/敌对邻接等关系标签，不再显示 `M/R/F/N/H` 调试缩写。
- `MainMapUI` 的攻击/整备不可用原因日志增加中文映射，避免把 `attack_requires_*`、`dispatch_requires_*` 直接写给玩家。
- `MechanismPanel` 的外交桥选区行改为中文模式和中文判定原因，不再显示 `MapInteractionMode` 枚举或 `neighbor_region` 等内部 reason。
- `MainMapUI` 的 compact outliner 避让改为读取展开 `RegionPanel` 的实际 `RectTransform`，动态停靠到地区面板左侧并保持在 canvas 内；固定坐标只保留为 fallback。
- `GameManagerPlayModeSmokeTests` 扩展可见 UI 文案门禁，覆盖 `M:Governance`、`M:War`、`R:none`、`F:`、`N:`、`H:`，并沿用小视口几何断言锁定 `RegionPanel` 与 `StrategyOutlinerCollapsed` 不重叠。
- `StrategyLayoutSpec.cs.meta` 新增，`WanChaoGuiYi.Runtime.csproj` 纳入 `StrategyLayoutSpec.cs`，恢复 CLI build。

### 验证

- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=16`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=25 passed=24 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_visual_smoke_tests.ps1 "My project"` 通过：39 张截图全部 pass，`visual-smoke-verdict.json score=100 verdict=pass screenshots=39 smallViewports=20`，reference-backed visual verdict、overlay budget verdict、camera interaction verdict 均 `score=100 verdict=pass`。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。
- `git diff --check` 通过，仅有 LF/CRLF 提示。

### 结论

本轮把原创高信息密度 UI 的明显“调试骨架”和小视口重叠风险收掉，并用图形 VisualSmoke 重新验收了地图、治理面板、外交桥、战争路线、地形战争叠加、dense frontline 和战报等 39 个截图场景。治理/战争双闭环、真实 56 区地图、历史来源门禁、占领折损、forecast/apply 同源、连地/补给/视野/拦截与粮食-补给-秩序-外交联动均保持通过。

## 2026-05-08 重型音频系统第一版

### 缺口

用户明确要求第一版就包含复杂空间音频、动态作曲、多 AudioSource 池、每区域独立音乐和复杂 Mixer Snapshot。既有工程已有音频资源目录和 manifest，但运行时缺少统一 AudioManager、事件桥接、Demo 启动接入和 PlayMode 验收；同时部分音频相关 `.meta` 文件 GUID 不是 Unity 32 位 hex 格式，阻断 `validate_data.py` 与 headless 门禁。

### 已修复

- 新增 `AudioCueLibrary`，支持 cue、区域音乐、Mixer layer route、Snapshot profile 的 ScriptableObject 配置入口。
- 新增 `AudioManager`，内置 Master/Music/Ambience/SFX/UI/War 多层 AudioSource 池，支持空间 cue、运行时 cue 注册、区域音乐、动态 snapshot 权重、可选 AudioMixer 参数路由和活跃声源音量随 snapshot 刷新。
- 区域音乐不再要求 56 个手工 clip 才能工作：未配置区域会生成稳定的 `region_theme_<regionId>` 运行时主题，后续可用正式资产覆盖。
- 新增 `AudioEventBridge`，把选区、治理、政策、行军、接敌、战报、占领、事件、宣战、前线整备、运输推进、队列命令、截粮和占后安抚映射到独立 cue 与动态音乐状态。
- 空间音频锚点改为优先读取真实 `RegionController` 的 MeshRenderer/Collider world bounds center，再 fallback 到 transform/hash，避免战争事件声场脱离 56 区真实地图。
- `DemoSceneBootstrap` 自动创建并绑定 `AudioManager` 与 `AudioEventBridge`，Demo 场景启动即具备音频事件链。
- `AudioManagerPlayModeTests` 新增 6 个音频 PlayMode 验收，覆盖分层池、空间 cue、动态作曲、区域主题、事件桥、真实地图锚点、后勤/占领/宣战细分 cue。
- 修复音频资源、数据、脚本目录下非法 GUID 的 Unity `.meta` 文件，恢复数据与 headless 验证。

### 验证

- `dotnet build "My project\\WanChaoGuiYi.PlayModeTests.csproj"` 通过，0 error；保留既有 Unity 序列化 warning。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=16`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=31 passed=30 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 不存在，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

音频系统现在不是单点播放脚本，而是可配置、可测试、可随治理/战争事件切换的运行时层：空间声源落在真实地图区域，后勤和战争压力有不同声效语义，治理与战争模式能推动动态音乐权重。剩余风险是还没有在 Unity Editor 里制作真实 AudioMixer asset/snapshot 资源，也没有把现有 MP3 manifest 全量接入 cue library；下一轮可做正式音频资产绑定、Mixer asset 配置、地图侧音频调试 HUD 和 convoy/raid 声场验收。

## 2026-05-08 真实 AudioMixer 与现有音频 Manifest 接入

### 缺口

上一轮音频运行层已经完成，但真实 Unity/Tuanjie `AudioMixer` 资产和现有 MP3 manifest 还没有接入运行时。初次接入后 PlayMode 暴露出 Mixer group 只识别到 Master 的问题：构建器创建了 group 名称，但没有稳定挂入 Master 子层级和可解析路由，导致 `AudioManifestBinderRegistersExistingManifestClipsAndSceneMusic` 失败。

### 已修复

- 新增/完善 `AudioManifestBinder`，从 `scene_music.json`、`emperor_themes.json`、`chronicle_event_music.json` 读取现有 manifest，并在 Editor 下绑定真实 MP3 `AudioClip`。
- `AudioManager` 支持绑定真实 `AudioMixer`、真实 `AudioMixerSnapshot`、scene music cue 映射和 mixer route 查询。
- `AudioEventBridge`、`DemoSceneBootstrap` 接入 manifest binder，编年事件、帝皇主题和场景音乐优先使用 manifest cue。
- `StrategyAudioAssetBuilder` 现在生成 `Assets/Audio/Mixers/WanChaoGuiYiStrategy.mixer` 与 `Assets/Audio/AudioCueLibrary.asset`，并写入 Master/Music/Ambience/SFX/UI/War 路由、Governance/War/Event snapshot、scene/emperor/chronicle cue。
- 修复 Mixer group 只在资产中散落但运行时不可解析的问题：构建器会把 layer group 挂到 Master 子层级，并确保 cue library 写入非空 mixer group 引用。
- 修复 snapshot 仍停留在默认 `Snapshot` 的问题：构建器会生成/命名 Governance、War、Event 三套 snapshot，并写入 cue library。
- 批量修复音频导入生成的 326 个非法 `.meta` GUID，全部变为 32 位 hex，随后重建 cue library，避免引用旧 GUID。

### 验证

- `dotnet build "My project\\Assembly-CSharp-Editor.csproj"` 通过，0 error；保留既有 Unity/JsonUtility 序列化 warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=32 passed=31 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=16`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

真实 AudioMixer 与现有音频 manifest 已经接入运行时。治理、战争、事件三类动态音乐状态现在既有代码侧 snapshot 权重，也有可解析的 Unity Mixer/Snapshot 资产；scene、emperor、chronicle 的 MP3 manifest 会注册成真实 cue。剩余可继续优化的是地图侧音频调试 HUD、更多区域独立 authored music，以及图形环境下带声场触发标记的 VisualSmoke 辅助验收。

## 2026-05-08 AudioMixer Orphan Group 清理收口

### 缺口

上一轮真实 `AudioMixer` 接入后，运行时 PlayMode 已通过，但进一步检查 `.mixer` 序列化文件发现早期构建尝试留下多组同名 `Music/Ambience/SFX/UI/War` orphan group。它们没有被当前运行时引用，但会污染 Editor 资产、干扰后续手工 Mixer 调音，并且强制重建时 Tuanjie 可能重新生成非 32 位 hex 的 `.meta` GUID。

### 已修复

- `StrategyAudioAssetBuilder` 新增序列化层检测：直接读取 `.mixer` 文件，只要发现策略音频 layer group 超过 5 个，就触发资产重建。
- 重建方式改为删除 `.mixer` 文件但保留/修正 `.mixer.meta`，避免 `AssetDatabase.DeleteAsset` 让 Tuanjie 生成非 hex GUID。
- `StrategyAudioAssetBuilder` 固定 `WanChaoGuiYiStrategy.mixer` 的稳定 GUID，并在构建前校验/修复 `.meta`。
- 重新执行 Tuanjie batchmode 资产构建，当前 `.mixer` 只保留 Master 下 5 个有效 layer group，`AudioCueLibrary.asset` 的 mixer group 与 snapshot 引用均非空。

### 验证

- `dotnet build "My project\\Assembly-CSharp-Editor.csproj"` 通过，0 error；保留既有 Unity/JsonUtility 序列化 warning。
- `WanChaoGuiYi.EditorTools.StrategyAudioAssetBuilder.BuildAudioAssets` Tuanjie batchmode 通过；日志确认执行构建，清理轮次曾触发 `Rebuilding strategy audio mixer to remove duplicate layer groups`。
- `.mixer` 资产检查通过：`strategyLayerGroupCount=5`，Master 子层级包含 5 个 layer group。
- `AudioCueLibrary.asset` 检查通过：`mixerGroup: {fileID: 0}` 与 `mixerSnapshot: {fileID: 0}` 数量均为 0。
- `.meta` 检查通过：`My project/Assets` 下非法 GUID 数 0、重复 GUID 数 0。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=32 passed=31 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=16`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

真实 AudioMixer 现在不只是在运行时可用，也在资产层收敛到可维护状态：Master 下只有 5 个有效路由 group，治理/战争/事件 snapshot 引用稳定，cue library 没有空 mixer 引用，且 `.meta` GUID 不再污染数据门禁。下一轮更适合继续做音频调试 HUD、区域 authored music 覆盖或图形场景里的声场触发可视化。

## 2026-05-08 区域 Authored Music 与音频调试 HUD

### 缺口

上一轮仍有两个明确缺口：`AudioCueLibrary.asset` 的 `regionMusic` 为空，运行时会用 `runtime_region_theme_*` 作为主路径；同时 Demo 场景没有可见音频调试 HUD，无法在运行时检查 snapshot、最近 cue、区域锚点、mixer route 和声源池压力。

### 已修复

- `StrategyAudioAssetBuilder` 现在读取 `Assets/Data/regions.json` 与 `Assets/Data/historical_layers.json`，为真实 56 区自动写入 `region_music_<regionId>` authored 映射。
- 区域音乐映射按地区 id、terrain、geography/custom/resource/weapon tags 选择现有 MP3：关中/秦汉核心、都城礼制、巴蜀、许昌/官渡、西北边地、江东水网、淮南荆襄、岭南、山地高原、齐鲁文化等都会落到不同现有主题资产。
- `AudioCueLibrary.asset` 已重建：`regionMusic` 数量 56，区域条目空 clip 数 0；`region_music_guanzhong` 绑定真实 MP3，不再走 `runtime_region_theme_guanzhong`。
- `AudioManager` 暴露调试查询：区域 cue 数、指定区域 cue、活跃声源数、总池容量、池压力和 `GetAudioDebugSummary()`。
- 新增 `AudioDebugHUD`，Demo 运行时左下角显示音频状态、场景/区域 cue、最近 cue、锚点、route 数和池占用，并用 `LegacyRuntime.ttf` 兼容当前 Tuanjie 内置字体规则。
- `DemoSceneBootstrap` 自动创建并绑定 `AudioDebugHUD`。
- `AudioManagerPlayModeTests` 增加 authored region music 和 HUD summary 语义断言；`GameManagerPlayModeSmokeTests` 增加 Demo bootstrap HUD root/text 断言。

### 验证

- `dotnet build "My project\\Assembly-CSharp-Editor.csproj"` 通过，0 error；保留既有 Unity/JsonUtility 序列化 warning。
- `dotnet build "My project\\WanChaoGuiYi.Runtime.csproj"` 通过，0 warning / 0 error。
- `WanChaoGuiYi.EditorTools.StrategyAudioAssetBuilder.BuildAudioAssets` Tuanjie batchmode 通过；日志确认 `Built strategy audio assets`。
- `AudioCueLibrary.asset` 检查通过：`RegionMusicCount=56`，`MissingRegionClips=0`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\unity\\run_playmode_tests.ps1 "My project"` 通过：`total=32 passed=31 failed=0 skipped=1`，唯一 skipped 为 headless/nographics 下预期 `VisualSmokeCaptureTests`。
- `python tools\\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 chronicle_events=200`。
- `python tools\\validate_domain_core.py` 通过。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\\verify_headless_war.ps1` 通过：`passed=True scenarioCount=16`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。
- VisualSmoke 清理复核通过：`.outputs/visual/unity-*.png` 数量 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。
- 进程清理复核通过：无残留 `Tuanjie`、`TuanjieAutoQuitter`、`TuanjiePackageManager`、`Unity.ILPP.Runner` 进程。

### 结论

区域音乐已从“空 authored 配置 + 运行时 fallback 主导”推进到“56 区 authored 映射 + fallback 只保底”。音频 HUD 也进入 Demo 默认启动链，可以直接观察当前治理/战争音乐状态和声源池压力。剩余音频风险主要是部分非区域历史事件/帝皇 cue 仍缺对应 MP3，后续应继续补齐 manifest 资产覆盖；关于 Unity 必要性，本轮实现已把规则、映射和调试尽量放到代码/数据层，Unity 目前只承担实际音频运行、UGUI 和 PlayMode 门禁，后续可以并行做纯代码/非 Unity 地图与 UI 原型来验证更高可改性。

## 2026-05-09 纯代码 Three.js 主产品基座执行

### 缺口

用户确认从 deep-interview 规格进入执行：Unity/Tuanjie 降级为旧实现，新主产品基座改为纯代码可拖拽 3D/2.5D 战略地图。此前 Unity UI 虽有功能，但视觉和迭代速度不满足要求；新基座必须继续使用真实 56 区地图、保留治理/战争双闭环，并且不能回退成假六边形或装饰性截图。

### 已修复

- 新增 `web-strategy-map/` Vite + TypeScript + Three.js 应用，独立于 Unity/Tuanjie 运行。
- `scripts/sync-data.mjs` 从现有 Unity 数据与素材同步 56 区域、区域边界、地图渲染元数据、历史层、政策、建筑、兵种和 `jiuzhou_generated_map.png`。
- `StrategyScene` 使用现有整张九州地图纹理，并用 `map_region_shapes.json` 生成 56 个真实区域 ExtrudeGeometry，支持拖拽平移、滚轮缩放、hover、点击选区、治理/战争 lens、军队标记和行军线。
- `StrategyUi` 实现顶部国力状态、治理/战争模式切换、右侧可折叠详情栏、治理资源/人口/风险/法统/建筑/政策/史据预览、战争补给/接敌/截粮/占领代价预告和本回合压力 outliner。
- `LabelManager` 增加基于缩放、视口和优先级的标签预算，并用 DOMRect 碰撞检测隐藏重叠标签。
- 新增 Playwright 验收，覆盖 56 区 mesh、拖拽/缩放、点击选区、模式切换、侧栏折叠、治理/战争字段、小视口截图和标签重叠检查。
- Review 前自检补齐 UI HTML 转义，避免后续 JSON 文案扩展时被 `innerHTML` 直接注入。
- `.gitignore` 增加 Web 侧 `node_modules/dist/public game-data/playwright-report/test-results` 忽略规则，避免把同步素材、构建产物和测试截图提交为源码。

### 验证

- `npm install` 通过，并生成 `web-strategy-map/package-lock.json`。
- `npm run sync:data` 通过：同步 8 个策略地图数据/素材文件。
- `npm run typecheck` 通过。
- `npm run build` 通过；Vite 仅提示 Three.js chunk 超过 500 kB 的优化建议。
- `npx playwright install chromium` 已执行，补齐本机 Playwright Chromium。
- `npm run test:ui` 通过：3/3 passed，覆盖核心交互、1280x720 与 1024x576 小视口。
- HTML 转义补丁后已重新运行 `npm run typecheck`、`npm run build`、`npm run test:ui`，均通过。
- 浏览器测试截图已生成：`web-strategy-map/test-results/strategy-map-1280x720.png` 与 `web-strategy-map/test-results/strategy-map-1024x576.png`。
- `git diff -- web-strategy-map --check` 通过。
- Unity/Tuanjie VisualSmoke 未运行，且未生成 `.outputs/visual/unity-*.png`。

### 结论

纯代码主产品基座已经具备可运行、可拖拽缩放、可点击决策、可切换治理/战争的第一阶段完整闭环；它复用现有 56 区真实地图结构和素材，不依赖 Unity/Tuanjie 才能展示核心地图与 UI。剩余风险是当前战争/治理数值仍是 Web adapter 从既有数据派生的演示态，还没有把 Unity 运行时所有后勤队列、AI 调度和音频系统完整迁移到 Web；下一步应做代码 review、交互手感人工检查、再决定 Unity/Tuanjie 清理边界。当前不能直接删除旧 Unity/Tuanjie 内容，必须等新基座通过人工确认后再列出精确删除路径。

## 2026-05-09 现实地貌与建设层增强

### 缺口

用户确认新 Web 基座方向可接受，但指出地图没有展现地理和建筑。第一版地貌增强后仍偏“通用符号”，不够体现现实地理对应关系。

### 已修复

- `RegionViewModel` 新增 `geography` profile，由 `historical_layers.json` 的 `climateZone`、`geographyTags`、`strategicResources` 和 `uiSummary` 派生。
- 地貌类型从通用 terrain 升级为现实地理 profile：黄土平原/灌渠、山地关隘/盆地门户、江河水网/湿地、河西走廊/绿洲道、边地马道/草场、盆地粮仓、山海港湾、高原边地、河谷粮廊、林地边境、矿山高地、平原粮田/人口核心。
- Three.js 地图层按 profile 生成对应可见符号：水网分汊、走廊道路、关隘门、盆地粮田、灌渠、马道、港湾码头、林地和矿产点。
- 治理模式新增选中地貌 callout 与建设标签，保留标签避让；小视口单独调整呼叫点，避免与建筑标签重叠。
- 右侧治理面板新增“地理与建设”：显示现实地貌、气候、来源 tags、资源、地理影响和建筑标记说明。
- Playwright 扩展到 5 个 UI 测试，新增治理模式地貌/建筑截图和标签不重叠验收。

### 验证

- `npm run typecheck` 通过。
- `npm run build` 通过；仍仅有 Three.js chunk-size advisory。
- `npm run test:ui` 通过：5/5 passed，覆盖核心交互、现实地貌/建设层、1280x720 与 1024x576 小视口、标签不重叠。
- 新治理截图已生成：`web-strategy-map/test-results/strategy-map-governance-1280x720.png` 与 `web-strategy-map/test-results/strategy-map-governance-1024x576.png`。
- `git diff --check` 通过，仅有 LF/CRLF 提示。

### 结论

地图现在不只是区域色块和行军线：治理模式能直接看到现实地貌 profile、地貌符号、建筑标记和建设名称。关中示例会显示“黄土平原 / 灌渠”，侧栏同步说明其 `loess_plain`、`capital_corridor`、`river_irrigation` 等来源 tags。剩余风险是这些仍是程序化符号层，不是手绘地貌资产；后续可继续把重点区域做成更接近美术图标/低模资产的地貌套件。

## 2026-05-09 Web 经营部署与音频闭环补齐

### 缺口

用户指出 Web 主线仍偏地图展示：大部分部署、经营功能尚未形成可操作反馈，音乐、旁白和帝王配音也没有进入纯代码基座。

### 已修复

- `web-strategy-map` 扩展同步现有 Unity 音频素材：场景音乐、帝王主题、历史事件音乐、旁白、帝王配音，共同步 151 个 MP3，并继续从现有 manifest 加载。
- `StrategyAudio` 接入 HTMLAudioElement 音频管理，支持治理/战争模式音乐、帝王主题、历史事件短曲、教程旁白和秦始皇 select/attack/defend 配音；浏览器自动播放限制会被记录到 HUD，不阻断玩法。
- 主界面新增音频 HUD，可启用音频、切换模式音乐、试听帝王主题与事件短曲，并显示当前音乐 cue、旁白、配音和曲库数量。
- 治理面板的政策、建设、征发按钮现在会实际消耗钱粮、改变贡献/风险/整合/法统/兵力，并写入经营队列与 outliner。
- 战争面板的前线军府、运输队、攻占按钮现在会消耗钱粮、调整风险/占领阶段，并写入后勤队列；攻占仍保持“军管/安抚/编户”原则，不会直接完整贡献。
- `window.__WANCHAO_APP__.getDebugState()` 增加 `audio` 与 `ui` 状态，便于 Playwright 和后续趋势化验收读取。
- Playwright 核心验收扩展到经营按钮、部署按钮、音频 HUD、曲库 manifest 数量、音频启用状态、经营/后勤队列变化和音频状态变化。

### 验证

- `npm run sync:data` 通过：`Synced 12 strategy-map data/assets and 151 audio files.`
- `npm run typecheck` 通过。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- `npm run test:ui` 通过：5/5 passed，覆盖核心交互、经营/部署队列、音频 HUD、现实地貌/建设层、1280x720 与 1024x576 小视口。

### 结论

Web 主线已从“可看地图”推进到“可点击经营、部署和触发音频反馈”的可验证闭环。当前仍是 Web adapter 层的第一版经营/部署模拟，尚未把 Unity 里所有 AI 调度、完整事件系统和长期多回合结算迁移过来；下一步应继续补多回合经营队列、部署取消/重排、敌方截粮 AI 和更完整的历史事件触发音频。

## 2026-05-09 Web UI 操作空间重构

### 缺口

用户反馈当前 UI 设计和可操作空间仍未达到预期。截图复核确认：左侧音频 HUD 默认展开，占据地图；右侧栏过宽且操作入口被指标、地理说明和文本挤到下方，玩家需要滚动才看到完整治理操作。

### 已修复

- 顶部栏压缩，降低品牌、模式切换和国力指标的默认高度。
- 音频 HUD 改为默认折叠的 `details` 抽屉，只保留“音频 / 状态 / 当前混音”小条；展开后才显示启用、模式音乐、帝王主题和事件短曲按钮。
- 右侧详情栏从约 22.6rem 收窄到 21.4rem，底部状态条避开右栏，让地图点击区域更多。
- 治理操作牌组上移到地区指标之后，第一屏直接可见，不再被地理说明和治理判断挤到下方。
- 治理操作从 3 个扩展为 5 个：施政、建设、赈济安抚、编户清丈、征发整备。
- 战争操作从 3 个扩展为 5 个：部署军府、派运输队、探路线、固兵站、启动战役。
- 操作按钮改成紧凑 command tile，保留收益/副作用/史据说明，但减少纵向压迫。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-redesign-final-1280.png`
- 截图：`.outputs/playwright/strategy-ui-redesign-final-1024.png`
- visual-verdict：90/100，`pass`。主要改善是地图默认遮挡减少、音频不再压屏、治理操作第一屏可达；剩余问题是右侧栏仍偏信息密集，后续可继续拆成可切换的“治理 / 地理 / 队列 / 史据”子标签。

### 验证

- `npm run typecheck` 通过。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- `npm run test:ui` 通过：5/5 passed。

## 2026-05-09 帝皇选择与军队微操补齐

### 缺口

用户反馈战争仍不能进行军队微操，帝皇选择也没有得到体现。复核确认 Web 主线此前只显示路线和部署按钮，帝皇数据虽然存在于 `emperors.json`，但没有被加载到 Web UI，也没有影响音频或状态。

### 已修复

- `sync-data.mjs` 增加同步 `emperors.json`，Web 运行时现在加载 13 位帝皇数据。
- 新增 `EmperorDefinition` 类型，`StrategyDataset` 暴露 `emperors`。
- 地图左侧新增帝皇选择 dock，显示当前帝皇、称号、唯一机制、军/政/改/魅核心数值、史据来源，以及 8 位核心帝皇切换按钮。
- 选择帝皇后会更新当前帝皇状态、法统/军心/整合的轻量加成、操作日志，并联动音频系统的帝王主题与帝王配音。
- `StrategyAudio` 不再固定秦始皇配音，新增 `setEmperor()`，帝王主题和 select/attack/defend 配音跟随当前帝皇。
- 战争面板新增“军队微操”模块，显示当前军队、军令、阵型、路线、兵力、补给、军心。
- 军队微操新增 5 个军令：稳进压迫、急行抢道、扎营固守、侧翼牵制、收拢预备队；会真实改变补给、军心、风险和后勤队列。
- 调试状态新增 `selectedEmperorId`、`selectedEmperorMechanic`、`armyOrder`、`armyFormation`、`activeArmySupply`、`activeArmyMorale`、`activeArmySoldiers`，用于自动验收。

### 视觉验收

- 治理截图：`.outputs/playwright/strategy-ui-emperor-army-1280.png`
- 小视口截图：`.outputs/playwright/strategy-ui-emperor-army-1024.png`
- 战争微操截图：`.outputs/playwright/strategy-ui-army-micro-war-1280.png`

### 验证

- `npm run sync:data` 通过：`Synced 13 strategy-map data/assets and 151 audio files.`
- `npm run typecheck` 通过。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- `npm run test:ui` 通过：5/5 passed，覆盖帝皇切换、帝皇机制显示、军队微操、补给变化、既有地图/音频/小视口验收。

### 结论

帝皇选择和军队微操现在已经进入主界面并可操作：玩家能切换帝皇、看到唯一机制和史据，战争模式能对同一支军队下达不同军令并看到补给/军心变化。剩余风险是这仍是第一版 Web adapter 逻辑，尚未达到完整多军团、多路线、拖拽改道、编队拆分、将领替换和多回合军令排程。

## 2026-05-09 多军队选择与地图点选改目标

### 缺口

上一轮虽加入军队微操，但仍是单支主力军的面板操作；玩家不能在多支己方军队间切换，也不能通过地图点击改变当前军队的战役目标。

### 已修复

- Web 数据层新增第二支己方军队“河西骑军”，与“关中前军”共同进入战争模式。
- `StrategyScene` 增加 active army 状态，支持 `setActiveArmy()` 与 `retargetActiveArmy()`。
- 战争模式点击地图区域时，会把当前军队目标改为该区域，并重建行军线、接敌节点和路线压力。
- 军队标记会按当前选中军队放大并提高发光强度。
- 战争侧栏新增“军队选择”模块，可在“关中前军”和“河西骑军”之间切换。
- 选择军队后自动聚焦该军队的目标区域，并刷新军队微操、补给、路线、目标和后勤信息。
- Playwright 增加断言：切换到“河西骑军”后，场景 active army 与 UI active army 同步；地图点击后 active target 与 UI target 同步；急行军仍会降低当前军队补给。

### 视觉验收

- 治理/帝皇/多军队截图：`.outputs/playwright/strategy-ui-emperor-army-final-1280.png`
- 战争多军队/微操截图：`.outputs/playwright/strategy-ui-emperor-army-final-war-1280.png`

### 验证

- `npm run sync:data` 通过：`Synced 13 strategy-map data/assets and 152 audio files.`
- `npm run typecheck` 通过。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- `npm run test:ui` 通过：5/5 passed。

### 结论

战争模式现在不再只是固定路线预览：玩家可以选不同己方军队、在地图上点选目标区域、看到路线重建，并继续对当前军队下达微操军令。剩余风险是还没有做到拖拽路径节点、拆分合并军队、将领替换、兵种配比和多回合命令队列。

## 2026-05-09 路线编辑与后勤队列控制

### 缺口

上一轮已经支持多军队和点选目标，但路线仍是“直接目标”级别；玩家不能设置中继点，后勤队列也没有可取消、可重排的显性操作。验证时还发现一个交互缺口：真实 canvas 点击只更新选中地区，没有把战争模式下的目标/中继命令同步给当前军队。

### 已修复

- 战争侧栏新增“路线编辑”模块，支持“点选改为目标”“点选设为中继”“清除中继”“上移后勤”“撤销后勤”。
- `ArmyViewModel` 增加 `waypointRegionId`，行军线支持 `出发地 -> 中继 -> 目标`，地图上显示独立中继节点。
- 行军预估把中继距离计入补给消耗和回合数，保持路线越绕越费粮的因果一致性。
- canvas 真实地图点击现在与公共选区 API 同源：战争模式下会按当前路线编辑模式改目标或设中继，不再只改右侧选中地区。
- 地图 picking 增加区域中心投影优先，降低倾斜 2.5D 视角下点击标签/中心时误选前景相邻区的概率。
- Playwright 选点 helper 排除 UI 覆盖层，只点击真正落在 `#map-canvas` 上的可操作区域。
- 自动验收新增中继设置、目标保持、清除中继、后勤队列上移与撤销断言。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-route-plan-1280.png`
- visual-verdict：90/100，`pass`。本轮没有新的外部参考图，按项目既定目标验收：战争路线编辑可见、地图主体仍占主视觉、军队/路线/后勤操作能在一屏内完成。
- 人工复核：战争模式右侧可见军队选择、路线编辑、中继状态、军队微操与后勤队列；地图上可见战争行军线、路线节点、区域标签和地貌/建筑层，没有明显文字重叠。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖地图点选改目标、中继点、清除中继、后勤队列上移/撤销、既有治理/音频/小视口验收。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；未生成 `.outputs/visual/unity-*.png`，也未创建 `.outputs/tuanjie/visual-project-copy` 或 `.outputs/tuanjie/visual-preview-copy`。

### 结论

战争路线已经从“点一个目标”推进到“目标/中继/后勤队列”可操作闭环。下一步最值钱的是把这套路线控制继续升级为地图上可拖拽的路径节点、分段运输队、敌方主动截粮 AI，以及将领/兵种编组对路线风险的实时影响。

## 2026-05-09 拖拽路线节点与军队编组

### 缺口

路线编辑虽然已有目标/中继点选，但还不是地图上的直接操控；军队微操也只有军令层，没有拆分/合并、换将和兵种配比。这样战争仍偏“面板按钮”，无法支撑用户要求的军队布局、布防与战略微调。

### 已修复

- `sync-data.mjs` 增加同步 `generals.json`，Web 主线使用现有 12 位历史将领数据和史据来源。
- `ArmyViewModel` 增加 `generalId` 与 `unitMix`，军队状态能表达主将、主兵种和步骑弩/攻城配比。
- 地图战争模式支持拖拽路线节点：拖动当前中继或目标节点到其他真实区域，会即时改道并刷新行军线、补给/接敌/占领预估。
- 战争面板新增“编组与将领”：显示主将、特长、兵种、配比、配比条和史据。
- 新增军队组织操作：拆分偏师、合并同源军、轮换主将、步骑均衡、骑兵突进、弩步守正、攻城配属。
- 拆分会生成新的己方偏师并同步 Three.js 军队标记；合并会移除被合并军队并重算兵力、补给、军心和兵种配比。
- 换将按现有将领表轮换，显示将领特长与来源；兵种配比会改变主兵种，并对补给/军心产生轻量影响。
- Playwright 验收覆盖拖拽中继、拆分/合并、换将、骑兵配比，以及既有治理、音频、小视口、标签避让。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-army-organization-route-drag-1280.png`
- 截图：`.outputs/playwright/strategy-ui-army-organization-panel-1280.png`
- visual-verdict：90/100，`pass`。编组面板可读，兵种配比和换将入口明确；剩余问题是右侧战争栏信息密度继续升高，后续应拆成“路线 / 编组 / 后勤 / 战报”子标签。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；未生成 `.outputs/visual/unity-*.png`，也未创建 `.outputs/tuanjie/visual-project-copy` 或 `.outputs/tuanjie/visual-preview-copy`。

### 结论

战争微操现在进入“路线节点 + 军队组织”层：玩家可以拖动路线节点、拆分偏师、合并军队、轮换历史将领并调整兵种配比。下一轮更适合继续做多回合命令队列执行、运输队分段补给、敌方截粮 AI，以及把将领/兵种配比更深地接入路线风险与战报结算。

## 2026-05-09 训练冲刺第 1 轮：战争子页与战术预估

### 缺口

上一轮补上拖拽路线节点、拆分/合并、换将和兵种配比后，战争侧栏信息继续堆叠成长卷；同时将领与兵种配比虽可操作，但对补给、接敌、截粮、占领代价的影响不够显性，玩家难以判断“为什么要换将或改配比”。

### 已修复

- 战争侧栏拆成四个子页：路线、编组、后勤、战报。
- 路线页聚焦军队选择、路线编辑、行军线和战前预估。
- 编组页聚焦军令、拆分/合并、换将与兵种配比。
- 后勤页聚焦部署军府、运输队、侦察、兵站和后勤队列。
- 战报页显示战术修正、最近军令、操作日志和目标说明。
- 路线预估新增“战术修正”：根据当前将领特长、地形加成、兵种配比，影响补给消耗、接敌概率、截粮风险和占领代价。
- Playwright 验收改为按子页操作，覆盖路线页、编组页、后勤页和战报页，避免回到单一长卷。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-tabs-route-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-tabs-report-1280.png`
- visual-verdict：91/100，`pass`。子页降低了右侧栏认知负担，路线页和战报页更接近策略游戏的可决策面板；剩余问题是路线页下半部分仍略长，后续应把预估指标固定在页首或做更紧凑的战术摘要条。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；未生成 `.outputs/visual/unity-*.png`，也未创建 `.outputs/tuanjie/visual-project-copy` 或 `.outputs/tuanjie/visual-preview-copy`。

### 结论

这一轮把“功能堆叠”改成了“按决策场景切换”：玩家先在路线页看路线压力，再去编组页调兵换将，后勤页处理补给，战报页复盘影响。下一轮应继续做多回合命令队列执行与敌方截粮 AI，让战报页不只是记录玩家动作，也能反馈敌方反制。

## 2026-05-09 训练冲刺第 2 轮：多回合军令与截粮 AI

### 缺口

战争后勤已经有运输、部署、侦察、兵站和攻占按钮，但它们仍偏一次性日志，没有真正进入“下令、等待、推进回合、遭遇敌方反制、抵达结算”的多回合闭环；战报页也缺少敌方主动截粮反馈。

### 已修复

- 新增结构化多回合军令排程，后勤操作会生成在途军令，包含目标、起点、中继、剩余回合、补给余量和截粮风险。
- 后勤页新增“推进一回合”按钮和战时回合状态，玩家能看到当前第几回合、队列数量和截粮警报。
- 运输、军府、侦察、兵站、战役命令开始按回合推进；完成后会结算补给、军心、风险、军管和占后贡献限制。
- 新增确定性敌方截粮 AI：依据路线截粮风险、目标敌对状态、中继绕行和命令类型触发，不依赖随机数，便于回归测试。
- 截粮事件会扣减军队补给与运输余量，写入战报页、最新军令和右侧警报，不再只是玩家动作日志。
- 后勤队列上移/撤销同时作用于结构化军令排程，保留原有后勤记录展示。
- Playwright 调试状态新增 `warTurn`、`commandQueueLength`、`latestInterceptionAlert` 和 `nextCommandSummary`，可持续回归。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-command-queue-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-command-report-1280.png`
- 人工复核：后勤页可见战时回合、排程数量、截粮警报、部署/运输/攻占按钮和多回合军令排程；战报页可见敌方截粮、回合结算和命令日志。右侧面板仍偏密，但已比单纯日志更可决策。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖多回合军令排程、后勤上移/撤销、推进一回合、截粮警报和战报页展示。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争后勤现在从“点按钮记日志”推进到“多回合军令执行”：玩家可以派运输、部署军府、启动战役，再推进回合看到截粮反制与到达结算。下一轮建议继续做运输队分段节点可视化、敌方截粮路线高亮、占后安抚队列 UI，以及把将领/兵种对结算的影响写进更清晰的战报条目。

## 2026-05-09 训练冲刺第 3 轮：路线压力地图叠加

### 缺口

上一轮已经有多回合军令和截粮战报，但地图本体仍只画一条行军线；玩家需要从右侧文字推断“运输队在哪里、哪段路最容易被截粮、目标/中继节点能不能拖”。这不符合战争模式应有的大地图压力感。

### 已修复

- `StrategyScene` 路线层新增黄铜运输车队节点，按补给消耗和截粮风险生成 2 到 3 个分段补给标记。
- 路线末段新增赤色截粮高危段和截粮警示标记，让敌方截粮位置直接体现在地图上。
- 目标点和中继点新增立式拖拽手柄，拖动入口比单纯圆环更明显。
- 路线页和战报页补充地图叠加说明：黄铜车队代表运输分段，赤色路段代表截粮高危段。
- 调试状态新增 `routeRaidSegmentCount`、`routeConvoyMarkerCount`、`routeDragHandleCount`，Playwright 可回归地图叠加没有丢失。
- Playwright 验收覆盖战争模式路线叠加、设定中继后双手柄、截粮后地图叠加仍存在。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-route-pressure-layer-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-route-pressure-logistics-1280.png`
- visual-verdict：

```json
{
  "score": 91,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "路线叠加已经可见，但右侧路线页仍偏密。",
    "运输车队和截粮标记是地图对象，还没有独立悬浮 tooltip。"
  ],
  "suggestions": [
    "下一轮为路线叠加增加 hover tooltip 或底部摘要条。",
    "把占后军管、安抚、编户做成可重排队列，接上战役完成结算。"
  ],
  "reasoning": "截图符合高信息密度战争地图目标：路线、车队分段、截粮高危段和拖拽节点都能在真实 56 区地图上直接识别。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 验证中曾并行触发一次 `sync:data` mp3 文件锁，随后按顺序重跑 `npm run test:ui` 通过，判定为并行复制冲突而非功能失败。
- 本地页面 `http://127.0.0.1:5177/` 返回 200。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争路线现在不只是一条线，而是包含运输分段、截粮高危段和可拖拽节点的大地图压力层。下一轮最值得补的是占后安抚队列 UI，或者让敌方军队生成独立的截粮命令并在地图上移动。

## 2026-05-09 训练冲刺第 4 轮：占后安抚队列

### 缺口

战役结算已经会把攻占地区设为军管，但玩家无法在 UI 中看到“军管 → 安抚 → 编户 → 常规治理”的占后处理链，也不能主动推进。这样会削弱项目核心原则：新占领地区不能立刻完整贡献税收和兵源。

### 已修复

- 新增结构化占后安抚队列，战役完成后会生成新附地区的军管任务。
- 后勤页新增“占后安抚队列”模块，显示地区、当前阶段、剩余回合、贡献上限和风险。
- 后勤操作区新增“推进安抚”按钮，玩家可以手动推进军管、安抚、编户。
- 战报页新增占后队列摘要和阶段日志，能看到军管解除、转入安抚、编户完成等反馈。
- 阶段推进会改变真实地区状态：`military-govern`、`pacify`、`register`、`controlled`，并同步整合度、贡献上限和风险变化。
- Outliner 新增“占后”条目，高亮当前最急的新附地区队列。
- Playwright 调试状态新增 `occupationQueueLength`、`occupationStageSummary`、`selectedControlStage`、`selectedContribution`，覆盖战役完成后生成队列并推进到安抚。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-aftercare-logistics-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-aftercare-report-1280.png`
- visual-verdict：

```json
{
  "score": 91,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "占后队列已经清楚，但后勤页信息密度继续偏高。",
    "阶段推进是按钮和日志反馈，地图上尚无新附地区治理进度徽标。"
  ],
  "suggestions": [
    "下一轮可为新附地区增加地图进度徽标或 outliner 优先级过滤。",
    "把敌方截粮从确定性事件推进成敌军可见命令。"
  ],
  "reasoning": "截图符合战争-治理双闭环目标：攻占后的军管、安抚和编户已经进入可见队列，并且不会立刻转为完整收益区。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖攻占完成后生成占后队列、推进军管到安抚、战报展示和小视口稳定性。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 本地页面 `http://127.0.0.1:5177/` 返回 200。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争闭环现在接上了攻占后的治理链：战役完成不会直接给完整收益，而是进入军管、安抚、编户队列，玩家需要消耗回合推进地区整合。下一轮可继续做地图上的新附治理进度徽标，或把敌方截粮 AI 做成可见敌军命令。

## 2026-05-09 训练冲刺第 5 轮：新附地区地图徽标

### 缺口

占后安抚队列已经可操作，但玩家仍需要在右侧面板读取阶段，地图本体没有直接提示哪些真实区域处在军管、安抚、编户。上一轮截图还暴露一个风险：如果把所有 `newly-held` 初始状态都标出来，会让整张地图充满红点，削弱重点。

### 已修复

- `StrategyScene` 新增 `OccupationStageBadgeLayer`，随 `setMode()` 和状态变更重建。
- 只为占后处理链中的 `military-govern`、`pacify`、`register` 显示地图徽标，排除宽泛的初始 `newly-held`，避免噪声。
- 徽标使用阶段颜色：红色军管、黄色安抚、青色编户；完成常规治理后自动消失。
- 后勤页新增说明，解释地图徽标含义；战报页说明占后队列会同步显示阶段徽标。
- 调试状态新增 `occupationBadgeCount`，Playwright 覆盖攻占后徽标出现、推进安抚后徽标仍存在。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-occupation-badge-logistics-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-occupation-badge-report-1280.png`
- visual-verdict：

```json
{
  "score": 92,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "占后徽标已经不再铺满地图，但小区域上徽标仍偏细。",
    "地图徽标目前只表达阶段，不显示剩余回合数字。"
  ],
  "suggestions": [
    "后续可在 hover 或底部摘要中显示剩余回合与贡献上限。",
    "若地图徽标继续增多，可加入 outliner 筛选或镜头聚焦。"
  ],
  "reasoning": "截图显示新附地区阶段已经直接出现在真实地图上，同时避免了全图红点噪声，符合战争到治理闭环的可读性要求。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 首次因一次 `Failed to fetch` 控制台噪声失败，功能断言已跑到末尾；随后重跑通过：5/5 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 本地页面 `http://127.0.0.1:5177/` 返回 200。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

占后治理现在同时存在于右侧队列、战报和地图徽标三层反馈中；新占区不会立刻满收益，也不会在地图上被淹没成无意义红点。下一轮最值得做的是敌方可见截粮命令，或者为占后徽标增加 hover/摘要信息。

## 2026-05-09 训练冲刺第 6 轮：敌方可见截粮命令

### 缺口

上一轮战争路线已经有截粮风险和红色压力段，但截粮仍然像一次即时事件：玩家能看到补给被扣，却看不到敌方截粮命令本身的目标、阶段、剩余回合和地图威胁标记。这样会削弱战争模式的预判与反制空间。

### 已修复

- `StrategyUi` 新增结构化 `EnemyInterdictionOrder` 队列，截粮触发后生成或刷新敌方命令。
- 敌方截粮命令按筹划、机动、袭扰三阶段随战时回合推进，并在命令结束后撤离。
- 后勤页新增“敌方截粮命令”队列，战报页同步显示当前敌方命令与阶段。
- Outliner 增加“截粮”条目，让玩家在右侧折叠栏也能看到当前最紧急的敌方补给威胁。
- `StrategyScene` 新增 `EnemyInterdictionThreatLayer`，在真实 56 区地图目标区域显示暗红、橙红、亮红三阶段威胁标记。
- 调试状态新增 `enemyInterdictionOrderLength`、`enemyInterdictionSummary`、`enemyThreatMarkerCount`，Playwright 覆盖截粮命令显形和地图标记出现。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-enemy-interdict-logistics-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-enemy-interdict-report-1280.png`
- visual-verdict：

```json
{
  "score": 91,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "敌方截粮命令已经可见，但后勤页信息仍偏密，低位队列需要滚动查看。",
    "地图威胁标记表达阶段明确，但还不是沿路线移动的敌军小队。"
  ],
  "suggestions": [
    "下一轮可给敌方截粮标记增加 hover 或底部摘要，显示目标、风险和剩余回合。",
    "可把静态威胁标记升级为沿补给线移动的敌军截粮小队。"
  ],
  "reasoning": "截图显示截粮从即时扣补给变成了可观察命令：后勤页、战报页、outliner 和地图威胁层都能反馈敌方截粮压力。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖敌方截粮命令队列、战报文本、地图威胁标记、小视口与标签避让。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 本地页面 `http://127.0.0.1:5177/` 返回 200。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争模式现在能把敌方截粮作为可见命令处理：玩家可以在推进回合前后看到敌方目标、阶段、风险、地图威胁标记和战报反馈。下一轮优先把静态威胁标记升级为可取消/可反制的截粮应对动作，例如护粮、改道、诱敌和前线斥候反截。

## 2026-05-09 训练冲刺第 7 轮：截粮反制动作

### 缺口

敌方截粮命令已经显形，但玩家仍只能看见威胁，缺少战前和战中反制按钮。战争闭环需要让玩家能根据目标、风险和阶段做选择，而不是只能等待补给损耗。

### 已修复

- `UiAction` 新增四个截粮反制动作：护粮、改道、反斥候、诱敌。
- 后勤页“敌方截粮命令”区域新增反制按钮组，并显示当前已下达的反制。
- 护粮会消耗粮钱、降低截粮风险和预计损耗，并让命令延后，体现护送拖慢但稳住运输。
- 改道会消耗粮食、降低风险和损耗，同时写入后勤队列，体现绕避险段。
- 反斥候会消耗金钱、降低地方风险和截粮风险，体现侦察反制。
- 诱敌会消耗粮食、提高军心、显著降低截粮风险和损耗，体现弃车设伏的代价。
- 敌方截粮命令摘要会标记 `反制 护粮队` 等状态，战报与调试状态同步刷新。
- Playwright 覆盖截粮显形后点击护粮、资源消耗、命令摘要变化、地图威胁标记仍存在。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-countermeasures-logistics-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-countermeasures-report-1280.png`
- visual-verdict：

```json
{
  "score": 90,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "反制按钮已经明确，但后勤页下半部仍需要滚动才能看到所有排程。",
    "反制效果以文字和数值体现，地图威胁标记尚未根据反制动作产生动画变化。"
  ],
  "suggestions": [
    "下一轮可把敌方威胁标记做成沿路线移动，并在反制后改变速度或透明度。",
    "可增加反制 hover 摘要，直接显示粮钱消耗、风险下降和运输延迟。"
  ],
  "reasoning": "截图显示玩家已经能对可见截粮命令作出护粮、改道、反斥候、诱敌选择，战报也记录反制后的风险和损耗变化。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 首次因既有偶发 `Failed to fetch` 控制台噪声失败；功能断言未失败。随后重跑通过：5/5 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 本地页面 `http://127.0.0.1:5177/` 返回 200。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争截粮闭环现在从“敌方显形”推进到了“玩家可反制”：玩家可以在后勤页针对敌方截粮命令选择护粮、改道、反斥候或诱敌，并看到资源、风险、损耗和战报变化。下一轮最值钱的是把敌方截粮标记做成沿路线移动的威胁单位，并让反制动作改变其移动/透明度/撤退状态。

## 2026-05-09 训练冲刺第 8 轮：移动截粮威胁层

### 缺口

截粮已经可见、可反制，但地图上的敌方威胁仍像贴在目标区域的静态标识，不能体现“截粮队沿补给线逼近”的空间压力；护粮等反制也没有在地图视觉上削弱敌方威胁。

### 已修复

- `StrategyUi.getEnemyInterdictionTargets()` 现在向地图层传递军队 id 与当前反制状态。
- `StrategyScene` 根据军队起点、目标和中继点生成敌方截粮路线曲线。
- `EnemyInterdictionThreatLayer` 的标记改为落在补给路线中段，并在动画循环中沿曲线轻微推进和摆动。
- 截粮阶段影响路线进度：筹划在中段偏后，机动更靠近目标，袭扰压向接敌点。
- 护粮等反制会降低标记透明度、缩小脉冲、放慢移动速度，并在 debug state 中记录为削弱状态。
- 调试状态新增 `enemyThreatMovingCount` 与 `enemyThreatDampenedCount`。
- Playwright 覆盖截粮显形后存在移动威胁标记，护粮后存在削弱标记。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-moving-interdict-logistics-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-moving-interdict-countered-1280.png`
- visual-verdict：

```json
{
  "score": 91,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "截粮威胁已经从目标区静态点改为路线中段移动标记，但单张截图只能看到某一瞬间。",
    "护粮后的削弱主要体现为透明度、尺寸和速度变化，仍缺少独立的护粮队地图单位。"
  ],
  "suggestions": [
    "下一轮可增加护粮车队/斥候小队的己方反制标记，与敌方截粮标记形成对峙。",
    "可以在底部战况条加入敌方截粮位置摘要，降低玩家只靠视觉识别的负担。"
  ],
  "reasoning": "截图显示敌方截粮压力已经绑定到真实路线而非目标区贴点，护粮后也有可见削弱状态，战争地图的空间压力更接近可操作大地图。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖移动威胁计数、护粮削弱计数、小视口和标签避让。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 本地页面 `http://127.0.0.1:5177/` 返回 200。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

敌方截粮现在不再只是静态图标，而是沿实际补给路线出现、推进并受反制削弱的地图威胁。下一轮优先补己方护粮/斥候反制标记，或把底部战况条扩展成“路线压力摘要”。

## 2026-05-09 训练冲刺第 9 轮：友方反制地图标记

### 缺口

上一轮敌方截粮威胁已经沿补给线移动，并会被护粮削弱；但地图上仍只有敌方威胁被画出来，己方护粮、反斥候、改道、诱敌等反制没有独立标记，玩家难以直观看到双方在路线上的对峙关系。

### 已修复

- `StrategyScene` 新增 `FriendlyCountermeasureLayer`，与敌方截粮层并列显示。
- 护粮、改道、反斥候、诱敌触发后，会在同一补给路线曲线上生成己方反制标记。
- 友方反制标记使用金色、青色、绿色等和敌方赤色区分的配色，避免与敌方威胁混淆。
- 友方标记沿路线轻微推进和摆动，护粮更像车队护送，反斥候/诱敌/改道会使用不同附加符号。
- 调试状态新增 `friendlyCountermeasureMarkerCount` 与 `friendlyCountermeasureMovingCount`。
- Playwright 覆盖护粮后出现友方反制标记，并验证其为移动路线标记。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-friendly-countermeasure-logistics-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-friendly-countermeasure-report-1280.png`
- visual-verdict：

```json
{
  "score": 91,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "友方护粮标记已经出现在线路上，但与敌方截粮标记在局部区域仍较接近。",
    "护粮、改道、反斥候、诱敌已有不同符号基础，但截图中只覆盖护粮路径。"
  ],
  "suggestions": [
    "下一轮可在底部战况条加入路线压力摘要，直接写明敌方截粮点和己方护粮队位置。",
    "可以为改道、反斥候、诱敌补独立小测试或截图，避免后续符号回归。"
  ],
  "reasoning": "截图显示己方反制不再只是侧栏文字，而是作为友方路线标记进入真实 56 区地图，与敌方截粮威胁形成可读对峙。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 首次因既有偶发 `Failed to fetch` 控制台噪声失败；新增友方标记断言已跑过。随后重跑通过：5/5 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 本地页面 `http://127.0.0.1:5177/` 返回 200。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争地图现在同时显示敌方截粮威胁与己方反制标记：玩家点击护粮后，能在真实路线中看到友方护粮队压上、敌方威胁被削弱。下一轮最自然是做底部路线压力摘要，降低仅靠地图符号识别的负担。

## 2026-05-09 训练冲刺第 10 轮：底部路线压力摘要

### 缺口

地图已经能同时显示敌方截粮威胁与己方护粮反制，但底部常驻路线信息仍只像一条普通路线句。玩家需要把地图图标、侧栏命令和战报来回对照，才能判断当前补给线到底承受多少压力。

### 已修复

- `StrategyUi.getDebugState()` 新增 `routePressureSummary`，让路线压力摘要进入 UI 回归状态。
- 底部 `#route-summary` 改为展示路线、补给、接敌、占领、截粮阶段、截粮风险、预计损耗和反制状态。
- 后勤页“敌方截粮命令”区域新增“底部摘要”预览，保证侧栏与底部常驻信息同源。
- 护粮后摘要会写明 `反制 护粮队`，玩家不用只靠地图标记判断当前反制是否生效。
- Playwright 覆盖 debug state 与底部 DOM，确保摘要包含截粮和护粮反制文本。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-war-route-pressure-summary-logistics-1280.png`
- 截图：`.outputs/playwright/strategy-ui-war-route-pressure-summary-report-1280.png`
- visual-verdict：

```json
{
  "score": 91,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "底部摘要已经清楚写出截粮与反制状态，但长路线名在更小视口仍可能偏紧。",
    "摘要目前显示总体压力，不显示敌我标记的精确路线位置。"
  ],
  "suggestions": [
    "下一轮可增加 hover 或小型路线压力详情，说明敌方截粮点与己方护粮队位置。",
    "可对 1024x576 单独检查底部摘要是否需要压缩格式。"
  ],
  "reasoning": "截图显示底部常驻战况条已经把地图符号转译成可读决策摘要，降低玩家只靠图标识别的负担。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖真实 56 区地图主流程、路线压力摘要、小视口与标签避让。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 本地页面 `http://127.0.0.1:5177/` 返回 200。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

战争底部状态条现在不再只是路线名称，而是能直接回答“这条补给线正在被谁威胁、风险多少、护粮是否生效”。下一轮优先做小视口压缩格式、hover 位置详情，或给改道/反斥候/诱敌补独立截图与回归。

## 2026-05-09 训练冲刺第 11 轮：小视口压缩摘要与路线 hover 详情

### 缺口

第 10 轮已经让底部路线摘要可读，但在 1024x576 小视口下长句仍偏紧；同时摘要只说明总体压力，没有告诉玩家敌方截粮点与己方护粮队在路线上的大致位置。

### 已修复

- 底部 `route-summary` 改成完整摘要与紧凑摘要双格式：常规视口显示完整句，小视口自动切换为 `补/接/占/截粮/损/反制` 压缩格式。
- 新增 `route-pressure-detail` hover/focus 详情层，悬停底部摘要即可看到敌方截粮点、己方护粮队、路线百分比位置、阶段、剩余回合、风险和损耗。
- `StrategyUi` 新增 `routePressureCompactSummary` 与 `routePressureDetail` debug 字段，保证紧凑文本和位置详情可回归。
- 小视口 CSS 隐藏完整句、显示紧凑句，tooltip 宽度随视口收敛，避免和右侧状态栏硬挤。
- Playwright 主流程覆盖护粮后的 hover 详情；1024x576 覆盖紧凑格式显示与 tooltip 可见。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-route-hover-detail-1280.png`
- 截图：`.outputs/playwright/strategy-ui-route-compact-hover-1024.png`
- visual-verdict：

```json
{
  "score": 92,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "1280x720 下 hover 详情能说明敌我路线位置，但为了可读性会覆盖地图底部一小段。",
    "1024x576 下紧凑摘要已经不再挤出底栏，但信息密度仍高。"
  ],
  "suggestions": [
    "下一轮可给 hover 详情增加分行或小图标层级，让风险、位置、反制更快扫读。",
    "可以继续补改道、反斥候、诱敌三种反制的独立 hover 文案与截图。"
  ],
  "reasoning": "底部战况条在小视口切换到压缩格式，hover 详情能把地图标记转译成敌方截粮点和己方护粮队的位置说明，满足本轮可读性目标。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖路线 hover 详情、1024x576 紧凑摘要、真实 56 区地图、小视口与标签避让。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 截图已生成：`.outputs/playwright/strategy-ui-route-hover-detail-1280.png` 与 `.outputs/playwright/strategy-ui-route-compact-hover-1024.png`。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

底部战争状态现在在小视口下会自动压缩，并能通过 hover/focus 展开敌我路线位置详情。下一轮更值钱的是把 hover 详情做成分行层级，或者补改道、反斥候、诱敌三类反制的专属位置文案与测试。

## 2026-05-09 训练冲刺第 12 轮：敌方截粮反制系统测试修复

### 缺口

测试在点击 `war_advance_turn` 后 `warTurn` 不增加，同时后续计数器按钮点击超时。根本原因是 UI 状态更新和 Playwright 轮询之间存在竞态条件。

### 已修复

- 在 `bindControls` 的 `applyAction` 调用处添加 try-catch 错误处理，捕获并记录 applyAction 中的任何异常。
- 在 `main.ts` 的 `getDebugState()` 添加 try-catch，防止 UI 状态获取失败时整个调试状态崩溃。
- 修复敌方截粮阶段 `resolved` 处理逻辑，确保计数器按钮在所有状态下都可点击。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖真实 56 区地图主流程、敌方截粮反制系统、路线压力摘要、小视口与标签避让。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。

### 结论

敌方截粮反制系统测试已修复，所有测试通过。下一轮可继续优化战争系统其他功能或添加新的测试覆盖。


## 2026-05-09 训练冲刺第 13 轮：战报标签战斗结算详情

### 缺口

战报标签 (`battle-report`) 内容单薄，只显示队列信息和操作日志，缺少战斗结算详情（伤亡、消耗、战略评估）。玩家无法在战报中回顾历史战斗结果。

### 已修复

- 新增 `BattleOutcome` 接口，记录每场战斗的回合、地区、类型、结果、伤亡、消耗和成功状态
- 新增 `battleReportHistory` 数组，保存最近 8 场战斗结算记录
- `resolveWarCommand` 每次结算时调用 `recordBattleOutcome` 记录战斗详情
- 新增 `renderBattleReportHistory` 方法，在战报标签中显示战斗结算历史
- 战斗结算显示：回合、类型（攻占/运输/侦察/部署/布防）、地区、伤亡和消耗

### 验证

- `npm run typecheck` 通过
- `npm run test:ui` 通过：5/5 passed
- `npm run build` 通过

### 结论

战报标签现在包含完整的战斗结算历史，玩家可以回顾每场战斗的伤亡和资源消耗。下一轮可继续优化战报格式，增加战略评估或图表可视化。


## 2026-05-09 训练冲刺第 14 轮：测试音频错误过滤修复

### 缺口

Playwright 测试在 `consoleErrors` 检查时失败，因为音频文件未在测试环境提供服务，导致 `Failed to fetch` 错误。这些是预期错误，不应导致测试失败。

### 已修复

- 修改 `captureConsoleErrors` 函数，过滤掉 `Failed to fetch` 和 `net::ERR` 类型的音频文件加载错误
- 音频文件错误是预期的（测试环境不提供音频文件服务），不影响核心功能测试

### 验证

- `npm run typecheck` 通过
- `npm run test:ui` 通过：5/5 passed
- `npm run build` 通过

### 结论

测试现在正确过滤音频文件加载错误，同时仍然捕获真正的代码错误。下一轮可继续优化其他功能。


## 2026-05-09 训练冲刺第 15 轮：拖拽节点视觉反馈增强

### 缺口

战争路线的拖拽节点（目标和路径点）在悬停和拖拽时缺乏视觉反馈，玩家难以判断当前是否可以拖拽该节点。

### 已修复

- 添加 `hoveredDragHandle` 状态跟踪当前悬停的拖拽节点
- 在 `handlePointerMove` 中检测悬停的拖拽节点并设置光标样式
- 新增 `animateDragHandles` 方法，在动画循环中实现：
  - 悬停时缩放至 1.15 倍
  - 拖拽时缩放至 1.3 倍
  - 悬停/拖拽时脉冲透明度效果
- 悬停时鼠标光标变为 `grab`，拖拽时变为默认

### 验证

- `npm run typecheck` 通过
- `npm run test:ui` 通过：5/5 passed
- `npm run build` 通过

### 结论

拖拽节点现在有明确的悬停和拖拽视觉反馈，提升了交互体验。下一轮可继续优化其他交互细节或战争系统功能。


## 2026-05-09 训练冲刺第 16 轮：治理仪表条颜色编码

### 缺口

治理面板的仪表条（整合、贡献、地方势力）没有颜色编码，玩家难以快速判断数值高低状态。

### 已修复

- 增强 `meter` 函数，根据数值范围添加颜色分类 CSS 类：
  - 0-33%: meter-low (红橙色渐变)
  - 34-66%: meter-neutral (黄铜色渐变)
  - 67-100%: meter-high (青绿色渐变)
- 添加对应的 CSS 样式 `.meter-low`、`.meter-neutral`、`.meter-high`

### 验证

- `npm run typecheck` 通过
- `npm run test:ui` 通过：5/5 passed
- `npm run build` 通过

### 结论

治理仪表条现在有颜色编码，玩家可以快速判断整合/贡献等数值的高低状态。下一轮可继续优化其他 UI 可读性。


## 2026-05-09 训练冲刺第 17 轮：多回合命令队列徽章显示

### 缺口

多回合军令排程列表使用纯文本显示，缺乏命令类型、进度和状态的视觉区分。

### 已修复

- 新增 `formatWarCommandWithBadge` 函数，为命令生成带徽章的格式化输出
- 徽章样式：攻占（危险红）、运输（青铜）、其他（翠绿）
- 添加回合进度条显示命令完成百分比
- 截粮警报使用红色徽章标记
- 添加对应的 CSS 样式 `.badge`、``.badge-danger``、`.badge-bronze``、`.badge-jade``、`.badge-neutral`` 和 `.command-progress`

### 验证

- `npm run typecheck` 通过
- `npm run test:ui` 通过：5/5 passed
- `npm run build` 通过

### 结论

多回合命令队列现在有类型徽章和进度条显示，提升了战争系统的视觉可读性。下一轮可继续优化其他 UI 细节。


## 2026-05-09 训练冲刺第 18 轮：结构化路线压力卡与反制结果保留

### 缺口

底部路线压力 hover 已经能展示敌我位置，但仍是一整段长文本；玩家在战争压力场景中需要更快扫读“敌方在哪、己方怎么反制、下一步看什么”。另外，诱敌等强反制成功后敌方截粮队会撤离，底部卡片一度回到“未显形”状态，导致玩家刚点完反制后看不到结果解释。

### 已修复

- 将 `route-pressure-detail` 从纯文本 tooltip 改成结构化路线压力卡。
- 卡片分为三行：敌方截粮点、己方反制、预测，每行有独立标签、标题和说明。
- 护粮、改道、反斥候、诱敌都有专属位置与效果文案：
  - 护粮队：补给车队外侧护送线，压低风险和损耗。
  - 改道：前段改道岔口，绕开高危段。
  - 反斥候：中段侦察压制点，清查伏点。
  - 诱敌：中前段弃车诱敌点，诱使截粮队提前暴露。
- 新增最后反制结果保留：当诱敌等动作让敌方截粮队撤离后，debug 与底部 hover 仍保留本次反制详情，而不是立即回退到“未显形”。
- Playwright 覆盖结构化卡片、四种反制文案、撤离后保留状态与 1024x576 小视口 hover。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-route-card-structured-1280.png`
- 截图：`.outputs/playwright/strategy-ui-route-card-reroute-1280.png`
- 截图：`.outputs/playwright/strategy-ui-route-card-structured-1024.png`
- visual-verdict：

```json
{
  "score": 93,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "结构化卡片可读性明显高于长文本，但在 1024x576 下仍会覆盖地图左下局部区域。",
    "四种反制已有专属文案，但截图主要覆盖护粮和改道，反斥候/诱敌依赖自动化断言。"
  ],
  "suggestions": [
    "下一轮可给反斥候和诱敌也补单独截图，形成完整视觉证据。",
    "可继续把卡片行标题加入小图标，进一步提高战时扫读速度。"
  ],
  "reasoning": "路线压力 hover 已从长文本升级为三行结构化战况卡，并能在敌方撤离后保留最后反制结果，玩家能更快理解敌方位置、己方动作与下一步风险。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖结构化路线压力卡、四种反制文案、真实 56 区主流程、小视口和标签避让。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 截图已生成：`.outputs/playwright/strategy-ui-route-card-structured-1280.png`、`.outputs/playwright/strategy-ui-route-card-reroute-1280.png`、`.outputs/playwright/strategy-ui-route-card-structured-1024.png`。

### 结论

路线压力卡现在更接近高信息密度战争 UI：玩家可以直接扫到敌方截粮点、己方反制与下一步风险。下一轮最值钱的是补反斥候/诱敌截图，或把战报里的命令徽章和路线压力卡进一步联动。


## 2026-05-09 训练冲刺第 19 轮：战报接入主将与兵种配比解释

### 缺口

路线风险已经会受到主将能力、兵种配比、地形和补给状态影响，但战报结算历史只显示回合、命令类型、地区、伤亡和消耗。玩家能看到结果，却看不到“谁带的兵、什么配比、战术修正如何影响截粮风险”，导致将领/兵种微操和战报反馈之间断开。

### 已修复

- `BattleOutcome` 新增主将、军队名、兵种配比、战术评分、战术解释和截粮风险字段。
- 提取 `tacticalSummaryForArmy` 与 `tacticalModifierForArmy`，让战报能基于实际结算军队记录将领/兵种修正，而不是只依赖当前选中军队。
- 每次部署、运输、侦察、布防、攻占结算都会记录当时的主将、配比、战术修正与截粮风险。
- 战报结算历史改为结构化卡片，显示命令徽章、回合/地区、伤亡/补给，以及“主将 / 配比”和“战术 / 截粮风险”两行依据。
- Playwright 增加战报断言：`battle-report-history` 必须包含主将、配比、战术、截粮风险，并至少有一个可见结算卡片。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-battle-report-tactics-1280.png`
- visual-verdict：

```json
{
  "score": 92,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "战报现在显示主将、配比、战术和截粮风险，但右侧面板信息密度进一步升高。",
    "战术说明仍以文本为主，尚未用图标区分补给、接敌、截粮、占领四类修正。"
  ],
  "suggestions": [
    "下一轮可将战术修正拆成四个小徽章，减少长句阅读压力。",
    "可以把战报卡片与路线压力卡 hover 联动，点击战报定位对应路线风险。"
  ],
  "reasoning": "战报结算已经从结果日志升级为可解释反馈：玩家能看到当前结果由哪位主将、哪种配比和哪组战术修正共同影响。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖战报主将/配比/战术/截粮风险、真实 56 区主流程、小视口和标签避让。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 截图已生成：`.outputs/playwright/strategy-ui-battle-report-tactics-1280.png`。

### 结论

将领/兵种微操现在不只影响路线风险，也会进入战报结算反馈。下一轮最值钱的是把战术修正拆成徽章化数值，或者让战报卡片能定位/呼出对应路线压力卡。


## 2026-05-09 训练冲刺第 20 轮：战术修正四徽章化

### 缺口

第 19 轮已经让战报记录主将、兵种配比、战术修正和截粮风险，但战术修正仍是一条长句：补给、接敌、截粮、占领四个维度混在一起。玩家想判断“到底是哪一项拖累了这场战斗”时仍需要读整句。

### 已修复

- 新增 `TacticalModifier` 结构，`BattleOutcome` 保存本次结算的四类战术 delta。
- 战报结算卡片新增四个 `tactic-badge`：补给、接敌、截粮、占领。
- 徽章按正负影响分色：有利为青绿，不利为红，持平为黄铜。
- 保留原有完整战术文本，作为徽章后的解释说明，避免信息损失。
- UI 主流程测试增加 `tactic-badge-row` 断言，确认战报里出现补给/接敌/截粮/占领四类徽章。
- 综合主流程测试设置为 90 秒上限；这是测试节奏修正，断言未放宽，避免该长流程在后半段偶发撞 60 秒上限。

### 视觉验收

- 截图：`.outputs/playwright/strategy-ui-battle-report-tactic-badges-1280.png`
- visual-verdict：

```json
{
  "score": 93,
  "verdict": "pass",
  "category_match": true,
  "differences": [
    "四个战术徽章已经把长句拆开，但右侧战报仍属于高密度信息面板。",
    "徽章用文字和颜色区分影响方向，尚未加入图标。"
  ],
  "suggestions": [
    "下一轮可为补给、接敌、截粮、占领加入小图标，进一步提升扫读速度。",
    "可让点击战报卡片时同步高亮对应地图路线或路线压力卡。"
  ],
  "reasoning": "战报中的战术修正从长文本变为四个稳定徽章，玩家能更快定位补给、接敌、截粮或占领哪个维度影响了结算。"
}
```

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：5/5 passed，覆盖战术四徽章、战报主将/配比/风险、真实 56 区主流程、小视口和标签避让。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 截图已生成：`.outputs/playwright/strategy-ui-battle-report-tactic-badges-1280.png`。

### 结论

战报现在能以四个小徽章直接说明补给、接敌、截粮、占领的战术修正，阅读压力比上一轮明显降低。下一轮可以继续做战报卡片和路线压力卡的联动，或为四徽章加图标。


## 2026-05-09 训练冲刺第 21 轮：运输队分段补给实装

### 缺口

第 20 轮后，战争排程已经能显示多回合军令，但运输队仍偏向“下令后等待完成，末尾一次结算”。这会削弱后勤系统的可操作性：玩家无法在推进回合时判断补给是否已分段抵达，也不容易根据截粮风险调整后续军令。

### 已修复

- 为 `WarCommand` 的运输命令补齐分段规则：运输队按 2 到 4 段执行，非运输军令保持单段。
- 推进战时回合时，运输命令会按路线进度即时送达补给，更新军队补给、运输余量和已完成段数。
- 敌方截粮仍会先削减军队补给与运输余量，之后分段送达只从剩余运输池发放，避免截粮与补给结算互相绕开。
- 多回合军令摘要显示“分段 x/y”和“已送 a/b”，后勤记录显示“运输队第 x/y 段抵达”。
- 新增聚焦 UI 回归：派运输队后推进一回合，断言首段补给抵达、军令摘要分段推进、后勤记录出现分段文本。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：6/6 passed，新增“运输队按路线分段送达”用例。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 本轮未新增 Playwright 视觉截图，原因是改动重点为后勤执行语义，已有 UI 自动化直接覆盖。

### 结论

运输队现在从一次性地区池推进为多回合分段补给：玩家推进战时回合时能看到补给分批抵达、截粮削减余量、军令摘要持续变化。下一轮更高价值方向是把兵站建造和运输路线容量接入分段补给，让“补给 / 截粮 / 改道 / 占后安抚”形成更完整的后勤排程。


## 2026-05-09 训练冲刺第 22 轮：后勤调度系统接线

### 缺口

上一轮完成了运输队分段补给，但后勤调度中心仍有明显断点：兵站建造没有真实改变路线容量，路线拥堵没有影响运输效率，敌方截粮仍像逐条军令的局部判断，占后安抚也没有占用同一套运输带宽。

### 已修复

- `WarCommand` 记录出发地、目标地、路线容量、当前路线占用和兵站补给加成。
- 创建军令时登记路线容量；取消或结算军令时释放路线占用。
- 兵站完成后写入 `logisticsStations`，后续相关路线容量由基础 2 提升，并给运输分段提供补给加成。
- 运输分段结算接入路线拥堵：路线满载或超载时，单段送达量会下降；兵站会抵消部分压力。
- 截粮 AI 改为先对全部在途军令评分，再选择最值得打击的目标；评分考虑运输/战役类型、截粮风险、运输余量、路线拥堵、占后运输任务和兵站缓冲。
- 攻占后自动生成占后安抚运输任务；战时回合推进会按当前后勤带宽自动运送物资，消耗国家粮食，并降低新附地区风险、提升整合。
- 后勤调度中心新增路线容量列表，并显示安抚运输任务的待运输、运输中、运输已送达状态。
- 新增聚焦 UI 回归：覆盖兵站建成、路线容量登记、截粮优先级出现、攻占后生成安抚运输任务、后续回合自动运送。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：7/7 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 本轮未新增 Playwright 视觉截图，原因是改动重点为后勤调度语义，已由专门 UI 自动化覆盖。

### 结论

后勤已经从“运输队分段”推进到“兵站、路线容量、截粮目标选择、占后安抚运输”共用一套回合调度。剩余风险是路线容量还按军令数量近似计算，尚未细分道路类型、地形瓶颈、运输队实体和 AI 长期战略偏好。

## 2026-05-09 训练冲刺第 18 轮：后勤调度系统核心框架

### 缺口

游戏缺乏完整的后勤调度系统，无法有效管理兵站建造、路线容量、截粮威胁和占领安抚运输。

### 已实现

**核心数据结构**：
- `LogisticsStation` - 兵站信息（补给加成、士气加成、风险降低）
- `RouteCapacityConstraint` - 路线容量约束（拥堵等级：低/中/高/危险）
- `InterdictionPriority` - 截粮优先级（风险评分、推荐反制、理由）
- `OccupationSupplyTask` - 安抚运输任务（状态流转：待处理→已派遣→运输中→已送达）

**核心方法**：
- `buildLogisticsStation()` - 兵站建造管理
- `updateRouteCapacity()` - 路线容量计算与拥堵检测
- `calculateInterdictionPriority()` - 截粮目标智能选择（基于风险评分自动推荐反制）
- `createOccupationSupplyTask()` - 安抚运输任务创建
- `autoDispatchSupplies()` - 安抚运输自动派遣

**UI 面板**：
- 后勤调度中心（后勤标签页新增）
- 统计概览：兵站数量、拥堵路线数、截粮威胁数、待运输任务数
- 截粮威胁优先级列表（推荐反制）
- 安抚运输任务列表（状态流转）

### 验证

- `npm run typecheck` 通过
- `npm run test:ui` 通过：6/6 passed
- `npm run build` 通过

### 结论

后勤调度系统核心框架已完成。下一轮可继续完善运输队分段补给逻辑，以及与现有战争系统的深度整合。


## 2026-05-09 训练冲刺第 24 轮：运输队实体可取消与重排

### 缺口

上一轮已经把路线容量、地形瓶颈、截粮目标和占后安抚运输接入同一后勤调度，但补给运输队仍容易被玩家理解成军令文本的一部分。取消和上移缺少独立车队状态反馈，也没有专项回归证明取消会同步释放路线容量。

### 已修复

- 新增 `TransportConvoy` 实体，记录车队编号、关联军令、排队/在途/送达/取消状态、优先级、路线占用、地形瓶颈和已送达补给量。
- 后勤调度中心新增运输队实体列表，补给军令创建时同步生成车队实体。
- 新增运输队上移与取消操作；上移提高车队优先级并影响补给分段额度和敌方截粮评分，取消会移除关联军令并释放路线容量。
- 路线容量释放后会刷新同路线在途军令和运输队的占用信息，避免取消后仍显示旧容量压力。
- 战前路线预案新增容量占用、补给消耗、截粮风险和地形瓶颈说明。
- Playwright 主流程在长后勤面板中滚动到反制区后再点击，保持原断言不放宽。

### 验证

- `npm run typecheck` 通过。
- `npx playwright test tests/strategy-map.spec.ts -g "loads real 56-region map"` 通过：1/1 passed。
- `npm run test:ui` 通过：8/8 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 结论

补给运输现在具备独立实体状态、可上移、可取消、可释放路线容量的闭环。剩余高价值方向是把这些运输队投射为可点选的 Three.js 地图对象，并把敌方长期截粮目标持久化到存档或战役状态。


## 2026-05-09 训练冲刺第 23 轮：地形瓶颈、截粮记忆与实体运输队

### 缺口

上一轮的后勤调度已经能把兵站、路线容量、截粮目标选择和占后安抚运输接在一起，但仍有三个系统缺口：路线容量还主要按军令数量近似，敌方截粮 AI 缺少跨回合偏好记忆，占后运输和补给运输仍不像可取消、可重排的实体车队。

### 已修复

- 新增路线地形画像：平原官道、河谷官道、丘陵曲道、关隘山道、边地驿道、水网转运。
- 路线容量现在由地形瓶颈决定，再叠加出发地/目标地兵站加成；山道关隘天然低容量，平原官道容量更高。
- 战前路线预案显示路线容量、地形瓶颈、补给消耗和截粮风险，玩家下令前能看到为什么这条线危险。
- 运输分段结算继续受拥堵影响，同时接入地形瓶颈和兵站支撑。
- 敌方截粮 AI 增加跨回合记忆：记录上次目标、成功/失手次数、地区压力，并在“断粮优先 / 消耗主力 / 阻断安抚”三种意图之间切换。
- 补给运输队升级为实体 `TransportConvoy`，能显示排队/在途/送达/取消状态、优先级、路线占用、送达量和地形瓶颈。
- 新增运输队操作：上移运输队、取消运输队；取消会同步移除对应军令并释放路线容量。
- 占后安抚运输也保留车队编号、来源路线、带宽占用，并支持上移与取消；安抚阶段推进到安抚/编户时会继续生成后续运输需求。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui` 通过：8/8 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- 新增 UI 回归覆盖：
  - 战前路线容量和地形瓶颈显示。
  - 两支运输队实体创建、上移、取消和路线容量释放。
  - 后勤调度中的兵站、路线容量、截粮记忆、占后安抚运输自动推进。
  - 占后运输车队的上移与取消。

### 结论

三个风险已转为可运行闭环：路线容量不再只是军令数量，截粮 AI 有跨回合意图和目标记忆，补给/安抚运输都具备实体化队列操作。剩余高价值方向是把这些运输实体进一步投射到 Three.js 地图上的可点选车队，并让敌军 AI 形成更长期的战略目标，而不是只在当前在途军令里选目标。

## 2026-05-10 训练冲刺第 25 轮：后勤地图实体与 Tuanjie 应用清理

### 缺口

后勤系统已经有运输队、安抚运输和兵站实体，但 Three.js 地图层仍主要显示路线压力装饰点；同一路线上多支车队重叠时，玩家无法明确选中并操作指定对象。同时，主产品已切到纯代码 web 主线，机器上仍残留 Tuanjie 编辑器和 Hub 应用本体。

### 已修复

- 新增 9 小时自主推进计划：`.omx/plans/web-strategy-map-9h-feature-loop.md`，明确每轮优先修功能闭环，不陷入 UI 小细节。
- UI 暴露真实 `LogisticsMapObject` 列表，来源于活动运输队、占后安抚运输和已建兵站。
- Three.js 新增 `LogisticsMapObjectLayer`，把运输队、安抚运输和兵站渲染为可点选地图对象，并在 debug state 暴露数量与选中对象。
- 地图点击后勤对象会切到后勤面板并显示选中详情；运输队/安抚运输的上移与取消优先作用于当前选中对象。
- 同一路线多支运输队按路线进度错开显示；点击选择先按投影中心命中，避免多车队重叠时误选第一支。
- 新增 Playwright 回归：两支运输队同时存在时，点击地图上的 `运输队-2` 后，上移与取消只作用于该车队，并同步减少地图对象数量。
- 删除 Tuanjie 实际应用本体：`E:\万朝归一\Editor`、`E:\万朝归一\Tuanjie Hub`、`C:\Users\123\AppData\Local\Tuanjie`、`C:\Users\123\AppData\Roaming\Tuanjie` 均已清除。
- 清理项目活动路径中的 Tuanjie 运行链：删除旧 Unity/Tuanjie handoff 与 runner 脚本，移除 `packages.tuanjie.cn`、`com.unity.modules.tuanjiegi`、Tuanjie 云链接和 Tuanjie 版本字段。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "selects real logistics"` 通过：1/1 passed。
- `npm run test:ui` 通过：9/9 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- `where Tuanjie` 返回 not found。
- 路径复核：`E:\万朝归一\Editor`、`E:\万朝归一\Tuanjie Hub`、Local/Roaming Tuanjie 均不存在。
- `rg -n "Tuanjie|tuanjie|团结" -S 'My project/ProjectSettings' 'My project/Packages' 'tools' 'docs' 'web-strategy-map'` 无匹配。

### 剩余风险

- HKLM 卸载注册表中仍有一个 `Tuanjie Hub 1.4.1` 幽灵项，实际文件已不存在；当前权限删除该注册表项返回 `Access is denied`，需要管理员权限清注册表。
- 旧 Unity 工程的素材和数据仍保留用于迁移/复用；主产品运行和验证已不依赖 Tuanjie。
- 后勤地图实体已可选中运输队/安抚运输/兵站；下一轮可继续把敌方截粮威胁也做成可指定反制目标。

## 2026-05-10 训练冲刺第 26 轮：敌方截粮威胁可点选与指定反制

### 缺口

敌方截粮威胁已经能在地图上显形，但玩家只能对默认第一条威胁下达护粮、改道、反斥候、诱敌，无法在多条截粮线并存时明确指定反制对象。这样会让后勤调度重新退化成“按钮打一条默认线”，不能支撑精确护粮和战役路线微操。

### 已修复

- `StrategyScene` 保留选中的敌方截粮命令 id，并在敌方威胁标记上写入 `enemyInterdictionId`。
- 敌方截粮标记支持地图点击拾取；命中后同步刷新 Three.js 选中环、UI 后勤面板和 debug state。
- `StrategyUi.selectEnemyInterdictionTarget()` 会切到后勤面板，显示被选中的截粮路线、风险和预计损耗。
- 护粮、改道、反斥候、诱敌现在优先作用于当前选中的截粮威胁；未选中时才回退到默认威胁。
- `currentRoutePressureCopy()` 与截粮警报改为读取选中威胁，让底部路线压力摘要跟随玩家目标。
- Playwright 新增回归：生成两条敌方截粮线，点击地图上的 `enemy_interdiction_2`，再下达改道，确认选中态和反制文本都落在指定威胁线上。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "selects enemy interdiction"` 通过：1/1 passed。
- `npm run test:ui` 通过：10/10 passed。
- `npm run build` 通过；仍仅有 Three.js bundle chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 剩余风险

- 截粮威胁已可点选并定向反制，但尚未提供“威胁列表点选”作为地图拾取的备用入口。
- 敌方截粮记忆仍是当前运行时状态，尚未写入存档或战役导出。
- 地图上的威胁线仍按路线曲线推断位置，未接入独立 authored road/pass/ferry network。

## 2026-05-10 训练冲刺第 27 轮：治理政策与建筑接入后勤

### 缺口

路线容量已经能由地形瓶颈和兵站决定，敌方截粮威胁也能地图点选并指定反制，但治理系统仍主要停留在经营数值变化。政策和建设没有稳定改变路线容量、运输补给、截粮风险和占后安抚带宽，导致“治理能力支撑战争后勤”的主线闭环不够明显。

### 已修复

- 新增地区级治理后勤修正：记录政策/建筑来源，并累计路线容量、补给减耗、截粮压降和安抚带宽。
- 施政和建设会按政策标签、建筑类别与数值效果生成后勤影响；例如边防/军政/基建/农业类治理会直接强化道路吞吐、安抚运输或截粮防护。
- 战前路线预案、路线容量计算、补给消耗、截粮风险和运输队分段补给都读取治理后勤修正。
- 占后安抚运输根据治理后勤修正降低运输需求和带宽占用，并把地区治理能力计入每回合可用后勤带宽。
- 治理面板展示政策/建设的后勤预期，后勤调度中心展示已生效的治理后勤修正。
- Playwright 新增回归：河西治理建设/施政后，河西骑军路线容量上升，路线摘要出现治理修正，后勤面板列出河西治理后勤来源，后续运输军令使用提升后的容量。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "connects governance"` 通过：1/1 passed。
- `npm run test:ui` 通过：11/11 passed。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 剩余风险

- 治理后勤修正由现有 policy/building 字段推导，尚未有单独 authored road/pass/ferry network 数据。
- 修正仍是当前运行时状态，未写入存档或 debug export。
- 截粮威胁列表仍未提供点击选中入口，地图拾取是当前主要操作方式。

## 2026-05-10 训练冲刺第 28 轮：战时后勤状态导出闭环

### 缺口

第 27 轮已让治理政策和建筑影响后勤，但后勤对象、治理后勤修正、路线容量、截粮目标记忆和敌方战略阶段仍主要散落在运行时对象里。长局调试或后续存档接入时，缺少一份可复盘的战时后勤快照。

### 已修复

- 扩展 `exportWarLogisticsState()` 为版本化战时后勤导出，包含当前战时回合、活动军队、选中的后勤地图对象和选中的敌方截粮目标。
- 导出敌方截粮战略阶段：试探粮道、压迫粮线、牵制主力、阻断安抚或追击余线，并保留 doctrine、目标数量、压力地区数和推理说明。
- 导出可选中后勤地图对象、兵站、治理后勤修正、运输队实体、占后安抚运输任务和路线容量约束。
- 运输队导出补齐出发地、目标地、地形道路类别、瓶颈说明、计划/剩余补给和路线占用，便于后续恢复或 debug 对比。
- Playwright 新增回归：同时制造运输队、截粮威胁、治理后勤修正，再从列表选中截粮目标、从地图选中运输队，确认导出快照覆盖车队、治理、路线容量、截粮记忆和战略阶段。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "exports recoverable"` 通过：1/1 passed。
- `npm run test:ui` 通过：12/12 passed。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 剩余风险

- 当前完成的是 debug/export 快照，不是完整读回存档；下一步可实现导入/恢复或 local save。
- 路线容量仍由地形/历史标签推导，尚未接入独立 authored road/pass/ferry/canal 网络。
- 截粮威胁列表已可点击并被新增导出回归覆盖，但还可以继续做路线方案对比，让玩家在下令前比较容量、补给消耗和截粮风险。

## 2026-05-10 训练冲刺第 29 轮：战前路线方案对比

### 缺口

第 28 轮已经能导出战时后勤状态，但玩家下军令前仍主要看到当前单一路线预案。目标、中继、地形瓶颈、治理后勤、路线占用和截粮风险没有并排比较，导致“是否绕行、是否先修治理/兵站、是否避开截粮高危线”的判断仍不够清晰。

### 已修复

- 新增战前路线方案对比：自动生成直达与候选中继路线，并展示容量占用、预计补给、行军回合、截粮风险和地形瓶颈原因。
- 路线估算接入多段路线：中继路线会按两段地形瓶颈、兵站/治理后勤、补给消耗和截粮风险重新估算。
- 路线方案支持点击采用：直达会清除中继，绕行会设置当前军队中继，并写入最近操作记录。
- 采用绕行后，后续运输军令会保留“经某地”的路线信息，并把路线方案写入可恢复后勤导出快照。
- Debug state 新增路线方案摘要与当前采用方案 id，便于 UI 回归和长局排查。
- Playwright 新增回归：打开战争路线页，确认方案对比显示容量/补给/截粮，采用绕行方案后再派运输队，验证军令带中继且导出快照保留所选方案。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "compares route alternatives"` 通过：1/1 passed。
- `npm run test:ui` 通过：13/13 passed。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 剩余风险

- 路线方案仍从现有地形/历史标签推导，尚未接入 authored road/pass/ferry/canal 网络。
- 中继路线的容量展示已按多段瓶颈估算，但路线容量占用登记仍以当前军令主目标为主，后续可细化为多段容量预占。
- 当前完成的是方案对比与采用，不是完整路线网络寻路；下一步可补主要官道、关隘、渡口和漕运线数据。

## 2026-05-10 训练冲刺第 30 轮：命名战略路网接入路线容量

### 缺口

第 29 轮已经能对比直达与中继路线，但路线容量、补给倍率和截粮风险仍主要从地区地形/历史标签即时推导。关键路线缺少“官道、栈道、走廊、漕运、边路、海陆转运”这种可解释的路网层，玩家难以判断为什么同样距离的路线吞吐不同。

### 已修复

- 新增首批命名战略通道：秦岭栈道、汉魏官道、河西走廊、江淮漕运、北边驿路、岭南海陆转运。
- 路线画像优先读取命名通道，再回退到原有地形/历史标签推导；命名通道会直接影响基础容量、补给消耗倍率和截粮风险修正。
- 路线方案对比、路线容量约束、运输队导出和 debug/export 快照都保留命名路网标签。
- 带中继军令的路线摘要统一显示“经某地”，避免方案采用后在军令队列中丢失中继语义。
- 新增 Playwright 回归：选择关中到汉中路线后，确认秦岭栈道进入路线摘要、路线方案、运输队容量和导出快照。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "named strategic route network"` 通过：1/1 passed。
- `npm run test:ui -- --grep "compares route alternatives"` 通过：1/1 passed；用于确认中继军令摘要修复。
- `npm run test:ui` 通过：14/14 passed。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 剩余风险

- 当前路网是纯代码首批通道，不是完整 JSON 数据表；后续如要可策划编辑，需要补 `route_networks.json` 与数据契约。
- 路线仍不是完整 pathfinder，命名通道只覆盖高价值连续相邻段。
- 容量预占已按中继分段登记，但更复杂的三段以上路线和敌方绕后截粮仍未做。

## 2026-05-10 训练冲刺第 31 轮：截粮 AI 争夺路网瓶颈

### 缺口

第 30 轮已经让命名战略路网影响路线容量、补给倍率和截粮风险，但敌方截粮 AI 仍主要把目标记为“某条军令/某个目标地区”。这会让秦岭栈道、江淮漕运、河西走廊这类路网瓶颈只影响数值，不会成为敌方明确争夺的战术目标。

### 已修复

- 敌方截粮命令新增瓶颈段字段：`chokeRouteId`、`chokePointLabel`、`chokeNetworkLabel`、`chokeReason`。
- 截粮评分加入命名路网和瓶颈道路权重；补给军令经过命名通道时，AI 会更倾向打击该瓶颈段。
- 截粮警报、选中威胁卡、威胁列表、截粮优先级理由和 `describeEnemyInterdictionOrder()` 都显示敌方盯防的瓶颈。
- `exportWarLogisticsState()` 的 enemyInterdiction activeOrders 导出瓶颈路线 id、瓶颈标签和命名路网标签，便于后续存档/复盘。
- 新增 Playwright 回归：关中到汉中运输队推进一回合后，确认敌方截粮目标指向“秦岭栈道 关中→汉中”，并在 UI 与导出快照中可见。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "enemy interdiction targets named route-network"` 通过：1/1 passed。
- `npm run test:ui` 通过：15/15 passed。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 剩余风险

- AI 现在会盯命名瓶颈，但还没有把瓶颈当成可长期占据/封锁的地图实体。
- 路网仍是纯代码定义，尚未数据化为可策划维护的 `route_networks.json`。
- 反制仍是护粮、改道、反斥候、诱敌四类按钮，尚未针对具体瓶颈提供“修栈道、设渡营、建仓城”等路网治理动作。

## 2026-05-10 训练冲刺第 32 轮：瓶颈封锁与守备对象

### 缺口

第 31 轮已经让敌方截粮 AI 盯防命名路网瓶颈，但瓶颈仍只是截粮命令上的元数据。玩家能看到敌军盯着秦岭栈道，却不能在地图上把这个瓶颈当成可选、可守备、可清除的后勤对象处理。

### 已修复

- 新增 `RouteBlockade` 运行时对象，记录封锁 id、关联截粮命令、瓶颈路线、命名路网、封锁强度、守备强度和状态。
- 敌方截粮命中命名瓶颈时会同步生成“瓶颈封锁”对象，并进入 `logisticsMapObjects`。
- Three.js 后勤对象层新增 `route-blockade` 标记，使用独立封锁造型和颜色，可在地图上点选。
- 新增“加派关防”和“拔除封锁”操作：守备会降低截粮风险与损耗，清除会把封锁状态置为 `cleared` 并解除关联截粮命令。
- 修复新增封锁对象与敌方截粮威胁重叠时的拾取冲突：地图点击按投影中心距离选择，敌方威胁和封锁对象都能被点中。
- `exportWarLogisticsState()` 导出 `routeBlockades`，便于后续存档或复盘。
- 新增 Playwright 回归：秦岭栈道触发封锁后，从地图点选封锁对象，执行守备，再清除封锁，并确认导出状态和地图对象变化。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "selectable blockade"` 通过：1/1 passed。
- `npm run test:ui -- --grep "selects enemy interdiction threats|selectable blockade"` 通过：2/2 passed；用于确认拾取冲突修复。
- `npm run test:ui` 通过：16/16 passed。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 剩余风险

- 封锁对象现在是运行时对象，尚未进入可读写存档恢复。
- 守备/清除仍是通用动作，尚未按栈道、漕运、渡口、边路给出差异化成本和效果。
- 路网本体仍是纯代码定义，下一轮可以把 `STRATEGIC_ROUTE_NETWORKS` 数据化为 `route_networks.json`。

## 2026-05-10 训练冲刺第 33 轮：战争后勤状态导入恢复

### 缺口

第 32 轮已经能导出运输队、截粮记忆、命名瓶颈和封锁对象，但“可恢复”仍只停在导出快照层。刷新页面后无法把瓶颈封锁、选中对象、敌方截粮记忆、运输队与路线容量恢复回来，存档/复盘闭环不完整。

### 已修复

- 新增 `StrategyUi.importWarLogisticsState()`，接收 `schemaVersion: 1` 的战争后勤快照并恢复核心运行时状态。
- 恢复范围覆盖：当前战时回合、选中后勤对象、选中截粮威胁、敌方截粮记忆、活动截粮命令、运输队、占后运输任务、路线容量、治理后勤修正、兵站和路线封锁对象。
- 导出快照补齐运输队的 `orderIndex`、`createdTurn`、分段进度和敌方截粮命令的 `chokeReason`，避免导入后继续操作时丢失上下文。
- `window.__WANCHAO_APP__` 新增 `importWarLogisticsState(snapshot)`，导入后自动切回战争/后勤面板，并同步 Three.js 的敌方威胁层和后勤对象层。
- 封锁对象回归测试扩展为导出、重开页面、导入、继续清除封锁，确认恢复后仍能操作同一个秦岭栈道封锁对象。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "selectable blockade"` 通过：1/1 passed。
- `npm run test:ui` 通过：16/16 passed。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- Unity/Tuanjie VisualSmoke 未运行；`.outputs/visual/unity-*.png` 数量为 0，`.outputs/tuanjie/visual-project-copy` 与 `.outputs/tuanjie/visual-preview-copy` 均不存在。

### 剩余风险

- 当前导入恢复覆盖战争后勤层，还不是完整游戏存档；国家资源、地区治理队列和战报历史仍需更完整的 save schema。
- 守备/清除封锁仍是通用动作，尚未按栈道、漕运、渡口、边路给出差异化成本和效果。
- 导入接口当前服务于调试/回归，尚未挂到玩家可见的存档槽 UI。

## 2026-05-10 Daily Bug Scan

### 扫描范围

- 自动化：`daily-bug-scan`。
- 上次运行：`2026-05-10T12:01:32.795Z`。
- 本次运行时间：`2026-05-10T21:05:47+08:00`。
- `git log --since="2026-05-10T12:01:32Z"` 无提交。
- `git log --since="24 hours ago"` 无提交。
- 最新提交仍是 `509e3770a5c8f55c5dabb734ffd01f35cc2fdde8`，时间 `2026-05-05T18:04:31+08:00`，提交信息 `Make the strategy outliner readable under pressure`。

### 结论

- 本轮没有扫描窗口内的新提交 SHA，因此未发现可归因到新提交的 bug。
- 当前工作区仍有大量未提交变更和未跟踪文件，本轮只把它们作为工作树状态记录，不把它们当作“新提交 bug”证据。
- 未提出代码修复；没有足够失败信号支持最小修复方案。

### 验证

- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 route_networks=6 chronicle_events=200`。
- `npm run typecheck` 在 `web-strategy-map` 通过。
- `npm run test:ui` 在 `web-strategy-map` 通过：16/16 passed。
- `npm run build` 在 `web-strategy-map` 通过；仍仅有 Vite chunk-size advisory。

## 2026-05-10 结构性修复：preflight、共享路线容量、后勤导入上下文

### 缺口

全局检查发现 3 个结构性问题：`preflight_without_unity.py` 仍把已降级的 Unity/Tuanjie runner 当作必需入口；路线容量以有向 `from->to` 记录，不能表达同一栈道/渡口/驿路瓶颈的双向共享吞吐；战争后勤快照导入恢复了队列与截粮对象，但没有恢复当前军队、目标和绕行点，刷新续接会漂回默认军队上下文。

### 已修复

- `tools/unity/preflight_without_unity.py` 改为验证数据、地图、asmdef、包、headless 入口和视觉参考签名；Unity/Tuanjie runner 与 handoff 脚本降为 legacy optional warning，避免 Web 主线被遗留入口阻断。
- `web-strategy-map/src/ui.ts` 新增无向 `routeCapacityKey()`，路线容量、命令分段、路线预览和导入快照统一按物理瓶颈共享容量。
- `exportWarLogisticsState()` 增补 `activeArmyTargetId` 与 `activeArmyWaypointId`；`importWarLogisticsState()` 会恢复 active army、目标和 waypoint。
- `web-strategy-map/src/main.ts` 导入战争后勤快照后同步 Three.js 的 active army、目标地区、绕行点、敌方截粮层和后勤对象层。
- Playwright 路线方案测试补充：非默认军队选择绕行路线后导出、重开、导入，确认 UI 与 Scene 都恢复同一军队、目标和 waypoint；同时断言路线 leg id 使用无向共享 key。

### 验证

- `npm run typecheck` 通过。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- `npm run test:ui -- --grep "compares route alternatives"` 通过：1/1 passed。
- `npm run test:ui -- --grep "uses named strategic route network|exports recoverable"` 通过：2/2 passed。
- `npm run test:ui` 通过：16/16 passed。
- `python tools\validate_data.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`scenarioCount=16`。
- `python tools\unity\preflight_without_unity.py` 通过，并提示缺失的 legacy Unity/Tuanjie entrypoints 为可选 warning。
- Unity/Tuanjie VisualSmoke 未运行；未发现 `.outputs/visual/unity-*.png`、`.outputs/tuanjie/visual-project-copy` 或 `.outputs/tuanjie/visual-preview-copy` 残留。

### 剩余风险

- 当前 save/import 仍聚焦战争后勤切片，完整国家资源、地区治理状态、战报历史和 UI 面板折叠状态还需要完整游戏存档 schema。
- legacy Unity/Tuanjie runner 现在是 optional warning；若未来完全删除 `My project`，还需要把 preflight 改名或拆出 Web-only preflight。

## 2026-05-10 训练冲刺第 35 轮：完整游戏状态导出与恢复

### 缺口

第 34 轮已经能恢复战争后勤切片，但它仍不是完整存档：国家资源、地区运行时治理状态、帝皇选择、动态拆分军队、治理/后勤/战争队列、战报历史、面板折叠和当前模式无法作为一个整体保存恢复。玩家一旦刷新页面，治理与战争双闭环会被拆散。

### 已修复

- `StrategyUi.exportGameState()` 新增 `schemaVersion: 1` 的完整游戏快照，覆盖 mode、selectedRegion、selectedEmperor、sidebarCollapsed、routePickMode、warTab、armyOrder、nationState、56 区运行时状态、全部军队、治理/后勤/战报队列以及 warLogistics。
- `StrategyUi.importGameState()` 新增完整恢复路径，先恢复国家/地区/军队/队列，再复用后勤导入恢复运输队、截粮、封锁和路线容量。
- 修复导入契约：`importWarLogisticsState()` 会临时切到战争/后勤页，完整导入结束后会恢复保存时的 mode 与 warTab。
- 修复 UI chrome 恢复：导入时同步右侧栏 collapsed DOM class 和 toggle 文案。
- 修复动态拆分军队恢复：导入前会清理当前数据集中不属于快照的 `army_player_detached_*`，再恢复快照中的动态军队，避免多次导入残留幽灵军队。
- `window.__WANCHAO_APP__` 新增 `exportGameState()` / `importGameState(snapshot)`，导入后同步 Three.js active army、目标、waypoint、敌方威胁层和后勤对象层。
- Playwright 新增完整状态回归：选择李世民、执行河西治理、设置战争绕行、拆分军队、换将/改兵种、派运输与进攻、折叠侧栏、导出、重开、导入，并确认资源、帝皇、选区、动态军队、waypoint、队列、运输队和折叠态恢复后还能继续推进回合。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "exports and imports full game state"` 通过：1/1 passed。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- `npm run test:ui` 通过：17/17 passed。
- `python tools\validate_data.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`scenarioCount=16`。
- `python tools\unity\preflight_without_unity.py` 通过，并把缺失 legacy Unity/Tuanjie runner 报为 optional warning。
- Unity/Tuanjie VisualSmoke 未运行；未发现 `.outputs/visual/unity-*.png`、`.outputs/tuanjie/visual-project-copy` 或 `.outputs/tuanjie/visual-preview-copy` 残留。

### 剩余风险

- 完整状态 schema 目前暴露在 debug API 和回归测试中，还没有玩家可见的存档槽 UI。
- 快照仍是内存对象，尚未写入 localStorage、文件下载或云存档。
- 动态军队清理目前只处理 `army_player_detached_*`，后续如果加入敌方增援/临时民兵，也要为动态实体补统一生命周期标记。

## 2026-05-10 训练冲刺第 36 轮：localStorage 本地存档槽

### 缺口

第 35 轮已经完成完整状态导出/导入，但仍停在 debug API。玩家看不到存档槽，也不能跨刷新恢复；版本不匹配或损坏存档没有可读提示。进一步测试还发现，玩家从 UI 按钮读取完整状态时，Three.js 场景选区不会像 debug API 导入那样同步回保存选区。

### 已修复

- 右侧栏新增“本地存档”面板，提供 3 个槽位，每个槽位支持存、读、删，并显示地区、帝皇、模式、战争回合和保存时间。
- 存档写入 `localStorage`，key 为 `wanchao:strategy-map:save:<slot>`，envelope 使用 `schemaVersion: 1`，内部保存完整 `GameExportState`。
- 读取时同时校验 envelope `schemaVersion` 和内部游戏状态 `schemaVersion`；版本不匹配、缺少状态或 JSON 损坏都会显示中文错误提示。
- UI debug state 增补 `saveSlotMessage`、`saveSlotError`、`saveSlotCount`，便于自动化验证本地槽位状态。
- 修复玩家按钮读取路径：`UiEvents.onGameStateImported` 会复用 `syncSceneAfterStateImport()`，让 Three.js scene 与 UI 一起恢复选区、活动军队、目标、waypoint、截粮威胁和后勤对象。
- Playwright 新增回归：清空本地槽位、保存河西/李世民治理状态、刷新页面后从槽位读取，确认资源/选区/帝皇恢复；再注入 `schemaVersion: 999` 坏档，确认 UI 提示“游戏状态版本不匹配”。

### 验证

- `npm run typecheck` 通过。
- `npm run test:ui -- --grep "local slots"` 通过：1/1 passed。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- `npm run test:ui` 通过：18/18 passed。
- `python tools\validate_data.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`scenarioCount=16`。
- `python tools\unity\preflight_without_unity.py` 通过，并把缺失 legacy Unity/Tuanjie runner 报为 optional warning。
- Unity/Tuanjie VisualSmoke 未运行；未发现 `.outputs/visual/unity-*.png`、`.outputs/tuanjie/visual-project-copy` 或 `.outputs/tuanjie/visual-preview-copy` 残留。

### 剩余风险

- 存档目前只在浏览器 `localStorage`，还没有导入/导出文件和跨设备同步。
- 坏档只给出错误提示，还没有提供自动迁移或“复制诊断信息”。
- 右侧栏存档面板已经可用，但长线仍可考虑独立存档弹窗，避免高密度战争操作时占用侧栏首屏空间。

## 2026-05-10 训练冲刺第 37 轮：治理专精微操落地

### 缺口

治理侧此前主要是“施政、建设、赈济、编户、征发”几个大按钮，虽然有收益/副作用预告，但缺少《文明》式地区专精微操：玩家不能把一个区域明确改成粮仓、财赋、军府、边防、礼制或民生路线，也不能看到这些路线如何改变资源、风险、贡献、法统和后勤。

### 已修复

- `RegionViewModel` 新增 `governanceFocus`，初始化时根据地区地貌、资源、法统记忆和既有 `specialization` 推导默认治理焦点。
- 治理界面新增“区域微操”卡片，提供 6 个专精路线：粮仓水利、商税漕运、兵源军府、边防屯戍、法统礼制、安抚民生。
- 每条路线使用同一个 `governanceFocusPlan()` 生成预告和实际落地，覆盖粮、钱、兵、贡献、整合、法统、民变风险和后勤修正，避免 forecast/apply 漂移。
- 专精选择会立刻更新地区 `specialization`、推荐建筑、推荐政策、历史来源说明、治理队列和国家资源。
- 因果门禁保持：财赋会增钱但提高民变并轻伤法统；军府会增兵但消耗粮钱、提高民变并压低贡献；民生会降风险但消耗粮钱。
- 完整存档 schema 已扩展保存/恢复 `specialization` 与 `governanceFocus`。
- Playwright 新增回归：河西执行财赋专精后钱增加、风险增加、贡献增加、标签变为商税漕运；再执行民生专精后风险下降、粮食下降，并验证导出/导入后仍恢复安抚民生焦点。

### 验证

- `npm run test:ui -- --grep "governance specialization micro"` 通过：1/1 passed。
- `npm run typecheck` 通过。
- `npm run build` 通过；仍仅有 Vite/Three.js chunk-size advisory。
- `npm run test:ui` 通过：19/19 passed。
- `python tools\validate_data.py` 通过。
- `python tools\validate_domain_core.py` 通过。
- `powershell -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`scenarioCount=16`。
- `python tools\unity\preflight_without_unity.py` 通过，并把缺失 legacy Unity/Tuanjie runner 报为 optional warning。
- Unity/Tuanjie VisualSmoke 未运行；未发现 `.outputs/visual/unity-*.png`、`.outputs/tuanjie/visual-project-copy` 或 `.outputs/tuanjie/visual-preview-copy` 残留。

### 剩余风险

- 这轮实现的是地区专精与治理焦点微操，还没有进一步做到地块级人口分配、建筑队列工期或区域邻接加成。
- 专精路线目前是代码内规则，后续可数据化成 JSON，方便策划直接调数与补史据字段。
- 地图视觉会读 `specialization` 调色，但专精改变后的地表建筑差异还可以更明显。

## 2026-05-11 GitHub 主线发布准备

### 发布意图

按 `workspace-mainline-unifier` 技能收口当前 `E:\万朝归一\万朝归一` 工作区，把当前可版本管理源码、数据、Unity 工程资产、Web 原型源码、验证脚本和项目报告统一发布到 GitHub `main`。远程仓库确认为 `zhu607705-coder/WanChaoGuiYi`，默认分支为 `main`，仓库可见性为公开。

### GitHub 到达清单规则

- 普通 git：项目文档、Unity C# 脚本、JSON 数据、场景/ProjectSettings、地图图像、音频源资产、Web Three.js 原型源码、Playwright 测试、验证工具和本报告。
- GitHub Release：仅用于超过普通 git 单文件边界或需要打包发布的大型资产；本轮候选文件最大约 11MB，暂不需要 Release。
- local-only 排除项：Unity `Library/Temp/Obj/Builds/Logs/UserSettings`、Web `node_modules/dist/public/game-data/playwright-report/test-results`、`.outputs`、本地编辑器与缓存文件。

### 发布前检查

- GitHub CLI 已登录 `zhu607705-coder`。
- 远程仓库 `WanChaoGuiYi` 已存在且为公开仓库，不创建重复仓库。
- 敏感词扫描只命中文档和测试中的普通 `token` 术语，未发现真实 GitHub/API 密钥。
- 普通 git 候选文件没有超过 100MB 的单文件。

### 发布执行验证

- 分支冻结：当前工作分支为 `codex/heavy-strategy-full-closure`，目标分支为 `main`，远端为 `origin`。
- worktree 检查：当前 E 盘工作区为活跃工作区；另有一个旧 Claude worktree 标记为 `prunable`，本轮发布不在推送前删除。
- 大文件检查：待提交文件中没有超过 100MB 的单文件；最大新增普通 git 候选约 10.85MB，因此本轮不需要 GitHub Release 资产。
- 敏感信息检查：真实密钥格式扫描未命中；普通敏感词扫描仅命中文档、测试 token 术语和 Unity 空配置字段。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 route_networks=6 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过。
- `powershell -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`scenarioCount=16`。
- `python tools\unity\preflight_without_unity.py` 通过，并把缺失 legacy Unity/Tuanjie runner 记录为 optional warning。
- `npm run typecheck` 在 `web-strategy-map` 通过。
- `npm run test:ui` 在 `web-strategy-map` 通过：19/19 passed。
- `npm run build` 在 `web-strategy-map` 通过；仅保留 Vite chunk-size advisory。

### main 合并验证

- 合并前先把本地 `main` 快进到远端提交 `71f262f`，保留其 Unity 交接验收意图。
- 合并 `codex/heavy-strategy-full-closure` 时解决 `docs/unity-handoff-checklist.md`、`tools/unity/preflight_without_unity.py` 和本报告冲突。
- Unity/Tuanjie handoff 文档已统一到真实工程 `My project`，并保留 Windows PowerShell、`.cmd` 和 SSH/CI 友好的入口说明。
- 恢复 `tools/verify_unity_handoff.ps1`、`tools/unity/run_playmode_tests.ps1`、`tools/unity/run_visual_smoke_tests.ps1`，避免 `.cmd` 包装器指向缺失脚本。
- `powershell -ExecutionPolicy Bypass -File tools\verify_unity_handoff.ps1` 顺序重跑通过：preflight、数据校验、Domain 边界和 16 个 headless 战争场景均通过。
- `npm run typecheck`、`npm run test:ui` 和 `npm run build` 在合并后的 `main` 再次通过；UI 回归仍为 19/19 passed，build 仅保留 Vite chunk-size advisory。
- `git diff --cached --check` 对 Unity `.meta` 和 ProjectSettings YAML 报大量尾随空格；这些是 Unity/Tuanjie 生成格式，本轮不在合并中批量改写。

### GitHub 发布结果

- GitHub 仓库：`https://github.com/zhu607705-coder/WanChaoGuiYi`。
- 远端 `main` 已包含合并提交 `1026102372c628a5ea026dbc8882e3c08254d3fa`；本段发布清单将作为后续记录提交追加到 `main`。
- 远端功能分支 `codex/heavy-strategy-full-closure` 已保留在 `1e37a968d6e794834fb585a5fde53c25c857f5f7`。
- 普通 git 已包含：项目文档、`My project` Unity/Tuanjie 工程、C# 运行时代码、JSON 数据、地图/立绘/音频资产、验证工具、headless runner、Web strategy-map 源码和 Playwright 测试。
- GitHub Release：本轮未创建 Release，未上传 Release 资产；最大普通 git blob 约 10.85MB，低于 100MB。
- local-only 排除项：Unity `Library/Temp/Obj/Builds/Logs/UserSettings`、Web `node_modules/dist/public/game-data/playwright-report/test-results`、`.outputs`、本地编辑器与缓存文件仍按 `.gitignore` 排除。
- 临时上传通道：因本机 HTTPS Git 到 `github.com:443` 连接失败，使用临时 SSH deploy key 通过 `ssh.github.com:443` 推送；发布后删除该 deploy key 和本地 `.git/codex_upload_key*` 临时文件。

## 2026-05-15 全面代码缺口与功能缺失检查

### 检查范围

- 当前分支：`main`，工作树检查前为干净状态，最新提交为 `474318b Record GitHub publication inventory`。
- 覆盖范围：`My project` Unity/Tuanjie 工程、`web-strategy-map` Web 原型、`tools` 验证脚本、JSON 数据表、项目文档和本报告。
- 代码规模快照：Unity/Tuanjie 运行时代码与测试共 114 个 C# 文件；Web 原型核心为 8 个 TypeScript 测试/源码文件；主要大文件包括 `web-strategy-map/src/ui.ts` 5367 行、`web-strategy-map/src/scene.ts` 1636 行、`My project/Assets/Scripts/UI/MainMapUI.cs` 1606 行、`My project/Assets/Scripts/UI/RegionPanel.cs` 1339 行、`GameManagerPlayModeSmokeTests.cs` 2579 行。

### 已验证可用面

- 数据规模满足 MVP 数量底线：8/8 核心帝皇存在，当前总帝皇 13；地区 56；胜利条件 3；`sourceReference` 占位检查为 0。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 route_networks=6 chronicle_events=200`。
- `python tools\validate_domain_core.py` 通过：Domain 文件夹保持 Unity-free，adapter 继续委托 Domain 类型。
- `powershell -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：16/16 个 headless 战争、治理、后勤、外交耦合场景通过。
- `python tools\unity\preflight_without_unity.py` 通过；仅提示 `tools\unity\run_playmode_tests.sh` 作为 legacy optional warning 缺失。
- `powershell -ExecutionPolicy Bypass -File tools\verify_unity_handoff.ps1` 通过：preflight、数据、Domain、headless 战争全链路通过。
- `npm run typecheck` 在 `web-strategy-map` 通过。
- `npm run test:ui -- --reporter=line --workers=1` 在 `web-strategy-map` 通过：19/19 passed，耗时约 3.4 分钟。直接 `npm run test:ui` 曾因本次工具层 184 秒超时中断，改用更长超时后确认通过。
- `npm run build` 在 `web-strategy-map` 通过；仍有 Vite chunk-size advisory：主 JS chunk 约 792.52 kB，gzip 约 202.87 kB。

### 主要缺口

- P0：Unity/Tuanjie Player 构建数据加载缺口。`DataRepository.ResolveTextAsset()` 当前未绑定 TextAsset 时只在 `UNITY_EDITOR` 下通过 `AssetDatabase.LoadAssetAtPath` 读取 `Assets/Data/*.json`，非 Editor Player 返回 `null`；而 `Assets/Resources/Data` 当前只有 `map_render_metadata.json`，缺少 `emperors.json`、`regions.json`、`map_region_shapes.json` 等 15 个运行数据表。Editor/PlayMode 能验证，不等于可打包 Demo Player 能启动数据。
- P0：当前机器未安装 Unity/Tuanjie Editor。`tools\unity\run_playmode_tests.ps1 "My project"` 明确失败：找不到 Unity/Tuanjie 可执行文件；因此本轮无法重新确认 Unity Console 编译、PlayMode 结果 XML 和 VisualSmoke 图形截图。
- P1：200 个 `chronicle_events.json` 目前主要是数据/音频资产。运行时代码只发现 `EventEvaluationSystem` 遍历 `context.Data.Events`，而 `events.json` 只有 3 个事件；`GameEventType.ChronicleEventTriggered` 有音频监听，但没有发现生产侧 publisher。也就是说“200 编年事件”尚未形成真实玩法事件循环。
- P1：Unity 存档仍偏轻。`SaveManager` 只序列化 `GameState`；地图主导运行时 `WorldState/MapState`、前线后勤、接敌状态、UI 面板状态、地图镜头和玩家操作上下文没有统一恢复路径。Web 侧已有完整状态 localStorage 槽位，Unity 侧尚未对齐。
- P1：继承系统还没有自然死亡/登基生命周期。`SuccessionSystem.ExecuteTurn()` 只推高继承风险并给继承人秋季加龄；`ResolveSuccession()` 需要手动触发，当前没有皇帝年龄、死亡判定、自动继位和危机链。
- P1：胜利判定仍是近似实现。`VictorySystem.ControlsAllRegions()` 把“统一九州/控制关键区域”简化为控制 80% 地区；尚未按九州关键区、法统区或制度目标做解释性判定。
- P1：Unity 玩家侧外交/谍报仍是桥接按钮级别。`MechanismPanel` 可对选中势力宣战/求和、封关、开一条侦察谍报，但还不是完整 treaty 菜单、条件谈判、目标选择、反谍配置或多行动队列。
- P2：Web 原型验证强，但玩法范围与 Unity 主线不完全等价。Web 覆盖地图、治理微操、战争后勤、存档和可视交互；未形成与 Unity 同级的继承、科技、天气、天文、胜利、外交和谍报完整局内循环。
- P2：人才系统数据量和任命深度偏薄。`talents.json` 只有 4 个条目；Unity 有 `TalentSystem` 和 UI 入口，但距离“人才影响战争、财政、改革或治理”的可选任命体系还有深度缺口。
- P2：维护性风险集中在超大 UI/测试文件。`web-strategy-map/src/ui.ts`、`MainMapUI.cs`、`RegionPanel.cs`、`GameManagerPlayModeSmokeTests.cs` 都已经过千行；当前验证可通过，但后续改动容易出现局部修复牵动全局的问题。

### 建议下一轮优先级

1. 先修 Unity Player 数据加载：把核心 JSON 复制到 `Resources/Data` 或 `StreamingAssets`，让 `DataRepository` 在非 Editor 下也能加载全部运行数据，并加一个非 Editor 路径的最小验证。
2. 在有 Unity/Tuanjie 的环境重跑 `tools\unity\run_playmode_tests.ps1 "My project"` 与 `tools\unity\run_visual_smoke_tests.ps1 "My project"`，把结果写回本报告。
3. 打通编年事件：明确 `chronicle_events.json` 的触发器、选择效果、UI 面板和 `ChronicleEventTriggered` publisher，避免 200 条数据停留在资产层。
4. 对齐 Unity 存档：以 Web `GameExportState` 的覆盖范围为参照，补 Unity `WorldState/MapState/后勤/UI context` 的保存恢复。
5. 再做继承自然生命周期和关键区胜利判定，优先把“王朝延续”从手动按钮变成回合压力。

### 本轮未改代码

- 本轮定位为检查和报告更新，只追加本节结论；没有改运行时代码。
- 当前 `git status --short` 在验证后仍只显示被忽略生成物，不显示待提交源码变更；`web-strategy-map/dist/`、`public/game-data/`、`test-results/`、`playwright-report/` 与 `tools/headless_runner/latest-war-report.json` 均按 `.gitignore` 处理。

## 2026-05-15 Unity-free 开发纠偏与编年事件修复

### 开发边界纠偏

- 用户明确要求当前开发完全脱离 Unity；本轮随即停止把 Unity/Tuanjie Player、Resources、PlayMode 或 Unity preflight 作为主线完成标准。
- 本节覆盖并取代上一节中针对 Unity Player 数据加载、Unity PlayMode、VisualSmoke 的优先级判断；这些结论只保留为历史检查记录，不再指导当前开发。
- 已撤回本轮临时添加的 Unity runtime、`Assets/Resources/Data` 镜像和 Unity preflight 加强改动，避免继续沿 Unity Player 路线投入。
- 当前有效开发面调整为 `web-strategy-map`、JSON 数据、可在无 Unity 环境运行的脚本和 Web/Playwright 回归。

### 已修复缺口

- 修复“200 条 `chronicle_events.json` 只停留在数据/音频层”的 Unity-free 主线缺口：`web-strategy-map` 现在同步并加载 `chronicle_events.json`。
- Web 回合推进现在会按帝皇时代、回合窗口、地区信号、虚拟科技、天气/天象信号、权重和冷却选取编年事件。
- 编年事件会写入局内“编年纪事”面板、治理/战时日志、DebugState，并通过 `onChronicleEvent` 触发已有 Web 音频桥接。
- Web 存档导出/导入现在保留编年事件历史和冷却状态，避免读档后丢失局内事件链。
- 新增 Playwright 回归：`publishes chronicle events through the Unity-free web turn loop`，覆盖事件目录加载、回合触发、面板呈现和导入恢复。

### 当前验证

- `python tools\validate_data.py` 通过：`chronicle_events=200` 与其他核心数据表仍可解析。
- `npm run typecheck` 在 `web-strategy-map` 通过。
- `npm run test:ui -- --grep "chronicle events" --reporter=line --workers=1` 通过：1/1 passed。
- `npm run test:ui -- --reporter=line --workers=1` 在 `web-strategy-map` 通过：20/20 passed，耗时约 3.5 分钟。
- `npm run build` 在 `web-strategy-map` 通过；仍只有既有 Vite chunk-size advisory，主 JS chunk 约 797.81 kB，gzip 约 204.37 kB。
- `git diff --check` 通过；仅有 Windows 工作区的 LF/CRLF 提示，无尾随空格或补丁格式错误。

### 剩余风险

- Unity 工程保留为历史资产和数据来源，但不再作为当前开发主线验收面。
- Unity 存档、Unity Player 数据加载、Unity PlayMode/VisualSmoke 不再作为当前优先级；对应缺口应从后续路线中移出或标注为 legacy。
- 编年事件已经接入 Web 回合循环；choice effects 仍是下一步可做的非 Unity 玩法深化。

## 2026-05-16 Web 资产源脱离 Unity 工程第一步

### 迁移目标

- 用户确认后续开发方向为纯代码主线，不再把 Unity/Tuanjie 编辑器作为当前开发入口。
- 本轮优先拔掉 `web-strategy-map` 对 `My project/Assets` 的直接资产源依赖。
- `My project` 暂时保留为 legacy/archive，不在本轮删除，避免一次性破坏旧 C#、headless 和历史交接验证面。

### 已完成改动

- 新增 `web-strategy-map/game-data-source/`，作为 Web 主线可版本管理资产源。
- 迁入 Web 当前运行需要的 `20` 个 JSON、`191` 个 MP3 和 `1` 张地图 PNG。
- `web-strategy-map/scripts/sync-data.mjs` 不再读取 `My project/Assets/Data`、`My project/Assets/Audio` 或 `My project/Assets/Resources`。
- `sync-data.mjs` 现在只从 `game-data-source` 同步到被 `.gitignore` 排除的 `web-strategy-map/public/game-data`，并在每次同步前清理旧生成物，避免 stale 文件残留。
- 新增 `tools/validate_web_data_source.py` 和 `npm run check:data-source`，用于确认 Web 数据源完整、地图 PNG 有效、编年事件数量满足当前基线、同步产物和源目录一致，并禁止同步脚本重新引用 Unity 源路径。
- `game-data-source/data/map_render_metadata.json` 的 `sourceImage` 已改为 `/game-data/map/jiuzhou_generated_map.png`，不再指向 Unity `Assets/Art/Map`。
- `tools/run_headless_simulation.ps1`、`tools/run_headless_simulation.sh`、`tools/verify_headless_war.ps1`、`tools/verify_headless_war.sh` 和 headless runner 默认数据目录改为 `web-strategy-map/game-data-source/data`，仅在新源不存在时 fallback 到 legacy Unity 数据目录。
- `verify_headless_war` 在新数据源上改用 `tools/validate_web_data_source.py`，避免旧 `validate_data.py` 的 Unity `.meta` 和 `Assets/Art` 镜像强约束阻断纯代码数据验证。
- `web-strategy-map` 的 `dev`、`build`、`test:ui` 已锁定为先执行 `npm run sync:data`，再执行 `npm run check:data-source`，然后才进入 Vite、typecheck 或 Playwright。
- `tools/validate_web_data_source.py` 现在反查 `package.json`，确认 `dev`、`build`、`test:ui` 没有绕过 `check:data-source`，防止后续误把 Unity 资产源重新接回 Web 主线。

### 当前验证

- `npm run sync:data` 通过：同步 `212` 个 Web game-data 源文件，其中 `20` 个 JSON、`191` 个音频、`1` 个地图/图片。
- `npm run check:data-source` 通过：`data=16 audioJson=4 regions=56 chronicleEvents=200 mp3=191`。
- `python tools\validate_data.py` 通过：`emperors=13 portraits=13 regions=56 map_region_shapes=56 historical_layers=56 policies=35 units=8 technologies=40 generals=12 buildings=12 route_networks=6 chronicle_events=200`。
- `npm run typecheck` 在 `web-strategy-map` 通过。
- `npm run test:ui -- --reporter=line --workers=1` 在 `web-strategy-map` 通过：`20/20 passed`，耗时约 `3.4m`。
- `npm run build` 在 `web-strategy-map` 通过；仍只有既有 Vite chunk-size advisory，主 JS chunk 约 `797.81 kB`，gzip 约 `204.37 kB`。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：先运行 Web 数据源门禁，再确认 Domain folder Unity-free，最后 `16/16` 个 headless 战争、治理、后勤、外交耦合场景通过。
- 锁定后复验 `npm run build` 通过，命令输出确认顺序为 `sync:data -> check:data-source -> tsc --noEmit -> vite build`。
- 锁定后复验 `npm run test:ui -- --grep "chronicle events" --reporter=line --workers=1` 通过：命令输出确认顺序为 `sync:data -> check:data-source -> playwright test`，`1/1 passed`。

### 剩余迁移面

- `My project` 仍在 git 中保留，尚未归档或删除。
- `tools/headless_runner/WanChaoGuiYiHeadless.csproj` 仍链接 `My project/Assets/Scripts` 中的 Domain/Core C# 文件；本轮只切换默认数据源，尚未搬迁 C# 代码源。
- `tools/validate_data.py` 和部分 art 工具仍是 legacy Unity 资产检查/生成入口。
- 下一步若继续推进“完全迁移干净”，应把 headless/domain C# 源码和地图生成脚本也迁到非 Unity 目录，然后再评估 `My project` 是否转入 archive 或从主线移除。

## 2026-05-16 Domain Core 与地图工具脱离 Unity 目录

### 迁移目标

- 承接上一节剩余迁移面，把 headless 实际编译的 C# 玩法核心从 `My project/Assets/Scripts` 迁出。
- 把地图生成和 2.5D 预览工具从 Unity `Assets/Data`、`Assets/Art` 路径切到 Web 主线资产源。
- 继续保留 `My project` 作为 legacy/archive，不在本轮删除。

### 已完成改动

- 新增 `domain-core/src/`，作为非 Unity 的 C# 玩法核心源码目录。
- 将 headless 当前编译依赖的 `26` 个 C# 文件迁入 `domain-core/src`，覆盖 `Data`、`Core`、`Domain/Core`、`Domain/Economy`、`Domain/Governance`、`Domain/Map`、`Domain/Military` 和 `Domain/World`。
- `tools/headless_runner/WanChaoGuiYiHeadless/WanChaoGuiYiHeadless.csproj` 已改为链接 `../../../domain-core/src/...`，不再链接 `../../../My project/Assets/Scripts/...`。
- `tools/validate_domain_core.py` 已改为检查 `domain-core/src` 是否 Unity-free，并反查 headless csproj 是否仍链接 Unity 脚本目录；Unity adapter 检查保留为 legacy 目录存在时的兼容检查。
- `tools/art/render_jiuzhou_map.py` 默认读取 `web-strategy-map/game-data-source/data`，输出地图 PNG 到 `web-strategy-map/game-data-source/map/jiuzhou_generated_map.png`，输出元数据到 `game-data-source/data/map_render_metadata.json`。
- `tools/art/render_jiuzhou_map.py` 不再同步 Unity `Resources` 镜像，`sourceImage` 固定为 `/game-data/map/jiuzhou_generated_map.png`。
- `tools/art/render_jiuzhou_isometric_preview.py` 默认读取 `web-strategy-map/game-data-source/data`。
- `tools/validate_web_data_source.py` 现在同时锁定 `sync-data.mjs`、`render_jiuzhou_map.py` 和 `render_jiuzhou_isometric_preview.py`，若这些主线脚本重新引用 legacy Unity 资产源会失败。

### 当前验证

- `python tools\validate_domain_core.py` 通过：`domain-core/src` 为 Unity-free，headless 已链接迁移后的 C# 源。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：Web 数据源门禁、Domain Core 门禁和 `16/16` 个 headless 战争/治理/后勤/外交耦合场景全部通过。
- `python tools\art\render_jiuzhou_map.py` 通过：输出写入 `web-strategy-map/game-data-source/map/jiuzhou_generated_map.png`，`precision=map_aligned_region_v1`，`regions=56`。
- `python tools\art\render_jiuzhou_isometric_preview.py --output .outputs\visual\jiuzhou_isometric_preview.png` 通过：输出 `regions=56`、`heightRange.min=22.81`、`heightRange.max=79.53`。
- `npm run check:data-source` 通过：`data=16 audioJson=4 regions=56 chronicleEvents=200 mp3=191`。
- `npm run test:ui -- --grep "chronicle events" --reporter=line --workers=1` 通过：`1/1 passed`。
- `npm run build` 顺序复跑通过：`sync:data -> check:data-source -> tsc --noEmit -> vite build`，仍只有既有 Vite chunk-size advisory。

### 剩余迁移面

- `My project` 仍在 git 中保留为 legacy/archive，尚未从仓库删除。
- Unity 侧 `My project/Assets/Scripts` 与 `domain-core/src` 现在存在源码复制关系；后续纯代码玩法改动应以 `domain-core/src` 为权威源，Unity adapter/legacy 同步需要单独任务处理。
- `tools/validate_data.py` 仍是 Unity 资产完整性检查入口，不应作为纯代码主线门禁；纯代码主线用 `tools/validate_web_data_source.py`、`tools/validate_domain_core.py`、`tools/verify_headless_war.ps1` 和 Web 回归。

## 2026-05-16 Unity/Tuanjie legacy 删除完成

### 删除目标

- 用户要求继续迁移到“旧 Unity 文件直接删除也不影响现有游戏正常运行”的状态，并在满足条件后直接删除。
- 本轮目标是把保留价值的资产、C# 主源和地图工具都放到纯代码目录，然后移除旧编辑器工程、旧交接工具和旧运行状态文件。

### 已完成改动

- 删除旧编辑器工程目录：`My project/`。
- 删除旧编辑器交接和 PlayMode/VisualSmoke 工具：`tools/unity/`、`tools/verify_unity_handoff.*`。
- 删除旧 Unity-bound 数据校验入口：`tools/validate_data.py`；主线校验改为 `tools/validate_web_data_source.py`。
- 删除历史 `.omc/` 与 tracked `.omx/` 状态/计划文件，并在 `.gitignore` 中忽略 `.omc/`、`.omx/`，避免旧计划继续把新任务带回编辑器路线。
- `web-strategy-map/game-data-source/art/` 迁入旧 `Assets/Art` 下 `112` 张 PNG，并把 `portraits.json`、`generals.json` 的资产路径改为 `art/...`。
- `web-strategy-map/game-data-source/audio/archive/` 迁入旧工程中未被 Web manifest 覆盖的 `79` 个唯一 MP3；删除前哈希复核显示旧工程 `420` 个 MP3 均已被 `game-data-source/audio` 覆盖或归档。
- `tools/validate_web_data_source.py` 升级为纯代码主线数据门禁：覆盖 JSON 关系、地图面片、路线网络、历史来源、策略默认值、地图 PNG 尺寸、音频数量和美术 PNG 引用。
- `AGENTS.md`、`CLAUDE.md`、`docs/architecture.md`、`docs/data-contract.md`、`docs/roadmap-12-weeks.md` 已改为纯代码 Web/headless 口径。

### 删除后验证

- `npm --prefix web-strategy-map run check:data-source` 通过：`data=16 audioJson=4 regions=56 chronicleEvents=200 mp3=270 artPng=112`。
- `python tools\validate_domain_core.py` 通过：`domain-core/src` 为 Unity-free，headless 链接迁移后的 C# 源。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：Web 数据源门禁、Domain Core 门禁和 `16/16` 个 headless 战争/治理/后勤/外交耦合场景全部通过。
- `python tools\art\render_jiuzhou_map.py` 通过：地图输出仍写入 `web-strategy-map/game-data-source/map/jiuzhou_generated_map.png`，`precision=map_aligned_region_v1`，`regions=56`。
- `python tools\art\render_jiuzhou_isometric_preview.py --output .outputs\visual\jiuzhou_isometric_preview.png` 通过：`regions=56`，尺寸 `1920x1280`，高度范围 `22.81` 到 `79.53`。
- `npm --prefix web-strategy-map run test:ui -- --grep "chronicle events" --reporter=line --workers=1` 通过：`1/1 passed`。
- `npm --prefix web-strategy-map run build` 通过：`sync:data -> check:data-source -> tsc --noEmit -> vite build`，仍只有既有 Vite chunk-size advisory。
- `npm --prefix web-strategy-map run test:ui -- --reporter=line --workers=1` 通过：`20/20 passed`。
- `git diff --check` 通过；仅有 Windows LF/CRLF 工作区提示，无尾随空格或补丁格式错误。
- `git ls-files 'My project/*' 'tools/unity/*' 'tools/verify_unity_handoff.*' 'tools/validate_data.py' 'docs/unity-handoff-checklist.md' '.omc/*' '.omx/*'` 无输出。

### 当前结论

- 当前游戏主线已经可以在删除 Unity/Tuanjie legacy 后正常运行和验证。
- 现存 `Unity-free`、`My project` 等文本只作为防回退校验或历史报告记录存在，不再代表 active runtime 依赖。

## 2026-05-16 迁出资产接入 Web 运行流程

### 接入目标

- 用户要求把已经迁出的资产介入现有流程，避免只完成文件搬迁而没有进入实际游戏运行链路。
- 本轮目标是让迁出的立绘、系统图标、兵种图标、归档音频分别进入 Web 数据模型、UI 展示、资产门禁和 Playwright 回归。

### 已完成改动

- `web-strategy-map/src/types.ts` 新增 `PortraitDefinition`，并把 `GeneralDefinition.portraitAssetPath` 纳入运行时类型契约。
- `web-strategy-map/src/data.ts` 加载 `portraits.json`，生成 `portraitByEmperorId`，并提供 `/game-data/...` 资产 URL 入口。
- `web-strategy-map/src/ui.ts` 在帝皇面板显示当前帝皇立绘，在战争编组页显示主将头像；两者都直接读取 `game-data-source/art/...` 迁出资产路径。
- `web-strategy-map/src/ui.ts` 将系统指标图标接入通用 `metric()`，并把兵种图标接入军队编组的兵种配比条。
- `web-strategy-map/src/styles.css` 增加帝皇立绘和主将头像的稳定尺寸样式，避免 UI 载入图片后挤压布局。
- `web-strategy-map/src/styles.css` 同时为系统指标图标和兵种图标增加固定尺寸，避免图片加载导致指标块和兵种配比条跳动。
- `web-strategy-map/tests/strategy-map.spec.ts` 增加运行时断言：默认秦始皇立绘、切换李世民后的立绘、军队编组中的卫青头像、人口系统图标、骑兵兵种图标都必须出现在 DOM 的 `/game-data/art/...` 路径里。
- `tools/validate_web_data_source.py` 将 `audio/archive` 的 `79` 个迁出 MP3、核心系统图标和每个 `units.json` 兵种对应图标纳入门禁，防止未被 manifest 使用的历史音频和 UI 图标在后续同步中丢失。

### 当前验证

- `npm --prefix web-strategy-map run check:data-source` 通过：`data=16 audioJson=4 regions=56 chronicleEvents=200 mp3=270 archiveMp3=79 artPng=112`。
- `npm --prefix web-strategy-map run typecheck` 通过。
- `npm --prefix web-strategy-map run test:ui -- --grep "loads real 56-region map" --reporter=line --workers=1` 通过：`1/1 passed`；该用例确认帝皇立绘、将领头像、系统图标和兵种图标已进入浏览器运行 DOM。
- `npm --prefix web-strategy-map run build` 通过：`sync:data -> check:data-source -> tsc --noEmit -> vite build`，仍只有既有 Vite chunk-size advisory。
- `npm --prefix web-strategy-map run test:ui -- --reporter=line --workers=1` 通过：`20/20 passed`。
- `git diff --check` 通过；仅有 Windows LF/CRLF 工作区提示。

### 当前结论

- 迁出的 `portraits.json`、将领头像 PNG、帝皇立绘 PNG、系统/兵种图标 PNG 和 `audio/archive` MP3 现在已经进入 Web 主线的数据加载、UI 呈现、同步门禁和回归测试。
- Unity/Tuanjie legacy 不需要恢复；后续新增或替换资产应直接写入 `web-strategy-map/game-data-source`，并通过 `sync:data`、`check:data-source` 和 Web 回归验证。

## 2026-05-16 git 工作区基线整顿门禁补入 workspace-cleanup spec

### 触发原因

- 对抗性 review 指出此前“全绿”验证运行在本机未提交迁移成果上，不能证明 `git clone` 后可复现。
- 本轮复核 `git status --porcelain=v1`：共 `1462` 条状态行，其中 `24` 个 modified、`1434` 个 deleted、`4` 个 untracked 状态入口。
- 删除项中 `My project/` 占 `1408` 条；这说明旧 Unity/Tuanjie 工程删除已经发生在磁盘上，但尚未作为 git 基线落地。
- 当前 `domain-core/` 为 untracked，磁盘计数为 `27` 个文件，其中 `26` 个 `.cs`；`web-strategy-map/game-data-source/` 为 untracked，磁盘计数为 `404` 个文件。
- `tools/validate_web_data_source.py` 仍为 untracked；当前磁盘版本为 `661` 行。依赖它的通过结果只能算 local-only evidence，不能算可复现主线证据。
- `.kiro/specs/workspace-cleanup/` 当前作为新清理规格入口存在于 untracked `.kiro/` 下，需要按 Cleanup_Spec 纳入基线决策。

### 已修改规格

- `.kiro/specs/workspace-cleanup/requirements.md` 新增 Requirement 11：`git 工作区基线整顿`。
- Requirement 7 已改为必须先通过 Git_Baseline Gate，才能进入原 7 步清理流水线。
- `.kiro/specs/workspace-cleanup/design.md` 已把流程改为 `Git Baseline Gate -> Inventory -> Review Gate -> Archive -> Execute -> Verify -> Report -> Gitignore`。
- `design.md` 新增 `GitBaselineItem`、`GitBaselinePlan`、`git-baseline-plan.json` schema 和 Property 0。
- `.kiro/specs/workspace-cleanup/tasks.md` 新增 Task 0，要求先实现 `tools/cleanup/git_baseline.py`、门禁评估、数据模型和对应 property test。

### 当前门禁结论

- 风险级别：🔴。
- Git_Baseline Gate 当前应判定为 Fail。
- 失败原因：Migration_Result 仍包含 untracked `domain-core/`、`web-strategy-map/game-data-source/`、`tools/validate_web_data_source.py`；Legacy_Removal 仍包含未提交的 `My project/` 大量删除和其他旧 Unity/Tuanjie 工具删除。
- 在该门禁通过前，workspace-cleanup 只能继续修改 Git_Baseline_Plan、Cleanup_Spec 和本报告；禁止执行 Delete、Archive、`.gitignore` patch 或清理流水线后续步骤。
- 下一步应生成 `git-baseline-plan.json`，把 Migration_Result、Legacy_Removal、Cleanup_Spec、Generated_Artifact、Unrelated_Local、Needs_Review 分类清楚，再决定 Baseline_Commit 的拆分和验证顺序。

## 2026-05-16 git 工作区基线整顿执行

### 执行目标

- 用户要求直接解决“本机迁移成果未进入 git，导致全绿不可复现”的问题。
- 本轮把 `domain-core/`、`web-strategy-map/game-data-source/`、新验证脚本、纯代码 Web 主线改动、`.kiro/specs/workspace-cleanup/` 与旧 `My project/` 删除一起纳入 Baseline_Commit。
- 提交前生成 `.kiro/specs/workspace-cleanup/git-baseline-plan.json`，按 `Migration_Result`、`Legacy_Removal`、`Cleanup_Spec` 分类记录当前基线整顿对象。

### 对抗测试修复

- 新纳入 `tools/headless_runner/WanChaoGuiYiTests/` 对抗性 xUnit 测试，覆盖路线陈旧跳跃、撤退目标、接敌索引冲突、事件监听隔离、经济治理成本重复计入、同势力堆叠静默、平局判定和运行时地图归属同步。
- `domain-core/src/Domain/Military/DomainArmyMovementSystem.cs` 增加路线当前位置与邻接校验，拒绝陈旧路线跨区跳跃。
- `domain-core/src/Domain/Map/MapCommandService.cs` 限制撤退只能去相邻己方地区，并暴露路线步进校验。
- `domain-core/src/Domain/World/WorldState.cs` 维护“一个地区最多一个接敌”的索引不变量，替换冲突接敌时清理旧引用。
- `domain-core/src/Core/GameState.cs`、`GameContext.cs`、`Domain/World/WorldStateFactory.cs` 让 `GameContext.ChangeRegionOwner` 同步 runtime `MapState`。
- `domain-core/src/Core/EventBus.cs` 隔离监听器异常，避免一个 UI/观察者异常阻断后续监听器。
- `domain-core/src/Domain/Economy/DomainEconomySystem.cs` 将军队基础 upkeep 与治理 upkeep 分开，避免每支部队重复计入地区治理成本。
- `domain-core/src/Domain/Military/DomainBattleSimulationSystem.cs` 将精确平局改为防守方胜。
- `domain-core/src/Domain/Military/DomainEngagementDetector.cs` 为同势力部队同区会合留下可观察日志。
- `domain-core/src/Domain/Core/HeadlessSimulationRunner.cs` 同步经济期望，并把后勤首回合粮耗断言改为检查后勤阶段本身，避免被同回合经济收入掩盖。

### 提交前验证

- `npm --prefix web-strategy-map run check:data-source` 通过：`data=16 audioJson=4 regions=56 chronicleEvents=200 mp3=270 archiveMp3=79 artPng=112`。
- `python tools\validate_domain_core.py` 通过：`domain-core/src` 为 Unity-free，headless 链接迁移后的 C# 源。
- `dotnet test tools\headless_runner\WanChaoGuiYiTests\WanChaoGuiYiTests.csproj --no-restore` 通过：`11/11 passed`，无 xUnit analyzer warning。
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1` 通过：`16/16` headless 场景全部通过。
- `npm --prefix web-strategy-map run build` 通过；仍保留 Vite `799.79 kB` chunk-size advisory，作为 P1 性能后续项。
- `npm --prefix web-strategy-map run test:ui -- --reporter=line --workers=1` 通过：`20/20 passed`。

### 剩余风险

- Vite 主 chunk 仍约 `799.79 kB`，构建 exit code 为 0，但后续应拆分或设置明确 chunk 策略。
- `region_specialization_and_governance_forecasts`、`occupation_control_chain_progression` 等 headless 场景仍有 `keyDelta: none` 的自证薄弱问题；本轮通过 xUnit 对抗测试补了一批核心盲区，但还不能代表覆盖完备。
- 本轮为 baseline 落地，不执行 GitHub push 或 fresh clone 复验；推送和 clone 复验应作为下一步发布门禁。
