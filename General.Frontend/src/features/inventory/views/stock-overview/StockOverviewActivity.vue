<script setup lang="ts">
import { Clock3, ExternalLink } from 'lucide-vue-next';
import type { InventoryMovement } from '../../model/stock-overview.types';
defineProps<{ rows: readonly InventoryMovement[] }>();
const emit = defineEmits<{ manage: []; select: [row: InventoryMovement] }>();
</script>

<template>
  <section :class="$style.panel">
    <header>
      <div>
        <h2>
          <Clock3
            :size="17"
            :stroke-width="1.8"
          />库存动态
        </h2>
        <p>近期调拨/借出/报损对库存的影响</p>
      </div>
      <button
        type="button"
        @click="emit('manage')"
      >
        调拨管理<ExternalLink
          :size="13"
          :stroke-width="1.8"
        />
      </button>
    </header>
    <div :class="$style.summary">
      <strong>调拨出库</strong><span>本月从主仓调出 <b>-47件</b></span
      ><strong>调入/归还</strong><span>本月调入主仓 <b>+34件</b></span>
    </div>
    <button
      v-for="row in rows"
      :key="row.id"
      type="button"
      :class="$style.row"
      @click="emit('select', row)"
    >
      <span
        ><b>{{ row.id }}</b
        ><small>{{ row.kind }}</small></span
      ><strong>{{ row.description }}</strong
      ><em>{{ row.quantity }}</em
      ><b>{{ row.amount }}</b>
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.panel {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.panel > header {
  @apply flex items-center justify-between border-b px-5 py-4;
  border-color: var(--ui-border-subtle);
}
.panel h2 {
  @apply flex items-center gap-2 text-[15px] font-semibold;
}
.panel h2 svg {
  color: #0052cc;
}
.panel header p {
  @apply mt-1 text-[10px];
  color: #97a0af;
}
.panel header button {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: #0052cc;
}
.summary {
  @apply grid grid-cols-[100px_1fr_100px_1fr] gap-3 border-b px-5 py-3 text-[11px];
  border-color: var(--ui-border-subtle);
  background: #fafbfc;
}
.summary span {
  color: #6b778c;
}
.summary b {
  color: #172b4d;
}
.row {
  @apply grid w-full grid-cols-[145px_1fr_80px_90px] items-center gap-3 border-b px-5 py-3 text-left text-[11px] transition-colors;
  border-color: var(--ui-border-subtle);
}
.row:hover {
  background: #f4f5f7;
}
.row > span {
  @apply flex flex-col;
}
.row > span b {
  color: #0052cc;
}
.row small {
  @apply mt-1 text-[9px];
  color: #97a0af;
}
.row > strong {
  @apply font-medium;
  color: #172b4d;
}
.row em {
  @apply text-right not-italic font-semibold;
  color: #de350b;
}
.row > b {
  @apply text-right;
  color: #42526e;
}
</style>
