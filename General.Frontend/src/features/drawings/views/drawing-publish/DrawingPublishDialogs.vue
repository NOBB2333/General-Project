<script setup lang="ts">
import { CalendarClock, CheckCircle2, Send } from 'lucide-vue-next';
import { UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';
import { drawingPublishModel } from '@/features/drawings/model/drawing-publish.model';
import type { DrawingPublishHistoryRow, DrawingPublishRow } from '@/features/drawings/model/drawing-publish.types';

defineProps<{
  activeRow: DrawingPublishRow | null;
  historyRows: readonly DrawingPublishHistoryRow[];
  detailOpen: boolean;
  historyOpen: boolean;
  settingsOpen: boolean;
  confirmOpen: boolean;
  settingsDate: string;
}>();

const emit = defineEmits<{
  'update:detailOpen': [value: boolean];
  'update:historyOpen': [value: boolean];
  'update:settingsOpen': [value: boolean];
  'update:confirmOpen': [value: boolean];
  'update:settingsDate': [value: string];
  check: [];
  settings: [];
  'save-settings': [];
  publish: [];
  'confirm-publish': [];
}>();
</script>

<template>
  <UiDialog
    :open="detailOpen"
    :title="activeRow?.drawingNo ?? '图纸发布详情'"
    :description="activeRow ? `${activeRow.name} · ${activeRow.publishStatus}` : undefined"
    size="lg"
    @update:open="emit('update:detailOpen', $event)"
  >
    <dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div
        v-for="column in drawingPublishModel.columns.filter((item) => item.key !== 'action')"
        :key="column.key"
      >
        <dt>{{ column.title }}</dt>
        <dd>{{ activeRow[column.key] || '—' }}</dd>
      </div>
    </dl>
    <template #footer>
      <UiButton @click="emit('update:detailOpen', false)">关闭</UiButton>
      <UiButton @click="emit('check')"
        ><template #leading
          ><CheckCircle2
            :size="14"
            :stroke-width="1.8" /></template
        >检查</UiButton
      >
      <UiButton @click="emit('settings')"
        ><template #leading
          ><CalendarClock
            :size="14"
            :stroke-width="1.8" /></template
        >设置</UiButton
      >
      <UiButton
        variant="primary"
        @click="emit('publish')"
        ><template #leading
          ><Send
            :size="14"
            :stroke-width="1.8" /></template
        >发布</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="historyOpen"
    title="最近发布记录"
    description="参考站已发布记录的只读快照；本地发布会追加到当前会话。"
    size="xl"
    @update:open="emit('update:historyOpen', $event)"
  >
    <div :class="$style.historyScroller">
      <table :class="$style.historyTable">
        <thead>
          <tr>
            <th>图号</th>
            <th>图纸名称</th>
            <th>发布版本</th>
            <th>原版本</th>
            <th>发布时间</th>
            <th>发布人</th>
            <th>生效日期</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in historyRows"
            :key="row.id"
          >
            <td>{{ row.drawingNo }}</td>
            <td>{{ row.name }}</td>
            <td>{{ row.publishedVersion }}</td>
            <td>{{ row.previousVersion }}</td>
            <td>{{ row.publishedAt }}</td>
            <td>{{ row.publisher }}</td>
            <td>{{ row.effectiveDate }}</td>
          </tr>
        </tbody>
      </table>
    </div>
    <template #footer><UiButton @click="emit('update:historyOpen', false)">关闭</UiButton></template>
  </UiDialog>

  <UiDialog
    :open="settingsOpen"
    :title="`设置 ${activeRow?.drawingNo ?? ''} 发布参数`"
    description="设置只保存在当前本地复刻会话。"
    @update:open="emit('update:settingsOpen', $event)"
  >
    <div :class="$style.settingBody">
      <UiFormField label="建议生效日期"
        ><UiInput
          :model-value="settingsDate"
          type="date"
          @update:model-value="emit('update:settingsDate', $event)"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton @click="emit('update:settingsOpen', false)">取消</UiButton
      ><UiButton
        variant="primary"
        @click="emit('save-settings')"
        >保存设置</UiButton
      ></template
    >
  </UiDialog>

  <UiAlertDialog
    :open="confirmOpen"
    title="确认发布图纸？"
    :description="
      activeRow
        ? `${activeRow.drawingNo} ${activeRow.pendingVersion} 将进入已发布状态，变更仅保存在本地会话。`
        : undefined
    "
    confirm-label="确认发布"
    tone="warning"
    @update:open="emit('update:confirmOpen', $event)"
    @confirm="emit('confirm-publish')"
  />
</template>

<style module>
@reference "../../../../styles/index.css";

.detailGrid {
  @apply grid grid-cols-2 gap-px;
  background: var(--ui-border-subtle);
}
.detailGrid div {
  @apply min-w-0 bg-white px-4 py-3;
}
.detailGrid dt {
  @apply text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
.detailGrid dd {
  @apply mt-1 truncate text-[13px];
  color: var(--ui-text);
}
.historyScroller {
  @apply overflow-auto;
}
.historyTable {
  @apply w-full min-w-[760px] border-collapse text-left text-[12px];
}
.historyTable th {
  @apply border-b px-4 py-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.historyTable td {
  @apply border-b px-4 py-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.settingBody {
  @apply p-5;
}
</style>
