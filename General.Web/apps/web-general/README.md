# web-general

统一前端入口 app，同时承载完整业务版与平台中心版。

- 包名：`@general/web-general`
- 职责：承载平台中心、项目执行、经营管理等页面；平台中心单独形态也从这里构建
- 开发命令：`pnpm run dev:general`
- 平台开发命令：`pnpm --filter @general/web-general run dev:platform`
- Web 构建：`pnpm run build:general`
- 平台 Web 构建：`pnpm --filter @general/web-general run build:platform`
- Desktop 构建：`pnpm run build:general:desktop`
- 平台 Desktop 构建：`pnpm --filter @general/web-general run build:platform:desktop`

说明：

- 本 app 基于框架原生 Ant Design Vue 技术栈扩展
- 页面统一走现有主题 token、菜单鉴权和路由守卫机制
- 通过脚本注入的 `VITE_GLOB_APP_CODES` 控制当前形态承接哪些板块
- `.env.desktop` 仅用于桌面构建模式，不参与常规 `dev`
