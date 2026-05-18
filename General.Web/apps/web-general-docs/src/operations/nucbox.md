# NUC 部署

NUC 部署用于局域网内的轻量服务器场景。

## 常用命令

```bash
./Script/package-all.sh
python3 ./Script/deploy-to-linux.py lan --init
```

如果只需要执行数据库迁移：

```bash
python3 ./Script/deploy-to-linux.py lan --migrate-only
```

## SQLite 位置

远端 SQLite 通常位于后端发布目录的 `data` 或 `build/data` 下，实际位置以进程环境变量和配置文件为准。

排查时可以查看运行进程环境：

```bash
tr '\0' '\n' < /proc/<pid>/environ
```

重点确认：

- `ConnectionStrings__Default`
- `Database__Provider`
- `GENERAL_ADMIN_DB_FILE`
- `GENERAL_ADMIN_DATA_DIR`

## 常见现象

`--init` 后数据没变，通常是因为部署脚本保留了远端 SQLite。此时应先跑迁移确认种子是否能补齐，而不是直接删库。

项目列表为空时，先查 `AppProjects` 和 `AppProjectMembers` 是否有数据，再查权限和数据范围。
