<script setup lang="ts">
import { Barcode, Boxes, Fingerprint, ScanBarcode, ShieldCheck } from 'lucide-vue-next';
import type { TraceView } from '../../model/product-trace.types';

defineProps<{ activeView: TraceView }>();
const emit = defineEmits<{ 'update:activeView': [view: TraceView] }>();
const views: readonly { id: TraceView; label: string; icon: typeof Barcode }[] = [
  { id: 'product-codes', label: '商品编码', icon: Barcode },
  { id: 'batch-codes', label: '批次码', icon: Boxes },
  { id: 'instances', label: '产品实例', icon: Fingerprint },
  { id: 'warranty', label: '质保管理', icon: ShieldCheck },
];
</script>

<template>
  <div>
    <header :class="$style.hero">
      <div :class="$style.copy">
        <nav
          :class="$style.breadcrumb"
          aria-label="面包屑"
        >
          <span>基础资料</span><b aria-hidden="true">›</b><strong>产品实例追溯</strong>
        </nav>
        <div :class="$style.identity">
          <span :class="$style.icon"
            ><ScanBarcode
              :size="23"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>产品实例追溯</h1>
            <p>三码管理 · 实例档案 · 质保追踪</p>
          </div>
        </div>
      </div>
      <div
        :class="$style.art"
        aria-hidden="true"
      >
        <i /><b>SN-2026-04572</b>
      </div>
    </header>
    <nav
      :class="$style.views"
      aria-label="追溯业务视图"
    >
      <button
        v-for="view in views"
        :key="view.id"
        type="button"
        :class="[$style.viewButton, activeView === view.id && $style.active]"
        :aria-pressed="activeView === view.id"
        @click="emit('update:activeView', view.id)"
      >
        <component
          :is="view.icon"
          :size="15"
          :stroke-width="1.8"
        />{{ view.label }}
      </button>
    </nav>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative flex min-h-[120px] items-center overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(110deg, #bf2600 0%, #de350b 58%, #ff7452 100%);
}
.copy {
  @apply relative z-[2];
}
.breadcrumb {
  @apply mb-3 flex items-center gap-1.5 text-[12px];
  color: rgb(255 255 255 / 76%);
}
.breadcrumb strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 12%);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-0.5 text-[12.5px];
  color: rgb(255 255 255 / 82%);
}
.art {
  @apply absolute inset-y-3 right-[190px] z-[1] flex w-[240px] flex-col items-center justify-center rounded-lg bg-white/90 px-4;
  color: #172b4d;
}
.art::before {
  @apply absolute -left-16 -top-20 h-64 w-64 rounded-full bg-white/5;
  content: '';
}
.art i {
  @apply h-16 w-full border-b-2;
  border-color: #ff7452;
  background: repeating-linear-gradient(
    90deg,
    #172b4d 0 2px,
    transparent 2px 5px,
    #172b4d 5px 6px,
    transparent 6px 10px
  );
}
.art b {
  @apply mt-1 text-[11px] font-bold;
  letter-spacing: 0;
}
.views {
  @apply mt-3 flex min-h-[48px] items-end gap-1 border-b bg-white px-2;
  border-color: var(--ui-border);
}
.viewButton {
  @apply relative flex min-h-[47px] items-center gap-1.5 px-3 text-[12px] font-medium transition-colors;
  color: var(--ui-text-muted);
}
.viewButton:hover {
  background: var(--ui-surface-hover);
}
.active {
  @apply font-semibold;
  color: var(--app-primary);
}
.active::after {
  @apply absolute inset-x-0 bottom-0 h-0.5;
  content: '';
  background: var(--app-primary);
}
@media (max-width: 820px) {
  .art {
    @apply -right-20 opacity-45;
  }
}
</style>
