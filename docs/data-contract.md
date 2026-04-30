# Data Contract

## 通用约定

- 所有表包含 `schemaVersion`。
- 所有稳定引用使用 `id`。
- 所有 `id` 使用 `snake_case`。
- 数值区间默认 0 到 100，除非字段说明另行定义。
- 全球版字段现在只预留，不驱动 MVP 玩法。

## Emperor

```json
{
  "id": "qin_shi_huang",
  "name": "秦始皇",
  "title": "始皇帝",
  "versionScope": ["china", "global"],
  "civilization": "huaxia",
  "mapScope": "china",
  "era": "classical",
  "legitimacyTypes": ["mandate", "law", "military"],
  "globalMechanicTag": "imperial_standardization",
  "stats": {
    "military": 88,
    "administration": 98,
    "reform": 100,
    "charisma": 76,
    "diplomacy": 52,
    "successionControl": 38
  },
  "uniqueMechanic": {
    "id": "liu_he_tong_gui",
    "name": "六合同轨",
    "description": "新征服区域更快完成制度统一，但徭役压力和继承风险增长更快。"
  },
  "historicalBurdens": ["民力透支", "继承脆弱", "地方反弹"],
  "preferredPolicies": ["jun_xian_system", "standardization", "strict_law", "grand_projects"],
  "aiPersonality": {
    "expansion": 85,
    "governance": 90,
    "riskTolerance": 72
  }
}
```

## Portrait

立绘表负责把每位帝皇的原画生成、资产路径和 UI 裁切规则固定下来。当前阶段允许 `sourceStatus` 标记为 `gpt_image_generation_requested`，但每位 MVP 帝皇必须有一条记录和可复用生成 prompt。

```json
{
  "id": "portrait_qin_shi_huang",
  "emperorId": "qin_shi_huang",
  "assetPath": "Assets/Art/Portraits/qin_shi_huang.png",
  "sourceStatus": "gpt_image_generation_requested",
  "version": "concept_v1",
  "visualIdentity": {
    "silhouette": "upright imperial posture",
    "costume": "black Qin imperial robe",
    "props": ["jade seal", "bamboo decree slips"],
    "palette": ["black", "deep red", "antique gold"],
    "backgroundMotif": "standardized roads",
    "expression": "cold, controlled"
  },
  "prompt": "Original game concept art portrait...",
  "uiCropHints": {"headCenterX": 0.5, "headCenterY": 0.31, "safeScale": 0.88}
}
```

## Region

```json
{
  "id": "guanzhong",
  "name": "关中",
  "terrain": "plain",
  "population": 800000,
  "foodOutput": 120,
  "taxOutput": 90,
  "manpower": 70,
  "landStructure": {
    "smallFarmers": 0.55,
    "localElites": 0.25,
    "stateLand": 0.15,
    "religiousLand": 0.05
  },
  "legitimacyMemory": ["mandate", "military"],
  "localPower": 45,
  "rebellionRisk": 12,
  "neighbors": ["hedong", "hanzhong", "longxi"],
  "eraProfile": {
    "classical": "imperial_core",
    "medieval": "frontier_gateway",
    "early_modern": "strategic_interior"
  }
}
```

## Historical Layer

历史层表挂在地区上，负责表达历史地理、风俗、兵器传统、气候、资源和事件权重。它借鉴《文明 6》的做法：地形、资源、气候和地方身份都要转成可解释的收益、风险、科技启发或事件权重。

```json
{
  "id": "history_guanzhong",
  "regionId": "guanzhong",
  "climateZone": "temperate_loess",
  "geographyTags": ["loess_plain", "capital_corridor"],
  "customTags": ["qin_han_ritual_core"],
  "weaponTraditions": ["crossbow", "heavy_infantry"],
  "strategicResources": ["grain", "bronze"],
  "seasonalProfile": {
    "spring": "irrigation_repair",
    "summerRisk": ["drought", "river_flood"],
    "autumnBonus": ["grain_harvest"],
    "winterRisk": ["cold_supply"]
  },
  "yieldModifiers": {"food": 8, "tax": 6, "manpower": 5, "mobility": 2, "legitimacy": 4},
  "eventWeights": {"flood": 8, "drought": 14, "storm": 3, "cold": 5, "astronomy": 8},
  "techAffinities": ["standard_script_law", "canal_granary"],
  "uiSummary": "关中适合中央集权、工程和标准化军备。"
}
```

## Map Region Shape

地图面片表是“实际建模”的基础。原画只做地形底图，地区点击、归属染色、边界和标签都由 `map_region_shapes.json` 驱动。

```json
{
  "id": "shape_guanzhong",
  "regionId": "guanzhong",
  "center": {"x": -5.5, "y": 1.1},
  "labelOffset": {"x": 0.0, "y": -0.35},
  "renderOrder": 10,
  "boundary": [
    {"x": -6.4, "y": 1.5},
    {"x": -5.4, "y": 1.9},
    {"x": -4.5, "y": 1.2},
    {"x": -4.9, "y": 0.4},
    {"x": -6.1, "y": 0.3}
  ]
}
```

规则：

- `regionId` 必须引用 `regions.json`。
- `center` 用于地图定位和自动布局。
- `boundary` 至少 3 个点，按顺时针或逆时针排列。
- `center` 必须落在 `boundary` 内，用于点击命中和默认聚焦验证。
- `labelOffset` 控制文字相对中心的位置，偏移后的标签点也必须落在 `boundary` 内。
- 后续精修地图时只改 JSON 边界点，不改渲染代码。

## Map-Led War Runtime Contract

本节定义地图主导战争闭环的运行时契约。原则：JSON 表提供初始事实和稳定配置，运行时状态记录会变化的战争事实；地图状态是军队位置、路线、接敌、战斗和占领的唯一权威入口。

### Persistent JSON Fields Used By Runtime

第一阶段不新增必须迁移的 JSON 表，直接复用现有字段：

- `regions.json`
  - `id`：区域稳定 id。
  - `terrain`：战斗地形和移动/补给修正输入。
  - `population`、`foodOutput`、`taxOutput`、`manpower`：占领后治理折损的基础值。
  - `localPower`、`rebellionRisk`、`landStructure`：占领后治理风险输入。
  - `neighbors`：`MapGraph` 路线和邻接查询的唯一数据来源。
- `map_region_shapes.json`
  - `regionId`、`center`、`boundary`：地图渲染、点击和行军路径显示的空间输入。
- `units.json`
  - `stats.attack`、`stats.defense`、`stats.mobility`、`stats.siege`：战斗结算和移动能力输入。
- `historical_layers.json`
  - `yieldModifiers.mobility`、`geographyTags`、`strategicResources`：后续补给、地形、围攻和增援规则输入。

### Runtime-Only State

这些字段默认不写回 JSON 数据表；未来存档系统需要序列化它们。

```json
{
  "world": {
    "turn": 1,
    "season": "Spring",
    "factions": ["faction_qin_shi_huang"],
    "regions": ["guanzhong"],
    "armies": ["army_qin_1"],
    "wars": ["war_qin_chu"],
    "logs": []
  },
  "map": {
    "regionRuntimeStates": {},
    "armiesByRegion": {},
    "engagementsByRegion": {}
  },
  "army": {
    "id": "army_qin_1",
    "ownerFactionId": "faction_qin_shi_huang",
    "locationRegionId": "guanzhong",
    "targetRegionId": "hedong",
    "route": ["guanzhong", "hedong"],
    "task": "Move",
    "soldiers": 1000,
    "morale": 70,
    "supply": 80,
    "movementPoints": 1,
    "engagementId": null
  }
}
```

### Runtime Enums

- `ArmyTask`
  - `Idle`：驻扎。
  - `Move`：沿地图路线行军。
  - `Attack`：向敌控区域推进，抵达后接敌。
  - `Retreat`：脱离接敌并向指定友方区域移动。
  - `Reinforce`：向已有接敌区域增援。
  - `Siege`：围攻区域，不直接等同于占领。
- `EngagementPhase`
  - `Forming`：接敌刚建立，允许合并多支部队。
  - `Resolving`：战斗正在结算。
  - `Resolved`：战斗已产生结果，等待占领/治理系统应用。
- `OccupationStatus`
  - `Controlled`：稳定控制。
  - `Contested`：有敌军或接敌。
  - `Occupied`：刚占领，治理惩罚生效。
  - `Rebelling`：民变或地方势力反弹。

### Runtime Query Requirements

`MapState` / `MapQueryService` 必须能回答：

- 指定区域的控制势力、占领状态、整合度、税粮折损和风险。
- 指定区域有哪些军队，按势力分组后有哪些友军/敌军。
- 指定军队当前位置、目标、路线、任务、是否接敌。
- 指定区域的邻接区域和从 A 到 B 的路线。
- 指定区域是否已有 engagement，多支军队是否应加入同一 engagement。

### Write Rules

- 军队位置、路线、任务只能通过 `MapCommandService` 或地图主导系统修改。
- 战斗系统只产出 `BattleResult`/`BattleRuntimeState`，不得直接修改区域归属。
- 区域控制权只能通过 `OccupationSystem` 修改。
- 占领后的整合度、税粮折损、民变风险、地方势力和兼并压力只能通过 `GovernanceImpactSystem` 应用。
- 每个关键变化必须发布事件并写入日志：行军开始、抵达、接敌、战斗结束、占领、治理后果。

### Governance Impact Contract

新占领区域默认使用以下运行时规则，后续可用政策/帝皇机制调整：

- `integration` 设置为低值或在原值基础上下降，首版默认 25。
- `taxOutput` 和 `foodOutput` 的实际贡献由运行时倍率折损，不直接覆盖 JSON 定义值。
- `rebellionRisk` 上升。
- `localPower` 或 `annexationPressure` 上升，用于表达地方势力和兼并压力。
- UI 必须显示“为什么该地区刚占领但贡献低、风险高”。

## Policy

政策必须声明成本、效果、风险和适配帝皇机制。

```json
{
  "id": "land_survey",
  "name": "清丈土地",
  "category": "domestic",
  "cost": {"money": 40, "legitimacy": 3},
  "effects": {"taxBase": 8, "annexationPressure": -10},
  "risks": {"eliteAnger": 12, "rebellionRisk": 4},
  "mechanicTags": ["land_control", "centralization"]
}
```

## Event

事件必须可追踪触发条件、选择、结果和冷却。

```json
{
  "id": "succession_dispute_minor",
  "name": "储位争议",
  "trigger": {
    "minSuccessionRisk": 50,
    "minCourtFactionPressure": 40
  },
  "choices": [
    {
      "id": "support_heir",
      "label": "支持太子",
      "effects": {"legitimacy": -5, "successionRisk": -12}
    }
  ],
  "cooldownTurns": 8
}
```

## Technology

科技和制度树统一放在 `technologies.json`。`kind` 可以是 `technology` 或 `civic`。设计上采用《文明 6》式的 boost/eureka：玩家完成具体行为后获得研究进度，科技再解锁单位、政策、事件和机制标签。

```json
{
  "id": "mounted_warfare",
  "name": "骑战马政",
  "kind": "technology",
  "era": "classical",
  "cost": 70,
  "prerequisites": ["bronze_casting"],
  "boost": {
    "id": "own_horse_region",
    "name": "掌控马源",
    "description": "控制带 horses 资源的边疆地区。",
    "progressBonus": 45
  },
  "unlocks": {
    "units": ["frontier_cavalry"],
    "policies": ["horse_policy", "frontier_campaign"],
    "events": ["border_horse_plague"],
    "mechanicTags": ["cavalry", "frontier"]
  },
  "effects": {"armyMorale": 4, "mobility": 5},
  "uiSummary": "让边疆马政变成扩张引擎，同时引入马疫和补给风险。"
}
```

## Chronicle Event

编年事件表负责时间、天气、天文、风俗、基础设施和兵器突破。它和旧 `events.json` 分工不同：旧表处理核心治理触发，新表处理跨地区、跨时代的世界状态事件。

```json
{
  "id": "solar_eclipse",
  "name": "日食示警",
  "eventType": "astronomy",
  "eraScope": ["classical", "medieval", "early_modern"],
  "turnWindow": {"startTurn": 1, "endTurn": 999},
  "regionScopeTags": ["ritual_center", "orthodox_memory"],
  "requiredTechs": [],
  "weatherTags": [],
  "astronomyTags": ["eclipse"],
  "triggerWeight": 6,
  "choices": [
    {
      "id": "issue_self_reproach",
      "label": "下诏自省",
      "effects": {"legitimacy": 5, "factionPressure": -2},
      "risks": {"successionRisk": 2},
      "followUpTags": ["mandate_repair"]
    }
  ],
  "cooldownTurns": 14,
  "uiSummary": "天文事件进入合法性系统。"
}
```

## General

将领表存储历史将领数据。每位将领有军事值、忠诚度、特殊技能、地形加成和兵种加成。将领通过人才系统授予势力，影响战斗结果。

```json
{
  "id": "guan_yu",
  "name": "关羽",
  "title": "武圣",
  "era": "classical",
  "military": 96,
  "loyalty": 95,
  "specialAbility": "water_attack",
  "specialAbilityName": "水攻",
  "specialAbilityDesc": "在河流区域战斗力+30%",
  "terrainBonus": {"river_plain": 30, "river_delta": 20},
  "unitBonus": {"infantry": 10, "cavalry": 5},
  "sourceReference": "《三国志·关羽传》"
}
```

规则：
- `military` 和 `loyalty` 范围 0-100。
- `terrainBonus` 的 key 必须是合法的 terrain 类型。
- `unitBonus` 的 key 必须是合法的 unit id。
- `sourceReference` 必填，引用正史列传。

## Building

建筑表存储区域建筑数据。建筑通过科技解锁，在区域中建造，提供产出加成。每个区域最多 3 个建筑。

```json
{
  "id": "granary",
  "name": "粮仓",
  "category": "agriculture",
  "requiresTech": "granary_system",
  "cost": 30,
  "effects": {"food": 15, "disasterMitigation": 3},
  "sourceReference": "《周礼·地官·廪人》"
}
```

规则：
- `requiresTech` 必须引用 `technologies.json` 中的 id。
- `cost` 为建造所需金钱。
- `effects` 使用 `EffectSet` 结构。
- `sourceReference` 必填，引用正史或典籍。
