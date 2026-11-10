<script setup lang="ts">
import { BookOpen, ChevronRight } from 'lucide-vue-next';
</script>
<template>
  <header :class="$style.hero">
    <svg
      :class="$style.scene"
      viewBox="0 0 1600 400"
      preserveAspectRatio="xMidYMid slice"
      aria-hidden="true"
    >
      <circle
        cx="1330"
        cy="210"
        r="190"
        :class="$style.halo"
      />
      <g transform="translate(1050,330)">
        <rect
          x="0"
          y="-55"
          width="105"
          height="55"
          rx="4"
          :class="$style.barOne"
        />
        <rect
          x="125"
          y="-155"
          width="105"
          height="155"
          rx="4"
          :class="$style.barTwo"
        />
        <rect
          x="250"
          y="-270"
          width="105"
          height="270"
          rx="4"
          :class="$style.barThree"
        />
        <text
          x="52"
          y="-20"
        >
          5%
        </text>
        <text
          x="177"
          y="-70"
        >
          10%
        </text>
        <text
          x="302"
          y="-125"
        >
          15%
        </text>
      </g>
    </svg>
    <div :class="$style.overlay" />
    <div :class="$style.content">
      <nav :class="$style.breadcrumb">
        <span>采购管理</span
        ><ChevronRight
          :size="12"
          :stroke-width="1.8"
        /><strong>供应商价格本</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><BookOpen
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>供应商价格本</h1>
          <p>管理供应商报价协议、批量导入与多级折扣，沉淀采购议价成果</p>
        </div>
      </div>
    </div>
  </header>
</template>
<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden rounded-2xl text-white;
  background: linear-gradient(135deg, #3c2a92 0%, #5541b5 58%, #8170d8 100%);
}
.scene,
.overlay {
  @apply pointer-events-none absolute inset-0 h-full w-full;
}
.overlay {
  background: linear-gradient(90deg, rgb(47 30 126/0.45) 0%, rgb(47 30 126/0.12) 55%, transparent 100%);
}
.content {
  @apply relative px-6 py-5;
}
.breadcrumb {
  @apply mb-3 flex items-center gap-1 text-[12px] font-medium;
  color: rgb(255 255 255/0.75);
}
.breadcrumb svg {
  color: rgb(255 255 255/0.5);
}
.breadcrumb strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex min-w-0 items-center gap-3;
}
.icon {
  @apply flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255/0.22);
  background: rgb(255 255 255/0.15);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
  letter-spacing: 0;
}
.identity p {
  @apply mt-1 text-[12.5px];
  color: rgb(255 255 255/0.85);
}
.halo {
  fill: #fff;
  opacity: 0.06;
}
.barOne,
.barTwo,
.barThree {
  fill: #fff;
}
.barOne {
  opacity: 0.25;
}
.barTwo {
  opacity: 0.4;
}
.barThree {
  opacity: 0.8;
}
.scene text {
  fill: #5541b5;
  font-size: 14px;
  font-weight: 700;
  text-anchor: middle;
}
</style>
