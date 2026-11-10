<script setup lang="ts">
import { BookOpenCheck, ShieldCheck, Trophy } from 'lucide-vue-next';
</script>
<template>
  <section :class="$style.profile">
    <span :class="$style.avatar">张</span>
    <div :class="$style.copy">
      <h2>张明远</h2>
      <p>销售部 · 销售经理</p>
      <p><b>L4 进阶者</b>1280 积分 · 32.5h 学习时长 · 3 张证书</p>
      <i /><small>距离 L5 专家 还差 1120 积分（约 38 门必修课）</small>
    </div>
    <div :class="$style.stats">
      <article>
        <BookOpenCheck
          :size="17"
          :stroke-width="1.8"
        /><strong>2</strong><span>在学</span>
      </article>
      <article>
        <Trophy
          :size="17"
          :stroke-width="1.8"
        /><strong>1</strong><span>已完成</span>
      </article>
      <article>
        <ShieldCheck
          :size="17"
          :stroke-width="1.8"
        /><strong>3</strong><span>证书</span>
      </article>
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.profile {
  @apply mt-4 flex h-[150px] items-center gap-5 rounded-lg px-6 text-white;
  background: linear-gradient(105deg, #5540b3, #075fd3 58%, #506da6);
}
.avatar {
  @apply flex h-16 w-16 items-center justify-center rounded-full border-2 border-white/35 text-[28px];
}
.copy {
  @apply flex-1;
}
.copy h2 {
  @apply m-0 text-[19px];
}
.copy p {
  @apply m-0 mt-1 text-[10px] text-white/80;
}
.copy p b {
  @apply mr-2 rounded px-2 py-1 text-[9px];
  background: rgb(255 255 255 / 20%);
}
.copy i {
  @apply mt-3 block h-2 w-[420px] rounded-full;
  background: rgb(255 255 255 / 22%);
}
.copy i::before {
  content: '';
  @apply block h-full w-[7%] rounded-full bg-white;
}
.copy small {
  @apply mt-1 block text-[9px] text-white/70;
}
.stats {
  @apply flex gap-3;
}
.stats article {
  @apply flex h-[82px] w-[58px] flex-col items-center justify-center rounded border border-white/20 bg-white/10;
}
.stats strong {
  @apply mt-1 text-[18px];
}
.stats span {
  @apply mt-1 text-[8px] text-white/70;
}
</style>
