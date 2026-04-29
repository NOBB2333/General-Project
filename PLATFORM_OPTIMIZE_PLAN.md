# Platform 模块优化计划

> 参考 Admin.NET 对比分析，按优先级排列待办事项。

---

## 优先级说明

- **P0**：核心缺失，影响日常使用，优先实现
- **P1**：重要改进，提升系统质量
- **P2**：增强功能，按需实现

---

## P0 — 配置参数管理模块

**目标**：支持运行时动态修改系统参数，无需重启服务。

**现状**：数据库连接、JWT过期时间等配置硬编码在 `appsettings.json`，运行时无法调整。

### 需要创建/修改的文件

- [ ] **Entity**: `General.Admin.Domain/Platform/AppConfig.cs`
  - 字段：`Name`, `Code`(唯一索引), `Value`, `GroupCode`, `SysFlag`(是否系统参数), `Remark`
  - 继承 `FullAuditedEntity<Guid>`
- [ ] **DTO**: `General.Admin.Application.Contracts/Platform/PlatformConfigDto.cs`
  - 输入：`PlatformConfigSaveInput`, `PlatformConfigGetInput`
  - 输出：`PlatformConfigDto`
- [ ] **Service接口**: `General.Admin.Application.Contracts/Platform/IPlatformConfigService.cs`
  - `GetListAsync(query)` / `GetByKeyAsync(key)` / `CreateAsync(input)` / `UpdateAsync(id, input)` / `DeleteAsync(id)`
- [ ] **Service实现**: `General.Admin.Application/Platform/PlatformConfigService.cs`
  - 使用 `IDistributedCache` 缓存配置值
  - `GetConfigValue<T>(key)` 泛型获取方法
  - 修改/删除时自动清除缓存
- [ ] **Controller**: `General.Admin.HttpApi.Host/Controllers/ConfigController.cs`
  - 权限：`Platform.Config.Manage`
- [ ] **权限注册**: `AdminPermissions.cs` + `AdminPermissionDefinitionProvider.cs`
  - 新增：`Platform.Config.Manage`
- [ ] **DB Migration**: `AdminDbContextBase.cs` 中注册 `DbSet<AppConfig>`

---

## P0 — 字典管理模块

**目标**：统一管理前端下拉框、状态枚举等数据，支持多租户隔离。

**现状**：前端下拉框/状态码硬编码在前端代码中，无统一管理入口。

### 需要创建/修改的文件

- [ ] **Entity**: `General.Admin.Domain/Platform/AppDictType.cs` + `AppDictData.cs`
  - `AppDictType`: `Code`(唯一索引), `Name`, `IsTenant`(是否租户级)
  - `AppDictData`: `DictTypeId`(外键), `Label`, `Value`, `Sort`, `Remark`
  - 均继承 `FullAuditedEntity<Guid>`
- [ ] **DTO**:
  - `PlatformDictTypeDto.cs` / `PlatformDictTypeSaveInput.cs`
  - `PlatformDictDataDto.cs` / `PlatformDictDataSaveInput.cs`
- [ ] **Service接口**: `IPlatformDictService.cs`
  - 字典类型：CRUD + 按Code查询
  - 字典数据：按DictTypeCode获取列表（前端用）
  - 批量获取：`GetDictItemsAsync(dictTypeCodes[])`
- [ ] **Service实现**: `PlatformDictService.cs`
  - 缓存策略：按DictTypeCode缓存，修改时失效
  - 租户字典数据隔离
- [ ] **Controller**: `DictController.cs`
  - 权限：`Platform.Dict.Manage`
- [ ] **权限注册**: `Platform.Dict.Manage`
- [ ] **DB Migration**: 注册 `DbSet<AppDictType>`, `DbSet<AppDictData>`

---

## P0 — 菜单/权限/组织架构缓存

**目标**：高频读取数据加缓存，提升登录和接口鉴权性能。

**现状**：每次登录都查数据库获取菜单树，每次授权都查角色权限。

### 需要修改的文件

- [ ] **PlatformMenuService.cs**
  - 菜单树查询结果缓存（按角色ID缓存，TTL 30min）
  - 菜单变更时清除相关角色缓存
  - 使用 `IDistributedCache` 或 ABP `IDistributedCache<T>`
- [ ] **PlatformRoleService.cs**
  - 角色权限数据缓存（权限分配后缓存）
  - `GetRolePermissionsAsync()` 优先从缓存读取
- [ ] **PlatformOrganizationUnitService.cs**
  - 组织架构树缓存（变更频率低，TTL 1h）
  - `GetOrganizationTreeAsync()` 优先从缓存读取

---

## P1 — 软删除启用

**目标**：防止误删数据，支持数据恢复。

**现状**：`FullAuditedAggregateRoot` 已包含 `IsDeleted` 字段，但删除操作均为物理删除，查询过滤器未启用 `ISoftDelete`。

### 需要修改的文件

- [ ] **EntityFrameworkCore/AdminDbContextBase.cs**
  - 启用全局软删除过滤器：`modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted)`
  - 或在Repository层使用 `IDataFilter<ISoftDelete>`
- [ ] **各Service的Delete方法**
  - `PlatformUserService` / `PlatformRoleService` / `PlatformMenuService` 等
  - 将物理删除改为软删除：`await _repository.UpdateAsync(entity)`（设置IsDeleted=true）
  - 注意：`IdentityRole`/`IdentityUser` 是ABP内置实体，需确认软删除兼容性
- [ ] **新增数据恢复接口**（可选）
  - `RestoreAsync(id)` — 恢复已删除数据
  - `GetDeletedListAsync()` — 查看回收站

---

## P1 — 文件存储抽象

**目标**：支持本地/阿里云OSS/MinIO多存储源切换。

**现状**：仅支持本地文件系统存储在 `App_Data/upload-files`。

### 需要创建/修改的文件

- [x] **接口定义**: `General.Admin.Domain/Platform/FileStorage/IPlatformFileStorageProvider.cs`
  ```csharp
  public interface IFileStorageProvider
  {
      Task<string> UploadAsync(Stream stream, string fileName, string contentType);
      Task<Stream> DownloadAsync(string fileKey);
      Task<bool> DeleteAsync(string fileKey);
      Task<string> GetPresignedUrlAsync(string fileKey, TimeSpan expiry);
  }
- [x] **本地存储实现**: `LocalPlatformFileStorageProvider.cs`
  - 重构现有 `FileController` 中的逻辑
- [x] **OSS存储实现**（可选）: `AliyunOssPlatformFileStorageProvider.cs`
  - 引入 `Aliyun.OSS.SDK.NetCore` NuGet包
- [x] **MinIO存储实现**（可选）: `MinioPlatformFileStorageProvider.cs`
  - 引入 `Minio` NuGet包
- [x] **配置**: `appsettings.json` 新增 `FileStorage` 配置节
  - `Provider`: Local / AliyunOSS / MinIO
  - 对应 provider 的连接配置
- [x] **FileController.cs** 重构
  - 注入 `IFileStorageProvider` 替代硬编码本地路径

---

## P1 — 定时任务增强

**目标**：任务持久化、执行记录、集群支持。

**现状**：Quartz调度正常，但执行记录未持久化，重启后任务状态丢失，多实例部署存在重复执行风险。

### 需要创建/修改的文件

- [x] **Entity**: `General.Admin.Domain/Platform/AppScheduledJobRecord.cs`
  - 字段：`JobKey`, `StartTime`, `EndTime`, `Status`(Running/Success/Failed), `Result`, `ErrorMessage`
  - 继承 `CreationAuditedEntity<Guid>`
- [x] **PlatformSchedulerService.cs** 修改
  - `ExecuteJobAsync()` 执行前后写入记录表
  - `GetJobRecordsAsync(jobKey, query)` 查询执行历史
- [x] **Quartz持久化配置**（可选）
  - `appsettings.json` 配置 Quartz JobStore 使用数据库
  - 或自行实现简单持久化（记录到 `AppScheduledJobRecord`）
  - 当前采用 ABP DDD 持久化：任务定义、触发器、执行记录、节点心跳进入业务表，Quartz 仅作为 Cron 解析能力使用。
- [x] **集群去重**（可选）
  - Quartz内置支持：配置 `quartz.jobStore.clustered = true`
  - 或使用分布式锁（`IDistributedLock`）
  - 当前采用业务表执行锁 + 节点心跳，避免直接引入 Quartz ADO JobStore 绕开领域模型。

---

## P2 — API限流

**目标**：防止接口被恶意刷取。

### 需要修改的文件

- [ ] **AdminHttpApiHostModule.cs** 或 **Program.cs**
  - 启用 ABP 内置限流：`app.UseRateLimiting()`
  - 配置限流规则（全局 + 特定接口）
- [ ] **appsettings.json**
  - 新增限流配置节

---

## P2 — 开放接口认证

**目标**：支持三方系统对接，AccessKey/SecretKey 签名认证。

**前置条件**：有三方系统对接需求时再实现。

### 需要创建/修改的文件

- [ ] **Entity**: `General.Admin.Domain/Platform/AppOpenAccess.cs`
  - 字段：`AccessKey`, `AccessSecret`(加密存储), `BindUserId`, `BindTenantId`, `Remark`
- [ ] **Service**: `PlatformOpenAccessService.cs`
  - 签名生成：HMAC-SHA256
  - 签名验证：时间戳 + Nonce 防重放
- [ ] **认证中间件**: `SignatureAuthenticationHandler.cs`
  - 实现 ASP.NET Core Authentication Handler
- [ ] **Controller**: `OpenAccessController.cs`
  - 权限：`Platform.OpenAccess.Manage`

---

## P2 — 在线用户实时监控（SignalR）

**目标**：实时查看在线用户状态，支持强制下线、消息推送。

### 需要创建/修改的文件

- [ ] **Hub**: `OnlineUserHub.cs`
  - 连接/断开事件管理
  - 在线用户列表广播
- [ ] **Service**: `PlatformOnlineUserService.cs`
  - 在线用户列表（从 `PlatformUserActivityService` 扩展）
  - 强制下线（通知Hub断开连接）
- [ ] **前端**: 接入 SignalR 客户端

---

## P2 — 健康检查增强

**目标**：监控数据库、缓存、外部依赖的健康状态。

### 需要修改的文件

- [ ] **Program.cs** / **AdminHttpApiHostModule.cs**
  - 添加 `AspNetCore.HealthChecks.NpgSql`（PostgreSQL）
  - 添加 `AspNetCore.HealthChecks.Redis`（Redis，接入后）
- [ ] **appsettings.json**
  - 配置健康检查端点

---

## 执行顺序建议

```
第一批（核心缺失，1-2周）：
  ├── 配置参数管理模块（P0）
  ├── 字典管理模块（P0）
  └── 菜单/权限/组织架构缓存（P0）

第二批（质量提升，1-2周）：
  ├── 软删除启用（P1）
  ├── 文件存储抽象（P1）
  └── 定时任务增强（P1）

第三批（按需实现）：
  ├── API限流（P2）
  ├── 开放接口认证（P2）
  ├── SignalR在线用户（P2）
  └── 健康检查增强（P2）
```
