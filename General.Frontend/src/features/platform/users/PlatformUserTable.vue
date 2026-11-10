<script setup lang="ts">
import { Eye, KeyRound, Pencil, Power, Trash2 } from 'lucide-vue-next';
import type { UserApi } from '@/api/core/user';
import { UiCheckbox } from '@/components/ui';
import { formatMonitoringDate } from '../monitoring/monitoring.model';

const props = withDefaults(
  defineProps<{
    canDelete: (user: UserApi.ListItem) => boolean;
    canManage: boolean;
    isWritable: (user: UserApi.ListItem) => boolean;
    loading: boolean;
    rows: UserApi.ListItem[];
    selectable?: boolean;
    selectedIds?: ReadonlySet<string>;
  }>(),
  {
    selectable: false,
    selectedIds: () => new Set<string>(),
  },
);
const emit = defineEmits<{
  delete: [user: UserApi.ListItem];
  edit: [user: UserApi.ListItem];
  resetPassword: [user: UserApi.ListItem];
  select: [userId: string, selected: boolean];
  setActive: [user: UserApi.ListItem, isActive: boolean];
  view: [user: UserApi.ListItem];
}>();

function initial(user: UserApi.ListItem) {
  return (user.displayName || user.username).slice(0, 1);
}

function statusLabel(user: UserApi.ListItem) {
  return user.isActive ? '启用' : '禁用';
}

function formatLastLogin(value: null | string) {
  return formatMonitoringDate(value, { dateStyle: 'medium', timeStyle: 'short' });
}
</script>

<template>
  <div
    :class="$style.tableFrame"
    :aria-busy="loading"
  >
    <table :class="[$style.table, props.selectable && $style.selectable]">
      <thead>
        <tr>
          <th v-if="props.selectable">选择</th>
          <th>用户</th>
          <th>账号/邮箱</th>
          <th>组织</th>
          <th>角色</th>
          <th>状态</th>
          <th>最近登录</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="user in rows"
          :key="user.id"
          :data-user-id="user.id"
        >
          <td v-if="props.selectable">
            <UiCheckbox
              :model-value="props.selectedIds.has(user.id)"
              :disabled="loading"
              :label="`选择${user.displayName || user.username}`"
              @update:model-value="emit('select', user.id, $event === true)"
            />
          </td>
          <td>
            <div :class="$style.userCell">
              <span :class="$style.avatar">{{ initial(user) }}</span>
              <span :class="$style.userCopy">
                <strong>{{ user.displayName || user.username }}</strong>
                <small>{{ user.employeeNo || user.phoneNumber || '—' }}</small>
              </span>
            </div>
          </td>
          <td>
            <span :class="$style.accountCopy">
              <strong>{{ user.username }}</strong>
              <small>{{ user.email || '暂无邮箱' }}</small>
            </span>
          </td>
          <td>{{ user.organizationUnitNames.join('、') || '—' }}</td>
          <td>
            <span
              v-if="user.roles.length"
              :class="$style.roleList"
            >
              <span
                v-for="role in user.roles"
                :key="role"
                :class="$style.roleBadge"
                >{{ role }}</span
              >
            </span>
            <span v-else>—</span>
          </td>
          <td>
            <span :class="[$style.statusBadge, user.isActive ? $style.status启用 : $style.status禁用]">{{
              statusLabel(user)
            }}</span>
          </td>
          <td :class="$style.lastLogin">{{ formatLastLogin(user.lastLoginTime) }}</td>
          <td>
            <div :class="$style.rowActions">
              <button
                type="button"
                title="查看用户详情"
                :aria-label="`查看${user.username}详情`"
                :disabled="loading"
                @click="emit('view', user)"
              >
                <Eye
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
              <template v-if="canManage && isWritable(user)">
                <button
                  type="button"
                  title="编辑用户"
                  :aria-label="`编辑${user.username}`"
                  :disabled="loading"
                  @click="emit('edit', user)"
                >
                  <Pencil
                    :size="15"
                    :stroke-width="1.8"
                  />
                </button>
                <button
                  type="button"
                  :title="user.isActive ? '停用用户' : '启用用户'"
                  :aria-label="`${user.isActive ? '停用' : '启用'}${user.username}`"
                  :disabled="loading"
                  @click="emit('setActive', user, !user.isActive)"
                >
                  <Power
                    :size="15"
                    :stroke-width="1.8"
                  />
                </button>
                <button
                  type="button"
                  title="重置用户密码"
                  :aria-label="`重置${user.username}的密码`"
                  :disabled="loading"
                  @click="emit('resetPassword', user)"
                >
                  <KeyRound
                    :size="15"
                    :stroke-width="1.8"
                  />
                </button>
                <button
                  type="button"
                  :title="canDelete(user) ? '删除用户' : '当前登录账号不允许删除'"
                  :aria-label="`删除${user.username}`"
                  :disabled="loading || !canDelete(user)"
                  @click="emit('delete', user)"
                >
                  <Trash2
                    :size="15"
                    :stroke-width="1.8"
                  />
                </button>
              </template>
              <span
                v-else-if="canManage"
                :class="$style.readOnlyAction"
                title="跨租户列表仅供查看；请进入目标租户后维护用户。"
              >
                当前视图只读
              </span>
            </div>
          </td>
        </tr>
        <tr v-if="loading">
          <td
            :colspan="props.selectable ? 8 : 7"
            :class="$style.emptyState"
            role="status"
          >
            正在读取用户…
          </td>
        </tr>
        <tr v-else-if="rows.length === 0">
          <td
            :colspan="props.selectable ? 8 : 7"
            :class="$style.emptyState"
          >
            没有找到匹配的用户
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../styles/index.css";

.tableFrame {
  @apply min-w-[1100px] overflow-x-auto border bg-white;
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
  @apply w-[220px];
}
.table th:nth-child(2) {
  @apply w-[270px];
}
.table th:nth-child(3) {
  @apply w-[190px];
}
.table th:nth-child(4) {
  @apply w-[160px];
}
.table th:nth-child(5) {
  @apply w-[130px];
}
.table th:nth-child(6) {
  @apply w-[180px];
}
.table th:nth-child(7) {
  @apply w-[180px];
}
.selectable th:nth-child(1) {
  @apply w-[54px];
}
.selectable th:nth-child(2) {
  @apply w-[220px];
}
.selectable th:nth-child(3) {
  @apply w-[245px];
}
.selectable th:nth-child(4) {
  @apply w-[170px];
}
.selectable th:nth-child(5) {
  @apply w-[160px];
}
.selectable th:nth-child(6) {
  @apply w-[130px];
}
.selectable th:nth-child(7) {
  @apply w-[180px];
}
.selectable th:nth-child(8) {
  @apply w-[180px];
}
.table td {
  @apply h-[69px] px-4;
  color: var(--ui-text-muted);
  border-bottom: 1px solid var(--ui-border-subtle);
}
.table tbody tr:last-child td {
  border-bottom: 0;
}
.table tbody tr:hover {
  background: var(--ui-surface-hover);
}
.userCell {
  @apply flex items-center gap-3;
}
.avatar {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-[12px] font-semibold text-white;
  background: var(--app-primary);
}
.userCopy,
.accountCopy {
  @apply flex min-w-0 flex-col;
}
.userCopy strong,
.accountCopy strong {
  @apply truncate text-[13px] font-semibold;
  color: var(--ui-text);
}
.userCopy small,
.accountCopy small {
  @apply mt-0.5 truncate text-[11px];
  color: #97a0af;
}
.accountCopy strong {
  color: var(--ui-text-muted);
}
.roleBadge,
.statusBadge {
  @apply inline-flex w-fit rounded px-2 py-1 text-[10px] font-semibold;
}
.roleList {
  @apply flex flex-wrap gap-1;
}
.roleBadge {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.role管理员 {
  color: white;
  background: #ff5630;
}
.role项目经理 {
  color: white;
  background: var(--app-primary);
}
.role销售经理 {
  color: white;
  background: #6554c0;
}
.role财务 {
  color: white;
  background: #ff8b00;
}
.role普通员工 {
  color: white;
  background: #6554c0;
}
.role仓库管理员 {
  color: white;
  background: #00875a;
}
.status启用 {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.status待激活 {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status禁用 {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.lastLogin {
  color: #7a869a;
}
.rowActions {
  @apply flex items-center gap-3;
}
.rowActions button {
  @apply rounded p-1 transition-colors;
  color: var(--ui-text-subtle);
}
.rowActions button:hover {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.rowActions button:disabled {
  @apply cursor-not-allowed opacity-40;
}
.readOnlyAction {
  @apply whitespace-nowrap text-[10px];
  color: var(--ui-text-subtle);
}
.emptyState {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}
</style>
