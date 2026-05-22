# tools 目录脚本审查报告

审查范围：`E:\万朝归一\万朝归一\tools\` 及子目录
审查时间：2026-05-17
审查文件：7 个（3 个 PowerShell、4 个 Python）

---

## 1. validate_web_data_source.py（785 行）

### 1.1 优点

- 相对路径从 `__file__` 推导，无硬编码绝对路径 ✅
- 完善的 `fail()` 函数终止验证并给出精确位置 ✅
- PNG 文件头魔数检测（防伪造文件）✅
- 双向邻居一致性检查（防止单向道路）✅
- `sourceReference` 有效性质检（防占位符）✅
- `require_collection` 确保 items 数组存在 ✅
- 多处数量门限检查（至少 40 区域、200 事件、8 帝皇等）✅

### 1.2 问题

#### P1 — 缺少 schemaVersion 版本门控

**问题**：`map_render_metadata.json` 写入了 `schemaVersion: 1`，但验证器从未检查版本号。所有数据 JSON（`emperors.json`、`regions.json` 等）也没有 schemaVersion 字段。当数据结构演进时，验证器和数据可能版本不同步。

**修复建议**：
```python
# 在 validate_map_render_metadata() 中加入
schema_version = metadata.get("schemaVersion", 0)
if schema_version < 1:
    fail("map_render_metadata.json schemaVersion must be >= 1")
```
建议对所有顶层 JSON 文件也要求 `schemaVersion` 字段。

#### P2 — `validate_art_path_references()` 条件表达式易误读（已回收）

**原审查问题**：第 768 行：
```python
if not asset_path.startswith("art/Portraits/") or not asset_path.endswith(".png"):
    fail(...)
```
复核结论：该表达式按布尔代数等价于 `not (startswith and endswith)`，不会拒绝合法 `art/Portraits/*.png` 路径。但为了避免继续被误读，已改成显式括号形式。

**已采用写法**：
```python
if not (asset_path.startswith("art/Portraits/") and asset_path.endswith(".png")):
    fail(...)
```
`generals.json` 的 `portraitAssetPath` 检查也已同步改为显式括号形式。

#### P3 — 缺少 JSON 格式精度验证

**问题**：验证了 JSON 是否可解析，但没有检查浮点数精度、坐标范围合理性等。`map_region_shapes.json` 的坐标可能超出地图边界但仍通过验证。

**修复建议**：在 `validate_regions_and_shapes()` 中对所有边界点做坐标范围合理性检查：
```python
for point in shape["boundary"]:
    if not (-20 <= point["x"] <= 20 and -20 <= point["y"] <= 20):
        fail(f"{shape['id']} boundary point out of map range")
```

#### P4 — `validate_route_networks()` 对空节点数组缺少处理

**问题**：第 534-536 行假设 `nodes` 是非空列表，但若 JSON 中 `nodes` 为 `[]`，会在第 537 行 `len(nodes) != len(set(nodes))` 通过（空集等于空集），随后在第 543 行 `zip(nodes, nodes[1:])` 返回空迭代，循环不执行，导致网络没有任何边时静默通过。

**修复建议**：
```python
nodes = network.get("nodes", [])
if not isinstance(nodes, list):
    fail(f"route network {network_id} nodes must be an array")
if len(nodes) < 2:
    fail(f"route network {network_id} needs at least two nodes, got {len(nodes)}")
```

#### P5 — `derive_strategy_specialization()` 依赖硬编码 ID 前缀

**问题**：`src/tools/validate_web_data_source.py:690-704` 的策略推导逻辑基于 region ID 字符串前缀（如 `"guanzhong"`、`"jiangnan"`），但这与 `regions.json` 中的实际 ID 耦合。若 ID 命名规范调整，验证器会错误推导 specialization。

**影响**：中等——当前只影响警告输出（`derived_specializations` 计数不足时 `fail`），不会阻断主流程。但此逻辑应该依赖数据而非代码中的 ID 列表。

**修复建议**：将硬编码 ID 列表移入配置常量或从 `regions.json` 的 `regionSpecialization` 字段直接读取。

#### P6 — `validate_portraits()` 肖像数量下限未检查

**问题**：只检查了 `portraits.json` 不为空，但没有检查每位皇帝是否至少有一张肖像。第 335 行对缺失肖像的皇帝只打印警告而非 fail。若部分皇帝缺少肖像，游戏端会出问题但验证通过。

**修复建议**：
```python
if missing_portraits:
    fail(f"missing portraits for emperors: {sorted(missing_portraits)}")
```

#### P7 — `validate_heavy_strategy_contract_docs()` 硬编码文档路径

**问题**：第 575 行硬编码了 `ROOT / "docs" / "data-contract.md"`。若文档重命名或移动，验证器会失败但不给出有用信息。

**修复建议**：将路径提取为模块级常量或通过环境变量/配置文件指定。

#### P8 — `validate_runtime_icon_assets()` 未检查图标内容有效性

**问题**：只检查 PNG 文件是否存在，未检查文件头魔数（与第 105 行对 `jiuzhou_generated_map.png` 的处理方式不一致）。可能存在 0 字节或损坏的 PNG。

**修复建议**：
```python
png_magic = path.read_bytes()[:8]
if png_magic != b"\x89PNG\r\n\x1a\n":
    fail(f"{path.relative_to(ROOT)} is not a PNG file")
```

#### P9 — `compare_synced_tree()` 只检查前 5 个差异文件

**问题**：第 199、201 行对 missing 和 extra 只打印前 5 个，但 fail 时可能遗漏其他文件，导致部分差异未被发现。

**修复建议**：改为检查总数超限：
```python
if len(missing) > 0:
    fail(f"public/game-data missing synced files: {len(missing)} total, e.g. {missing[:5]}")
```

#### P10 — 无网络超时处理（外部文件访问）

**问题**：若通过 HTTP 获取任何数据，没有超时设置。但当前 `validate_web_data_source.py` 是纯本地文件检查，无此风险。

---

### 1.3 验证覆盖缺口

| 检查项 | 是否覆盖 | 备注 |
|--------|---------|------|
| JSON schemaVersion | ❌ | 缺失 |
| PNG 内容魔数（art/*.png） | ❌ | 只检查了 jiuzhou_generated_map.png |
| 皇帝肖像完整性 | ⚠️ | 只警告，不断言 |
| 空 nodes 数组 | ❌ | 静默通过 |
| sourceImage 值合理性 | ❌ | 只检查不等于默认值，未验证实际文件存在 |
| 坐标范围合理性 | ❌ | 无边界检查 |
| package.json scripts | ✅ | 完整覆盖 |
| Unity 残留 token | ✅ | 完整覆盖 |

---

## 2. verify_headless_war.ps1（183 行）

### 2.1 优点

- 清晰的参数解析（数据目录、玩家派系 ID）✅
- Python 环境检测（尝试 `python` 和 `python3`）✅
- 依赖链式调用（validate → headless → report 解析）✅
- 完整的场景断言映射（16 个场景 × 断言 ID 检查）✅
- 严格的数值门控（scenarioCount >= 16、failedCount == 0）✅

### 2.2 问题

#### P1 — 对 report JSON 结构缺少类型和完整性校验

**问题**：第 35-39 行只检查了 `runName`、`passed`、`scenarioCount`、`failedCount` 四个字段，但没有检查：
- `passedCount` 字段是否存在
- `scenarios` 是否是数组
- 每个 scenario 是否有 `name` 字段
- 每个 scenario 的 `assertions` 是否是数组

若 JSON 结构不完整（如缺少 `passedCount`），PowerShell 的 `ConvertFrom-Json` 会静默接受（字段为 `$null`），导致第 39 行的 `$Report.passedCount -ne $Report.scenarioCount` 执行 `$null -ne $null` 为 `$false`，误判为通过。

**修复建议**：
```powershell
if (-not $Report.passedCount) { Write-Error "report missing passedCount" }
if ($Report.passedCount -ne $Report.scenarioCount) {
    Write-Error "passedCount mismatch: $Report.passedCount vs $Report.scenarioCount"
}
```

#### P2 — `scenarioCount -lt 16` 应为 `-le`

**问题**：第 37 行检查 `scenarioCount -lt 16`。若恰好有 16 个场景（满足 MVP 要求），`15 -lt 16` 为 True，会误判为不足。当前断言映射有 16 个 scenario，但若有新增场景，阈值应为 `<`（严格小于），而非 `<=`。

**说明**：若目标是"至少 16 个"，则 `-lt 16` 是正确的（16 个时 16 < 16 = False，通过）。但若注释说"Expected at least 16 scenarios"，语义上应写 `-lt 16`（≥16 通过）。当前写法语义正确，只是注释歧义。

#### P3 — 无 report 文件存在性前置检查

**问题**：第 33 行直接读取 `$ReportPath` 内容，若文件不存在或为空，`Get-Content -Raw` 会抛出异常。但这是预期行为（脚本会终止）。不过若 JSON 解析失败（第 33 行），`ConvertFrom-Json` 报错不够明确。

**修复建议**：
```powershell
if (-not (Test-Path $ReportPath)) {
    Write-Error "Headless war report not found: $ReportPath"
}
```

#### P4 — 断言检查顺序问题

**问题**：第 169-182 行先检查 scenario 是否存在，再检查断言列表中每个断言是否通过。若 scenario 名称不匹配但断言 ID 存在，会产生误导性错误信息。

**修复建议**：改为先打印 `Missing scenario` 错误，再检查断言。

#### P5 — 无法区分"断言缺失"和"断言失败"

**问题**：第 180 行同时检查断言是否存在且通过。若断言列表中存在但 `passed != $true`，第 175 行的检查会先触发错误，第 180 行不会执行，导致用户不知道是"缺失"还是"失败"。

**当前逻辑**：第 175 行会在第 180 行之前触发，但错误信息只说"Failed assertion"，不能说清楚是 scenario 缺失还是断言 ID 缺失。需要调整顺序。

**修复建议**：调整检查顺序，先检查 scenario 存在性，再检查断言存在性，最后检查断言通过性。

---

### 2.3 验证覆盖缺口

| 检查项 | 是否覆盖 | 备注 |
|--------|---------|------|
| runName 合法性 | ✅ | 但只检查值，不验证与 data dir 一致性 |
| passedCount 与 scenarioCount 一致性 | ✅ | |
| 每个 scenario 有 name | ❌ | 只检查 scenario 存在，不检查 name 非空 |
| 每个 assertion 有 id | ❌ | 直接读取 id，不验证非空字符串 |
| JSON 结构类型（数组 vs 对象） | ❌ | |

---

## 3. run_all_checks.ps1（57 行）

### 3.1 优点

- 依赖顺序清晰（数据验证 → C# 测试 → war → Web → Playwright）✅
- `Step` 函数封装了计时和错误处理 ✅
- Playwright 可选跳过（`-SkipPlaywright`）✅

### 3.2 问题

#### P1 — `Step` 函数的 `$cwd` 参数未使用

**问题**：第 17-34 行定义 `Step($name, $command, $cwd)`，但 `$cwd` 传入后只在第 19 行 `Push-Location $cwd` 使用。若命令是绝对路径（如 `python tools\...`），`Push-Location` 不会改变工作目录，但命令仍会执行。这在实践中没问题（PowerShell 会从系统 PATH 找到 python），但语义不清。

**修复建议**：若 $command 是字符串，PowerShell 会在当前目录执行而非 `$cwd`。需要改成：
```powershell
& $command
```
或者改用 `$ExecutionContext.InvokeCommand.CommandNotFoundAction` 处理。

**实际风险**：低。当前命令都是 `python tools\...` 或 `npm run ...`，PowerShell 会从 PATH 解析 python/npm，工作目录不影响。但如果系统有多个 Python 版本，在 `$cwd` 中执行可能找不到正确的 python。

#### P2 — xunit 测试数量硬编码（"70+ tests"）

**问题**：第 42 行注释说"70+ tests"，但实际测试数量可能变化。若测试数少于 70，脚本仍会通过，没有数量门控。

**修复建议**：移除硬编码数量注释，或改为检查 test run 是否成功（xunit 本身会在失败时返回非零 exit code）。

#### P3 — 缺少 .NET SDK 检测前置

**问题**：第 42 行调用 `dotnet test`，但 `run_all_checks.ps1` 没有在调用前检测 .NET SDK 是否存在。若 .NET 未安装，`dotnet test` 的错误信息不够友好。

**修复建议**：在 PowerShell 脚本开头添加 .NET SDK 检测。

---

## 4. run_headless_simulation.ps1（31 行）

### 4.1 优点

- 清晰的参数解析 ✅
- 多路径 .NET 检测（Source、Program Files x86）✅

### 4.2 问题

#### P1 — 硬编码 .NET 路径（x86 分支）

**问题**：第 17-18 行硬编码了 `C:/Program Files (x86)/dotnet/dotnet.exe`。这是 32 位路径，现代 Windows 系统通常是 64 位，.NET 安装在 `C:/Program Files/dotnet/`。虽然 x86 分支作为 fallback 可以工作，但不够准确（64 位 Windows 没有 Program Files (x86) 下的 dotnet）。

**修复建议**：移除 x86 分支，或调整优先级（先检测 x64，再检测 x86）。

#### P2 — `runtimes` 变量解析不可靠

**问题**：第 23 行 `& $DotnetBin --list-runtimes` 返回字符串数组，但正则匹配 `"Microsoft\.NETCore\.App 8\."` 依赖输出格式。若 .NET 团队更改输出格式（虽然不太可能），正则会失效。

**修复建议**：使用 `dotnet --list-runtimes | Select-String "Microsoft\.NETCore\.App 8\."` 或解析 JSON 输出（`dotnet --list-runtimes --json`，但这需要 .NET 8+）。

#### P3 — 无数据目录存在性检查

**问题**：第 30 行将 `$DataDir` 传给 headless project，但未检查该目录是否存在。headless C# 程序会报错，但错误信息可能不够友好。

**修复建议**：
```powershell
if (-not (Test-Path $DataDir)) {
    Write-Error "Data directory not found: $DataDir"
}
```

---

## 5. validate_domain_core.py（62 行）

### 5.1 优点

- 简洁清晰 ✅
- 硬编码 Unity token 列表 ✅

### 5.2 问题

#### P1 — `FORBIDDEN_DOMAIN_TOKENS` 列表不完整

**问题**：第 10-19 行列出了 9 个禁止 token，但遗漏了：
- `Transform.`（非 UnityEngine 的变换 API）
- `[SerializeField]`（属性）
- `Application.`（Unity 应用类）
- `Resources.`（Unity 资源加载）
- `Time.`（Unity 时间类）

**修复建议**：
```python
FORBIDDEN_DOMAIN_TOKENS = [
    "using UnityEngine",
    "MonoBehaviour",
    "SerializeField",
    "GetComponent",
    "gameObject",
    "Mathf.",
    "MapGraph ",
    "MapGraph)",
    "Application.",
    "Resources.",
    "Time.",
    "Transform.",
]
```

#### P2 — `MapGraph ` 和 `MapGraph)` 检测不够精确

**问题**：这两个 token 可能出现在注释、字符串字面量或测试代码中，导致误报。例如 `// MapGraph is a placeholder` 会被标记。

**修复建议**：改为检测 namespace 声明或文件名，确保只检查源代码文件中的实际代码。

#### P3 — 缺少 schemaVersion 或版本标记

**问题**：该脚本没有版本标记或 schemaVersion 检查。当禁止 token 列表需要更新时，无法追踪哪些数据文件已用新验证器验证过。

---

## 6. render_jiuzhou_map.py（340 行）

### 6.1 优点

- 生成地图 PNG 并写入 metadata JSON（自动化完整）✅
- Voronoi 细分 + 不规则 fallback 逻辑合理 ✅
- 字体回退机制（msyh → simhei → simsun → default）✅

### 6.2 问题

#### P1 — 直接修改源数据文件

**问题**：第 86 行 `save_json(DATA_DIR / "map_region_shapes.json", shapes)` 直接覆盖源数据目录的文件。Voronoi 细分是有损操作，若数据有问题会直接破坏原始数据。

**修复建议**：修改前先备份原始文件，或输出到 `.outputs/` 目录，由用户手动确认后覆盖。

#### P2 — 硬编码地图参数（SHAPE_CENTER、PIXELS_PER_SHAPE_UNIT）

**问题**：第 19-22 行硬编码了 `SHAPE_CENTER = (-3.65, 0.05)` 和像素转换参数。这些值与 `map_render_metadata.json` 中的值应保持一致，但脚本不验证一致性。

**修复建议**：从 `map_render_metadata.json` 读取参数（而非写入），或验证写入后的 metadata 与脚本参数一致。

#### P3 — `write_metadata()` 不检查现有文件

**问题**：第 277 行直接覆盖 `map_render_metadata.json`。若已有版本更高的 metadata，直接覆盖会丢失版本信息。

**修复建议**：
```python
if METADATA_PATH.exists():
    existing = load_json(METADATA_PATH)
    if existing.get("schemaVersion", 0) >= metadata["schemaVersion"]:
        print(f"Warning: metadata already exists with version {existing['schemaVersion']}")
```

#### P4 — 字体加载失败时静默使用 default

**问题**：`load_font()` 在所有候选字体都不存在时返回 `ImageFont.load_default()`，不给出警告。生产环境中若字体缺失，生成的图片标签可能显示为方块。

**修复建议**：若最终使用了 `load_default()`，打印警告。

#### P5 — 硬编码河流坐标

**问题**：第 258-259 行硬编码了两条河流的坐标点。这些坐标与 `regions.json` 或 `map_region_shapes.json` 中的地理数据没有关联。若区域数据更新，河流可能与实际地形不匹配。

**修复建议**：从 `route_networks.json` 读取河流路线，自动绘制。

---

## 7. render_jiuzhou_isometric_preview.py（348 行）

### 7.1 优点

- 等角透视渲染，视觉质量高 ✅
- 自动计算边界和缩放 ✅
- 路线图 BFS 搜索 ✅

### 7.2 问题

#### P1 — 字体加载逻辑与 `render_jiuzhou_map.py` 重复

**问题**：`load_font()` 函数（第 335-344 行）与 `render_jiuzhou_map.py` 中的实现几乎完全相同，应抽取为共享模块。

#### P2 — `find_route()` 未处理无路径情况

**问题**：第 300-324 行 BFS 搜索路线。若起点和终点不连通，会返回空列表，调用方只检查 `len(route) < 2` 后 `continue`，静默跳过无连通路径的场景。这在地图数据不完整时可能掩盖问题。

**修复建议**：若 graph 中的节点不连通，打印警告而非静默跳过。

#### P3 — 硬编码 ROUTES 列表

**问题**：第 58-62 行硬编码了三条路线。这与 `route_networks.json` 中的数据不同步。

**修复建议**：从 `route_networks.json` 读取关键路线。

#### P4 — 字体回退机制无警告

**问题**：与 `render_jiuzhou_map.py` 相同，中文字体缺失时静默使用 default，图片标签可能乱码。

---

## 8. 综合发现

### 8.1 版本管理缺失

所有脚本均无 schemaVersion 或版本检查：
- `validate_web_data_source.py`：不检查数据文件的 schemaVersion
- `validate_domain_core.py`：无版本标记
- `render_jiuzhou_map.py`：写入 metadata 时不检查版本
- PowerShell 脚本：同样无版本标记

### 8.2 误报风险

| 脚本 | 误报场景 | 风险等级 |
|------|---------|---------|
| validate_web_data_source.py | `validate_art_path_references()` 的 `or` 逻辑错误 | 高 |
| validate_web_data_source.py | `FORBIDDEN_DOMAIN_TOKENS` 可能在注释中匹配 | 低 |
| verify_headless_war.ps1 | JSON 字段缺失时误判为通过（第 39 行） | 中 |
| render_jiuzhou_isometric_preview.py | 路线不连通时静默跳过 | 低 |

### 8.3 修复优先级

1. **P1（高优先级）**：`validate_web_data_source.py` 第 768、775 行 `or` 改为 `and`
2. **P1（高优先级）**：`verify_headless_war.ps1` JSON 结构完整性检查
3. **P2（中优先级）**：所有脚本加入 schemaVersion 检查
4. **P2（中优先级）**：地图渲染脚本从 JSON 读取配置而非硬编码
5. **P3（低优先级）**：字体回退警告、硬编码路径清理

---

## 9. 脚本间依赖关系

```
run_all_checks.ps1
  ├── validate_domain_core.py
  ├── validate_web_data_source.py
  ├── dotnet test (C# xunit)
  ├── verify_headless_war.ps1
  │     ├── validate_web_data_source.py
  │     ├── validate_domain_core.py
  │     └── run_headless_simulation.ps1
  │           └── dotnet run (headless)
  └── npm (Web TypeScript / vitest / build)

render_jiuzhou_map.py ←写入→ map_render_metadata.json ←验证→ validate_web_data_source.py
```

---

## 10. 建议改进

1. **抽取共享模块**：创建 `tools/lib/` 目录，放置 `load_json()`、`load_font()`、`fail()` 等共享函数，避免重复代码。

2. **统一版本标记**：所有 JSON 数据文件和脚本都应有 `schemaVersion` 字段，验证器检查版本兼容性。

3. **强化错误信息**：所有 `fail()` 调用应包含相对路径和字段名，便于快速定位问题。

4. **添加 dry-run 模式**：地图生成脚本应支持 `--dry-run` 输出到临时目录，由用户确认后覆盖源文件。

5. **CI 集成**：确保 `run_all_checks.ps1` 在每次 PR 中执行，报告应包含各步骤耗时。
