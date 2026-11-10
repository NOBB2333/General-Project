<script setup lang="ts">
import { BadgePercent, TicketPercent } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav aria-label="面包屑"><span>销售业务</span><b aria-hidden="true">›</b><strong>特价管理</strong></nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><BadgePercent
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>特价管理</h1>
          <p>销售特价申请、审批结果及关联订单跟踪</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.decoration"
      aria-hidden="true"
    >
      <span :class="$style.ticket"><strong>%</strong></span>
      <TicketPercent
        :class="$style.tag"
        :size="70"
        :stroke-width="1.8"
      />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(108deg, #de350b 0%, #ff5630 70%, #ff8b00 100%);
}
.hero::before {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background:
    radial-gradient(circle at 18% 8%, rgb(255 255 255 / 14%) 0 2px, transparent 3px),
    linear-gradient(90deg, rgb(191 38 0 / 25%), transparent 58%);
  background-size:
    120px 120px,
    auto;
}
.copy {
  @apply relative z-[1];
}
.copy nav {
  @apply mb-3 flex items-center gap-1.5 text-[12px];
  color: rgb(255 255 255 / 78%);
}
.copy nav strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex items-center gap-3;
}
.icon {
  @apply flex h-11 w-11 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255 / 22%);
  background: rgb(255 255 255 / 15%);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-1 text-[12.5px];
  color: rgb(255 255 255 / 85%);
}
.decoration {
  @apply pointer-events-none absolute inset-y-0 right-8 flex items-center gap-12;
  color: rgb(255 255 255 / 75%);
}
.ticket {
  @apply flex h-[122px] w-[175px] items-center justify-center;
  color: #de350b;
  background: rgb(255 255 255 / 88%);
  clip-path: polygon(
    10% 0,
    90% 0,
    82% 22%,
    100% 36%,
    82% 50%,
    100% 64%,
    82% 78%,
    90% 100%,
    10% 100%,
    18% 78%,
    0 64%,
    18% 50%,
    0 36%,
    18% 22%
  );
}
.ticket strong {
  @apply text-[48px] font-bold;
}
.tag {
  @apply opacity-80;
}
</style>
