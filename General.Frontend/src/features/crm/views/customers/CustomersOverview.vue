<script setup lang="ts">
import { Star, UserRound, UsersRound } from 'lucide-vue-next';
import type { CustomerScope } from '@/features/crm/model/customers.types';

defineProps<{
  scope: CustomerScope;
  type: string;
  total: number;
  totalSales: string;
  receivable: string;
  averageTerms: number;
}>();

const emit = defineEmits<{
  'update:scope': [value: CustomerScope];
  'update:type': [value: string];
}>();

const scopes: { id: CustomerScope; label: string; icon: typeof UsersRound }[] = [
  { id: 'all', label: '所有', icon: UsersRound },
  { id: 'mine', label: '我负责的', icon: UserRound },
  { id: 'subordinates', label: '下属负责的', icon: UsersRound },
  { id: 'followed', label: '我关注的', icon: Star },
];
const types = ['全部', '终端客户', '集成商', 'EPC', '贸易商', 'OEM'];
</script>

<template>
  <nav
    :class="$style.scopes"
    aria-label="客户数据范围"
  >
    <button
      v-for="item in scopes"
      :key="item.id"
      type="button"
      :class="[$style.scopeButton, scope === item.id && $style.scopeActive]"
      :aria-pressed="scope === item.id"
      @click="emit('update:scope', item.id)"
    >
      <component
        :is="item.icon"
        :size="14"
        :stroke-width="1.8"
      />
      {{ item.label }}
    </button>
  </nav>

  <section
    :class="$style.stats"
    aria-label="客户概览"
  >
    <article>
      <span>客户总数</span>
      <strong>{{ total }}</strong>
    </article>
    <article>
      <span>累计销售额</span>
      <strong>{{ totalSales }}</strong>
    </article>
    <article>
      <span>应收余额</span>
      <strong :class="$style.receivable">{{ receivable }}</strong>
    </article>
    <article>
      <span>平均账期</span>
      <strong>{{ averageTerms }}天</strong>
    </article>
  </section>

  <nav
    :class="$style.typeTabs"
    aria-label="客户类型"
  >
    <button
      v-for="item in types"
      :key="item"
      type="button"
      :class="[$style.typeButton, type === item && $style.typeActive]"
      :aria-pressed="type === item"
      @click="emit('update:type', item)"
    >
      {{ item }}
    </button>
  </nav>
</template>

<style module>
@reference "../../../../styles/index.css";

.scopes {
  @apply mt-4 flex min-h-9 items-center gap-1 rounded border p-1;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.scopeButton {
  @apply flex min-h-7 items-center gap-1.5 rounded px-3 text-[12px] font-medium transition-colors;
  color: var(--ui-text-muted);
}

.scopeButton:hover {
  background: var(--ui-surface-hover);
}

.scopeActive {
  color: white;
  background: var(--app-primary);
  box-shadow: 0 1px 2px rgb(9 30 66 / 20%);
}

.scopeActive:hover {
  background: var(--app-primary);
}

.stats {
  @apply mt-4 grid grid-cols-4 gap-4;
}

.stats article {
  @apply flex min-h-[84px] flex-col justify-center rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}

.stats span {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}

.stats strong {
  @apply mt-1 text-[22px] font-bold;
  color: var(--ui-text);
}

.stats .receivable {
  color: #ff5630;
}

.typeTabs {
  @apply mt-4 flex items-center border-b;
  border-color: var(--ui-border);
}

.typeButton {
  @apply relative min-h-11 px-4 text-[13px] transition-colors;
  color: var(--ui-text-muted);
}

.typeButton:hover {
  color: var(--ui-text);
}

.typeActive {
  @apply font-semibold;
  color: var(--app-primary);
}

.typeActive::after {
  @apply absolute bottom-0 left-0 right-0 h-0.5;
  content: '';
  background: var(--app-primary);
}

@media (max-width: 900px) {
  .stats {
    @apply grid-cols-2;
  }

  .scopes,
  .typeTabs {
    @apply overflow-x-auto;
  }
}
</style>
