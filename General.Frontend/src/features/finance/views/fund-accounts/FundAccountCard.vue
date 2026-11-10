<script setup lang="ts">
import {
  Banknote,
  Building2,
  CircleDollarSign,
  LockKeyhole,
  MoreHorizontal,
  Smartphone,
  WalletCards,
} from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiDropdownMenu } from '@/components/ui';
import type { UiDropdownMenuItem } from '@/components/ui';
import type { FundAccount, FundAccountAction } from '../../model/fund-accounts.types';

const props = defineProps<{ account: FundAccount }>();
const emit = defineEmits<{ action: [action: FundAccountAction, account: FundAccount] }>();

const icon = computed(() => {
  if (props.account.tone === 'wechat') return Smartphone;
  if (props.account.tone === 'alipay') return CircleDollarSign;
  if (props.account.tone === 'cash') return Banknote;
  if (props.account.tone === 'bank') return Building2;
  return WalletCards;
});

const menuItems = computed<UiDropdownMenuItem[]>(() => [
  { id: 'view', label: '查看账户详情' },
  { id: 'edit', label: '编辑账户' },
  { id: 'opening', label: props.account.hasOpeningBalance ? '修改期初余额' : '设置期初余额' },
  { id: 'copy', label: '复制账户' },
  {
    id: 'toggle-status',
    label: props.account.status === '冻结' ? '恢复正常' : '冻结账户',
    separatorBefore: true,
  },
  { id: 'delete', label: '删除账户', tone: 'danger' },
]);

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function selectAction(id: string) {
  emit('action', id as FundAccountAction, props.account);
}
</script>

<template>
  <article
    data-fund-account-card
    :data-account-id="account.id"
    :class="[$style.accountCard, $style[account.tone]]"
  >
    <header :class="$style.cardHeader">
      <span :class="$style.accountIcon">
        <component
          :is="icon"
          :size="20"
          :stroke-width="1.8"
        />
      </span>
      <div :class="$style.identity">
        <div :class="$style.titleRow">
          <h2 :class="$style.accountName">{{ account.name }}</h2>
          <span :class="[$style.tag, $style.kindTag]">{{ account.kind }}</span>
          <span
            v-if="account.kind === '银行账户' || account.kind === '承兑汇票账户'"
            :class="[
              $style.tag,
              $style.categoryTag,
              {
                [$style.categoryPrimary]: account.category === '基本户',
                [$style.categoryPurple]: account.category === '专用户',
                [$style.categoryNeutral]: account.category === '临时户',
              },
            ]"
            >{{ account.category }}</span
          >
          <span
            :class="[
              $style.tag,
              $style.statusTag,
              {
                [$style.statusFrozen]: account.status === '冻结',
                [$style.statusClosed]: account.status === '注销',
              },
            ]"
            >{{ account.status }}</span
          >
        </div>
        <p :class="$style.company">{{ account.company }}</p>
      </div>
      <UiDropdownMenu
        :items="menuItems"
        :label="`${account.name}更多操作`"
        @select="selectAction"
      >
        <template #trigger>
          <UiButton
            variant="ghost"
            size="icon-sm"
            :title="`${account.name}更多操作`"
            :aria-label="`${account.name}更多操作`"
            :class="$style.moreButton"
          >
            <MoreHorizontal
              :size="16"
              :stroke-width="1.8"
            />
          </UiButton>
        </template>
      </UiDropdownMenu>
    </header>

    <div :class="$style.accountMeta">
      <strong :class="$style.accountNumber">{{ account.accountNumber }}</strong>
      <span :class="$style.branch">{{ account.branch }}</span>
    </div>

    <div :class="$style.balanceArea">
      <div>
        <span :class="$style.balanceLabel">余额</span>
        <strong :class="$style.balanceValue">{{ money(account.balance) }}</strong>
      </div>
      <p
        v-if="account.frozenBalance"
        :class="$style.frozenBalance"
      >
        <LockKeyhole
          :size="12"
          :stroke-width="1.8"
        />
        冻结 {{ money(account.frozenBalance) }}
        <span v-if="account.frozenNote">（{{ account.frozenNote }}）</span>
      </p>
    </div>

    <div :class="$style.openingRow">
      <span :class="$style.openingIcon"><WalletCards :size="12" /></span>
      <template v-if="account.hasOpeningBalance">
        期初 {{ money(account.openingBalance ?? 0) }}
        <span :class="$style.dot">·</span>
        {{ account.openingDate }}
      </template>
      <button
        v-else
        type="button"
        :class="$style.openingAction"
        @click="emit('action', 'opening', account)"
      >
        尚未录入期初余额，立即建账
      </button>
    </div>
    <p :class="$style.purpose">{{ account.purpose }}</p>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.accountCard {
  @apply relative min-w-0 overflow-hidden border bg-white px-4 pb-3 pt-[15px];
  min-height: 204px;
  color: var(--ui-text);
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}

.accountCard::before {
  @apply absolute inset-x-0 top-0 h-[3px];
  content: '';
  background: var(--app-primary);
}

.wechat::before {
  background: #22c55e;
}

.alipay::before {
  background: #1677ff;
}

.cash::before {
  background: #ff8b00;
}

.other::before {
  background: #6554c0;
}

.cardHeader {
  @apply flex min-w-0 items-start gap-3;
}

.accountIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.wechat .accountIcon {
  color: #00875a;
  background: #e3fcef;
}

.alipay .accountIcon {
  color: #1677ff;
  background: #e8f3ff;
}

.cash .accountIcon {
  color: #974f0c;
  background: #fff7e6;
}

.identity {
  @apply min-w-0 flex-1;
}

.titleRow {
  @apply flex min-w-0 flex-wrap items-center gap-1.5;
}

.accountName {
  @apply mr-0.5 truncate text-[14px] font-semibold;
}

.tag {
  @apply inline-flex shrink-0 items-center rounded px-1.5 py-0.5 text-[10px] font-medium;
}

.kindTag {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.categoryTag {
  color: #006644;
  background: #e3fcef;
}

.categoryPrimary {
  color: #0052cc;
  background: #deebff;
}

.categoryPurple {
  color: #5243aa;
  background: #eae6ff;
}

.categoryNeutral {
  color: #6b778c;
  background: #ebecf0;
}

.statusTag {
  color: #006644;
  background: #e3fcef;
}

.wechat .kindTag {
  color: #00875a;
  background: #e3fcef;
}

.alipay .kindTag {
  color: #0065ff;
  background: #e8f3ff;
}

.cash .kindTag {
  color: #974f0c;
  background: #fff7e6;
}

.statusFrozen {
  color: #974f0c;
  background: #fff7e6;
}

.statusClosed {
  color: #6b778c;
  background: #ebecf0;
}

.company {
  @apply mt-1 truncate text-[12px];
  color: var(--ui-text-subtle);
}

.moreButton {
  @apply -mr-1 -mt-1 shrink-0 opacity-0 transition-opacity;
}

.accountCard:hover .moreButton,
.accountCard:focus-within .moreButton {
  @apply opacity-100;
}

.accountMeta {
  @apply mt-2.5 flex flex-col gap-0.5;
}

.accountNumber {
  @apply font-mono text-[12px] font-semibold tracking-wide;
  color: var(--ui-text-muted);
}

.branch {
  @apply truncate text-[11px];
  color: var(--ui-text-subtle);
}

.balanceArea {
  @apply mt-2.5 flex min-w-0 items-end justify-between gap-3 border-t pt-2.5;
  border-color: var(--ui-border-subtle);
}

.balanceLabel {
  @apply block text-[10px];
  color: var(--ui-text-subtle);
}

.balanceValue {
  @apply mt-0.5 block text-[18px] font-semibold leading-6;
  color: var(--ui-text);
}

.frozenBalance {
  @apply flex min-w-0 items-center justify-end gap-1 truncate pb-0.5 text-[10px];
  color: #ff5630;
}

.frozenBalance span {
  color: #6554c0;
}

.openingRow {
  @apply mt-2 flex min-w-0 items-center gap-1 truncate text-[10px];
  color: var(--ui-text-subtle);
}

.openingIcon {
  color: #974f0c;
}

.dot {
  @apply px-0.5;
  color: var(--ui-border);
}

.openingAction {
  @apply truncate text-left font-medium;
  color: #974f0c;
}

.openingAction:hover {
  text-decoration: underline;
}

.purpose {
  @apply mt-2 truncate text-[10px];
  color: var(--ui-text-subtle);
}

@media (prefers-reduced-motion: reduce) {
  .openingAction {
    transition: none;
  }
}
</style>
