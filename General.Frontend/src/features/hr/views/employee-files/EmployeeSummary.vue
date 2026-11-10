<script setup lang="ts">
import { CalendarClock, Clock3, UserCheck, UserMinus, Users } from 'lucide-vue-next';
const items = [
  { label: '在职员工', value: '8', hint: '共12人', tone: 'green' },
  { label: '试用期', value: '2', hint: '待转正', tone: 'blue' },
  { label: '休假中', value: '1', hint: '产假/年假', tone: 'orange' },
  { label: '已离职', value: '1', hint: '本年度', tone: 'neutral' },
  { label: '近期合同到期', value: '0', hint: '近30天', tone: 'red' },
] as const;
</script>

<template>
  <section :class="$style.grid">
    <article
      v-for="(item, index) in items"
      :key="item.label"
      :data-tone="item.tone"
    >
      <header>
        <UserCheck
          v-if="index === 0"
          :size="16"
          :stroke-width="1.8"
        /><Clock3
          v-else-if="index === 1"
          :size="16"
          :stroke-width="1.8"
        /><CalendarClock
          v-else-if="index === 2"
          :size="16"
          :stroke-width="1.8"
        /><UserMinus
          v-else-if="index === 3"
          :size="16"
          :stroke-width="1.8"
        /><Users
          v-else
          :size="16"
          :stroke-width="1.8"
        />{{ item.label }}
      </header>
      <strong>{{ item.value }}</strong>
      <p>{{ item.hint }}</p>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply grid grid-cols-5 gap-4;
}
.grid article {
  @apply h-[102px] rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.grid header {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.grid article[data-tone='green'] header,
.grid article[data-tone='green'] strong {
  color: #00875a;
}
.grid article[data-tone='blue'] header,
.grid article[data-tone='blue'] strong {
  color: #0c66e4;
}
.grid article[data-tone='orange'] header,
.grid article[data-tone='orange'] strong {
  color: #ff8b00;
}
.grid article[data-tone='red'] header,
.grid article[data-tone='red'] strong {
  color: #de350b;
}
.grid strong {
  @apply mt-2 block text-[20px];
  color: var(--ui-text-muted);
}
.grid p {
  @apply m-0 mt-1 text-[9px];
  color: #97a0af;
}
</style>
