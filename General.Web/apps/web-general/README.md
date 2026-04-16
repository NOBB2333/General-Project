# web-general

一期项目管理业务 app。

- 包名：`@general/web-general`
- 职责：承载项目进度、项目空间、PMO 视图等一期业务页面
- 开发命令：`pnpm run dev:general`
- Web 构建：`pnpm run build:general`
- Desktop 构建：`pnpm run build:general:desktop`

说明：

- 本 app 基于框架原生 Ant Design Vue 技术栈扩展
- 业务页面统一走现有主题 token、菜单鉴权和路由守卫机制
- `.env.desktop` 仅用于桌面构建模式，不参与常规 `dev`
