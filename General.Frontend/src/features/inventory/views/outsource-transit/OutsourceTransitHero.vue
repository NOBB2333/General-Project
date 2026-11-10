<script setup lang="ts">
import { ArrowLeftRight, Download, Factory } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';

const emit = defineEmits<{ feed: []; export: [] }>();
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.content">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>库存管理</span><b>›</b><strong>委外厂库存</strong>
      </nav>
      <div :class="$style.heading">
        <span :class="$style.icon"
          ><ArrowLeftRight
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>委外厂库存</h1>
          <p>按 工单 / 采购订单 + 物料 维度查询发到外协厂的在途库存</p>
        </div>
      </div>
    </div>
    <div :class="$style.actions">
      <UiButton
        size="sm"
        variant="secondary"
        @click="emit('feed')"
        ><Factory
          :size="14"
          :stroke-width="1.8"
        />去发料</UiButton
      >
      <UiButton
        size="sm"
        variant="secondary"
        @click="emit('export')"
        ><Download
          :size="14"
          :stroke-width="1.8"
        />导出</UiButton
      >
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative flex h-[114px] items-center justify-between overflow-hidden rounded-2xl px-6;
  color: white;
  background-color: #1658d6;
  background-image:
    radial-gradient(circle, rgba(255, 255, 255, 0.12) 1px, transparent 1px),
    linear-gradient(105deg, #5033b6 0%, #225bd7 48%, #0065d1 100%);
  background-size:
    26px 26px,
    auto;
}
.content {
  @apply relative;
}
.breadcrumb {
  @apply flex items-center gap-2 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.72);
}
.breadcrumb strong {
  color: white;
}
.heading {
  @apply mt-4 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  color: white;
  border-color: rgba(255, 255, 255, 0.3);
  background: rgba(255, 255, 255, 0.14);
}
.heading h1 {
  @apply m-0 text-[21px] font-bold leading-7;
}
.heading p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.82);
}
.actions {
  @apply relative flex items-center gap-2;
}
.actions button:first-child {
  color: white;
  border-color: rgba(255, 255, 255, 0.44);
  background: rgba(255, 255, 255, 0.12);
}
</style>
