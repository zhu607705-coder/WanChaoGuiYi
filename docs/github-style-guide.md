# GitHub Style Guide

本文件用于统一仓库 README、About、Topics、Social Preview 和 Issue/PR 模板的展示口径。

## 一句话定位

```text
纯代码历史策略 Demo：Vite + Three.js Web 九州地图，C# headless Domain Core，验证帝皇机制、王朝治理、战争后勤和继承风险闭环。
```

## 第一屏结构

GitHub 首页 README 第一屏保持以下顺序：

1. 项目名。
2. CI / 技术栈 / 数据规模 badge。
3. 一句话定位。
4. `docs/assets/github-repository-banner.svg`。
5. 九州地图展示图。

## 关键词

优先使用这些关键词：

- 帝皇机制
- 九州统一
- 王朝治理
- 继承风险
- 战争后勤
- headless 验证
- 纯代码 Web Demo

避免把项目描述成完整发行版、全球策略游戏或大型 3D 项目。MVP 仍聚焦国内版 Demo。

## 配色

仓库展示图、badge 和 README 文案应沿用项目 UI 的四个主色：

| 名称 | 色值 | 用途 |
| --- | --- | --- |
| Bronze | `#d4ad61` | 标题、制度、历史感 |
| Jade | `#57a696` | Web、治理、验证通过 |
| Field | `#7e9352` | 地图、地区、数据规模 |
| Ink | `#101918` | 背景、深色主视觉 |

## 推荐 Topics

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

## 截图和预览

- README 第一屏使用 SVG banner。
- 项目实机展示优先使用 `web-strategy-map/game-data-source/art/Map/jiuzhou_generated_map.png`。
- 不把 `.outputs/`、`playwright-report/` 或本地 debug log 作为仓库门面素材直接提交。