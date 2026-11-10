<script setup lang="ts">
import {
  Archive,
  CreditCard,
  Download,
  Expand,
  Grid2X2,
  List,
  Plus,
  Search,
  TrendingUp,
  WalletCards,
  X,
} from 'lucide-vue-next';
import { computed, watch } from 'vue';
import { UiAlertDialog, UiButton, UiDialog, UiSelect, UiTooltip } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { FundAccount, FundAccountAction, FundAccountKind, FundAccountStatus } from '../model/fund-accounts.types';
import FundAccountCard from './fund-accounts/FundAccountCard.vue';
import FundAccountTable from './fund-accounts/FundAccountTable.vue';
import { useFundAccountsPage } from './fund-accounts/useFundAccountsPage';

const {
  summary,
  query,
  kind,
  status,
  layout,
  operationMessage,
  filteredAccounts,
  missingOpeningAccounts,
  editorOpen,
  editingId,
  accountDraft,
  openingOpen,
  openingAccountId,
  openingAmount,
  openingDate,
  openingAccount,
  detailOpen,
  detailAccount,
  deleteOpen,
  pendingDeleteAccount,
  resetFilters,
  openCreate,
  saveAccount,
  openOpening,
  saveOpeningBalance,
  runAccountAction,
  confirmDelete,
  exportAccounts,
} = useFundAccountsPage();

const kindOptions: readonly UiSelectOption[] = [
  { value: '全部类型', label: '全部类型' },
  { value: '银行账户', label: '银行账户' },
  { value: '承兑汇票账户', label: '承兑汇票账户' },
  { value: '微信', label: '微信' },
  { value: '支付宝', label: '支付宝' },
  { value: '现金账户', label: '现金账户' },
  { value: '其他', label: '其他' },
];

const statusOptions: readonly UiSelectOption[] = [
  { value: '全部状态', label: '全部状态' },
  { value: '正常', label: '正常' },
  { value: '冻结', label: '冻结' },
  { value: '注销', label: '注销' },
];

const editorKindOptions = kindOptions.filter((option) => option.value !== '全部类型');
const editorStatusOptions = statusOptions.filter((option) => option.value !== '全部状态');
const openingAccountOptions = computed<UiSelectOption[]>(() => {
  const accounts = [...missingOpeningAccounts.value];
  if (openingAccount.value && !accounts.some((account) => account.id === openingAccount.value?.id)) {
    accounts.unshift(openingAccount.value);
  }
  return accounts.map((account) => ({ value: account.id, label: account.name }));
});

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

let dialogReturnTarget: HTMLElement | null = null;

function rememberReturnTarget(target: EventTarget | null) {
  if (target instanceof HTMLElement) dialogReturnTarget = target;
}

function accountMenuTrigger(accountId: string) {
  const escapedId = CSS.escape(accountId);
  return document.querySelector<HTMLElement>(`[data-account-id="${escapedId}"] button[aria-label$="更多操作"]`);
}

function handleAccountAction(action: FundAccountAction, account: FundAccount) {
  const activeElement = document.activeElement;
  const accountCard = activeElement instanceof HTMLElement ? activeElement.closest('[data-account-id]') : null;
  dialogReturnTarget =
    accountCard?.getAttribute('data-account-id') === account.id
      ? (activeElement as HTMLElement)
      : accountMenuTrigger(account.id);
  runAccountAction(action, account);
}

function handleOpenOpening(event: MouseEvent) {
  rememberReturnTarget(event.currentTarget);
  openOpening();
}

function restoreDialogFocus() {
  const target = dialogReturnTarget;
  dialogReturnTarget = null;
  window.setTimeout(() => {
    if (target?.isConnected) target.focus();
    else document.querySelector<HTMLElement>('[data-fund-account-create]')?.focus();
  });
}

watch([editorOpen, openingOpen, detailOpen, deleteOpen], (current, previous) => {
  if (previous.some((wasOpen, index) => wasOpen && !current[index])) restoreDialogFocus();
});
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="cards"
    data-page-label="资金账户"
    data-page-module="财务管理"
    data-page-layout="fund-account-cards"
    data-page-route="/finance/fund-accounts"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <p :class="$style.breadcrumb">财务管理 <span>›</span> 资金账户</p>
        <div :class="$style.heroTitleRow">
          <span :class="$style.heroIcon">
            <Expand
              :size="22"
              :stroke-width="1.8"
            />
          </span>
          <div>
            <h1 :class="$style.title">资金账户管理</h1>
            <p :class="$style.subtitle">管理银行、微信、支付宝、现金等全部资金账户</p>
          </div>
        </div>
      </div>
      <div
        aria-hidden="true"
        :class="$style.heroArtwork"
      >
        <span :class="$style.heroBand" />
        <span :class="$style.heroCross" />
      </div>
    </header>

    <section :class="$style.openingNotice">
      <Archive
        :size="19"
        :stroke-width="1.8"
        :class="$style.noticeIcon"
      />
      <div :class="$style.noticeCopy">
        <strong>有 {{ missingOpeningAccounts.length }} 个账户尚未录入期初余额</strong>
        <p>系统启用日的初始余额将作为后续银行流水的起算基础。点击账户卡片右上菜单中的“设置期初余额”完成建账。</p>
      </div>
      <UiButton
        size="sm"
        variant="warning"
        :class="$style.setupButton"
        @click="handleOpenOpening"
      >
        <template #leading>
          <Archive
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        立即建账
      </UiButton>
    </section>

    <section
      aria-label="资金账户指标"
      :class="$style.metrics"
    >
      <article :class="$style.metricCard">
        <div :class="$style.metricHeader">
          <span>账户总余额 (CNY)</span>
          <span :class="[$style.metricIcon, $style.metricIconBlue]">
            <CreditCard
              :size="18"
              :stroke-width="1.8"
            />
          </span>
        </div>
        <strong :class="$style.metricValue">{{ money(summary.totalBalance) }}</strong>
      </article>
      <article :class="$style.metricCard">
        <div :class="$style.metricHeader">
          <span>可用余额 (CNY)</span>
          <span :class="[$style.metricIcon, $style.metricIconGreen]">
            <TrendingUp
              :size="18"
              :stroke-width="1.8"
            />
          </span>
        </div>
        <strong :class="[$style.metricValue, $style.availableValue]">{{ money(summary.availableBalance) }}</strong>
      </article>
      <article :class="$style.metricCard">
        <div :class="$style.metricHeader">
          <span>活跃账户</span>
          <span :class="[$style.metricIcon, $style.metricIconPurple]">
            <WalletCards
              :size="18"
              :stroke-width="1.8"
            />
          </span>
        </div>
        <strong :class="$style.metricValue"
          >{{ summary.activeCount }}<small :class="$style.metricDenominator">/ {{ summary.totalCount }}</small></strong
        >
      </article>
      <article :class="$style.metricCard">
        <div :class="$style.metricHeader">
          <span>余额趋势</span>
          <span :class="[$style.metricIcon, $style.metricIconBlue]">
            <TrendingUp
              :size="18"
              :stroke-width="1.8"
            />
          </span>
        </div>
        <svg
          viewBox="0 0 260 72"
          role="img"
          aria-label="余额总体呈上升趋势"
          :class="$style.trendChart"
        >
          <defs>
            <linearGradient
              id="fundTrendFill"
              x1="0"
              y1="0"
              x2="0"
              y2="1"
            >
              <stop
                offset="0%"
                stop-color="#4C9AFF"
                stop-opacity="0.24"
              />
              <stop
                offset="100%"
                stop-color="#4C9AFF"
                stop-opacity="0"
              />
            </linearGradient>
          </defs>
          <path
            d="M2 66 C24 60 42 54 58 46 C76 36 86 19 104 17 C124 15 135 34 153 38 C172 42 189 39 202 39 C218 39 228 22 242 18 C249 16 254 17 258 18 L258 72 L2 72 Z"
            fill="url(#fundTrendFill)"
          />
          <path
            d="M2 66 C24 60 42 54 58 46 C76 36 86 19 104 17 C124 15 135 34 153 38 C172 42 189 39 202 39 C218 39 228 22 242 18 C249 16 254 17 258 18"
            :class="$style.trendLine"
          />
        </svg>
      </article>
    </section>

    <section :class="$style.toolbar">
      <label :class="$style.searchField">
        <Search
          :size="16"
          :stroke-width="1.8"
        />
        <input
          v-model="query"
          type="search"
          placeholder="搜索账户名称、账号、持有人..."
          aria-label="搜索资金账户"
        />
        <button
          v-if="query"
          type="button"
          aria-label="清空搜索"
          :class="$style.clearSearch"
          @click="query = ''"
        >
          <X
            :size="14"
            :stroke-width="1.8"
          />
        </button>
      </label>
      <UiSelect
        v-model="kind"
        :options="kindOptions"
        label="账户类型"
        :class="$style.filterSelect"
      />
      <UiSelect
        v-model="status"
        :options="statusOptions"
        label="账户状态"
        :class="$style.filterSelectCompact"
      />
      <span :class="$style.toolbarSpacer" />
      <div
        role="group"
        aria-label="账户展示方式"
        :class="$style.layoutSwitch"
      >
        <UiTooltip content="卡片视图">
          <button
            type="button"
            aria-label="卡片视图"
            :aria-pressed="layout === 'cards'"
            :class="[$style.layoutButton, layout === 'cards' && $style.layoutButtonActive]"
            @click="layout = 'cards'"
          >
            <Grid2X2
              :size="16"
              :stroke-width="1.8"
            />
          </button>
        </UiTooltip>
        <UiTooltip content="表格视图">
          <button
            type="button"
            aria-label="表格视图"
            :aria-pressed="layout === 'table'"
            :class="[$style.layoutButton, layout === 'table' && $style.layoutButtonActive]"
            @click="layout = 'table'"
          >
            <List
              :size="16"
              :stroke-width="1.8"
            />
          </button>
        </UiTooltip>
      </div>
      <UiButton
        variant="secondary"
        size="sm"
        @click="exportAccounts"
      >
        <template #leading>
          <Download
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        导出
      </UiButton>
      <UiButton
        data-fund-account-create
        variant="primary"
        size="sm"
        @click="openCreate"
      >
        <template #leading>
          <Plus
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        新增账户
      </UiButton>
    </section>

    <p
      v-if="operationMessage"
      role="status"
      aria-live="polite"
      :class="$style.operationMessage"
    >
      {{ operationMessage }}
    </p>

    <section
      v-if="filteredAccounts.length && layout === 'cards'"
      aria-label="资金账户卡片"
      :class="$style.accountGrid"
    >
      <FundAccountCard
        v-for="account in filteredAccounts"
        :key="account.id"
        :account="account"
        @action="handleAccountAction"
      />
    </section>

    <FundAccountTable
      v-else-if="filteredAccounts.length"
      :accounts="filteredAccounts"
      @action="handleAccountAction"
    />

    <section
      v-else
      :class="$style.emptyState"
    >
      <WalletCards
        :size="34"
        :stroke-width="1.8"
      />
      <strong>暂无匹配的资金账户</strong>
      <p>请调整搜索关键词或筛选条件后重试。</p>
      <UiButton
        variant="secondary"
        size="sm"
        @click="resetFilters"
        >重置筛选</UiButton
      >
    </section>

    <UiDialog
      v-model:open="editorOpen"
      :title="editingId ? '编辑资金账户' : '新增资金账户'"
      description="账户信息仅保存在当前本地复刻页面中"
      size="lg"
    >
      <form
        :class="$style.formGrid"
        @submit.prevent="saveAccount"
      >
        <label :class="$style.formField">
          <span>账户名称</span>
          <input
            v-model="accountDraft.name"
            required
            placeholder="例如：中国工商银行"
          />
        </label>
        <label :class="$style.formField">
          <span>账户类型</span>
          <UiSelect
            :model-value="accountDraft.kind"
            :options="editorKindOptions"
            label="账户类型"
            @update:model-value="accountDraft.kind = $event as FundAccountKind"
          />
        </label>
        <label :class="$style.formField">
          <span>账户用途</span>
          <input
            v-model="accountDraft.category"
            placeholder="例如：基本户"
          />
        </label>
        <label :class="$style.formField">
          <span>账户状态</span>
          <UiSelect
            :model-value="accountDraft.status"
            :options="editorStatusOptions"
            label="账户状态"
            @update:model-value="accountDraft.status = $event as FundAccountStatus"
          />
        </label>
        <label :class="[$style.formField, $style.formFieldWide]">
          <span>所属企业</span>
          <input v-model="accountDraft.company" />
        </label>
        <label :class="$style.formField">
          <span>持有人</span>
          <input
            v-model="accountDraft.holder"
            placeholder="账户持有人"
          />
        </label>
        <label :class="$style.formField">
          <span>账号</span>
          <input
            v-model="accountDraft.accountNumber"
            required
            placeholder="银行卡号或平台账号"
          />
        </label>
        <label :class="$style.formField">
          <span>开户行 / 平台</span>
          <input v-model="accountDraft.branch" />
        </label>
        <label :class="$style.formField">
          <span>当前余额</span>
          <input
            v-model="accountDraft.balance"
            inputmode="decimal"
          />
        </label>
        <label :class="[$style.formField, $style.formFieldWide]">
          <span>账户说明</span>
          <input
            v-model="accountDraft.purpose"
            placeholder="例如：主要经营账户"
          />
        </label>
      </form>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="editorOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          @click="saveAccount"
          >保存账户</UiButton
        >
      </template>
    </UiDialog>

    <UiDialog
      v-model:open="openingOpen"
      title="设置期初余额"
      description="期初余额将作为后续银行流水的起算基础"
      size="sm"
    >
      <div :class="$style.openingForm">
        <label :class="$style.formField">
          <span>资金账户</span>
          <UiSelect
            v-model="openingAccountId"
            :options="openingAccountOptions"
            label="选择待建账账户"
          />
        </label>
        <label :class="$style.formField">
          <span>期初余额</span>
          <input
            v-model="openingAmount"
            inputmode="decimal"
            placeholder="0.00"
          />
        </label>
        <label :class="$style.formField">
          <span>启用日期</span>
          <input
            v-model="openingDate"
            type="date"
          />
        </label>
        <p :class="$style.openingHint">当前账户：{{ openingAccount?.name ?? '请选择账户' }}</p>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="openingOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          @click="saveOpeningBalance"
          >完成建账</UiButton
        >
      </template>
    </UiDialog>

    <UiDialog
      v-model:open="detailOpen"
      :title="detailAccount?.name ?? '账户详情'"
      description="资金账户本地详情"
      size="md"
    >
      <dl
        v-if="detailAccount"
        :class="$style.detailGrid"
      >
        <div>
          <dt>账户类型</dt>
          <dd>{{ detailAccount.kind }} · {{ detailAccount.category }}</dd>
        </div>
        <div>
          <dt>账户状态</dt>
          <dd>{{ detailAccount.status }}</dd>
        </div>
        <div>
          <dt>账号</dt>
          <dd>{{ detailAccount.accountNumber }}</dd>
        </div>
        <div>
          <dt>开户行 / 平台</dt>
          <dd>{{ detailAccount.branch }}</dd>
        </div>
        <div>
          <dt>账户余额</dt>
          <dd>{{ money(detailAccount.balance) }}</dd>
        </div>
        <div>
          <dt>持有人</dt>
          <dd>{{ detailAccount.holder }}</dd>
        </div>
        <div :class="$style.detailWide">
          <dt>所属企业</dt>
          <dd>{{ detailAccount.company }}</dd>
        </div>
        <div :class="$style.detailWide">
          <dt>账户说明</dt>
          <dd>{{ detailAccount.purpose }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="detailOpen = false"
          >关闭</UiButton
        >
      </template>
    </UiDialog>

    <UiAlertDialog
      v-model:open="deleteOpen"
      title="删除资金账户"
      :description="`确定删除账户「${pendingDeleteAccount?.name ?? ''}」吗？此操作仅影响当前本地页面。`"
      confirm-label="确认删除"
      tone="danger"
      @confirm="confirmDelete"
    />
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-w-0 pb-8;
  margin-top: 20px;
  margin-inline: 21px;
  color: var(--ui-text);
}

.hero {
  @apply relative flex min-h-[120px] overflow-hidden px-6 py-4;
  color: white;
  background: linear-gradient(108deg, #2c355e 0%, #3d4b7c 45%, #6079b2 76%, #b18e18 100%);
  border-radius: 18px;
}

.heroCopy {
  @apply relative z-10 flex flex-col justify-center;
}

.breadcrumb {
  @apply mb-5 text-[12px] font-semibold;
  color: color-mix(in srgb, white 72%, transparent);
}

.breadcrumb span {
  @apply px-1.5;
  color: color-mix(in srgb, white 42%, transparent);
}

.heroTitleRow {
  @apply flex items-center gap-3;
}

.heroIcon {
  @apply flex h-11 w-11 items-center justify-center border;
  color: white;
  background: color-mix(in srgb, white 10%, transparent);
  border-color: color-mix(in srgb, white 25%, transparent);
  border-radius: 9px;
}

.title {
  @apply text-[25px] font-semibold leading-8;
}

.subtitle {
  @apply mt-1 text-[13px];
  color: color-mix(in srgb, white 78%, transparent);
}

.heroArtwork {
  @apply pointer-events-none absolute inset-y-0 right-0 w-[59.4%];
}

.heroArtwork::after {
  @apply absolute inset-0;
  content: '';
  background: linear-gradient(90deg, color-mix(in srgb, #9fb5d4 45%, transparent), transparent 62%);
  opacity: 0.55;
}

.heroBand {
  @apply absolute inset-y-0 left-[3%] w-[25.5%];
  background: color-mix(in srgb, #c2d0df 30%, transparent);
  box-shadow:
    -23px 0 0 color-mix(in srgb, #8da3c2 30%, transparent),
    23px 0 0 color-mix(in srgb, #223b78 28%, transparent);
}

.heroCross,
.heroCross::after {
  @apply absolute rounded-full;
  content: '';
  background: #172d68;
}

.heroCross {
  @apply left-[28%] top-1/2 h-3.5 w-14 -translate-x-1/2 -translate-y-1/2;
}

.heroCross::after {
  @apply left-1/2 top-1/2 h-14 w-3.5 -translate-x-1/2 -translate-y-1/2;
}

.openingNotice {
  @apply flex min-h-[63px] items-center gap-3 border px-4 py-2;
  margin-top: 13px;
  color: var(--ui-text);
  background: #fffbea;
  border-color: #ffe380;
  border-radius: var(--app-surface-radius);
}

.noticeIcon {
  @apply shrink-0;
  color: #974f0c;
}

.noticeCopy {
  @apply min-w-0 flex-1;
}

.noticeCopy strong {
  @apply text-[13px] font-semibold;
}

.noticeCopy p {
  @apply mt-1 text-[11px] leading-5;
  color: var(--ui-text-muted);
}

.setupButton {
  background: #9a4d00;
}

.setupButton:hover {
  background: #7a3d00;
}

.metrics {
  @apply grid grid-cols-4 gap-4;
  margin-top: 14px;
}

.metricCard {
  @apply min-w-0 border bg-white px-4 py-4;
  min-height: 118px;
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}

.metricHeader {
  @apply flex items-center justify-between gap-3 text-[11px];
  color: var(--ui-text-subtle);
}

.metricIcon {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded;
}

.metricIconBlue {
  color: #0052cc;
  background: #deebff;
}

.metricIconGreen {
  color: #006644;
  background: #e3fcef;
}

.metricIconPurple {
  color: #5243aa;
  background: #eae6ff;
}

.metricValue {
  @apply mt-3 block truncate text-[23px] font-semibold leading-7;
  color: var(--ui-text);
}

.availableValue {
  color: #006644;
}

.metricDenominator {
  @apply ml-1 text-[13px] font-medium;
  color: var(--ui-text-subtle);
}

.trendChart {
  @apply mt-1 h-[52px] w-full overflow-visible;
}

.trendLine {
  fill: none;
  stroke: #0052cc;
  stroke-width: 2;
  stroke-linecap: round;
}

.toolbar {
  @apply flex min-h-[60px] items-center gap-3 border bg-white px-4 py-2;
  margin-top: 20px;
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}

.searchField {
  @apply flex h-9 w-[320px] items-center gap-2 border bg-white px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.searchField:focus-within {
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 1px var(--ui-focus);
}

.searchField input {
  @apply min-w-0 flex-1 border-0 bg-transparent text-[12px] outline-none;
  color: var(--ui-text);
}

.searchField input::placeholder {
  color: var(--ui-text-subtle);
}

.searchField input::-webkit-search-cancel-button {
  display: none;
}

.clearSearch {
  @apply rounded p-1;
  color: var(--ui-text-subtle);
}

.clearSearch:hover {
  background: var(--ui-surface-hover);
}

.filterSelect {
  @apply h-9 min-w-[120px];
}

.filterSelectCompact {
  @apply h-9 min-w-[96px];
}

.toolbarSpacer {
  @apply min-w-2 flex-1;
}

.layoutSwitch {
  @apply inline-flex h-9 overflow-hidden border bg-white;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.layoutButton {
  @apply flex h-full w-9 items-center justify-center border-r transition-colors last:border-r-0;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}

.layoutButton:hover {
  background: var(--ui-surface-hover);
}

.layoutButtonActive {
  color: white;
  background: var(--app-primary);
}

.layoutButtonActive:hover {
  background: var(--app-primary-hover);
}

.operationMessage {
  @apply mt-3 border px-3 py-2 text-[11px];
  color: #006644;
  background: #e3fcef;
  border-color: #abf5d1;
  border-radius: var(--app-control-radius);
}

.accountGrid {
  @apply mt-4 grid grid-cols-3 gap-3;
}

.emptyState {
  @apply mt-4 flex min-h-64 flex-col items-center justify-center gap-2 border bg-white p-8 text-center;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}

.emptyState strong {
  @apply mt-2 text-[14px];
  color: var(--ui-text);
}

.emptyState p {
  @apply mb-2 text-[12px];
}

.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}

.formField {
  @apply flex min-w-0 flex-col gap-1.5;
}

.formField > span {
  @apply text-[11px] font-semibold;
  color: var(--ui-text-muted);
}

.formField input {
  @apply h-9 w-full border bg-white px-3 text-[12px] outline-none;
  color: var(--ui-text);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.formField input:focus {
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 1px var(--ui-focus);
}

.formField [data-ui-select] {
  @apply w-full;
}

.formFieldWide {
  @apply col-span-2;
}

.openingForm {
  @apply grid gap-4 p-5;
}

.openingHint {
  @apply border px-3 py-2 text-[11px];
  color: var(--ui-text-muted);
  background: var(--ui-surface-subtle);
  border-color: var(--ui-border-subtle);
  border-radius: var(--app-control-radius);
}

.detailGrid {
  @apply grid grid-cols-2 gap-3 p-5;
}

.detailGrid div {
  @apply border p-3;
  border-color: var(--ui-border-subtle);
  border-radius: var(--app-control-radius);
}

.detailGrid dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply mt-1 text-[12px] font-medium;
  color: var(--ui-text);
}

.detailWide {
  @apply col-span-2;
}

@media (max-width: 1180px) {
  .metrics {
    @apply grid-cols-2;
  }

  .accountGrid {
    @apply grid-cols-2;
  }

  .searchField {
    @apply min-w-48 flex-1;
  }

  .toolbarSpacer {
    @apply hidden;
  }
}

@media (max-width: 820px) {
  .page {
    margin: 0;
  }

  .hero {
    @apply min-h-[136px] px-5;
  }

  .heroArtwork {
    @apply opacity-40;
  }

  .openingNotice,
  .toolbar {
    @apply flex-wrap;
  }

  .setupButton {
    @apply ml-8;
  }

  .metrics,
  .accountGrid {
    @apply grid-cols-1;
  }

  .searchField {
    @apply w-full;
  }
}

@media (prefers-reduced-motion: reduce) {
  .layoutButton,
  .clearSearch {
    transition: none;
  }
}
</style>
