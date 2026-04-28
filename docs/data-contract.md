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
