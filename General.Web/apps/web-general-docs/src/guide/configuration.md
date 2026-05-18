# 配置文件

General Admin 后端配置拆成多个 `.jsonc` 文件，按文件名排序加载。编号只表达加载顺序，不表达业务优先级。

## 运行时配置

后端 API 配置目录：

```text
General.Admin/src/General.Admin.HttpApi.Host/appsettings
```

当前文件顺序：

- `01-app.jsonc`：应用地址、前端地址、CORS、API 文档入口和 OpenAPI 分组。
- `02-auth-server.jsonc`：AuthServer 地址、HTTPS 元数据要求、Swagger ClientId。
- `03-jwt.jsonc`：轻量 JWT 的 issuer、audience、过期时间和签名密钥。
- `04-connection-strings.jsonc`：数据库类型和默认连接字符串。
- `05-string-encryption.jsonc`：字符串加密默认短语。
- `06-logging.jsonc`：日志通道、保留数量、日志级别覆盖。
- `07-file-storage.jsonc`：本地文件、阿里云 OSS、MinIO 的文件存储配置。
- `08-platform-hardening.jsonc`：限流、可信代理、登录和开放接口保护。

## 迁移器配置

数据库迁移器配置目录：

```text
General.Admin/src/General.Admin.DbMigrator/appsettings
```

当前文件顺序：

- `01-openiddict.jsonc`：OpenIddict 应用种子配置。
- `02-connection-strings.jsonc`：迁移器使用的数据库类型和连接字符串。

迁移器必须连接到和后端一致的数据库。切换 SQLite、PostgreSQL、MySQL 时，两边的 `Database:Provider` 和 `ConnectionStrings:Default` 要同步。

## 加载规则

配置加载由 `AddJsoncAppSettingsDirectory` 完成。它会读取指定目录下的 `.json` 和 `.jsonc` 文件，并按文件路径字典序加载。

因此文件名使用 `01`、`02`、`03` 这类前缀，能明确表达加载顺序。后加载的文件会覆盖前面相同 key 的值。

## 环境变量覆盖

部署脚本会通过环境变量覆盖部分配置，例如：

- `Database__Provider`
- `ConnectionStrings__Default`
- `App__SelfUrl`
- `App__ClientUrl`
- `App__CorsOrigins`
- `AuthServer__Authority`

.NET 配置里的 `:` 在环境变量中写成双下划线。生产环境建议优先通过环境变量或安全配置注入数据库连接、JWT 密钥和对象存储密钥。

## 本地开发建议

SQLite 开发环境可以保留默认连接：

```jsonc
"Default": "Data Source=build/data/general-admin.db"
```

切换 PostgreSQL 时，修改 API 和 Migrator 两处连接配置，并先运行迁移器：

```bash
cd General.Admin
dotnet run --project src/General.Admin.DbMigrator
```

## 部署注意

`Script/deploy-to-linux.py --init` 会同步应用并执行迁移，但不会自动删除远端 `data/` 下的 SQLite 文件。已有数据库会被保留，种子逻辑负责补齐默认数据。

如果服务器上的连接串和仓库不同，应把对应配置文件加入部署脚本排除规则，或改用环境变量注入，避免发布时覆盖生产配置。
