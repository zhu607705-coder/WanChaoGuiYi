# Implementation Plan: workspace-cleanup

## Overview

实现工作区清理工具链，由 Python CLI 脚本 `tools/workspace_cleanup.py` 驱动 Stage 0 git 基线门禁 + 7 步流水线（Git Baseline Gate → Inventory → Review Gate → Archive → Execute → Verify → Report → Gitignore）。所有模块使用 Python 3.10+ 标准库，Archive 格式为 `.zip`，路径处理兼容 Windows。

## Tasks

- [ ] 0. Implement Git Baseline Gate before any cleanup action
  - [ ] 0.1 Implement `tools/cleanup/git_baseline.py`
    - Run `git status --short`, `git ls-files --others --exclude-standard`, `git ls-files --deleted`, and `git diff --name-only`
    - Build `GitBaselineItem` rows with path, status_code, tracked flag, classification, recommended_action, and reason
    - Classify pure-code migration outputs as `Migration_Result`
    - Classify deleted Unity/Tuanjie legacy paths and tracked `.omc/.omx` removals as `Legacy_Removal`
    - Classify `.kiro/specs/workspace-cleanup/` as `Cleanup_Spec`
    - Classify generated outputs as `Generated_Artifact`, not baseline commit material
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.10_

  - [ ] 0.2 Implement Git Baseline Gate evaluation
    - Fail when any `Migration_Result` is untracked
    - Fail when any `Legacy_Removal` deletion is uncommitted
    - Downgrade verification depending on untracked `tools/validate_web_data_source.py` to `local-only evidence`
    - Allow cleanup scan only when baseline gate passes or only allowed local-only exclusions remain
    - _Requirements: 11.7, 11.8, 11.9, 11.13, 11.14_

  - [ ] 0.3 Add Git baseline data models
    - Extend `tools/cleanup/models.py` with `GitBaselineItem` and `GitBaselinePlan`
    - Add JSON serialization/deserialization helpers for `git-baseline-plan.json`
    - _Requirements: 11.3_

  - [ ]* 0.4 Write property test for Git Baseline Gate — Property 0
    - **Property 0: Git Baseline Gate Correctness**
    - Generate git status snapshots with tracked/untracked migration outputs and committed/uncommitted legacy removals
    - Verify gate pass/fail behavior and `local-only evidence` downgrade
    - **Validates: Requirements 11.1, 11.7, 11.8, 11.9, 11.13, 11.14**

- [ ] 1. Set up project structure and configuration files
  - [ ] 1.1 Create configuration directory and JSON config files
    - Create `tools/cleanup-config/` directory
    - Create `tools/cleanup-config/protected-directories.json` with protected dirs/files list per design
    - Create `tools/cleanup-config/verification-suite.json` with verification commands
    - Create `tools/cleanup-config/rebuild-commands.json` with rebuild command mappings
    - _Requirements: 1.2, 1.6, 4.2, 8.2_

  - [ ] 1.2 Create package structure and shared data models
    - Create `tools/cleanup/__init__.py`
    - Create `tools/cleanup/models.py` with `GitBaselineItem`, `GitBaselinePlan`, `InventoryItem`, `ManifestEntry`, `ExecutionResult`, `VerificationResult` dataclasses
    - Define Literal types for layer, item_type, disposition, classification, schemes
    - _Requirements: 2.2, 11.3_

  - [ ] 1.3 Set up test framework and conftest
    - Create `tools/tests/__init__.py`
    - Create `tools/tests/test_workspace_cleanup/__init__.py`
    - Create `tools/tests/test_workspace_cleanup/conftest.py` with hypothesis profiles and shared fixtures (temp workspace builder)
    - _Requirements: Testing Strategy_

- [ ] 2. Implement Protection Guard module
  - [ ] 2.1 Implement `tools/cleanup/protection.py`
    - Implement `load_protected_directories(config_path)` to load from JSON
    - Implement `is_protected(target_path, protected_dirs, inventory_items)` with case-insensitive Windows path comparison
    - Implement `validate_plan(plan, protected_dirs)` returning list of violations
    - _Requirements: 1.3, 1.4, 1.6, 7.4_

  - [ ]* 2.2 Write property test for Protection Guard — Property 2
    - **Property 2: Protected Directory Invariant**
    - Generate arbitrary paths inside/outside protected directories and verify guard behavior
    - **Validates: Requirements 1.2, 1.3, 1.4, 7.4**

  - [ ]* 2.3 Write unit tests for Protection Guard
    - Test case-insensitive path matching on Windows
    - Test explicit Inventory_Item override (item inside protected dir that IS listed)
    - Test nested protected directory resolution
    - _Requirements: 1.3, 1.4_

- [ ] 3. Implement Inventory Scanner module
  - [ ] 3.1 Implement `tools/cleanup/scanner.py`
    - Implement `scan_workspace(outer_root, inner_project)` to walk both directory trees
    - Determine layer classification (Outer_Root vs Inner_Project) with `outside-git` flag
    - Collect size_bytes, item_type for each candidate
    - Check gitignore coverage for Inner_Project items
    - _Requirements: 1.1, 1.5, 2.1, 2.2, 2.3_

  - [ ]* 3.2 Write property test for layer classification — Property 1
    - **Property 1: Layer Classification Correctness**
    - Generate arbitrary paths under outer_root/inner_project and verify layer assignment and outside-git flag
    - **Validates: Requirements 1.1, 1.5**

- [ ] 4. Implement Classifier Engine module
  - [ ] 4.1 Implement `tools/cleanup/classifier.py`
    - Implement `is_foreign_asset(path)` for `*.pth` and `tools/ml/*.py` detection
    - Implement `is_rebuildable(path, rebuild_db)` checking rebuild-commands.json
    - Implement `is_agent_metadata(path)` for `.omc/` and `.claude/` patterns
    - Implement `check_truly_rebuildable(path, rebuild_db)` for non-rebuildable content detection
    - Implement `classify_item(item, protected_dirs, rebuild_db)` orchestrating all classification logic
    - Assign disposition and reason based on classification rules
    - _Requirements: 2.4, 2.5, 3.1, 4.1, 4.3, 4.5, 4.6, 5.1_

  - [ ]* 4.2 Write property test for disposition reason validity — Property 4
    - **Property 4: Disposition Reason Validity**
    - Generate InventoryItems with various dispositions and verify reason field references valid categories
    - **Validates: Requirements 2.4, 2.5**

  - [ ]* 4.3 Write property test for Foreign_Asset scheme selection — Property 5
    - **Property 5: Foreign_Asset Scheme Selection**
    - Generate confirmation states (0-3 dimensions confirmed) and verify scheme A/B/C validity
    - **Validates: Requirements 3.3, 3.4**

  - [ ]* 4.4 Write property test for rebuildable classification — Property 7
    - **Property 7: Rebuildable Classification Integrity**
    - Generate items with/without matching rebuild commands and verify reclassification
    - **Validates: Requirements 4.2, 4.3, 4.6**

- [ ] 5. Checkpoint — Ensure core classification logic is solid
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 6. Implement Archive Generator module
  - [ ] 6.1 Implement `tools/cleanup/archiver.py`
    - Implement `generate_archive(items_to_archive, output_dir, skip_large)` using `zipfile`
    - Compute SHA-256 for each file before archiving
    - Generate `manifest.json` with all required fields
    - Handle Skip_Archive_Allowed items (>1 GiB) — metadata-only entries
    - Return archive path and manifest entries
    - Raise on failure (FATAL level) so caller can abort
    - _Requirements: 6.1, 6.3, 6.4, 6.5, 6.6_

  - [ ]* 6.2 Write property test for Archive Manifest Schema — Property 8
    - **Property 8: Archive Manifest Schema Completeness**
    - Generate ManifestEntry objects and verify all required fields present with correct types
    - **Validates: Requirements 6.4**

  - [ ]* 6.3 Write property test for archive generation trigger — Property 9
    - **Property 9: Archive Generation Trigger**
    - Generate plans with/without Delete items and verify archive step executes iff Delete exists
    - **Validates: Requirements 6.1, 6.2**

  - [ ]* 6.4 Write property test for large item skip logic — Property 10
    - **Property 10: Large Item Skip Logic**
    - Generate items with size > 1 GiB and verify Skip_Archive_Allowed flag and manifest behavior
    - **Validates: Requirements 6.6**

  - [ ]* 6.5 Write property test for Foreign_Asset bundle integrity — Property 6
    - **Property 6: Foreign_Asset Bundle Integrity**
    - Generate archive operations for Foreign_Assets and verify .pth + .py are bundled together
    - **Validates: Requirements 3.5**

- [ ] 7. Implement Execution Engine module
  - [ ] 7.1 Implement `tools/cleanup/executor.py`
    - Implement `execute_plan(plan, protected_dirs)` iterating Delete/Archive items
    - Run protection guard check per-item before each operation (double validation)
    - Use `shutil.rmtree` for directories, `os.remove` for files
    - Handle relocate (scheme B) by moving to `_sandbox-ml-legacy/` with README
    - Stop on first FS error (permission denied, file locked), record partial results
    - _Requirements: 7.1, 7.3, 7.4, 3.2 (scheme B)_

  - [ ]* 7.2 Write property test for pipeline state machine — Property 11
    - **Property 11: Pipeline State Machine Validity**
    - Generate sequences of state transitions and verify only valid forward transitions allowed
    - **Validates: Requirements 7.1, 7.2**

- [ ] 8. Implement Verification Runner module
  - [ ] 8.1 Implement `tools/cleanup/verifier.py`
    - Implement `run_verification_suite(suite_config, deleted_items, rebuild_db, cwd)`
    - Load commands from `verification-suite.json`
    - Check if prerequisite_trigger path was deleted; if so, run prerequisite first
    - Execute each command with `subprocess.run`, capture exit_code, last 50 lines, duration
    - _Requirements: 8.1, 8.2, 8.3, 8.4_

  - [ ]* 8.2 Write property test for verification status determination — Property 12
    - **Property 12: Verification Status Determination**
    - Generate sets of VerificationResults and verify overall Pass/Fail logic
    - **Validates: Requirements 7.5, 8.5**

  - [ ]* 8.3 Write property test for prerequisite injection — Property 13
    - **Property 13: Verification Prerequisite Injection**
    - Generate deleted item lists and verify prerequisite commands are injected when needed
    - **Validates: Requirements 8.3**

  - [ ]* 8.4 Write property test for verification result recording — Property 14
    - **Property 14: Verification Result Recording**
    - Generate VerificationResults and verify all required fields present
    - **Validates: Requirements 8.4**

- [ ] 9. Checkpoint — Ensure execution and verification modules work
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 10. Implement Report Writer module
  - [ ] 10.1 Implement `tools/cleanup/reporter.py`
    - Implement `write_report(report_path, plan, archive_path, verification_results, foreign_asset_scheme, agent_metadata_scheme, gitignore_changes)`
    - Append section to `project-development-report.md` with title "工作区清理 — {date}"
    - Include: scope, disposition statistics, archive path, verification results, scheme selections
    - Add a separate "git 工作区基线整顿" subsection with Git_Baseline_Plan path, Migration_Result count, Legacy_Removal count, Cleanup_Spec count, verification commands, Baseline_Commit status, and local-only exclusions
    - For scheme C, include Foreign_Asset description section
    - Reference Cleanup_Plan and Archive_Bundle file paths
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 11.12_

  - [ ]* 10.2 Write property test for report content completeness — Property 15
    - **Property 15: Report Content Completeness**
    - Generate cleanup results and verify report contains all required sections
    - **Validates: Requirements 9.2**

- [ ] 11. Implement Gitignore Patcher module
  - [ ] 11.1 Implement `tools/cleanup/gitignore_patcher.py`
    - Implement `find_uncovered_paths(deleted_inner_paths, gitignore_path)` with pattern matching
    - Implement `generate_rules(uncovered_paths)` producing minimal .gitignore rules
    - Implement `apply_patch(gitignore_path, new_rules, dry_run=True)` returning diff string
    - Ensure required patterns from Req 10.3 are included if missing
    - _Requirements: 10.1, 10.2, 10.3, 10.5_

  - [ ]* 11.2 Write property test for gitignore coverage — Property 16
    - **Property 16: Gitignore Coverage Completeness**
    - Generate deleted paths and .gitignore rules, verify gap detection correctness
    - **Validates: Requirements 10.1, 10.2**

- [ ] 12. Implement CLI Entry Point and pipeline state machine
  - [ ] 12.1 Implement `tools/workspace_cleanup.py` CLI
    - Parse subcommands: `baseline`, `scan`, `approve`, `execute`, `status`
    - Implement state persistence (read/write cleanup-plan.json state field)
    - `baseline`: orchestrate Git Baseline Scanner → Git Baseline Classifier → Gate evaluation → write git-baseline-plan.json
    - `scan`: refuse to run unless Git Baseline Gate has passed; then orchestrate Scanner → Classifier → Protection Guard → write cleanup-plan.json
    - `approve`: validate plan exists and is in `draft` state, transition to `approved`
    - `execute`: enforce `approved` state gate, run Archive → Execute → Verify → Report → Gitignore
    - `status`: print current pipeline state and summary
    - Implement FATAL/HALT/WARN/INFO error handling per design
    - _Requirements: 7.1, 7.2, 7.3, 7.5, 11.1, 11.13, 11.14_

  - [ ]* 12.2 Write property test for InventoryItem schema — Property 3
    - **Property 3: Inventory_Item Schema Completeness**
    - Generate InventoryItem objects, serialize to JSON and back, verify roundtrip
    - **Validates: Requirements 2.2**

- [ ] 13. Write integration tests
  - [ ]* 13.1 Write end-to-end integration test
    - Create temp directory simulating Workspace_Tree structure
    - Verify untracked migration outputs block `scan`
    - Verify committed/tracked migration outputs and legacy removals allow `baseline → scan → approve → execute`
    - Verify Archive_Bundle contents match manifest
    - Verify deleted items are gone, protected items remain
    - Verify .gitignore patch covers deleted inner paths
    - _Requirements: 11.7, 11.8, 7.1, 6.4, 1.3, 10.2_

- [ ] 14. Final checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Git_Baseline Gate runs before Cleanup_Plan generation; if it fails, cleanup actions are limited to spec/report/baseline-plan edits.
- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document
- Unit tests validate specific examples and edge cases
- All modules use only Python 3.10+ standard library (`json`, `hashlib`, `zipfile`, `pathlib`, `subprocess`, `shutil`)
- Tests use `pytest` + `hypothesis` (the only external test dependencies)
- Windows path handling (case-insensitive, backslash) is a cross-cutting concern in all path-related modules

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["0.1", "0.2", "0.3", "1.1", "1.2", "1.3"] },
    { "id": 1, "tasks": ["0.4", "2.1", "3.1"] },
    { "id": 2, "tasks": ["2.2", "2.3", "3.2", "4.1"] },
    { "id": 3, "tasks": ["4.2", "4.3", "4.4", "6.1"] },
    { "id": 4, "tasks": ["6.2", "6.3", "6.4", "6.5", "7.1"] },
    { "id": 5, "tasks": ["7.2", "8.1"] },
    { "id": 6, "tasks": ["8.2", "8.3", "8.4", "10.1"] },
    { "id": 7, "tasks": ["10.2", "11.1"] },
    { "id": 8, "tasks": ["11.2", "12.1"] },
    { "id": 9, "tasks": ["12.2", "13.1"] }
  ]
}
```
