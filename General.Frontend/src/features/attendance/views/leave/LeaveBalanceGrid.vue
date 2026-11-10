<script setup lang="ts">
import type { Component } from 'vue';
import { BriefcaseBusiness, Coffee, Heart, HeartPulse, Sun, Umbrella } from 'lucide-vue-next';
import type { LeaveBalance, LeaveType } from '../../model/leave.types';

defineProps<{ balances: readonly LeaveBalance[] }>();

const icons: Record<LeaveType, Component> = {
  年假: Sun,
  病假: HeartPulse,
  事假: BriefcaseBusiness,
  婚假: Heart,
  产假: Heart,
  陪产假: Heart,
  丧假: Umbrella,
  调休: Coffee,
  哺乳假: Coffee,
};
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="假期余额"
  >
    <article
      v-for="balance in balances"
      :key="balance.type"
      :class="$style.card"
      :data-tone="balance.tone"
    >
      <header>
        <span :class="$style.icon"
          ><component
            :is="icons[balance.type]"
            :size="17"
            :stroke-width="1.8"
        /></span>
        <strong>{{ balance.type }}</strong>
      </header>
      <div :class="$style.balanceRow">
        <strong :class="$style.remaining">{{ balance.remaining }}</strong>
        <span>总计 {{ balance.total }} 天</span>
      </div>
      <div :class="$style.meta">
        <span>剩余可用</span>
        <span>已用 {{ balance.used }} | 审批中 {{ balance.pending }}</span>
      </div>
      <progress
        :value="balance.used"
        :max="balance.total"
        :aria-label="`${balance.type}已用额度`"
      />
      <p v-if="balance.note">{{ balance.note }}</p>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply grid grid-cols-6 gap-3;
}
.card {
  @apply min-w-0 rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
  --leave-accent: #ff8b00;
  --leave-soft: #fff0b3;
}
.card[data-tone='red'] {
  --leave-accent: #ff5630;
  --leave-soft: #ffebe6;
}
.card[data-tone='purple'] {
  --leave-accent: #6554c0;
  --leave-soft: #eae6ff;
}
.card[data-tone='green'] {
  --leave-accent: #00875a;
  --leave-soft: #e3fcef;
}
.card[data-tone='navy'] {
  --leave-accent: #42526e;
  --leave-soft: #f4f5f7;
}
.card header {
  @apply flex items-center gap-2 text-[13px];
}
.icon {
  @apply flex h-7 w-7 items-center justify-center rounded;
  color: var(--leave-accent);
  background: var(--leave-soft);
}
.balanceRow {
  @apply mt-3 flex items-end justify-between;
}
.balanceRow > span,
.meta {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.remaining {
  @apply text-[23px] leading-6;
  color: var(--leave-accent);
}
.meta {
  @apply mt-1 flex justify-between gap-1;
}
.card progress {
  @apply mt-2 block h-1 w-full overflow-hidden rounded-full;
  accent-color: var(--leave-accent);
}
.card progress::-webkit-progress-bar {
  background: #f4f5f7;
}
.card progress::-webkit-progress-value {
  background: var(--leave-accent);
  border-radius: 999px;
}
.card p {
  @apply mt-2 text-[9px] font-medium;
  color: var(--leave-accent);
}
</style>
