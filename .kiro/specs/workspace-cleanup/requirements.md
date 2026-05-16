# Requirements Document

## Introduction

本特性目标是把整个工作树（包含外层 `e:\万朝归一\` 散落资产与内层主项目 `e:\万朝归一\万朝归一\`）整理成一份"可信、可重建、可验证"的工程化基线。

清理本身被当作一项工程任务，而不是一次性手工操作。它需要：

- 先盘点（Inventory）：枚举所有候选条目，标注归属、可重建性、磁盘占用、是否已被 `.gitignore` 覆盖。
- 再决策（Disposition）：每个条目落到三类处置之一——删除（Delete）、归档（Archive）、保留（Keep）。
- 后执行（Execute）：在执行前完成可逆备份，在执行后跑主线验证脚本，确认主项目没坏。
- 留痕（Record）：把决策与结果写回 `project-development-report.md`，并按需扩充 `.gitignore` 防止杂物再次产生。

特性范围只覆盖**工作树清理工程**本身（盘点、决策、执行、验证、留痕、防回归），不修改任何 MVP 玩法代码、不改动 `domain-core/src`、`web-strategy-map/src`、`web-strategy-map/game-data-source`、`docs/` 中的内容。

## Glossary

- **Outer_Root**：外层目录 `e:\万朝归一\`（非 git 仓库，散落 ML 资产与 `.omc` 代理元数据的位置）。
- **Inner_Project**：内层主项目目录 `e:\万朝归一\万朝归一\`（包含 `.git`、`AGENTS.md`、`project-development-report.md`，是受版本管理的真正项目仓库）。
- **Workspace_Tree**：Outer_Root 与 Inner_Project 合在一起的整个工作目录树，是本次清理的最大潜在范围。
- **Cleanup_Plan**：一份枚举所有候选条目并为每条标注处置决策的盘点表，作为执行前的唯一权威依据。
- **Inventory_Item**：Cleanup_Plan 中的一行，对应一个文件或一个目录（按"最小独立可处置单元"取粒度）。
- **Disposition**：Inventory_Item 的处置决策，取值集合为 `{Delete, Archive, Keep}`。
- **Rebuildable_Artifact**：可由仓库中已跟踪源（源码、JSON、配置、脚本）在合理时间内重新生成的产物，例如 `node_modules/`、`dist/`、`public/game-data/`、`playwright-report/`、`test-results/`、`.outputs/playwright/` 截图。
- **Foreign_Asset**：在 Inner_Project 文档（`AGENTS.md`、`CLAUDE.md`、`project-development-report.md`）中没有出处、且不在 Inner_Project git 跟踪范围内的资产。当前已知 Foreign_Asset 包括 Outer_Root 下 11 个 `.pth` 模型权重与 `tools/ml/` 下 10 个 ML 训练脚本。
- **Agent_Metadata**：代理工具（`.omc/`、`.claude/worktrees/` 等）写入的会话、检查点、回放、子代理跟踪等过程数据。
- **Archive_Bundle**：执行删除前由 Cleanup 流程生成的可逆备份压缩包（`.zip` 或 `.7z`），含被删除条目与一份 manifest（路径、大小、哈希、处置原因）。
- **Verification_Suite**：清理执行后用于确认 Inner_Project 主线未损坏的命令集合，包括 `python tools/validate_data.py`（如存在）、`python tools/validate_domain_core.py`、`python tools/validate_web_data_source.py`、`powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1`、`npm --prefix web-strategy-map run check:data-source`。
- **Cleanup_Operator**：执行清理工程的人或代理（即"系统"），负责生成 Cleanup_Plan、产出 Archive_Bundle、执行 Disposition、运行 Verification_Suite、回写 `project-development-report.md`。
- **Git_Baseline**：Inner_Project 中被 git 记录且可被他人 clone 后复现的项目状态。干净 Git_Baseline 不是指没有任何历史债，而是指 `git status --short` 中不存在未分类的 `M`、`D`、`??` 项。
- **Git_Baseline_Plan**：清理流水线第 0 步生成的 git 工作区基线整顿计划，逐条盘点 `git status --short` 项并分类为 Migration_Result、Legacy_Removal、Cleanup_Spec、Generated_Artifact、Unrelated_Local 或 Needs_Review。
- **Migration_Result**：纯代码主线迁移的成果文件和目录，当前包括 `domain-core/`、`web-strategy-map/game-data-source/`、`tools/validate_web_data_source.py`、纯代码主线相关 `tools/` 修改、`docs/` 修改、`AGENTS.md`、`CLAUDE.md`、`.gitignore`、`project-development-report.md` 中的迁移记录。
- **Legacy_Removal**：从当前主线移除旧 Unity/Tuanjie 形态所产生的受跟踪删除项，当前包括 `My project/`、`tools/unity/`、`tools/verify_unity_handoff.*`、`tools/validate_data.py`、`docs/unity-handoff-checklist.md`、旧 `.omc/` 和 tracked `.omx/` 状态/计划文件。
- **Cleanup_Spec**：本工作区清理特性的 `.kiro/specs/workspace-cleanup/` 需求、设计和任务文件。
- **Baseline_Commit**：将 Migration_Result、Legacy_Removal 和 Cleanup_Spec 按可审查边界落地到 git 历史中的一个或多个提交；提交前必须通过适配当前纯代码主线的验证命令。

## Requirements

### Requirement 1：清理范围与边界

**User Story:** 作为项目维护者，我希望先把"清理范围"用文字钉死，避免误删落在 Outer_Root 但其实有用的资产。

#### Acceptance Criteria

1. THE Cleanup_Operator SHALL 在 Cleanup_Plan 中显式声明本次清理覆盖 Outer_Root 与 Inner_Project 两层，并标记每个 Inventory_Item 所在层。
2. THE Cleanup_Operator SHALL 在 Cleanup_Plan 中将以下目录列为受保护目录，处置决策必须为 `Keep`：`e:\万朝归一\万朝归一\.git\`、`e:\万朝归一\万朝归一\domain-core\src\`、`e:\万朝归一\万朝归一\web-strategy-map\src\`、`e:\万朝归一\万朝归一\web-strategy-map\game-data-source\`、`e:\万朝归一\万朝归一\docs\`、`e:\万朝归一\万朝归一\tools\`（除明确列入 Inventory_Item 的子项外）、`e:\万朝归一\万朝归一\AGENTS.md`、`e:\万朝归一\万朝归一\CLAUDE.md`、`e:\万朝归一\万朝归一\project-development-report.md`、`e:\万朝归一\万朝归一\.gitignore`、`e:\万朝归一\万朝归一\.kiro\`。
3. IF 一个候选条目位于受保护目录之内且未被显式列为 Inventory_Item，THEN THE Cleanup_Operator SHALL 拒绝对该条目执行任何 Delete 或 Archive 操作。
4. THE Cleanup_Operator SHALL 在执行 Disposition 之前，对每一条 Delete 或 Archive 操作运行受保护目录前置校验：将目标路径与受保护目录列表逐一比对，命中受保护目录且未列入 Inventory_Item 的目标 SHALL 触发即时中止并写入失败原因，从机制上阻止违反 1.2 的操作完成。
5. WHEN Cleanup_Plan 涉及 Outer_Root 中任何路径，THE Cleanup_Operator SHALL 在该 Inventory_Item 上标注 "outside-git"，提醒该条目不受 Inner_Project 的 `.git` 历史保护。
6. THE Cleanup_Operator SHALL 在 Inventory_Item 之外保留一个独立的 protected-directories 配置清单，并把 1.4 中的前置校验实现为对该清单的查表检查，避免操作员绕过 1.2 的约束。

### Requirement 2：盘点与决策表（Cleanup_Plan）

**User Story:** 作为项目维护者，我希望在动手前看到一张完整的盘点表，每一条都有处置理由，我可以逐行审。

#### Acceptance Criteria

1. THE Cleanup_Operator SHALL 在执行任何删除或移动之前生成 Cleanup_Plan，并将其作为本特性的可交付产物提交评审。
2. THE Cleanup_Plan SHALL 为每个 Inventory_Item 至少记录以下字段：绝对路径、所在层（Outer_Root 或 Inner_Project）、类型（File 或 Directory）、字节大小、是否被现行 `.gitignore` 覆盖、Disposition、处置理由。
3. THE Cleanup_Plan SHALL 至少覆盖以下已知候选 Inventory_Item：
   - Outer_Root 下 11 个 `.pth` 模型权重：`cnn_model.pth`、`dqn_model.pth`、`gan_model.pth`、`gnn_model.pth`、`lstm_best.pth`、`vae_model.pth`、`mlp_optimized.pth`、`mlp_opt_0.pth`、`mlp_opt_1.pth`、`mlp_opt_2.pth`、`mlp_opt_3.pth`。
   - Outer_Root 下 `tools/ml/` 中 10 个训练脚本：`dqn_train.py`、`optimize_models.py`、`train_cnn.py`、`train_dqn.py`、`train_gan.py`、`train_gnn.py`、`train_lstm.py`、`train_transformer.py`、`train_vae.py`、`train_xgboost.py`。
   - Outer_Root 下 `tools/headless_runner/latest-war-report.json`。
   - Outer_Root 下 `.omc/` 代理元数据（`sessions/`、`state/`、`state/checkpoints/`、`state/sessions/`、`state/agent-replay-*.jsonl`、`state/last-tool-error.json`、`state/subagent-tracking.json`）。
   - Inner_Project 下 `web-strategy-map/node_modules/`、`web-strategy-map/dist/`、`web-strategy-map/playwright-report/`、`web-strategy-map/test-results/`、`web-strategy-map/dev-server.log`、`web-strategy-map/public/game-data/`。
   - Inner_Project 下 `.outputs/generated_images/`、`.outputs/imagegen/`、`.outputs/playwright/`。
   - Inner_Project 下 `.DS_Store`。
   - Inner_Project 下 `tools/headless_runner/latest-war-report.json`（如磁盘上仍存在）。
   - Inner_Project 下 `.claude/` 中除 `.claude/worktrees/` 之外的非空内容（如存在）。
4. WHEN Cleanup_Plan 中某个 Inventory_Item 的 Disposition 为 `Delete`，THE Cleanup_Operator SHALL 在处置理由中明确指出该条目属于 Rebuildable_Artifact、临时输出、过期日志、Agent_Metadata 或经显式确认的 Foreign_Asset。
5. WHEN Cleanup_Plan 中某个 Inventory_Item 的 Disposition 为 `Keep`，THE Cleanup_Operator SHALL 在处置理由中说明保留依据（例如：在 `AGENTS.md` 文档化、被构建脚本引用、被 `project-development-report.md` 引用）。

### Requirement 3：Foreign_Asset 决策（外层 ML 资产）

**User Story:** 作为项目维护者，我希望对来源不明的 ML 模型权重和训练脚本做一次显式决策，而不是糊里糊涂地删掉或留着。

#### Acceptance Criteria

1. THE Cleanup_Operator SHALL 在 Cleanup_Plan 中将 Outer_Root 下所有匹配 `*.pth` 的模型权重文件与 `tools/ml/` 下所有 `*.py` 训练脚本统一标注为 Foreign_Asset，无论实际数量是否仍为初稿调研时的 11 个 `.pth` 与 10 个 `.py`。Inventory_Item 中 SHALL 以盘点时刻实际存在的文件为准。
2. THE Cleanup_Operator SHALL 为 Foreign_Asset 的最终 Disposition 提供以下三个可选方案中的恰好一个，并记录选择理由：
   - 方案 A（Archive_Then_Delete）：先打包到 Archive_Bundle，再从 Workspace_Tree 删除。
   - 方案 B（Relocate_To_Sandbox）：将 Foreign_Asset 整体迁移到 Outer_Root 下一个新的、与主线明确隔离的目录（默认名 `_sandbox-ml-legacy/`），并在该目录根写一份 `README.md` 说明来源、归属、与主线的关系状态。
   - 方案 C（Keep_With_Documentation）：原地保留，但必须在 `project-development-report.md` 增加一节"外层 ML 资产说明"，记录用途、归属、是否纳入主线规划。
3. IF Foreign_Asset 的来源、归属、与主线关系三项确认条件中已有两项或两项以上明确（例如归属与关系已确认，仅来源不明），THEN THE Cleanup_Operator MAY 选择方案 C，并在 `project-development-report.md` 中显式记录已确认与未确认的具体维度。
4. IF Foreign_Asset 的来源、归属、与主线关系三项确认条件中明确数量少于两项，THEN THE Cleanup_Operator SHALL 选择方案 A 或方案 B，禁止选择方案 C。
5. WHERE Foreign_Asset 选择方案 A 或方案 B，THE Archive_Bundle 或 sandbox 目录 SHALL 同时保存对应的 `.pth` 权重与 `tools/ml/` 训练脚本作为一个整体，避免拆散权重与训练代码。

### Requirement 4：Rebuildable_Artifact 决策（内层产物）

**User Story:** 作为项目维护者，我希望可重建的产物可以被放心删除，但删除前要确认对应的"重建路径"是真的存在的。

#### Acceptance Criteria

1. THE Cleanup_Operator SHALL 在 Cleanup_Plan 中将以下条目标注为 Rebuildable_Artifact：`web-strategy-map/node_modules/`、`web-strategy-map/dist/`、`web-strategy-map/playwright-report/`、`web-strategy-map/test-results/`、`web-strategy-map/dev-server.log`、`web-strategy-map/public/game-data/`、`.outputs/playwright/`。
2. WHEN Inventory_Item 被标注为 Rebuildable_Artifact，THE Cleanup_Operator SHALL 在 Cleanup_Plan 中记录其重建命令（例如：`npm install`、`npm run build`、`npm run check:data-source`、对应 Playwright 测试命令）。
3. IF 某个 Rebuildable_Artifact 没有可在仓库脚本中找到的明确重建命令，THEN THE Cleanup_Operator SHALL 将该条目从 Rebuildable_Artifact 重新归类为待人工确认条目，并暂不进入本轮 Delete 列表。
4. WHERE 用户在评审 Cleanup_Plan 时显式选择"保留以避免重建成本"，THE Cleanup_Operator SHALL 把对应 Rebuildable_Artifact 的 Disposition 改为 `Keep`，并在 `.gitignore` 检查中确认这些条目继续被忽略。
5. WHEN `.outputs/imagegen/` 或 `.outputs/generated_images/` 中存在不可由仓库脚本直接重建的内容（例如外部 API 调用产物），THE Cleanup_Operator SHALL 在 Cleanup_Plan 中将其单独列为"非可重建生成物"，并强制走 Archive_Bundle 后再删除的流程，禁止直接 Delete。
6. WHEN 任意 Inventory_Item 在初稿盘点时被标注为 Rebuildable_Artifact，THE Cleanup_Operator SHALL 对该条目内容做一次自动检查：若内容中包含无法由仓库内脚本、配置、源码重建的部分（例如外部 API 返回的图片、第三方密钥派生的产物），THEN THE Cleanup_Operator SHALL 强制将该条目重新归类为"非可重建生成物"，并按 4.5 处理，无视初稿标注。

### Requirement 5：Agent_Metadata 决策

**User Story:** 作为项目维护者，我希望 `.omc/` 这类代理过程数据有一个明确的去留标准，而不是每次都临时拍脑袋。

#### Acceptance Criteria

1. THE Cleanup_Operator SHALL 在 Cleanup_Plan 中将 Outer_Root 下 `.omc/` 整体标注为 Agent_Metadata。
2. THE Cleanup_Operator SHALL 为 Agent_Metadata 提供以下两个可选方案中的恰好一个：
   - 方案 M1（Purge）：将 `.omc/` 整体打包到 Archive_Bundle 后从 Outer_Root 删除。
   - 方案 M2（Retain_As_History）：原地保留 `.omc/`，但必须确认 Inner_Project `.gitignore` 已经覆盖 `.omc/`（当前已覆盖）。
3. WHEN Agent_Metadata 选择方案 M1，THE Archive_Bundle SHALL 完整保存 `.omc/sessions/`、`.omc/state/checkpoints/`、`.omc/state/sessions/`、`.omc/state/agent-replay-*.jsonl`、`.omc/state/subagent-tracking.json`、`.omc/state/last-tool-error.json` 的目录结构与文件内容。
4. IF Inner_Project 中存在 `.claude/worktrees/` 之外的其他 `.claude/` 内容，THEN THE Cleanup_Operator SHALL 单独为这些内容生成 Inventory_Item，并按照 Agent_Metadata 同样的两套方案做决策。
5. WHERE 用户在评审阶段对 `.claude/` 中尚未生成 Inventory_Item 的具体路径做出 M1 或 M2 的预先决策，THE Cleanup_Operator SHALL 把该预先决策直接落入 Cleanup_Plan，并在 Inventory_Item 字段中标注 "pre-decided"，对应内容的 Disposition 不再二次征求评审。

### Requirement 6：可逆备份（Archive_Bundle）

**User Story:** 作为项目维护者，我希望删除是可逆的，万一删错了能在 7 天内恢复。

#### Acceptance Criteria

1. WHEN Cleanup_Plan 中存在任何 Disposition 为 `Delete` 的 Inventory_Item，THE Cleanup_Operator SHALL 在执行删除前先生成 Archive_Bundle。
2. IF Cleanup_Plan 中没有任何 Disposition 为 `Delete` 的 Inventory_Item，THEN THE Cleanup_Operator SHALL 跳过 Archive_Bundle 生成步骤，并在最终报告中明确标注"无需备份"。
3. THE Archive_Bundle SHALL 以 `cleanup-archive-<UTC-时间戳>.zip`（或 `.7z`）的形式存放在 Outer_Root 下一个明确的 `_archive/` 目录中，并在 Inner_Project `.gitignore` 中确认 Outer_Root 不会污染 git 状态（Outer_Root 本身不在 git 范围）。
4. THE Archive_Bundle SHALL 包含一份 `manifest.json`，对每个被打包的条目记录原始绝对路径、字节大小、SHA-256 哈希、处置理由、来源 Cleanup_Plan 行号。
5. IF Archive_Bundle 生成失败或 `manifest.json` 写入失败，THEN THE Cleanup_Operator SHALL 中止本次清理执行，不删除任何文件，并向用户报告失败原因。
6. WHERE Inventory_Item 的 Disposition 为 `Delete` 且该条目体积超过 1 GiB（例如完整的 `node_modules/`），THE Cleanup_Operator SHALL 在 Cleanup_Plan 中标注"Skip_Archive_Allowed"，并在征得用户在评审阶段的显式同意后跳过对该条目的 Archive_Bundle 打包，仅在 `manifest.json` 中登记元数据（路径、大小、跳过原因）。

### Requirement 7：执行顺序与原子性

**User Story:** 作为项目维护者，我希望执行清理时不要把工作树打到一个半完成的中间状态。

#### Acceptance Criteria

1. THE Cleanup_Operator SHALL 在 Requirement 11 的 Git_Baseline Gate 通过后，按以下固定顺序执行清理：（1）盘点并写出 Cleanup_Plan；（2）等待用户评审通过 Cleanup_Plan；（3）生成 Archive_Bundle；（4）执行 Disposition；（5）运行 Verification_Suite；（6）回写 `project-development-report.md`；（7）必要时更新 `.gitignore`。
2. IF Cleanup_Plan 未被用户显式评审通过，THEN THE Cleanup_Operator SHALL 拒绝进入步骤（3）及之后任何步骤。
3. WHEN 步骤（4）执行 Disposition 时遇到任何文件系统错误（权限拒绝、文件被占用、路径不存在），THE Cleanup_Operator SHALL 立即停止剩余删除/移动动作，记录失败的 Inventory_Item，并报告当前状态。
4. WHILE 步骤（4）正在执行，THE Cleanup_Operator SHALL 不修改受保护目录中未列入 Inventory_Item 的任何内容；对受保护目录之外、未列入 Inventory_Item 的条目不施加额外修改禁令。
5. WHEN 步骤（5）Verification_Suite 中任意一条命令以非零退出码结束，THE Cleanup_Operator SHALL 将清理流程标记为"验证失败"，并在最终报告中突出失败命令、失败输出与可能的回滚指引。

### Requirement 8：清理后的主线验证

**User Story:** 作为项目维护者，我希望清理结束后能用一组明确的命令证明主线还活着。

#### Acceptance Criteria

1. WHEN Disposition 执行完成，THE Cleanup_Operator SHALL 在 Inner_Project 根目录依次运行 Verification_Suite 中的所有命令。
2. THE Verification_Suite SHALL 至少包含以下命令：
   - `python tools/validate_domain_core.py`
   - `python tools/validate_web_data_source.py`
   - `npm --prefix web-strategy-map run check:data-source`
   - `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1`
3. IF Verification_Suite 中某个命令所依赖的产物已在清理中被删除（例如 `web-strategy-map/node_modules/`），THEN THE Cleanup_Operator SHALL 先执行该命令的重建前置（例如 `npm --prefix web-strategy-map install`），再执行验证命令。
4. THE Cleanup_Operator SHALL 把每条验证命令的退出码、关键输出片段、运行时长记录到清理流程的最终报告中。
5. IF Verification_Suite 全部通过，THEN THE Cleanup_Operator SHALL 在最终报告中将清理状态标记为"Pass"，否则标记为"Fail"并保留 Archive_Bundle 直到用户确认处置。

### Requirement 9：留痕到开发报告

**User Story:** 作为项目维护者，我希望这次清理在 `project-development-report.md` 留下一段可被未来检索的记录。

#### Acceptance Criteria

1. WHEN 清理流程进入步骤（6），THE Cleanup_Operator SHALL 在 `project-development-report.md` 末尾追加一节，标题包含"工作区清理"与执行日期。
2. THE Cleanup_Operator SHALL 在该节中至少记录：覆盖范围（Outer_Root + Inner_Project）、Cleanup_Plan 中各 Disposition 的条目数量统计、Archive_Bundle 路径、Verification_Suite 各命令结果、对 Foreign_Asset 选择的方案（A/B/C）、对 Agent_Metadata 选择的方案（M1/M2）。
3. WHERE 选择了方案 C（Keep_With_Documentation）保留 Foreign_Asset，THE Cleanup_Operator SHALL 在同一节中补一段 Foreign_Asset 用途与归属说明。
4. THE Cleanup_Operator SHALL 在记录中以引用方式指向 Cleanup_Plan 与 Archive_Bundle 的实际文件路径，便于事后追溯。

### Requirement 10：防回归（`.gitignore` 与目录契约）

**User Story:** 作为项目维护者，我希望这次清理之后，类似的杂物不要再次悄悄长回来。

#### Acceptance Criteria

1. WHEN 清理流程进入步骤（7），THE Cleanup_Operator SHALL 比对当前 `.gitignore` 与 Cleanup_Plan 中所有 Disposition 为 `Delete` 的 Inventory_Item，列出未被任何忽略规则覆盖的条目。
2. IF 存在 Inner_Project 内部的已删除条目未被 `.gitignore` 覆盖，THEN THE Cleanup_Operator SHALL 向 `.gitignore` 追加最小必要规则覆盖该条目，并在追加前预览 diff 由用户确认。
3. THE Cleanup_Operator SHALL 在 `.gitignore` 中显式覆盖以下与本次清理直接相关的路径模式（若现行规则尚未覆盖）：`web-strategy-map/dev-server.log`、`web-strategy-map/playwright-report/`、`web-strategy-map/test-results/`、`web-strategy-map/dist/`、`web-strategy-map/node_modules/`、`web-strategy-map/public/game-data/`、`.outputs/`、`.DS_Store`、`tools/headless_runner/latest-war-report.json`、`.omc/`、`.claude/worktrees/`。
4. WHERE Outer_Root 中的 Foreign_Asset 在本次选择了方案 B（Relocate_To_Sandbox），THE Cleanup_Operator SHALL 在 sandbox 目录的 `README.md` 中明确声明该目录不进入 Inner_Project 的版本管理范围，并在 Inner_Project 中保持现状（不引用、不构建、不打包）。
5. THE Cleanup_Operator SHALL 把 `.gitignore` 的最终更新列入步骤（6）中的留痕记录，确保未来读 `project-development-report.md` 的人能复原本次防回归改动。

### Requirement 11：git 工作区基线整顿

**User Story:** 作为项目维护者，我希望清理流程在删除缓存和归档杂物之前，先确认当前纯代码迁移已经进入 git 历史，避免在只存在于本机磁盘的假绿状态上继续清理。

#### Acceptance Criteria

1. THE Cleanup_Operator SHALL 在执行 Requirement 7 的 7 步流水线之前，先运行 Git_Baseline Gate，并把它作为流水线第 0 步。
2. THE Git_Baseline Gate SHALL 运行 `git status --short`、`git ls-files --others --exclude-standard`、`git ls-files --deleted` 和 `git diff --name-only`，并将所有条目写入 Git_Baseline_Plan。
3. THE Git_Baseline_Plan SHALL 为每个 git 状态条目至少记录：路径、git 状态码、是否已被 git 跟踪、分类（Migration_Result / Legacy_Removal / Cleanup_Spec / Generated_Artifact / Unrelated_Local / Needs_Review）、建议动作（Commit / Ignore / Keep_Local / Reject / Needs_Decision）和处置理由。
4. THE Git_Baseline_Plan SHALL 将以下路径族强制分类为 Migration_Result，除非磁盘上已经不存在：`domain-core/`、`web-strategy-map/game-data-source/`、`tools/validate_web_data_source.py`、`tools/run_headless_simulation.*`、`tools/verify_headless_war.*`、`tools/validate_domain_core.py`、`tools/art/` 中切到 Web 数据源的脚本、`web-strategy-map/scripts/sync-data.mjs`、`web-strategy-map/package.json`、`web-strategy-map/src/`、`web-strategy-map/tests/`、`docs/`、`AGENTS.md`、`CLAUDE.md`、`.gitignore`、`project-development-report.md`。
5. THE Git_Baseline_Plan SHALL 将以下受跟踪删除强制分类为 Legacy_Removal，除非用户显式要求恢复旧 Unity/Tuanjie 主线：`My project/`、`tools/unity/`、`tools/verify_unity_handoff.*`、`tools/validate_data.py`、`docs/unity-handoff-checklist.md`、tracked `.omc/` 和 tracked `.omx/` 历史状态/计划文件。
6. THE Git_Baseline_Plan SHALL 将 `.kiro/specs/workspace-cleanup/` 下的 `requirements.md`、`design.md`、`tasks.md` 分类为 Cleanup_Spec，并明确它们是清理流程的规则输入，不是可删除杂物。
7. IF 任意 Migration_Result 文件或目录为 untracked，THEN THE Cleanup_Operator SHALL 将 Git_Baseline Gate 标记为 Fail，并禁止进入 Requirement 7 步骤（1），直到这些迁移成果被纳入 Baseline_Commit 或被明确重分类为 Reject / Needs_Decision。
8. IF 任意 Legacy_Removal 删除项尚未进入 Baseline_Commit，THEN THE Cleanup_Operator SHALL 将 Git_Baseline Gate 标记为 Fail，并禁止进入 Requirement 7 步骤（1），直到这些删除被提交或被显式恢复。
9. IF `tools/validate_web_data_source.py` 未被 git 跟踪，THEN THE Cleanup_Operator SHALL 将所有依赖该脚本的 Verification_Suite 结果标记为 "local-only evidence"，禁止将其作为可复现通过证据写成最终 Pass。
10. WHEN Git_Baseline Gate 发现 Generated_Artifact（例如 `web-strategy-map/dist/`、`web-strategy-map/node_modules/`、`web-strategy-map/public/game-data/`、`web-strategy-map/test-results/`、`web-strategy-map/playwright-report/`、`.outputs/`），THE Cleanup_Operator SHALL 仅将其列为后续 Cleanup_Plan 候选，不得把它们与 Migration_Result 或 Legacy_Removal 混在同一个 Baseline_Commit 中。
11. THE Cleanup_Operator SHALL 在 Baseline_Commit 前运行适配当前纯代码主线的验证命令，最低包括 `npm --prefix web-strategy-map run check:data-source`、`python tools\validate_domain_core.py`、`powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1`、`npm --prefix web-strategy-map run build`，并记录 Vite chunk-size advisory 等非零风险警告。
12. THE Cleanup_Operator SHALL 在 `project-development-report.md` 中追加 "git 工作区基线整顿" 记录，至少包含 Git_Baseline_Plan 路径、Migration_Result 条目数、Legacy_Removal 条目数、Cleanup_Spec 条目数、验证命令结果、是否已产生 Baseline_Commit，以及尚未进入 git 的 local-only 排除项。
13. IF Git_Baseline Gate 未通过，THEN THE Cleanup_Operator SHALL 只允许继续修改 Git_Baseline_Plan、Cleanup_Spec 和 `project-development-report.md`，禁止执行 Delete / Archive / `.gitignore` patch 等清理动作。
14. WHEN Git_Baseline Gate 通过，THE Cleanup_Operator SHALL 确认 `git status --short` 中只剩允许的 local-only 排除项或为空，然后才允许进入 Requirement 7 步骤（1）。
