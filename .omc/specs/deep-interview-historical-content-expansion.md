# Deep Interview Spec: 历史内容深度扩展

## Metadata
- Rounds: 6
- Final Ambiguity Score: 20%
- Type: brownfield
- Generated: 2026-04-28
- Threshold: 20%
- Status: PASSED

## Clarity Breakdown
| 维度 | 分数 | 权重 | 加权 |
|------|------|------|------|
| 目标清晰度 | 0.90 | 35% | 0.315 |
| 约束清晰度 | 0.75 | 25% | 0.188 |
| 成功标准 | 0.70 | 25% | 0.175 |
| 上下文 | 0.85 | 15% | 0.128 |
| **总清晰度** | | | **0.806** |
| **模糊度** | | | **19.4%** |

## 目标

在现有 8 位帝皇、14 项科技、11 个编年事件的基础上，扩展为 **13 位帝皇**的完整阵容，并为科技树和事件系统补充**严格史料引用**的深度内容。所有新增内容必须有可验证的二十四史或其他权威史料出处。

## 核心扩展内容

### 1. 新增 5 位帝皇

| 帝皇 | 评分 | 朝代 | 核心机制方向 |
|------|------|------|-------------|
| 隋文帝杨坚 | 8.24 | 隋 | 开皇制度、户籍财政、官僚精简 |
| 后周世宗柴荣 | 8.90 | 后周 | 短期高强度改革、统一临界点 |
| 北魏孝文帝元宏 | 8.18 | 北魏 | 汉化改革、迁都、文明建设 |
| 后赵明帝石勒 | 8.01 | 后赵 | 底层崛起、北方秩序重建 |
| 汉昭烈帝刘备 | 7.80 | 蜀汉 | 弱势建国、德与民心凝聚 |

每位帝皇需要：
- 完整的 `EmperorDefinition` JSON 数据
- 独特机制实现（`IEmperorMechanic`）
- 评分公式 12 维度打分（德、智、体、美、劳、雄心、尊严、气量、欲望自控、人事管理、国力、民心）
- 对应断代史引用

### 2. 科技树扩展

基于真实中国科技发展路线，扩展 technologies.json：
- 为现有 14 项科技补充史料引用
- 新增帝皇时代对应的科技节点
- 参考来源：《天工开物》《梦溪笔谈》《齐民要术》《考工记》

### 3. 事件系统扩展

基于二十四史天文志、五行志、灾异志，扩展 chronicle_events.json：
- 日食、月食、彗星、客星等天文事件，引用具体史书记载
- 洪涝、旱灾、蝗灾、瘟疫等灾异事件
- 新增帝皇在位期间的重大历史事件

### 4. 帝皇评分公式数据化

将评分公式集成到 EmperorDefinition：
```
综合得分 = 德×12% + 智×8% + 体×4% + 美×4% + 劳×5% + 雄心×5% + 尊严×8% + 气量×5% + 欲望自控×6% + 人事管理×10% + 国力×15% + 民心×18%
```

## 约束

- 所有新增内容必须有**严格史料引用**（书名、篇目）
- 史料范围：8 位帝皇对应断代史 + 先秦史料 + 编年体史书 + 古代科技专著 + 天文灾异专志
- 参考渤海小吏的内容作为叙事和解读参考
- 新增帝皇不替换现有 8 位，而是在其基础上扩展
- 评分公式用于帝皇数据结构，驱动 AI 行为权重

## 非目标

- 不做全球帝王
- 不做全部中国皇帝（只做 MVP 范围内的 13 位）
- 不做完整历史年表
- 不做多人联机

## 成功标准

- [ ] 13 位帝皇全部有完整 JSON 数据和独特机制
- [ ] 每位帝皇有 12 维度评分
- [ ] 科技树扩展到 20+ 项，每项有史料引用
- [ ] 编年事件扩展到 20+ 个，每个有史料引用
- [ ] 所有新增内容通过 `python3 tools/validate_data.py` 验证
- [ ] 渤海小吏内容作为参考融入叙事

## 技术上下文

### 现有代码结构
- `WanChaoGuiYi/Assets/Data/emperors.json` — 8 位帝皇
- `WanChaoGuiYi/Assets/Data/technologies.json` — 14 项科技
- `WanChaoGuiYi/Assets/Data/chronicle_events.json` — 11 个事件
- `WanChaoGuiYi/Assets/Scripts/Emperor/EmperorMechanic.cs` — 帝皇机制注册表
- `WanChaoGuiYi/Assets/Scripts/Tech/TechSystem.cs` — 科技研究系统
- `WanChaoGuiYi/Assets/Scripts/Celestial/CelestialEventSystem.cs` — 天文事件系统
- `/Users/zhuhangcheng/Downloads/kz/渤海小吏` — 渤海小吏内容目录

### 数据模型已支持
- `EmperorDefinition` — 已有 stats、uniqueMechanic、preferredPolicies
- `TechnologyDefinition` — 已有 prerequisites、boost、unlocks、effects
- `ChronicleEventDefinition` — 已有 eraScope、requiredTechs、weatherTags、astronomyTags

## 本体（关键实体）

| 实体 | 类型 | 字段 | 关系 |
|------|------|------|------|
| Emperor | 核心域 | id, name, stats(12维), uniqueMechanic, era | has many Regions, has Tech preferences |
| Technology | 核心域 | id, name, era, cost, prerequisites, boost, unlocks | unlocks Equipment/Policy, requires Tech |
| ChronicleEvent | 核心域 | id, name, eventType, eraScope, triggerWeight, choices | triggers by Weather/Astronomy/Tech |
| HistoricalLayer | 支撑 | regionId, geographyTags, customTags, weaponTraditions | defines Region yields and customs |
| Region | 核心域 | id, terrain, population, foodOutput, customs | belongs to Faction, has HistoricalLayer |
| 二十四史 | 外部参考 | 书名, 篇目, 记载内容 | sources for Emperor/Event/Tech |
| 渤海小吏 | 外部参考 | 叙事风格, 解读方式 | narrative reference |
| 评分公式 | 计算规则 | 12维度, 权重 | evaluates Emperor |

## 访谈记录

### 第 1 轮
**Q:** 刚实现了 5 个历史深度系统，最想推进哪个方向？
**A:** 会让 Codex 用 GPT image 2 生成历史地图，现在以扩展内容深度为主。
**模糊度:** 68%

### 第 2 轮
**Q:** "扩展内容深度"具体指哪些？
**A:** 查询真实科技发展路线展现真实性与硬核性，参考二十四史，科技树扩展，事件系统扩展。
**模糊度:** 54%

### 第 3 轮
**Q:** "真实性与硬核性"具体到什么程度？
**A:** 严格史料引用。
**模糊度:** 38%

### 第 4 轮
**Q:** 史料范围是什么？
**A:** 覆盖 8 位帝皇对应的断代史，加上先秦史料、编年体史书、古代科技专著、天文灾异专志，以及渤海小吏内容作为参考。
**A（补充）:** 给出了帝皇评分公式和 Top10 排名，用于扩展帝皇阵容。
**模糊度:** 30%

### 第 5 轮
**Q:** MVP 阶段扩展到多少位帝皇？
**A:** 只加 3-4 位核心新帝皇。
**模糊度:** 25%

### 第 6 轮
**Q:** 选择新增的帝皇？
**A:** 隋文帝杨坚、后周世宗柴荣、北魏孝文帝元宏、后赵明帝石勒、汉昭烈帝刘备（共 5 位）。
**模糊度:** 20% — 阈值达标。
