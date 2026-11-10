<script setup lang="ts">
import { UserCheck, UserRound } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>CRM客户管理</span>
        <span aria-hidden="true">›</span>
        <strong>客户管理</strong>
      </nav>
      <div :class="$style.titleRow">
        <span :class="$style.titleIcon">
          <UserCheck
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>客户管理</h1>
          <p>客户档案、联系人、跟进记录与商务往来管理</p>
        </div>
      </div>
    </div>

    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="[$style.connection, $style.connectionTop]" />
      <span :class="[$style.connection, $style.connectionBottom]" />
      <span :class="$style.centerNode">
        <UserCheck
          :size="34"
          :stroke-width="1.5"
        />
      </span>
      <span :class="[$style.personCard, $style.personTop]">
        <i><UserRound :size="18" /></i>
        <b />
        <b />
      </span>
      <span :class="[$style.personCard, $style.personBottom]">
        <i><UserRound :size="18" /></i>
        <b />
        <b />
      </span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(112deg, #0b2d69 0%, #123d83 48%, #2b4da6 76%, #5147b8 100%);
}

.hero::after {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background: linear-gradient(90deg, rgb(9 30 75 / 35%) 0%, transparent 56%);
}

.copy {
  @apply relative z-10;
}

.breadcrumb {
  @apply mb-4 flex items-center gap-2 text-[12px];
  color: rgb(255 255 255 / 72%);
}

.breadcrumb strong {
  color: white;
}

.titleRow {
  @apply flex items-center gap-3;
}

.titleIcon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded border;
  color: white;
  border-color: rgb(255 255 255 / 30%);
  background: rgb(255 255 255 / 12%);
}

.titleRow h1 {
  @apply text-[22px] font-semibold;
}

.titleRow p {
  @apply mt-1 text-[13px];
  color: rgb(255 255 255 / 76%);
}

.scene {
  @apply pointer-events-none absolute inset-y-0 right-0 w-[58%] overflow-hidden;
}

.connection {
  @apply absolute left-[26%] h-px w-[76%];
  background: rgb(255 255 255 / 70%);
  transform-origin: left center;
}

.connectionTop {
  top: 55%;
  transform: rotate(-7deg);
}

.connectionBottom {
  top: 58%;
  transform: rotate(8deg);
}

.centerNode {
  @apply absolute left-[47%] top-1/2 flex h-[88px] w-[88px] -translate-x-1/2 -translate-y-1/2 items-center justify-center rounded-full border-2;
  color: #ffb020;
  border-color: rgb(255 255 255 / 62%);
  background: rgb(255 255 255 / 12%);
  box-shadow: 0 0 0 12px rgb(255 255 255 / 7%);
}

.personCard {
  @apply absolute right-[7%] flex h-12 w-[136px] items-center gap-2 rounded border px-3;
  border-color: rgb(255 255 255 / 64%);
  background: rgb(255 255 255 / 10%);
}

.personTop {
  top: -5px;
}

.personBottom {
  bottom: -9px;
}

.personCard i {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded-full;
  color: white;
  background: #4c9aff;
}

.personBottom i {
  background: #ffab00;
}

.personCard b {
  @apply absolute left-[48px] h-1.5 rounded-full;
  width: 70px;
  background: rgb(255 255 255 / 72%);
}

.personCard b:last-child {
  @apply mt-5;
  width: 48px;
  background: rgb(255 255 255 / 38%);
}

@media (max-width: 900px) {
  .scene {
    @apply opacity-40;
  }
}

@media (prefers-reduced-motion: reduce) {
  .scene * {
    animation: none;
  }
}
</style>
