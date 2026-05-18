# 前端工程

General Web 使用 pnpm monorepo 管理。

## 主应用

`apps/web-general` 是业务主应用，负责平台中心、项目中心和经营中心。

常用命令：

```bash
cd General.Web
pnpm dev:general
pnpm build:general
```

只构建平台中心：

```bash
cd General.Web
pnpm dev:general-platform
pnpm build:general-platform
```

## 文档站

`apps/web-general-docs` 是当前项目文档站。

```bash
cd General.Web
pnpm dev:general-docs
pnpm build:general-docs
```

文档站使用 VitePress。首页可使用 Vue 组件做视觉呈现，正文仍使用 Markdown。

## 约定

- 业务页面优先使用现有 Vben 和 Ant Design Vue 组件。
- 权限判断走统一 access 能力。
- API 类型放在 `src/api/core`。
- 文档内容只写入 `apps/web-general-docs`。
