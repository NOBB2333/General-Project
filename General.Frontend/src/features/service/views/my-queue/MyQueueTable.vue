<script setup lang="ts">
import { ArrowLeftRight } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { MyQueueTabId, MyQueueTicketRow } from '../../model/my-queue.types';

defineProps<{
  rows: readonly MyQueueTicketRow[];
  activeTab: MyQueueTabId;
  ownQueue: boolean;
}>();

const emit = defineEmits<{
  open: [row: MyQueueTicketRow];
}>();
</script>

<template>
  <section :class="$style.frame">
    <span :class="$style.scrollHint">
      <ArrowLeftRight
        :size="13"
        :stroke-width="1.8"
      />
      左右滑动
    </span>

    <div :class="$style.scroller">
      <table :class="$style.table">
        <colgroup>
          <col :class="$style.codeColumn" />
          <col :class="$style.typeColumn" />
          <col :class="$style.titleColumn" />
          <col :class="$style.customerColumn" />
          <col :class="$style.productColumn" />
          <col :class="$style.urgencyColumn" />
          <col :class="$style.statusColumn" />
          <col :class="$style.timeColumn" />
          <col :class="$style.modeColumn" />
          <col :class="$style.actionColumn" />
        </colgroup>
        <thead>
          <tr>
            <th>工单编号</th>
            <th>类型</th>
            <th>工单标题</th>
            <th>客户 / 联系人</th>
            <th>产品 / SN</th>
            <th>紧急</th>
            <th>状态</th>
            <th>预约 / 创建</th>
            <th>{{ ownQueue ? '服务方式' : '负责人' }}</th>
            <th :class="$style.actionHeading">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <button
                type="button"
                :class="$style.codeLink"
                @click="emit('open', row)"
              >
                {{ row.code }}
              </button>
            </td>
            <td>{{ row.type }}</td>
            <td :class="$style.primaryCell">{{ row.title }}</td>
            <td>
              <span :class="$style.stackedCell">
                <strong>{{ row.customerName }}</strong>
                <small>{{ row.contactPerson }} · {{ row.contactPhone }}</small>
              </span>
            </td>
            <td>
              <span :class="$style.stackedCell">
                <strong>{{ row.productName }}</strong>
                <small>{{ row.serialNumber }}</small>
              </span>
            </td>
            <td>{{ row.urgency }}</td>
            <td>{{ row.status }}</td>
            <td>
              <span :class="$style.stackedCell">
                <strong>{{ row.planStart || '未预约' }}</strong>
                <small>建 {{ row.createdAt }}</small>
              </span>
            </td>
            <td>{{ ownQueue ? row.serviceMode || '—' : row.assignee || '—' }}</td>
            <td :class="$style.actionCell">
              <UiButton
                variant="secondary"
                size="xs"
                @click="emit('open', row)"
              >
                查看
              </UiButton>
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              :class="$style.emptyCell"
              colspan="10"
            >
              「{{ activeTab }}」当前筛选条件下暂无工单
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.frame {
  @apply relative min-w-0 overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}

.scroller {
  @apply min-w-0 overflow-x-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}

.table {
  @apply w-full table-fixed border-collapse text-left text-[11px];
  min-width: 1420px;
  color: var(--ui-text);
}

.table thead {
  background: #f4f5f7;
}

.table th {
  @apply px-3 py-3 font-semibold;
  height: 42px;
  color: #5e6c84;
}

.table td {
  @apply border-t px-3 py-3 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:not(:last-child) {
  @apply transition-colors;
}

.table tbody tr:not(:last-child):hover {
  background: #fafbfc;
}

.codeColumn {
  width: 140px;
}

.typeColumn {
  width: 84px;
}

.titleColumn {
  width: 230px;
}

.customerColumn {
  width: 160px;
}

.productColumn {
  width: 180px;
}

.urgencyColumn {
  width: 60px;
}

.statusColumn {
  width: 90px;
}

.timeColumn {
  width: 150px;
}

.modeColumn {
  width: 100px;
}

.actionColumn {
  width: 210px;
}

.actionHeading,
.actionCell {
  @apply text-center;
}

.scrollHint {
  @apply absolute right-2 top-2 z-[2] flex items-center gap-1 rounded-full border px-2 py-1 text-[10px] font-semibold shadow-sm;
  color: var(--app-primary);
  background: #deebff;
  border-color: #b3d4ff;
}

.emptyCell {
  @apply py-16 text-center text-[13px];
  color: var(--ui-text-subtle);
}

.codeLink {
  @apply font-mono text-[12px] font-semibold hover:underline;
  color: var(--app-primary);
}

.primaryCell {
  @apply truncate text-[12px] font-medium;
}

.stackedCell {
  @apply block;
}

.stackedCell strong {
  @apply block truncate text-[12px] font-normal;
}

.stackedCell small {
  @apply mt-0.5 block truncate text-[10px];
  color: var(--ui-text-subtle);
}
</style>
