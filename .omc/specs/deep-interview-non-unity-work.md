# Deep Interview Spec: 不依赖 Unity 编译器的工作

## Metadata
- Rounds: 4
- Final Ambiguity Score: 15%
- Type: brownfield
- Status: PASSED

## Clarity Breakdown
| 维度 | 分数 | 权重 | 加权 |
|------|------|------|------|
| 目标清晰度 | 0.92 | 35% | 0.322 |
| 约束清晰度 | 0.78 | 25% | 0.195 |
| 成功标准 | 0.80 | 25% | 0.200 |
| 上下文 | 0.90 | 15% | 0.135 |
| **总清晰度** | | | **0.852** |
| **模糊度** | | | **14.8%** |

## 目标

完成不依赖 Unity 编译器的五项工作，全部完成才算验收通过。

## 五项工作

### 1. 世界观设定文档
- 游戏整体世界观设定：时代跨度、帝皇选择逻辑、九州概念、法统体系
- 输出文件：`docs/world-building.md`

### 2. JSON 数据扩展
- 补全所有缺失数据（已基本完成，需检查完整性）
- 确保所有帝皇有 score 字段
- 确保所有区域有完整的 historical_layer
- 确保 generals.json 和 buildings.json 数据完整

### 3. 数据验证脚本增强
- `tools/validate_data.py` 增强：新增 generals、buildings 表的引用检查
- 新增 cross-table 引用验证（如 generals 的 terrainBonus key 是否是合法 terrain）
- 新增数据完整性报告

### 4. 文档更新
- `docs/data-contract.md` 更新：新增 General、Building 数据契约
- `docs/architecture.md` 更新：新增将领系统、建筑系统架构说明
- `project-development-report.md` 更新：记录最新进展

### 5. 地图边界精修
- `map_region_shapes.json` 边界点精修
- 确保 56 个区域的边界点数据合理
- 邻接区域的边界点应大致吻合

## 约束
- 不依赖 Unity 编译器
- 所有工作可通过 Python 脚本和 JSON 验证完成
- 五项全部完成才算验收通过

## 非目标
- 不修改 C# 代码逻辑
- 不添加新系统
- 不做 UI 实现
