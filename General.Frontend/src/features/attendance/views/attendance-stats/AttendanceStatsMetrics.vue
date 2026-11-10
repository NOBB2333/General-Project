<script setup lang="ts">
import { Activity, CheckCircle2, Clock3, LogIn, UserRoundX, Timer } from 'lucide-vue-next';
interface Metric {
  label: string;
  value: string;
  delta: string;
  tone: string;
  icon: typeof CheckCircle2;
}
const metrics: Metric[] = [
  { label: '总出勤率', value: '57.0%', delta: '↗ +2.1%', tone: 'green', icon: CheckCircle2 },
  { label: '总记录数', value: '86', delta: '', tone: 'neutral', icon: Activity },
  { label: '迟到次数', value: '17', delta: '↘ -15%', tone: 'orange', icon: Clock3 },
  { label: '早退次数', value: '8', delta: '↘ -8%', tone: 'red', icon: LogIn },
  { label: '旷工次数', value: '5', delta: '↗ +1', tone: 'red', icon: UserRoundX },
  { label: '平均工时', value: '7.9h', delta: '↗ +0.2h', tone: 'blue', icon: Timer },
];
</script>

<template>
  <section
    :class="$style.metrics"
    aria-label="考勤汇总"
  >
    <article
      v-for="item in metrics"
      :key="item.label"
      :data-tone="item.tone"
    >
      <header>
        <span
          ><component
            :is="item.icon"
            :size="17"
            :stroke-width="1.8" /></span
        ><b v-if="item.delta">{{ item.delta }}</b>
      </header>
      <strong>{{ item.value }}</strong
      ><small>{{ item.label }}</small>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.metrics {
  @apply grid grid-cols-6 gap-3;
}
.metrics article {
  @apply min-h-[92px] rounded border bg-white p-3;
  border-color: var(--ui-border);
}
.metrics article header {
  @apply flex items-center justify-between;
}
.metrics article header span {
  @apply flex h-8 w-8 items-center justify-center rounded;
  color: #0052cc;
  background: #deebff;
}
.metrics article header b {
  @apply rounded px-2 py-1 text-[10px] font-medium;
  color: #00875a;
  background: #e3fcef;
}
.metrics article strong {
  @apply mt-2 block text-[23px] font-bold;
  color: #172b4d;
}
.metrics article small {
  @apply mt-1 block text-[11px];
  color: #6b778c;
}
.metrics article[data-tone='green'] strong {
  color: #00875a;
}
.metrics article[data-tone='orange'] strong {
  color: #ff8b00;
}
.metrics article[data-tone='red'] strong {
  color: #de350b;
}
.metrics article[data-tone='blue'] strong {
  color: #0052cc;
}
.metrics article[data-tone='orange'] header span {
  color: #ff8b00;
  background: #fff0b3;
}
.metrics article[data-tone='red'] header span {
  color: #de350b;
  background: #ffebe6;
}
.metrics article[data-tone='green'] header span {
  color: #00875a;
  background: #e3fcef;
}
</style>
