# web-general-platform

一期平台基础建设 app。

- 包名：`@general/web-general-platform`
- 职责：承载组织架构、用户管理、角色权限、平台审计等基础平台能力
- 开发命令：`pnpm run dev:general-platform`
- Web 构建：`pnpm run build:general-platform`
- Desktop 构建：`pnpm run build:general-platform:desktop`

说明：

- 本 app 基于框架原生 Ant Design Vue 技术栈扩展
- 平台菜单、页面鉴权、组织权限范围与角色授权均在这里收敛
- `.env.desktop` 仅用于桌面构建模式，不参与常规 `dev`
