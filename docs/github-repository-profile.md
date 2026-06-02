# GitHub Repository Profile

本文件记录仓库主页推荐展示信息，便于维护 GitHub About 区域、Topics 和社交预览。

## About

Description:

```text
纯代码历史策略 Demo：Vite + Three.js Web 九州地图，C# headless Domain Core，验证帝皇机制、王朝治理、战争后勤和继承风险闭环。
```

Website:

```text

```

Topics:

```text
historical-strategy-game
threejs
vite
typescript
csharp
headless-simulation
game-data
strategy-game
chinese-history
web-game
```

## Social Preview

推荐使用仓库内展示图：

```text
docs/assets/github-repository-banner.svg
```

## GitHub Automation

- `CODEOWNERS`: `* @zhu607705-coder`
- Dependabot: weekly npm updates for `web-strategy-map` and weekly NuGet updates for `tools/headless_runner/WanChaoGuiYiTests`

如果后续要上传专用社交预览图，建议尺寸为 `1280x640`，内容包含项目名、九州地图和「帝皇机制 / 王朝治理 / 继承风险」三项关键词。

## README 维护要点

- 第一屏保留 CI badge、技术栈 badge、项目定位和九州地图。
- 快速运行命令保持与 `web-strategy-map/package.json` 同步。
- 常用验证命令保持与 `.github/workflows/ci.yml` 和 `tools/run_all_checks.ps1` 同步。
- 新增玩法系统时，同步更新 README 的「当前看点」和 `docs/mvp-closure-ledger.md`。

## 发布检查

提交和推送仓库门面变更前，按 `docs/github-publication-checklist.md` 复核文件范围、链接、SVG 和 GitHub 页面确认项。

## 风格口径

README、About、Topics 和社交预览图的文案与配色按 `docs/github-style-guide.md` 维护。
