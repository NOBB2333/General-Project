<script setup lang="ts">
import { AlertTriangle, CalendarDays } from 'lucide-vue-next';

defineProps<{
  leaveCount: number;
  travelCount: number;
  conflictDates: number;
  totalCount: number;
}>();
</script>

<template>
  <aside
    :class="$style.sidebar"
    aria-label="团队日历概览"
  >
    <section :class="$style.card">
      <h2 :class="$style.cardTitle">
        <CalendarDays
          :size="15"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        本月概览
      </h2>
      <div :class="$style.stats">
        <article :class="[$style.stat, $style.leaveStat]">
          <strong>{{ leaveCount }}</strong>
          <span>请假记录</span>
        </article>
        <article :class="[$style.stat, $style.travelStat]">
          <strong>{{ travelCount }}</strong>
          <span>出差记录</span>
        </article>
        <article :class="[$style.stat, $style.conflictStat]">
          <strong>{{ conflictDates }}</strong>
          <span>冲突日期</span>
        </article>
        <article :class="[$style.stat, $style.totalStat]">
          <strong>{{ totalCount }}</strong>
          <span>总事件数</span>
        </article>
      </div>
    </section>

    <section :class="$style.card">
      <h2 :class="[$style.cardTitle, $style.warningTitle]">
        <AlertTriangle
          :size="15"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        冲突预警
      </h2>
      <div :class="$style.emptyWarning">
        <p>暂无人力冲突预警</p>
      </div>
    </section>
  </aside>
</template>

<style module>
@reference "../../../../styles/index.css";

.sidebar {
  @apply flex w-[280px] shrink-0 flex-col gap-3;
}

.card {
  @apply rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
  border-radius: 3.5px;
}

.cardTitle {
  @apply mb-3 flex items-center gap-1.5 text-[13px] font-semibold;
  color: var(--ui-text);
}

.cardTitle svg {
  color: var(--app-primary);
}

.stats {
  @apply grid grid-cols-2 gap-3;
}

.stat {
  @apply flex h-[62.5px] flex-col items-center justify-center rounded text-center;
  border-radius: 3.5px;
}

.stat strong {
  @apply text-[20px] font-bold leading-7;
}

.stat span {
  @apply text-[10px] leading-4;
}

.leaveStat {
  color: #ff8b00;
  background: #fff0b3;
}

.travelStat {
  color: #0052cc;
  background: #deebff;
}

.conflictStat {
  color: #ff5630;
  background: #ffebe6;
}

.totalStat {
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}

.warningTitle svg {
  color: #ff5630;
}

.emptyWarning {
  @apply flex min-h-[37.5px] flex-col items-center justify-center text-center;
  color: #97a0af;
}

.emptyWarning p {
  @apply text-[11px];
}
</style>
