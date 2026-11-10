<script setup lang="ts">
import { KeyRound, LogIn, Pencil, Power, ShieldCheck, Trash2 } from 'lucide-vue-next';
import type { TenantApi } from '@/api/core/tenant';

defineProps<{
  canEnter: boolean;
  canManage: boolean;
  entering: boolean;
  loading: boolean;
  rows: TenantApi.ListItem[];
}>();

const emit = defineEmits<{
  authorize: [tenant: TenantApi.ListItem];
  delete: [tenant: TenantApi.ListItem];
  edit: [tenant: TenantApi.ListItem];
  enter: [tenant: TenantApi.ListItem];
  setActive: [tenant: TenantApi.ListItem, isActive: boolean];
}>();

function formatDate(value: string) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '—' : new Intl.DateTimeFormat('zh-CN', { dateStyle: 'medium' }).format(date);
}

function isProtected(tenant: TenantApi.ListItem) {
  return tenant.name.trim().toLocaleLowerCase() === 'default';
}
</script>

<template>
  <div
    :class="$style.tableFrame"
    :aria-busy="loading"
  >
    <table :class="$style.table">
      <thead>
        <tr>
          <th>租户</th>
          <th>租户管理员</th>
          <th>访问配置</th>
          <th>数据库</th>
          <th>创建时间</th>
          <th>状态</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="tenant in rows"
          :key="tenant.id"
          :data-tenant-id="tenant.id"
        >
          <td>
            <div :class="$style.tenantCell">
              <span :class="$style.avatar">{{ Array.from(tenant.name)[0] ?? '租' }}</span>
              <span :class="$style.primaryCopy">
                <strong>{{ tenant.name }}</strong>
                <small>{{ tenant.id }}</small>
              </span>
              <span
                v-if="isProtected(tenant)"
                :class="$style.protectedTag"
                >默认</span
              >
            </div>
          </td>
          <td>
            <span :class="$style.primaryCopy">
              <strong>{{ tenant.adminUserName || '未指定' }}</strong>
              <small>{{ tenant.adminEmail || '暂无邮箱' }}</small>
            </span>
          </td>
          <td>
            <span :class="$style.accessBadges">
              <span
                :class="[$style.badge, tenant.hasExplicitAuthorization ? $style.badgePrimary : $style.badgeNeutral]"
              >
                {{ tenant.hasExplicitAuthorization ? '已配置权限' : '默认权限' }}
              </span>
              <span
                v-if="tenant.apiBlacklist.length"
                :class="$style.badgeWarning"
                >限制 {{ tenant.apiBlacklist.length }}</span
              >
            </span>
          </td>
          <td>
            <span :class="$style.databaseCopy">
              <strong>{{ tenant.hasDefaultConnectionString ? '独立连接' : '平台默认' }}</strong>
              <small>{{ tenant.defaultConnectionStringDisplay || '使用平台配置' }}</small>
            </span>
          </td>
          <td :class="$style.dateCell">{{ formatDate(tenant.creationTime) }}</td>
          <td>
            <span :class="[$style.statusBadge, tenant.isActive ? $style.statusActive : $style.statusInactive]">
              {{ tenant.isActive ? '启用' : '停用' }}
            </span>
          </td>
          <td>
            <div :class="$style.rowActions">
              <button
                v-if="canEnter"
                type="button"
                :aria-label="`进入${tenant.name}`"
                title="进入租户代操作"
                :disabled="loading || entering || !tenant.isActive"
                @click="emit('enter', tenant)"
              >
                <LogIn
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
              <template v-if="canManage">
                <button
                  type="button"
                  :aria-label="`配置${tenant.name}授权`"
                  title="菜单、API和管理员"
                  :disabled="loading"
                  @click="emit('authorize', tenant)"
                >
                  <KeyRound
                    :size="15"
                    :stroke-width="1.8"
                  />
                </button>
                <button
                  type="button"
                  :aria-label="`编辑${tenant.name}`"
                  title="编辑租户"
                  :disabled="loading"
                  @click="emit('edit', tenant)"
                >
                  <Pencil
                    :size="15"
                    :stroke-width="1.8"
                  />
                </button>
                <button
                  type="button"
                  :aria-label="`${tenant.isActive ? '停用' : '启用'}${tenant.name}`"
                  :title="tenant.isActive ? '停用租户' : '启用租户'"
                  :disabled="loading"
                  @click="emit('setActive', tenant, !tenant.isActive)"
                >
                  <Power
                    :size="15"
                    :stroke-width="1.8"
                  />
                </button>
                <button
                  type="button"
                  :aria-label="`删除${tenant.name}`"
                  :title="isProtected(tenant) ? '默认租户不可删除' : '删除租户及租户数据'"
                  :disabled="loading || isProtected(tenant)"
                  @click="emit('delete', tenant)"
                >
                  <Trash2
                    :size="15"
                    :stroke-width="1.8"
                  />
                </button>
              </template>
              <span
                v-if="!canManage && !canEnter"
                :class="$style.readOnly"
              >
                <ShieldCheck
                  :size="14"
                  :stroke-width="1.8"
                />
                只读
              </span>
            </div>
          </td>
        </tr>
        <tr v-if="loading">
          <td
            colspan="7"
            :class="$style.emptyState"
            role="status"
          >
            正在读取租户…
          </td>
        </tr>
        <tr v-else-if="rows.length === 0">
          <td
            colspan="7"
            :class="$style.emptyState"
          >
            没有找到匹配的租户
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.tableFrame {
  @apply min-w-[1000px] overflow-x-auto border bg-white;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.table {
  @apply w-full table-fixed border-collapse text-[12px];
}
.table th {
  @apply h-[43px] px-4 text-left text-[11px] font-semibold;
  color: var(--ui-text-muted);
  background: #f4f5f7;
  border-bottom: 1px solid var(--ui-border);
}
.table th:nth-child(1) {
  @apply w-[245px];
}
.table th:nth-child(2) {
  @apply w-[190px];
}
.table th:nth-child(3) {
  @apply w-[155px];
}
.table th:nth-child(4) {
  @apply w-[160px];
}
.table th:nth-child(5) {
  @apply w-[130px];
}
.table th:nth-child(6) {
  @apply w-[85px];
}
.table th:nth-child(7) {
  @apply w-[195px];
}
.table td {
  @apply h-[68px] px-4;
  color: var(--ui-text-muted);
  border-bottom: 1px solid var(--ui-border-subtle);
}
.table tbody tr:last-child td {
  border-bottom: 0;
}
.table tbody tr:hover {
  background: var(--ui-surface-hover);
}
.tenantCell {
  @apply flex min-w-0 items-center gap-2.5;
}
.avatar {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-[12px] font-semibold text-white;
  background: var(--app-primary);
}
.primaryCopy,
.databaseCopy {
  @apply flex min-w-0 flex-col;
}
.primaryCopy strong,
.databaseCopy strong {
  @apply truncate text-[12px] font-semibold;
  color: var(--ui-text);
}
.primaryCopy small,
.databaseCopy small {
  @apply mt-0.5 truncate text-[10px];
  color: var(--ui-text-subtle);
}
.protectedTag {
  @apply shrink-0 rounded px-1.5 py-0.5 text-[9px] font-semibold;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.accessBadges {
  @apply flex flex-wrap gap-1;
}
.badge,
.badgeWarning,
.statusBadge {
  @apply inline-flex w-fit rounded px-2 py-1 text-[9px] font-semibold;
}
.badgePrimary {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.badgeNeutral {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.badgeWarning {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.statusActive {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.statusInactive {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.dateCell {
  @apply text-[11px];
}
.rowActions {
  @apply flex items-center gap-1;
}
.rowActions button {
  @apply inline-flex h-7 w-7 items-center justify-center rounded transition-colors hover:bg-white disabled:cursor-not-allowed disabled:opacity-40;
  color: var(--ui-text-muted);
}
.rowActions button:hover:not(:disabled) {
  color: var(--app-primary);
  box-shadow: 0 0 0 1px var(--ui-border);
}
.readOnly {
  @apply inline-flex items-center gap-1 text-[10px];
  color: var(--ui-text-subtle);
}
.emptyState {
  @apply h-20 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
