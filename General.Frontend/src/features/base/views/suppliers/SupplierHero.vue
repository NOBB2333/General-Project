<script setup lang="ts">
import { Building2, ChevronRight } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>基础资料</span>
        <ChevronRight
          :size="12"
          :stroke-width="1.8"
        />
        <strong>供应商管理</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <Building2
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>供应商管理</h1>
          <p>供应商档案、资质审核、合作绩效与协同沟通</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.network"
      aria-hidden="true"
    >
      <span :class="$style.lineNorthWest" />
      <span :class="$style.lineNorthEast" />
      <span :class="$style.lineSouthWest" />
      <span :class="$style.lineSouthEast" />
      <span :class="$style.nodeCenter">
        <Building2
          :size="31"
          :stroke-width="1.8"
        />
      </span>
      <span :class="[$style.node, $style.nodeA]">A</span>
      <span :class="[$style.node, $style.nodeB]">B</span>
      <span :class="[$style.node, $style.nodeC]">C</span>
      <span :class="[$style.node, $style.nodeD]">D</span>
    </div>
    <span :class="$style.totalBadge">供应商 86</span>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(135deg, #b65c00 0%, #e07700 55%, #ff8b00 100%);
}
.copy {
  @apply relative z-[2];
}
.breadcrumb {
  @apply mb-3 flex items-center gap-1 text-[12px] font-medium;
  color: rgb(255 255 255 / 75%);
}
.breadcrumb svg {
  color: rgb(255 255 255 / 50%);
}
.breadcrumb strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex min-w-0 items-center gap-3;
}
.icon {
  @apply flex h-11 w-11 shrink-0 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 20%);
  background: rgb(255 255 255 / 15%);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
  letter-spacing: 0;
}
.identity p {
  @apply mt-1 text-[12.5px];
  color: rgb(255 255 255 / 85%);
}
.network {
  @apply pointer-events-none absolute inset-y-0 right-[84px] w-[470px];
}
.network::before {
  @apply absolute left-[116px] top-[-88px] h-[280px] w-[280px] rounded-full;
  content: '';
  background: rgb(255 255 255 / 6%);
}
.nodeCenter,
.node {
  @apply absolute z-[2] flex items-center justify-center rounded-full bg-white font-bold;
  color: #b65c00;
}
.nodeCenter {
  @apply left-[215px] top-[25px] h-[68px] w-[68px];
  color: #ff8b00;
  background: rgb(255 255 255 / 95%);
}
.node {
  @apply h-9 w-9 text-[12px];
  background: rgb(255 255 255 / 95%);
}
.nodeA {
  @apply left-[90px] top-[4px];
}
.nodeB {
  @apply right-[32px] top-[6px];
}
.nodeC {
  @apply bottom-[-12px] left-[72px];
}
.nodeD {
  @apply bottom-[-10px] right-[12px];
}
.lineNorthWest,
.lineNorthEast,
.lineSouthWest,
.lineSouthEast {
  @apply absolute z-[1] h-px border-t border-dashed;
  border-color: rgb(255 255 255 / 58%);
  transform-origin: left center;
}
.lineNorthWest {
  @apply left-[119px] top-[30px] w-[115px];
  transform: rotate(14deg);
}
.lineNorthEast {
  @apply left-[273px] top-[57px] w-[130px];
  transform: rotate(-14deg);
}
.lineSouthWest {
  @apply left-[107px] top-[105px] w-[124px];
  transform: rotate(-21deg);
}
.lineSouthEast {
  @apply left-[270px] top-[82px] w-[148px];
  transform: rotate(18deg);
}
.totalBadge {
  @apply absolute right-8 top-4 rounded-full bg-white px-3 py-1 text-[11px] font-bold;
  color: #b65c00;
}
@media (max-width: 820px) {
  .network,
  .totalBadge {
    @apply opacity-30;
  }
  .network {
    @apply -right-40;
  }
}
</style>
