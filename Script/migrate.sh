#!/usr/bin/env bash
set -euo pipefail

# 用法：
#   ./migrate.sh pg AddProjectSprintTable       生成 PostgreSql migration 并跑 DbMigrator
#   ./migrate.sh sqlite AddProjectSprintTable   生成 SQLite migration 并跑 DbMigrator
#   ./migrate.sh pg                             只跑 DbMigrator，不生成 migration
#
# 说明：
#   本脚本不删库。改完实体后先执行带名字的命令生成 migration，再执行 DbMigrator。
#   如果只想重新跑种子（没改实体），可以只传 provider，不传 migration 名字。

PROJECT_DIR="./General.Admin/src/General.Admin.EntityFrameworkCore"
STARTUP_PROJECT="./General.Admin/src/General.Admin.DbMigrator"

PROVIDER="${1:-}"
NAME="${2:-}"

if [[ -z "$PROVIDER" ]]; then
    echo "错误：必须指定数据库 provider"
    echo "用法："
    echo "  ./migrate.sh pg <MigrationName>      # PostgreSql"
    echo "  ./migrate.sh sqlite <MigrationName>  # SQLite"
    echo "  ./migrate.sh pg                      # 只跑 DbMigrator"
    exit 1
fi

case "$PROVIDER" in
    pg|postgres|postgresql)
        CONTEXT="AdminPostgreSqlMigrationsDbContext"
        OUTPUT_DIR="Migrations/PostgreSql"
        ;;
    sqlite|sql)
        CONTEXT="AdminSqliteMigrationsDbContext"
        OUTPUT_DIR="Migrations/Sqlite"
        ;;
    *)
        echo "错误：不支持的 provider: $PROVIDER"
        echo "支持的值：pg / sqlite"
        exit 1
        ;;
esac

if [[ -n "$NAME" ]]; then
    echo "正在生成 $PROVIDER migration: $NAME"
    dotnet ef migrations add "$NAME" \
        --project "$PROJECT_DIR" \
        --startup-project "$STARTUP_PROJECT" \
        --context "$CONTEXT" \
        --output-dir "$OUTPUT_DIR"
fi

echo "正在运行 DbMigrator..."
dotnet run --project "$STARTUP_PROJECT"
