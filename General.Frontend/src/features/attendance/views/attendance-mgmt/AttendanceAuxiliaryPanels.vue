<script setup lang="ts">
import { ref } from 'vue';
import { UiButton, UiSwitch, toast } from '@/components/ui';

defineProps<{ tab: '个人考勤' | '补签申请' | '考勤规则' }>();
const rules = ref([
  { id: 'late', label: '迟到自动标记', description: '超过规定上班时间自动计入迟到分钟', enabled: true },
  { id: 'early', label: '早退自动标记', description: '早于规定下班时间自动计入早退分钟', enabled: true },
  { id: 'location', label: '限制打卡位置', description: '仅允许在办公地点 500 米范围内打卡', enabled: false },
]);
</script>

<template>
  <section
    v-if="tab === '个人考勤'"
    :class="$style.panel"
  >
    <header>
      <h2>我的本月考勤</h2>
      <p>2026 年 3 月个人出勤概览</p>
    </header>
    <div :class="$style.metrics">
      <article><strong>9</strong><span>出勤天数</span></article>
      <article><strong>0</strong><span>迟到</span></article>
      <article><strong>0</strong><span>早退</span></article>
      <article><strong>76.5h</strong><span>累计工时</span></article>
    </div>
  </section>
  <section
    v-else-if="tab === '补签申请'"
    :class="$style.panel"
  >
    <header>
      <h2>补签申请</h2>
      <p>当前有 2 条申请等待处理</p>
    </header>
    <div :class="$style.requests">
      <article>
        <div>
          <strong>李小明 · 2026-03-10 签到</strong>
          <p>忘记打卡，申请补签到 09:00</p>
        </div>
        <span>待审批</span
        ><UiButton
          size="xs"
          variant="primary"
          @click="toast.success('补签申请已批准')"
          >批准</UiButton
        >
      </article>
      <article>
        <div>
          <strong>孙丽丽 · 2026-03-10 签到</strong>
          <p>地铁故障，申请补签到 09:00</p>
        </div>
        <span>待审批</span
        ><UiButton
          size="xs"
          variant="primary"
          @click="toast.success('补签申请已批准')"
          >批准</UiButton
        >
      </article>
    </div>
  </section>
  <section
    v-else
    :class="$style.panel"
  >
    <header>
      <h2>考勤规则</h2>
      <p>当前工作日 09:00 至 17:30</p>
    </header>
    <div :class="$style.rules">
      <label
        v-for="rule in rules"
        :key="rule.id"
        ><span
          ><strong>{{ rule.label }}</strong
          ><small>{{ rule.description }}</small></span
        ><UiSwitch
          v-model="rule.enabled"
          :label="rule.label"
      /></label>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.panel {
  @apply mt-4 overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.panel > header {
  @apply border-b px-5 py-4;
  border-color: var(--ui-border-subtle);
}
.panel h2 {
  @apply text-[15px] font-semibold;
}
.panel header p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.metrics {
  @apply grid grid-cols-4 gap-3 p-4;
}
.metrics article {
  @apply flex flex-col rounded-lg border p-4;
  border-color: var(--ui-border);
}
.metrics strong {
  @apply text-[21px];
  color: var(--app-primary);
}
.metrics span {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.requests,
.rules {
  @apply divide-y;
}
.requests article {
  @apply flex items-center gap-4 px-5 py-4;
}
.requests article > div {
  @apply flex-1;
}
.requests p {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.requests article > span {
  @apply rounded bg-[#FFF0B3] px-2 py-1 text-[10px] text-[#FF8B00];
}
.rules label {
  @apply flex items-center justify-between px-5 py-4;
}
.rules label > span {
  @apply flex flex-col;
}
.rules small {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
</style>
