<script setup lang="ts">
import { CheckCircle2, Eye, FileSignature } from 'lucide-vue-next';
</script>

<template>
  <section :class="$style.grid">
    <article data-tone="primary">
      <Eye
        :size="17"
        :stroke-width="1.8"
      />
      <div>
        <span>我的阅读进度</span>
        <p><strong>18</strong> /18 条 <b>100%</b></p>
        <i :class="$style.progress" />
      </div>
    </article>
    <article data-tone="warning">
      <FileSignature
        :size="17"
        :stroke-width="1.8"
      />
      <div>
        <span>重要条款签字</span>
        <p><strong>5</strong> /5 条 <b>100%</b></p>
        <i :class="$style.progress" />
      </div>
    </article>
    <article data-tone="success">
      <CheckCircle2
        :size="17"
        :stroke-width="1.8"
      />
      <div>
        <span>当前版本确认</span>
        <p><strong>已确认 V3.2</strong></p>
        <small>上次阅读 2026-02-02 14:00</small>
      </div>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply grid grid-cols-3 gap-3;
}
.grid article {
  @apply flex h-[80px] items-center gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.grid article > svg {
  @apply box-content rounded p-2;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.grid article[data-tone='warning'] > svg {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.grid article[data-tone='success'] > svg {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.grid article > div {
  @apply min-w-0 flex-1;
}
.grid span,
.grid small {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.grid p {
  @apply m-0 mt-1 text-[9px];
  color: var(--ui-text-muted);
}
.grid strong {
  @apply text-[17px];
  color: var(--ui-text);
}
.grid b {
  @apply float-right text-[10px];
  color: var(--app-primary);
}
.progress {
  @apply mt-1 block h-1.5 w-full rounded-full;
  background: var(--app-primary);
}
.grid article[data-tone='warning'] .progress {
  background: var(--ui-warning);
}
.grid article[data-tone='success'] strong {
  @apply text-[13px];
  color: var(--ui-success);
}
.grid small {
  @apply mt-1 block;
}
</style>
