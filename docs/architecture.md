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
| 地图布局 | `MapSetup`, `CameraController` | 自动摆放区域节点、摄像机平移缩放 |
| UI | `MainMapUI`, panels | 地图、地区、帝皇、朝廷、事件、战报 |

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
