<script setup lang="ts">
import { MoreHorizontal } from 'lucide-vue-next';
import { UiButton, UiDropdownMenu } from '@/components/ui';
import type { UiDropdownMenuItem } from '@/components/ui';
import type { FundAccount, FundAccountAction } from '../../model/fund-accounts.types';

defineProps<{ accounts: readonly FundAccount[] }>();
const emit = defineEmits<{ action: [action: FundAccountAction, account: FundAccount] }>();

function menuItems(account: FundAccount): UiDropdownMenuItem[] {
  return [
    { id: 'view', label: '查看账户详情' },
    { id: 'edit', label: '编辑账户' },
    { id: 'opening', label: account.hasOpeningBalance ? '修改期初余额' : '设置期初余额' },
    { id: 'copy', label: '复制账户' },
    {
      id: 'toggle-status',
      label: account.status === '冻结' ? '恢复正常' : '冻结账户',
      separatorBefore: true,
    },
    { id: 'delete', label: '删除账户', tone: 'danger' },
  ];
}

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}
</script>

<template>
  <div
    data-fund-account-table
    :class="$style.tableShell"
  >
    <table :class="$style.table">
      <thead>
        <tr>
          <th>账户名称</th>
          <th>类型 / 用途</th>
          <th>账号</th>
          <th>开户行 / 平台</th>
          <th :class="$style.amountColumn">余额</th>
          <th>状态</th>
          <th :class="$style.actionColumn">操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="account in accounts"
          :key="account.id"
          :data-account-id="account.id"
        >
          <td>
            <strong :class="$style.accountName">{{ account.name }}</strong>
            <span :class="$style.company">{{ account.company }}</span>
          </td>
          <td>
            <span :class="$style.kind">{{ account.kind }}</span>
            <span :class="$style.category">{{ account.category }}</span>
          </td>
          <td :class="$style.accountNumber">{{ account.accountNumber }}</td>
          <td>
            <span :class="$style.branch">{{ account.branch }}</span>
            <small :class="$style.purpose">{{ account.purpose }}</small>
          </td>
          <td :class="$style.amountColumn">
            <strong :class="$style.balance">{{ money(account.balance) }}</strong>
            <small
              v-if="account.frozenBalance"
              :class="$style.frozen"
              >冻结 {{ money(account.frozenBalance) }}</small
            >
          </td>
          <td>
            <span
              :class="[
                $style.status,
                {
                  [$style.statusFrozen]: account.status === '冻结',
                  [$style.statusClosed]: account.status === '注销',
                },
              ]"
              >{{ account.status }}</span
            >
          </td>
          <td :class="$style.actionColumn">
            <UiDropdownMenu
              :items="menuItems(account)"
              :label="`${account.name}更多操作`"
              @select="emit('action', $event as FundAccountAction, account)"
            >
              <template #trigger>
                <UiButton
                  variant="ghost"
                  size="icon-sm"
                  :aria-label="`${account.name}更多操作`"
                >
                  <MoreHorizontal
                    :size="16"
                    :stroke-width="1.8"
                  />
                </UiButton>
              </template>
            </UiDropdownMenu>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableShell {
  @apply overflow-x-auto border bg-white;
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}

.table {
  @apply w-full min-w-[1100px] border-collapse text-left text-[12px];
}

.table th {
  @apply border-b px-4 py-3 text-[11px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
  border-color: var(--ui-border);
}

.table td {
  @apply border-b px-4 py-3 align-middle;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}

.table tbody tr:last-child td {
  border-bottom: 0;
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.accountName,
.company,
.branch,
.purpose {
  @apply block;
}

.accountName {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}

.company,
.purpose {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}

.kind,
.category,
.status {
  @apply mr-1 inline-flex rounded px-1.5 py-0.5 text-[10px] font-medium;
}

.kind {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.category,
.status {
  color: #006644;
  background: #e3fcef;
}

.statusFrozen {
  color: #974f0c;
  background: #fff7e6;
}

.statusClosed {
  color: #6b778c;
  background: #ebecf0;
}

.accountNumber {
  @apply font-mono text-[11px];
}

.balance {
  @apply block font-semibold;
  color: var(--ui-text);
}

.frozen {
  @apply mt-1 block text-[10px];
  color: #ff5630;
}

.amountColumn {
  @apply text-right;
}

.actionColumn {
  @apply w-16 text-center;
}
</style>
