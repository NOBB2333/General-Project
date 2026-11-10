<script setup lang="ts">
import { Star } from 'lucide-vue-next';
import type { PublicSeaLevel, PublicSeaRow, PublicSeaSource } from '@/features/crm/model/public-sea.types';

defineProps<{ row: PublicSeaRow }>();
const emit = defineEmits<{ follow: [id: string]; claim: [id: string] }>();

const levelClass: Record<PublicSeaLevel, string> = { A: 'levelA', B: 'levelB', C: 'levelC' };
const sourceClass: Record<PublicSeaSource, string> = {
  主动释放: 'sourcePrimary',
  超期回收: 'sourceDanger',
  合并释放: 'sourceWarning',
  新建: 'sourceSuccess',
};
const formatWan = (value: number) => `¥${(value / 10000).toFixed(1)}万`;
</script>

<template>
  <tr :class="$style.row">
    <td>
      <button
        type="button"
        :class="[$style.star, row.followed && $style.starActive]"
        :aria-label="row.followed ? `取消关注${row.customer}` : `关注${row.customer}`"
        @click="emit('follow', row.id)"
      >
        <Star
          :size="15"
          :fill="row.followed ? 'currentColor' : 'none'"
        />
      </button>
    </td>
    <td>
      <strong :class="$style.customer">{{ row.customer }}</strong>
    </td>
    <td>{{ row.industry }}</td>
    <td>
      <span :class="[$style.level, $style[levelClass[row.level]]]">{{ row.level }}</span>
    </td>
    <td>{{ row.contact }}</td>
    <td>{{ row.city }}</td>
    <td>
      <span :class="[$style.source, $style[sourceClass[row.source]]]">{{ row.source }}</span>
    </td>
    <td>
      <strong :class="$style.amount">{{ formatWan(row.estimatedValue) }}</strong>
    </td>
    <td :class="row.releaseDays > 30 ? $style.overdue : undefined">{{ row.releaseDays }}天</td>
    <td>
      <button
        type="button"
        :class="$style.claim"
        @click="emit('claim', row.id)"
      >
        领取
      </button>
    </td>
  </tr>
</template>

<style module>
@reference "../../../../styles/index.css";

.row {
  @apply h-[49px] border-b transition-colors;
  border-color: var(--ui-border-subtle);
}
.row:hover {
  background: var(--ui-surface-hover);
}
.row td {
  @apply whitespace-nowrap px-4;
}
.star {
  @apply flex h-6 w-6 items-center justify-center transition-colors;
  color: #c1c7d0;
}
.star:hover,
.starActive {
  color: #ffab00;
}
.customer {
  @apply font-semibold;
  color: var(--app-primary);
}
.level,
.source {
  @apply inline-flex rounded px-2 py-1 text-[10px] font-semibold;
}
.levelA {
  color: #0052cc;
  background: #deebff;
}
.levelB,
.sourceSuccess {
  color: #00875a;
  background: #e3fcef;
}
.levelC,
.sourceWarning {
  color: #ff8b00;
  background: #fff0b3;
}
.sourcePrimary {
  color: #0052cc;
  background: #deebff;
}
.sourceDanger {
  color: #de350b;
  background: #ffebe6;
}
.amount {
  color: var(--ui-text);
}
.overdue {
  color: var(--ui-warning);
}
.claim {
  @apply text-[11px] font-medium opacity-0 transition-opacity;
  color: var(--app-primary);
}
.row:focus-within .claim,
.row:hover .claim {
  @apply opacity-100;
}
</style>
