# web-general

这是 `General.Web` 下面为你单独新建的业务 app。

当前处理方式：

- 以 `web-antd` 为基础复制出一份可独立运行的 app
- 保留模板现有工程能力，便于后续继续跟模板升级
- 已将首页与分析页替换为你的项目占位内容

当前入口：

- 项目名：`@general/web-general`
- 开发命令：`pnpm run dev:general`
- 构建命令：`pnpm run build:general`

当前说明：

- 开发环境默认仍保留模板 mock
- 后续接入 `General.Admin` 真接口时，再切换接口代理与鉴权流程



> .env.desktop 是不是没用


• 不是没用，但它只对 dist 模式有用，对 dev 模式没用。

  区分一下：

  - dev 模式
    desktop 加载的是 http://localhost:5677/workspace
    这时候用的是前端开发服务器，读的是 .env.development
  - dist 模式
    desktop 加载的是本地 dist/index.html#/workspace
    这时候不是走端口了，而是走本地文件 file://...
    所以必须有一套桌面专用构建配置，把资源路径改成相对路径
    这就是 .env.desktop 的作用

  你可以直接这么理解：

  - 你如果平时只用 dev，那 .env.desktop 你暂时感知不到
  - 但你一旦要用 sh Script/start-desktop.sh dist 或 package-all.sh，它就有用了
  - 没它的话，本地 dist 很容易出现 js/css 路径不对，表现就是“能打开但没样式/没脚本”

  所以结论：

  - 对 dev 来说，.env.desktop 可以不管
  - 对 dist 来说，.env.desktop 是有必要的

  如果你想，我也可以把它改名成更直白的，比如 .env.desktop-build，这样语义更清楚。