<script setup lang="ts">
import { FileSpreadsheet, FileText, Presentation } from 'lucide-vue-next';
import type { DocumentRecord } from '../../model/docs.types';

defineProps<{ rows: readonly DocumentRecord[] }>();
const emit = defineEmits<{ view: [document: DocumentRecord] }>();
</script>

<template>
  <div :class="$style.tableWrap">
    <table>
      <thead>
        <tr>
          <th>文件名</th>
          <th>大小</th>
          <th>上传者</th>
          <th>更新时间</th>
          <th>版本</th>
          <th>下载</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          tabindex="0"
          @click="emit('view', row)"
          @keydown.enter="emit('view', row)"
        >
          <td>
            <span
              :class="$style.fileIcon"
              :data-type="row.type"
              ><Presentation
                v-if="row.type === 'PPTX'"
                :size="16"
                :stroke-width="1.8" /><FileSpreadsheet
                v-else-if="row.type === 'XLSX'"
                :size="16"
                :stroke-width="1.8" /><FileText
                v-else
                :size="16"
                :stroke-width="1.8"
            /></span>
            <div>
              <strong>{{ row.name }}</strong
              ><em v-if="row.favorite">★</em><small v-if="row.references">↪{{ row.references }}</small>
              <p>{{ row.description }}</p>
            </div>
          </td>
          <td>{{ row.size }}</td>
          <td>
            <span :class="$style.avatar">{{ row.publisher.slice(0, 1) }}</span
            >{{ row.publisher }}
          </td>
          <td>{{ row.updatedAt }}</td>
          <td>
            <b>{{ row.version }}</b>
          </td>
          <td>{{ row.downloads }}次</td>
        </tr>
      </tbody>
    </table>
    <p
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前文件夹或筛选条件下没有文档
    </p>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.tableWrap {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.tableWrap table {
  @apply w-full min-w-[900px] border-collapse text-left text-[10px];
}
.tableWrap th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-hover);
}
.tableWrap td {
  @apply h-[64px] whitespace-nowrap border-b px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.tableWrap tbody tr {
  @apply cursor-pointer outline-none transition-colors;
}
.tableWrap tbody tr:hover {
  background: var(--ui-surface-hover);
}
.tableWrap tbody tr:focus-visible {
  box-shadow: inset 0 0 0 2px var(--ui-focus);
}
.tableWrap td:first-child {
  @apply flex items-center gap-3;
}
.fileIcon {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded;
  color: #0052cc;
  background: #deebff;
}
.fileIcon[data-type='PDF'] {
  color: #de350b;
  background: #ffebe6;
}
.fileIcon[data-type='PPTX'] {
  color: #ff8b00;
  background: #fff0b3;
}
.fileIcon[data-type='XLSX'] {
  color: #00875a;
  background: #e3fcef;
}
.tableWrap td:first-child div {
  @apply min-w-0;
}
.tableWrap td strong {
  @apply font-semibold;
  color: var(--ui-text);
}
.tableWrap td em {
  @apply ml-1 not-italic;
  color: #ffab00;
}
.tableWrap td small {
  @apply ml-1 rounded px-1 text-[8px];
  color: #6554c0;
  background: #eae6ff;
}
.tableWrap td p {
  @apply mt-1 max-w-[330px] truncate text-[9px];
  color: var(--ui-text-subtle);
}
.avatar {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full text-[8px] font-semibold text-white;
  background: #0052cc;
}
.tableWrap td b {
  @apply rounded px-2 py-1 text-[8px];
  background: var(--ui-surface-hover);
}
.empty {
  @apply py-16 text-center text-[11px];
  color: var(--ui-text-subtle);
}
</style>
