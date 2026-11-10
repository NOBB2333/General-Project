<script setup lang="ts">
import { Download, GitCompareArrows, Network, Plus } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';

const emit = defineEmits<{ compare: []; export: []; create: [] }>();
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>基础资料</span><b aria-hidden="true">›</b><strong>BOM管理</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><Network
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>BOM管理</h1>
          <p>统一管理标准 BOM 与项目 BOM 的结构、版本与成本</p>
        </div>
      </div>
    </div>
    <div :class="$style.actions">
      <UiButton
        :class="$style.glassButton"
        size="sm"
        @click="emit('compare')"
      >
        <template #leading
          ><GitCompareArrows
            :size="14"
            :stroke-width="1.8" /></template
        >BOM对比
      </UiButton>
      <UiButton
        :class="$style.glassButton"
        size="sm"
        @click="emit('export')"
      >
        <template #leading
          ><Download
            :size="14"
            :stroke-width="1.8" /></template
        >导出
      </UiButton>
      <UiButton
        :class="$style.primaryAction"
        size="sm"
        @click="emit('create')"
      >
        <template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8" /></template
        >新建BOM
      </UiButton>
    </div>
    <div
      :class="$style.art"
      aria-hidden="true"
    >
      <span /><span /><span /><Network
        :size="72"
        :stroke-width="1.2"
      />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative flex min-h-[120px] items-center overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(110deg, #321064 0%, #5243aa 56%, #9980ed 100%);
}
.copy,
.actions {
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
.actions {
  @apply ml-auto flex items-center gap-2;
}
.glassButton {
  @apply border text-white;
  border-color: rgb(255 255 255 / 30%);
  background: rgb(255 255 255 / 14%);
}
.glassButton:hover {
  background: rgb(255 255 255 / 23%);
}
.primaryAction {
  @apply border-white bg-white font-semibold;
  color: #5243aa;
}
.primaryAction:hover {
  background: #f4f5f7;
}
.art {
  @apply pointer-events-none absolute inset-y-0 right-[180px] z-[1] w-[560px];
  color: rgb(255 255 255 / 58%);
}
.art::before,
.art::after {
  @apply absolute border-t-2 border-dashed;
  content: '';
  border-color: rgb(255 255 255 / 42%);
}
.art::before {
  @apply right-12 top-7 w-[310px] rotate-[8deg];
}
.art::after {
  @apply bottom-7 right-10 w-[310px] -rotate-[8deg];
}
.art > span {
  @apply absolute h-3 w-3 rounded-sm bg-white/45;
}
.art > span:nth-child(1) {
  @apply left-20 top-7;
}
.art > span:nth-child(2) {
  @apply left-20 top-1/2;
}
.art > span:nth-child(3) {
  @apply bottom-5 left-20;
}
.art svg {
  @apply absolute right-0 top-1/2 -translate-y-1/2 rounded-2xl border bg-white/15 p-3;
  border-color: rgb(255 255 255 / 40%);
}
@media (max-width: 960px) {
  .art {
    @apply opacity-30;
  }
  .actions {
    @apply hidden;
  }
}
</style>
