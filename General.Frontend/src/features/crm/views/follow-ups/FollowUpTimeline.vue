<script setup lang="ts">
import { Building2, CalendarDays, Mail, MessageCircle, Phone, UserRound, UsersRound } from 'lucide-vue-next';
import { computed } from 'vue';
import type { FollowUpRow, FollowUpType } from '@/features/crm/model/follow-ups.types';

const props = defineProps<{ rows: readonly FollowUpRow[] }>();

const typeIcons = {
  电话: Phone,
  拜访: Building2,
  微信: MessageCircle,
  邮件: Mail,
  会议: UsersRound,
  其他: CalendarDays,
} satisfies Record<FollowUpType, typeof Phone>;

const groupedRows = computed(() => {
  const groups = new Map<string, FollowUpRow[]>();
  props.rows.forEach((row) => groups.set(row.date, [...(groups.get(row.date) ?? []), row]));
  return [...groups.entries()];
});
</script>

<template>
  <div
    :class="$style.timeline"
    data-follow-up-timeline
  >
    <section
      v-for="[date, entries] in groupedRows"
      :key="date"
      :class="$style.dayGroup"
    >
      <header :class="$style.dayHeader">
        <span><CalendarDays :size="14" />{{ date }}</span>
        <small>{{ entries.length }} 条记录</small>
      </header>
      <div :class="$style.entries">
        <article
          v-for="entry in entries"
          :key="entry.id"
          :class="$style.entry"
          data-follow-up-entry
        >
          <span :class="$style.timelineDot" />
          <header :class="$style.entryHeader">
            <span
              :class="$style.typeBadge"
              :data-type="entry.type"
            >
              <component
                :is="typeIcons[entry.type]"
                :size="13"
              />
              {{ entry.type }}
            </span>
            <strong><Building2 :size="13" />{{ entry.customer }}</strong>
            <span><UserRound :size="13" />{{ entry.contact }}</span>
            <a
              v-if="entry.opportunity"
              href="#"
              @click.prevent
              >{{ entry.opportunity }}</a
            >
            <small>{{ entry.owner }}</small>
          </header>
          <div :class="$style.entryBody">
            <p>{{ entry.content }}</p>
            <div :class="$style.resultBox">
              <span>跟进结果</span>
              <strong>{{ entry.result }}</strong>
            </div>
            <div
              v-if="entry.nextDate"
              :class="$style.nextBox"
            >
              <CalendarDays :size="14" />
              <p>
                <strong>下次跟进: {{ entry.nextDate }}</strong
                ><span>{{ entry.nextPlan }}</span>
              </p>
            </div>
          </div>
        </article>
      </div>
    </section>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前筛选下没有跟进记录
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.timeline {
  @apply mt-5;
}
.dayGroup + .dayGroup {
  @apply mt-5;
}
.dayHeader {
  @apply flex h-7 items-center gap-3;
}
.dayHeader > span {
  @apply flex items-center gap-2 rounded-full px-3 py-1 text-[12px] font-semibold;
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}
.dayHeader::after {
  @apply h-px flex-1;
  content: '';
  background: var(--ui-border-subtle);
}
.dayHeader small {
  @apply order-3 text-[11px];
  color: #97a0af;
}
.entries {
  @apply relative ml-4 border-l pl-5 pt-2;
  border-color: var(--ui-border);
}
.entry {
  @apply relative overflow-visible rounded border bg-white;
  border-color: var(--ui-border);
}
.entry + .entry {
  @apply mt-3;
}
.timelineDot {
  @apply absolute h-3 w-3 rounded-full border-[3px] bg-white;
  left: -27px;
  top: 16px;
  border-color: var(--app-primary);
}
.entryHeader {
  @apply flex min-h-10 items-center gap-3 border-b px-4 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.entryHeader strong {
  @apply flex items-center gap-1.5 text-[12px];
  color: var(--app-primary);
}
.entryHeader > span:not(.typeBadge) {
  @apply flex items-center gap-1;
}
.entryHeader a {
  color: var(--ui-warning);
}
.entryHeader small {
  @apply ml-auto;
}
.typeBadge {
  @apply flex items-center gap-1 rounded px-2 py-1 font-medium;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.typeBadge[data-type='拜访'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.typeBadge[data-type='微信'] {
  color: #006644;
  background: #e3fcef;
}
.typeBadge[data-type='邮件'],
.typeBadge[data-type='会议'] {
  color: #6554c0;
  background: #eae6ff;
}
.entryBody {
  @apply px-4 py-4;
}
.entryBody > p {
  @apply text-[12px] leading-5;
  color: var(--ui-text);
}
.resultBox {
  @apply mt-3 rounded border-l-2 px-3 py-2;
  border-color: var(--app-primary);
  background: var(--ui-surface-hover);
}
.resultBox span {
  @apply block text-[10px] font-medium;
  color: var(--ui-text-subtle);
}
.resultBox strong {
  @apply mt-1 block text-[12px] font-medium;
}
.nextBox {
  @apply mt-2 flex items-start gap-2 rounded border px-3 py-3 text-[11px];
  color: var(--ui-warning);
  border-color: #ffc400;
  background: var(--ui-warning-soft);
}
.nextBox p {
  color: var(--ui-text-muted);
}
.nextBox strong,
.nextBox span {
  @apply block;
}
.nextBox span {
  @apply mt-0.5;
}
.empty {
  @apply flex h-36 items-center justify-center rounded border bg-white text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
</style>
