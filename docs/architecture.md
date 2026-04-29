# Architecture

## 目标

项目大框架先解决三个问题：

1. 数据能被稳定读取和校验。
2. 回合推进能调度每个玩法系统。
3. 地图、帝皇、经济、政治、军事、人才、AI、UI 有清晰接入点。

## 分层

```text
Data JSON
  ↓
DataRepository
  ↓
GameState + GameContext + EventBus
  ↓
TurnManager
  ↓
IGameSystem modules
  ↓
UI panels / Map interaction
```

## 核心入口

- `GameManager`：游戏启动、读数据、创建新局、组装系统。
- `DataRepository`：读取 JSON 表，建立 id 索引，校验区域邻接。
- `GameState`：运行态数据，存档使用这个对象。
- `GameContext`：系统之间共享的最小上下文。
- `TurnManager`：按顺序执行所有 `IGameSystem`。
- `EventBus`：区域选择、政策、战斗、继承、胜利等事件通知。

## 系统模块

| 模块 | 入口 | 职责 |
| --- | --- | --- |
| 地图 | `MapGraph`, `RegionController`, `MapRenderer` | 邻接、点击、归属表现 |
| 帝皇 | `EmperorMechanicSystem` | 每位帝皇独特机制 |
| 经济 | `EconomySystem`, `TaxSystem`, `PopulationSystem`, `LandSystem` | 收入、人口、土地兼并 |
| 政治 | `LegitimacySystem`, `FactionSystem`, `ReformSystem`, `RebellionSystem` | 法统、派系、改革、民变 |
| 军事 | `ArmyMovementSystem`, `BattleResolver`, `SiegeSystem` | 行军、战斗、围城 |
| 继承 | `SuccessionSystem` | 继承风险和继位结算 |
| 人才 | `TalentSystem` | 人才获得和任命入口 |
| AI | `StrategicAI`, `PolicyAI`, `MilitaryAI` | 政策倾向和扩张目标 |
| 胜利 | `VictorySystem` | 每回合检查三种胜利条件 |
| 科技 | `TechSystem` | 研究点积累、科技解锁、Boost 启发 |
| 天气 | `WeatherSystem` | 季节天气生成、灾害概率、产出修正 |
| 风俗 | `CultureSystem` | 区域风俗推导、兵源/税收/民变修正 |
| 装备 | `EquipmentSystem` | 装备数据、科技解锁、战力加成 |
| 天文 | `CelestialEventSystem` | 天文事件触发、合法性修正 |
| 将领 | `GeneralSystem` | 将领数据、地形加成、兵种加成 |
| 建筑 | `BuildingSystem` | 区域建筑建造、效果应用、槽位管理 |
| 地图布局 | `MapSetup`, `CameraController` | 自动摆放区域节点、摄像机平移缩放 |
| UI | `MainMapUI`, panels | 地图、地区、帝皇、朝廷、事件、战报 |

## 新增数据层

| 表 | 用途 | 第一阶段接入方式 |
| --- | --- | --- |
| `portraits.json` | 帝皇立绘原画路径、生成 prompt、UI 裁切规则 | 帝皇面板读取 `emperorId` 对应头像 |
| `historical_layers.json` | 历史地理、风俗、兵器传统、战略资源、天气权重 | 地区面板显示摘要，事件系统读取权重 |
| `technologies.json` | 科技/制度树、前置、boost、解锁单位/政策/事件 | 回合系统后续接入研究进度 |
| `chronicle_events.json` | 时间、天气、天文、风俗、兵器突破事件 | 事件系统按地区标签、时代和科技筛选 |

## 地图颜色层

高精度地形图只作为底图，不直接染色。势力范围使用独立区域覆盖层：

1. 每个地区面片挂 `RegionController`、`MeshRenderer` 和 `MeshCollider`；旧节点 fallback 使用 `SpriteRenderer`。
2. `MapRenderer` 根据 `RegionState.ownerFactionId` 给覆盖层上色。
3. 归属变化必须通过 `GameContext.ChangeRegionOwner`，它会更新 `FactionState.regionIds`，再发布 `RegionOwnerChanged`。
4. `MapRenderer` 订阅 `RegionOwnerChanged`，只刷新变化的地区。

当前主地图使用地区面片：每个地区对象挂 `MeshRenderer`、`MeshCollider` 和 `RegionController`。旧圆形节点只作为显式开启的调试 fallback，不能在正式主地图中静默混用。

## 地图实际建模

原画只负责视觉方向，实际游戏地图必须拆成三层：

| 层级 | 资产/数据 | 作用 |
| --- | --- | --- |
| 地形底图 | 朝代地图原画 PNG | 山脉、河流、海岸、风格氛围 |
| 地区面片 | `map_region_shapes.json` | 每个地区的中心点、边界点、标签偏移、渲染顺序 |
| 交互状态 | `RegionController` + `RegionMeshBuilder` + `MapRenderer` | 点击、碰撞、势力染色、归属刷新 |

建模标准：

1. 每个 `regions.json` 地区必须有一个 `map_region_shapes.json` 面片。
2. 面片边界使用本地地图坐标，不写死到代码里。
3. 面片必须生成 `MeshRenderer`、`MeshCollider` 和 `RegionController`。
4. 地形底图不能承担点击和归属逻辑。
5. `MapSetup` 默认要求 56 个地区全部有可用面片；缺面片时记录错误，只有开启 `allowNodeFallback` 才生成旧节点。
6. 当前可用低精度面片先跑通交互，后续在 Unity 或矢量工具中精修边界点。

## 开发规则

- 新系统优先实现 `IGameSystem`，由 `TurnManager` 调度。
- 新数据字段先写 `docs/data-contract.md`，再改 JSON 和 C# model。
- 新事件通过 `EventBus` 通知 UI，避免系统直接操作 UI。
- 动态游戏状态放入 `GameState`，静态配置放入 `Assets/Data/*.json`。
- AI 先只做可解释倾向，等核心闭环稳定后再增加复杂决策。

## Week 1 最小可玩目标

1. 在 Unity 场景中创建一个 `GameManager` 对象。
2. 使用 `DataRepository` 自动读取 `Assets/Data/*.json`。
3. 手动放置 5 到 10 个 `RegionController` 节点。
4. 点击区域后通过 `MainMapUI` 和 `RegionPanel` 显示地区数据。
5. 点击下一回合后 `TurnManager` 跑完整个系统链。
