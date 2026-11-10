<script setup lang="ts">
import { Clock3 } from 'lucide-vue-next';
import { computed } from 'vue';
import type { ServiceCardRow } from '@/features/crm/model/service-cards.types';

const props = defineProps<{ card: ServiceCardRow }>();

const statusLabel = computed(() => {
  if (props.card.status === 'active') return '生效中';
  if (props.card.status === 'expiring') return '即将到期';
  return '已过期';
});

const amountLabel = computed(() =>
  new Intl.NumberFormat('zh-CN', {
    style: 'currency',
    currency: 'CNY',
    maximumFractionDigits: 0,
  })
    .format(props.card.amount)
    .replace('CN¥', '¥'),
);
</script>

<template>
  <article :class="$style.cardItem">
    <div :class="[$style.cardFace, $style[card.tone]]">
      <span :class="[$style.glow, $style.glowTop]" />
      <span :class="[$style.glow, $style.glowBottom]" />
      <span :class="$style.lightSweep" />
      <header :class="$style.cardHeader">
        <div>
          <span :class="$style.brand">RISEMAP · 服务权益</span>
          <h2>{{ card.serviceName }}</h2>
        </div>
        <span :class="$style.tier">{{ card.tierLabel }}</span>
      </header>
      <div :class="$style.cardNumber">
        <span :class="$style.chip" />
        <strong>{{ card.cardNumber }}</strong>
      </div>
      <footer :class="$style.cardFooter">
        <div>
          <span>持卡客户</span>
          <strong>{{ card.customer }}</strong>
        </div>
        <div :class="$style.expiry">
          <span>有效期至</span>
          <strong>{{ card.validUntil }}</strong>
        </div>
        <span
          :class="$style.mark"
          aria-hidden="true"
        />
      </footer>
    </div>
    <div :class="$style.statusLine">
      <span :class="[$style.status, $style[card.status]]">{{ statusLabel }}</span>
      <span :class="[$style.remaining, $style[card.status]]"> <Clock3 :size="12" />{{ card.remainingText }} </span>
    </div>
    <progress
      :class="[$style.progress, $style[card.status]]"
      :value="card.progress"
      max="100"
      :aria-label="card.remainingText"
    />
    <div :class="$style.meta">
      <span>{{ card.validityLabel }} · 至 {{ card.validUntil }}</span>
      <strong>{{ amountLabel }}</strong>
    </div>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.cardItem {
  @apply min-w-0;
}

.cardFace {
  @apply relative flex h-[271px] min-h-[271px] flex-col overflow-hidden rounded-2xl p-4 text-white;
}

.diamond {
  background: linear-gradient(135deg, #1e3a8a 0%, #4f46e5 45%, #7c3aed 100%);
}

.platinum {
  background: linear-gradient(135deg, #111827 0%, #374151 50%, #6b7280 100%);
}

.gold {
  background: linear-gradient(135deg, #92400e 0%, #d97706 45%, #fbbf24 100%);
}

.silver {
  background: linear-gradient(135deg, #475569 0%, #64748b 45%, #94a3b8 100%);
}

.cardFace {
  padding: 16px;
  box-shadow: 0 12px 30px -10px rgb(15 23 42 / 45%);
}

.glow,
.lightSweep {
  @apply pointer-events-none absolute;
}

.glowTop {
  top: -30%;
  right: -15%;
  width: 60%;
  height: 120%;
  background: radial-gradient(circle, rgb(255 255 255 / 28%) 0%, rgb(255 255 255 / 0%) 70%);
}

.glowBottom {
  bottom: -40%;
  left: -10%;
  width: 55%;
  height: 110%;
  background: radial-gradient(circle, rgb(255 255 255 / 14%) 0%, rgb(255 255 255 / 0%) 70%);
}

.lightSweep {
  inset: 0;
  background: linear-gradient(115deg, rgb(255 255 255 / 0%) 40%, rgb(255 255 255 / 16%) 50%, rgb(255 255 255 / 0%) 60%);
}

.cardHeader,
.cardFooter {
  @apply relative z-10 flex items-start justify-between gap-3;
}

.cardHeader {
  transform: translateY(-8px);
}

.brand {
  @apply text-[9px] font-semibold uppercase leading-[13.5px] tracking-[.18em];
  color: rgb(199 210 254);
}

.cardHeader h2 {
  @apply mt-[3px] text-[15px] font-bold tracking-[-.01em];
}

.tier {
  @apply shrink-0 rounded-full border px-2.5 py-1 text-[10px] font-semibold;
  border-color: rgb(255 255 255 / 45%);
  background: rgb(255 255 255 / 10%);
}

.cardNumber {
  @apply relative z-10 my-auto flex items-center gap-3;
}

.chip {
  @apply h-[23px] w-[30px] rounded-md;
  background:
    linear-gradient(90deg, transparent 48%, rgb(91 70 0 / 28%) 50%, transparent 52%),
    linear-gradient(0deg, transparent 48%, rgb(91 70 0 / 28%) 50%, transparent 52%),
    linear-gradient(135deg, #ffe16d, #c99900);
  box-shadow: inset 0 0 0 1px rgb(89 63 0 / 25%);
}

.cardNumber strong {
  @apply font-mono text-[12.5px] leading-[21px] tracking-[.08em];
}

.cardNumber {
  transform: translateY(-4px);
}

.cardFooter {
  @apply items-end;
  transform: translateY(-3px);
}

.cardFooter div {
  @apply min-w-0;
}

.cardFooter span {
  @apply block text-[8.5px] leading-[12.75px] tracking-[.12em];
  color: rgb(199 210 254);
}

.cardFooter strong {
  @apply mt-0.5 block truncate text-[11.5px] font-semibold;
}

.expiry strong {
  @apply text-[10.5px];
}

.expiry {
  @apply ml-auto text-right;
}

.mark {
  @apply ml-1 h-6 w-10 shrink-0 rounded-full;
  background:
    radial-gradient(circle at 35% 50%, rgb(255 255 255 / 80%) 0 34%, transparent 36%),
    radial-gradient(circle at 65% 50%, rgb(255 255 255 / 62%) 0 34%, transparent 36%);
}

.statusLine,
.meta {
  @apply mt-2 flex items-center justify-between gap-3 text-[11px];
}

.statusLine {
  @apply mt-[13px] px-1;
}

.status,
.remaining {
  @apply inline-flex items-center gap-1 font-semibold;
}

.status {
  @apply rounded-full px-2 py-0.5;
}

.status.active {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.status.expiring {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.status.expired {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.remaining.active {
  color: var(--ui-success);
}

.remaining.expiring {
  color: var(--ui-warning);
}

.remaining.expired {
  color: var(--ui-danger);
}

.progress {
  @apply mt-2 block h-1.5 w-full overflow-hidden rounded-full border-0;
  appearance: none;
  background: var(--ui-border-subtle);
}

.progress::-webkit-progress-bar {
  background: var(--ui-border-subtle);
}

.progress::-webkit-progress-value {
  border-radius: 999px;
  background: var(--ui-success);
}

.progress.expiring::-webkit-progress-value {
  background: var(--ui-warning);
}

.progress.expired::-webkit-progress-value {
  background: var(--ui-danger);
}

.progress::-moz-progress-bar {
  background: var(--ui-success);
}

.progress.expiring::-moz-progress-bar {
  background: var(--ui-warning);
}

.progress.expired::-moz-progress-bar {
  background: var(--ui-danger);
}

.meta {
  @apply mt-[10px] px-1;
  color: var(--ui-text-subtle);
}

.meta strong {
  color: var(--ui-text);
}
</style>
