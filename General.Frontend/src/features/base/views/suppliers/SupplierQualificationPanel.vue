<script setup lang="ts">
import { computed, ref } from 'vue';
import {
  Ban,
  ChevronDown,
  Eye,
  FileCheck2,
  Grid2X2,
  List,
  Search,
  Star,
  TriangleAlert,
  XCircle,
} from 'lucide-vue-next';
import {
  UiDialog,
  UiDropdownMenu,
  UiInput,
  UiSelect,
  type UiDropdownMenuItem,
  type UiSelectOption,
  toast,
} from '@/components/ui';
import { supplierCertificates, supplierQualificationSummaries } from '../../model/suppliers.data';
import type {
  QualificationView,
  SupplierBusinessStatus,
  SupplierCertificate,
  SupplierQualificationSummary,
} from '../../model/suppliers.types';

const summaries = ref<SupplierQualificationSummary[]>(supplierQualificationSummaries.map((item) => ({ ...item })));
const activeView = ref<QualificationView>('supplier');
const query = ref('');
const status = ref('all');
const detailOpen = ref(false);
const activeDetail = ref<SupplierQualificationSummary | SupplierCertificate | null>(null);

const businessStatusOptions: UiSelectOption[] = [
  { value: 'all', label: '供应商状态: 全部' },
  { value: '启用', label: '启用' },
  { value: '停用', label: '停用' },
  { value: '暂停', label: '暂停' },
  { value: '拉黑', label: '拉黑' },
];
const certificateStatusOptions: UiSelectOption[] = [
  { value: 'all', label: '资质状态: 全部' },
  { value: '有效', label: '有效' },
  { value: '即将到期', label: '即将到期' },
  { value: '已过期', label: '已过期' },
];
const statusItems: readonly UiDropdownMenuItem[] = [
  { id: '启用', label: '启用' },
  { id: '停用', label: '停用' },
  { id: '暂停', label: '暂停' },
  { id: '拉黑', label: '拉黑', tone: 'danger', separatorBefore: true },
];
const businessClass: Record<SupplierBusinessStatus, string> = {
  启用: 'businessActive',
  停用: 'businessInactive',
  暂停: 'businessPaused',
  拉黑: 'businessBlocked',
};
const certificateClass: Record<SupplierCertificate['status'], string> = {
  有效: 'certificateValid',
  即将到期: 'certificateExpiring',
  已过期: 'certificateExpired',
};

const filteredSummaries = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return summaries.value.filter((item) => {
    const matchesQuery = !keyword || item.supplierName.toLowerCase().includes(keyword);
    const matchesStatus = status.value === 'all' || item.businessStatus === status.value;
    return matchesQuery && matchesStatus;
  });
});
const filteredCertificates = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return supplierCertificates.filter((item) => {
    const haystack = `${item.name} ${item.supplierName} ${item.certificateNumber}`.toLowerCase();
    const matchesQuery = !keyword || haystack.includes(keyword);
    const matchesStatus = status.value === 'all' || item.status === status.value;
    return matchesQuery && matchesStatus;
  });
});

function setView(view: QualificationView) {
  activeView.value = view;
  query.value = '';
  status.value = 'all';
}
function updateBusinessStatus(supplierId: string, value: string) {
  if (!['启用', '停用', '暂停', '拉黑'].includes(value)) return;
  const item = summaries.value.find((summary) => summary.supplierId === supplierId);
  if (!item) return;
  item.businessStatus = value as SupplierBusinessStatus;
  toast.success(`供应商状态已更新为「${value}」`);
}
function openDetail(detail: SupplierQualificationSummary | SupplierCertificate) {
  activeDetail.value = detail;
  detailOpen.value = true;
}
function isCertificate(detail: SupplierQualificationSummary | SupplierCertificate): detail is SupplierCertificate {
  return 'certificateNumber' in detail;
}
</script>

<template>
  <section data-supplier-qualification-panel>
    <div :class="$style.stats">
      <article>
        <p><FileCheck2 :size="12" />资质总数</p>
        <strong>13</strong>
      </article>
      <article :class="$style.warning">
        <p><TriangleAlert :size="12" />即将到期</p>
        <strong>3</strong>
      </article>
      <article :class="$style.danger">
        <p><XCircle :size="12" />已过期</p>
        <strong>2</strong>
      </article>
      <article>
        <p><Ban :size="12" />拉黑供应商</p>
        <strong>{{ summaries.filter((item) => item.businessStatus === '拉黑').length }}</strong>
      </article>
    </div>

    <div :class="$style.toolbar">
      <label :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        />
        <UiInput
          v-model="query"
          :aria-label="activeView === 'supplier' ? '搜索供应商名称' : '搜索资质名称、供应商、证书编号'"
          :placeholder="activeView === 'supplier' ? '搜索供应商名称...' : '搜索资质名称、供应商、证书编号...'"
        />
      </label>
      <UiSelect
        v-model="status"
        :options="activeView === 'supplier' ? businessStatusOptions : certificateStatusOptions"
        label="状态筛选"
        size="sm"
      />
      <div :class="$style.segmented">
        <button
          type="button"
          :class="[$style.segment, activeView === 'supplier' && $style.segmentActive]"
          :aria-pressed="activeView === 'supplier'"
          @click="setView('supplier')"
        >
          <List
            :size="13"
            :stroke-width="1.8"
          />供应商视图
        </button>
        <button
          type="button"
          :class="[$style.segment, activeView === 'certificate' && $style.segmentActive]"
          :aria-pressed="activeView === 'certificate'"
          @click="setView('certificate')"
        >
          <Grid2X2
            :size="13"
            :stroke-width="1.8"
          />资质证书视图
        </button>
      </div>
    </div>

    <div :class="$style.frame">
      <div :class="$style.scroll">
        <table
          v-if="activeView === 'supplier'"
          :class="[$style.table, $style.supplierTable]"
        >
          <thead>
            <tr>
              <th>供应商名称</th>
              <th>供应商状态</th>
              <th>资质数量</th>
              <th>有效</th>
              <th>即将到期</th>
              <th>已过期</th>
              <th>综合评分</th>
              <th>负责人</th>
              <th>详情</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in filteredSummaries"
              :key="item.supplierId"
            >
              <td>
                <button
                  type="button"
                  :class="$style.link"
                  @click="openDetail(item)"
                >
                  {{ item.supplierName }}
                </button>
              </td>
              <td :class="$style.center">
                <UiDropdownMenu
                  :items="statusItems"
                  :label="`修改 ${item.supplierName} 的状态`"
                  align="center"
                  @select="updateBusinessStatus(item.supplierId, $event)"
                >
                  <template #trigger>
                    <button
                      type="button"
                      :class="[$style.status, $style[businessClass[item.businessStatus]]]"
                    >
                      {{ item.businessStatus
                      }}<ChevronDown
                        :size="10"
                        :stroke-width="1.8"
                      />
                    </button>
                  </template>
                </UiDropdownMenu>
              </td>
              <td :class="$style.center">{{ item.qualificationCount }}</td>
              <td :class="[$style.center, $style.successText]">{{ item.validCount }}</td>
              <td :class="$style.center">
                <span :class="item.expiringCount > 0 ? $style.warningBadge : undefined">{{ item.expiringCount }}</span>
              </td>
              <td :class="$style.center">
                <span :class="item.expiredCount > 0 ? $style.dangerBadge : undefined">{{ item.expiredCount }}</span>
              </td>
              <td :class="$style.center">
                <span :class="$style.score"><Star :size="12" />{{ item.score.toFixed(1) }}</span>
              </td>
              <td>{{ item.responsible }}</td>
              <td :class="$style.center">
                <button
                  type="button"
                  :class="$style.detailButton"
                  aria-label="查看资质详情"
                  @click="openDetail(item)"
                >
                  <Eye
                    :size="14"
                    :stroke-width="1.8"
                  />
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <table
          v-else
          :class="[$style.table, $style.certificateTable]"
        >
          <thead>
            <tr>
              <th>资质名称</th>
              <th>供应商</th>
              <th>证书编号</th>
              <th>有效期</th>
              <th>提醒</th>
              <th>负责人</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in filteredCertificates"
              :key="item.id"
            >
              <td>
                <button
                  type="button"
                  :class="$style.link"
                  @click="openDetail(item)"
                >
                  {{ item.name }}
                </button>
              </td>
              <td>{{ item.supplierName }}</td>
              <td>{{ item.certificateNumber }}</td>
              <td :class="$style.center">{{ item.issueDate }} 至 {{ item.expiryDate }}</td>
              <td :class="$style.center">{{ item.reminderDays }}天</td>
              <td>{{ item.responsible }}</td>
              <td :class="$style.center">
                <span :class="[$style.status, $style[certificateClass[item.status]]]">{{ item.status }}</span>
              </td>
              <td :class="$style.center">
                <button
                  type="button"
                  :class="$style.detailButton"
                  aria-label="查看证书详情"
                  @click="openDetail(item)"
                >
                  <Eye
                    :size="14"
                    :stroke-width="1.8"
                  />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <footer :class="$style.footer">
        共 {{ activeView === 'supplier' ? filteredSummaries.length : filteredCertificates.length }} 条记录
      </footer>
    </div>
  </section>

  <UiDialog
    v-model:open="detailOpen"
    :title="activeDetail ? (isCertificate(activeDetail) ? activeDetail.name : activeDetail.supplierName) : '资质详情'"
    description="资质信息来自参考站只读快照"
    size="md"
  >
    <dl
      v-if="activeDetail"
      :class="$style.details"
    >
      <template v-if="isCertificate(activeDetail)">
        <div>
          <dt>供应商</dt>
          <dd>{{ activeDetail.supplierName }}</dd>
        </div>
        <div>
          <dt>证书编号</dt>
          <dd>{{ activeDetail.certificateNumber }}</dd>
        </div>
        <div>
          <dt>有效期</dt>
          <dd>{{ activeDetail.issueDate }} 至 {{ activeDetail.expiryDate }}</dd>
        </div>
        <div>
          <dt>负责人</dt>
          <dd>{{ activeDetail.responsible }}</dd>
        </div>
      </template>
      <template v-else>
        <div>
          <dt>业务状态</dt>
          <dd>{{ activeDetail.businessStatus }}</dd>
        </div>
        <div>
          <dt>综合评分</dt>
          <dd>{{ activeDetail.score.toFixed(1) }}</dd>
        </div>
        <div>
          <dt>资质数量</dt>
          <dd>{{ activeDetail.qualificationCount }}</dd>
        </div>
        <div>
          <dt>负责人</dt>
          <dd>{{ activeDetail.responsible }}</dd>
        </div>
      </template>
    </dl>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.stats {
  @apply mb-5 grid grid-cols-4 gap-4;
}
.stats article {
  @apply rounded-lg border bg-white p-4 transition-shadow;
  border-color: var(--ui-border);
}
.stats article:hover {
  @apply shadow-md;
}
.stats p {
  @apply flex items-center gap-1.5 text-[12px];
  color: var(--ui-text-subtle);
}
.stats strong {
  @apply mt-1 block text-[22px] font-bold;
  color: var(--ui-text);
}
.warning p,
.warning strong {
  color: var(--ui-warning);
}
.danger p,
.danger strong {
  color: var(--ui-danger);
}
.toolbar {
  @apply mb-4 flex items-center gap-3;
}
.search {
  @apply relative block w-[300px];
}
.search > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}
.search input {
  @apply h-8 border-0 bg-[#F4F5F7] pl-8;
  box-shadow: none;
}
.segmented {
  @apply ml-auto flex rounded-lg bg-[#F4F5F7] p-0.5;
}
.segment {
  @apply flex min-h-8 items-center gap-1.5 rounded px-3 text-[12px] transition-colors;
  color: var(--ui-text-subtle);
}
.segmentActive {
  @apply bg-white font-semibold shadow-sm;
  color: var(--app-primary);
}
.frame {
  @apply overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.scroll {
  @apply overflow-x-auto;
}
.table {
  @apply w-full table-fixed border-collapse text-left text-[12px];
  color: var(--ui-text-muted);
}
.supplierTable {
  @apply min-w-[1100px];
}
.certificateTable {
  @apply min-w-[1200px];
}
.table th {
  @apply h-10 border-b bg-[#F4F5F7] px-3 text-[11px] font-semibold;
  color: #5e6c84;
  border-color: var(--ui-border);
}
.table td {
  @apply h-[52px] border-b px-3;
  border-color: var(--ui-border-subtle);
}
.table tr:hover {
  background: var(--ui-surface-subtle);
}
.center {
  @apply text-center;
}
.link {
  @apply font-medium;
  color: var(--app-primary);
}
.link:hover {
  text-decoration: underline;
}
.status {
  @apply inline-flex items-center gap-1 rounded px-2 py-0.5 text-[10px] font-semibold;
}
.businessActive,
.certificateValid {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.businessInactive {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.businessPaused,
.certificateExpiring {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.businessBlocked,
.certificateExpired {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.successText {
  color: var(--ui-success);
}
.warningBadge,
.dangerBadge {
  @apply inline-flex min-w-6 justify-center rounded px-2 py-0.5 text-[11px] font-semibold;
}
.warningBadge {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.dangerBadge {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.score {
  @apply inline-flex items-center gap-1 font-semibold;
  color: var(--ui-text);
}
.score svg {
  color: #ffab00;
  fill: #ffab00;
}
.detailButton {
  @apply inline-flex h-7 w-7 items-center justify-center rounded transition-colors;
  color: var(--ui-text-subtle);
}
.detailButton:hover {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.footer {
  @apply border-t px-5 py-4 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
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
@media (max-width: 820px) {
  .stats {
    @apply grid-cols-2;
  }
  .toolbar {
    @apply flex-wrap;
  }
  .segmented {
    @apply ml-0;
  }
}
</style>
