# 文档与规范一致性审查报告

**审查时间**：2026-05-17
**审查范围**：`E:\万朝归一\万朝归一\`（docs/ + 根目录）
**审查者**：Mavis（基于实际文档读取）
**修复回收状态（2026-05-21）**：`AGENTS.md` 与 `CLAUDE.md` 的帝皇数量已同步为 13 位；`docs/architecture.md` 已移除不存在的 `state.ts` / `systems.ts` 引用并写明纯代码 Web + headless Domain Core 路线；`docs/data-contract.md` 已补齐 Emperor.score；`docs/mvp-design.md` 已补齐 13 位帝皇机制表；已回收条目的风险标题已改为“原始风险等级”，避免轻量扫描误判为活跃风险；两份根规则文件重复问题已通过同步维护提示降为低风险兼容策略。

---

## 1. CLAUDE.md / AGENTS.md 内容完全重复

### 问题
`CLAUDE.md`（根目录）和 `AGENTS.md`（根目录）内容**完全相同**，当前均为 61 行项目规则文本。

这是维护隐患：同一份内容出现在两个文件中，任何修改都需要同步两处，更容易产生不一致。

### 风险等级：🟢 低
两文件内容一致且标题下已有同步维护提示，当前作为兼容不同工具入口的低成本策略保留；残余风险是后续修改者仍可能忽略提示。

### 建议
- 删除其中一个文件，另一个通过软链接或导入引用
- 或在文件顶部注明"此文件与 CLAUDE.md/AGENTS.md 同步，请勿单独修改"

### 2026-05-21 复核补充

两文件当前仍完全一致，且均已同步到 13 位帝皇范围。已在两文件标题下加入同一句同步维护提示，保留双文件兼容入口，同时降低单边修改风险。

---

## 2. AGENTS.md 帝皇数量过时（写8位，实际13位）

### 问题
`AGENTS.md` 第 15 行：
```
- 8 位核心帝皇：秦始皇、刘邦、汉武帝、曹操、李世民、赵匡胤、朱元璋、康熙。
```

`project-development-report.md`（项目开发记录）明确：
```
| 帝皇 | 13 | 8 位 MVP + 5 位扩展 |
```

`emperors.json` 实际数据：**13 位帝皇**（8 位 MVP + 杨坚/柴荣/元宏/石勒/刘备 5 位区域帝皇）

### 影响
- 规则文档与实际数据不符
- 可能导致新增帝皇数据被误认为"MVP 范围外"
- `mvp-design.md` 中帝皇表格同样只列了 8 位

### 原始风险等级：🔴 高
MVP 范围定义错误会影响团队对开发范围的判断。

### 建议
```markdown
# 修正 AGENTS.md 第 14-16 行
- 8 位核心帝皇：秦始皇、刘邦、汉武帝、曹操、李世民、赵匡胤、朱元璋、康熙。
+ 13 位帝皇：8 位核心 MVP（秦始皇、刘邦、汉武帝、曹操、李世民、赵匡胤、朱元璋、康熙）+ 5 位区域帝皇（杨坚、柴荣、元宏、石勒、刘备）。
```

---

## 3. architecture.md 引用了不存在的模块文件

### 问题
`architecture.md` 第 31 行：
```text
- `web-strategy-map/src/state.ts` / `types.ts`：运行态数据与存档结构。
```

`web-strategy-map/src/` 下**不存在 `state.ts`**，只有 `types.ts`。状态管理通过 `data.ts` 的模块级变量 + UI 层管理。

`architecture.md` 第 32 行：
```text
- `web-strategy-map/src/systems.ts`：回合、治理、战争、事件、胜利等规则推进。
```

`web-strategy-map/src/` 下**不存在 `systems.ts`**。玩法逻辑在 `domain-core/src`（C#）和 UI 层事件处理中。

### 影响
文档引导的代码路径找不到，开发者会困惑。

### 原始风险等级：🟠 中
不影响游戏运行，但影响文档可信度。

### 建议
- `state.ts` → 确认是否应该存在，或将路径指向正确的状态管理位置
- `systems.ts` → 说明这是指 `domain-core/src` 的 C# 系统，Web 层通过 headless runner 调用

---

## 4. architecture.md vs AGENTS.md 技术栈描述

### 问题
`AGENTS.md` 明确：
```
- 当前主线：纯代码 Web + headless Domain Core，不再使用 Unity/Tuanjie 编辑器作为开发入口。
```

但 `architecture.md` 全文没有明确说明"Unity 已从技术栈中移除"。模块表中仍可能暗示旧的 Unity 架构。

### 影响
新加入的开发者看到 architecture.md 可能会以为项目还用 Unity。

### 原始风险等级：🟡 中

### 建议
在 architecture.md 开头或技术路线部分加一句：
```
当前技术路线：纯代码 Web（Vite + TypeScript + Three.js）+ headless C# Domain Core。Unity/Tuanjie 编辑器已不使用。
```

---

## 5. mvp-design.md 帝皇机制表格过时

### 问题
`mvp-design.md` 第 59-68 行帝皇机制表格只包含 8 位核心帝皇，缺少杨坚、柴荣、元宏、石勒、刘备 5 位区域帝皇的机制定义。

### 影响
这 5 位帝皇在 emperors.json 中存在但 MVP 设计文档未覆盖。

### 原始风险等级：🟡 中

### 建议
补充 5 位区域帝皇的机制描述，或注明"区域帝皇机制待定，优先实现 8 位核心帝皇"。

### 2026-05-21 复核补充

该缺口仍成立。`web-strategy-map/game-data-source/data/emperors.json` 中 5 位区域帝皇已有可直接同步到 MVP 表格的机制：

| 帝皇 | 独特机制 | 强项 | 代价 |
|------|----------|------|------|
| 杨坚 | 开皇改制 | 户籍、财政、官僚精简 | 猜忌心重，功臣和宗室压力累积 |
| 柴荣 | 十年开拓 | 短期改革、军事经济同步推进 | 寿命风险大，继承安排脆弱 |
| 元宏 | 汉化改革 | 文明建设、制度汉化 | 旧代人集团反弹，边镇隐患累积 |
| 石勒 | 底层崛起 | 军事扩张、杂胡整合 | 合法性基础弱，继承问题严重 |
| 刘备 | 以德聚人 | 民心、人才凝聚、弱势维持 | 国力增长慢，军事上限受限 |

### 状态
已回收。`mvp-design.md` 的“初始帝皇机制”表已补齐 13 位帝皇，并将成功标准改为“8 位核心帝皇优先形成明显差异，5 位区域帝皇保留可解释机制差异”。

---

## 6. data-contract.md Emperor.score 字段定义

### 澄清
reviewer 原报告声称 `data-contract.md` 缺少 Emperor.score 定义。经核实：
- `data-contract.md` 的 Emperor 节原本确实没有 Emperor.score 的 12 字段结构定义
- 但 `types.ts` 实际已有 `EmperorScore` 接口定义（reviewer verifier 确认）
- 问题是**文档与实际代码不同步**，不是代码缺失

### 影响
开发者参考 data-contract.md 无法了解 EmperorScore 的完整字段结构。

### 原始风险等级：🟡 中

### 状态
已回收。`data-contract.md` Emperor 节已补充 `score` 完整 12 字段结构和字段说明：
```json
"score": {
  "virtue": 45,
  "wisdom": 92,
  "physique": 70,
  "aesthetics": 68,
  "diligence": 96,
  "ambition": 100,
  "dignity": 98,
  "tolerance": 35,
  "selfControl": 86,
  "personnelManagement": 90,
  "nationalPower": 100,
  "popularSupport": 62
}
```

---

## 7. data-contract.md OccupationStatus 定义（reviewer 误报，已更正）

### 澄清
reviewer 原报告声称 `data-contract.md` 未提及 OccupationStatus。经核实：
- `data-contract.md` 第 311-315 行明确定义了 `OccupationStatus` 枚举
- reviewer 误报，无需修改

---

## 8. roadmap-12-weeks.md 与 project-development-report.md 状态对比

### 问题
`roadmap-12-weeks.md` 是周计划模板，内容为"目标"而非"现状"。
`project-development-report.md` 更新日期为 2026-04-30，记录了大量已完成模块。

两文件**不矛盾**，但 roadmap 没有反映当前实际进度状态。

### 风险等级：🟢 低
roadmap 是规划文档，不需要与实际进度同步更新。

---

## 9. project-development-report.md 自动化轮次顺序漂移

### 问题
2026-05-21 的 5 分钟交替自动化记录中，轮 20、21、22 被插入到轮 17 之前，报告尾部仍显示轮 19，和实际最近完成轮次不一致。

轮 23 preflight 时检索到的原始顺序为：
- `修补问题轮 22`
- `修补问题轮 20`
- `找缺口轮 21`
- `找缺口轮 17`
- `修补问题轮 18`
- `找缺口轮 19`

轮 23 已即时做文档归档小修，将轮 17-23 按 17 → 18 → 19 → 20 → 21 → 22 → 23 整理到报告尾部。

### 影响
自动化 preflight 依赖 `project-development-report.md` 的最近尾部内容判断上一轮状态。轮次顺序漂移会让下一轮误判最近验证、下一步建议和提交状态。

### 原始风险等级：🟢 低
当前不影响运行时代码，但会影响自动化连续推进的上下文可靠性。

### 状态
已回收。后续追加自动化轮次时必须使用唯一尾部锚点或脚本化追加，避免再次命中较早的 `### 提交判断` 段落。

---

## 10. audit-docs-consistency.md 总结风险分布仍含已回收项

### 问题
本报告正文顶部已经说明 `AGENTS.md` / `CLAUDE.md` 帝皇数量、`architecture.md` 不存在路径与技术栈描述、`data-contract.md Emperor.score`、`mvp-design.md` 13 位帝皇机制表均已修正或回收。

但“风险等级分布”仍将 `AGENTS.md 帝皇数量过时` 计入 🔴 高风险，将 `architecture.md 引用不存在文件` 和 `技术栈描述不清` 计入 🟠 中风险。正文状态与总结统计不一致。

### 影响
后续自动化或人工读者会把已经修正的文档问题继续当作最高优先级，导致修补顺序偏离真实剩余风险。

### 原始风险等级：🟢 低
不影响 runtime，但影响 docs 审查报告的优先级可信度。

### 建议
下一轮修补时统一重写总结表：只保留仍成立的活跃风险，把已修正项全部移入“已回收”统计，并同步“最高优先级修复”列表。

### 状态
已回收。本报告总结表已改为只保留仍成立的活跃风险；已修正项统一归入“已回收”，OccupationStatus 误报归入“已澄清”。

---

## 11. 已回收条目仍保留原始风险等级标题

### 问题
第 2、3、4、5、6、9、10 节已经在顶部状态或各节 `### 状态` 中标记为已回收，但正文仍保留原始 `### 风险等级：🔴/🟠/🟡/🟢` 标题。

### 影响
后续自动化如果用 `rg "风险等级"`、`rg "🔴|🟠|🟡"` 这类轻量扫描做 preflight，会把已回收条目的历史风险误判为当前活跃风险。

### 原始风险等级：🟢 低
不影响 runtime，也不影响总结表当前结论，但会降低审查文档对自动化轮次的可机器读取性。

### 建议
下一轮修补时将已回收条目的风险标题改为“原始风险等级”，或在每个已回收条目的风险标题附近补充状态标记，确保轻量检索能区分“历史风险”和“当前活跃风险”。

### 状态
已回收。第 2、3、4、5、6、9、10 节及本节的标题已改为 `### 原始风险等级`，当前 `^### 风险等级` 仅保留仍活跃的维护风险或低风险说明项。

---

## 总结

### 风险等级分布

| 等级 | 问题数 | 说明 |
|------|--------|------|
| 🔴 高 | 0 | 无 |
| 🟠 中 | 0 | 无 |
| 🟡 中 | 0 | 无 |
| 🟢 低 | 2 | CLAUDE.md/AGENTS.md 重复文件保留为兼容入口；roadmap 是计划模板，不要求同步当前实际进度 |
| 🟢 已回收 | 8 | AGENTS/CLAUDE 帝皇数量、architecture 路径、architecture 技术路线、data-contract Emperor.score、mvp-design 13 位帝皇机制、project-development-report 轮次顺序、本报告总结表、已回收条目风险标题 |
| 🟢 已澄清 | 1 | data-contract.md OccupationStatus 误报 |

### 最高优先级修复
1. **已缓解重复根规则文件风险**：`CLAUDE.md` / `AGENTS.md` 仍为重复根规则文件，保留是为了兼容不同工具入口；标题下已加入同步维护提示，后续修改仍需同步两处。
2. **已回收历史风险标题误判**：已回收条目的 `风险等级` 标题已改为历史/原始风险标记，避免自动化误判活跃风险。
3. **已修正 AGENTS.md / CLAUDE.md 帝皇数量**：8 → 13
4. **已修正 architecture.md**：移除不存在的 `state.ts`/`systems.ts` 引用，更新技术栈说明
5. **已整理 project-development-report.md 轮次顺序**：后续追加需保持自动化最近记录位于报告尾部
6. **已补齐 mvp-design.md 帝皇机制表**：13 位帝皇均有机制、强项和代价描述
7. **已补齐 data-contract.md Emperor.score**：文档契约与 TypeScript 字段对齐

---

*审查完成日期：2026-05-17 | 已交叉验证所有文档与实际代码*
