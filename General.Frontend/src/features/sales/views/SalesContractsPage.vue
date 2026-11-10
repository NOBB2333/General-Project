<script setup lang="ts">
import {
  Download,
  FileSignature,
  Filter,
  Handshake,
  Info,
  Plus,
  Search,
  Star,
  UserRoundCheck,
  UsersRound,
  X,
} from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDropdownMenu, UiInput, UiTabs, UiTabsList, UiTabsTrigger, toast } from '@/components/ui';
import type { UiDropdownMenuItem } from '@/components/ui';
import { salesContractsReferenceFixture } from '@/features/sales/model/sales-contracts.data';
import { salesContractsModel } from '@/features/sales/model/sales-contracts.model';
import type {
  SalesContractCreatePayload,
  SalesContractProgressFilter,
  SalesContractRow,
  SalesContractScopeId,
  SalesContractStatus,
  SalesContractStatusFilter,
} from '@/features/sales/model/sales-contracts.types';
import SalesContractDetailDialog from './sales-contracts/SalesContractDetailDialog.vue';
import SalesContractEditorDialog from './sales-contracts/SalesContractEditorDialog.vue';
import SalesContractsTable from './sales-contracts/SalesContractsTable.vue';

const rows = ref(cloneReferenceRows(salesContractsReferenceFixture.rows));
const activeScope = ref<SalesContractScopeId>('all');
const activeStatus = ref<SalesContractStatusFilter>('全部');
const progressFilter = ref<SalesContractProgressFilter>('all');
const query = ref('');
const noticeVisible = ref(true);
const detailOpen = ref(false);
const createOpen = ref(false);
const activeRow = ref<SalesContractRow | null>(null);
const localSequence = ref(829);

const statusOrder: readonly SalesContractStatus[] = ['草稿', '待审批', '执行中', '已完成', '已终止'];
const progressMenuItems: readonly UiDropdownMenuItem[] = [
  { id: 'all', label: '全部进度' },
  { id: 'invoice-pending', label: '开票未完成' },
  { id: 'shipping-pending', label: '发货未完成' },
  { id: 'collection-pending', label: '收款未完成' },
];

const scopeOptions = computed(
  () =>
    [
      { id: 'all', label: '所有', count: rows.value.length, icon: UsersRound },
      {
        id: 'mine',
        label: '我负责的',
        count: rows.value.filter((row) => row.owner === '张明').length,
        icon: UserRoundCheck,
      },
      { id: 'subordinates', label: '下属负责的', count: 0, icon: UsersRound },
      {
        id: 'followed',
        label: '我关注的',
        count: rows.value.filter((row) => row.followed).length,
        icon: Star,
      },
    ] as const,
);

const statusTabs = computed(() => [
  { value: '全部', label: '全部' },
  ...statusOrder.map((status) => ({
    value: status,
    label: `${status}(${rows.value.filter((row) => row.status === status).length})`,
  })),
]);

const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    if (activeScope.value === 'mine' && row.owner !== '张明') return false;
    if (activeScope.value === 'subordinates') return false;
    if (activeScope.value === 'followed' && !row.followed) return false;
    if (activeStatus.value !== '全部' && row.status !== activeStatus.value) return false;
    if (progressFilter.value === 'invoice-pending' && Number.parseInt(row.invoicePct, 10) >= 100) return false;
    if (progressFilter.value === 'shipping-pending' && Number.parseInt(row.shipPct, 10) >= 100) return false;
    if (progressFilter.value === 'collection-pending' && Number.parseInt(row.collectPct, 10) >= 100) return false;
    if (!normalized) return true;
    return [row.code, row.name, row.subtitle, row.customer, row.source, row.owner].some((value) =>
      value.toLocaleLowerCase('zh-CN').includes(normalized),
    );
  });
});

const nextCode = computed(() => `HT-2026-${String(localSequence.value).padStart(4, '0')}`);
const progressLabel = computed(
  () => progressMenuItems.find((item) => item.id === progressFilter.value)?.label ?? '进度筛选',
);

function setScope(scope: SalesContractScopeId) {
  activeScope.value = scope;
}

function setStatus(status: string) {
  activeStatus.value = status as SalesContractStatusFilter;
}

function setProgressFilter(id: string) {
  progressFilter.value = id as SalesContractProgressFilter;
}

function openDetail(row: SalesContractRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function requestInvoice(row: SalesContractRow) {
  toast.success(`${row.code} 开票申请已在本地创建`);
}

function saveContract(payload: SalesContractCreatePayload) {
  const amountValue = Number(payload.amount.replace(/[^\d.]/g, '')) || 0;
  const row: SalesContractRow = {
    id: `sales-contract-local-${Date.now()}`,
    code: nextCode.value,
    name: payload.title,
    subtitle: `${payload.category} · 本地新建`,
    customer: payload.customer,
    source: '直接下单',
    amount: `¥${new Intl.NumberFormat('zh-CN').format(amountValue)}`,
    amountValue,
    invoicePct: '0%',
    shipPct: '0%',
    collectPct: '0%',
    status: payload.status,
    orderStatus: '-',
    traffic: 'neutral',
    signDate: payload.signDate,
    owner: '张明',
    followed: false,
  };
  rows.value = [row, ...rows.value];
  localSequence.value += 1;
  createOpen.value = false;
  toast.success(payload.status === '草稿' ? '销售合同已保存为本地草稿' : '销售合同已在本地提交审批');
}

function exportContracts() {
  const headers = salesContractsModel.columns.map((column) => column.title);
  const keys = salesContractsModel.columns.map((column) => column.key).filter((key) => key !== 'action');
  const csv = [
    headers.slice(0, keys.length),
    ...filteredRows.value.map((row) => keys.map((key) => String(row[key as keyof SalesContractRow] ?? ''))),
  ]
    .map((line) => line.map((value) => `"${value.replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = 'sales-contracts.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${filteredRows.value.length} 份销售合同`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/sales/contracts"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav
          :class="$style.breadcrumb"
          aria-label="面包屑"
        >
          <span>销售业务</span>
          <span aria-hidden="true">›</span>
          <strong>销售合同</strong>
        </nav>
        <div :class="$style.titleRow">
          <span :class="$style.heroIcon">
            <FileSignature
              :size="23"
              :stroke-width="1.8"
            />
          </span>
          <div>
            <h1 :class="$style.title">销售合同</h1>
            <p :class="$style.description">客户合同签订、条款管理、收款跟进与执行情况追踪</p>
          </div>
        </div>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <span :class="$style.heroRing" />
        <Handshake
          :size="148"
          :stroke-width="1.2"
        />
      </div>
    </header>

    <aside
      v-if="noticeVisible"
      :class="$style.notice"
    >
      <Info
        :size="16"
        :stroke-width="1.8"
      />
      <strong>什么时候该用销售合同？多数业务直接用订单即可</strong>
      <button
        type="button"
        :class="$style.noticeLink"
        @click="toast.info('长期框架合作、条款约束或分阶段履约时建议先签销售合同')"
      >
        查看说明 →
      </button>
      <button
        type="button"
        :class="$style.noticeClose"
        aria-label="关闭合同说明"
        @click="noticeVisible = false"
      >
        <X
          :size="14"
          :stroke-width="1.8"
        />
      </button>
    </aside>

    <nav
      :class="$style.scopes"
      aria-label="合同数据范围"
    >
      <button
        v-for="scope in scopeOptions"
        :key="scope.id"
        type="button"
        :class="[$style.scopeButton, activeScope === scope.id && $style.scopeActive]"
        :aria-pressed="activeScope === scope.id"
        @click="setScope(scope.id)"
      >
        <component
          :is="scope.icon"
          :size="14"
          :stroke-width="1.8"
        />
        {{ scope.label }}
        <span v-if="scope.count">{{ scope.count }}</span>
      </button>
    </nav>

    <section
      :class="$style.stats"
      aria-label="合同统计"
    >
      <article
        v-for="stat in salesContractsModel.stats"
        :key="stat.key"
        :class="[$style.statCard, $style[`stat_${stat.tone}`]]"
      >
        <span>{{ stat.label }}</span>
        <strong>{{ stat.value }}</strong>
        <small v-if="stat.hint">{{ stat.hint }}</small>
      </article>
    </section>

    <section :class="$style.listCard">
      <header :class="$style.listToolbar">
        <UiTabs
          :model-value="activeStatus"
          @update:model-value="setStatus"
        >
          <UiTabsList
            :class="$style.statusTabs"
            label="合同状态"
          >
            <UiTabsTrigger
              v-for="tab in statusTabs"
              :key="tab.value"
              :class="$style.statusButton"
              :value="tab.value"
            >
              {{ tab.label }}
            </UiTabsTrigger>
          </UiTabsList>
        </UiTabs>

        <div :class="$style.toolbarActions">
          <UiDropdownMenu
            :items="progressMenuItems"
            label="进度筛选"
            align="end"
            @select="setProgressFilter"
          >
            <template #trigger>
              <UiButton
                variant="secondary"
                size="sm"
              >
                <template #leading>
                  <Filter
                    :size="14"
                    :stroke-width="1.8"
                  />
                </template>
                {{ progressFilter === 'all' ? '进度筛选' : progressLabel }}
              </UiButton>
            </template>
          </UiDropdownMenu>

          <label :class="$style.searchField">
            <Search
              :size="14"
              :stroke-width="1.8"
              aria-hidden="true"
            />
            <UiInput
              v-model="query"
              :class="$style.searchInput"
              placeholder="搜索编号、客户、项目..."
              aria-label="搜索编号、客户、项目"
            />
          </label>

          <UiButton
            variant="secondary"
            size="sm"
            @click="exportContracts"
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
            variant="primary"
            size="sm"
            @click="createOpen = true"
          >
            <template #leading>
              <Plus
                :size="14"
                :stroke-width="1.8"
              />
            </template>
            新建合同
          </UiButton>
        </div>
      </header>

      <SalesContractsTable
        :rows="filteredRows"
        @detail="openDetail"
        @invoice="requestInvoice"
      />
    </section>
  </main>

  <SalesContractDetailDialog
    v-model:open="detailOpen"
    :row="activeRow"
    @invoice="requestInvoice"
  />
  <SalesContractEditorDialog
    v-model:open="createOpen"
    :next-code="nextCode"
    @save="saveContract"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto flex min-h-full w-full max-w-[1400px] flex-col gap-3;
}

.hero {
  @apply relative flex min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(112deg, #0747a6 0%, #0052cc 58%, #2684ff 100%);
}

.heroCopy {
  @apply relative z-10;
}

.breadcrumb {
  @apply mb-5 flex items-center gap-2 text-[12px];
  color: color-mix(in srgb, white 78%, transparent);
}

.breadcrumb strong {
  color: white;
}

.titleRow {
  @apply flex items-center gap-3;
}

.heroIcon {
  @apply flex h-12 w-12 items-center justify-center rounded-lg border;
  color: white;
  border-color: color-mix(in srgb, white 35%, transparent);
  background: color-mix(in srgb, white 13%, transparent);
}

.title {
  @apply text-[24px] font-semibold;
}

.description {
  @apply mt-1 text-[13px];
  color: color-mix(in srgb, white 82%, transparent);
}

.heroArt {
  @apply absolute inset-y-0 right-24 flex w-80 items-center justify-center;
  color: color-mix(in srgb, white 88%, transparent);
}

.heroArt::before,
.heroArt::after,
.heroRing {
  @apply absolute rounded-full border;
  content: '';
  border-color: color-mix(in srgb, white 34%, transparent);
}

.heroArt::before {
  width: 240px;
  height: 240px;
}

.heroArt::after {
  width: 280px;
  height: 280px;
}

.heroRing {
  width: 340px;
  height: 340px;
}

.notice {
  @apply flex min-h-8 w-fit max-w-full items-center gap-2 rounded-full border px-3 text-[12px];
  color: var(--ui-text-muted);
  border-color: var(--app-primary-border);
  background: var(--ui-surface);
  box-shadow: 0 1px 3px color-mix(in srgb, var(--app-primary-dark) 12%, transparent);
}

.notice > svg,
.noticeLink {
  color: var(--app-primary);
}

.noticeLink {
  @apply font-semibold;
}

.noticeClose {
  @apply flex h-6 w-6 items-center justify-center rounded-full transition-colors;
  color: var(--ui-text-subtle);
}

.noticeClose:hover {
  background: var(--ui-surface-hover);
}

.scopes {
  @apply flex items-center gap-1 rounded border p-1;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.scopeButton {
  @apply flex min-h-8 items-center gap-2 rounded px-3 text-[12px] transition-colors;
  color: var(--ui-text-muted);
}

.scopeButton span {
  @apply rounded-full px-1.5 py-0.5 text-[10px];
  background: var(--ui-surface-hover);
}

.scopeActive {
  color: white;
  background: var(--app-primary);
}

.scopeActive span {
  color: var(--app-primary);
  background: white;
}

.stats {
  @apply grid grid-cols-4 gap-3 xl:grid-cols-8;
}

.statCard {
  @apply flex min-h-[72px] flex-col justify-center rounded border px-3 py-2;
}

.statCard span {
  @apply text-[11px];
  color: var(--ui-text-muted);
}

.statCard strong {
  @apply mt-1 text-[20px] font-semibold;
}

.statCard small {
  @apply mt-0.5 text-[10px];
  color: var(--ui-text-subtle);
}

.stat_primary {
  color: var(--app-primary);
  border-color: var(--app-primary-border);
  background: var(--app-primary-soft);
}
.stat_success {
  color: var(--ui-success);
  border-color: color-mix(in srgb, var(--ui-success) 30%, transparent);
  background: var(--ui-success-soft);
}
.stat_warning {
  color: var(--ui-warning);
  border-color: color-mix(in srgb, var(--ui-warning) 35%, transparent);
  background: var(--ui-warning-soft);
}
.stat_danger {
  color: var(--ui-danger);
  border-color: color-mix(in srgb, var(--ui-danger) 30%, transparent);
  background: var(--ui-danger-soft);
}
.stat_neutral {
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.listCard {
  @apply min-h-0 overflow-hidden rounded border;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.listToolbar {
  @apply flex min-h-[44px] items-center gap-3 border-b px-4;
  border-color: var(--ui-border);
}

.statusTabs {
  @apply flex items-center;
}

.statusButton {
  @apply min-h-[42px] rounded-none border-b-2 border-transparent px-3 text-[12px];
  color: var(--ui-text-muted);
  background: transparent;
}

.statusButton[data-state='active'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: transparent;
}

.toolbarActions {
  @apply ml-auto flex items-center gap-2;
}

.searchField {
  @apply flex min-w-[280px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.searchInput {
  @apply border-0 bg-transparent px-0 shadow-none;
}

.searchInput:focus {
  border-color: transparent;
  box-shadow: none;
}

@media (max-width: 1100px) {
  .heroArt {
    @apply right-2 opacity-50;
  }
  .listToolbar {
    @apply flex-wrap py-2;
  }
  .toolbarActions {
    @apply w-full;
  }
  .searchField {
    @apply min-w-0 flex-1;
  }
}

@media (max-width: 700px) {
  .hero {
    @apply min-h-[132px] px-5 py-4;
  }
  .heroArt {
    @apply hidden;
  }
  .stats {
    @apply grid-cols-2;
  }
  .scopes,
  .statusTabs {
    @apply overflow-x-auto;
  }
  .scopeButton,
  .statusButton {
    @apply shrink-0;
  }
  .toolbarActions {
    @apply flex-wrap;
  }
  .searchField {
    @apply order-first w-full flex-none;
  }
}
</style>
