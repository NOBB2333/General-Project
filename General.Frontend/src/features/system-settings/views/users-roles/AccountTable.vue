<script setup lang="ts">
import { Edit3, KeyRound, LockKeyhole } from 'lucide-vue-next';
import type { UserAccountRow } from '../../model/users-roles.types';

defineProps<{ rows: UserAccountRow[] }>();
const emit = defineEmits<{
  view: [row: UserAccountRow];
  lock: [row: UserAccountRow];
  reset: [row: UserAccountRow];
}>();

function initial(name: string) {
  return name.slice(0, 1);
}
</script>

<template>
  <div :class="$style.tableFrame">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>用户</th>
          <th>账号/邮箱</th>
          <th>部门</th>
          <th>角色</th>
          <th>状态</th>
          <th>最近登录</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-account-id="row.id"
        >
          <td>
            <div :class="$style.userCell">
              <span :class="$style.avatar">{{ initial(row.name) }}</span>
              <span :class="$style.userCopy">
                <strong>{{ row.name }}</strong>
                <small>{{ row.jobTitle }}</small>
              </span>
            </div>
          </td>
          <td>
            <span :class="$style.accountCopy">
              <strong>{{ row.account }}</strong>
              <small>{{ row.email }}</small>
            </span>
          </td>
          <td>{{ row.department }}</td>
          <td>
            <span :class="[$style.roleBadge, $style[`role${row.role}`]]">{{ row.role }}</span>
          </td>
          <td>
            <span :class="[$style.statusBadge, $style[`status${row.status}`]]">{{ row.status }}</span>
          </td>
          <td :class="$style.lastLogin">{{ row.lastLogin }}</td>
          <td>
            <div :class="$style.rowActions">
              <button
                title="编辑账号"
                aria-label="编辑账号"
                @click="emit('view', row)"
              >
                <Edit3
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
              <button
                :title="row.status === '禁用' ? '启用账号' : '停用账号'"
                :aria-label="row.status === '禁用' ? '启用账号' : '停用账号'"
                @click="emit('lock', row)"
              >
                <LockKeyhole
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
              <button
                title="重置密码"
                aria-label="重置密码"
                @click="emit('reset', row)"
              >
                <KeyRound
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
            </div>
          </td>
        </tr>
        <tr v-if="rows.length === 0">
          <td
            colspan="7"
            :class="$style.emptyState"
          >
            没有找到匹配的账号
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableFrame {
  @apply min-w-[1100px] overflow-hidden border bg-white;
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
  @apply w-[130px];
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
.emptyState {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}
</style>
