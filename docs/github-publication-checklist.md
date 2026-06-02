# GitHub Publication Checklist

用于权限恢复后发布本轮仓库修饰，并在 GitHub 页面确认效果。

## 提交范围

只提交仓库门面和社区健康文件：

- `README.md`
- `CONTRIBUTING.md`
- `SECURITY.md`
- `CODE_OF_CONDUCT.md`
- `SUPPORT.md`
- `.github/ISSUE_TEMPLATE/bug_report.md`
- `.github/ISSUE_TEMPLATE/feature_request.md`
- `.github/ISSUE_TEMPLATE/config.yml`
- `.github/CODEOWNERS`
- `.github/dependabot.yml`
- `.github/pull_request_template.md`
- `docs/assets/github-repository-banner.svg`
- `docs/github-repository-profile.md`
- `docs/github-style-guide.md`
- `docs/github-publication-checklist.md`
- `project-development-report.md`

不要把以下本地运行产物并入仓库修饰提交：

- `web-strategy-map/.codex-runlogs/`
- `web-strategy-map/debug.log`
- `.outputs/`
- `test-results/`

## 提交前本地检查

```powershell
git diff --check
```

README 链接检查：

```powershell
$content = Get-Content -Raw README.md
$matches = [regex]::Matches($content, '\[[^\]]+\]\(([^)]+)\)|!\[[^\]]*\]\(([^)]+)\)')
foreach ($m in $matches) {
  $p = if ($m.Groups[1].Value) { $m.Groups[1].Value } else { $m.Groups[2].Value }
  if ($p -notmatch '^https?://') { "$p => $(Test-Path $p)" }
}
```

SVG 解析检查：

```powershell
$svg = Get-Content -Raw docs\assets\github-repository-banner.svg
[xml]$svg | Out-Null
```

## 推荐提交命令

```powershell
git add -- README.md CONTRIBUTING.md SECURITY.md CODE_OF_CONDUCT.md SUPPORT.md .github/CODEOWNERS .github/dependabot.yml .github/ISSUE_TEMPLATE/bug_report.md .github/ISSUE_TEMPLATE/feature_request.md .github/ISSUE_TEMPLATE/config.yml .github/pull_request_template.md docs/assets/github-repository-banner.svg docs/github-repository-profile.md docs/github-style-guide.md docs/github-publication-checklist.md project-development-report.md
git commit -m "Improve the GitHub repository front door"
git push origin main
```

提交正文应记录：

- 根 README 缺失是主要问题。
- 新增社区健康文件和仓库展示图。
- 本地链接、SVG、diff hygiene 已验证。
- GitHub About/Topics 如未能通过 CLI 设置，应在 `Not-tested` 或 `Not-done` 中说明。

## GitHub 页面确认

发布后检查：

- 仓库首页显示 `README.md`。
- README 第一屏显示 CI badge 和 `docs/assets/github-repository-banner.svg`。
- README 中九州地图能正常渲染。
- Issue 创建页显示 Bug report / Feature request 模板。
- PR 创建页显示 PR 模板。
- `Security` 或仓库根目录能访问 `SECURITY.md`。
- 仓库根目录能访问 `CONTRIBUTING.md`、`CODE_OF_CONDUCT.md` 和 `SUPPORT.md`。

## GitHub About 推荐值

Description:

```text
纯代码历史策略 Demo：Vite + Three.js Web 九州地图，C# headless Domain Core，验证帝皇机制、王朝治理、战争后勤和继承风险闭环。
```

Topics:

```text
historical-strategy-game, threejs, vite, typescript, csharp, headless-simulation, game-data, strategy-game, chinese-history, web-game
```

Social Preview:

```text
docs/assets/github-repository-banner.svg
```
