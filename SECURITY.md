# Security Policy

本仓库当前是游戏 Demo 与本地验证工具项目，不处理线上账号、支付、真实用户隐私或服务端生产密钥。

## 报告方式

如果发现硬编码密钥、危险脚本、依赖供应链风险或可导致本地文件破坏的问题，请通过 GitHub Issue 提交最小复现信息，并避免公开真实凭据。

## 范围

- Web 前端运行时代码。
- `domain-core/src` headless 玩法核心。
- `tools/` 下的校验、构建和验证脚本。
- GitHub Actions workflow。

不在范围：本地缓存、构建产物、`node_modules`、`.outputs`、编辑器临时目录。