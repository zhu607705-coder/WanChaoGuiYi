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
