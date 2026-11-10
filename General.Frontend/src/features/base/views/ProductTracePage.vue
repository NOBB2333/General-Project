<script setup lang="ts">
import { computed, ref } from 'vue';
import { Boxes, PackageSearch, ShieldCheck } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { productTraceReferenceFixture } from '../model/product-trace.data';
import type { ProductTraceRow, TraceSource, TraceView } from '../model/product-trace.types';
import ProductTraceHero from './product-trace/ProductTraceHero.vue';
import ProductTraceStats from './product-trace/ProductTraceStats.vue';
import ProductTraceTable from './product-trace/ProductTraceTable.vue';
import ProductTraceToolbar from './product-trace/ProductTraceToolbar.vue';

const rows = ref<ProductTraceRow[]>(cloneReferenceRows(productTraceReferenceFixture.rows));
const activeView = ref<TraceView>('instances');
const query = ref('');
const status = ref('全部状态');
const source = ref('全部来源');
const warranty = ref('全部质保');
const detailOpen = ref(false);
const createOpen = ref(false);
const activeRow = ref<ProductTraceRow | null>(null);
const draftProduct = ref('');
const draftModel = ref('');
const draftExternalSn = ref('');
const draftSource = ref<TraceSource>('外购');
const draftCount = ref('1');

const sourceOptions: UiSelectOption[] = [
  { value: '外购', label: '外购' },
  { value: '自制', label: '自制' },
  { value: '委外', label: '委外' },
];

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return rows.value.filter((row) => {
    const matchesStatus = status.value === '全部状态' || row.status === status.value;
    const matchesSource = source.value === '全部来源' || row.source === source.value;
    const matchesWarranty = warranty.value === '全部质保' || row.warrantyStatus === warranty.value;
    const matchesQuery =
      !keyword ||
      `${row.traceCode} ${row.externalSn} ${row.productName} ${row.customer}`.toLowerCase().includes(keyword);
    return matchesStatus && matchesSource && matchesWarranty && matchesQuery;
  });
});

const productSummary = computed(() => {
  const grouped = new Map<string, { name: string; model: string; count: number }>();
  for (const row of rows.value) {
    const current = grouped.get(row.model);
    if (current) current.count += 1;
    else grouped.set(row.model, { name: row.productName, model: row.model, count: 1 });
  }
  return [...grouped.values()];
});

const batchSummary = computed(() => {
  const grouped = new Map<string, number>();
  for (const row of rows.value) {
    const batch = row.traceCode.slice(2, 10);
    grouped.set(batch, (grouped.get(batch) ?? 0) + 1);
  }
  return [...grouped].map(([batch, count]) => ({ batch, count }));
});

const warrantySummary = computed(() =>
  ['未开始', '在保', '即将到期', '已过保'].map((label) => ({
    label,
    count: rows.value.filter((row) => row.warrantyStatus === label).length,
  })),
);

function openRow(row: ProductTraceRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function openCreate() {
  draftProduct.value = '';
  draftModel.value = '';
  draftExternalSn.value = '';
  draftSource.value = '外购';
  draftCount.value = '1';
  createOpen.value = true;
}

function setDraftSource(value: string) {
  if (value === '外购' || value === '自制' || value === '委外') draftSource.value = value;
}

function saveTraceCodes() {
  if (!draftProduct.value.trim() || !draftModel.value.trim()) {
    toast.warning('请填写商品名称和型号');
    return;
  }
  const count = Math.max(1, Math.min(20, Number(draftCount.value) || 1));
  const stamp = '20260916';
  const newRows: ProductTraceRow[] = Array.from({ length: count }, (_, index) => ({
    id: `trace-local-${Date.now()}-${index}`,
    traceCode: `SN${stamp}-${String(rows.value.length + index + 1).padStart(6, '0')}`,
    externalSn: count === 1 && draftExternalSn.value.trim() ? draftExternalSn.value.trim() : '—',
    productName: draftProduct.value.trim(),
    model: draftModel.value.trim(),
    source: draftSource.value,
    status: '在库',
    warrantyStatus: '未开始',
    customer: '—',
    outboundDate: '—',
  }));
  rows.value = [...newRows, ...rows.value];
  createOpen.value = false;
  toast.success(`已在本地生成 ${count} 个追溯码`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/base/product-trace"
    data-product-trace-page
    :data-active-view="activeView"
  >
    <ProductTraceHero v-model:active-view="activeView" />

    <template v-if="activeView === 'instances'">
      <ProductTraceStats
        :total="rows.length"
        :stock="rows.filter((row) => row.status === '在库').length"
        :outbound="rows.filter((row) => row.status === '已出库').length"
        :repair="rows.filter((row) => row.status === '已报修' || row.status === '维修中').length"
        :scrapped="rows.filter((row) => row.status === '已报废').length"
      />
      <ProductTraceToolbar
        v-model:query="query"
        v-model:status="status"
        v-model:source="source"
        v-model:warranty="warranty"
        @create="openCreate"
      />
      <ProductTraceTable
        :rows="filteredRows"
        @open="openRow"
      />
    </template>

    <section
      v-else
      :class="$style.alternate"
      :data-trace-view="activeView"
    >
      <template v-if="activeView === 'product-codes'">
        <header>
          <PackageSearch
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <h2>商品编码</h2>
            <p>按商品型号汇总已生成的实例编码</p>
          </div>
        </header>
        <div :class="$style.summaryGrid">
          <article
            v-for="item in productSummary"
            :key="item.model"
          >
            <strong>{{ item.model }}</strong
            ><span>{{ item.name }}</span
            ><b>{{ item.count }} 个实例</b>
          </article>
        </div>
      </template>
      <template v-else-if="activeView === 'batch-codes'">
        <header>
          <Boxes
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <h2>批次码</h2>
            <p>按生成日期聚合实例批次</p>
          </div>
        </header>
        <div :class="$style.summaryGrid">
          <article
            v-for="item in batchSummary"
            :key="item.batch"
          >
            <strong>BAT-{{ item.batch }}</strong
            ><span>生成批次</span><b>{{ item.count }} 个实例</b>
          </article>
        </div>
      </template>
      <template v-else>
        <header>
          <ShieldCheck
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <h2>质保管理</h2>
            <p>跟踪在保、到期和过保实例</p>
          </div>
        </header>
        <div :class="$style.warrantyGrid">
          <article
            v-for="item in warrantySummary"
            :key="item.label"
          >
            <span>{{ item.label }}</span
            ><strong>{{ item.count }}</strong>
          </article>
        </div>
      </template>
    </section>
  </main>

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.traceCode ?? '实例详情'"
    :description="activeRow?.productName"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>外部 SN</dt>
        <dd>{{ activeRow.externalSn }}</dd>
      </div>
      <div>
        <dt>型号 / 来源</dt>
        <dd>{{ activeRow.model }} · {{ activeRow.source }}</dd>
      </div>
      <div>
        <dt>实例状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>质保状态</dt>
        <dd>{{ activeRow.warrantyStatus }}</dd>
      </div>
      <div>
        <dt>客户</dt>
        <dd>{{ activeRow.customer }}</dd>
      </div>
      <div>
        <dt>出库日期</dt>
        <dd>{{ activeRow.outboundDate }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>

  <UiDialog
    v-model:open="createOpen"
    title="生成追溯码"
    description="追溯码只生成到本地复刻页面"
    size="md"
  >
    <form
      :class="$style.form"
      @submit.prevent="saveTraceCodes"
    >
      <UiFormField
        label="商品名称"
        required
        wide
        ><UiInput
          v-model="draftProduct"
          placeholder="请输入商品名称"
      /></UiFormField>
      <UiFormField
        label="型号"
        required
        ><UiInput
          v-model="draftModel"
          placeholder="请输入型号"
      /></UiFormField>
      <UiFormField label="来源"
        ><UiSelect
          :model-value="draftSource"
          :options="sourceOptions"
          label="来源"
          @update:model-value="setDraftSource"
      /></UiFormField>
      <UiFormField label="外部 SN"
        ><UiInput
          v-model="draftExternalSn"
          placeholder="单个实例时可填写"
      /></UiFormField>
      <UiFormField label="生成数量"
        ><UiInput
          v-model="draftCount"
          type="number"
          min="1"
          max="20"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="createOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveTraceCodes"
        >生成追溯码</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto flex min-h-full w-full max-w-[1400px] flex-col gap-3 pb-8;
  color: var(--ui-text);
}
.alternate {
  @apply min-h-[430px] rounded border bg-white p-5;
  border-color: var(--ui-border);
}
.alternate > header {
  @apply mb-5 flex items-center gap-3 border-b pb-4;
  border-color: var(--ui-border);
}
.alternate > header > svg {
  color: var(--app-primary);
}
.alternate h2 {
  @apply text-[16px] font-semibold;
}
.alternate p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.summaryGrid {
  @apply grid grid-cols-3 gap-3;
}
.summaryGrid article {
  @apply flex min-h-20 flex-col rounded border p-3;
  border-color: var(--ui-border);
}
.summaryGrid strong {
  @apply text-[12px];
  color: var(--app-primary);
}
.summaryGrid span {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.summaryGrid b {
  @apply mt-auto text-[11px] font-semibold;
  color: var(--ui-text-muted);
}
.warrantyGrid {
  @apply grid grid-cols-4 gap-3;
}
.warrantyGrid article {
  @apply rounded border p-4;
  border-color: var(--ui-border);
}
.warrantyGrid span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.warrantyGrid strong {
  @apply mt-2 block text-[24px];
  color: var(--app-primary);
}
.details {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 p-5;
}
.details dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[13px] font-medium;
  color: var(--ui-text);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
