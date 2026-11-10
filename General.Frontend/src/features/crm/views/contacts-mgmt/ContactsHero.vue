<script setup lang="ts">
import { ArrowLeftRight, ContactRound, UserRound } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>CRM客户管理</span><span aria-hidden="true">›</span><strong>联系人管理</strong>
      </nav>
      <div :class="$style.titleRow">
        <span :class="$style.titleIcon"><ContactRound :size="22" /></span>
        <div>
          <h1>联系人管理</h1>
          <p>以「人」为中心管理所有客户联系人，追踪离职、跳槽等人脉变动</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="[$style.connection, $style.connectionTop]" />
      <span :class="[$style.connection, $style.connectionBottom]" />
      <span :class="$style.centerNode"><ArrowLeftRight :size="34" /></span>
      <span :class="[$style.memberCard, $style.memberTop]"
        ><i><UserRound :size="18" /></i><b /><b
      /></span>
      <span :class="[$style.memberCard, $style.memberBottom]"
        ><i><UserRound :size="18" /></i><b /><b
      /></span>
      <span :class="[$style.memberCard, $style.memberLeftTop]"
        ><i><UserRound :size="18" /></i><b /><b
      /></span>
      <span :class="[$style.memberCard, $style.memberLeftBottom]"
        ><i><UserRound :size="18" /></i><b /><b
      /></span>
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
  @apply absolute left-[18%] h-px w-[82%];
  background: rgb(255 255 255 / 66%);
  transform-origin: left center;
}
.connectionTop {
  top: 57%;
  transform: rotate(-8deg);
}
.connectionBottom {
  top: 60%;
  transform: rotate(8deg);
}
.centerNode {
  @apply absolute left-[47%] top-1/2 flex h-[88px] w-[88px] -translate-x-1/2 -translate-y-1/2 items-center justify-center rounded-full border-2;
  color: #ffab00;
  border-color: rgb(255 255 255 / 62%);
  background: rgb(255 255 255 / 10%);
  box-shadow: 0 0 0 12px rgb(255 255 255 / 7%);
}
.memberCard {
  @apply absolute right-[7%] flex h-12 w-[136px] items-center gap-2 rounded border px-3;
  border-color: rgb(255 255 255 / 64%);
  background: rgb(255 255 255 / 10%);
}
.memberTop {
  top: -5px;
}
.memberBottom {
  bottom: -9px;
}
.memberLeftTop {
  top: -5px;
  left: -48%;
}
.memberLeftBottom {
  bottom: -9px;
  left: -48%;
}
.memberCard i {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded-full;
  background: #4c9aff;
}
.memberBottom i,
.memberLeftBottom i {
  background: #ffab00;
}
.memberCard b {
  @apply absolute left-[48px] h-1.5 rounded-full;
  width: 70px;
  background: rgb(255 255 255 / 72%);
}
.memberCard b:last-child {
  @apply mt-5;
  width: 48px;
  background: rgb(255 255 255 / 38%);
}
@media (max-width: 900px) {
  .scene {
    @apply opacity-40;
  }
}
</style>
