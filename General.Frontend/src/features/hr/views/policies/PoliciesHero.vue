<script setup lang="ts">
import { ScrollText } from 'lucide-vue-next';
withDefaults(defineProps<{ breadcrumb?: string; title?: string; subtitle?: string }>(), {
  breadcrumb: '规章制度',
  title: '公司规章制度',
  subtitle: '共 7 项制度 · 6 个类别',
});
</script>
<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>人事管理 <span>›</span> {{ breadcrumb }}</p>
      <div :class="$style.title">
        <span
          ><ScrollText
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>{{ title }}</h1>
          <p>{{ subtitle }}</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.visual"
      aria-hidden="true"
    >
      <i /><b /><span>规</span>
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[500px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(105deg, #17123f, #332c70 56%, #5843ab);
}
.copy {
  @apply relative z-10 px-7 py-4;
  background: linear-gradient(90deg, rgb(20 17 55 / 88%), transparent);
}
.copy > p {
  @apply m-0 text-[10px] font-semibold text-white/75;
}
.copy > p span {
  @apply px-1.5;
}
.title {
  @apply mt-3 flex items-center gap-3;
}
.title > span {
  @apply flex h-10 w-10 items-center justify-center rounded border border-white/30 bg-white/15;
}
.title h1 {
  @apply m-0 text-[22px] font-semibold;
}
.title p {
  @apply m-0 mt-1 text-[11px] text-white/75;
}
.visual {
  @apply relative overflow-hidden;
}
.visual::before {
  content: '';
  @apply absolute bottom-0 left-[15%] h-[125px] w-[280px] border bg-[#e9e0d2];
  border-color: #806d53;
}
.visual::after {
  content: '';
  @apply absolute left-[21%] top-2 h-px w-[200px];
  background: #4e5364;
  box-shadow:
    0 16px #4e5364,
    0 32px #4e5364,
    0 48px #4e5364,
    0 64px #4e5364,
    0 80px #4e5364;
}
.visual i {
  @apply absolute bottom-0 left-[-10%] h-[120px] w-[120px];
  background: #9a7b4a;
  clip-path: polygon(15% 0, 85% 0, 100% 70%, 50% 100%, 0 70%);
}
.visual b {
  @apply absolute bottom-8 right-[19%] h-12 w-14 rounded bg-[#df321d];
  box-shadow: 0 14px 0 #92286b;
}
.visual span {
  @apply absolute bottom-[43px] right-[20.5%] z-10 text-[17px] font-bold;
}
</style>
