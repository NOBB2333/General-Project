<script setup lang="ts">
import { Star } from 'lucide-vue-next';
import { UiCheckbox } from '@/components/ui';
import type { LeadRow, LeadSource, LeadStatus } from '@/features/crm/model/leads.types';

defineProps<{ row: LeadRow; selected: boolean }>();
const emit = defineEmits<{ select: [id: string, selected: boolean]; convert: [id: string] }>();

const currency = new Intl.NumberFormat('zh-CN');
const statusClass: Record<LeadStatus, string> = {
  新线索: 'statusNew',
  已联系: 'statusContacted',
  已确认: 'statusConfirmed',
  已转化: 'statusConverted',
};
const sourceClass: Record<LeadSource, string> = {
  展会: 'sourceExhibition',
  网站: 'sourceWeb',
  转介绍: 'sourceReferral',
  电话开发: 'sourcePhone',
  客户推荐: 'sourceReferral',
};
</script>

<template>
  <tr :class="$style.row">
    <td>
      <UiCheckbox
        :model-value="selected"
        :label="`选择${row.code}`"
        @update:model-value="emit('select', row.id, $event === true)"
      />
    </td>
    <td>
      <strong :class="$style.code">{{ row.code }}</strong>
    </td>
    <td>
      <strong :class="$style.company">{{ row.company }}</strong>
      <span :class="$style.tags"
        ><i v-if="row.intent">{{ row.intent }}</i
        ><i>{{ row.industry }}</i></span
      >
    </td>
    <td>
      <strong>{{ row.contact }}</strong
      ><small>{{ row.role }}</small>
    </td>
    <td>
      <span :class="[$style.badge, $style[statusClass[row.status]]]">{{ row.status }}</span>
    </td>
    <td>
      <span :class="[$style.badge, $style[sourceClass[row.source]]]">{{ row.source }}</span>
    </td>
    <td>
      <span :class="$style.score"><Star :size="13" />{{ row.score }}</span>
    </td>
    <td>
      <strong :class="$style.amount">¥{{ currency.format(row.amount) }}</strong>
    </td>
    <td>
      <strong>{{ row.region }}</strong
      ><small>{{ row.industry }}</small>
    </td>
    <td>
      {{ row.updatedAt }}<small>{{ row.owner }}</small>
    </td>
    <td>
      <button
        v-if="row.status !== '已转化'"
        type="button"
        :class="$style.convert"
        @click="emit('convert', row.id)"
      >
        转商机
      </button>
    </td>
  </tr>
</template>

<style module>
@reference "../../../../styles/index.css";

.row {
  @apply h-[65px] border-b transition-colors;
  border-color: var(--ui-border-subtle);
}
.row:hover {
  background: var(--ui-surface-hover);
}
.row td {
  @apply whitespace-nowrap px-3;
}
.row strong {
  @apply font-medium;
}
.row small {
  @apply mt-1 block text-[10px];
  color: #97a0af;
}
.code,
.company {
  color: var(--app-primary);
}
.tags {
  @apply mt-1 flex gap-1;
}
.tags i {
  @apply rounded px-1 py-0.5 text-[9px] not-italic;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.badge {
  @apply inline-flex rounded px-2 py-1 text-[10px] font-semibold;
}
.statusNew,
.sourceWeb {
  color: #0052cc;
  background: #deebff;
}
.statusContacted,
.sourceReferral {
  color: #ff8b00;
  background: #fff0b3;
}
.statusConfirmed,
.sourcePhone {
  color: #42526e;
  background: #ebecf0;
}
.statusConverted {
  color: #6554c0;
  background: #eae6ff;
}
.sourceExhibition {
  color: #00875a;
  background: #e3fcef;
}
.score {
  @apply flex items-center gap-0.5 font-semibold;
  color: #00875a;
}
.amount {
  color: var(--ui-text);
}
.convert {
  @apply text-[11px] font-medium opacity-0 transition-opacity;
  color: var(--app-primary);
}
.row:focus-within .convert,
.row:hover .convert {
  @apply opacity-100;
}
</style>
