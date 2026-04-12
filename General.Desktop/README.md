# General.Desktop

`General.Desktop` 是桌面客户端壳项目。

当前定位：

- 基于 Avalonia 构建跨平台桌面壳
- 后续通过 WebView 承载 `General.Web`
- 负责系统通知、文件选择、下载桥接、本地缓存、自动更新等桌面能力

边界原则：

- 业务页面不在桌面端重复实现
- 业务逻辑仍统一收敛到 `General.Web` 与 `General.Admin`
- 桌面端只补足浏览器环境不方便处理的本地能力

当前状态：

- 已完成基础项目初始化
- 已完成桌面壳首页占位
- WebView 具体接入包与宿主桥接留在后续阶段实现
