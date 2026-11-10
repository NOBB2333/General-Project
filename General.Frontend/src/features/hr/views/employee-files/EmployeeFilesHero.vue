<script setup lang="ts">
import { UsersRound } from 'lucide-vue-next';
</script>

<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>人事管理 <span>›</span> 员工档案</p>
      <div :class="$style.title">
        <span
          ><UsersRound
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>员工档案</h1>
          <p>员工花名册、组织架构、个人档案与人事变动记录</p>
        </div>
      </div>
    </div>
    <div :class="$style.org">
      <i data-person="1" /><i data-person="2" /><i data-person="3" /><i data-person="4" /><i data-person="5" />
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[390px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(105deg, #331b6f, #333caa 62%, #0654bb);
}
.copy {
  @apply px-7 py-4;
  background: rgb(51 27 111 / 66%);
}
.copy > p {
  @apply m-0 text-[10px] font-semibold text-white/70;
}
.copy > p span {
  @apply px-1.5;
}
.title {
  @apply mt-3 flex items-center gap-3;
}
.title > span {
  @apply flex h-10 w-10 items-center justify-center rounded border border-white/25 bg-white/15;
}
.title h1 {
  @apply m-0 text-[22px] font-semibold;
}
.title p {
  @apply m-0 mt-1 text-[11px] text-white/75;
}
.org {
  @apply relative;
  background-image: linear-gradient(28deg, transparent 49.7%, rgb(255 255 255 / 50%) 50%, transparent 50.3%);
}
.org::before {
  content: '';
  @apply absolute left-1/2 top-0 h-full w-px bg-white/45;
}
.org i {
  @apply absolute h-9 w-9 rounded-full border-2 border-white/55 bg-cyan-300/60;
  box-shadow: inset 0 -8px 0 rgb(255 255 255 / 25%);
}
.org i[data-person='1'] {
  @apply left-[12%] top-10;
}
.org i[data-person='2'] {
  @apply left-[32%] top-8 bg-orange-300/70;
}
.org i[data-person='3'] {
  @apply left-[50%] top-10 bg-blue-300/70;
}
.org i[data-person='4'] {
  @apply left-[68%] top-8 bg-violet-300/70;
}
.org i[data-person='5'] {
  @apply right-[9%] top-16 bg-orange-400/80;
}
</style>
