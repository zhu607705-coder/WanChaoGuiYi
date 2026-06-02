# Contributing

感谢关注《万朝归一：九州帝业》。本项目当前以纯代码 Web Demo 和 headless Domain Core 为主线，所有改动都应保持可验证、可回滚、可解释。

## 开发前

1. 先阅读 [MVP 设计](docs/mvp-design.md)、[系统架构](docs/architecture.md) 和 [数据契约](docs/data-contract.md)。
2. 非平凡改动需要同步更新 `project-development-report.md`。
3. 新数据字段先写入 `docs/data-contract.md`，再进入 `web-strategy-map/game-data-source`。

## 提交前验证

按改动范围选择验证，数据、Web、Domain Core 至少覆盖对应入口：

```powershell
python tools\validate_web_data_source.py
python tools\validate_domain_core.py
npm --prefix web-strategy-map run typecheck
npm --prefix web-strategy-map run test:ui
npm --prefix web-strategy-map run build
```

## 提交说明

提交信息优先说明为什么改，而不是重复文件变更。涉及机制、数据或验证边界时，请在正文中记录约束、验证和未验证风险。