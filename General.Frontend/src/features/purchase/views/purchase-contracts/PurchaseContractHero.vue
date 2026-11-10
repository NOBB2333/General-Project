<script setup lang="ts">
import { ChevronRight, FilePenLine } from 'lucide-vue-next';
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
        cx="1310"
        cy="205"
        r="190"
        :class="$style.halo"
      />
      <g transform="translate(1180,20)">
        <rect
          x="0"
          y="0"
          width="200"
          height="300"
          rx="8"
          :class="$style.paper"
        />
        <rect
          v-for="y in [50, 82, 114, 146]"
          :key="y"
          x="24"
          :y="y"
          width="145"
          height="8"
          rx="4"
          :class="$style.line"
        />
        <circle
          cx="156"
          cy="248"
          r="43"
          :class="$style.stamp"
        />
        <path
          d="M156 220 L163 239 L184 239 L167 251 L174 271 L156 259 L138 271 L145 251 L128 239 L149 239 Z"
          :class="$style.star"
        />
      </g>
    </svg>
    <div :class="$style.overlay" />
    <div :class="$style.content">
      <nav :class="$style.breadcrumb">
        <span>采购管理</span
        ><ChevronRight
          :size="12"
          :stroke-width="1.8"
        /><strong>采购合同</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><FilePenLine
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>采购合同</h1>
          <p>合同台账 · 框架/分批下单 · 付款节点 · 质保金与履约管控</p>
        </div>
      </div>
    </div>
  </header>
</template>
<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden rounded-2xl text-white;
  background: linear-gradient(135deg, #07427f 0%, #0f5daa 58%, #2599df 100%);
}
.scene,
.overlay {
  @apply pointer-events-none absolute inset-0 h-full w-full;
}
.overlay {
  background: linear-gradient(90deg, rgb(7 52 110/0.55) 0%, rgb(7 52 110/0.16) 54%, transparent 100%);
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
.paper {
  fill: #f3f7fc;
  opacity: 0.92;
}
.line {
  fill: #b7c4d5;
}
.stamp {
  fill: none;
  stroke: #ff5f45;
  stroke-width: 5;
}
.star {
  fill: #ff5f45;
}
</style>
