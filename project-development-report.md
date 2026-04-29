# 《万朝归一：九州帝业》Project Development Report

更新日期：2026-04-29

## 当前状态

- 阶段：MVP 内容扩展完成，准备进入 Unity 编译验证。
- 当前目标：所有不依赖 Unity 编译器的工作已完成，等待 Unity 环境编译验证。
- 技术方向：Unity + C#，2D 节点地图 + 区域面片，UGUI，JSON 数据表。
- 版本策略：国内版 MVP 先行，全球版只保留数据结构接口。

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
