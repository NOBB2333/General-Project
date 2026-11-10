<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiAlertDialog, UiButton, UiDialog, toast } from '@/components/ui';
import { suppliersReferenceFixture } from '../model/suppliers.data';
import type { SupplierDraft, SupplierRow, SupplierScope, SupplierView } from '../model/suppliers.types';
import SupplierFormDialog from './suppliers/SupplierFormDialog.vue';
import SupplierHero from './suppliers/SupplierHero.vue';
import SupplierPageTabs from './suppliers/SupplierPageTabs.vue';
import SupplierQualificationPanel from './suppliers/SupplierQualificationPanel.vue';
import SupplierScopeBar from './suppliers/SupplierScopeBar.vue';
import SupplierStats from './suppliers/SupplierStats.vue';
import SupplierTable from './suppliers/SupplierTable.vue';

const rows = ref<SupplierRow[]>(cloneReferenceRows(suppliersReferenceFixture.rows));
const activeView = ref<SupplierView>('list');
const scope = ref<SupplierScope>('all');
const query = ref('');
const followedIds = ref<Set<string>>(new Set(suppliersReferenceFixture.defaults.followedIds));
const detailOpen = ref(false);
const formOpen = ref(false);
const deleteOpen = ref(false);
const activeRow = ref<SupplierRow | null>(null);
const editingRow = ref<SupplierRow | null>(null);
const pendingDelete = ref<SupplierRow | null>(null);

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return rows.value.filter((row) => {
    const matchesQuery = !keyword || row.name.toLowerCase().includes(keyword) || row.contact.includes(keyword);
    const matchesScope =
      scope.value === 'all' ||
      (scope.value === 'mine' && row.ownerRole === 'self') ||
      (scope.value === 'subordinate' && row.ownerRole === 'subordinate') ||
      (scope.value === 'starred' && followedIds.value.has(row.id));
    return matchesQuery && matchesScope;
  });
});
const totalPurchase = computed(() => rows.value.reduce((total, row) => total + row.totalPurchase, 0));
const totalPayable = computed(() => rows.value.reduce((total, row) => total + row.payable, 0));
const purchaseLabel = computed(() => `¥${(totalPurchase.value / 10_000).toFixed(0)}万`);
const payableLabel = computed(() => `¥${(totalPayable.value / 10_000).toFixed(1)}万`);

function openDetail(row: SupplierRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function openCreate() {
  editingRow.value = null;
  formOpen.value = true;
}
function openEdit(row: SupplierRow) {
  editingRow.value = row;
  formOpen.value = true;
}
function editActiveRow() {
  if (!activeRow.value) return;
  detailOpen.value = false;
  openEdit(activeRow.value);
}
function requestRemove(row: SupplierRow) {
  pendingDelete.value = row;
  deleteOpen.value = true;
}
function confirmRemove() {
  if (!pendingDelete.value) return;
  const removed = pendingDelete.value;
  rows.value = rows.value.filter((row) => row.id !== removed.id);
  const nextFollowed = new Set(followedIds.value);
  nextFollowed.delete(removed.id);
  followedIds.value = nextFollowed;
  deleteOpen.value = false;
  pendingDelete.value = null;
  toast.success(`${removed.name} 已从本地列表删除`);
}
function toggleFollow(id: string) {
  const nextFollowed = new Set(followedIds.value);
  if (nextFollowed.has(id)) {
    nextFollowed.delete(id);
    toast.info('已取消关注');
  } else {
    nextFollowed.add(id);
    toast.success('已关注');
  }
  followedIds.value = nextFollowed;
}
function saveSupplier(draft: SupplierDraft, submitForApproval: boolean) {
  const current = editingRow.value;
  if (current) {
    Object.assign(current, {
      name: draft.name.trim(),
      contact: draft.contact.trim(),
      phone: draft.phone.trim(),
      payMethod: draft.payMethod,
      paymentDays: Number(draft.paymentDays) || 0,
      level: draft.level,
      owner: draft.owner,
      ownerRole: draft.owner === '张经理' ? 'self' : 'subordinate',
      status: submitForApproval ? '待审批' : current.status,
    });
  } else {
    rows.value = [
      {
        id: `S-LOCAL-${Date.now()}`,
        name: draft.name.trim(),
        contact: draft.contact.trim() || '—',
        phone: draft.phone.trim() || '—',
        payMethod: draft.payMethod,
        paymentDays: Number(draft.paymentDays) || 0,
        totalPurchase: 0,
        payable: 0,
        level: draft.level,
        owner: draft.owner || '未分配',
        ownerRole: draft.owner === '张经理' ? 'self' : 'subordinate',
        status: submitForApproval ? '待审批' : '草稿',
      },
      ...rows.value,
    ];
  }
  formOpen.value = false;
  editingRow.value = null;
  toast.success(submitForApproval ? '供应商信息已提交审核' : '供应商草稿已保存');
}
function exportSuppliers() {
  const header = ['供应商名称', '级别', '联系人', '联系电话', '负责人', '付款方式', '历史采购额', '应付余额', '状态'];
  const csvRows = filteredRows.value.map((row) => [
    row.name,
    row.level,
    row.contact,
    row.phone,
    row.owner,
    row.payMethod,
    row.totalPurchase,
    row.payable,
    row.status,
  ]);
  const csv = [header, ...csvRows]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const anchor = document.createElement('a');
  anchor.href = href;
  anchor.download = '供应商列表.csv';
  anchor.click();
  URL.revokeObjectURL(href);
  toast.success(`已导出 ${filteredRows.value.length} 条供应商记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/base/suppliers"
    data-suppliers-page
    :data-active-view="activeView"
    :data-active-scope="scope"
  >
    <SupplierHero />
    <SupplierPageTabs
      v-model="activeView"
      @export="exportSuppliers"
      @create="openCreate"
    />

    <template v-if="activeView === 'list'">
      <SupplierStats
        :total="rows.length"
        :purchase="purchaseLabel"
        :payable="payableLabel"
      />
      <SupplierScopeBar
        v-model="scope"
        v-model:query="query"
        :followed-count="followedIds.size"
      />
      <SupplierTable
        :rows="filteredRows"
        :followed-ids="followedIds"
        @open="openDetail"
        @edit="openEdit"
        @remove="requestRemove"
        @toggle-follow="toggleFollow"
      />
    </template>
    <SupplierQualificationPanel v-else />
  </main>

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.name ?? '供应商详情'"
    :description="activeRow?.id"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>供应商级别</dt>
        <dd>{{ activeRow.level }}</dd>
      </div>
      <div>
        <dt>审批状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>联系人</dt>
        <dd>{{ activeRow.contact }} · {{ activeRow.phone }}</dd>
      </div>
      <div>
        <dt>负责人</dt>
        <dd>{{ activeRow.owner }}</dd>
      </div>
      <div>
        <dt>付款方式</dt>
        <dd>{{ activeRow.payMethod }} · {{ activeRow.paymentDays }} 天</dd>
      </div>
      <div>
        <dt>应付余额</dt>
        <dd>¥{{ activeRow.payable.toLocaleString('en-US') }}</dd>
      </div>
    </dl>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      >
      <UiButton
        variant="primary"
        @click="editActiveRow"
        >编辑供应商</UiButton
      >
    </template>
  </UiDialog>

  <SupplierFormDialog
    v-model:open="formOpen"
    :editing="editingRow"
    @save="saveSupplier"
  />

  <UiAlertDialog
    v-model:open="deleteOpen"
    title="删除供应商"
    :description="`确认从本地列表删除“${pendingDelete?.name ?? ''}”吗？此操作不会影响外部参考站。`"
    confirm-label="确认删除"
    @confirm="confirmRemove"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] pb-8;
  color: var(--ui-text);
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
</style>
