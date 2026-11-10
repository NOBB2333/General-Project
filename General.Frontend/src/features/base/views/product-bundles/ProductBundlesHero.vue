<script setup lang="ts">
import { Boxes, Layers3, PackageOpen } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>基础资料</span><b aria-hidden="true">›</b><strong>商品组合</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><Layers3
            :size="24"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>商品组合</h1>
          <p>管理常用商品套装组合，报价时可快速引用</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.art"
      aria-hidden="true"
    >
      <PackageOpen
        :size="42"
        :stroke-width="1.4"
      />
      <span :class="$style.path" />
      <Boxes
        :size="38"
        :stroke-width="1.4"
      />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative flex min-h-[120px] items-center overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(112deg, #403294 0%, #5243aa 58%, #8777d9 100%);
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
  @apply pointer-events-none absolute inset-y-0 right-20 flex w-[390px] items-center justify-between;
  color: rgb(255 255 255 / 72%);
}

.art::before,
.art::after {
  @apply absolute rounded-full;
  content: '';
  background: rgb(255 255 255 / 7%);
}

.art::before {
  @apply -right-16 -top-24 h-64 w-64;
}

.art::after {
  @apply right-28 top-[-52px] h-56 w-56;
}

.art svg {
  @apply relative z-[2] rounded-lg border p-2;
  border-color: rgb(255 255 255 / 35%);
}

.path {
  @apply relative z-[1] h-px flex-1 border-t border-dashed;
  border-color: rgb(255 255 255 / 55%);
}

@media (max-width: 760px) {
  .art {
    @apply -right-28 opacity-40;
  }
}
</style>
