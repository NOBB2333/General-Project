<script setup lang="ts">
import {
  BookOpen,
  CalendarClock,
  ChevronRight,
  Clock3,
  ReceiptText,
  ScanLine,
  Send,
  Sparkles,
  Wrench,
  Zap,
} from 'lucide-vue-next';
import type {
  WorkbenchQuickLink,
  WorkbenchScheduleSection,
  WorkbenchSummaryCard,
  WorkbenchTabId,
} from '../../model/my-workbench.types';

defineProps<{
  scheduleSections: readonly WorkbenchScheduleSection[];
  summaryCards: readonly WorkbenchSummaryCard[];
  quickLinks: readonly WorkbenchQuickLink[];
}>();

const emit = defineEmits<{
  'select-tab': [tab: WorkbenchTabId];
  navigate: [route: string];
}>();
</script>

<template>
  <div :class="$style.todayGrid">
    <div :class="$style.scheduleColumn">
      <section
        v-for="section in scheduleSections"
        :key="section.id"
        :class="$style.scheduleSection"
      >
        <header :class="$style.sectionHeader">
          <Clock3
            v-if="section.id === 'schedule-today'"
            :size="14"
            :stroke-width="1.8"
            :class="$style.primaryIcon"
          />
          <CalendarClock
            v-else-if="section.id === 'schedule-tomorrow'"
            :size="14"
            :stroke-width="1.8"
            :class="$style.cyanIcon"
          />
          <Zap
            v-else
            :size="14"
            :stroke-width="1.8"
            :class="$style.dangerIcon"
          />
          <strong>{{ section.title }}</strong>
          <span :class="[$style.count, section.compact && $style.mutedCount]">{{ section.count }}</span>
        </header>
        <p :class="[$style.emptySchedule, section.compact && $style.compactEmpty]">
          {{ section.emptyText }}
        </p>
      </section>
    </div>

    <aside :class="$style.sideColumn">
      <button
        v-for="card in summaryCards"
        :key="card.id"
        type="button"
        :class="[$style.summaryCard, $style[card.tone]]"
        @click="emit('select-tab', card.targetTab)"
      >
        <span :class="$style.summaryTitle">
          <Wrench
            v-if="card.targetTab === 'parts'"
            :size="13"
            :stroke-width="1.8"
          />
          <ReceiptText
            v-else
            :size="13"
            :stroke-width="1.8"
          />
          <strong>{{ card.title }}</strong>
          <b>{{ card.count }}</b>
        </span>
        <span :class="$style.summaryDescription">{{ card.description }}</span>
        <span :class="$style.summaryAction"
          >{{ card.actionLabel }}
          <ChevronRight
            :size="11"
            :stroke-width="1.8"
        /></span>
      </button>

      <section :class="$style.quickCard">
        <header>
          <Sparkles
            :size="13"
            :stroke-width="1.8"
          />
          <strong>快捷入口</strong>
        </header>
        <div :class="$style.quickGrid">
          <button
            v-for="link in quickLinks"
            :key="link.id"
            type="button"
            @click="emit('navigate', link.route)"
          >
            <Send
              v-if="link.icon === 'send'"
              :size="12"
              :stroke-width="1.8"
            />
            <BookOpen
              v-else-if="link.icon === 'book'"
              :size="12"
              :stroke-width="1.8"
            />
            <CalendarClock
              v-else-if="link.icon === 'calendar'"
              :size="12"
              :stroke-width="1.8"
            />
            <ScanLine
              v-else
              :size="12"
              :stroke-width="1.8"
            />
            {{ link.label }}
          </button>
        </div>
      </section>
    </aside>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.todayGrid {
  @apply grid gap-4;
  grid-template-columns: minmax(0, 1fr) 320px;
}

.scheduleColumn,
.sideColumn {
  @apply space-y-4;
}

.scheduleSection {
  @apply min-w-0;
}

.sectionHeader {
  @apply mb-2 flex items-center gap-1.5 text-[12.5px];
  color: var(--ui-text);
}

.sectionHeader strong {
  @apply font-semibold;
}

.primaryIcon {
  color: #0052cc;
}

.cyanIcon {
  color: #00a3bf;
}

.dangerIcon {
  color: #de350b;
}

.count {
  @apply rounded bg-[#DEEBFF] px-1.5 py-0.5 text-[10px] font-bold text-[#0052CC];
}

.mutedCount {
  @apply bg-[#F4F5F7] text-[#6B778C];
}

.emptySchedule {
  @apply py-8 text-center text-[13px];
  color: #97a0af;
}

.compactEmpty {
  @apply py-4 text-[12px];
}

.summaryCard {
  @apply w-full rounded-lg border p-3 text-left transition-shadow;
}

.summaryCard:hover {
  box-shadow: 0 2px 6px rgb(9 30 66 / 12%);
}

.purple {
  color: #6554c0;
  background: #faf9ff;
  border-color: #eae6ff;
}

.warning {
  color: #ff8b00;
  background: #fffaf0;
  border-color: #ffe4b8;
}

.summaryTitle {
  @apply flex items-center gap-1.5;
}

.summaryTitle strong {
  @apply text-[12.5px] font-semibold;
}

.summaryTitle b {
  @apply ml-auto rounded px-1.5 py-0.5 text-[10.5px] font-bold text-white;
}

.purple .summaryTitle b {
  background: #6554c0;
}

.warning .summaryTitle b {
  background: #ff8b00;
}

.summaryDescription {
  @apply mt-1 block text-[11.5px];
  color: var(--ui-text-muted);
}

.summaryAction {
  @apply mt-1.5 flex items-center gap-1 text-[11px] font-semibold;
}

.quickCard {
  @apply rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}

.quickCard header {
  @apply mb-2 flex items-center gap-1.5 text-[12.5px] font-semibold;
  color: var(--ui-text);
}

.quickCard header svg {
  color: #6554c0;
}

.quickGrid {
  @apply grid grid-cols-2 gap-1.5;
}

.quickGrid button {
  @apply flex min-h-8 items-center justify-center gap-1.5 rounded border px-2 py-1.5 text-[11.5px] transition-colors active:scale-[.97];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}

.quickGrid button:hover {
  color: var(--app-primary);
  background: #f4f8ff;
  border-color: var(--app-primary);
}

@media (max-width: 920px) {
  .todayGrid {
    @apply grid-cols-1;
  }
}

@media (prefers-reduced-motion: reduce) {
  .quickGrid button {
    transform: none;
  }
}
</style>
