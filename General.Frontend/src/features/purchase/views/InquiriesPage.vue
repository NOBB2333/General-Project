<script setup lang="ts">
import { BriefcaseBusiness, ClipboardList } from 'lucide-vue-next';
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, toast } from '@/components/ui';
import { businessInquiryTasks, inquiriesReferenceFixture } from '../model/inquiries.data';
import type { InquiryDraft, InquiryRow, InquiryScope, InquirySortKey, InquiryStatus } from '../model/inquiries.types';
import BusinessInquiryTasks from './inquiries/BusinessInquiryTasks.vue';
import InquiryFormDialog from './inquiries/InquiryFormDialog.vue';
import InquiryHero from './inquiries/InquiryHero.vue';
import InquiryOverview from './inquiries/InquiryOverview.vue';
import InquiryTable from './inquiries/InquiryTable.vue';
import InquiryToolbar from './inquiries/InquiryToolbar.vue';

const router = useRouter();
const rows = ref<InquiryRow[]>(cloneReferenceRows(inquiriesReferenceFixture.rows));
const mode = ref<'rfq' | 'business'>('rfq');
const scope = ref<InquiryScope>('all');
const status = ref<'全部' | InquiryStatus>('全部');
const query = ref('');
const sortKey = ref<InquirySortKey | ''>('');
const sortDirection = ref<'asc' | 'desc'>('asc');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<InquiryRow | null>(null);

const pendingTaskCount = computed(() => businessInquiryTasks.filter((task) => task.status === '待分配').length);
const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  const result = rows.value.filter((row) => {
    const matchesScope =
      scope.value === 'all' ||
      (scope.value === 'mine' && row.owner === '张经理') ||
      (scope.value === 'team' && ['王工', '李工'].includes(row.owner)) ||
      (scope.value === 'watched' && row.watched);
    const matchesStatus = status.value === '全部' || row.status === status.value;
    const matchesQuery =
      !keyword ||
      row.id.toLowerCase().includes(keyword) ||
      row.title.toLowerCase().includes(keyword) ||
      row.projectName.toLowerCase().includes(keyword);
    return matchesScope && matchesStatus && matchesQuery;
  });
  if (!sortKey.value) return result;
  return [...result].sort((left, right) => {
    const key = sortKey.value as InquirySortKey;
    const leftValue = left[key];
    const rightValue = right[key];
    const comparison =
      typeof leftValue === 'number' && typeof rightValue === 'number'
        ? leftValue - rightValue
        : String(leftValue).localeCompare(String(rightValue), 'zh-CN');
    return sortDirection.value === 'asc' ? comparison : -comparison;
  });
});

onMounted(() => {
  const expired = rows.value.filter(
    (row) => row.status === '报价收集中' && ['RFQ-2026-001', 'RFQ-2026-003'].includes(row.id),
  );
  if (!expired.length) return;
  rows.value = rows.value.map((row) =>
    expired.some((item) => item.id === row.id)
      ? { ...row, status: '失效关闭', closeReason: '报价截止日期已过期，系统自动关闭' }
      : row,
  );
  toast.error(`${expired.length} 个询价单已超过截止日期，已自动关闭`, {
    description: expired.map((row) => row.id).join(', '),
  });
});

function updateSort(key: InquirySortKey) {
  if (sortKey.value !== key) {
    sortKey.value = key;
    sortDirection.value = 'asc';
    return;
  }
  if (sortDirection.value === 'asc') {
    sortDirection.value = 'desc';
    return;
  }
  sortKey.value = '';
  sortDirection.value = 'asc';
}
function openDetail(row: InquiryRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
async function copyLink(row: InquiryRow) {
  const link = `${window.location.origin}/supplier/rfq/${row.id}`;
  try {
    await navigator.clipboard.writeText(link);
    toast.success('供应商报价链接已复制到剪贴板');
  } catch {
    toast.info('请手动复制链接', { description: link });
  }
}
function exportRows() {
  const header = [
    '询价单号',
    '标题',
    '项目',
    '来源类型',
    '来源单号',
    '负责采购',
    '供应商数',
    '已报价数',
    '物料数',
    '预算金额',
    '状态',
    '截止日期',
  ];
  const data = filteredRows.value.map((row) => [
    row.id,
    row.title,
    row.projectName,
    row.sourceType,
    row.sourceId,
    row.assignedBuyer || '-',
    row.supplierCount,
    row.quotedCount,
    row.materialCount,
    row.totalBudget,
    row.status,
    row.deadline,
  ]);
  const csv = [header, ...data]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const anchor = document.createElement('a');
  anchor.href = href;
  anchor.download = `询价管理_${new Date().toISOString().slice(0, 10)}.csv`;
  anchor.click();
  URL.revokeObjectURL(href);
  toast.success(`已导出 ${filteredRows.value.length} 条询价单记录`);
}
function saveInquiry(draft: InquiryDraft) {
  const sequence = Math.max(...rows.value.map((row) => Number(row.id.slice(-3)))) + 1;
  rows.value = [
    {
      id: `RFQ-2026-${String(sequence).padStart(3, '0')}`,
      title: draft.title.trim(),
      projectName: draft.projectName.trim() || '-',
      sourceType: draft.sourceType,
      sourceId: draft.sourceId.trim() || '-',
      supplierCount: Number(draft.supplierCount) || 1,
      quotedCount: 0,
      materialCount: Number(draft.materialCount) || 1,
      totalBudget: Number(draft.totalBudget) || 0,
      status: '待发布',
      createdAt: new Date().toISOString().slice(0, 10),
      deadline: draft.deadline,
      createdBy: '张经理',
      owner: '张经理',
      assignedBuyer: draft.assignedBuyer,
      watched: false,
    },
    ...rows.value,
  ];
  formOpen.value = false;
  scope.value = 'all';
  status.value = '全部';
  toast.success('询价单草稿已保存到本地列表');
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/purchase/inquiries"
    data-inquiries-page
    :data-mode="mode"
  >
    <InquiryHero />
    <nav
      :class="$style.modeTabs"
      aria-label="询价工作区"
    >
      <button
        type="button"
        :class="[$style.modeTab, mode === 'rfq' && $style.modeTabActive]"
        @click="mode = 'rfq'"
      >
        <ClipboardList
          :size="14"
          :stroke-width="1.8"
        />询价单管理
      </button>
      <button
        type="button"
        :class="[$style.modeTab, mode === 'business' && $style.modeTabActive]"
        @click="mode = 'business'"
      >
        <BriefcaseBusiness
          :size="14"
          :stroke-width="1.8"
        />业务询价任务<span>{{ pendingTaskCount }}</span>
      </button>
    </nav>
    <template v-if="mode === 'rfq'">
      <InquiryOverview
        v-model:scope="scope"
        :rows="rows"
      />
      <InquiryToolbar
        v-model:status="status"
        v-model:query="query"
        @export="exportRows"
        @create="formOpen = true"
      />
      <InquiryTable
        :rows="filteredRows"
        :sort-key="sortKey"
        :sort-direction="sortDirection"
        @sort="updateSort"
        @view="openDetail"
        @copy-link="copyLink"
      />
    </template>
    <BusinessInquiryTasks
      v-else
      :tasks="businessInquiryTasks"
      @open-module="router.push('/purchase/biz-inquiries')"
      @view="toast.info($event.title)"
    />
  </main>

  <InquiryFormDialog
    v-model:open="formOpen"
    @save="saveInquiry"
  />
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.title ?? '询价单详情'"
    :description="activeRow?.id"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>负责采购</dt>
        <dd>{{ activeRow.assignedBuyer || '未分配' }}</dd>
      </div>
      <div>
        <dt>关联项目</dt>
        <dd>{{ activeRow.projectName === '-' ? '无关联项目' : activeRow.projectName }}</dd>
      </div>
      <div>
        <dt>来源</dt>
        <dd>{{ activeRow.sourceType }} · {{ activeRow.sourceId }}</dd>
      </div>
      <div>
        <dt>供应商报价</dt>
        <dd>{{ activeRow.quotedCount }}/{{ activeRow.supplierCount }}</dd>
      </div>
      <div>
        <dt>物料数</dt>
        <dd>{{ activeRow.materialCount }}</dd>
      </div>
      <div>
        <dt>预算金额</dt>
        <dd>¥{{ activeRow.totalBudget.toLocaleString('en-US') }}</dd>
      </div>
      <div>
        <dt>截止日期</dt>
        <dd>{{ activeRow.deadline }}</dd>
      </div>
      <div
        v-if="activeRow.closeReason"
        :class="$style.wide"
      >
        <dt>关闭原因</dt>
        <dd>{{ activeRow.closeReason }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton @click="copyLink(activeRow!)">复制报价链接</UiButton
      ><UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.modeTabs {
  @apply mb-5 flex items-center gap-1 rounded-lg border bg-white p-1;
  border-color: var(--ui-border);
}
.modeTab {
  @apply flex min-h-10 items-center gap-2 rounded px-5 py-2.5 text-[13px] transition-colors;
  color: #42526e;
}
.modeTab:hover {
  background: #f4f5f7;
}
.modeTab span {
  @apply rounded-full px-1.5 py-0.5 text-[10px] font-bold text-white;
  background: #de350b;
}
.modeTabActive {
  @apply font-semibold text-white shadow-sm;
  background: #0052cc;
}
.modeTabActive:hover {
  background: #0052cc;
}
.modeTabActive span {
  background: rgb(255 255 255 / 20%);
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[13px] font-medium;
  color: var(--ui-text);
}
.wide {
  @apply col-span-2;
}
</style>
