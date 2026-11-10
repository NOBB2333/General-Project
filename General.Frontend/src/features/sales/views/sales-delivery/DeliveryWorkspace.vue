<script setup lang="ts">
import {
  ArrowLeftRight,
  Box,
  ChevronDown,
  Download,
  FileText,
  Layers3,
  MapPin,
  PackageOpen,
  Plus,
  Search,
  Truck,
} from 'lucide-vue-next';
import { ref } from 'vue';
import { UiButton, UiPopover, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type {
  DeliveryDetailRow,
  DeliveryStatus,
  DeliveryView,
  SalesDeliveryRecord,
} from '../../model/sales-delivery.types';

const props = defineProps<{
  view: DeliveryView;
  status: '全部' | DeliveryStatus;
  query: string;
  rows: readonly SalesDeliveryRecord[];
  detailRows: readonly DeliveryDetailRow[];
  detailCount: number;
  statusCounts: Record<'全部' | DeliveryStatus, number>;
}>();

const emit = defineEmits<{
  'update:view': [view: DeliveryView];
  'update:status': [status: '全部' | DeliveryStatus];
  'update:query': [query: string];
  create: [];
  export: [];
  ship: [row: SalesDeliveryRecord];
  open: [row: SalesDeliveryRecord];
}>();

const expandedIds = ref<string[]>([]);
const statuses: readonly ('全部' | DeliveryStatus)[] = ['全部', '待发货', '部分发货', '已发货', '已签收'];

function toggleExpanded(id: string) {
  expandedIds.value = expandedIds.value.includes(id)
    ? expandedIds.value.filter((entry) => entry !== id)
    : [...expandedIds.value, id];
}

function statusClass(status: DeliveryStatus) {
  return {
    待发货: 'statusPending',
    部分发货: 'statusPartial',
    已发货: 'statusShipped',
    已签收: 'statusSigned',
  }[status];
}

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}

function orders(row: SalesDeliveryRecord) {
  return row.soIds?.length ? row.soIds : [row.soId];
}
</script>

<template>
  <section :class="$style.workspace">
    <header :class="$style.toolbar">
      <UiTabs
        :model-value="view"
        @update:model-value="emit('update:view', $event as DeliveryView)"
      >
        <UiTabsList
          label="发货视图"
          :class="$style.viewTabs"
        >
          <UiTabsTrigger value="list">
            <Box
              :size="13"
              :stroke-width="1.8"
            />发货单列表
          </UiTabsTrigger>
          <UiTabsTrigger value="detail">
            <FileText
              :size="13"
              :stroke-width="1.8"
            />发货明细
            <b :class="$style.countBadge">{{ detailCount }}</b>
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>

      <i :class="$style.divider" />

      <UiTabs
        :model-value="status"
        @update:model-value="emit('update:status', $event as '\u5168\u90e8' | DeliveryStatus)"
      >
        <UiTabsList
          label="发货状态"
          :class="$style.statusTabs"
        >
          <UiTabsTrigger
            v-for="item in statuses"
            :key="item"
            :value="item"
          >
            {{ item }} <small>({{ statusCounts[item] }})</small>
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>

      <div :class="$style.actions">
        <label :class="$style.searchBox">
          <Search
            :size="14"
            :stroke-width="1.8"
          />
          <input
            :value="query"
            :placeholder="view === 'detail' ? '搜索发货单号/物料/客户...' : '搜索发货单号/客户/订单号...'"
            @input="emit('update:query', ($event.target as HTMLInputElement).value)"
          />
        </label>
        <UiButton
          size="sm"
          @click="emit('export')"
        >
          <template #leading
            ><Download
              :size="13"
              :stroke-width="1.8"
          /></template>
          导出
        </UiButton>
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('create')"
        >
          <template #leading
            ><Plus
              :size="13"
              :stroke-width="1.8"
          /></template>
          新建发货单
        </UiButton>
      </div>
    </header>

    <span :class="$style.scrollHint">
      <ArrowLeftRight
        :size="13"
        :stroke-width="1.8"
      />左右滑动
    </span>

    <div :class="$style.scroller">
      <table
        v-if="view === 'list'"
        :class="$style.table"
      >
        <thead>
          <tr>
            <th :class="$style.expandColumn" />
            <th>发货单号</th>
            <th>关联订单</th>
            <th>客户</th>
            <th>收货人</th>
            <th>状态</th>
            <th>发货进度</th>
            <th :class="$style.numeric">金额</th>
            <th>出库单</th>
            <th>物流单号</th>
            <th>日期</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <template
            v-for="row in rows"
            :key="row.id"
          >
            <tr :class="expandedIds.includes(row.id) && $style.expandedRow">
              <td :class="$style.expandCell">
                <button
                  v-if="row.outbounds.length"
                  type="button"
                  :class="[$style.expandButton, expandedIds.includes(row.id) && $style.expandedButton]"
                  :aria-label="expandedIds.includes(row.id) ? '收起出库记录' : '展开出库记录'"
                  :aria-expanded="expandedIds.includes(row.id)"
                  @click="toggleExpanded(row.id)"
                >
                  <ChevronDown
                    :size="14"
                    :stroke-width="1.8"
                  />
                </button>
                <i v-else />
              </td>
              <td>
                <button
                  type="button"
                  :class="$style.primaryLink"
                  @click="emit('open', row)"
                >
                  {{ row.id }}
                </button>
              </td>
              <td>
                <span
                  v-if="orders(row).length > 1"
                  :class="$style.mergeBadge"
                  ><Layers3
                    :size="10"
                    :stroke-width="1.8"
                  />合并 {{ orders(row).length }} 单</span
                >
                <button
                  v-for="order in orders(row).slice(0, 2)"
                  :key="order"
                  type="button"
                  :class="$style.orderLink"
                >
                  {{ order }}
                </button>
                <small v-if="orders(row).length > 2">+{{ orders(row).length - 2 }} 个订单</small>
              </td>
              <td>
                <strong>{{ row.customer }}</strong>
              </td>
              <td>
                <span :class="$style.stacked"
                  ><strong
                    ><MapPin
                      :size="11"
                      :stroke-width="1.8"
                    />{{ row.receiver }}</strong
                  ><small>{{ row.project }}</small></span
                >
              </td>
              <td>
                <span :class="[$style.status, $style[statusClass(row.status)]]">{{ row.status }}</span>
              </td>
              <td>
                <span :class="$style.progressCell">
                  <progress
                    :value="row.shippedQty"
                    :max="row.totalQty"
                  />
                  <strong>{{ row.shippedQty }}/{{ row.totalQty }}</strong>
                </span>
              </td>
              <td :class="[$style.numeric, $style.amount]">{{ money(row.amount) }}</td>
              <td>
                <span :class="$style.outboundCount"
                  ><PackageOpen
                    :size="12"
                    :stroke-width="1.8"
                  />{{ row.outbounds.length }}</span
                >
              </td>
              <td>
                <span
                  v-if="row.outbounds.length"
                  :class="$style.trackingList"
                >
                  <UiPopover
                    v-for="outbound in row.outbounds"
                    :key="outbound.id"
                    size="xs"
                    align="start"
                  >
                    <template #trigger>
                      <button type="button">
                        <Truck
                          :size="11"
                          :stroke-width="1.8"
                        />{{ outbound.tracking }}
                      </button>
                    </template>
                    <dl :class="$style.trackingPopover">
                      <div>
                        <dt>承运商</dt>
                        <dd>{{ outbound.carrier }}</dd>
                      </div>
                      <div>
                        <dt>物流单号</dt>
                        <dd>{{ outbound.tracking }}</dd>
                      </div>
                      <div>
                        <dt>出库日期</dt>
                        <dd>{{ outbound.date }}</dd>
                      </div>
                    </dl>
                  </UiPopover>
                </span>
                <span
                  v-else
                  :class="$style.emptyValue"
                  >—</span
                >
              </td>
              <td :class="$style.date">{{ row.date }}</td>
              <td>
                <UiButton
                  v-if="row.status === '待发货'"
                  variant="primary"
                  size="xs"
                  @click="emit('ship', row)"
                  >发货</UiButton
                >
                <UiButton
                  v-else
                  variant="link"
                  size="xs"
                  @click="emit('open', row)"
                  >查看</UiButton
                >
              </td>
            </tr>
            <tr
              v-if="expandedIds.includes(row.id)"
              :class="$style.outboundRow"
            >
              <td colspan="12">
                <div :class="$style.outboundPanel">
                  <article
                    v-for="outbound in row.outbounds"
                    :key="outbound.id"
                  >
                    <strong>{{ outbound.id }}</strong
                    ><span>{{ outbound.warehouse }}</span
                    ><span>{{ outbound.qty }} 件</span> <span>{{ outbound.carrier }} · {{ outbound.tracking }}</span
                    ><span>{{ outbound.date }}</span>
                  </article>
                </div>
              </td>
            </tr>
          </template>
          <tr v-if="!rows.length">
            <td
              colspan="12"
              :class="$style.emptyState"
            >
              <Truck
                :size="32"
                :stroke-width="1.4"
              /><strong>暂无匹配的发货单</strong><span>尝试调整筛选条件或搜索关键词</span>
            </td>
          </tr>
        </tbody>
      </table>

      <table
        v-else
        :class="[$style.table, $style.detailTable]"
      >
        <thead>
          <tr>
            <th>#</th>
            <th>物料编码</th>
            <th>物料名称</th>
            <th>规格</th>
            <th>发货单</th>
            <th>订单</th>
            <th>客户</th>
            <th>数量</th>
            <th>已发/待发</th>
            <th>金额</th>
            <th>出库状态</th>
            <th>仓库</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="(row, index) in detailRows"
            :key="row.key"
          >
            <td>{{ index + 1 }}</td>
            <td :class="$style.itemCode">{{ row.itemCode }}</td>
            <td>
              <strong>{{ row.name }}</strong>
            </td>
            <td>{{ row.spec }}</td>
            <td :class="$style.primaryText">{{ row.shipmentId }}</td>
            <td>{{ row.soId }}</td>
            <td>{{ row.customer }}</td>
            <td>{{ row.qty }} {{ row.unit }}</td>
            <td>{{ row.shippedQty }} / {{ row.pendingQty }}</td>
            <td :class="$style.amount">{{ money(row.amount) }}</td>
            <td>
              <span
                :class="[$style.status, row.outboundStatus === '已出库' ? $style.statusSigned : $style.statusPending]"
                >{{ row.outboundStatus }}</span
              >
            </td>
            <td>{{ row.warehouse }}</td>
          </tr>
          <tr v-if="!detailRows.length">
            <td
              colspan="12"
              :class="$style.emptyState"
            >
              <FileText
                :size="32"
                :stroke-width="1.4"
              /><strong>暂无匹配的发货明细</strong>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <footer :class="$style.pagination">
      <span>{{
        view === 'list'
          ? `共 ${rows.length} 条记录`
          : `显示 1 - ${Math.min(15, detailRows.length)} 条，共 ${detailRows.length} 条物料明细`
      }}</span>
      <span v-if="view === 'detail' && detailRows.length > 15"
        ><button
          type="button"
          disabled
        >
          ‹</button
        ><b>1</b><button type="button">2</button><button type="button">›</button></span
      >
    </footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workspace {
  @apply relative overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex min-h-[60px] items-center gap-2 border-b px-1;
  border-color: var(--ui-border);
}
.viewTabs,
.statusTabs {
  @apply flex-nowrap gap-0;
}
.viewTabs button[data-state],
.statusTabs button[data-state] {
  @apply min-h-[59px] rounded-none px-3.5 text-[12px];
  background: transparent;
}
.viewTabs button[data-state='active'],
.statusTabs button[data-state='active'] {
  @apply font-semibold;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}
.countBadge {
  @apply rounded-full px-1.5 py-0.5 text-[10px] font-semibold;
  color: #6b778c;
  background: #ebecf0;
}
.statusTabs small {
  @apply text-[10px];
  color: #97a0af;
}
.divider {
  @apply h-5 w-px shrink-0;
  background: var(--ui-border);
}
.actions {
  @apply ml-auto flex items-center gap-2 pr-3;
}
.searchBox {
  @apply flex h-8 w-[240px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: #f4f5f7;
}
.searchBox input {
  @apply min-w-0 flex-1 bg-transparent text-[12px] outline-none;
  color: var(--ui-text);
}
.searchBox input::placeholder {
  color: #97a0af;
}
.scrollHint {
  @apply absolute right-2 top-[67px] z-[2] flex items-center gap-1 rounded-full border px-2 py-1 text-[10px] font-semibold shadow-sm;
  color: var(--app-primary);
  background: #deebff;
  border-color: #b3d4ff;
}
.scroller {
  @apply overflow-x-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.table {
  @apply w-full border-collapse text-left text-[11px];
  min-width: 1260px;
  color: var(--ui-text);
}
.table thead {
  background: #f4f5f7;
}
.table th {
  @apply h-[42px] whitespace-nowrap px-3 py-3 font-semibold;
  color: #5e6c84;
}
.table td {
  @apply border-t px-3 py-3 align-middle;
  border-color: #f4f5f7;
}
.table tbody tr:not(.outboundRow) {
  @apply transition-colors;
}
.table tbody tr:not(.outboundRow):hover {
  background: #fafbfc;
}
.table th:nth-child(2),
.table td:nth-child(2) {
  min-width: 140px;
}
.table th:nth-child(3),
.table td:nth-child(3) {
  min-width: 145px;
}
.table th:nth-child(4),
.table td:nth-child(4) {
  min-width: 135px;
}
.table th:nth-child(5),
.table td:nth-child(5) {
  min-width: 155px;
}
.table th:nth-child(7),
.table td:nth-child(7) {
  min-width: 125px;
}
.table th:nth-child(10),
.table td:nth-child(10) {
  min-width: 150px;
}
.expandColumn,
.expandCell {
  @apply w-11 px-2 text-center;
}
.expandCell > i {
  @apply mx-auto block h-1.5 w-1.5 rounded-full;
  background: #dfe1e6;
}
.expandButton {
  @apply mx-auto flex h-6 w-6 items-center justify-center rounded transition-colors;
  color: #6b778c;
  transform: rotate(-90deg);
}
.expandButton:hover {
  background: #ebecf0;
}
.expandedButton {
  @apply text-white;
  background: var(--app-primary);
  transform: rotate(0deg);
}
.expandedRow {
  background: #f4f8ff;
}
.primaryLink,
.orderLink {
  @apply block font-semibold hover:underline;
  color: var(--app-primary);
}
.orderLink {
  @apply mt-1 text-[11px] font-medium;
}
.mergeBadge {
  @apply mb-0.5 inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-[9px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.table td small {
  @apply mt-1 block text-[10px];
  color: #97a0af;
}
.stacked {
  @apply flex flex-col gap-1;
}
.stacked strong {
  @apply flex items-center gap-1 font-medium;
}
.status {
  @apply inline-flex whitespace-nowrap rounded px-2 py-0.5 text-[10px] font-semibold;
}
.statusPending {
  color: #ff8b00;
  background: #fffae6;
}
.statusPartial {
  color: #0065ff;
  background: #e6fcff;
}
.statusShipped {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.statusSigned {
  color: var(--ui-success);
  background: #e3fcef;
}
.progressCell {
  @apply flex items-center gap-2;
}
.progressCell progress {
  @apply h-1.5 w-[72px] overflow-hidden rounded-full border-0;
  background: #ebecf0;
}
.progressCell progress::-webkit-progress-bar {
  background: #ebecf0;
}
.progressCell progress::-webkit-progress-value {
  @apply rounded-full;
  background: var(--app-primary);
}
.progressCell progress::-moz-progress-bar {
  @apply rounded-full;
  background: var(--app-primary);
}
.progressCell strong {
  @apply text-[11px] font-semibold;
}
.numeric {
  @apply text-right;
}
.amount {
  @apply whitespace-nowrap text-[12px] font-bold;
}
.outboundCount {
  @apply inline-flex items-center gap-1 rounded-full px-2 py-1 font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.trackingList {
  @apply flex flex-col items-start gap-1;
}
.trackingList button {
  @apply flex items-center gap-1 border-b border-dashed text-[11px] hover:underline;
  color: var(--app-primary);
  border-color: #b3d4ff;
}
.trackingPopover {
  @apply space-y-2 p-3 text-[11px];
}
.trackingPopover div {
  @apply flex items-center justify-between gap-3;
}
.trackingPopover dt {
  color: var(--ui-text-subtle);
}
.trackingPopover dd {
  @apply font-medium;
}
.emptyValue,
.date {
  color: #6b778c;
}
.outboundRow td {
  @apply p-0;
  background: #f4f8ff;
}
.outboundPanel {
  @apply border-t px-12 py-3;
  border-color: #b3d4ff;
}
.outboundPanel article {
  @apply grid grid-cols-5 gap-4 border-b py-2 text-[11px] last:border-b-0;
  border-color: #ebecf0;
}
.outboundPanel article strong {
  color: var(--app-primary);
}
.emptyState {
  @apply text-center;
  padding-top: 4rem;
  padding-bottom: 4rem;
  color: #97a0af;
}
.emptyState svg,
.emptyState strong,
.emptyState span {
  @apply mx-auto block;
}
.emptyState strong {
  @apply mt-3 text-[14px] font-medium;
}
.emptyState span {
  @apply mt-1 text-[12px];
}
.detailTable {
  min-width: 1560px;
}
.detailTable td {
  @apply whitespace-nowrap;
}
.itemCode {
  @apply font-mono;
  color: var(--app-primary);
}
.primaryText {
  @apply font-semibold;
  color: var(--app-primary);
}
.pagination {
  @apply flex min-h-12 items-center justify-between border-t px-5 text-[12px];
  color: #6b778c;
  border-color: var(--ui-border);
}
.pagination > span:last-child {
  @apply flex items-center gap-1;
}
.pagination button,
.pagination b {
  @apply flex h-8 w-8 items-center justify-center rounded font-normal;
}
.pagination b {
  @apply text-white;
  background: var(--app-primary);
}
@media (max-width: 1250px) {
  .toolbar {
    @apply flex-wrap py-1;
  }
  .actions {
    @apply w-full pb-2;
  }
}
</style>
