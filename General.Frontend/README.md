# General Admin 管理端

这是独立的 Vue 3 + TypeScript + Vite 前端工程。应用源码、页面组件、路由、布局与样式直接迁入自 `/Users/wong/Code/_Github_my/Chao_fisemap`，原参考仓库保持只读；本工程不依赖 `General.Web` workspace。

## 本地开发

```bash
pnpm install --frozen-lockfile
cp .env.example .env.local
pnpm dev
```

默认绑定 `0.0.0.0:5011`。Vite 会按 `VITE_API_PROXY_TARGET` 配置 `/api`、`/health/live` 与 `/health/ready` 的开发代理。开发和生产构建共用真实Router、登录守卫及菜单白名单；未登录时回到登录页，后端菜单未登记的页面不开放。参考源的fixture和模拟操作不属于已接入能力，生产入口只能通过真实General.Admin认证、权限和菜单数据访问已验收页面。接口与页面适配按 `.doc/08-前端重建/02-分阶段待办清单.md` 逐步完成。

## 环境变量

| 变量                    | 含义                          |
| ----------------------- | ----------------------------- |
| `VITE_APP_NAME`         | 浏览器标题/应用名称           |
| `VITE_ASSET_BASE`       | Vite 静态资源部署路径         |
| `VITE_ROUTER_BASE`      | Vue Router history base       |
| `VITE_API_BASE_URL`     | API请求前缀                   |
| `VITE_API_PROXY_TARGET` | 本地开发API代理地址，不含密钥 |

实际本机设置放在被Git忽略的 `.env.local`；不要提交密钥或生产环境私密配置。

## 静态部署与后端代理

生产构建时使用同源API前缀 `VITE_API_BASE_URL=/api`，再将 `dist/` 内容部署到静态站点根目录。浏览器只访问同源 `/api`，后端地址由反向代理在服务端配置，不写入前端构建产物。

`deploy/default.conf.template` 可用于官方 Nginx 容器的模板配置目录 `/etc/nginx/templates/`。启动容器时设置 `GENERAL_API_UPSTREAM` 为 General.Admin HTTP Host 的内部地址（例如 `http://127.0.0.1:5010`），并将 `NGINX_ENVSUBST_FILTER=^GENERAL_API_UPSTREAM$` 限制为只替换该变量；将构建后的 `dist/` 挂载到 `/usr/share/nginx/html/`。模板将深层前端路由回退到 `index.html`，HTML不缓存，带哈希的 `/assets/` 资源长期缓存，并把 `/api/`、`/health/live` 与 `/health/ready` 转发到后端。生产密钥不得写入 `VITE_*` 变量。

## 检查与构建

```bash
pnpm lint
pnpm format:check
pnpm typecheck
pnpm typecheck:active
pnpm check:boundaries
pnpm test:unit
pnpm build
pnpm preview
```

工程保留迁入源的 CSS Modules、Tailwind 与 UI 组件样式。前端视觉基线不主动重做；适配业务时优先替换 API、身份、菜单和数据来源。

`pnpm typecheck` 检查完整迁入源码；当前还会覆盖未接入 General.Admin 的 Chao_fisemap 示例模块，94项历史诊断已记入重建台账，目标是在 G12 清零。`pnpm typecheck:active` 以严格模式检查已接入 General.Admin 的平台、集成、项目、经营和 AI 页面及其共享代码，是本工程 CI 当前执行的类型检查门禁。

## 接入新业务页面

按[新模块接入与 CI 指南](../.doc/08-前端重建/08-新模块接入与CI指南.md)从最接近的 Chao_fisemap 页面复制或扩展，保留页面结构、控件、交互和样式，再将请求、DTO、权限码及路由映射到 General.Admin 的真实契约。每个步骤都要先核对后端 Contracts/应用服务和当前迁入代码；后端没有的能力先登记缺口，不在前端编造接口、成功状态或业务字段。

GitHub Actions 在 `General.Frontend/` 内独立安装和检查，不依赖旧 `General.Web` workspace。CI 当前执行格式检查、lint、架构边界、单元测试、已接入模块类型检查和生产构建；G12 完成全量迁入源码类型清理后，再将全量类型检查加入必过门禁。
